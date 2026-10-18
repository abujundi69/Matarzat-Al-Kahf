import { useQuery } from '@tanstack/react-query'
import { CalendarDays, CreditCard, PackageCheck, ReceiptText } from 'lucide-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { EmptyState, ErrorState, LoadingState } from '@/components/ui/Feedback'
import { api } from '@/lib/api/client'
import { formatDate, formatMoney } from '@/lib/utils'
import type { Schema } from '@/types/models'
import { statusBadgeClass } from '@/lib/statusTone'

export function CustomerOrdersPage() {
  const { t, i18n } = useTranslation()
  const [page, setPage] = useState(1)
  const locale = i18n.language.startsWith('ar') ? 'ar-PS-u-nu-latn' : 'en-US'
  const query = useQuery({
    queryKey: ['customer', 'orders', page, i18n.language],
    queryFn: () => api
      .get<Schema<'OrderSummaryResponsePagedResult'>>('/customer/orders', {
        params: { page, pageSize: 10 },
      })
      .then((response) => response.data),
  })
  const orders = query.data?.items ?? []
  const totalPages = Math.max(1, query.data?.totalPages ?? 1)

  return <section className="min-w-0">
    <h1 className="mb-6 [overflow-wrap:anywhere] text-3xl font-extrabold">{t('customer.resources.orders')}</h1>

    {query.isLoading
      ? <LoadingState />
      : query.isError
        ? <ErrorState error={query.error} retry={() => void query.refetch()} />
        : !orders.length
          ? <EmptyState message={t('customer.empty.orders')} />
          : <div className="grid min-w-0 gap-4">
            {orders.map((order) => <article className="card min-w-0 overflow-hidden" key={order.id}>
              <div className="flex min-w-0 flex-wrap items-start justify-between gap-3 border-b bg-[var(--heritage-beige)]/45 p-4 sm:p-5">
                <div className="min-w-0">
                  <span className="muted text-xs">{t('orders.orderNumber')}</span>
                  <h2 className="mt-1 [overflow-wrap:anywhere] text-xl font-extrabold" dir="ltr">{order.orderNumber ?? '—'}</h2>
                </div>
                <div className="flex min-w-0 flex-wrap gap-2">
                  <span className={statusBadgeClass(order.status)}>{t(`orderStatuses.${order.status}`, { defaultValue: order.status ?? '—' })}</span>
                  <span className={statusBadgeClass(order.paymentStatus)}>{t(`paymentStatuses.${order.paymentStatus}`, { defaultValue: order.paymentStatus ?? '—' })}</span>
                </div>
              </div>
              <dl className="grid min-w-0 gap-4 p-4 sm:grid-cols-3 sm:p-5">
                <div className="flex min-w-0 items-start gap-3">
                  <CalendarDays aria-hidden className="mt-0.5 text-[var(--heritage-gold)]" size={19} />
                  <div className="min-w-0">
                    <dt className="muted text-xs">{t('orders.date')}</dt>
                    <dd className="mt-1 [overflow-wrap:anywhere] font-bold">{formatDate(order.createdAtUtc, locale)}</dd>
                  </div>
                </div>
                <div className="flex min-w-0 items-start gap-3">
                  <CreditCard aria-hidden className="mt-0.5 text-[var(--heritage-gold)]" size={19} />
                  <div className="min-w-0">
                    <dt className="muted text-xs">{t('orders.paymentMethod')}</dt>
                    <dd className="mt-1 [overflow-wrap:anywhere] font-bold">{t(`paymentMethods.${order.paymentMethod}`, { defaultValue: order.paymentMethod ?? '—' })}</dd>
                  </div>
                </div>
                <div className="flex min-w-0 items-start gap-3">
                  <PackageCheck aria-hidden className="mt-0.5 text-[var(--heritage-gold)]" size={19} />
                  <div className="min-w-0">
                    <dt className="muted text-xs">{t('cart.total')}</dt>
                    <dd className="mt-1 [overflow-wrap:anywhere] text-lg font-extrabold text-[var(--heritage-burgundy)]" dir="ltr">
                      {formatMoney(order.total ?? 0, order.currency ?? 'ILS', locale)}
                    </dd>
                  </div>
                </div>
              </dl>
              {order.id && <div className="border-t p-4 text-end sm:px-5">
                <Link className="button-secondary w-full no-underline sm:w-auto" to={`/account/orders/${order.id}`}>
                  <ReceiptText aria-hidden size={18} />
                  {t('orderDetails.viewDetails')}
                </Link>
              </div>}
            </article>)}
          </div>}

    {totalPages > 1 && <nav aria-label={t('pagination.label')} className="mt-5 flex flex-wrap items-center justify-center gap-2 sm:justify-end">
      <button className="button-secondary min-w-0 flex-1 sm:flex-none" disabled={page <= 1 || query.isFetching} onClick={() => setPage((value) => value - 1)} type="button">{t('actions.previous')}</button>
      <span className="badge order-first flex w-full justify-center sm:order-none sm:w-auto">{t('pagination.pageOf', { page, total: totalPages })}</span>
      <button className="button-secondary min-w-0 flex-1 sm:flex-none" disabled={page >= totalPages || query.isFetching} onClick={() => setPage((value) => value + 1)} type="button">{t('actions.next')}</button>
    </nav>}
  </section>
}
