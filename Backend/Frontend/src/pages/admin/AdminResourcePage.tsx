import { useDeferredValue, useEffect, useId, useMemo, useRef, useState } from 'react'
import { Archive, Download, Eye, Pencil, Plus, Search, SlidersHorizontal, X } from 'lucide-react'
import { useQuery } from '@tanstack/react-query'
import type { TFunction } from 'i18next'
import { useTranslation } from 'react-i18next'
import { Link, useParams, useSearchParams } from 'react-router-dom'
import { toast } from 'sonner'
import { api, downloadFile } from '@/lib/api/client'
import {
  getAdminResource,
  hasAnyPermission,
  isWritableAdminResource,
  type AdminResource,
} from '@/config/adminResources'
import { EmptyState, ErrorState, ForbiddenState, LoadingState } from '@/components/ui/Feedback'
import { parseApiProblem } from '@/lib/api/problemDetails'
import { formatDate, formatMoney } from '@/lib/utils'
import { useSessionStore } from '@/stores/sessionStore'
import { statusBadgeClass } from '@/lib/statusTone'
import { AdminImagePreview } from '@/components/admin/AdminImagePreview'
import {
  AdminContactMessageDialog,
  AdminCustomerDialog,
  AdminResourceEditor,
  type ResourceWriteAction,
} from '@/pages/admin/AdminResourceEditor'

type Row = Record<string, unknown>
type ResourceResult =
  | { items?: Row[]; totalPages?: number }
  | Row[]
  | Row
  | undefined

const humanize = (value: string) =>
  value
    .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
    .replace(/[-_]/g, ' ')
    .replace(/^./, (letter) => letter.toUpperCase())

const technicalField = /(?:^id$|id$|email|phone|url|uri|sku|slug|code|number|currency|price|amount|total|quantity|count|date|time|utc$)/i
const monetaryField = /(?:price|fee|cost|amount|subtotal|discount|delivery|total|sales|spending|revenue)$/i
const isImageField = (field: string) => /^(?:imageUrl|thumbnailUrl)$/i.test(field)

const valueDirection = (field: string, value: unknown): 'auto' | 'ltr' =>
  technicalField.test(field) || typeof value === 'number' ? 'ltr' : 'auto'

const localizedTranslation = (row: Row, language: string) => {
  if (!Array.isArray(row.translations)) return undefined
  const preferred = language.startsWith('ar') ? 'Arabic' : 'English'
  return row.translations.find(
    (item): item is Row => typeof item === 'object' && item !== null && item.language === preferred,
  ) ?? row.translations.find((item): item is Row => typeof item === 'object' && item !== null)
}

const uuidPattern = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i

/** A foreign key such as `categoryId`: useful to the API, noise in a table meant for people. */
const isReferenceId = (field: string, value: unknown) =>
  field !== 'id' && /Id$/.test(field) && typeof value === 'string' && uuidPattern.test(value)

const enrichRow = (row: Row, language: string, categoryNames?: ReadonlyMap<string, string>): Row => {
  const translation = localizedTranslation(row, language)
  const variants = Array.isArray(row.variants) ? row.variants : []
  const firstVariant = variants.find((item): item is Row => typeof item === 'object' && item !== null)
  const categoryName = typeof row.categoryId === 'string' ? categoryNames?.get(row.categoryId) : undefined
  return {
    ...row,
    ...(categoryName ? { categoryName } : {}),
    ...(row.name == null && translation?.name != null ? { name: translation.name } : {}),
    ...(row.title == null && translation?.title != null ? { title: translation.title } : {}),
    ...(row.slug == null && translation?.slug != null ? { slug: translation.slug } : {}),
    ...(row.sku == null && firstVariant?.sku != null ? { sku: firstVariant.sku } : {}),
  }
}

const displayValue = (field: string, value: unknown, t: TFunction, locale: string) => {
  if (value == null || value === '') return '—'
  if (typeof value === 'boolean') return t(value ? 'common.yes' : 'common.no')
  if (typeof value === 'number' && monetaryField.test(field)) return formatMoney(value, 'ILS', locale)
  if (typeof value === 'number') return new Intl.NumberFormat(locale).format(value)
  if (typeof value === 'string' && /^\d{4}-\d{2}-\d{2}T/.test(value)) {
    return formatDate(value, locale)
  }
  if (typeof value === 'string') {
    return t(`admin.values.${value}`, { defaultValue: value })
  }
  if (Array.isArray(value)) return t('admin.table.itemCount', { count: value.length })
  return t('admin.table.details')
}

