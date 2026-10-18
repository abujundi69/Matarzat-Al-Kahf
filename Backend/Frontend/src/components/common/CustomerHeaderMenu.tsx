import { Bell, LogOut, MapPin, Package, RefreshCw, UserRound } from 'lucide-react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useId, useMemo, useRef, useState, type ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useLocation } from 'react-router-dom'
import { ConfirmDialog } from '@/components/common/ConfirmDialog'
import { api } from '@/lib/api/client'
import { useCustomerLogout } from '@/lib/customerAuth'
import { formatRelativeTime } from '@/lib/utils'
import { useSessionStore, type SessionActor } from '@/stores/sessionStore'
import type { Schema } from '@/types/models'

type OpenMenu = 'notifications' | 'account' | null
type NotificationPage = Schema<'NotificationResponsePagedResult'>
type Notification = Schema<'NotificationResponse'>

const previewCount = 5

const initialsFor = (value: string | null | undefined) => {
  const words = value?.trim().split(/\s+/).filter(Boolean) ?? []
  if (!words.length) return null
  return words.slice(0, 2).map((word) => Array.from(word)[0]).join('').toUpperCase()
}

function DropdownPanel({ children, id, title }: { children: ReactNode; id: string; title: string }) {
  return (
    <section
      aria-label={title}
      className="card absolute end-0 top-[calc(100%+.65rem)] z-[70] flex max-h-[min(30rem,calc(100dvh-6rem))] w-[min(22rem,calc(100vw-1.25rem))] flex-col overflow-hidden"
      id={id}
      role="dialog"
    >
      {children}
    </section>
  )
}

function PanelState({
  action,
  actionLabel,
  message,
}: {
  action?: () => void
  actionLabel?: string
  message: string
}) {
  return (
    <div className="grid min-h-28 place-items-center gap-3 px-5 py-8 text-center">
      <p className="muted text-sm">{message}</p>
      {action && actionLabel && (
        <button className="button-secondary !min-h-9 !px-3 text-sm" onClick={action} type="button">
          <RefreshCw aria-hidden size={15} />
          {actionLabel}
        </button>
      )}
    </div>
  )
}

function UnreadDot({ label }: { label: string }) {
  return (
    <span className="mt-1.5 size-2 shrink-0 rounded-full bg-[var(--heritage-red)]">
      <span className="sr-only">{label}</span>
    </span>
  )
}

function NotificationsPanel({
  close,
  id,
  notifications,
  onMarkAllRead,
  onMarkOneRead,
  query,
}: {
  close: () => void
  id: string
  notifications: Notification[]
  onMarkAllRead: () => void
  onMarkOneRead: (id: string) => void
  query: ReturnType<typeof useCustomerNotificationsQuery>
}) {
  const { t, i18n } = useTranslation()
  const locale = i18n.language.startsWith('ar') ? 'ar-PS-u-nu-latn' : 'en-US'
  const hasUnread = notifications.some((notification) => !notification.readAtUtc)

  return (
    <DropdownPanel id={id} title={t('customer.header.notifications.title')}>
      <header className="flex shrink-0 items-center justify-between gap-3 border-b border-black/5 px-4 py-3.5">
        <h2 className="text-base font-extrabold">{t('customer.header.notifications.title')}</h2>
        {hasUnread && (
          <button className="[overflow-wrap:anywhere] text-xs font-bold text-[var(--heritage-burgundy)] hover:underline" onClick={onMarkAllRead} type="button">
            {t('customer.header.notifications.markAllRead')}
          </button>
        )}
      </header>
      <div className="min-h-0 overflow-y-auto overscroll-contain">
        {query.isLoading
          ? <PanelState message={t('states.loading')} />
          : query.isError
            ? <PanelState action={() => void query.refetch()} actionLabel={t('actions.retry')} message={t('customer.header.notifications.loadError')} />
            : !notifications.length
              ? <PanelState message={t('customer.header.notifications.empty')} />
              : (
                <ul className="divide-y divide-black/5">
                  {notifications.map((notification, index) => {
                    const unread = !notification.readAtUtc
                    return (
                      <li key={notification.id ?? index}>
                        <Link
                          className={`grid min-w-0 grid-cols-[auto_minmax(0,1fr)] gap-3 px-4 py-3.5 transition hover:bg-[var(--heritage-beige)]/50 ${unread ? 'bg-[var(--heritage-cream)]' : 'bg-white'}`}
                          onClick={() => {
                            if (notification.id && unread) onMarkOneRead(notification.id)
                            close()
                          }}
                          to="/account/notifications"
                        >
                          {unread
                            ? <UnreadDot label={t('customer.header.notifications.unread')} />
                            : <span aria-hidden className="mt-1.5 size-2 shrink-0" />}
                          <span className="min-w-0">
                            <span className="clip-safe block truncate text-sm font-extrabold">
                              {notification.title?.trim() || t('customer.header.notifications.untitled')}
                            </span>
                            {notification.body?.trim() && <span className="muted mt-1 clip-safe block line-clamp-2 text-xs leading-relaxed">
                              {notification.body.trim()}
                            </span>}
                            <time className="mt-1.5 block text-xs font-bold text-[var(--heritage-burgundy)]" dateTime={notification.createdAtUtc}>
                              {formatRelativeTime(notification.createdAtUtc, locale)}
                            </time>
                          </span>
                        </Link>
                      </li>
                    )
                  })}
                </ul>
              )}
      </div>
      <footer className="shrink-0 border-t border-black/5 bg-[var(--heritage-beige)]/40 p-2.5">
        <Link
          className="flex min-h-11 w-full items-center justify-center rounded-xl px-3 text-center text-sm font-extrabold text-[var(--heritage-burgundy)] transition hover:bg-white"
          onClick={close}
          to="/account/notifications"
        >
          {t('customer.header.notifications.viewAll')}
        </Link>
      </footer>
    </DropdownPanel>
  )
}

