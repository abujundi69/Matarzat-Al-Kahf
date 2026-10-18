import {
  useEffect,
  useId,
  useRef,
  useState,
  type FormEvent,
  type ReactNode,
} from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Archive, Minus, Pencil, Plus, X } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { api } from '@/lib/api/client'
import { uploadAdminImage } from '@/lib/media'
import { parseApiProblem } from '@/lib/api/problemDetails'
import { formatDate, formatMoney } from '@/lib/utils'
import type { components } from '@/types/api.generated'
import type { WritableAdminResource } from '@/config/adminResources'
import { ErrorState, LoadingState } from '@/components/ui/Feedback'
import { ImageFilePicker } from '@/components/admin/ImageFilePicker'
import { statusBadgeClass } from '@/lib/statusTone'

type Row = Record<string, unknown>
type AdminCategory = components['schemas']['AdminCategoryDto']
type AdminProduct = components['schemas']['AdminProductDto']
type AdminBanner = components['schemas']['AdminBannerResponse']
type AdminInventory = components['schemas']['AdminInventoryDto']
type Coupon = components['schemas']['CouponResponse']
type AdminCustomer = components['schemas']['AdminCustomerResponse']
type AdminContactMessage = components['schemas']['ContactMessageResponse']
type ContactMessageStatus = NonNullable<AdminContactMessage['status']>
type ApiErrors = Record<string, string[]>

export type ResourceWriteAction = 'create' | 'edit' | 'adjust' | 'archive'

const nullable = (value: string) => value.trim() || null
const localDateTime = (value?: string | null) => {
  if (!value) return ''
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return ''
  const local = new Date(date.getTime() - date.getTimezoneOffset() * 60_000)
  return local.toISOString().slice(0, 16)
}
const utcDateTime = (value: string) => value ? new Date(value).toISOString() : null
const numberOrNull = (value: string) => value === '' ? null : Number(value)
const normalizeErrorKey = (value: string) => value.replace(/[^a-z0-9]/gi, '').toLowerCase()

const errorFor = (errors: ApiErrors, ...fields: string[]) => {
  const wanted = fields.map(normalizeErrorKey)
  const entry = Object.entries(errors).find(([key]) => wanted.includes(normalizeErrorKey(key)))
  return entry?.[1]?.[0]
}

const translationFor = <T extends { language?: string }>(
  translations: T[] | null | undefined,
  language: 'Arabic' | 'English',
) => translations?.find((translation) => translation.language === language)

const optionName = (
  item: AdminCategory | AdminProduct,
  language: string,
) => {
  const preferred = language.startsWith('ar') ? 'Arabic' : 'English'
  return translationFor(item.translations, preferred)?.name
    ?? translationFor(item.translations, preferred === 'Arabic' ? 'English' : 'Arabic')?.name
    ?? item.id
    ?? '—'
}

const categoryBranchIds = (categories: AdminCategory[], rootId?: string) => {
  const blocked = new Set<string>()
  if (!rootId) return blocked
  blocked.add(rootId)
  let changed = true
  while (changed) {
    changed = false
    for (const category of categories) {
      if (
        category.id
        && category.parentCategoryId
        && blocked.has(category.parentCategoryId)
        && !blocked.has(category.id)
      ) {
        blocked.add(category.id)
        changed = true
      }
    }
  }
  return blocked
}

const extractRows = (data: unknown): Row[] => {
  if (Array.isArray(data)) return data.filter((item): item is Row => typeof item === 'object' && item !== null)
  if (typeof data !== 'object' || data === null) return []
  const items = (data as { items?: unknown }).items
  return Array.isArray(items)
    ? items.filter((item): item is Row => typeof item === 'object' && item !== null)
    : []
}

async function fetchAll(endpoint: string) {
  const first = await api.get(endpoint, { params: { page: 1, pageSize: 100 } })
  const firstRows = extractRows(first.data)
  const totalPages = Math.max(1, Number((first.data as { totalPages?: unknown } | undefined)?.totalPages ?? 1))
  if (totalPages === 1) return firstRows

  const remaining = await Promise.all(
    Array.from({ length: totalPages - 1 }, (_, index) =>
      api.get(endpoint, { params: { page: index + 2, pageSize: 100 } }),
    ),
  )
  return firstRows.concat(remaining.flatMap((response) => extractRows(response.data)))
}

function useAdminOptions(endpoint: string, enabled = true) {
  return useQuery({
    enabled,
    queryKey: ['admin-editor-options', endpoint],
    queryFn: () => fetchAll(endpoint),
    staleTime: 30_000,
  })
}

function FormField({
  children,
  error,
  label,
  required,
}: {
  children: ReactNode
  error?: string
  label: string
  required?: boolean
}) {
  return (
    <label className="grid min-w-0 gap-1.5 text-sm font-semibold">
      <span className="break-words [overflow-wrap:anywhere]">
        {label}
        {required && <span aria-hidden className="text-[var(--heritage-red)]"> *</span>}
      </span>
      {children}
      {error && <span className="break-words text-xs text-[var(--heritage-red)] [overflow-wrap:anywhere]" role="alert">{error}</span>}
    </label>
  )
}

function GeneralFormError({ message }: { message?: string }) {
  if (!message) return null
  return (
    <div className="alert alert-error font-normal" role="alert">
      {message}
    </div>
  )
}

function EditorDialogFrame({
  children,
  heading,
  onClose,
}: {
  children: ReactNode
  heading: string
  onClose: () => void
}) {
  const { t } = useTranslation()
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

  return (
    <div
      className="fixed inset-0 z-[90] grid place-items-center overflow-y-auto bg-black/60 p-3 sm:p-5"
      onMouseDown={(event) => {
        if (event.target === event.currentTarget) onClose()
      }}
      role="presentation"
    >
      <section
        aria-labelledby={headingId}
        aria-modal="true"
        className="card flex max-h-[calc(100dvh-1.5rem)] w-full max-w-5xl flex-col overflow-hidden"
        ref={dialogRef}
        role="dialog"
      >
        <header className="flex shrink-0 items-start justify-between gap-4 border-b p-4 sm:p-5">
          <h2 className="min-w-0 break-words text-xl font-extrabold [overflow-wrap:anywhere]" id={headingId}>{heading}</h2>
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
        <div className="min-h-0 flex-1 overflow-y-auto overscroll-contain p-4 sm:p-5">{children}</div>
      </section>
    </div>
  )
}

type TranslationDraft = {
  language: 'Arabic' | 'English'
  name: string
  slug: string
  description: string
  shortDescription?: string
  story?: string
  metaTitle: string
  metaDescription: string
}