const isRow = (value: unknown): value is Row =>
  typeof value === 'object' && value !== null && !Array.isArray(value)

const withoutHiddenFields = (row: Row, resource: AdminResource): Row =>
  resource.hiddenFields?.length
    ? Object.fromEntries(Object.entries(row).filter(([key]) => !resource.hiddenFields?.includes(key)))
    : row

/** A product list row has no single image field; use its primary (else first) uploaded image. */
const primaryImageUrl = (row: Row): string | null => {
  const images = Array.isArray(row.images) ? row.images.filter(isRow) : []
  const image = images.find((item) => item.isPrimary === true) ?? images[0]
  const url = image?.imageUrl ?? image?.thumbnailUrl
  return typeof url === 'string' ? url : null
}

// Falls back to productVariantId for resources (like inventory) whose rows have no top-level "id".
const rowIdentifier = (row: Row): unknown => row.id ?? row.productVariantId

const fieldLabel = (field: string, t: TFunction) =>
  isImageField(field) ? t('admin.fields.image') : t(`admin.fields.${field}`, { defaultValue: humanize(field) })

const translationLanguage = (translation: Row) =>
  typeof translation.language === 'string' ? translation.language : ''

const isTranslationList = (value: unknown): value is Row[] =>
  Array.isArray(value)
  && value.every((item) => isRow(item) && Boolean(translationLanguage(item)))

const translationSortOrder = (language: string) => {
  if (language === 'Arabic') return 0
  if (language === 'English') return 1
  return 2
}

type DetailValueProps = {
  depth?: number
  field: string
  locale: string
  t: TFunction
  value: unknown
}

function DetailFields({
  depth = 0,
  entries,
  locale,
  t,
}: {
  depth?: number
  entries: [string, unknown][]
  locale: string
  t: TFunction
}) {
  return (
    <dl className="grid min-w-0 gap-0">
      {entries
        .filter(([key]) => !key.toLowerCase().includes('rowversion') && key.toLowerCase() !== 'currency')
        .map(([key, value]) => (
          <div
            className="grid min-w-0 gap-1 border-b py-3 first:pt-0 last:border-0 sm:grid-cols-[minmax(7rem,160px)_minmax(0,1fr)]"
            key={key}
          >
            <dt className="muted min-w-0 break-words text-sm [overflow-wrap:anywhere]">
              {fieldLabel(key, t)}
            </dt>
            <dd className="min-w-0 text-sm font-semibold">
              <DetailValue depth={depth} field={key} locale={locale} t={t} value={value} />
            </dd>
          </div>
        ))}
    </dl>
  )
}

function TranslationCards({ locale, t, translations }: {
  locale: string
  t: TFunction
  translations: Row[]
}) {
  const orderedTranslations = [...translations].sort((left, right) => {
    const languageComparison = translationSortOrder(translationLanguage(left)) - translationSortOrder(translationLanguage(right))
    return languageComparison || translationLanguage(left).localeCompare(translationLanguage(right))
  })

  if (!orderedTranslations.length) {
    return <span className="muted">—</span>
  }

  return (
    <div className="grid min-w-0 gap-3">
      {orderedTranslations.map((translation, index) => {
        const language = translationLanguage(translation)
        return (
          <section
            className="min-w-0 rounded-xl border bg-stone-50 p-3 sm:p-4"
            key={`${language}-${index}`}
          >
            <header className="mb-3 flex min-w-0 items-center gap-2 border-b pb-3">
              <span className="badge max-w-full whitespace-normal break-words [overflow-wrap:anywhere]">
                {t(`admin.values.${language}`, { defaultValue: language })}
              </span>
            </header>
            <DetailFields
              depth={1}
              entries={Object.entries(translation).filter(([key]) => key !== 'language')}
              locale={locale}
              t={t}
            />
          </section>
        )
      })}
    </div>
  )
}

