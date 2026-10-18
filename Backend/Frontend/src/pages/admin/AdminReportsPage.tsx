import { useState, type KeyboardEvent, type ReactNode } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import { useTranslation } from 'react-i18next'
import { api } from '@/lib/api/client'
import { formatDate, formatDateCompact, formatMoney } from '@/lib/utils'
import { EmptyState, ErrorState, ForbiddenState, LoadingState } from '@/components/ui/Feedback'
import type { Schema } from '@/types/models'
import { adminPermissions } from '@/config/adminResources'
import { useSessionStore } from '@/stores/sessionStore'

type ReportKey = 'sales' | 'products' | 'customers' | 'orders' | 'inventory'
type ReportRange = { from: string; to: string }
type PagedReportProps = ReportRange & { page: number; setPage: (page: number) => void }
type NamedMetric = Schema<'NamedMetricResponse'>

const pageSize = 20
const maximumRangeMilliseconds = 731 * 24 * 60 * 60 * 1000
const reportDefinitions = [
  { key: 'sales', permission: adminPermissions.reportsSalesRead },
  { key: 'products', permission: adminPermissions.reportsProductsRead },
  { key: 'customers', permission: adminPermissions.reportsCustomersRead },
  { key: 'orders', permission: adminPermissions.reportsOrdersRead },
  { key: 'inventory', permission: adminPermissions.reportsInventoryRead },
] as const

const startUtc = (date: string) => `${date}T00:00:00Z`

const exclusiveEndUtc = (date: string) => {
  const end = new Date(`${date}T00:00:00Z`)
  end.setUTCDate(end.getUTCDate() + 1)
  return end.toISOString()
}

const isValidRange = ({ from, to }: ReportRange) => {
  if (!/^\d{4}-\d{2}-\d{2}$/.test(from) || !/^\d{4}-\d{2}-\d{2}$/.test(to)) return false
  const fromTime = Date.parse(startUtc(from))
  const toTime = Date.parse(`${to}T00:00:00Z`) + 24 * 60 * 60 * 1000
  const duration = toTime - fromTime
  return Number.isFinite(duration) && duration > 0 && duration <= maximumRangeMilliseconds
}

function useReportQuery<T>(
  report: ReportKey,
  range: ReportRange,
  page?: number,
) {
  const { i18n } = useTranslation()
  return useQuery({
    queryKey: ['admin-report', report, range.from, range.to, page ?? null, i18n.language],
    queryFn: () => api
      .get<T>(`/admin/reports/${report}`, {
        params: {
          FromUtc: startUtc(range.from),
          ToUtc: exclusiveEndUtc(range.to),
          ...(page === undefined ? {} : { Page: page, PageSize: pageSize }),
        },
      })
      .then((response) => response.data),
  })
}

function SummaryCard({ label, value }: { label: string; value: ReactNode }) {
  return (
    <article className="card min-w-0 p-5">
      <p className="muted break-words text-sm [overflow-wrap:anywhere]">{label}</p>
      <strong className="mt-2 block break-words text-2xl [overflow-wrap:anywhere]" dir="auto">{value}</strong>
    </article>
  )
}

function ReportDetail({
  children,
  direction = 'auto',
  label,
}: {
  children: ReactNode
  direction?: 'auto' | 'ltr'
  label: string
}) {
  return (
    <div className="grid min-w-0 gap-1 border-b py-3 last:border-0">
      <dt className="muted break-words text-xs font-semibold [overflow-wrap:anywhere]">{label}</dt>
      <dd className="min-w-0 break-words text-sm font-semibold [overflow-wrap:anywhere]" dir={direction}>{children}</dd>
    </div>
  )
}

