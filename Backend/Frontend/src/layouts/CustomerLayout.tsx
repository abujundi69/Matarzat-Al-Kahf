import { Bell, CreditCard, LogOut, Mail, MapPin, Package, UserRound } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { NavLink, Outlet } from 'react-router-dom'
import { ConfirmDialog } from '@/components/common/ConfirmDialog'
import { useCustomerLogout } from '@/lib/customerAuth'

const navigation = [
  ['profile', 'customer.resources.profile', UserRound],
  ['addresses', 'customer.resources.addresses', MapPin],
  ['orders', 'customer.resources.orders', Package],
  ['messages', 'customer.resources.messages', Mail],
  ['payments', 'customer.resources.payments', CreditCard],
  ['notifications', 'customer.resources.notifications', Bell],
] as const

export function CustomerLayout() {
  const { t } = useTranslation()
  const { cancelLogout, confirmLogout, confirmOpen, loggingOut, requestLogout } = useCustomerLogout('/login')

  return (
    <div className="page-shell grid min-w-0 gap-7 py-8 sm:py-12 lg:grid-cols-[260px_minmax(0,1fr)]">
      <aside aria-label={t('customer.navigation')} className="card h-fit p-3">
        <nav className="grid grid-cols-2 gap-1 sm:grid-cols-3 lg:grid-cols-1">
          {navigation.map(([path, key, Icon]) => (
            <NavLink
              className={({ isActive }) =>
                `flex min-h-11 min-w-0 items-center gap-2 rounded-xl px-3 py-3 text-start text-sm font-bold ${
                  isActive ? 'bg-[var(--heritage-burgundy)] text-white' : 'hover:bg-[var(--heritage-beige)]'
                }`
              }
              key={path}
              to={`/account/${path}`}
            >
              <Icon aria-hidden className="shrink-0" size={18} />
              <span className="min-w-0 leading-snug [overflow-wrap:anywhere]">{t(key)}</span>
            </NavLink>
          ))}
        </nav>
        <button
          className="mt-3 flex min-h-11 w-full items-center gap-2 rounded-xl border border-[var(--heritage-burgundy)]/20 px-3 py-3 text-start text-sm font-bold text-[var(--heritage-burgundy)] hover:bg-[var(--heritage-beige)]"
          disabled={loggingOut}
          onClick={requestLogout}
          type="button"
        >
          <LogOut aria-hidden className="shrink-0" size={18} />
          <span className="min-w-0 leading-snug [overflow-wrap:anywhere]">
            {loggingOut ? t('customer.signingOut') : t('actions.logout')}
          </span>
        </button>
      </aside>
      <section className="min-w-0"><Outlet /></section>
      <ConfirmDialog
        cancelLabel={t('actions.cancel')}
        confirmLabel={t('common.logoutConfirm.confirm')}
        message={t('common.logoutConfirm.message')}
        onCancel={cancelLogout}
        onConfirm={() => void confirmLogout()}
        open={confirmOpen}
        pending={loggingOut}
        title={t('common.logoutConfirm.title')}
        tone="danger"
      />
    </div>
  )
}