function CategoryForm({
  mode,
  onClose,
  onSaved,
  row,
}: {
  mode: 'create' | 'edit'
  onClose: () => void
  onSaved: () => void
  row?: Row
}) {
  const { t, i18n } = useTranslation()
  const source = row as AdminCategory | undefined
  const categoriesQuery = useAdminOptions('/admin/categories')
  const [parentCategoryId, setParentCategoryId] = useState(source?.parentCategoryId ?? '')
  const [imageFile, setImageFile] = useState<File | null>(null)
  const [sortOrder, setSortOrder] = useState(source?.sortOrder ?? 0)
  const [isActive, setIsActive] = useState(source?.isActive ?? true)
  const [translations, setTranslations] = useState<TranslationDraft[]>(() =>
    (['Arabic', 'English'] as const).map((language) => {
      const existing = translationFor(source?.translations, language)
      return {
        language,
        name: existing?.name ?? '',
        slug: existing?.slug ?? '',
        description: existing?.description ?? '',
        metaTitle: existing?.metaTitle ?? '',
        metaDescription: existing?.metaDescription ?? '',
      }
    }),
  )
  const [errors, setErrors] = useState<ApiErrors>({})
  const [generalError, setGeneralError] = useState('')

  const mutation = useMutation({
    mutationFn: async () => {
      const imageUrl = imageFile
        ? (await uploadAdminImage(imageFile, 'categories')).url
        : source?.imageUrl ?? null
      const payload: components['schemas']['CreateCategoryRequest'] = {
        parentCategoryId: parentCategoryId || null,
        imageUrl,
        sortOrder,
        isActive,
        translations: translations.map((translation) => ({
          language: translation.language,
          name: translation.name.trim(),
          slug: translation.slug.trim(),
          description: nullable(translation.description),
          metaTitle: nullable(translation.metaTitle),
          metaDescription: nullable(translation.metaDescription),
        })),
      }
      return mode === 'create'
        ? api.post('/admin/categories', payload)
        : api.put(`/admin/categories/${source?.id}`, payload)
    },
    onSuccess: onSaved,
    onError: (error) => {
      const problem = parseApiProblem(error)
      setErrors(problem.errors)
      setGeneralError(problem.title)
    },
  })

  const setTranslation = (
    index: number,
    key: keyof Omit<TranslationDraft, 'language'>,
    value: string,
  ) => setTranslations((current) =>
    current.map((translation, itemIndex) =>
      itemIndex === index ? { ...translation, [key]: value } : translation,
    ))

  const submit = (event: FormEvent) => {
    event.preventDefault()
    const localErrors: ApiErrors = {}
    if (!Number.isInteger(sortOrder) || sortOrder < 0) {
      localErrors.sortOrder = [t('admin.editor.validation.wholeNonNegative')]
    }
    translations.forEach((translation, index) => {
      if (!translation.name.trim()) localErrors[`translations[${index}].name`] = [t('validation.required')]
      if (!translation.slug.trim()) localErrors[`translations[${index}].slug`] = [t('validation.required')]
    })
    setErrors(localErrors)
    setGeneralError('')
    if (Object.keys(localErrors).length) return
    mutation.mutate()
  }

  const categories = (categoriesQuery.data ?? []) as AdminCategory[]
  const blockedParents = categoryBranchIds(categories, source?.id)
  return (
    <form className="grid min-w-0 gap-5" noValidate onSubmit={submit}>
      <GeneralFormError message={generalError} />
      <div className="grid min-w-0 gap-4 sm:grid-cols-2">
        <FormField label={t('admin.editor.parentCategory')}>
          <select
            className="field"
            disabled={categoriesQuery.isLoading}
            onChange={(event) => setParentCategoryId(event.target.value)}
            value={parentCategoryId ?? ''}
          >
            <option value="">{t('admin.editor.noParent')}</option>
            {categories
              .filter((category) => !category.id || !blockedParents.has(category.id))
              .map((category) => (
                <option key={category.id} value={category.id}>
                  {optionName(category, i18n.language)}
                </option>
              ))}
          </select>
        </FormField>
        <FormField
          error={errorFor(errors, 'sortOrder')}
          label={t('admin.fields.sortOrder')}
          required
        >
          <input
            className="field"
            min={0}
            onChange={(event) => setSortOrder(Number(event.target.value))}
            type="number"
            value={sortOrder}
          />
        </FormField>
        <ImageFilePicker
          currentUrl={source?.imageUrl}
          disabled={mutation.isPending}
          error={errorFor(errors, 'imageFile', 'imageUrl')}
          label={t('admin.editor.categoryImage')}
          onError={(message) => setErrors((current) => ({ ...current, imageFile: message ? [message] : [] }))}
          onSelect={setImageFile}
          selectedFile={imageFile}
        />
        <label className="flex min-h-11 min-w-0 items-center gap-3 self-end rounded-xl border p-3 font-semibold">
          <input
            checked={isActive}
            onChange={(event) => setIsActive(event.target.checked)}
            className="shrink-0"
            type="checkbox"
          />
          {t('admin.fields.isActive')}
        </label>
      </div>
      <div className="grid min-w-0 gap-4 lg:grid-cols-2">
        {translations.map((translation, index) => (
          <fieldset className="grid min-w-0 gap-4 rounded-xl border p-4" key={translation.language}>
            <legend className="max-w-full break-words px-2 font-extrabold [overflow-wrap:anywhere]">{t(`admin.values.${translation.language}`)}</legend>
            <FormField
              error={errorFor(errors, `translations[${index}].name`)}
              label={t('admin.fields.name')}
              required
            >
              <input
                className="field"
                dir={translation.language === 'Arabic' ? 'rtl' : 'ltr'}
                maxLength={200}
                onChange={(event) => setTranslation(index, 'name', event.target.value)}
                value={translation.name}
              />
            </FormField>
            <FormField
              error={errorFor(errors, `translations[${index}].slug`)}
              label={t('admin.fields.slug')}
              required
            >
              <input
                className="field"
                dir="ltr"
                maxLength={250}
                onChange={(event) => setTranslation(index, 'slug', event.target.value)}
                value={translation.slug}
              />
            </FormField>
            <FormField
              error={errorFor(errors, `translations[${index}].description`)}
              label={t('admin.fields.description')}
            >
              <textarea
                className="field min-h-28 resize-y"
                dir={translation.language === 'Arabic' ? 'rtl' : 'ltr'}
                maxLength={4000}
                onChange={(event) => setTranslation(index, 'description', event.target.value)}
                value={translation.description}
              />
            </FormField>
            <FormField label={t('admin.editor.metaTitle')}>
              <input
                className="field"
                dir={translation.language === 'Arabic' ? 'rtl' : 'ltr'}
                maxLength={250}
                onChange={(event) => setTranslation(index, 'metaTitle', event.target.value)}
                value={translation.metaTitle}
              />
            </FormField>
            <FormField label={t('admin.editor.metaDescription')}>
              <textarea
                className="field min-h-20 resize-y"
                dir={translation.language === 'Arabic' ? 'rtl' : 'ltr'}
                maxLength={500}
                onChange={(event) => setTranslation(index, 'metaDescription', event.target.value)}
                value={translation.metaDescription}
              />
            </FormField>
          </fieldset>
        ))}
      </div>
      <FormActions onClose={onClose} pending={mutation.isPending} />
    </form>
  )
}

type VariantDraft = {
  id?: string | null
  sku: string
  size: string
  color: string
  weightInGrams: number | null
  capacity: string
  priceOverride: number | null
  isActive: boolean
  initialQuantity: number
  reorderLevel: number
  currentQuantity?: number
}

type ImageDraft = {
  id?: string | null
  imageUrl: string
  thumbnailUrl: string
  file?: File | null
  sortOrder: number
  isPrimary: boolean
  altArabic: string
  altEnglish: string
}

