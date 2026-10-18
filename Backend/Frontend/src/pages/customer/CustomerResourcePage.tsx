import { useMemo, useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import type { TFunction } from 'i18next'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { api } from '@/lib/api/client'
import { EmptyState, ErrorState, LoadingState } from '@/components/ui/Feedback'
import { formatDate, formatMoney } from '@/lib/utils'
import type { Schema } from '@/types/models'

type Row = Record<string, unknown>
type ResourceKey = keyof typeof resources
type PagedRows = { items?: Row[]; totalPages?: number }

const resources = {
  profile: { titleKey: 'customer.resources.profile', endpoint: '/customer/profile', paged: false },
  addresses: { titleKey: 'customer.resources.addresses', endpoint: '/customer/addresses', paged: false },
  orders: { titleKey: 'customer.resources.orders', endpoint: '/customer/orders', paged: true },
  messages: { titleKey: 'customer.resources.messages', endpoint: '/customer/messages', paged: true },
  notifications: { titleKey: 'customer.resources.notifications', endpoint: '/customer/notifications', paged: true },
  payments: { titleKey: 'customer.resources.payments', endpoint: '', paged: true },
} as const

const humanize = (value: string) =>
  value
    .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
    .replace(/[-_]/g, ' ')
    .replace(/^./, (letter) => letter.toUpperCase())

const isTechnicalField = (key: string) =>
  /(^id$|id$|email|phone|mobile|url|uri|slug|code|sku|number|reference|currency|amount|price|total|discount|delivery|quantity|count|postal|zip|tax)/i.test(key)

const displayValue = (
  key: string,
  value: unknown,
  row: Row,
  t: TFunction,
  locale: string,
) => {
  if (value == null || value === '') return '—'
  if (typeof value === 'boolean') return t(value ? 'common.yes' : 'common.no')
  if (typeof value === 'string' && /^\d{4}-\d{2}-\d{2}T/.test(value)) return formatDate(value, locale)
  if (typeof value === 'string') return t(`admin.values.${value}`, { defaultValue: value })
  if (typeof value === 'number' && /(?:price|fee|cost|amount|subtotal|discount|delivery|total|sales|spending|revenue)$/i.test(key)) {
    return formatMoney(value, typeof row.currency === 'string' ? row.currency : 'ILS', locale)
  }
  if (typeof value === 'number') return new Intl.NumberFormat(locale).format(value)
  return String(value)
}

const loadPayments = async (page: number) => {
  const orders = await api
    .get<Schema<'OrderSummaryResponsePagedResult'>>('/customer/orders', {
      params: { page, pageSize: 10 },
    })
    .then((response) => response.data)
  const orderRows = orders.items ?? []
  const paymentGroups = await Promise.all(
    orderRows
      .filter((order) => Boolean(order.id))
      .map(async (order) => {
        const payments = await api
          .get<Schema<'PaymentResponse'>[]>(`/customer/orders/${order.id}/payments`)
          .then((response) => response.data)
        return payments.map((payment) => ({
          ...payment,
          orderNumber: order.orderNumber,
        }))
      }),
  )
  return {
    items: paymentGroups.flat() as Row[],
    totalPages: orders.totalPages ?? 1,
  } satisfies PagedRows
}

export function CustomerResourcePage({ resource }: { resource: ResourceKey }) {
  const { t, i18n } = useTranslation()
  const config = resources[resource]
  const [page, setPage] = useState(1)
  const query = useQuery({
    queryKey: ['customer', resource, page, i18n.language],
    queryFn: async () => {
      if (resource === 'payments') return loadPayments(page)
      return api
        .get(config.endpoint, { params: config.paged ? { page, pageSize: 10 } : undefined })
        .then((response) => response.data as unknown)
    },
  })
  const raw = query.data as PagedRows | Row[] | Row | undefined
  const rows = useMemo(
    () => Array.isArray(raw)
      ? raw
      : raw && 'items' in raw && Array.isArray(raw.items)
        ? raw.items
        : raw
          ? [raw]
          : [],
    [raw],
  )
  const totalPages = !Array.isArray(raw) && raw && 'totalPages' in raw
    ? Math.max(1, Number(raw.totalPages ?? 1))
    : 1
  const locale = i18n.language.startsWith('ar') ? 'ar-PS-u-nu-latn' : 'en-US'

  return (
    <div className="min-w-0">
      <h1 className="mb-6 [overflow-wrap:anywhere] text-3xl font-extrabold">{t(config.titleKey)}</h1>
      {query.isLoading
        ? <LoadingState />
        : query.isError
          ? <ErrorState error={query.error} retry={() => void query.refetch()} />
          : !rows.length
            ? <EmptyState message={t(`customer.empty.${resource}`)} />
            : (
              <div className="grid min-w-0 gap-4">
                {rows.map((row, index) => (
                  <article className="card min-w-0 p-5" key={String(row.id ?? index)}>
                    <dl className="grid min-w-0 gap-0">
                      {Object.entries(row)
                        .filter(([key, value]) =>
                          value != null
                          && typeof value !== 'object'
                          && key.toLowerCase() !== 'id'
                          && !key.toLowerCase().endsWith('id')
                          && !key.toLowerCase().includes('token')
                          && key !== 'retryCount'
                          && key.toLowerCase() !== 'currency')
                        .slice(0, 12)
                        .map(([key, value]) => (
                          <div className="grid min-w-0 gap-1 border-b py-2 last:border-0 sm:grid-cols-[minmax(8rem,10rem)_minmax(0,1fr)] sm:gap-3" key={key}>
                            <dt className="muted [overflow-wrap:anywhere] text-sm">
                              {t(`customer.fields.${key}`, { defaultValue: humanize(key) })}
                            </dt>
                            <dd className="min-w-0 whitespace-pre-wrap [overflow-wrap:anywhere] text-start text-sm font-semibold" dir={isTechnicalField(key) ? 'ltr' : 'auto'}>
                              {displayValue(key, value, row, t, locale)}
                            </dd>
                          </div>
                        ))}
                    </dl>
                    {resource === 'orders' && typeof row.id === 'string' && (
                      <Link
                        className="button-secondary mt-4 max-w-full w-full whitespace-normal text-center sm:w-auto"
                        to={`/account/orders/${row.id}`}
                      >
                        {t('orderDetails.viewDetails')}
                      </Link>
                    )}
                  </article>
                ))}
              </div>
            )}
      {totalPages > 1 && (
        <nav aria-label={t('admin.table.pagination')} className="mt-5 flex flex-wrap items-center justify-center gap-2 sm:justify-end">
          <button className="button-secondary max-w-full whitespace-normal text-center" disabled={page <= 1} onClick={() => setPage((value) => value - 1)} type="button">
            {t('actions.previous')}
          </button>
          <span className="badge max-w-full whitespace-normal text-center">{t('admin.table.pageOf', { page, total: totalPages })}</span>
          <button className="button-secondary max-w-full whitespace-normal text-center" disabled={page >= totalPages} onClick={() => setPage((value) => value + 1)} type="button">
            {t('actions.next')}
          </button>
        </nav>
      )}
    </div>
  )
}
