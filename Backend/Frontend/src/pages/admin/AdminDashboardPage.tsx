import { useQuery } from '@tanstack/react-query'
import { CircleDollarSign, Package, ShoppingCart, Users } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { api } from '@/lib/api/client'
import type { Dashboard } from '@/types/models'
import { formatMoney } from '@/lib/utils'
import { EmptyState, ErrorState, ForbiddenState, LoadingState } from '@/components/ui/Feedback'
import { adminPermissions } from '@/config/adminResources'
import { useSessionStore } from '@/stores/sessionStore'

function DashboardContent() {
  const { t, i18n } = useTranslation()
  const query = useQuery({
    queryKey: ['admin-dashboard', i18n.language],
    queryFn: () => api.get<Dashboard>('/admin/dashboard').then((response) => response.data),
  })
  if (query.isLoading) return <LoadingState />
  if (query.isError) return <ErrorState error={query.error} retry={() => void query.refetch()} />

  const data = query.data
  const locale = i18n.language.startsWith('ar') ? 'ar-PS-u-nu-latn' : 'en-US'
  const topSales = Math.max(...(data?.bestSellingProducts ?? []).map((item) => item.sales ?? 0), 1)
  const cards = [
    [t('admin.dashboardCards.todaySales'), formatMoney(data?.todaySales ?? 0, 'ILS', locale), CircleDollarSign],
    [t('admin.dashboardCards.newOrders'), data?.newOrderCount ?? 0, ShoppingCart],
    [t('admin.dashboardCards.customers'), data?.customerCount ?? 0, Users],
    [t('admin.dashboardCards.lowStock'), data?.lowStockProductCount ?? 0, Package],
  ] as const

  return (
    <section className="min-w-0">
      <h1 className="mb-7 min-w-0 break-words text-3xl font-extrabold [overflow-wrap:anywhere]">{t('admin.dashboard')}</h1>
      <div className="grid grid-cols-2 gap-3 sm:gap-4 xl:grid-cols-4">
        {cards.map(([label, value, Icon]) => (
          <article className="card min-w-0 p-4 sm:p-5" key={label}>
            <div className="grid size-10 place-items-center rounded-xl bg-[var(--heritage-beige)] text-[var(--heritage-burgundy)]">
              <Icon aria-hidden size={20} />
            </div>
            <p className="muted mt-4 break-words text-sm [overflow-wrap:anywhere]">{label}</p>
            <strong className="mt-1 block break-words text-xl font-extrabold sm:text-2xl [overflow-wrap:anywhere]"><bdi>{value}</bdi></strong>
          </article>
        ))}
      </div>
      <div className="mt-6 grid gap-6 xl:grid-cols-[1.5fr_1fr]">
        <article className="card min-w-0 p-4 sm:p-5">
          <h2 className="mb-5 break-words text-xl font-bold [overflow-wrap:anywhere]">{t('admin.bestSelling')}</h2>
          {data?.bestSellingProducts?.length
            ? (
              // Products are a ranking, not a trend, so they are bars (longest first) rather than a
              // line through unrelated names: nothing is truncated and every amount is written out.
              <ol aria-label={t('admin.bestSelling')} className="grid min-w-0 gap-5">
                {data.bestSellingProducts.map((item, index) => (
                  <li className="min-w-0" key={`${item.name ?? 'product'}-${index}`}>
                    <div className="flex min-w-0 items-baseline justify-between gap-3">
                      <span className="min-w-0 flex-1 break-words font-semibold [overflow-wrap:anywhere]">{item.name}</span>
                      <strong className="shrink-0 text-sm text-[var(--heritage-burgundy)]" dir="ltr">{formatMoney(item.sales ?? 0, 'ILS', locale)}</strong>
                    </div>
                    <div aria-hidden className="mt-2 h-2 overflow-hidden rounded-full bg-[var(--heritage-beige)]">
                      <div className="h-full rounded-full bg-[var(--heritage-burgundy)]" style={{ width: `${Math.max(3, ((item.sales ?? 0) / topSales) * 100)}%` }} />
                    </div>
                  </li>
                ))}
              </ol>
            )
            : <EmptyState message={t('admin.noSalesData')} />}
        </article>
        <article className="card min-w-0 p-4 sm:p-5">
          <h2 className="break-words text-xl font-bold [overflow-wrap:anywhere]">{t('admin.importantNotifications')}</h2>
          {data?.importantNotifications?.length
            ? (
              <div className="mt-5 grid gap-3">
                {data.importantNotifications.slice(0, 6).map((item, index) => (
                  <div className="min-w-0 rounded-xl bg-stone-50 p-3" key={`${item.title ?? 'notification'}-${index}`}>
                    <strong className="block break-words text-sm [overflow-wrap:anywhere]" dir="auto">{item.title}</strong>
                    <p className="muted mt-1 break-words text-xs [overflow-wrap:anywhere]">
                      {t(`admin.values.${item.channel}`, { defaultValue: item.channel })} •{' '}
                      {t(`admin.values.${item.status}`, { defaultValue: item.status })}
                    </p>
                  </div>
                ))}
              </div>
            )
            : <div className="mt-5"><EmptyState message={t('admin.noNotifications')} /></div>}
        </article>
      </div>
    </section>
  )
}

export function AdminDashboardPage() {
  const allowed = useSessionStore(
    (state) => state.admin?.permissions.includes(adminPermissions.dashboard) ?? false,
  )
  return allowed ? <DashboardContent /> : <ForbiddenState />
}
