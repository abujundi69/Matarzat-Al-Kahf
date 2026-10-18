import { ChevronDown, Menu, PanelLeftClose, PanelLeftOpen, X } from 'lucide-react'
import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, NavLink, Navigate, Outlet, useLocation } from 'react-router-dom'
import { AdminTopbar } from '@/components/admin/AdminTopbar'
import { ForbiddenState } from '@/components/ui/Feedback'
import { Tooltip } from '@/components/ui/Tooltip'
import { adminNavigationGroups, getAdminLandingPath } from '@/config/adminNavigation'
import { hasAnyPermission } from '@/config/adminResources'
import { useSessionStore } from '@/stores/sessionStore'
import { BrandLogo } from '@/components/common/BrandLogo'
import { useStoreBranding } from '@/features/settings/storeBranding'

export function AdminLandingPage() {
  const permissions = useSessionStore((state) => state.admin?.permissions ?? [])
  const target = getAdminLandingPath(permissions)
  return target ? <Navigate replace to={target} /> : <ForbiddenState />
}

const focusableSelector = [
  'a[href]',
  'button:not([disabled])',
  'input:not([disabled])',
  'select:not([disabled])',
  'textarea:not([disabled])',
  '[tabindex]:not([tabindex="-1"])',
].join(',')
const noPermissions: readonly string[] = []

