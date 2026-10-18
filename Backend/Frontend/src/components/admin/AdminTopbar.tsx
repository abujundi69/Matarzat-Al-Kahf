import { Bell, ChevronDown, LogOut, Mail, RefreshCw, UserRound } from 'lucide-react'
import { useQuery } from '@tanstack/react-query'
import { useEffect, useId, useMemo, useRef, useState, type ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useLocation } from 'react-router-dom'
import { ConfirmDialog } from '@/components/common/ConfirmDialog'
import { LanguageSwitcher } from '@/components/common/LanguageSwitcher'
import { adminPermissions } from '@/config/adminResources'
import { useAdminLogout } from '@/lib/adminAuth'
import { api } from '@/lib/api/client'
import { formatRelativeTime } from '@/lib/utils'
import { useSessionStore } from '@/stores/sessionStore'
import type { Schema } from '@/types/models'

type OpenMenu = 'notifications' | 'messages' | 'profile' | null
type NotificationPage = Schema<'NotificationResponsePagedResult'>
type Notification = Schema<'NotificationResponse'>
type MessagePage = Schema<'ContactMessageResponsePagedResult'>
type Message = Schema<'ContactMessageResponse'>
type AdminProfile = Schema<'AdminProfileDto'>

const latestItemCount = 5
const noPermissions: readonly string[] = []

const initialsFor = (value: string | null | undefined, fallback: string) => {
  const words = value?.trim().split(/\s+/).filter(Boolean) ?? []
  if (!words.length) return fallback
  return words.slice(0, 2).map((word) => Array.from(word)[0]).join('').toUpperCase()
}

function DropdownPanel({
  children,
  id,
  title,
}: {
  children: ReactNode
  id: string
  title: string
}) {
  return (
    <section
      aria-label={title}
      className="absolute end-0 top-[calc(100%+.65rem)] z-50 flex max-h-[min(38rem,calc(100dvh-5.5rem))] w-[min(23rem,calc(100vw-1.25rem))] flex-col overflow-hidden rounded-2xl border border-stone-200 bg-white text-[var(--heritage-charcoal)] shadow-[0_20px_55px_rgb(41_36_33_/.2)]"
      id={id}
      role="dialog"
    >
      {children}
    </section>
  )
}

function PanelHeader({ title }: { title: string }) {
  return (
    <header className="flex shrink-0 items-center justify-between gap-3 border-b border-stone-200 px-4 py-3.5">
      <h2 className="text-base font-extrabold">{title}</h2>
    </header>
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
    <div className="grid min-h-32 place-items-center gap-3 px-5 py-8 text-center">
      <p className="muted text-sm">{message}</p>
      {action && actionLabel && (
        <button className="button-secondary !min-h-10 !px-3 text-sm" onClick={action} type="button">
          <RefreshCw aria-hidden size={16} />
          {actionLabel}
        </button>
      )}
    </div>
  )
}

function PanelFooter({ children, onClick, to }: { children: ReactNode; onClick: () => void; to: string }) {
  return (
    <footer className="shrink-0 border-t border-stone-200 bg-stone-50 p-2.5">
      <Link
        className="flex min-h-11 w-full items-center justify-center rounded-xl px-3 text-center text-sm font-extrabold text-[var(--heritage-burgundy)] transition hover:bg-[var(--heritage-beige)]"
        onClick={onClick}
        to={to}
      >
        {children}
      </Link>
    </footer>
  )
}

function UnreadMarker({ label }: { label: string }) {
  return (
    <span className="mt-1.5 size-2.5 shrink-0 rounded-full bg-[var(--heritage-red)] shadow-[0_0_0_3px_rgb(159_39_55_/.12)]">
      <span className="sr-only">{label}</span>
    </span>
  )
}