function ProductForm({
  mode,
  onClose,
  onSaved,
  row,
}: {
  mode: 'create' | 'edit'
  onClose: () => void
  onSaved: () => void
  row?: Row
}) {
  const { t, i18n } = useTranslation()
  const source = row as AdminProduct | undefined
  const categoriesQuery = useAdminOptions('/admin/categories')
  const [categoryId, setCategoryId] = useState(source?.categoryId ?? '')
  const [basePrice, setBasePrice] = useState(source?.basePrice ?? 0)
  const [status, setStatus] = useState(source?.status ?? 'Draft')
  const [isFeatured, setIsFeatured] = useState(source?.isFeatured ?? false)
  const [publishedAtUtc, setPublishedAtUtc] = useState(localDateTime(source?.publishedAtUtc))
  const [translations, setTranslations] = useState<TranslationDraft[]>(() =>
    (['Arabic', 'English'] as const).map((language) => {
      const existing = translationFor(source?.translations, language)
      return {
        language,
        name: existing?.name ?? '',
        slug: existing?.slug ?? '',
        shortDescription: existing?.shortDescription ?? '',
        description: existing?.description ?? '',
        story: existing?.story ?? '',
        metaTitle: existing?.metaTitle ?? '',
        metaDescription: existing?.metaDescription ?? '',
      }
    }),
  )
  const [variants, setVariants] = useState<VariantDraft[]>(() =>
    source?.variants?.length
      ? source.variants.map((variant) => ({
          id: variant.id,
          sku: variant.sku ?? '',
          size: variant.size ?? '',
          color: variant.color ?? '',
          weightInGrams: variant.weightInGrams ?? null,
          capacity: variant.capacity ?? '',
          priceOverride: variant.priceOverride ?? null,
          isActive: variant.isActive ?? true,
          initialQuantity: 0,
          reorderLevel: variant.inventory?.reorderLevel ?? 0,
          currentQuantity: variant.inventory?.quantityOnHand ?? 0,
        }))
      : [{
          sku: '',
          size: '',
          color: '',
          weightInGrams: null,
          capacity: '',
          priceOverride: null,
          isActive: true,
          initialQuantity: 0,
          reorderLevel: 5,
        }],
  )
  const [images, setImages] = useState<ImageDraft[]>(() =>
    source?.images?.map((image) => ({
      id: image.id,
      imageUrl: image.imageUrl ?? '',
      thumbnailUrl: image.thumbnailUrl ?? '',
      sortOrder: image.sortOrder ?? 0,
      isPrimary: image.isPrimary ?? false,
      altArabic: translationFor(image.translations, 'Arabic')?.altText ?? '',
      altEnglish: translationFor(image.translations, 'English')?.altText ?? '',
    })) ?? [],
  )
  const [errors, setErrors] = useState<ApiErrors>({})
  const [generalError, setGeneralError] = useState('')

  const mutation = useMutation({
    mutationFn: async () => {
      const uploadedImages = await Promise.all(images.map(async (image) => {
        if (!image.file) return image
        const uploaded = await uploadAdminImage(image.file, 'products')
        return { ...image, imageUrl: uploaded.url, thumbnailUrl: uploaded.url }
      }))
      const common = {
        categoryId,
        basePrice,
        currency: 'ILS' as const,
        status,
        isFeatured,
        publishedAtUtc: utcDateTime(publishedAtUtc),
        translations: translations.map((translation) => ({
          language: translation.language,
          name: translation.name.trim(),
          slug: translation.slug.trim(),
          shortDescription: nullable(translation.shortDescription ?? ''),
          description: translation.description.trim(),
          story: nullable(translation.story ?? ''),
          metaTitle: nullable(translation.metaTitle),
          metaDescription: nullable(translation.metaDescription),
        })),
      }
      const mappedImages = uploadedImages.map((image) => ({
        ...(mode === 'edit' ? { id: image.id ?? null } : {}),
        imageUrl: image.imageUrl.trim(),
        thumbnailUrl: image.thumbnailUrl.trim() || image.imageUrl.trim(),
        sortOrder: image.sortOrder,
        isPrimary: image.isPrimary,
        translations: [
          { language: 'Arabic' as const, altText: nullable(image.altArabic) },
          { language: 'English' as const, altText: nullable(image.altEnglish) },
        ],
      }))
      if (mode === 'create') {
        const payload: components['schemas']['CreateProductRequest'] = {
          ...common,
          variants: variants.map((variant) => ({
            sku: variant.sku.trim(),
            size: nullable(variant.size),
            color: nullable(variant.color),
            weightInGrams: variant.weightInGrams,
            capacity: nullable(variant.capacity),
            priceOverride: variant.priceOverride,
            isActive: variant.isActive,
            initialQuantity: variant.initialQuantity,
            reorderLevel: variant.reorderLevel,
          })),
          images: mappedImages,
        }
        return api.post('/admin/products', payload)
      }
      const payload: components['schemas']['UpdateProductRequest'] = {
        ...common,
        variants: variants.map((variant) => ({
          id: variant.id ?? null,
          sku: variant.sku.trim(),
          size: nullable(variant.size),
          color: nullable(variant.color),
          weightInGrams: variant.weightInGrams,
          capacity: nullable(variant.capacity),
          priceOverride: variant.priceOverride,
          isActive: variant.isActive,
          initialQuantity: variant.id ? null : variant.initialQuantity,
          reorderLevel: variant.reorderLevel,
        })),
        images: mappedImages,
      }
      return api.put(`/admin/products/${source?.id}`, payload)
    },
    onSuccess: onSaved,
    onError: (error) => {
      const problem = parseApiProblem(error)
      setErrors(problem.errors)
      setGeneralError(problem.title)
    },
  })

  const setTranslation = (
    index: number,
    key: keyof Omit<TranslationDraft, 'language'>,
    value: string,
  ) => setTranslations((current) =>
    current.map((translation, itemIndex) =>
      itemIndex === index ? { ...translation, [key]: value } : translation,
    ))
  const setVariant = <K extends keyof VariantDraft>(index: number, key: K, value: VariantDraft[K]) =>
    setVariants((current) => current.map((variant, itemIndex) =>
      itemIndex === index ? { ...variant, [key]: value } : variant,
    ))
  const setImage = <K extends keyof ImageDraft>(index: number, key: K, value: ImageDraft[K]) =>
    setImages((current) => current.map((image, itemIndex) =>
      itemIndex === index ? { ...image, [key]: value } : image,
    ))

  const submit = (event: FormEvent) => {
    event.preventDefault()
    const localErrors: ApiErrors = {}
    if (!categoryId) localErrors.categoryId = [t('validation.required')]
    if (!Number.isFinite(basePrice) || basePrice < 0) localErrors.basePrice = [t('admin.editor.validation.nonNegative')]
    translations.forEach((translation, index) => {
      if (!translation.name.trim()) localErrors[`translations[${index}].name`] = [t('validation.required')]
      if (!translation.slug.trim()) localErrors[`translations[${index}].slug`] = [t('validation.required')]
      if (!translation.description.trim()) localErrors[`translations[${index}].description`] = [t('validation.required')]
    })
    if (!variants.length) localErrors.variants = [t('admin.editor.validation.variantRequired')]
    variants.forEach((variant, index) => {
      if (!variant.sku.trim()) localErrors[`variants[${index}].sku`] = [t('validation.required')]
      if (!Number.isInteger(variant.initialQuantity) || variant.initialQuantity < 0) {
        localErrors[`variants[${index}].initialQuantity`] = [t('admin.editor.validation.wholeNonNegative')]
      }
      if (!Number.isInteger(variant.reorderLevel) || variant.reorderLevel < 0) {
        localErrors[`variants[${index}].reorderLevel`] = [t('admin.editor.validation.wholeNonNegative')]
      }
    })
    if (status === 'Active' && variants.every((variant) => !variant.isActive)) {
      localErrors.variants = [t('admin.editor.validation.activeVariant')]
    }
    images.forEach((image, index) => {
      if (!image.file && !image.imageUrl.trim()) localErrors[`images[${index}].imageUrl`] = [t('validation.required')]
      if (!Number.isInteger(image.sortOrder) || image.sortOrder < 0) {
        localErrors[`images[${index}].sortOrder`] = [t('admin.editor.validation.wholeNonNegative')]
      }
    })
    setErrors(localErrors)
    setGeneralError('')
    if (Object.keys(localErrors).length) return
    mutation.mutate()
  }

  const categories = (categoriesQuery.data ?? []) as AdminCategory[]
  return (
    <form className="grid min-w-0 gap-6" noValidate onSubmit={submit}>
      <GeneralFormError message={generalError} />
      <fieldset className="grid min-w-0 gap-4 rounded-xl border p-4">
        <legend className="max-w-full break-words px-2 font-extrabold [overflow-wrap:anywhere]">{t('admin.editor.productDetails')}</legend>
        <div className="grid min-w-0 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          <FormField error={errorFor(errors, 'categoryId')} label={t('admin.fields.categoryId')} required>
            <select
              className="field"
              disabled={categoriesQuery.isLoading}
              onChange={(event) => setCategoryId(event.target.value)}
              value={categoryId}
            >
              <option value="">{t('admin.editor.selectCategory')}</option>
              {categories.map((category) => (
                <option key={category.id} value={category.id}>
                  {optionName(category, i18n.language)}
                </option>
              ))}
            </select>
          </FormField>
          <FormField error={errorFor(errors, 'basePrice')} label={t('admin.fields.basePrice')} required>
            <input
              className="field"
              min={0}
              onChange={(event) => setBasePrice(Number(event.target.value))}
              step="0.01"
              type="number"
              value={basePrice}
            />
          </FormField>
          <FormField label={t('admin.fields.status')} required>
            <select className="field" onChange={(event) => setStatus(event.target.value as typeof status)} value={status}>
              {(['Draft', 'Active', 'OutOfStock', 'Hidden', 'Discontinued'] as const).map((value) => (
                <option key={value} value={value}>{t(`admin.values.${value}`)}</option>
              ))}
            </select>
          </FormField>
          <FormField label={t('admin.fields.publishedAtUtc')}>
            <input
              className="field"
              dir="ltr"
              onChange={(event) => setPublishedAtUtc(event.target.value)}
              type="datetime-local"
              value={publishedAtUtc}
            />
          </FormField>
          <div className="grid gap-2 self-end">
            <label className="flex min-h-11 min-w-0 items-center gap-3 rounded-xl border px-3">
              <input checked={isFeatured} className="shrink-0" onChange={(event) => setIsFeatured(event.target.checked)} type="checkbox" />
              {t('admin.fields.isFeatured')}
            </label>
          </div>
        </div>
      </fieldset>

      <div className="grid min-w-0 gap-4 lg:grid-cols-2">
        {translations.map((translation, index) => (
          <fieldset className="grid min-w-0 gap-4 rounded-xl border p-4" key={translation.language}>
            <legend className="max-w-full break-words px-2 font-extrabold [overflow-wrap:anywhere]">{t(`admin.values.${translation.language}`)}</legend>
            <FormField error={errorFor(errors, `translations[${index}].name`)} label={t('admin.fields.name')} required>
              <input
                className="field"
                dir={translation.language === 'Arabic' ? 'rtl' : 'ltr'}
                maxLength={200}
                onChange={(event) => setTranslation(index, 'name', event.target.value)}
                value={translation.name}
              />
            </FormField>
            <FormField error={errorFor(errors, `translations[${index}].slug`)} label={t('admin.fields.slug')} required>
              <input className="field" dir="ltr" maxLength={250} onChange={(event) => setTranslation(index, 'slug', event.target.value)} value={translation.slug} />
            </FormField>
            <FormField label={t('admin.editor.shortDescription')}>
              <textarea className="field min-h-20 resize-y" dir={translation.language === 'Arabic' ? 'rtl' : 'ltr'} maxLength={1000} onChange={(event) => setTranslation(index, 'shortDescription', event.target.value)} value={translation.shortDescription} />
            </FormField>
            <FormField error={errorFor(errors, `translations[${index}].description`)} label={t('admin.fields.description')} required>
              <textarea className="field min-h-32 resize-y" dir={translation.language === 'Arabic' ? 'rtl' : 'ltr'} maxLength={4000} onChange={(event) => setTranslation(index, 'description', event.target.value)} value={translation.description} />
            </FormField>
            <FormField label={t('admin.editor.story')}>
              <textarea className="field min-h-24 resize-y" dir={translation.language === 'Arabic' ? 'rtl' : 'ltr'} onChange={(event) => setTranslation(index, 'story', event.target.value)} value={translation.story} />
            </FormField>
            <FormField label={t('admin.editor.metaTitle')}>
              <input className="field" dir={translation.language === 'Arabic' ? 'rtl' : 'ltr'} maxLength={250} onChange={(event) => setTranslation(index, 'metaTitle', event.target.value)} value={translation.metaTitle} />
            </FormField>
            <FormField label={t('admin.editor.metaDescription')}>
              <textarea className="field min-h-20 resize-y" dir={translation.language === 'Arabic' ? 'rtl' : 'ltr'} maxLength={500} onChange={(event) => setTranslation(index, 'metaDescription', event.target.value)} value={translation.metaDescription} />
            </FormField>
          </fieldset>
        ))}
      </div>

      <fieldset className="grid min-w-0 gap-4 rounded-xl border p-4">
        <legend className="max-w-full break-words px-2 font-extrabold [overflow-wrap:anywhere]">{t('admin.editor.variants')}</legend>
        {errorFor(errors, 'variants') && <p className="text-sm text-[var(--heritage-red)]" role="alert">{errorFor(errors, 'variants')}</p>}
        {variants.map((variant, index) => (
          <div className="grid min-w-0 gap-3 rounded-xl bg-stone-50 p-3 lg:grid-cols-4" key={variant.id ?? `new-${index}`}>
            <FormField error={errorFor(errors, `variants[${index}].sku`)} label={t('admin.fields.sku')} required>
              <input className="field" dir="ltr" onChange={(event) => setVariant(index, 'sku', event.target.value)} value={variant.sku} />
            </FormField>
            <FormField label={t('admin.editor.size')}>
              <input className="field" onChange={(event) => setVariant(index, 'size', event.target.value)} value={variant.size} />
            </FormField>
            <FormField label={t('admin.editor.color')}>
              <input className="field" onChange={(event) => setVariant(index, 'color', event.target.value)} value={variant.color} />
            </FormField>
            <FormField label={t('admin.editor.capacity')}>
              <input className="field" onChange={(event) => setVariant(index, 'capacity', event.target.value)} value={variant.capacity} />
            </FormField>
            <FormField label={t('admin.editor.weight')}>
              <input className="field" min={0} onChange={(event) => setVariant(index, 'weightInGrams', numberOrNull(event.target.value))} step="0.01" type="number" value={variant.weightInGrams ?? ''} />
            </FormField>
            <FormField label={t('admin.editor.priceOverride')}>
              <input className="field" min={0} onChange={(event) => setVariant(index, 'priceOverride', numberOrNull(event.target.value))} step="0.01" type="number" value={variant.priceOverride ?? ''} />
            </FormField>
            {variant.id
              ? (
                <FormField label={t('admin.fields.quantityOnHand')}>
                  <input className="field" disabled type="number" value={variant.currentQuantity ?? 0} />
                </FormField>
              )
              : (
                <FormField error={errorFor(errors, `variants[${index}].initialQuantity`)} label={t('admin.editor.initialQuantity')} required>
                  <input className="field" min={0} onChange={(event) => setVariant(index, 'initialQuantity', Number(event.target.value))} type="number" value={variant.initialQuantity} />
                </FormField>
              )}
            <FormField error={errorFor(errors, `variants[${index}].reorderLevel`)} label={t('admin.fields.reorderLevel')} required>
              <input className="field" min={0} onChange={(event) => setVariant(index, 'reorderLevel', Number(event.target.value))} type="number" value={variant.reorderLevel} />
            </FormField>
            <label className="flex min-h-11 min-w-0 items-center gap-3 rounded-xl border bg-white px-3 font-semibold">
              <input checked={variant.isActive} className="shrink-0" onChange={(event) => setVariant(index, 'isActive', event.target.checked)} type="checkbox" />
              {t('admin.fields.isActive')}
            </label>
            <button
              className="button-secondary self-end"
              disabled={variants.length === 1}
              onClick={() => setVariants((current) => current.filter((_, itemIndex) => itemIndex !== index))}
              type="button"
            >
              <Minus aria-hidden size={17} />
              {t('admin.editor.removeVariant')}
            </button>
          </div>
        ))}
        <button
          className="button-secondary justify-self-start"
          onClick={() => setVariants((current) => [...current, {
            sku: '',
            size: '',
            color: '',
            weightInGrams: null,
            capacity: '',
            priceOverride: null,
            isActive: true,
            initialQuantity: 0,
            reorderLevel: 5,
          }])}
          type="button"
        >
          <Plus aria-hidden size={17} />
          {t('admin.editor.addVariant')}
        </button>
      </fieldset>

      <fieldset className="grid min-w-0 gap-4 rounded-xl border p-4">
        <legend className="max-w-full break-words px-2 font-extrabold [overflow-wrap:anywhere]">{t('admin.editor.images')}</legend>
        {images.map((image, index) => (
          <div className="grid min-w-0 gap-3 rounded-xl bg-stone-50 p-3 lg:grid-cols-2" key={image.id ?? `new-${index}`}>
            <div className="lg:col-span-2">
              <ImageFilePicker
                currentUrl={image.imageUrl}
                disabled={mutation.isPending}
                error={errorFor(errors, `images[${index}].imageUrl`)}
                label={t('admin.editor.productImage', { count: index + 1 })}
                onError={(message) => setErrors((current) => ({
                  ...current,
                  [`images[${index}].imageUrl`]: message ? [message] : [],
                }))}
                onSelect={(file) => setImage(index, 'file', file)}
                selectedFile={image.file}
              />
            </div>
            <FormField label={t('admin.editor.altArabic')}>
              <input className="field" dir="rtl" maxLength={500} onChange={(event) => setImage(index, 'altArabic', event.target.value)} value={image.altArabic} />
            </FormField>
            <FormField label={t('admin.editor.altEnglish')}>
              <input className="field" dir="ltr" maxLength={500} onChange={(event) => setImage(index, 'altEnglish', event.target.value)} value={image.altEnglish} />
            </FormField>
            <FormField error={errorFor(errors, `images[${index}].sortOrder`)} label={t('admin.fields.sortOrder')}>
              <input className="field" min={0} onChange={(event) => setImage(index, 'sortOrder', Number(event.target.value))} type="number" value={image.sortOrder} />
            </FormField>
            <label className="flex min-h-11 min-w-0 items-center gap-3 rounded-xl border bg-white px-3 font-semibold">
              <input
                checked={image.isPrimary}
                className="shrink-0"
                onChange={(event) => {
                  const checked = event.target.checked
                  setImages((current) => current.map((item, itemIndex) => ({
                    ...item,
                    isPrimary: checked ? itemIndex === index : itemIndex === index ? false : item.isPrimary,
                  })))
                }}
                type="checkbox"
              />
              {t('admin.editor.primaryImage')}
            </label>
            <button className="button-secondary justify-self-start" onClick={() => setImages((current) => current.filter((_, itemIndex) => itemIndex !== index))} type="button">
              <Minus aria-hidden size={17} />
              {t('admin.editor.removeImage')}
            </button>
          </div>
        ))}
        <button
          className="button-secondary justify-self-start"
          onClick={() => setImages((current) => [...current, {
            imageUrl: '',
            thumbnailUrl: '',
            sortOrder: current.length,
            isPrimary: current.length === 0,
            altArabic: '',
            altEnglish: '',
          }])}
          type="button"
        >
          <Plus aria-hidden size={17} />
          {t('admin.editor.addImage')}
        </button>
      </fieldset>
      <FormActions onClose={onClose} pending={mutation.isPending} />
    </form>
  )
}