function DetailValue({ depth = 0, field, locale, t, value }: DetailValueProps) {
  if (isImageField(field)) {
    return <AdminImagePreview alt={t('admin.fields.image')} className="h-56 w-full max-w-sm" src={typeof value === 'string' ? value : null} />
  }
  if (value == null || typeof value !== 'object') {
    return (
      <bdi className="block break-words [overflow-wrap:anywhere] [text-align:match-parent]" dir={valueDirection(field, value)}>
        {displayValue(field, value, t, locale)}
      </bdi>
    )
  }

  if (field.toLowerCase() === 'translations' && Array.isArray(value)) {
    const translations = isTranslationList(value) ? value : []
    return <TranslationCards locale={locale} t={t} translations={translations} />
  }

  if (depth >= 3) {
    return <span className="muted">{t('admin.table.details')}</span>
  }

  if (Array.isArray(value)) {
    if (!value.length) return <span className="muted">—</span>
    return (
      <div className="grid min-w-0 gap-3">
        {value.map((item, index) => (
          <div className="min-w-0 rounded-xl border bg-stone-50 p-3" key={index}>
            {isRow(item)
              ? <DetailFields depth={depth + 1} entries={Object.entries(item)} locale={locale} t={t} />
              : <DetailValue depth={depth + 1} field={field} locale={locale} t={t} value={item} />}
          </div>
        ))}
      </div>
    )
  }

  if (isRow(value)) {
    return (
      <div className="min-w-0 rounded-xl border bg-stone-50 p-3">
        <DetailFields depth={depth + 1} entries={Object.entries(value)} locale={locale} t={t} />
      </div>
    )
  }

  return <span className="muted">{t('admin.table.details')}</span>
}

function ResourceDetailsDialog({
  onClose,
  row,
}: {
  onClose: () => void
  row: Row
}) {
  const { t, i18n } = useTranslation()
  const headingId = useId()
  const dialogRef = useRef<HTMLElement>(null)
  const closeRef = useRef<HTMLButtonElement>(null)

  useEffect(() => {
    const previouslyFocused = document.activeElement as HTMLElement | null
    const previousOverflow = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    closeRef.current?.focus()
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        event.preventDefault()
        onClose()
        return
      }
      if (event.key !== 'Tab') return

      const focusable = Array.from(
        dialogRef.current?.querySelectorAll<HTMLElement>(
          'button:not([disabled]), input:not([disabled]), textarea:not([disabled]), select:not([disabled]), [href], [tabindex]:not([tabindex="-1"])',
        ) ?? [],
      )
      if (!focusable.length) return
      const first = focusable[0]
      const last = focusable[focusable.length - 1]
      if (event.shiftKey && document.activeElement === first) {
        event.preventDefault()
        last.focus()
      } else if (!event.shiftKey && document.activeElement === last) {
        event.preventDefault()
        first.focus()
      }
    }
    document.addEventListener('keydown', onKeyDown)
    return () => {
      document.removeEventListener('keydown', onKeyDown)
      document.body.style.overflow = previousOverflow
      previouslyFocused?.focus()
    }
  }, [onClose])

  const locale = i18n.language.startsWith('ar') ? 'ar-PS-u-nu-latn' : 'en-US'
  return (
    <div
      className="fixed inset-0 z-[80] grid place-items-center overflow-y-auto bg-black/55 p-3 sm:p-4"
      onMouseDown={(event) => {
        if (event.target === event.currentTarget) onClose()
      }}
      role="presentation"
    >
      <section
        aria-labelledby={headingId}
        aria-modal="true"
        className="card flex max-h-[calc(100dvh-1.5rem)] w-full max-w-3xl flex-col overflow-hidden"
        ref={dialogRef}
        role="dialog"
      >
        <header className="flex shrink-0 items-start justify-between gap-4 border-b p-4 sm:p-5">
          <h2 className="min-w-0 break-words text-xl font-bold [overflow-wrap:anywhere]" id={headingId}>{t('admin.table.recordDetails')}</h2>
          <button
            aria-label={t('actions.close')}
            className="button-secondary !size-11 shrink-0 !p-0"
            onClick={onClose}
            ref={closeRef}
            type="button"
          >
            <X aria-hidden size={19} />
          </button>
        </header>
        <div className="min-h-0 flex-1 overflow-y-auto overscroll-contain p-4 sm:p-5">
          <DetailFields entries={Object.entries(row)} locale={locale} t={t} />
        </div>
      </section>
    </div>
  )
}