function AccountPanel({
  actor,
  close,
  id,
  loggingOut,
  logout,
}: {
  actor: SessionActor | undefined
  close: () => void
  id: string
  loggingOut: boolean
  logout: () => void
}) {
  const { t } = useTranslation()
  const displayName = actor?.fullName?.trim() || t('customer.account')
  const initials = initialsFor(actor?.fullName)

  return (
    <DropdownPanel id={id} title={t('customer.header.account')}>
      <div className="flex min-w-0 items-center gap-3 border-b border-black/5 p-4">
        <span aria-hidden className="grid size-11 shrink-0 place-items-center rounded-full bg-[var(--heritage-burgundy)] font-extrabold text-white">
          {initials ?? <UserRound size={18} />}
        </span>
        <span className="min-w-0">
          <strong className="clip-safe block truncate text-sm [overflow-wrap:anywhere]" dir="auto">{displayName}</strong>
          {actor?.phoneNumber && <bdi className="muted mt-0.5 clip-safe block truncate text-xs" dir="ltr">{actor.phoneNumber}</bdi>}
          {actor?.email && <bdi className="muted mt-0.5 clip-safe block truncate text-xs" dir="ltr">{actor.email}</bdi>}
        </span>
      </div>
      <nav aria-label={t('customer.header.account')} className="grid gap-1 p-2">
        <Link className="flex min-h-11 items-center gap-3 rounded-xl px-3 text-sm font-bold transition hover:bg-[var(--heritage-beige)]/60" onClick={close} to="/account/profile">
          <UserRound aria-hidden className="shrink-0" size={18} />
          {t('customer.resources.profile')}
        </Link>
        <Link className="flex min-h-11 items-center gap-3 rounded-xl px-3 text-sm font-bold transition hover:bg-[var(--heritage-beige)]/60" onClick={close} to="/account/orders">
          <Package aria-hidden className="shrink-0" size={18} />
          {t('customer.resources.orders')}
        </Link>
        <Link className="flex min-h-11 items-center gap-3 rounded-xl px-3 text-sm font-bold transition hover:bg-[var(--heritage-beige)]/60" onClick={close} to="/account/addresses">
          <MapPin aria-hidden className="shrink-0" size={18} />
          {t('customer.resources.addresses')}
        </Link>
        <button
          className="flex min-h-11 w-full items-center gap-3 rounded-xl px-3 text-start text-sm font-bold text-[var(--heritage-red)] transition hover:bg-[var(--state-danger-surface)]"
          disabled={loggingOut}
          onClick={logout}
          type="button"
        >
          <LogOut aria-hidden className="shrink-0" size={18} />
          {loggingOut ? t('customer.signingOut') : t('actions.logout')}
        </button>
      </nav>
    </DropdownPanel>
  )
}

