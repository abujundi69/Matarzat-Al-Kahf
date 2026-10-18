import { useDeferredValue, useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Download, Eye, Search } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { toast } from 'sonner'
import { EmptyState, ErrorState, ForbiddenState, LoadingState } from '@/components/ui/Feedback'
import { adminPermissions } from '@/config/adminResources'
import { api, downloadFile } from '@/lib/api/client'
import { parseApiProblem } from '@/lib/api/problemDetails'
import { formatDate, formatMoney } from '@/lib/utils'
import { useSessionStore } from '@/stores/sessionStore'
import type { Schema } from '@/types/models'
import { statusBadgeClass } from '@/lib/statusTone'

type OrderStatus = NonNullable<Schema<'OrderSummaryResponse'>['status']>
type PaymentMethod = NonNullable<Schema<'OrderSummaryResponse'>['paymentMethod']>
type PaymentStatus = NonNullable<Schema<'OrderSummaryResponse'>['paymentStatus']>

const orderStatuses: readonly OrderStatus[] = [
  'New', 'UnderReview', 'Confirmed', 'Preparing', 'ReadyForShipping',
  'Shipped', 'Delivered', 'Cancelled', 'Returned', 'PaymentFailed',
]
const paymentMethods: readonly PaymentMethod[] = [
  'CashOnDelivery', 'BankTransfer', 'ElectronicWallet', 'CreditCard',
]
const paymentStatuses: readonly PaymentStatus[] = [
  'Pending', 'Processing', 'Paid', 'Failed', 'Cancelled', 'Refunded', 'PartiallyRefunded',
]