function AdminResourceContent({
  resource,
  resourceKey,
}: {
  resource: AdminResource
  resourceKey: string
}) {
  const { t, i18n } = useTranslation()
  const permissions = useSessionStore((state) => state.admin?.permissions ?? [])
  const [search, setSearch] = useState('')
  const deferredSearch = useDeferredValue(search)
  const [page, setPage] = useState(1)
  const [selectedRow, setSelectedRow] = useState<Row | null>(null)
  const [searchParams, setSearchParams] = useSearchParams()
  const [writeAction, setWriteAction] = useState<{
    action: ResourceWriteAction
    row?: Row
  } | null>(null)
  const [exporting, setExporting] = useState(false)
  const locale = i18n.language.startsWith('ar') ? 'ar-PS-u-nu-latn' : 'en-US'
  const query = useQuery({
    queryKey: ['admin-resource', resourceKey, page, deferredSearch, i18n.language],
    queryFn: () => api
      .get(resource.endpoint, {
        params: {
          page,
          pageSize: 20,
          search: resource.supportsSearch && deferredSearch ? deferredSearch : undefined,
        },
      })
      .then((response) => response.data as unknown),
  })
  // Product rows only carry a category id, so names come from the (small, cached) category list.
  const categoryLookup = useQuery({
    queryKey: ['admin-category-names', i18n.language],
    queryFn: () => api
      .get('/admin/categories', { params: { page: 1, pageSize: 100 } })
      .then((response) => response.data as ResourceResult),
    enabled: resourceKey === 'products',
    staleTime: 5 * 60_000,
    retry: false,
  })
  const categoryNames = useMemo(() => {
    const data = categoryLookup.data
    const items = Array.isArray(data) ? data : data && 'items' in data && Array.isArray(data.items) ? data.items : []
    return new Map(
      items
        .map((item) => [String(item.id ?? ''), String(localizedTranslation(item, i18n.language)?.name ?? '')] as const)
        .filter(([id, name]) => id && name),
    )
  }, [categoryLookup.data, i18n.language])
  const raw = query.data as ResourceResult
  const rows = useMemo(() => {
    const source = Array.isArray(raw)
      ? raw
      : raw && 'items' in raw && Array.isArray(raw.items)
        ? raw.items
        : raw
          ? [raw as Row]
          : []
    const withImage = (row: Row): Row =>
      resourceKey === 'categories'
        ? { ...row, imageUrl: row.imageUrl ?? null }
        : resourceKey === 'products'
          ? { ...row, imageUrl: primaryImageUrl(row) }
          : row
    return source.map((row) => enrichRow(withImage(row), i18n.language, categoryNames))
  }, [categoryNames, i18n.language, raw, resourceKey])
  const columns = useMemo(() => {
    if (resource.listColumns) {
      return resource.listColumns.filter((key) => isImageField(key) || rows.some((row) => row[key] != null))
    }
    const priority = ['name', 'fullName', 'orderNumber', 'code', 'title', 'sku', 'categoryName', 'status', 'total', 'createdAtUtc']
    const keys = Array.from(new Set(rows.flatMap((row) => Object.keys(row))))
      .filter((key) =>
        key !== 'id'
        && !resource.hiddenFields?.includes(key)
        && !key.toLowerCase().includes('rowversion')
        && key.toLowerCase() !== 'currency'
        && !isReferenceId(key, rows.find((row) => row[key] != null)?.[key])
        && (isImageField(key) || typeof rows.find((row) => row[key] != null)?.[key] !== 'object'))
    return [...priority.filter((key) => keys.includes(key)), ...keys.filter((key) => !priority.includes(key))].slice(0, 7)
  }, [resource, rows])
  const totalPages = !Array.isArray(raw) && raw && 'totalPages' in raw
    ? Math.max(1, Number(raw.totalPages ?? 1))
    : 1
  const canExport = Boolean(
    resource.exportEndpoint
    && resource.exportPermission
    && permissions.includes(resource.exportPermission),
  )
  const canWrite = Boolean(
    resource.writePermission
    && permissions.includes(resource.writePermission),
  )
  const supportsWrites = isWritableAdminResource(resourceKey)
  const canWriteStatus = Boolean(
    resource.statusWritePermission
    && permissions.includes(resource.statusWritePermission),
  )

  const requestedRowId = searchParams.get('open')
  useEffect(() => {
    if (!requestedRowId) return
    const matchingRow = rows.find((row) => String(rowIdentifier(row) ?? '') === requestedRowId)
    if (!matchingRow) return
    setSelectedRow((current) => String((current && rowIdentifier(current)) ?? '') === requestedRowId ? current : matchingRow)
  }, [requestedRowId, rows])

  const openRow = (row: Row) => {
    setSelectedRow(row)
    const identifier = rowIdentifier(row)
    if (!identifier) return
    const updated = new URLSearchParams(searchParams)
    updated.set('open', String(identifier))
    setSearchParams(updated, { replace: true })
  }

  const closeRow = () => {
    setSelectedRow(null)
    if (!searchParams.has('open')) return
    const updated = new URLSearchParams(searchParams)
    updated.delete('open')
    setSearchParams(updated, { replace: true })
  }

  const exportRows = async () => {
    if (!resource.exportEndpoint || exporting) return
    setExporting(true)
    try {
      const toUtc = new Date()
      const fromUtc = new Date(toUtc)
      fromUtc.setUTCFullYear(fromUtc.getUTCFullYear() - 1)
      const exportQuery = new URLSearchParams({
        fromUtc: fromUtc.toISOString(),
        toUtc: toUtc.toISOString(),
        language: i18n.language.startsWith('ar') ? 'Arabic' : 'English',
      })
      await downloadFile(
        `${resource.exportEndpoint}?${exportQuery.toString()}`,
        `${resourceKey}.xlsx`,
      )
      toast.success(t('admin.export.success'))
    } catch (error) {
      const problem = parseApiProblem(error)
      toast.error(t('admin.export.failed'), { description: problem.title })
    } finally {
      setExporting(false)
    }
  }

  // Catalog lists (products, categories) keep their row actions visually secondary: muted until hovered.
  const isCatalog = Boolean(resource.listColumns)
  const quietAction = 'inline-flex min-w-0 items-center gap-1.5 whitespace-nowrap text-start text-sm font-semibold text-[var(--heritage-muted)] transition-colors hover:text-[var(--heritage-burgundy)] hover:underline'
  const rowActions = (row: Row) => (
    <div className={`flex min-w-0 flex-wrap items-center gap-y-2 ${isCatalog ? 'gap-x-5' : 'gap-x-4'}`}>
      {resourceKey === 'orders' && row.id
        ? (
          <Link
            className="inline-flex min-w-0 items-center gap-1.5 whitespace-nowrap text-start text-sm font-bold transition-colors hover:underline text-[var(--heritage-charcoal)] hover:text-[var(--heritage-burgundy)]"
            to={`/admin/orders/${String(row.id)}`}
          >
            <Eye aria-hidden className="shrink-0" size={17} />
            <span className="break-words">{t('actions.view')}</span>
          </Link>
        )
        : (
          <button
            className={isCatalog ? quietAction : 'inline-flex min-w-0 items-center gap-1.5 whitespace-nowrap text-start text-sm font-bold transition-colors hover:underline text-[var(--heritage-charcoal)] hover:text-[var(--heritage-burgundy)]'}
            onClick={() => openRow(row)}
            type="button"
          >
            <Eye aria-hidden className="shrink-0" size={17} />
            <span className="break-words">{t('actions.view')}</span>
          </button>
        )}
      {canWrite && supportsWrites && resourceKey === 'inventory' && (
        <button
          className="inline-flex min-w-0 items-center gap-1.5 whitespace-nowrap text-start text-sm font-bold transition-colors hover:underline text-[var(--heritage-burgundy)] hover:text-[var(--heritage-red)]"
          onClick={() => setWriteAction({ action: 'adjust', row })}
          type="button"
        >
          <SlidersHorizontal aria-hidden className="shrink-0" size={17} />
          <span className="break-words">{t('admin.editor.adjust')}</span>
        </button>
      )}
      {canWrite && supportsWrites && resourceKey !== 'inventory' && (
        <>
          <button
            className={isCatalog ? quietAction : 'inline-flex min-w-0 items-center gap-1.5 whitespace-nowrap text-start text-sm font-bold transition-colors hover:underline text-[var(--heritage-burgundy)] hover:text-[var(--heritage-red)]'}
            onClick={() => setWriteAction({ action: 'edit', row })}
            type="button"
          >
            <Pencil aria-hidden className="shrink-0" size={17} />
            <span className="break-words">{t('admin.editor.edit')}</span>
          </button>
          <button
            className={isCatalog ? `${quietAction} hover:!text-[var(--state-danger-text)]` : 'inline-flex min-w-0 items-center gap-1.5 whitespace-nowrap text-start text-sm font-bold transition-colors hover:underline text-[var(--state-danger-text)]'}
            onClick={() => setWriteAction({ action: 'archive', row })}
            type="button"
          >
            <Archive aria-hidden className="shrink-0" size={17} />
            <span className="break-words">{t('admin.editor.archive')}</span>
          </button>
        </>
      )}
    </div>
  )

  // On phones each row is a card: the first column is its title, the rest a compact two-column grid.
  // Catalog rows lead with their image, shown next to the title instead of as a labelled field.
  const titleColumn = columns.find((column) => !isImageField(column))
  const catalogImageColumn = isCatalog ? columns.find(isImageField) : undefined
  const detailColumns = columns.filter((column) => column !== titleColumn && column !== catalogImageColumn)

  return (
    <section>
      <div className="mb-6 flex min-w-0 flex-wrap items-center justify-between gap-4">
        <h1 className="min-w-0 break-words text-3xl font-extrabold [overflow-wrap:anywhere]">{t(resource.titleKey)}</h1>
        <div className="flex w-full flex-wrap gap-3 sm:w-auto">
          {canWrite && supportsWrites && resourceKey !== 'inventory' && (
            <button
              className="button-primary min-w-0 flex-1 whitespace-normal sm:flex-none"
              onClick={() => setWriteAction({ action: 'create' })}
              type="button"
            >
              <Plus aria-hidden size={17} />
              {t('admin.editor.create')}
            </button>
          )}
          {canExport && (
            <button className="button-secondary min-w-0 flex-1 whitespace-normal sm:flex-none" disabled={exporting} onClick={() => void exportRows()} type="button">
              <Download aria-hidden size={17} />
              {exporting ? t('admin.export.inProgress') : t('admin.export.action')}
            </button>
          )}
        </div>
      </div>
      {resource.supportsSearch && (
        <label className="card mb-5 flex min-w-0 items-center gap-3 p-3">
          <Search aria-hidden className="muted shrink-0" size={19} />
          <span className="sr-only">{t('admin.search')}</span>
          <input
            className="min-h-10 min-w-0 flex-1 border-0 bg-transparent outline-none"
            placeholder={t('admin.search')}
            value={search}
            onChange={(event) => {
              setSearch(event.target.value)
              setPage(1)
            }}
          />
        </label>
      )}
      {query.isLoading
        ? <LoadingState />
        : query.isError
          ? <ErrorState error={query.error} retry={() => void query.refetch()} />
          : !rows.length
            ? <EmptyState />
            : (
              <div className="min-w-0">
                <div className="grid gap-4 md:hidden">
                  {rows.map((row, index) => (
                    <article className="card min-w-0 p-4" key={String(rowIdentifier(row) ?? index)}>
                      <div className="flex min-w-0 items-center gap-3">
                        {catalogImageColumn && (
                          <AdminImagePreview
                            alt={String(row.name ?? t('admin.fields.image'))}
                            className="size-16 shrink-0"
                            compact
                            src={typeof row[catalogImageColumn] === 'string' ? row[catalogImageColumn] : null}
                          />
                        )}
                        {titleColumn && (
                          <h2 className="min-w-0 break-words text-base font-extrabold [overflow-wrap:anywhere]">
                            <bdi dir={valueDirection(titleColumn, row[titleColumn])}>{displayValue(titleColumn, row[titleColumn], t, locale)}</bdi>
                          </h2>
                        )}
                      </div>
                      <dl className="mt-3 grid min-w-0 grid-cols-2 gap-x-4 gap-y-3">
                        {detailColumns.map((column) => (
                          <div className="min-w-0" key={column}>
                            <dt className="muted break-words text-xs font-semibold [overflow-wrap:anywhere]">
                              {fieldLabel(column, t)}
                            </dt>
                            <dd className="mt-0.5 min-w-0 text-sm font-semibold">
                              {isImageField(column)
                                ? <AdminImagePreview alt={String(row.name ?? t('admin.fields.image'))} src={typeof row[column] === 'string' ? row[column] : null} />
                                : column.toLowerCase().includes('status')
                                ? <span className={statusBadgeClass(row[column]) + ' max-w-full whitespace-normal break-words [overflow-wrap:anywhere]'}>{displayValue(column, row[column], t, locale)}</span>
                                : <bdi className="block break-words [overflow-wrap:anywhere] [text-align:match-parent]" dir={valueDirection(column, row[column])}>{displayValue(column, row[column], t, locale)}</bdi>}
                            </dd>
                          </div>
                        ))}
                      </dl>
                      <div className="mt-4 border-t border-[var(--heritage-line)] pt-3">
                        {rowActions(row)}
                      </div>
                    </article>
                  ))}
                </div>
                <div className="card hidden min-w-0 overflow-x-auto md:block">
                  <table className={`w-full table-auto text-sm ${isCatalog ? 'min-w-[560px]' : 'min-w-[720px]'}`}>
                    <thead className="bg-[var(--heritage-beige)]">
                      <tr>
                        {columns.map((column) => (
                          <th className="break-words px-4 py-3 text-start [overflow-wrap:anywhere]" key={column}>
                            {fieldLabel(column, t)}
                          </th>
                        ))}
                        <th className="break-words px-4 py-3 text-start [overflow-wrap:anywhere]">{t('admin.table.actions')}</th>
                      </tr>
                    </thead>
                    <tbody>
                      {rows.map((row, index) => (
                        <tr className="border-t align-top hover:bg-stone-50" key={String(rowIdentifier(row) ?? index)}>
                          {columns.map((column) => (
                            <td className={`max-w-56 px-4 ${isCatalog ? 'py-3 !align-middle' : 'py-4'}`} key={column}>
                              {isImageField(column)
                                ? <AdminImagePreview alt={String(row.name ?? t('admin.fields.image'))} className={isCatalog ? 'size-14' : undefined} compact={isCatalog} src={typeof row[column] === 'string' ? row[column] : null} />
                                : column.toLowerCase().includes('status')
                                ? <span className={statusBadgeClass(row[column]) + ' max-w-full whitespace-normal break-words [overflow-wrap:anywhere]'}>{displayValue(column, row[column], t, locale)}</span>
                                : <bdi className="block break-words [overflow-wrap:anywhere] [text-align:match-parent]" dir={valueDirection(column, row[column])}>{displayValue(column, row[column], t, locale)}</bdi>}
                            </td>
                          ))}
                          <td className={`px-4 ${isCatalog ? 'py-3 !align-middle' : 'py-4'}`}>
                            {rowActions(row)}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            )}
      {totalPages > 1 && (
        <nav aria-label={t('admin.table.pagination')} className="mt-5 flex flex-wrap items-center justify-center gap-2 sm:justify-end">
          <button className="button-secondary min-w-0 flex-1 whitespace-normal sm:flex-none" disabled={page <= 1} onClick={() => setPage((value) => value - 1)} type="button">
            {t('actions.previous')}
          </button>
          <span className="badge order-first flex w-full justify-center whitespace-normal text-center sm:order-none sm:w-auto">{t('admin.table.pageOf', { page, total: totalPages })}</span>
          <button className="button-secondary min-w-0 flex-1 whitespace-normal sm:flex-none" disabled={page >= totalPages} onClick={() => setPage((value) => value + 1)} type="button">
            {t('actions.next')}
          </button>
        </nav>
      )}
      {selectedRow && resourceKey === 'customers' && selectedRow.id
        ? (
          <AdminCustomerDialog
            canStatusWrite={canWriteStatus}
            canWrite={canWrite}
            customerId={String(selectedRow.id)}
            onClose={closeRow}
          />
        )
        : selectedRow && resourceKey === 'contact' && selectedRow.id
          ? (
            <AdminContactMessageDialog
              canWrite={canWrite}
              messageId={String(selectedRow.id)}
              onClose={closeRow}
            />
          )
          : selectedRow && <ResourceDetailsDialog onClose={closeRow} row={withoutHiddenFields(selectedRow, resource)} />}
      {writeAction && supportsWrites && (
        <AdminResourceEditor
          action={writeAction.action}
          onClose={() => setWriteAction(null)}
          resource={resourceKey}
          row={writeAction.row}
        />
      )}
    </section>
  )
}

export function AdminResourcePage({ resource: fixedResource }: { resource?: string }) {
  const { t } = useTranslation()
  const params = useParams()
  const permissions = useSessionStore((state) => state.admin?.permissions ?? [])
  const resourceKey = fixedResource ?? params.resource ?? ''
  const resource = getAdminResource(resourceKey)

  if (!resource) {
    return (
      <ErrorState
        message={t('errors.resourceNotFound.message')}
        status={404}
        title={t('errors.resourceNotFound.title')}
      />
    )
  }
  if (!hasAnyPermission(permissions, resource.readPermissions)) return <ForbiddenState />
  return <AdminResourceContent resource={resource} resourceKey={resourceKey} />
}
