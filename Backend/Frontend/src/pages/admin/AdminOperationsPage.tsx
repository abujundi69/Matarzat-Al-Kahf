import { CircleDollarSign, ShoppingCart } from 'lucide-react'
import { useRef, type KeyboardEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { useSearchParams } from 'react-router-dom'
import { ForbiddenState } from '@/components/ui/Feedback'
import { adminPermissions } from '@/config/adminResources'
import { AdminOrdersPage } from '@/pages/admin/AdminOrdersPage'
import { AdminResourcePage } from '@/pages/admin/AdminResourcePage'
import { useSessionStore } from '@/stores/sessionStore'

type OperationsTab = 'orders' | 'payments'

const operationsPanelId = 'admin-operations-panel'
const operationTabId = (tab: OperationsTab) => `admin-operations-tab-${tab}`

export function AdminOperationsPage() {
  const { i18n, t } = useTranslation()
  const [searchParams, setSearchParams] = useSearchParams()
  const permissions = useSessionStore((state) => state.admin?.permissions ?? [])
  const tabRefs = useRef<Partial<Record<OperationsTab, HTMLButtonElement | null>>>({})
  const tabs = [
    permissions.includes(adminPermissions.ordersRead) ? 'orders' : null,
    permissions.includes(adminPermissions.paymentsRead) ? 'payments' : null,
  ].filter((value): value is OperationsTab => value !== null)
  if (!tabs.length) return <ForbiddenState />

  const requested = searchParams.get('tab') === 'payments' ? 'payments' : 'orders'
  const active = tabs.includes(requested) ? requested : tabs[0]
  const selectTab = (tab: OperationsTab) => {
    const next = new URLSearchParams(searchParams)
    if (tab === 'orders') next.delete('tab')
    else next.set('tab', tab)
    setSearchParams(next, { replace: true })
  }
  const handleTabKeyDown = (event: KeyboardEvent<HTMLButtonElement>, tab: OperationsTab) => {
    let nextIndex: number | undefined
    const currentIndex = tabs.indexOf(tab)

    if (event.key === 'Home') nextIndex = 0
    else if (event.key === 'End') nextIndex = tabs.length - 1
    else if (event.key === 'ArrowRight') {
      nextIndex = (currentIndex + (i18n.dir() === 'rtl' ? -1 : 1) + tabs.length) % tabs.length
    } else if (event.key === 'ArrowLeft') {
      nextIndex = (currentIndex + (i18n.dir() === 'rtl' ? 1 : -1) + tabs.length) % tabs.length
    }

    if (nextIndex === undefined) return
    event.preventDefault()
    const nextTab = tabs[nextIndex]
    selectTab(nextTab)
    tabRefs.current[nextTab]?.focus()
  }

  return (
    <section className="min-w-0">
      {tabs.length > 1 && (
        <div className="card mb-6 flex min-w-0 flex-wrap gap-2 p-2" role="tablist" aria-label={t('admin.operationTabs.label')}>
          <button
            aria-controls={operationsPanelId}
            aria-selected={active === 'orders'}
            className={`${active === 'orders' ? 'button-primary' : 'button-secondary'} min-w-0 flex-1 whitespace-normal sm:flex-none`}
            id={operationTabId('orders')}
            onClick={() => selectTab('orders')}
            onKeyDown={(event) => handleTabKeyDown(event, 'orders')}
            ref={(element) => { tabRefs.current.orders = element }}
            role="tab"
            tabIndex={active === 'orders' ? 0 : -1}
            type="button"
          >
            <ShoppingCart aria-hidden className="shrink-0" size={17} /> {t('admin.resources.orders')}
          </button>
          <button
            aria-controls={operationsPanelId}
            aria-selected={active === 'payments'}
            className={`${active === 'payments' ? 'button-primary' : 'button-secondary'} min-w-0 flex-1 whitespace-normal sm:flex-none`}
            id={operationTabId('payments')}
            onClick={() => selectTab('payments')}
            onKeyDown={(event) => handleTabKeyDown(event, 'payments')}
            ref={(element) => { tabRefs.current.payments = element }}
            role="tab"
            tabIndex={active === 'payments' ? 0 : -1}
            type="button"
          >
            <CircleDollarSign aria-hidden className="shrink-0" size={17} /> {t('admin.resources.payments')}
          </button>
        </div>
      )}
      <div
        aria-label={tabs.length === 1 ? t(`admin.resources.${active}`) : undefined}
        aria-labelledby={tabs.length > 1 ? operationTabId(active) : undefined}
        className="min-w-0"
        id={operationsPanelId}
        role="tabpanel"
        tabIndex={0}
      >
        {active === 'orders' ? <AdminOrdersPage /> : <AdminResourcePage resource="payments" />}
      </div>
    </section>
  )
}