export function AdminOrdersPage() {
  const { t, i18n } = useTranslation()
  const permissions = useSessionStore((state) => state.admin?.permissions ?? [])
  const [search, setSearch] = useState('')
  const deferredSearch = useDeferredValue(search.trim())
  const [status, setStatus] = useState<OrderStatus | ''>('')
  const [paymentMethod, setPaymentMethod] = useState<PaymentMethod | ''>('')
  const [paymentStatus, setPaymentStatus] = useState<PaymentStatus | ''>('')
  const [page, setPage] = useState(1)
  const [exporting, setExporting] = useState(false)
  const canRead = permissions.includes(adminPermissions.ordersRead)
  const canExport = permissions.includes(adminPermissions.ordersExport)
  const locale = i18n.language.startsWith('ar') ? 'ar-PS-u-nu-latn' : 'en-US'
  const query = useQuery({
    enabled: canRead,
    queryKey: ['admin-resource', 'orders', page, deferredSearch, status, paymentMethod, paymentStatus, i18n.language],
    queryFn: () => api
      .get<Schema<'OrderSummaryResponsePagedResult'>>('/admin/orders', {
        params: {
          page,
          pageSize: 20,
          search: deferredSearch || undefined,
          status: status || undefined,
          paymentMethod: paymentMethod || undefined,
          paymentStatus: paymentStatus || undefined,
        },
      })
      .then((response) => response.data),
  })
  const orders = query.data?.items ?? []
  const totalPages = Math.max(1, query.data?.totalPages ?? 1)

  const changeFilter = (setter: (value: never) => void, value: string) => {
    setter(value as never)
    setPage(1)
  }
  const exportOrders = async () => {
    if (exporting) return
    setExporting(true)
    try {
      const toUtc = new Date()
      const fromUtc = new Date(toUtc)
      fromUtc.setUTCFullYear(fromUtc.getUTCFullYear() - 1)
      const params = new URLSearchParams({
        fromUtc: fromUtc.toISOString(),
        toUtc: toUtc.toISOString(),
        language: i18n.language.startsWith('ar') ? 'Arabic' : 'English',
      })
      await downloadFile(`/admin/exports/orders?${params.toString()}`, 'orders.xlsx')
      toast.success(t('admin.export.success'))
    } catch (error) {
      toast.error(t('admin.export.failed'), { description: parseApiProblem(error).title })
    } finally {
      setExporting(false)
    }
  }

  if (!canRead) return <ForbiddenState />

  return <section className="min-w-0">
    <div className="mb-6 flex min-w-0 flex-wrap items-center justify-between gap-4">
      <h1 className="min-w-0 [overflow-wrap:anywhere] text-3xl font-extrabold">{t('admin.resources.orders')}</h1>
      {canExport && <button className="button-secondary w-full sm:w-auto" disabled={exporting} onClick={() => void exportOrders()} type="button">
        <Download aria-hidden size={17} />
        {exporting ? t('admin.export.inProgress') : t('admin.export.action')}
      </button>}
    </div>

    <div className="card mb-5 grid min-w-0 gap-3 p-3 sm:grid-cols-2 xl:grid-cols-[minmax(14rem,1fr)_repeat(3,minmax(10rem,auto))]">
      <label className="flex min-w-0 items-center gap-3 rounded-xl border bg-white px-3">
        <Search aria-hidden className="muted shrink-0" size={19} />
        <span className="sr-only">{t('admin.orders.searchLabel')}</span>
        <input
          className="min-h-11 min-w-0 flex-1 border-0 bg-transparent outline-none"
          placeholder={t('admin.orders.searchPlaceholder')}
          value={search}
          onChange={(event) => {
            setSearch(event.target.value)
            setPage(1)
          }}
        />
      </label>
      <label className="min-w-0">
        <span className="sr-only">{t('admin.orders.statusFilter')}</span>
        <select className="field" value={status} onChange={(event) => changeFilter(setStatus, event.target.value)}>
          <option value="">{t('admin.orders.allOrderStatuses')}</option>
          {orderStatuses.map((value) => <option key={value} value={value}>{t(`orderStatuses.${value}`)}</option>)}
        </select>
      </label>
      <label className="min-w-0">
        <span className="sr-only">{t('admin.orders.paymentMethodFilter')}</span>
        <select className="field" value={paymentMethod} onChange={(event) => changeFilter(setPaymentMethod, event.target.value)}>
          <option value="">{t('admin.orders.allPaymentMethods')}</option>
          {paymentMethods.map((value) => <option key={value} value={value}>{t(`paymentMethods.${value}`)}</option>)}
        </select>
      </label>
      <label className="min-w-0">
        <span className="sr-only">{t('admin.orders.paymentStatusFilter')}</span>
        <select className="field" value={paymentStatus} onChange={(event) => changeFilter(setPaymentStatus, event.target.value)}>
          <option value="">{t('admin.orders.allPaymentStatuses')}</option>
          {paymentStatuses.map((value) => <option key={value} value={value}>{t(`paymentStatuses.${value}`)}</option>)}
        </select>
      </label>
    </div>

    {query.isLoading
      ? <LoadingState />
      : query.isError
        ? <ErrorState error={query.error} retry={() => void query.refetch()} />
        : !orders.length
          ? <EmptyState message={t('admin.orders.empty')} />
          : <div className="min-w-0">
            <div className="grid gap-4 md:hidden">
              {orders.map((order) => <article className="card min-w-0 p-4" key={order.id}>
                <div className="flex min-w-0 flex-wrap items-start justify-between gap-2">
                  <div className="min-w-0">
                    <span className="muted text-xs">{t('admin.fields.orderNumber')}</span>
                    <h2 className="mt-1 [overflow-wrap:anywhere] text-lg font-extrabold" dir="ltr">{order.orderNumber ?? '—'}</h2>
                  </div>
                  <span className={statusBadgeClass(order.status)}>{t(`orderStatuses.${order.status}`, { defaultValue: order.status ?? '—' })}</span>
                </div>
                <dl className="mt-4 grid min-w-0 gap-3 border-y py-4 sm:grid-cols-2">
                  <div><dt className="muted text-xs">{t('admin.fields.customerName')}</dt><dd className="mt-1 [overflow-wrap:anywhere] font-bold" dir="auto">{order.customerName ?? '—'}</dd><dd className="muted text-xs" dir="ltr">{order.phoneNumber ?? '—'}</dd></div>
                  <div><dt className="muted text-xs">{t('admin.fields.createdAtUtc')}</dt><dd className="mt-1 font-bold">{formatDate(order.createdAtUtc, locale)}</dd></div>
                  <div><dt className="muted text-xs">{t('admin.fields.paymentMethod')}</dt><dd className="mt-1 font-bold">{t(`paymentMethods.${order.paymentMethod}`, { defaultValue: order.paymentMethod ?? '—' })}</dd></div>
                  <div><dt className="muted text-xs">{t('admin.fields.paymentStatus')}</dt><dd className="mt-1"><span className={statusBadgeClass(order.paymentStatus)}>{t(`paymentStatuses.${order.paymentStatus}`, { defaultValue: order.paymentStatus ?? '—' })}</span></dd></div>
                  <div><dt className="muted text-xs">{t('admin.fields.total')}</dt><dd className="mt-1 text-lg font-extrabold text-[var(--heritage-burgundy)]" dir="ltr">{formatMoney(order.total ?? 0, order.currency ?? 'ILS', locale)}</dd></div>
                </dl>
                {order.id && <Link className="button-secondary mt-4 w-full no-underline" to={`/admin/orders/${order.id}`}><Eye aria-hidden size={17} />{t('actions.view')}</Link>}
              </article>)}
            </div>
            <div className="card hidden min-w-0 overflow-x-auto md:block">
              <table className="w-full min-w-[980px] text-sm">
                <thead className="bg-[var(--heritage-beige)]">
                  <tr>
                    <th className="px-4 py-3">{t('admin.fields.orderNumber')}</th>
                    <th className="px-4 py-3">{t('admin.fields.customerName')}</th>
                    <th className="px-4 py-3">{t('admin.fields.createdAtUtc')}</th>
                    <th className="px-4 py-3">{t('admin.fields.total')}</th>
                    <th className="px-4 py-3">{t('admin.fields.paymentMethod')}</th>
                    <th className="px-4 py-3">{t('admin.fields.paymentStatus')}</th>
                    <th className="px-4 py-3">{t('admin.fields.status')}</th>
                    <th className="px-4 py-3">{t('admin.table.actions')}</th>
                  </tr>
                </thead>
                <tbody>
                  {orders.map((order) => <tr className="border-t align-top hover:bg-stone-50" key={order.id}>
                    <td className="px-4 py-4 font-extrabold" dir="ltr">{order.orderNumber ?? '—'}</td>
                    <td className="max-w-56 px-4 py-4"><span className="block [overflow-wrap:anywhere] font-bold" dir="auto">{order.customerName ?? '—'}</span><span className="muted block text-xs" dir="ltr">{order.phoneNumber ?? '—'}</span></td>
                    <td className="px-4 py-4">{formatDate(order.createdAtUtc, locale)}</td>
                    <td className="px-4 py-4 font-extrabold" dir="ltr">{formatMoney(order.total ?? 0, order.currency ?? 'ILS', locale)}</td>
                    <td className="px-4 py-4">{t(`paymentMethods.${order.paymentMethod}`, { defaultValue: order.paymentMethod ?? '—' })}</td>
                    <td className="px-4 py-4"><span className={statusBadgeClass(order.paymentStatus)}>{t(`paymentStatuses.${order.paymentStatus}`, { defaultValue: order.paymentStatus ?? '—' })}</span></td>
                    <td className="px-4 py-4"><span className={statusBadgeClass(order.status)}>{t(`orderStatuses.${order.status}`, { defaultValue: order.status ?? '—' })}</span></td>
                    <td className="px-4 py-4">{order.id && <Link className="inline-flex items-center gap-2 font-bold text-[var(--heritage-burgundy)]" to={`/admin/orders/${order.id}`}><Eye aria-hidden size={17} />{t('actions.view')}</Link>}</td>
                  </tr>)}
                </tbody>
              </table>
            </div>
          </div>}

    {totalPages > 1 && <nav aria-label={t('pagination.label')} className="mt-5 flex flex-wrap items-center justify-center gap-2 sm:justify-end">
      <button className="button-secondary min-w-0 flex-1 sm:flex-none" disabled={page <= 1 || query.isFetching} onClick={() => setPage((value) => value - 1)} type="button">{t('actions.previous')}</button>
      <span className="badge order-first flex w-full justify-center sm:order-none sm:w-auto">{t('pagination.pageOf', { page, total: totalPages })}</span>
      <button className="button-secondary min-w-0 flex-1 sm:flex-none" disabled={page >= totalPages || query.isFetching} onClick={() => setPage((value) => value + 1)} type="button">{t('actions.next')}</button>
    </nav>}
  </section>
}