type BannerTranslationDraft = {
  language: 'Arabic' | 'English'
  title: string
  desktopImageUrl: string
  mobileImageUrl: string
  desktopFile: File | null
  mobileFile: File | null
  altText: string
  linkUrl: string
}

function BannerForm({
  mode,
  onClose,
  onSaved,
  row,
}: {
  mode: 'create' | 'edit'
  onClose: () => void
  onSaved: () => void
  row?: Row
}) {
  const { t } = useTranslation()
  const source = row as AdminBanner | undefined
  const [position, setPosition] = useState(source?.position ?? 'HomeMain')
  const [sortOrder, setSortOrder] = useState(source?.sortOrder ?? 0)
  const [isActive, setIsActive] = useState(source?.isActive ?? true)
  const [startsAtUtc, setStartsAtUtc] = useState(localDateTime(source?.startsAtUtc))
  const [endsAtUtc, setEndsAtUtc] = useState(localDateTime(source?.endsAtUtc))
  const [translations, setTranslations] = useState<BannerTranslationDraft[]>(() =>
    (['Arabic', 'English'] as const).map((language) => {
      const current = source?.translations?.find((translation) => translation.language === language)
      return {
        language,
        title: current?.title ?? '',
        desktopImageUrl: current?.desktopImageUrl ?? '',
        mobileImageUrl: current?.mobileImageUrl ?? '',
        desktopFile: null,
        mobileFile: null,
        altText: current?.altText ?? '',
        linkUrl: current?.linkUrl ?? '',
      }
    }),
  )
  const [errors, setErrors] = useState<ApiErrors>({})
  const [generalError, setGeneralError] = useState('')

  const setTranslation = <Key extends keyof BannerTranslationDraft>(
    index: number,
    key: Key,
    value: BannerTranslationDraft[Key],
  ) => setTranslations((current) => current.map((translation, itemIndex) =>
    itemIndex === index ? { ...translation, [key]: value } : translation,
  ))

  const mutation = useMutation({
    mutationFn: async () => {
      const uploaded = await Promise.all(translations.map(async (translation) => {
        const [desktop, mobile] = await Promise.all([
          translation.desktopFile
            ? uploadAdminImage(translation.desktopFile, 'banners')
            : Promise.resolve(null),
          translation.mobileFile
            ? uploadAdminImage(translation.mobileFile, 'banners')
            : Promise.resolve(null),
        ])
        return {
          language: translation.language,
          title: translation.title.trim(),
          desktopImageUrl: desktop?.url ?? translation.desktopImageUrl,
          mobileImageUrl: mobile?.url ?? translation.mobileImageUrl,
          altText: translation.altText.trim(),
          linkUrl: nullable(translation.linkUrl),
        }
      }))
      const payload: components['schemas']['SaveBannerRequest'] = {
        position,
        sortOrder,
        isActive,
        startsAtUtc: utcDateTime(startsAtUtc),
        endsAtUtc: utcDateTime(endsAtUtc),
        translations: uploaded,
      }
      return mode === 'create'
        ? api.post('/admin/banners', payload)
        : api.put(`/admin/banners/${source?.id}`, payload)
    },
    onSuccess: onSaved,
    onError: (error) => {
      const problem = parseApiProblem(error)
      setErrors(problem.errors)
      setGeneralError(problem.title)
    },
  })

  const submit = (event: FormEvent) => {
    event.preventDefault()
    const next: ApiErrors = {}
    if (!Number.isInteger(sortOrder) || sortOrder < 0) {
      next.sortOrder = [t('admin.editor.validation.wholeNonNegative')]
    }
    if (startsAtUtc && endsAtUtc && new Date(startsAtUtc) >= new Date(endsAtUtc)) {
      next.endsAtUtc = [t('admin.editor.validation.endAfterStart')]
    }
    translations.forEach((translation, index) => {
      if (!translation.desktopFile && !translation.desktopImageUrl) {
        next[`translations[${index}].desktopImageUrl`] = [t('validation.required')]
      }
      if (!translation.mobileFile && !translation.mobileImageUrl) {
        next[`translations[${index}].mobileImageUrl`] = [t('validation.required')]
      }
      if (translation.title.length > 300) {
        next[`translations[${index}].title`] = [t('admin.editor.validation.maximumLength', { count: 300 })]
      }
      if (translation.altText.length > 500) {
        next[`translations[${index}].altText`] = [t('admin.editor.validation.maximumLength', { count: 500 })]
      }
    })
    setErrors(next)
    setGeneralError('')
    if (Object.keys(next).length) return
    mutation.mutate()
  }

  return (
    <form className="grid min-w-0 gap-5" noValidate onSubmit={submit}>
      <GeneralFormError message={generalError} />
      <div className="grid min-w-0 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <FormField label={t('admin.editor.bannerPosition')} required>
          <select className="field" onChange={(event) => setPosition(event.target.value as typeof position)} value={position}>
            {(['HomeMain', 'HomeSecondary', 'Category', 'Footer'] as const).map((value) => (
              <option key={value} value={value}>{t(`admin.values.${value}`)}</option>
            ))}
          </select>
        </FormField>
        <FormField error={errorFor(errors, 'sortOrder')} label={t('admin.fields.sortOrder')} required>
          <input className="field" min={0} onChange={(event) => setSortOrder(Number(event.target.value))} type="number" value={sortOrder} />
        </FormField>
        <label className="flex min-h-11 min-w-0 items-center gap-3 self-end rounded-xl border px-3 font-semibold">
          <input checked={isActive} onChange={(event) => setIsActive(event.target.checked)} type="checkbox" />
          {t('admin.fields.isActive')}
        </label>
        <FormField label={t('admin.editor.startsAt')}>
          <input className="field" dir="ltr" onChange={(event) => setStartsAtUtc(event.target.value)} type="datetime-local" value={startsAtUtc} />
        </FormField>
        <FormField error={errorFor(errors, 'endsAtUtc')} label={t('admin.editor.endsAt')}>
          <input className="field" dir="ltr" onChange={(event) => setEndsAtUtc(event.target.value)} type="datetime-local" value={endsAtUtc} />
        </FormField>
      </div>
      <div className="grid min-w-0 gap-4 lg:grid-cols-2">
        {translations.map((translation, index) => (
          <fieldset className="grid min-w-0 gap-4 rounded-xl border p-4" key={translation.language}>
            <legend className="px-2 font-extrabold">{t(`admin.values.${translation.language}`)}</legend>
            <FormField error={errorFor(errors, `translations[${index}].title`)} label={t('admin.fields.title')}>
              <input
                className="field"
                dir={translation.language === 'Arabic' ? 'rtl' : 'ltr'}
                maxLength={300}
                onChange={(event) => setTranslation(index, 'title', event.target.value)}
                value={translation.title}
              />
            </FormField>
            <ImageFilePicker
              currentUrl={translation.desktopImageUrl}
              disabled={mutation.isPending}
              error={errorFor(errors, `translations[${index}].desktopImageUrl`)}
              label={t('admin.editor.desktopBannerImage')}
              onError={(message) => setErrors((current) => ({ ...current, [`translations[${index}].desktopImageUrl`]: message ? [message] : [] }))}
              onSelect={(file) => setTranslation(index, 'desktopFile', file)}
              selectedFile={translation.desktopFile}
            />
            <ImageFilePicker
              currentUrl={translation.mobileImageUrl}
              disabled={mutation.isPending}
              error={errorFor(errors, `translations[${index}].mobileImageUrl`)}
              label={t('admin.editor.mobileBannerImage')}
              onError={(message) => setErrors((current) => ({ ...current, [`translations[${index}].mobileImageUrl`]: message ? [message] : [] }))}
              onSelect={(file) => setTranslation(index, 'mobileFile', file)}
              selectedFile={translation.mobileFile}
            />
            <FormField error={errorFor(errors, `translations[${index}].altText`)} label={t('admin.editor.imageAltText')}>
              <input
                className="field"
                dir={translation.language === 'Arabic' ? 'rtl' : 'ltr'}
                maxLength={500}
                onChange={(event) => setTranslation(index, 'altText', event.target.value)}
                value={translation.altText}
              />
            </FormField>
            <FormField label={t('admin.editor.bannerLink')}>
              <input className="field" dir="ltr" maxLength={2000} onChange={(event) => setTranslation(index, 'linkUrl', event.target.value)} value={translation.linkUrl} />
            </FormField>
          </fieldset>
        ))}
      </div>
      <FormActions onClose={onClose} pending={mutation.isPending} />
    </form>
  )
}