const useCustomerNotificationsQuery = (language: string) => useQuery({
  queryKey: ['customer-header', 'notifications', language],
  queryFn: () => api.get<NotificationPage>('/customer/notifications', {
    params: { page: 1, pageSize: previewCount },
  }).then((response) => response.data),
  refetchInterval: 60_000,
  staleTime: 30_000,
})

export function CustomerHeaderMenu() {
  const { t, i18n } = useTranslation()
  const [openMenu, setOpenMenu] = useState<OpenMenu>(null)
  const location = useLocation()
  const queryClient = useQueryClient()
  const actor = useSessionStore((state) => state.customer?.actor)
  const containerRef = useRef<HTMLDivElement>(null)
  const activeTriggerRef = useRef<HTMLButtonElement | null>(null)
  const notificationsId = useId()
  const accountId = useId()
  const { cancelLogout, confirmLogout, confirmOpen, loggingOut, requestLogout } = useCustomerLogout('/')
  const notificationQuery = useCustomerNotificationsQuery(i18n.language)
  const notifications = useMemo(() => notificationQuery.data?.items ?? [], [notificationQuery.data?.items])
  const unreadCount = notifications.filter((notification) => !notification.readAtUtc).length

  useEffect(() => {
    setOpenMenu(null)
  }, [location.pathname, location.search])

  useEffect(() => {
    if (!openMenu) return

    const closeOnOutsidePress = (event: PointerEvent) => {
      if (!containerRef.current?.contains(event.target as Node)) setOpenMenu(null)
    }
    const closeOnEscape = (event: KeyboardEvent) => {
      if (event.key !== 'Escape') return
      event.preventDefault()
      setOpenMenu(null)
      activeTriggerRef.current?.focus()
    }
    document.addEventListener('pointerdown', closeOnOutsidePress)
    document.addEventListener('keydown', closeOnEscape)
    return () => {
      document.removeEventListener('pointerdown', closeOnOutsidePress)
      document.removeEventListener('keydown', closeOnEscape)
    }
  }, [openMenu])

  const toggleMenu = (menu: Exclude<OpenMenu, null>, trigger: HTMLButtonElement) => {
    activeTriggerRef.current = trigger
    setOpenMenu((current) => current === menu ? null : menu)
  }
  const close = () => setOpenMenu(null)

  const invalidateNotifications = () => {
    void queryClient.invalidateQueries({
      predicate: ({ queryKey }) => queryKey[0] === 'customer-header'
        || (queryKey[0] === 'customer' && queryKey[1] === 'notifications'),
    })
  }
  const markOneRead = (notificationId: string) => {
    api.post(`/customer/notifications/${notificationId}/read`).then(invalidateNotifications).catch(() => undefined)
  }
  const markAllRead = () => {
    api.post('/customer/notifications/read-all').then(invalidateNotifications).catch(() => undefined)
  }
  const handleLogoutRequest = () => {
    close()
    requestLogout()
  }

  const iconButton = 'button-secondary relative !size-11 !p-0'

  return (
    <div className="contents" ref={containerRef}>
      <button
        aria-controls={notificationsId}
        aria-expanded={openMenu === 'notifications'}
        aria-haspopup="dialog"
        aria-label={t('customer.resources.notifications')}
        className={iconButton}
        onClick={(event) => toggleMenu('notifications', event.currentTarget)}
        type="button"
      >
        <Bell aria-hidden size={19} />
        {unreadCount > 0 && (
          <span aria-hidden className="absolute -end-1.5 -top-1.5 grid min-h-5 min-w-5 place-items-center rounded-full border-2 border-[var(--heritage-cream)] bg-[var(--heritage-red)] px-1 text-[10px] font-bold leading-none text-white" dir="ltr">
            {unreadCount}
          </span>
        )}
      </button>
      <button
        aria-controls={accountId}
        aria-expanded={openMenu === 'account'}
        aria-haspopup="dialog"
        aria-label={t('customer.header.account')}
        className={iconButton}
        onClick={(event) => toggleMenu('account', event.currentTarget)}
        type="button"
      >
        <UserRound aria-hidden size={19} />
      </button>

      {openMenu === 'notifications' && (
        <NotificationsPanel
          close={close}
          id={notificationsId}
          notifications={notifications}
          onMarkAllRead={markAllRead}
          onMarkOneRead={markOneRead}
          query={notificationQuery}
        />
      )}
      {openMenu === 'account' && (
        <AccountPanel actor={actor} close={close} id={accountId} loggingOut={loggingOut} logout={handleLogoutRequest} />
      )}
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