export function AdminLayout() {
  const { t } = useTranslation()
  const { storeName } = useStoreBranding()
  const brandName = storeName || t('brand')
  const [mobileOpen, setMobileOpen] = useState(false)
  const [desktopCollapsed, setDesktopCollapsed] = useState(
    () => localStorage.getItem('matarzat-admin-sidebar-collapsed') === 'true',
  )
  const [desktopViewport, setDesktopViewport] = useState(
    () => window.matchMedia?.('(min-width: 1024px)').matches ?? false,
  )
  const [openGroups, setOpenGroups] = useState<ReadonlySet<string>>(() => new Set())
  const location = useLocation()
  const permissions = useSessionStore((state) => state.admin?.permissions ?? noPermissions)
  const drawerRef = useRef<HTMLElement>(null)
  const mobileTriggerRef = useRef<HTMLButtonElement>(null)
  const mobileCloseRef = useRef<HTMLButtonElement>(null)

  const visibleGroups = useMemo(
    () => adminNavigationGroups
      .map((group) => ({
        ...group,
        items: group.items.filter((item) => hasAnyPermission(permissions, item.permissions)),
      }))
      .filter((group) => group.items.length > 0),
    [permissions],
  )
  const visibleNavigation = useMemo(
    () => visibleGroups.flatMap((group) => group.items),
    [visibleGroups],
  )
  const activeItem = visibleNavigation.find((item) =>
    location.pathname === item.path || location.pathname.startsWith(`${item.path}/`),
  )
  const activeGroup = visibleGroups.find((group) =>
    group.items.some((item) => item.path === activeItem?.path),
  )
  const standaloneTitleKey = location.pathname === '/admin/profile'
    ? 'profile.title'
    : location.pathname.startsWith('/admin/notifications')
      ? 'admin.resources.notifications'
      : location.pathname.startsWith('/admin/messages') || location.pathname.startsWith('/admin/contact')
        ? 'admin.resources.contact'
        : null
  const pageTitle = standaloneTitleKey
    ? t(standaloneTitleKey)
    : activeItem
      ? t(activeItem.key)
      : t('admin.navigation')
  const closeMobile = useCallback(() => setMobileOpen(false), [])

  useEffect(() => {
    if (!activeGroup?.collapsible) return
    setOpenGroups((current) => {
      if (current.has(activeGroup.key)) return current
      return new Set([activeGroup.key])
    })
  }, [activeGroup, location.pathname])

  useEffect(() => {
    localStorage.setItem('matarzat-admin-sidebar-collapsed', String(desktopCollapsed))
  }, [desktopCollapsed])

  useEffect(() => {
    document.title = `${pageTitle} | ${brandName}`
  }, [pageTitle, brandName])

  useEffect(() => {
    if (!mobileOpen) return

    const mobileTrigger = mobileTriggerRef.current
    const previousOverflow = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    const focusFrame = window.requestAnimationFrame(() => mobileCloseRef.current?.focus())

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        event.preventDefault()
        closeMobile()
        return
      }
      if (event.key !== 'Tab' || !drawerRef.current) return

      const focusable = Array.from(drawerRef.current.querySelectorAll<HTMLElement>(focusableSelector))
        .filter((element) => !element.hasAttribute('disabled'))
      if (!focusable.length) {
        event.preventDefault()
        drawerRef.current.focus()
        return
      }

      const first = focusable[0]
      const last = focusable.at(-1)
      if (event.shiftKey && document.activeElement === first) {
        event.preventDefault()
        last?.focus()
      } else if (!event.shiftKey && document.activeElement === last) {
        event.preventDefault()
        first.focus()
      }
    }

    document.addEventListener('keydown', handleKeyDown)
    return () => {
      window.cancelAnimationFrame(focusFrame)
      document.removeEventListener('keydown', handleKeyDown)
      document.body.style.overflow = previousOverflow
      mobileTrigger?.focus()
    }
  }, [closeMobile, mobileOpen])

  useEffect(() => {
    if (!window.matchMedia) return
    const desktop = window.matchMedia('(min-width: 1024px)')
    const closeAtDesktop = (event: MediaQueryListEvent) => {
      setDesktopViewport(event.matches)
      if (event.matches) setMobileOpen(false)
    }
    setDesktopViewport(desktop.matches)
    desktop.addEventListener('change', closeAtDesktop)
    return () => desktop.removeEventListener('change', closeAtDesktop)
  }, [])

  useEffect(() => {
    if (desktopViewport || !activeGroup?.collapsible) return
    setOpenGroups(new Set([activeGroup.key]))
  }, [activeGroup, desktopViewport, location.pathname])

  const sidebarWidth = desktopCollapsed ? 'lg:w-[5.5rem]' : 'lg:w-[18.5rem]'
  const gridColumns = desktopCollapsed
    ? 'lg:grid-cols-[5.5rem_minmax(0,1fr)]'
    : 'lg:grid-cols-[18.5rem_minmax(0,1fr)]'

  const toggleGroup = (key: string) => {
    if (desktopCollapsed && window.matchMedia?.('(min-width: 1024px)').matches) {
      setDesktopCollapsed(false)
      setOpenGroups(new Set([key]))
      return
    }
    setOpenGroups((current) => {
      const next = new Set(current)
      if (next.has(key)) next.delete(key)
      else return new Set([key])
      return next
    })
  }

  const toggleDesktopSidebar = () => {
    const nextCollapsed = !desktopCollapsed
    setDesktopCollapsed(nextCollapsed)
    setOpenGroups(nextCollapsed || !activeGroup?.collapsible
      ? new Set()
      : new Set([activeGroup.key]))
  }

  return (
    <div className={`min-h-screen min-h-dvh bg-[#f7f3ed] transition-[grid-template-columns] duration-300 lg:grid ${gridColumns}`}>
      {mobileOpen && (
        <button
          aria-label={t('admin.menu.close')}
          className="fixed inset-0 z-40 bg-black/50 lg:hidden"
          onClick={closeMobile}
          type="button"
        />
      )}
      <aside
        aria-label={t('admin.navigation')}
        aria-modal={mobileOpen || undefined}
        className={`admin-sidebar fixed inset-y-0 start-0 z-50 flex w-[min(18.5rem,calc(100vw-1rem))] flex-col bg-[var(--heritage-charcoal)] p-4 text-white transition-[transform,width] duration-300 lg:sticky lg:top-0 lg:h-dvh ${sidebarWidth} ${mobileOpen ? 'visible' : 'invisible lg:visible'}`}
        data-mobile-open={mobileOpen}
        ref={drawerRef}
        role={mobileOpen ? 'dialog' : undefined}
        tabIndex={-1}
      >
        <div className="mb-4 flex min-h-14 items-center justify-between gap-2 border-b border-white/10 pb-4">
          <Link
            aria-label={brandName}
            className={`flex min-w-0 items-center gap-3 text-lg font-extrabold text-white ${desktopCollapsed ? 'lg:flex-1 lg:justify-center' : ''}`}
            onClick={closeMobile}
            to="/admin"
          >
            <BrandLogo className="size-11" />
            <span className={`min-w-0 leading-snug whitespace-normal [overflow-wrap:anywhere] ${desktopCollapsed ? 'lg:sr-only' : ''}`}>{brandName}</span>
          </Link>
          <button
            aria-label={t('admin.menu.close')}
            className="grid size-11 shrink-0 place-items-center rounded-xl text-white hover:bg-white/10 lg:hidden"
            onClick={closeMobile}
            ref={mobileCloseRef}
            type="button"
          >
            <X aria-hidden size={21} />
          </button>
        </div>
        <nav className="admin-sidebar-nav min-h-0 flex-1 overflow-y-auto overflow-x-hidden pe-1" id="admin-navigation">
          <div className="grid min-w-0 gap-1">
            {visibleGroups.map((group) => {
              const GroupIcon = group.icon
              const groupLabel = t(group.key)
              const groupOpen = openGroups.has(group.key)
              const groupIsActive = group.key === activeGroup?.key
              const groupExpanded = groupOpen && !(desktopCollapsed && desktopViewport)
              return (
                <section className="min-w-0" key={group.key}>
                  {group.collapsible && (
                    <Tooltip content={groupLabel} disabled={!desktopCollapsed || !desktopViewport}>
                      {(tooltipProps) => (
                        <button
                          {...tooltipProps}
                          aria-controls={`admin-group-${group.key.replace(/\W/g, '-')}`}
                          aria-expanded={groupExpanded}
                          aria-label={groupLabel}
                          className={`admin-nav-item relative flex min-h-11 w-full min-w-0 items-center gap-3 rounded-xl px-3 py-2 text-start text-sm font-bold hover:bg-white/10 ${
                            desktopCollapsed ? 'lg:justify-center lg:px-2' : ''
                          } ${groupIsActive ? 'bg-white/10 text-white' : 'text-white/85'}`}
                          onClick={() => toggleGroup(group.key)}
                          type="button"
                        >
                          <GroupIcon aria-hidden className="shrink-0 text-[var(--heritage-sand)]" size={19} />
                          <span className={`min-w-0 flex-1 leading-snug whitespace-normal [overflow-wrap:anywhere] ${desktopCollapsed ? 'lg:sr-only' : ''}`}>{groupLabel}</span>
                          <ChevronDown
                            aria-hidden
                            className={`shrink-0 transition-transform ${groupExpanded ? 'rotate-180' : ''} ${desktopCollapsed ? 'lg:hidden' : ''}`}
                            size={17}
                          />
                        </button>
                      )}
                    </Tooltip>
                  )}
                  {(!group.collapsible || groupOpen) && (
                    <div
                      className={`grid min-w-0 gap-1 ${group.collapsible ? 'mt-1 border-s border-white/10 ps-2' : ''} ${desktopCollapsed && group.collapsible ? 'lg:hidden' : ''}`}
                      id={group.collapsible ? `admin-group-${group.key.replace(/\W/g, '-')}` : undefined}
                    >
                      {group.items.map((item) => {
                        const Icon = item.icon
                        const label = t(item.key)
                        return (
                          <Tooltip content={label} disabled={!desktopCollapsed || !desktopViewport} key={item.path}>
                            {(tooltipProps) => (
                              <NavLink
                                {...tooltipProps}
                                aria-label={label}
                                className={({ isActive }) =>
                                  `admin-nav-item relative flex min-h-11 min-w-0 items-center gap-3 rounded-xl px-3 py-2 text-sm font-semibold ${
                                    desktopCollapsed ? 'lg:justify-center lg:px-2' : ''
                                  } ${isActive ? 'bg-[var(--heritage-burgundy)] text-white before:absolute before:inset-y-2 before:start-0 before:w-1 before:rounded-full before:bg-[var(--heritage-gold)]' : 'text-white/70 hover:bg-white/10 hover:text-white'}`
                                }
                                end={item.path === '/admin/dashboard'}
                                onClick={closeMobile}
                                to={item.path}
                              >
                                <Icon aria-hidden className="shrink-0" size={19} />
                                <span className={`min-w-0 flex-1 leading-snug whitespace-normal [overflow-wrap:anywhere] ${desktopCollapsed ? 'lg:sr-only' : ''}`}>{label}</span>
                              </NavLink>
                            )}
                          </Tooltip>
                        )
                      })}
                    </div>
                  )}
                </section>
              )
            })}
          </div>
        </nav>
      </aside>
      <div className="min-w-0">
        <header className="sticky top-0 z-30 flex min-h-18 items-center justify-between gap-1.5 border-b bg-white/90 px-3 py-2 backdrop-blur sm:gap-3 sm:px-7">
          <div className="flex min-w-0 flex-1 items-center gap-2 sm:gap-3">
            <button
              aria-controls="admin-navigation"
              aria-expanded={mobileOpen}
              aria-label={t('admin.menu.open')}
              className="button-secondary !size-11 !p-0 lg:hidden"
              onClick={() => setMobileOpen(true)}
              ref={mobileTriggerRef}
              type="button"
            >
              <Menu aria-hidden size={20} />
            </button>
            <button
              aria-controls="admin-navigation"
              aria-expanded={!desktopCollapsed}
              aria-label={desktopCollapsed ? t('admin.menu.expand') : t('admin.menu.collapse')}
              className="admin-sidebar-toggle button-secondary hidden !size-11 !p-0 lg:inline-flex"
              onClick={toggleDesktopSidebar}
              type="button"
            >
              {desktopCollapsed ? <PanelLeftOpen aria-hidden size={20} /> : <PanelLeftClose aria-hidden size={20} />}
            </button>
            <div className="hidden min-w-0 items-center gap-2 min-[540px]:flex" aria-label={t('admin.breadcrumb')}>
              {activeGroup && (
                <span className="muted hidden shrink-0 text-xs font-semibold sm:inline">
                  {t(activeGroup.key)}
                  <span aria-hidden className="mx-2">/</span>
                </span>
              )}
              <h1 className="min-w-0 font-bold leading-snug text-[var(--heritage-burgundy)] [overflow-wrap:anywhere]">
                {pageTitle}
              </h1>
            </div>
          </div>
          <AdminTopbar />
        </header>
        <main className="min-w-0 p-4 sm:p-7"><Outlet /></main>
      </div>
    </div>
  )
}