function MetricTable({
  items,
  locale,
  title,
}: {
  items: NamedMetric[]
  locale: string
  title: string
}) {
  const { t } = useTranslation()
  return (
    <article className="card min-w-0 overflow-hidden">
      <h3 className="break-words p-5 text-lg font-bold [overflow-wrap:anywhere]">{title}</h3>
      {!items.length
        ? <p className="muted border-t p-5">{t('admin.reports.noBreakdown')}</p>
        : (
          <>
            <div className="grid gap-3 border-t p-4 md:hidden">
              {items.map((item, index) => (
                <article className="min-w-0 rounded-xl border bg-stone-50 px-4" key={`${item.name ?? 'metric'}-${index}`}>
                  <dl className="grid min-w-0 gap-0">
                    <ReportDetail label={t('admin.reports.metric')}>
                      {t(`admin.values.${item.name}`, { defaultValue: item.name ?? '—' })}
                    </ReportDetail>
                    <ReportDetail direction="ltr" label={t('admin.reports.count')}>
                      {new Intl.NumberFormat(locale).format(item.count ?? 0)}
                    </ReportDetail>
                    <ReportDetail direction="ltr" label={t('admin.reports.amount')}>
                      {formatMoney(item.amount ?? 0, 'ILS', locale)}
                    </ReportDetail>
                  </dl>
                </article>
              ))}
            </div>
            <div className="hidden overflow-x-auto border-t md:block">
            <table className="w-full table-auto text-sm">
              <caption className="sr-only">{title}</caption>
              <thead className="bg-[var(--heritage-beige)]">
                <tr>
                  <th className="px-4 py-3 text-start">{t('admin.reports.metric')}</th>
                  <th className="px-4 py-3 text-start">{t('admin.reports.count')}</th>
                  <th className="px-4 py-3 text-start">{t('admin.reports.amount')}</th>
                </tr>
              </thead>
              <tbody>
                {items.map((item, index) => (
                  <tr className="border-t" key={`${item.name ?? 'metric'}-${index}`}>
                    <td className="max-w-64 break-words px-4 py-3 font-semibold [overflow-wrap:anywhere]">
                      {t(`admin.values.${item.name}`, { defaultValue: item.name ?? '—' })}
                    </td>
                    <td className="break-words px-4 py-3 [overflow-wrap:anywhere]" dir="ltr">{new Intl.NumberFormat(locale).format(item.count ?? 0)}</td>
                    <td className="break-words px-4 py-3 [overflow-wrap:anywhere]" dir="ltr">{formatMoney(item.amount ?? 0, 'ILS', locale)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
            </div>
          </>
        )}
    </article>
  )
}

function ReportPagination({
  page,
  setPage,
  totalPages,
}: {
  page: number
  setPage: (page: number) => void
  totalPages: number
}) {
  const { t } = useTranslation()
  if (totalPages <= 1) return null
  return (
    <nav aria-label={t('admin.table.pagination')} className="mt-5 flex flex-wrap items-center justify-center gap-2 sm:justify-end">
      <button
        className="button-secondary min-w-0 flex-1 whitespace-normal sm:flex-none"
        disabled={page <= 1}
        onClick={() => setPage(page - 1)}
        type="button"
      >
        {t('actions.previous')}
      </button>
      <span className="badge order-first flex w-full justify-center whitespace-normal text-center sm:order-none sm:w-auto">{t('admin.table.pageOf', { page, total: totalPages })}</span>
      <button
        className="button-secondary min-w-0 flex-1 whitespace-normal sm:flex-none"
        disabled={page >= totalPages}
        onClick={() => setPage(page + 1)}
        type="button"
      >
        {t('actions.next')}
      </button>
    </nav>
  )
}

function SalesReport({ from, to }: ReportRange) {
  const { t, i18n } = useTranslation()
  const query = useReportQuery<Schema<'SalesReportResponse'>>('sales', { from, to })
  if (query.isLoading) return <LoadingState />
  if (query.isError) return <ErrorState error={query.error} retry={() => void query.refetch()} />

  const data = query.data
  const locale = i18n.language.startsWith('ar') ? 'ar-PS-u-nu-latn' : 'en-US'
  const daily = data?.daily ?? []
  return (
    <>
      <div className="grid gap-4 sm:grid-cols-3">
        <SummaryCard
          label={t('admin.reports.sales.totalSales')}
          value={formatMoney(data?.totalSales ?? 0, 'ILS', locale)}
        />
        <SummaryCard
          label={t('admin.reports.sales.orders')}
          value={new Intl.NumberFormat(locale).format(data?.orderCount ?? 0)}
        />
        <SummaryCard
          label={t('admin.reports.sales.averageOrder')}
          value={formatMoney(data?.averageOrderValue ?? 0, 'ILS', locale)}
        />
      </div>
      <article className="card mt-6 min-w-0 p-4 sm:p-5">
        <h3 className="mb-5 break-words text-xl font-bold [overflow-wrap:anywhere]">{t('admin.reports.sales.dailySales')}</h3>
        {!daily.length
          ? <EmptyState message={t('admin.reports.emptyRange')} />
          : (
            <div className="min-w-0">
              {/* Axis text is positioned for left-to-right layout, so in an RTL page it landed on top of the axis line. Numbers read left to right anyway. */}
              <div aria-hidden className="h-64 sm:h-96" dir="ltr">
                <ResponsiveContainer height="100%" width="100%">
                <BarChart data={daily} margin={{ bottom: 8, left: 4, right: 12, top: 16 }}>
                  <CartesianGrid strokeDasharray="3 3" vertical={false} />
                  <XAxis dataKey="dateUtc" height={44} minTickGap={24} padding={{ left: 20, right: 20 }} tickFormatter={(value) => formatDateCompact(value, locale)} tickMargin={10} />
                  <YAxis tickMargin={4} width={64} />
                  <Tooltip />
                  <Bar dataKey="sales" fill="#850b12" radius={[4, 4, 0, 0]} />
                </BarChart>
                </ResponsiveContainer>
              </div>
              <ul aria-label={t('admin.reports.sales.dailySales')} className="mt-4 grid min-w-0 gap-2 text-sm sm:sr-only">
                {daily.map((item, index) => (
                  <li className="flex min-w-0 flex-wrap items-start justify-between gap-2 rounded-xl bg-stone-50 p-3" key={`${item.dateUtc ?? 'date'}-${index}`}>
                    <span className="min-w-0 flex-1 break-words [overflow-wrap:anywhere]">{formatDate(item.dateUtc, locale)}</span>
                    <strong className="shrink-0" dir="ltr">{formatMoney(item.sales ?? 0, 'ILS', locale)}</strong>
                  </li>
                ))}
              </ul>
            </div>
          )}
      </article>
      <div className="mt-6 grid gap-6 lg:grid-cols-2 2xl:grid-cols-3">
        <MetricTable
          items={data?.byPaymentMethod ?? []}
          locale={locale}
          title={t('admin.reports.sales.byPaymentMethod')}
        />
        <MetricTable
          items={data?.byOrderStatus ?? []}
          locale={locale}
          title={t('admin.reports.sales.byOrderStatus')}
        />
        <MetricTable
          items={data?.byCategory ?? []}
          locale={locale}
          title={t('admin.reports.sales.byCategory')}
        />
      </div>
    </>
  )
}

function ProductsReport({ from, page, setPage, to }: PagedReportProps) {
  const { t, i18n } = useTranslation()
  const query = useReportQuery<Schema<'ProductReportResponse'>>('products', { from, to }, page)
  if (query.isLoading) return <LoadingState />
  if (query.isError) return <ErrorState error={query.error} retry={() => void query.refetch()} />

  const data = query.data
  const products = data?.products
  const items = products?.items ?? []
  const locale = i18n.language.startsWith('ar') ? 'ar-PS-u-nu-latn' : 'en-US'
  return (
    <>
      <div className="grid gap-4 sm:grid-cols-3">
        <SummaryCard
          label={t('admin.reports.products.totalProducts')}
          value={new Intl.NumberFormat(locale).format(products?.totalItems ?? 0)}
        />
        <SummaryCard
          label={t('admin.reports.products.lowStock')}
          value={new Intl.NumberFormat(locale).format(data?.lowStockCount ?? 0)}
        />
        <SummaryCard
          label={t('admin.reports.products.outOfStock')}
          value={new Intl.NumberFormat(locale).format(data?.outOfStockCount ?? 0)}
        />
      </div>
      {!items.length
        ? <div className="mt-6"><EmptyState message={t('admin.reports.emptyRange')} /></div>
        : (
          <div className="mt-6 min-w-0">
            <div className="grid gap-4 md:hidden">
              {items.map((item, index) => (
                <article className="card min-w-0 px-4" key={item.productId ?? `${item.sku ?? 'product'}-${index}`}>
                  <dl className="grid min-w-0 gap-0 sm:grid-cols-2">
                    <ReportDetail label={t('admin.fields.name')}>{item.name ?? '—'}</ReportDetail>
                    <ReportDetail direction="ltr" label={t('admin.fields.sku')}><span className="break-all">{item.sku ?? '—'}</span></ReportDetail>
                    <ReportDetail direction="ltr" label={t('admin.reports.products.quantitySold')}>{new Intl.NumberFormat(locale).format(item.quantitySold ?? 0)}</ReportDetail>
                    <ReportDetail direction="ltr" label={t('admin.reports.products.sales')}>{formatMoney(item.sales ?? 0, 'ILS', locale)}</ReportDetail>
                    <ReportDetail direction="ltr" label={t('admin.fields.availableQuantity')}>{new Intl.NumberFormat(locale).format(item.availableQuantity ?? 0)}</ReportDetail>
                  </dl>
                </article>
              ))}
            </div>
            <div className="card hidden min-w-0 overflow-x-auto md:block">
              <table className="w-full table-auto text-sm">
                <caption className="sr-only">{t('admin.reports.tabs.products')}</caption>
                <thead className="bg-[var(--heritage-beige)]">
                  <tr>
                    <th className="px-4 py-3 text-start">{t('admin.fields.name')}</th>
                    <th className="px-4 py-3 text-start">{t('admin.fields.sku')}</th>
                    <th className="px-4 py-3 text-start">{t('admin.reports.products.quantitySold')}</th>
                    <th className="px-4 py-3 text-start">{t('admin.reports.products.sales')}</th>
                    <th className="px-4 py-3 text-start">{t('admin.fields.availableQuantity')}</th>
                  </tr>
                </thead>
                <tbody>
                  {items.map((item, index) => (
                    <tr className="border-t" key={item.productId ?? `${item.sku ?? 'product'}-${index}`}>
                      <td className="max-w-72 break-words px-4 py-3 font-semibold [overflow-wrap:anywhere]" dir="auto">{item.name ?? '—'}</td>
                      <td className="max-w-56 break-all px-4 py-3 text-start" dir="ltr">{item.sku ?? '—'}</td>
                      <td className="break-words px-4 py-3 [overflow-wrap:anywhere]" dir="ltr">{new Intl.NumberFormat(locale).format(item.quantitySold ?? 0)}</td>
                      <td className="break-words px-4 py-3 [overflow-wrap:anywhere]" dir="ltr">{formatMoney(item.sales ?? 0, 'ILS', locale)}</td>
                      <td className="break-words px-4 py-3 [overflow-wrap:anywhere]" dir="ltr">{new Intl.NumberFormat(locale).format(item.availableQuantity ?? 0)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      <ReportPagination page={page} setPage={setPage} totalPages={products?.totalPages ?? 0} />
    </>
  )
}

function CustomersReport({ from, page, setPage, to }: PagedReportProps) {
  const { t, i18n } = useTranslation()
  const query = useReportQuery<Schema<'CustomerReportResponse'>>('customers', { from, to }, page)
  if (query.isLoading) return <LoadingState />
  if (query.isError) return <ErrorState error={query.error} retry={() => void query.refetch()} />

  const data = query.data
  const customers = data?.customers
  const items = customers?.items ?? []
  const locale = i18n.language.startsWith('ar') ? 'ar-PS-u-nu-latn' : 'en-US'
  return (
    <>
      <div className="grid gap-4 sm:grid-cols-3">
        <SummaryCard
          label={t('admin.reports.customers.totalCustomers')}
          value={new Intl.NumberFormat(locale).format(customers?.totalItems ?? 0)}
        />
        <SummaryCard
          label={t('admin.reports.customers.newCustomers')}
          value={new Intl.NumberFormat(locale).format(data?.newCustomerCount ?? 0)}
        />
        <SummaryCard
          label={t('admin.reports.customers.inactiveCustomers')}
          value={new Intl.NumberFormat(locale).format(data?.inactiveCustomerCount ?? 0)}
        />
      </div>
      {!items.length
        ? <div className="mt-6"><EmptyState message={t('admin.reports.emptyRange')} /></div>
        : (
          <div className="mt-6 min-w-0">
            <div className="grid gap-4 md:hidden">
              {items.map((item, index) => (
                <article className="card min-w-0 px-4" key={item.customerId ?? `${item.phoneNumber ?? 'customer'}-${index}`}>
                  <dl className="grid min-w-0 gap-0 sm:grid-cols-2">
                    <ReportDetail label={t('admin.fields.name')}>{item.name ?? '—'}</ReportDetail>
                    <ReportDetail direction="ltr" label={t('admin.fields.phoneNumber')}><span className="break-all">{item.phoneNumber ?? '—'}</span></ReportDetail>
                    <ReportDetail direction="ltr" label={t('admin.fields.orderCount')}>{new Intl.NumberFormat(locale).format(item.orderCount ?? 0)}</ReportDetail>
                    <ReportDetail direction="ltr" label={t('admin.reports.customers.spending')}>{formatMoney(item.spending ?? 0, 'ILS', locale)}</ReportDetail>
                    <ReportDetail direction="ltr" label={t('admin.fields.createdAtUtc')}>{formatDateCompact(item.createdAtUtc, locale)}</ReportDetail>
                    <ReportDetail direction="ltr" label={t('admin.fields.lastLoginAtUtc')}>{formatDateCompact(item.lastLoginAtUtc, locale)}</ReportDetail>
                  </dl>
                </article>
              ))}
            </div>
            <div className="card hidden min-w-0 overflow-x-auto md:block">
              <table className="w-full table-auto text-sm">
                <caption className="sr-only">{t('admin.reports.tabs.customers')}</caption>
                <thead className="bg-[var(--heritage-beige)]">
                  <tr>
                    <th className="px-4 py-3 text-start">{t('admin.fields.name')}</th>
                    <th className="px-4 py-3 text-start">{t('admin.fields.phoneNumber')}</th>
                    <th className="px-4 py-3 text-start">{t('admin.fields.orderCount')}</th>
                    <th className="px-4 py-3 text-start">{t('admin.reports.customers.spending')}</th>
                    <th className="px-4 py-3 text-start">{t('admin.fields.createdAtUtc')}</th>
                    <th className="px-4 py-3 text-start">{t('admin.fields.lastLoginAtUtc')}</th>
                  </tr>
                </thead>
                <tbody>
                  {items.map((item, index) => (
                    <tr className="border-t" key={item.customerId ?? `${item.phoneNumber ?? 'customer'}-${index}`}>
                      <td className="max-w-72 break-words px-4 py-3 font-semibold [overflow-wrap:anywhere]" dir="auto">{item.name ?? '—'}</td>
                      <td className="max-w-56 break-all px-4 py-3 text-start" dir="ltr">{item.phoneNumber ?? '—'}</td>
                      <td className="break-words px-4 py-3 [overflow-wrap:anywhere]" dir="ltr">{new Intl.NumberFormat(locale).format(item.orderCount ?? 0)}</td>
                      <td className="break-words px-4 py-3 [overflow-wrap:anywhere]" dir="ltr">{formatMoney(item.spending ?? 0, 'ILS', locale)}</td>
                      <td className="break-words px-4 py-3 [overflow-wrap:anywhere]" dir="ltr">{formatDateCompact(item.createdAtUtc, locale)}</td>
                      <td className="break-words px-4 py-3 [overflow-wrap:anywhere]" dir="ltr">{formatDateCompact(item.lastLoginAtUtc, locale)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      <ReportPagination page={page} setPage={setPage} totalPages={customers?.totalPages ?? 0} />
    </>
  )
}

function OrdersReport({ from, to }: ReportRange) {
  const { t, i18n } = useTranslation()
  const query = useReportQuery<Schema<'OrderReportResponse'>>('orders', { from, to })
  if (query.isLoading) return <LoadingState />
  if (query.isError) return <ErrorState error={query.error} retry={() => void query.refetch()} />

  const data = query.data
  const locale = i18n.language.startsWith('ar') ? 'ar-PS-u-nu-latn' : 'en-US'
  const byCity = data?.byCity ?? []
  const byPaymentMethod = data?.byPaymentMethod ?? []
  return (
    <>
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <SummaryCard label={t('admin.reports.orders.new')} value={new Intl.NumberFormat(locale).format(data?.newCount ?? 0)} />
        <SummaryCard label={t('admin.reports.orders.completed')} value={new Intl.NumberFormat(locale).format(data?.completedCount ?? 0)} />
        <SummaryCard label={t('admin.reports.orders.cancelled')} value={new Intl.NumberFormat(locale).format(data?.cancelledCount ?? 0)} />
        <SummaryCard label={t('admin.reports.orders.returned')} value={new Intl.NumberFormat(locale).format(data?.returnedCount ?? 0)} />
      </div>
      {!byCity.length && !byPaymentMethod.length
        ? <div className="mt-6"><EmptyState message={t('admin.reports.emptyRange')} /></div>
        : (
          <div className="mt-6 grid gap-6 xl:grid-cols-2">
            <MetricTable items={byCity} locale={locale} title={t('admin.reports.orders.byCity')} />
            <MetricTable items={byPaymentMethod} locale={locale} title={t('admin.reports.orders.byPaymentMethod')} />
          </div>
        )}
    </>
  )
}

function InventoryReport({ from, page, setPage, to }: PagedReportProps) {
  const { t, i18n } = useTranslation()
  const query = useReportQuery<Schema<'InventoryReportResponse'>>('inventory', { from, to }, page)
  if (query.isLoading) return <LoadingState />
  if (query.isError) return <ErrorState error={query.error} retry={() => void query.refetch()} />

  const data = query.data
  const inventory = data?.items
  const items = inventory?.items ?? []
  const locale = i18n.language.startsWith('ar') ? 'ar-PS-u-nu-latn' : 'en-US'
  const totalOnHand = data?.totalOnHand ?? 0
  const totalReserved = data?.totalReserved ?? 0
  return (
    <>
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <SummaryCard label={t('admin.reports.inventory.totalItems')} value={new Intl.NumberFormat(locale).format(inventory?.totalItems ?? 0)} />
        <SummaryCard label={t('admin.reports.inventory.onHand')} value={new Intl.NumberFormat(locale).format(totalOnHand)} />
        <SummaryCard label={t('admin.reports.inventory.reserved')} value={new Intl.NumberFormat(locale).format(totalReserved)} />
        <SummaryCard label={t('admin.reports.inventory.available')} value={new Intl.NumberFormat(locale).format(totalOnHand - totalReserved)} />
      </div>
      {!items.length
        ? <div className="mt-6"><EmptyState /></div>
        : (
          <div className="mt-6 min-w-0">
            <div className="grid gap-4 md:hidden">
              {items.map((item, index) => {
                const status = item.isOutOfStock
                  ? t('admin.reports.inventory.outOfStock')
                  : item.isLowStock
                    ? t('admin.reports.inventory.lowStock')
                    : t('admin.reports.inventory.inStock')
                return (
                  <article className="card min-w-0 px-4" key={item.productVariantId ?? `${item.sku ?? 'inventory'}-${index}`}>
                    <dl className="grid min-w-0 gap-0 sm:grid-cols-2">
                      <ReportDetail direction="ltr" label={t('admin.fields.sku')}><span className="break-all">{item.sku ?? '—'}</span></ReportDetail>
                      <ReportDetail label={t('admin.reports.inventory.product')}>{item.productName ?? '—'}</ReportDetail>
                      <ReportDetail direction="ltr" label={t('admin.fields.quantityOnHand')}>{new Intl.NumberFormat(locale).format(item.quantityOnHand ?? 0)}</ReportDetail>
                      <ReportDetail direction="ltr" label={t('admin.fields.reservedQuantity')}>{new Intl.NumberFormat(locale).format(item.reservedQuantity ?? 0)}</ReportDetail>
                      <ReportDetail direction="ltr" label={t('admin.fields.availableQuantity')}>{new Intl.NumberFormat(locale).format(item.availableQuantity ?? 0)}</ReportDetail>
                      <ReportDetail direction="ltr" label={t('admin.fields.reorderLevel')}>{new Intl.NumberFormat(locale).format(item.reorderLevel ?? 0)}</ReportDetail>
                      <ReportDetail label={t('admin.fields.status')}><span className="badge max-w-full whitespace-normal break-words [overflow-wrap:anywhere]">{status}</span></ReportDetail>
                    </dl>
                  </article>
                )
              })}
            </div>
            <div className="card hidden min-w-0 overflow-x-auto md:block">
              <table className="w-full min-w-[940px] table-auto text-sm">
                <caption className="sr-only">{t('admin.reports.tabs.inventory')}</caption>
                <thead className="bg-[var(--heritage-beige)]">
                  <tr>
                    <th className="px-4 py-3 text-start">{t('admin.fields.sku')}</th>
                    <th className="px-4 py-3 text-start">{t('admin.reports.inventory.product')}</th>
                    <th className="px-4 py-3 text-start">{t('admin.fields.quantityOnHand')}</th>
                    <th className="px-4 py-3 text-start">{t('admin.fields.reservedQuantity')}</th>
                    <th className="px-4 py-3 text-start">{t('admin.fields.availableQuantity')}</th>
                    <th className="px-4 py-3 text-start">{t('admin.fields.reorderLevel')}</th>
                    <th className="px-4 py-3 text-start">{t('admin.fields.status')}</th>
                  </tr>
                </thead>
                <tbody>
                  {items.map((item, index) => {
                    const status = item.isOutOfStock
                      ? t('admin.reports.inventory.outOfStock')
                      : item.isLowStock
                        ? t('admin.reports.inventory.lowStock')
                        : t('admin.reports.inventory.inStock')
                    return (
                      <tr className="border-t" key={item.productVariantId ?? `${item.sku ?? 'inventory'}-${index}`}>
                        <td className="max-w-56 break-all px-4 py-3 text-start" dir="ltr">{item.sku ?? '—'}</td>
                        <td className="max-w-72 break-words px-4 py-3 font-semibold [overflow-wrap:anywhere]" dir="auto">{item.productName ?? '—'}</td>
                        <td className="break-words px-4 py-3 [overflow-wrap:anywhere]" dir="ltr">{new Intl.NumberFormat(locale).format(item.quantityOnHand ?? 0)}</td>
                        <td className="break-words px-4 py-3 [overflow-wrap:anywhere]" dir="ltr">{new Intl.NumberFormat(locale).format(item.reservedQuantity ?? 0)}</td>
                        <td className="break-words px-4 py-3 [overflow-wrap:anywhere]" dir="ltr">{new Intl.NumberFormat(locale).format(item.availableQuantity ?? 0)}</td>
                        <td className="break-words px-4 py-3 [overflow-wrap:anywhere]" dir="ltr">{new Intl.NumberFormat(locale).format(item.reorderLevel ?? 0)}</td>
                        <td className="px-4 py-3"><span className="badge max-w-full whitespace-normal break-words [overflow-wrap:anywhere]">{status}</span></td>
                      </tr>
                    )
                  })}
                </tbody>
              </table>
            </div>
          </div>
        )}
      <ReportPagination page={page} setPage={setPage} totalPages={inventory?.totalPages ?? 0} />
    </>
  )
}

export function AdminReportsPage() {
  const { t, i18n } = useTranslation()
  const permissions = useSessionStore((state) => state.admin?.permissions ?? [])
  const [selected, setSelected] = useState<ReportKey>('sales')
  const [from, setFrom] = useState(new Date(Date.now() - 30 * 86400000).toISOString().slice(0, 10))
  const [to, setTo] = useState(new Date().toISOString().slice(0, 10))
  const [page, setPage] = useState(1)
  const allowedReports = reportDefinitions.filter((report) => permissions.includes(report.permission))
  const activeReport = allowedReports.find((report) => report.key === selected) ?? allowedReports[0]

  if (!activeReport) return <ForbiddenState />

  const selectReport = (report: ReportKey) => {
    if (!allowedReports.some((item) => item.key === report)) return
    setSelected(report)
    setPage(1)
  }
  const handleTabKeyDown = (event: KeyboardEvent<HTMLButtonElement>, index: number) => {
    let nextIndex: number | undefined
    const rtl = i18n.language.startsWith('ar')
    if (event.key === 'Home') nextIndex = 0
    if (event.key === 'End') nextIndex = allowedReports.length - 1
    if (event.key === 'ArrowRight') nextIndex = (index + (rtl ? -1 : 1) + allowedReports.length) % allowedReports.length
    if (event.key === 'ArrowLeft') nextIndex = (index + (rtl ? 1 : -1) + allowedReports.length) % allowedReports.length
    if (nextIndex === undefined) return

    event.preventDefault()
    const report = allowedReports[nextIndex]
    selectReport(report.key)
    window.requestAnimationFrame(() => document.getElementById(`report-tab-${report.key}`)?.focus())
  }
  const range = { from, to }
  const rangeValid = isValidRange(range)
  const dateErrorId = 'reports-date-error'

  return (
    <section>
      <h1 className="mb-6 min-w-0 break-words text-3xl font-extrabold [overflow-wrap:anywhere]">{t('admin.reports.title')}</h1>

      <label className="mb-4 block sm:hidden">
        <span className="mb-1 block text-sm font-bold">{t('admin.reports.selectLabel')}</span>
        <select
          className="field"
          onChange={(event) => selectReport(event.target.value as ReportKey)}
          value={activeReport.key}
        >
          {allowedReports.map((report) => (
            <option key={report.key} value={report.key}>{t(`admin.reports.tabs.${report.key}`)}</option>
          ))}
        </select>
      </label>
      <div
        aria-label={t('admin.reports.tabsLabel')}
        className="mb-6 hidden min-w-0 flex-wrap gap-2 rounded-2xl bg-white p-2 shadow-sm sm:flex"
        role="tablist"
      >
        {allowedReports.map((report, index) => {
          const active = activeReport.key === report.key
          return (
            <button
              aria-controls="report-panel"
              aria-selected={active}
              className={`min-h-11 min-w-0 whitespace-normal break-words rounded-xl px-4 py-2 text-sm font-bold [overflow-wrap:anywhere] ${
                active
                  ? 'bg-[var(--heritage-burgundy)] text-white'
                  : 'text-[var(--heritage-charcoal)] hover:bg-[var(--heritage-beige)]'
              }`}
              id={`report-tab-${report.key}`}
              key={report.key}
              onClick={() => selectReport(report.key)}
              onKeyDown={(event) => handleTabKeyDown(event, index)}
              role="tab"
              tabIndex={active ? 0 : -1}
              type="button"
            >
              {t(`admin.reports.tabs.${report.key}`)}
            </button>
          )
        })}
      </div>

      <fieldset
        aria-describedby={!rangeValid ? dateErrorId : undefined}
        className="card mb-6 grid min-w-0 gap-4 p-4 sm:grid-cols-2"
      >
        <legend className="max-w-full break-words px-2 text-sm font-bold [overflow-wrap:anywhere]">{t('admin.reports.dateRange')}</legend>
        <label className="grid min-w-0 gap-1 text-sm font-semibold">
          <span className="break-words [overflow-wrap:anywhere]">{t('admin.reports.from')}</span>
          <input
            aria-invalid={!rangeValid}
            className="field"
            dir="ltr"
            max={to}
            type="date"
            value={from}
            onChange={(event) => {
              setFrom(event.target.value)
              setPage(1)
            }}
          />
        </label>
        <label className="grid min-w-0 gap-1 text-sm font-semibold">
          <span className="break-words [overflow-wrap:anywhere]">{t('admin.reports.to')}</span>
          <input
            aria-invalid={!rangeValid}
            className="field"
            dir="ltr"
            min={from}
            type="date"
            value={to}
            onChange={(event) => {
              setTo(event.target.value)
              setPage(1)
            }}
          />
        </label>
        {!rangeValid && (
          <p className="break-words text-sm font-semibold text-[var(--heritage-red)] [overflow-wrap:anywhere] sm:col-span-2" id={dateErrorId} role="alert">
            {t('admin.reports.invalidRange')}
          </p>
        )}
      </fieldset>

      <div aria-labelledby="active-report-heading" className="min-w-0" id="report-panel" role="tabpanel" tabIndex={0}>
        <h2 className="mb-5 break-words text-2xl font-extrabold [overflow-wrap:anywhere]" id="active-report-heading">
          {t(`admin.reports.tabs.${activeReport.key}`)}
        </h2>
        {rangeValid && activeReport.key === 'sales' && <SalesReport from={from} to={to} />}
        {rangeValid && activeReport.key === 'products' && (
          <ProductsReport from={from} page={page} setPage={setPage} to={to} />
        )}
        {rangeValid && activeReport.key === 'customers' && (
          <CustomersReport from={from} page={page} setPage={setPage} to={to} />
        )}
        {rangeValid && activeReport.key === 'orders' && <OrdersReport from={from} to={to} />}
        {rangeValid && activeReport.key === 'inventory' && (
          <InventoryReport from={from} page={page} setPage={setPage} to={to} />
        )}
      </div>
    </section>
  )
}