function NotificationsPanel({
  canRead,
  close,
  id,
  notifications,
  query,
}: {
  canRead: boolean
  close: () => void
  id: string
  notifications: Notification[]
  query: ReturnType<typeof useNotificationQuery>
}) {
  const { t, i18n } = useTranslation()
  const locale = i18n.language.startsWith('ar') ? 'ar-SA' : 'en-US'

  return (
    <DropdownPanel id={id} title={t('admin.topbar.notifications.title')}>
      <PanelHeader title={t('admin.topbar.notifications.title')} />
      <div className="min-h-0 overflow-y-auto overscroll-contain">
        {!canRead
          ? <PanelState message={t('admin.topbar.noAccess')} />
          : query.isLoading
            ? <PanelState message={t('states.loading')} />
            : query.isError
              ? <PanelState action={() => void query.refetch()} actionLabel={t('actions.retry')} message={t('admin.topbar.notifications.loadError')} />
              : !notifications.length
                ? <PanelState message={t('admin.topbar.notifications.empty')} />
                : (
                  <ul className="divide-y divide-stone-100">
                    {notifications.map((notification, index) => {
                      const unread = !notification.readAtUtc
                      return (
                        <li key={notification.id ?? index}>
                          <Link
                            className={`grid min-w-0 grid-cols-[auto_minmax(0,1fr)] gap-3 px-4 py-3.5 transition hover:bg-stone-50 ${unread ? 'bg-[var(--heritage-cream)]' : 'bg-white'}`}
                            onClick={close}
                            to={notification.id ? `/admin/notifications?open=${encodeURIComponent(notification.id)}` : '/admin/notifications'}
                          >
                            {unread
                              ? <UnreadMarker label={t('admin.topbar.notifications.unread')} />
                              : <span aria-hidden className="mt-1.5 size-2.5 shrink-0" />}
                            <span className="min-w-0">
                              <span className="clip-safe block truncate text-sm font-extrabold">
                                {notification.title?.trim() || t('admin.topbar.notifications.untitled')}
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
      {canRead && (
        <PanelFooter onClick={close} to="/admin/notifications">
          {t('admin.topbar.notifications.viewAll')}
        </PanelFooter>
      )}
    </DropdownPanel>
  )
}

function MessagesPanel({
  canRead,
  close,
  id,
  messages,
  query,
}: {
  canRead: boolean
  close: () => void
  id: string
  messages: Message[]
  query: ReturnType<typeof useMessageQuery>
}) {
  const { t, i18n } = useTranslation()
  const locale = i18n.language.startsWith('ar') ? 'ar-SA' : 'en-US'

  return (
    <DropdownPanel id={id} title={t('admin.topbar.messages.title')}>
      <PanelHeader title={t('admin.topbar.messages.title')} />
      <div className="min-h-0 overflow-y-auto overscroll-contain">
        {!canRead
          ? <PanelState message={t('admin.topbar.noAccess')} />
          : query.isLoading
            ? <PanelState message={t('states.loading')} />
            : query.isError
              ? <PanelState action={() => void query.refetch()} actionLabel={t('actions.retry')} message={t('admin.topbar.messages.loadError')} />
              : !messages.length
                ? <PanelState message={t('admin.topbar.messages.empty')} />
                : (
                  <ul className="divide-y divide-stone-100">
                    {messages.map((message, index) => {
                      const unread = message.status === 'New'
                      const sender = message.name?.trim() || t('admin.topbar.messages.unknownSender')
                      return (
                        <li key={message.id ?? index}>
                          <Link
                            className={`grid min-w-0 grid-cols-[auto_minmax(0,1fr)_auto] gap-3 px-4 py-3.5 transition hover:bg-stone-50 ${unread ? 'bg-[var(--heritage-cream)]' : 'bg-white'}`}
                            onClick={close}
                            to={message.id ? `/admin/messages?open=${encodeURIComponent(message.id)}` : '/admin/messages'}
                          >
                            <span className="grid size-10 shrink-0 place-items-center rounded-full bg-[var(--heritage-beige)] text-sm font-extrabold text-[var(--heritage-burgundy)]" aria-hidden>
                              {initialsFor(sender, '•')}
                            </span>
                            <span className="min-w-0">
                              <span className="clip-safe block truncate text-sm font-extrabold">{sender}</span>
                              {message.subject?.trim() && <span className="mt-0.5 clip-safe block truncate text-xs font-bold">{message.subject}</span>}
                              {message.message?.trim() && <span className="muted mt-0.5 clip-safe block truncate text-xs">{message.message.trim()}</span>}
                              <time className="mt-1.5 block text-xs font-bold text-[var(--heritage-burgundy)]" dateTime={message.createdAtUtc}>
                                {formatRelativeTime(message.createdAtUtc, locale)}
                              </time>
                            </span>
                            {unread
                              ? <UnreadMarker label={t('admin.topbar.messages.unread')} />
                              : <span aria-hidden className="mt-1.5 size-2.5 shrink-0" />}
                          </Link>
                        </li>
                      )
                    })}
                  </ul>
                )}
      </div>
      {canRead && (
        <PanelFooter onClick={close} to="/admin/messages">
          {t('admin.topbar.messages.viewAll')}
        </PanelFooter>
      )}
    </DropdownPanel>
  )
}

function ProfilePanel({
  close,
  id,
  loggingOut,
  logout,
  profile,
}: {
  close: () => void
  id: string
  loggingOut: boolean
  logout: () => void
  profile: AdminProfile | undefined
}) {
  const { t } = useTranslation()
  const displayName = profile?.fullName?.trim() || t('admin.topbar.profile.administrator')

  return (
    <DropdownPanel id={id} title={t('admin.topbar.profile.label')}>
      <div className="flex min-w-0 items-center gap-3 border-b border-stone-200 p-4">
        <span className="grid size-11 shrink-0 place-items-center rounded-full bg-[var(--heritage-burgundy)] font-extrabold text-white" aria-hidden>
          {initialsFor(displayName, t('admin.topbar.profile.initial'))}
        </span>
        <span className="min-w-0">
          <strong className="clip-safe block truncate text-sm">{displayName}</strong>
          {profile?.email && <bdi className="muted mt-0.5 clip-safe block truncate text-xs" dir="ltr">{profile.email}</bdi>}
        </span>
      </div>
      <nav aria-label={t('admin.topbar.profile.label')} className="grid gap-1 p-2">
        <Link
          className="flex min-h-11 items-center gap-3 rounded-xl px-3 text-sm font-bold transition hover:bg-stone-50"
          onClick={close}
          to="/admin/profile"
        >
          <UserRound aria-hidden size={18} />
          {t('admin.topbar.profile.myProfile')}
        </Link>
        <button
          className="flex min-h-11 w-full items-center gap-3 rounded-xl px-3 text-start text-sm font-bold text-[var(--heritage-red)] transition hover:bg-[var(--state-danger-surface)]"
          disabled={loggingOut}
          onClick={logout}
          type="button"
        >
          <LogOut aria-hidden size={18} />
          {t('actions.logout')}
        </button>
      </nav>
    </DropdownPanel>
  )
}

const useNotificationQuery = (enabled: boolean, language: string) => useQuery({
  enabled,
  queryKey: ['admin-topbar', 'notifications', language],
  queryFn: () => api.get<NotificationPage>('/admin/notifications', {
    params: { page: 1, pageSize: latestItemCount },
  }).then((response) => response.data),
  refetchInterval: 60_000,
  staleTime: 30_000,
})

const useMessageQuery = (enabled: boolean, language: string) => useQuery({
  enabled,
  queryKey: ['admin-topbar', 'messages', language],
  queryFn: () => api.get<MessagePage>('/admin/contact-messages', {
    params: { page: 1, pageSize: latestItemCount },
  }).then((response) => response.data),
  refetchInterval: 60_000,
  staleTime: 30_000,
})

export function AdminTopbar() {
  const { t, i18n } = useTranslation()
  const [openMenu, setOpenMenu] = useState<OpenMenu>(null)
  const location = useLocation()
  const permissions = useSessionStore((state) => state.admin?.permissions ?? noPermissions)
  const { cancelLogout, confirmLogout, confirmOpen, loggingOut, requestLogout } = useAdminLogout('/login')
  const containerRef = useRef<HTMLDivElement>(null)
  const activeTriggerRef = useRef<HTMLButtonElement | null>(null)
  const notificationsId = useId()
  const messagesId = useId()
  const profileId = useId()
  const canReadNotifications = permissions.includes(adminPermissions.notificationsRead)
  const canReadMessages = permissions.includes(adminPermissions.contactRead)
  const notificationQuery = useNotificationQuery(canReadNotifications, i18n.language)
  const messageQuery = useMessageQuery(canReadMessages, i18n.language)
  const profileQuery = useQuery({
    queryKey: ['admin', 'profile'],
    queryFn: () => api.get<AdminProfile>('/admin/profile').then((response) => response.data),
    staleTime: 60_000,
  })
  const notifications = useMemo(() => notificationQuery.data?.items ?? [], [notificationQuery.data?.items])
  const messages = useMemo(() => messageQuery.data?.items ?? [], [messageQuery.data?.items])
  const hasUnreadNotifications = notifications.some((notification) => !notification.readAtUtc)
  const hasUnreadMessages = messages.some((message) => message.status === 'New')

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

  const handleLogoutRequest = () => {
    setOpenMenu(null)
    requestLogout()
  }

  const iconButton = 'relative grid size-11 shrink-0 place-items-center rounded-xl border border-stone-200 bg-white text-[var(--heritage-burgundy)] shadow-sm transition hover:border-[var(--heritage-gold)] hover:bg-[var(--heritage-cream)]'
  const close = () => setOpenMenu(null)

  return (
    <div className="relative flex shrink-0 items-center gap-1.5 sm:gap-2" ref={containerRef}>
      <LanguageSwitcher compact />
      <button
        aria-controls={notificationsId}
        aria-expanded={openMenu === 'notifications'}
        aria-haspopup="dialog"
        aria-label={t('admin.topbar.notifications.label')}
        className={iconButton}
        onClick={(event) => toggleMenu('notifications', event.currentTarget)}
        type="button"
      >
        <Bell aria-hidden size={20} />
        {hasUnreadNotifications && <span aria-hidden className="absolute end-1.5 top-1.5 size-2.5 rounded-full border-2 border-white bg-[var(--heritage-red)]" />}
      </button>
      <button
        aria-controls={messagesId}
        aria-expanded={openMenu === 'messages'}
        aria-haspopup="dialog"
        aria-label={t('admin.topbar.messages.label')}
        className={iconButton}
        onClick={(event) => toggleMenu('messages', event.currentTarget)}
        type="button"
      >
        <Mail aria-hidden size={20} />
        {hasUnreadMessages && <span aria-hidden className="absolute end-1.5 top-1.5 size-2.5 rounded-full border-2 border-white bg-[var(--heritage-red)]" />}
      </button>
      <button
        aria-controls={profileId}
        aria-expanded={openMenu === 'profile'}
        aria-haspopup="dialog"
        aria-label={t('admin.topbar.profile.label')}
        className={`${iconButton} !flex gap-1.5 px-1.5 sm:w-auto sm:min-w-11`}
        onClick={(event) => toggleMenu('profile', event.currentTarget)}
        type="button"
      >
        <span className="grid size-8 place-items-center rounded-full bg-[var(--heritage-burgundy)] text-xs font-extrabold text-white" aria-hidden>
          {initialsFor(profileQuery.data?.fullName, t('admin.topbar.profile.initial'))}
        </span>
        <ChevronDown aria-hidden className="hidden sm:block" size={15} />
      </button>

      {openMenu === 'notifications' && (
        <NotificationsPanel canRead={canReadNotifications} close={close} id={notificationsId} notifications={notifications} query={notificationQuery} />
      )}
      {openMenu === 'messages' && (
        <MessagesPanel canRead={canReadMessages} close={close} id={messagesId} messages={messages} query={messageQuery} />
      )}
      {openMenu === 'profile' && (
        <ProfilePanel close={close} id={profileId} loggingOut={loggingOut} logout={handleLogoutRequest} profile={profileQuery.data} />
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