function CouponForm({
  mode,
  onClose,
  onSaved,
  row,
}: {
  mode: 'create' | 'edit'
  onClose: () => void
  onSaved: () => void
  row?: Row
}) {
  const { t, i18n } = useTranslation()
  const source = row as Coupon | undefined
  const categoriesQuery = useAdminOptions('/admin/categories')
  const productsQuery = useAdminOptions('/admin/products')
  const [code, setCode] = useState(source?.code ?? '')
  const [discountType, setDiscountType] = useState(source?.discountType ?? 'Percentage')
  const [discountValue, setDiscountValue] = useState(source?.discountValue ?? 0)
  const [minimumOrderAmount, setMinimumOrderAmount] = useState<number | null>(source?.minimumOrderAmount ?? null)
  const [maximumDiscountAmount, setMaximumDiscountAmount] = useState<number | null>(source?.maximumDiscountAmount ?? null)
  const [startsAtUtc, setStartsAtUtc] = useState(localDateTime(source?.startsAtUtc))
  const [endsAtUtc, setEndsAtUtc] = useState(localDateTime(source?.endsAtUtc))
  const [totalUsageLimit, setTotalUsageLimit] = useState<number | null>(source?.totalUsageLimit ?? null)
  const [usageLimitPerCustomer, setUsageLimitPerCustomer] = useState<number | null>(source?.usageLimitPerCustomer ?? null)
  const [status, setStatus] = useState(source?.status ?? 'Draft')
  const [includedCategoryIds, setIncludedCategoryIds] = useState<string[]>(source?.includedCategoryIds ?? [])
  const [includedProductIds, setIncludedProductIds] = useState<string[]>(source?.includedProductIds ?? [])
  const [excludedProductIds, setExcludedProductIds] = useState<string[]>(source?.excludedProductIds ?? [])
  const [errors, setErrors] = useState<ApiErrors>({})
  const [generalError, setGeneralError] = useState('')

  const mutation = useMutation({
    mutationFn: () => {
      const payload: components['schemas']['SaveCouponRequest'] = {
        code: code.trim(),
        discountType,
        discountValue,
        minimumOrderAmount,
        maximumDiscountAmount,
        startsAtUtc: utcDateTime(startsAtUtc),
        endsAtUtc: utcDateTime(endsAtUtc),
        totalUsageLimit,
        usageLimitPerCustomer,
        status,
        includedCategoryIds,
        includedProductIds,
        excludedProductIds,
      }
      return mode === 'create'
        ? api.post('/admin/coupons', payload)
        : api.put(`/admin/coupons/${source?.id}`, payload)
    },
    onSuccess: onSaved,
    onError: (error) => {
      const problem = parseApiProblem(error)
      setErrors(problem.errors)
      setGeneralError(problem.title)
    },
  })

  const selectedValues = (event: React.ChangeEvent<HTMLSelectElement>) =>
    Array.from(event.target.selectedOptions, (option) => option.value)
  const submit = (event: FormEvent) => {
    event.preventDefault()
    const localErrors: ApiErrors = {}
    if (!code.trim()) localErrors.code = [t('validation.required')]
    if (!Number.isFinite(discountValue) || discountValue <= 0 || (discountType === 'Percentage' && discountValue > 100)) {
      localErrors.discountValue = [t('admin.editor.validation.discountValue')]
    }
    if (minimumOrderAmount !== null && minimumOrderAmount < 0) localErrors.minimumOrderAmount = [t('admin.editor.validation.nonNegative')]
    if (maximumDiscountAmount !== null && maximumDiscountAmount < 0) localErrors.maximumDiscountAmount = [t('admin.editor.validation.nonNegative')]
    if (totalUsageLimit !== null && totalUsageLimit <= 0) localErrors.totalUsageLimit = [t('admin.editor.validation.positive')]
    if (usageLimitPerCustomer !== null && usageLimitPerCustomer <= 0) localErrors.usageLimitPerCustomer = [t('admin.editor.validation.positive')]
    if (startsAtUtc && endsAtUtc && new Date(startsAtUtc) >= new Date(endsAtUtc)) {
      localErrors.endsAtUtc = [t('admin.editor.validation.endAfterStart')]
    }
    if (includedProductIds.some((id) => excludedProductIds.includes(id))) {
      localErrors.includedProductIds = [t('admin.editor.validation.productScopeConflict')]
    }
    setErrors(localErrors)
    setGeneralError('')
    if (Object.keys(localErrors).length) return
    mutation.mutate()
  }

  const categories = (categoriesQuery.data ?? []) as AdminCategory[]
  const products = (productsQuery.data ?? []) as AdminProduct[]
  return (
    <form className="grid min-w-0 gap-5" noValidate onSubmit={submit}>
      <GeneralFormError message={generalError} />
      <div className="grid min-w-0 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <FormField error={errorFor(errors, 'code')} label={t('admin.fields.code')} required>
          <input className="field" dir="ltr" maxLength={100} onChange={(event) => setCode(event.target.value)} value={code} />
        </FormField>
        <FormField label={t('admin.editor.discountType')} required>
          <select className="field" onChange={(event) => setDiscountType(event.target.value as typeof discountType)} value={discountType}>
            <option value="Percentage">{t('admin.editor.percentage')}</option>
            <option value="FixedAmount">{t('admin.editor.fixedAmount')}</option>
          </select>
        </FormField>
        <FormField error={errorFor(errors, 'discountValue')} label={t('admin.editor.discountValue')} required>
          <input className="field" min={0.01} onChange={(event) => setDiscountValue(Number(event.target.value))} step="0.01" type="number" value={discountValue} />
        </FormField>
        <FormField error={errorFor(errors, 'minimumOrderAmount')} label={t('admin.editor.minimumOrderAmount')}>
          <input className="field" min={0} onChange={(event) => setMinimumOrderAmount(numberOrNull(event.target.value))} step="0.01" type="number" value={minimumOrderAmount ?? ''} />
        </FormField>
        <FormField error={errorFor(errors, 'maximumDiscountAmount')} label={t('admin.editor.maximumDiscountAmount')}>
          <input className="field" min={0} onChange={(event) => setMaximumDiscountAmount(numberOrNull(event.target.value))} step="0.01" type="number" value={maximumDiscountAmount ?? ''} />
        </FormField>
        <FormField label={t('admin.fields.status')} required>
          <select className="field" onChange={(event) => setStatus(event.target.value as typeof status)} value={status}>
            {(['Draft', 'Active', 'Expired', 'Disabled'] as const).map((value) => (
              <option key={value} value={value}>{t(`admin.values.${value}`)}</option>
            ))}
          </select>
        </FormField>
        <FormField label={t('admin.editor.startsAt')}>
          <input className="field" dir="ltr" onChange={(event) => setStartsAtUtc(event.target.value)} type="datetime-local" value={startsAtUtc} />
        </FormField>
        <FormField error={errorFor(errors, 'endsAtUtc')} label={t('admin.editor.endsAt')}>
          <input className="field" dir="ltr" onChange={(event) => setEndsAtUtc(event.target.value)} type="datetime-local" value={endsAtUtc} />
        </FormField>
        <FormField error={errorFor(errors, 'totalUsageLimit')} label={t('admin.editor.totalUsageLimit')}>
          <input className="field" min={1} onChange={(event) => setTotalUsageLimit(numberOrNull(event.target.value))} type="number" value={totalUsageLimit ?? ''} />
        </FormField>
        <FormField error={errorFor(errors, 'usageLimitPerCustomer')} label={t('admin.editor.usageLimitPerCustomer')}>
          <input className="field" min={1} onChange={(event) => setUsageLimitPerCustomer(numberOrNull(event.target.value))} type="number" value={usageLimitPerCustomer ?? ''} />
        </FormField>
      </div>
      <fieldset className="grid min-w-0 gap-4 rounded-xl border p-4">
        <legend className="max-w-full break-words px-2 font-extrabold [overflow-wrap:anywhere]">{t('admin.editor.couponScope')}</legend>
        <p className="muted text-sm">{t('admin.editor.multiSelectHelp')}</p>
        <div className="grid min-w-0 gap-4 lg:grid-cols-3">
          <FormField label={t('admin.editor.includedCategories')}>
            <select
              className="field min-h-40"
              disabled={categoriesQuery.isLoading}
              multiple
              onChange={(event) => setIncludedCategoryIds(selectedValues(event))}
              value={includedCategoryIds}
            >
              {categories.map((category) => <option key={category.id} value={category.id}>{optionName(category, i18n.language)}</option>)}
            </select>
          </FormField>
          <FormField error={errorFor(errors, 'includedProductIds')} label={t('admin.editor.includedProducts')}>
            <select
              className="field min-h-40"
              disabled={productsQuery.isLoading}
              multiple
              onChange={(event) => setIncludedProductIds(selectedValues(event))}
              value={includedProductIds}
            >
              {products.map((product) => <option key={product.id} value={product.id}>{optionName(product, i18n.language)}</option>)}
            </select>
          </FormField>
          <FormField label={t('admin.editor.excludedProducts')}>
            <select
              className="field min-h-40"
              disabled={productsQuery.isLoading}
              multiple
              onChange={(event) => setExcludedProductIds(selectedValues(event))}
              value={excludedProductIds}
            >
              {products.map((product) => <option key={product.id} value={product.id}>{optionName(product, i18n.language)}</option>)}
            </select>
          </FormField>
        </div>
      </fieldset>
      <FormActions onClose={onClose} pending={mutation.isPending} />
    </form>
  )
}

function InventoryAdjustmentForm({
  onClose,
  onSaved,
  row,
}: {
  onClose: () => void
  onSaved: () => void
  row: Row
}) {
  const { t } = useTranslation()
  const source = row as AdminInventory
  const [direction, setDirection] = useState<'increase' | 'decrease'>('increase')
  const [amount, setAmount] = useState(1)
  const [reference, setReference] = useState('')
  const [notes, setNotes] = useState('')
  const [errors, setErrors] = useState<ApiErrors>({})
  const [generalError, setGeneralError] = useState('')
  const mutation = useMutation({
    mutationFn: () => {
      const quantityChange = direction === 'increase' ? amount : -amount
      const payload: components['schemas']['InventoryAdjustmentRequest'] = {
        quantityChange,
        type: direction === 'increase' ? 'AdjustmentIncrease' : 'AdjustmentDecrease',
        reference: nullable(reference),
        notes: nullable(notes),
        rowVersion: source.rowVersion,
      }
      return api.post(`/admin/inventory/${source.productVariantId}/adjust`, payload)
    },
    onSuccess: onSaved,
    onError: (error) => {
      const problem = parseApiProblem(error)
      setErrors(problem.errors)
      setGeneralError(problem.title)
    },
  })
  const submit = (event: FormEvent) => {
    event.preventDefault()
    const localErrors: ApiErrors = {}
    if (!Number.isInteger(amount) || amount <= 0) localErrors.quantityChange = [t('admin.editor.validation.wholePositive')]
    if (direction === 'decrease' && amount > (source.quantityOnHand ?? 0)) {
      localErrors.quantityChange = [t('admin.editor.validation.insufficientStock')]
    }
    if (!source.rowVersion) localErrors.rowVersion = [t('admin.editor.validation.reloadInventory')]
    setErrors(localErrors)
    setGeneralError('')
    if (Object.keys(localErrors).length) return
    mutation.mutate()
  }

  return (
    <form className="grid min-w-0 gap-5" noValidate onSubmit={submit}>
      <GeneralFormError message={generalError} />
      <div className="rounded-xl bg-[var(--heritage-beige)] p-4">
        <p className="break-words font-extrabold [overflow-wrap:anywhere]" dir="auto">{source.productName}</p>
        <p className="break-all text-start text-sm" dir="ltr">{source.sku}</p>
        <p className="muted mt-2 text-sm">
          {t('admin.editor.currentStock', { count: source.quantityOnHand ?? 0 })}
          {' · '}
          {t('admin.editor.availableStock', { count: source.availableQuantity ?? 0 })}
        </p>
      </div>
      <fieldset className="grid gap-3">
        <legend className="font-bold">{t('admin.editor.adjustmentDirection')}</legend>
        <div className="grid gap-3 sm:grid-cols-2">
          <label className="flex min-h-11 min-w-0 items-center gap-3 rounded-xl border p-3 font-semibold">
            <input checked={direction === 'increase'} className="shrink-0" name="direction" onChange={() => setDirection('increase')} type="radio" />
            {t('admin.editor.increaseStock')}
          </label>
          <label className="flex min-h-11 min-w-0 items-center gap-3 rounded-xl border p-3 font-semibold">
            <input checked={direction === 'decrease'} className="shrink-0" name="direction" onChange={() => setDirection('decrease')} type="radio" />
            {t('admin.editor.decreaseStock')}
          </label>
        </div>
      </fieldset>
      <div className="grid gap-4 sm:grid-cols-2">
        <FormField error={errorFor(errors, 'quantityChange')} label={t('admin.editor.adjustmentAmount')} required>
          <input className="field" min={1} onChange={(event) => setAmount(Number(event.target.value))} step={1} type="number" value={amount} />
        </FormField>
        <FormField error={errorFor(errors, 'reference')} label={t('admin.editor.reference')}>
          <input className="field technical-input" dir="ltr" maxLength={250} onChange={(event) => setReference(event.target.value)} value={reference} />
        </FormField>
      </div>
      <FormField error={errorFor(errors, 'notes')} label={t('admin.editor.notes')}>
        <textarea className="field min-h-28 resize-y" maxLength={2000} onChange={(event) => setNotes(event.target.value)} value={notes} />
      </FormField>
      {errorFor(errors, 'rowVersion') && <p className="text-sm text-[var(--heritage-red)]" role="alert">{errorFor(errors, 'rowVersion')}</p>}
      <FormActions onClose={onClose} pending={mutation.isPending} saveLabel={t('admin.editor.applyAdjustment')} />
    </form>
  )
}

function ArchiveForm({
  endpoint,
  onClose,
  onSaved,
}: {
  endpoint: string
  onClose: () => void
  onSaved: () => void
}) {
  const { t } = useTranslation()
  const [generalError, setGeneralError] = useState('')
  const mutation = useMutation({
    mutationFn: () => api.delete(endpoint),
    onSuccess: onSaved,
    onError: (error) => {
      const problem = parseApiProblem(error)
      setGeneralError(problem.title)
    },
  })
  return (
      <div className="grid min-w-0 gap-5">
      <GeneralFormError message={generalError} />
      <p className="break-words [overflow-wrap:anywhere]">{t('admin.editor.archiveMessage')}</p>
      <p className="muted break-words text-sm [overflow-wrap:anywhere]">{t('admin.editor.archiveHelp')}</p>
      <div className="flex flex-col-reverse gap-3 sm:flex-row sm:flex-wrap sm:justify-end">
        <button className="button-secondary w-full whitespace-normal sm:w-auto" disabled={mutation.isPending} onClick={onClose} type="button">
          {t('admin.editor.cancel')}
        </button>
        <button className="button-primary w-full whitespace-normal bg-[var(--heritage-red)] sm:w-auto" disabled={mutation.isPending} onClick={() => mutation.mutate()} type="button">
          <Archive aria-hidden size={17} />
          {mutation.isPending ? t('states.saving') : t('admin.editor.confirmArchive')}
        </button>
      </div>
    </div>
  )
}

function CustomerEditForm({
  customer,
  onCancel,
  onSaved,
}: {
  customer: AdminCustomer
  onCancel: () => void
  onSaved: () => Promise<void>
}) {
  const { t } = useTranslation()
  const [fullName, setFullName] = useState(customer.fullName ?? '')
  const [email, setEmail] = useState(customer.email ?? '')
  const [preferredLanguage, setPreferredLanguage] = useState(customer.preferredLanguage ?? 'Arabic')
  const [administrativeNotes, setAdministrativeNotes] = useState(customer.administrativeNotes ?? '')
  const [errors, setErrors] = useState<ApiErrors>({})
  const [generalError, setGeneralError] = useState('')
  const mutation = useMutation({
    mutationFn: () => {
      const payload: components['schemas']['UpdateAdminCustomerRequest'] = {
        fullName: fullName.trim(),
        email: nullable(email),
        preferredLanguage,
        administrativeNotes: nullable(administrativeNotes),
      }
      return api.put(`/admin/customers/${customer.id}`, payload)
    },
    onSuccess: async () => {
      toast.success(t('admin.editor.success.customerUpdated'))
      await onSaved()
    },
    onError: (error) => {
      const problem = parseApiProblem(error)
      setErrors(problem.errors)
      setGeneralError(problem.title)
    },
  })
  const submit = (event: FormEvent) => {
    event.preventDefault()
    const localErrors: ApiErrors = {}
    if (!fullName.trim()) localErrors.fullName = [t('validation.required')]
    if (fullName.trim().length > 200) localErrors.fullName = [t('admin.editor.validation.maximumLength', { count: 200 })]
    if (email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) localErrors.email = [t('validation.email')]
    if (administrativeNotes.length > 4000) {
      localErrors.administrativeNotes = [t('admin.editor.validation.maximumLength', { count: 4000 })]
    }
    setErrors(localErrors)
    setGeneralError('')
    if (Object.keys(localErrors).length) return
    mutation.mutate()
  }

  return (
    <form className="grid min-w-0 gap-4 rounded-xl border p-4" noValidate onSubmit={submit}>
      <h3 className="font-extrabold">{t('admin.editor.customer.editDetails')}</h3>
      <GeneralFormError message={generalError} />
      <div className="grid min-w-0 gap-4 sm:grid-cols-2">
        <FormField error={errorFor(errors, 'fullName')} label={t('admin.fields.fullName')} required>
          <input className="field" maxLength={200} onChange={(event) => setFullName(event.target.value)} value={fullName} />
        </FormField>
        <FormField error={errorFor(errors, 'email')} label={t('admin.fields.email')}>
          <input className="field" dir="ltr" maxLength={320} onChange={(event) => setEmail(event.target.value)} type="email" value={email} />
        </FormField>
        <FormField label={t('admin.fields.preferredLanguage')} required>
          <select className="field" onChange={(event) => setPreferredLanguage(event.target.value as typeof preferredLanguage)} value={preferredLanguage}>
            <option value="Arabic">{t('admin.values.Arabic')}</option>
            <option value="English">{t('admin.values.English')}</option>
          </select>
        </FormField>
      </div>
      <FormField error={errorFor(errors, 'administrativeNotes')} label={t('admin.fields.administrativeNotes')}>
        <textarea
          className="field min-h-28 resize-y"
          maxLength={4000}
          onChange={(event) => setAdministrativeNotes(event.target.value)}
          value={administrativeNotes}
        />
      </FormField>
      <div className="flex flex-col-reverse gap-3 sm:flex-row sm:flex-wrap sm:justify-end">
        <button className="button-secondary w-full whitespace-normal sm:w-auto" disabled={mutation.isPending} onClick={onCancel} type="button">
          {t('admin.editor.cancel')}
        </button>
        <button className="button-primary w-full whitespace-normal sm:w-auto" disabled={mutation.isPending} type="submit">
          {mutation.isPending ? t('states.saving') : t('actions.save')}
        </button>
      </div>
    </form>
  )
}

function CustomerStatusControl({
  customer,
  onSaved,
}: {
  customer: AdminCustomer
  onSaved: () => Promise<void>
}) {
  const { t } = useTranslation()
  const initialStatus = customer.status === 'Suspended' ? 'Suspended' : 'Active'
  const [status, setStatus] = useState<'Active' | 'Suspended'>(initialStatus)
  const [generalError, setGeneralError] = useState('')
  useEffect(() => {
    setStatus(customer.status === 'Suspended' ? 'Suspended' : 'Active')
  }, [customer.status])
  const mutation = useMutation({
    mutationFn: (nextStatus: 'Active' | 'Suspended') =>
      api.post(`/admin/customers/${customer.id}/${nextStatus === 'Active' ? 'activate' : 'suspend'}`),
    onSuccess: async () => {
      setGeneralError('')
      toast.success(t('admin.editor.success.customerStatusUpdated'))
      await onSaved()
    },
    onError: (error) => {
      const problem = parseApiProblem(error)
      setGeneralError(problem.title)
    },
  })
  return (
    <form
      className="alert alert-warning grid gap-4 p-4"
      onSubmit={(event) => {
        event.preventDefault()
        mutation.mutate(status)
      }}
    >
      <h3 className="font-extrabold">{t('admin.editor.customer.accountStatus')}</h3>
      <GeneralFormError message={generalError} />
      <div className="grid min-w-0 gap-3 sm:grid-cols-[minmax(0,1fr)_auto] sm:items-end">
        <FormField label={t('admin.fields.status')} required>
          <select
            className="field w-full sm:min-w-52"
            disabled={mutation.isPending}
            onChange={(event) => setStatus(event.target.value as 'Active' | 'Suspended')}
            value={status}
          >
            <option value="Active">{t('admin.values.Active')}</option>
            <option value="Suspended">{t('admin.values.Suspended')}</option>
          </select>
        </FormField>
        <button
          className="button-primary w-full whitespace-normal sm:w-auto"
          disabled={mutation.isPending || status === customer.status}
          type="submit"
        >
          {mutation.isPending ? t('states.saving') : t('admin.editor.customer.applyStatus')}
        </button>
      </div>
      {status === 'Suspended' && (
        <p className="text-sm font-semibold text-[var(--state-warning-text)]">{t('admin.editor.customer.suspensionWarning')}</p>
      )}
    </form>
  )
}

function CustomerDetail({
  canStatusWrite,
  canWrite,
  customer,
  onChanged,
}: {
  canStatusWrite: boolean
  canWrite: boolean
  customer: AdminCustomer
  onChanged: () => Promise<void>
}) {
  const { t, i18n } = useTranslation()
  const [editing, setEditing] = useState(false)
  const locale = i18n.language.startsWith('ar') ? 'ar-PS-u-nu-latn' : 'en-US'
  const date = (value?: string | null) => formatDate(value, locale)
  const details = [
    ['phoneNumber', customer.phoneNumber ?? '—'],
    ['email', customer.email || '—'],
    ['status', t(`admin.values.${customer.status}`, { defaultValue: customer.status ?? '—' })],
    ['isPhoneVerified', t(customer.isPhoneVerified ? 'common.yes' : 'common.no')],
    ['preferredLanguage', t(`admin.values.${customer.preferredLanguage}`, { defaultValue: customer.preferredLanguage ?? '—' })],
    ['addressCount', new Intl.NumberFormat(locale).format(customer.addressCount ?? 0)],
    ['orderCount', new Intl.NumberFormat(locale).format(customer.orderCount ?? 0)],
    ['totalSpending', formatMoney(customer.totalSpending ?? 0, 'ILS', locale)],
    ['lastLoginAtUtc', date(customer.lastLoginAtUtc)],
    ['createdAtUtc', date(customer.createdAtUtc)],
  ] as const

  return (
    <div className="grid min-w-0 gap-5">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h3 className="break-words text-xl font-extrabold [overflow-wrap:anywhere]" dir="auto">{customer.fullName}</h3>
          <p className="muted mt-1 break-all text-start" dir="ltr">{customer.phoneNumber}</p>
        </div>
        {canWrite && !editing && (
          <button className="button-secondary" onClick={() => setEditing(true)} type="button">
            <Pencil aria-hidden size={17} />
            {t('admin.editor.edit')}
          </button>
        )}
      </div>
      <dl className="grid gap-0 rounded-xl border px-4 sm:grid-cols-2">
        {details.map(([key, value]) => (
          <div className="border-b py-3 sm:px-2" key={key}>
            <dt className="muted text-xs">{t(`admin.fields.${key}`)}</dt>
            <dd className="mt-1 break-words font-semibold [overflow-wrap:anywhere]" dir={key === 'phoneNumber' || key === 'email' || key.endsWith('Utc') ? 'ltr' : 'auto'}>{value}</dd>
          </div>
        ))}
      </dl>
      {customer.administrativeNotes && (
        <div className="rounded-xl bg-stone-50 p-4">
          <h3 className="text-sm font-extrabold">{t('admin.fields.administrativeNotes')}</h3>
          <p className="mt-2 whitespace-pre-wrap break-words text-sm [overflow-wrap:anywhere]" dir="auto">{customer.administrativeNotes}</p>
        </div>
      )}
      {editing && (
        <CustomerEditForm
          customer={customer}
          onCancel={() => setEditing(false)}
          onSaved={async () => {
            setEditing(false)
            await onChanged()
          }}
        />
      )}
      {canStatusWrite && <CustomerStatusControl customer={customer} onSaved={onChanged} />}
    </div>
  )
}

export function AdminCustomerDialog({
  canStatusWrite,
  canWrite,
  customerId,
  onClose,
}: {
  canStatusWrite: boolean
  canWrite: boolean
  customerId: string
  onClose: () => void
}) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const detailQuery = useQuery({
    queryKey: ['admin-customer', customerId],
    queryFn: () => api
      .get<AdminCustomer>(`/admin/customers/${customerId}`)
      .then((response) => response.data),
  })
  const onChanged = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: ['admin-customer', customerId] }),
      queryClient.invalidateQueries({ queryKey: ['admin-resource', 'customers'] }),
    ])
  }

  return (
    <EditorDialogFrame heading={t('admin.editor.customer.detailsTitle')} onClose={onClose}>
      {detailQuery.isLoading
        ? <LoadingState cards={2} />
        : detailQuery.isError
          ? <ErrorState error={detailQuery.error} retry={() => void detailQuery.refetch()} />
          : detailQuery.data
            ? (
              <CustomerDetail
                canStatusWrite={canStatusWrite}
                canWrite={canWrite}
                customer={detailQuery.data}
                onChanged={onChanged}
              />
            )
            : <ErrorState status={404} />}
    </EditorDialogFrame>
  )
}

const contactStatusValues: ContactMessageStatus[] = ['New', 'UnderReview', 'Replied', 'Closed', 'Spam']

function ContactMessageStatusControl({
  message,
  onSaved,
}: {
  message: AdminContactMessage
  onSaved: () => Promise<void>
}) {
  const { t } = useTranslation()
  const [status, setStatus] = useState<ContactMessageStatus>(message.status ?? 'New')
  const [generalError, setGeneralError] = useState('')
  useEffect(() => {
    setStatus(message.status ?? 'New')
  }, [message.status])
  const mutation = useMutation({
    mutationFn: (nextStatus: ContactMessageStatus) => api.post(`/admin/contact-messages/${message.id}/status`, { status: nextStatus }),
    onSuccess: async () => {
      setGeneralError('')
      toast.success(t('admin.editor.success.contactStatusUpdated'))
      await onSaved()
    },
    onError: (error) => {
      setGeneralError(parseApiProblem(error).title)
    },
  })
  return (
    <form
      className="alert alert-warning grid gap-4 p-4"
      onSubmit={(event) => {
        event.preventDefault()
        mutation.mutate(status)
      }}
    >
      <div>
        <h3 className="font-extrabold">{t('contact.updateStatus')}</h3>
      </div>
      <GeneralFormError message={generalError} />
      <div className="grid min-w-0 gap-3 sm:grid-cols-[minmax(0,1fr)_auto] sm:items-end">
        <FormField label={t('admin.fields.status')} required>
          <select
            className="field w-full sm:min-w-52"
            disabled={mutation.isPending}
            onChange={(event) => setStatus(event.target.value as ContactMessageStatus)}
            value={status}
          >
            {contactStatusValues.map((value) => <option key={value} value={value}>{t(`admin.values.${value}`)}</option>)}
          </select>
        </FormField>
        <button className="button-secondary" disabled={mutation.isPending || status === message.status} type="submit">
          {mutation.isPending ? t('states.saving') : t('actions.save')}
        </button>
      </div>
    </form>
  )
}

function ContactMessageDetail({
  canWrite,
  message,
  onChanged,
}: {
  canWrite: boolean
  message: AdminContactMessage
  onChanged: () => Promise<void>
}) {
  const { t, i18n } = useTranslation()
  const locale = i18n.language.startsWith('ar') ? 'ar-PS-u-nu-latn' : 'en-US'
  const date = (value?: string | null) => formatDate(value, locale)
  const [reply, setReply] = useState(message.administrativeReply ?? '')
  const [generalError, setGeneralError] = useState('')
  useEffect(() => {
    setReply(message.administrativeReply ?? '')
  }, [message.id, message.administrativeReply])
  const replyMutation = useMutation({
    mutationFn: (text: string) => api.post<AdminContactMessage>(`/admin/contact-messages/${message.id}/reply`, { reply: text }).then((response) => response.data),
    onSuccess: async () => {
      setGeneralError('')
      toast.success(t('admin.editor.success.contactReplied'))
      await onChanged()
    },
    onError: (error) => {
      setGeneralError(parseApiProblem(error).title)
    },
  })
  const details = [
    ['name', message.name ?? '—'],
    ['phoneNumber', message.phoneNumber ?? '—'],
    ['email', message.email || '—'],
    ['createdAtUtc', date(message.createdAtUtc)],
  ] as const

  return (
    <div className="grid min-w-0 gap-5">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <h3 className="min-w-0 break-words text-xl font-extrabold [overflow-wrap:anywhere]" dir="auto">{message.subject}</h3>
        <span className={statusBadgeClass(message.status) + ' shrink-0'}>{t(`admin.values.${message.status}`, { defaultValue: message.status ?? '—' })}</span>
      </div>
      <dl className="grid gap-0 rounded-xl border px-4 sm:grid-cols-2">
        {details.map(([key, value]) => (
          <div className="border-b py-3 sm:px-2" key={key}>
            <dt className="muted text-xs">{t(`admin.fields.${key}`)}</dt>
            <dd className="mt-1 break-words font-semibold [overflow-wrap:anywhere]" dir={key === 'phoneNumber' || key === 'email' || key.endsWith('Utc') ? 'ltr' : 'auto'}>{value}</dd>
          </div>
        ))}
      </dl>
      <div className="rounded-xl bg-stone-50 p-4">
        <h3 className="text-sm font-extrabold">{t('contact.message')}</h3>
        <p className="mt-2 whitespace-pre-wrap break-words text-sm [overflow-wrap:anywhere]" dir="auto">{message.message}</p>
      </div>
      {message.administrativeReply && (
        <div className="rounded-xl border-s-4 border-s-[var(--heritage-gold)] bg-[var(--heritage-beige)]/40 p-4">
          <h3 className="text-sm font-extrabold">{t('contact.reply')}</h3>
          <p className="mt-2 whitespace-pre-wrap break-words text-sm [overflow-wrap:anywhere]" dir="auto">{message.administrativeReply}</p>
          <p className="muted mt-2 text-xs">{date(message.repliedAtUtc)}</p>
        </div>
      )}
      {canWrite && (
        <form
          className="card grid min-w-0 gap-4 border-s-4 border-s-[var(--heritage-burgundy)] p-4"
          onSubmit={(event) => {
            event.preventDefault()
            if (!reply.trim()) {
              setGeneralError(t('contact.replyRequired'))
              return
            }
            replyMutation.mutate(reply.trim())
          }}
        >
          <div>
            <h3 className="font-extrabold">{message.administrativeReply ? t('contact.updateReply') : t('contact.writeReply')}</h3>
          </div>
          <GeneralFormError message={generalError} />
          <label className="grid min-w-0 gap-2 font-bold">
            <span>{t('contact.replyMessage')}</span>
            <textarea
              className="field min-h-32"
              disabled={replyMutation.isPending}
              maxLength={10000}
              onChange={(event) => setReply(event.target.value)}
              value={reply}
            />
          </label>
          <div>
            <button className="button-primary" disabled={replyMutation.isPending} type="submit">
              {replyMutation.isPending ? t('states.saving') : t('contact.sendReply')}
            </button>
          </div>
        </form>
      )}
      {canWrite && <ContactMessageStatusControl message={message} onSaved={onChanged} />}
    </div>
  )
}

export function AdminContactMessageDialog({
  canWrite,
  messageId,
  onClose,
}: {
  canWrite: boolean
  messageId: string
  onClose: () => void
}) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const detailQuery = useQuery({
    queryKey: ['admin-contact-message', messageId],
    queryFn: () => api
      .get<AdminContactMessage>(`/admin/contact-messages/${messageId}`)
      .then((response) => response.data),
  })
  const onChanged = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: ['admin-contact-message', messageId] }),
      queryClient.invalidateQueries({ queryKey: ['admin-resource', 'contact'] }),
    ])
  }

  return (
    <EditorDialogFrame heading={t('admin.editor.contact.detailsTitle')} onClose={onClose}>
      {detailQuery.isLoading
        ? <LoadingState cards={2} />
        : detailQuery.isError
          ? <ErrorState error={detailQuery.error} retry={() => void detailQuery.refetch()} />
          : detailQuery.data
            ? (
              <ContactMessageDetail
                canWrite={canWrite}
                message={detailQuery.data}
                onChanged={onChanged}
              />
            )
            : <ErrorState status={404} />}
    </EditorDialogFrame>
  )
}

function FormActions({
  onClose,
  pending,
  saveLabel,
}: {
  onClose: () => void
  pending: boolean
  saveLabel?: string
}) {
  const { t } = useTranslation()
  return (
    // Pulled out over the scroll area's padding (negative inset and margins) so the bar sits flush
    // with the dialog's bottom edge instead of floating above it with form fields visible beneath.
    <div className="sticky -bottom-4 z-10 -mx-4 -mb-4 flex flex-col-reverse gap-3 border-t border-[var(--heritage-line)] bg-white px-4 py-4 sm:-bottom-5 sm:-mx-5 sm:-mb-5 sm:flex-row sm:flex-wrap sm:justify-end sm:px-5">
      <button className="button-secondary w-full whitespace-normal sm:w-auto" disabled={pending} onClick={onClose} type="button">
        {t('admin.editor.cancel')}
      </button>
      <button className="button-primary w-full whitespace-normal sm:w-auto" disabled={pending} type="submit">
        {pending ? t('states.saving') : saveLabel ?? t('actions.save')}
      </button>
    </div>
  )
}

const actionHeading = (
  resource: WritableAdminResource,
  action: ResourceWriteAction,
  t: ReturnType<typeof useTranslation>['t'],
) => t(`admin.editor.headings.${action}.${resource}`)

export function AdminResourceEditor({
  action,
  onClose,
  resource,
  row,
}: {
  action: ResourceWriteAction
  onClose: () => void
  resource: WritableAdminResource
  row?: Row
}) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const onSaved = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: ['admin-resource', resource] }),
      queryClient.invalidateQueries({ queryKey: ['admin-editor-options'] }),
    ])
    toast.success(t(`admin.editor.success.${action}`))
    onClose()
  }
  const id = String(row?.id ?? row?.productVariantId ?? '')

  return (
    <EditorDialogFrame heading={actionHeading(resource, action, t)} onClose={onClose}>
      {action === 'archive'
        ? <ArchiveForm endpoint={`/admin/${resource}/${id}`} onClose={onClose} onSaved={() => void onSaved()} />
        : resource === 'products'
          ? <ProductForm mode={action as 'create' | 'edit'} onClose={onClose} onSaved={() => void onSaved()} row={row} />
          : resource === 'categories'
            ? <CategoryForm mode={action as 'create' | 'edit'} onClose={onClose} onSaved={() => void onSaved()} row={row} />
            : resource === 'banners'
              ? <BannerForm mode={action as 'create' | 'edit'} onClose={onClose} onSaved={() => void onSaved()} row={row} />
              : resource === 'coupons'
                ? <CouponForm mode={action as 'create' | 'edit'} onClose={onClose} onSaved={() => void onSaved()} row={row} />
                : row
                  ? <InventoryAdjustmentForm onClose={onClose} onSaved={() => void onSaved()} row={row} />
                  : null}
    </EditorDialogFrame>
  )
}
