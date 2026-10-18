import { useEffect, useLayoutEffect, useRef, useState } from 'react'
import { Link, NavLink, Outlet, useLocation } from 'react-router-dom'
import { Menu, Search, ShoppingBag, UserRound, X } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { BrandLogo } from '@/components/common/BrandLogo'
import { CustomerHeaderMenu } from '@/components/common/CustomerHeaderMenu'
import { LanguageSwitcher } from '@/components/common/LanguageSwitcher'
import { SocialLinks } from '@/components/common/SocialLinks'
import { useStoreBranding } from '@/features/settings/storeBranding'
import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api/client'
import { isSessionValid, useSessionStore } from '@/stores/sessionStore'
import type { Cart } from '@/types/models'

export function StorefrontLayout() {
  const { t, i18n } = useTranslation()
  const { storeName } = useStoreBranding()
  const brandName = storeName || t('brand')
  const { pathname } = useLocation()
  const [open, setOpen] = useState(false)
  const isCustomerLoggedIn = useSessionStore((state) => isSessionValid(state.customer))
  const menuButtonRef = useRef<HTMLButtonElement>(null)
  const menuRef = useRef<HTMLElement>(null)
  const cart = useQuery({
    queryKey: ['cart', i18n.language],
    queryFn: () => api.get<Cart>('/cart').then((response) => response.data),
  })
  const count = cart.data?.items?.reduce((sum, item) => sum + (item.quantity ?? 0), 0) ?? 0
  const nav = [['/', t('nav.home')], ['/products', t('nav.products')], ['/categories', t('nav.categories')], ['/contact', t('nav.contact')]]

  useLayoutEffect(() => {
    if (window.scrollY > 0) window.scrollTo({ left: 0, top: 0 })
  }, [pathname])

  useEffect(() => {
    if (!open) return

    const previousOverflow = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    const focusable = () => Array.from(menuRef.current?.querySelectorAll<HTMLElement>('a[href], button:not([disabled])') ?? [])
    focusable()[0]?.focus()
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        setOpen(false)
        menuButtonRef.current?.focus()
        return
      }
      if (event.key !== 'Tab') return
      const items = focusable()
      if (!items.length) return
      const first = items[0]
      const last = items.at(-1)
      if (event.shiftKey && document.activeElement === first) {
        event.preventDefault()
        last?.focus()
      } else if (!event.shiftKey && document.activeElement === last) {
        event.preventDefault()
        first.focus()
      }
    }
    const media = window.matchMedia('(min-width: 1024px)')
    const closeForDesktop = () => {
      if (media.matches) setOpen(false)
    }
    document.addEventListener('keydown', handleKeyDown)
    media.addEventListener('change', closeForDesktop)
    return () => {
      document.body.style.overflow = previousOverflow
      document.removeEventListener('keydown', handleKeyDown)
      media.removeEventListener('change', closeForDesktop)
    }
  }, [open])

  const closeMenu = () => {
    setOpen(false)
    menuButtonRef.current?.focus()
  }

  return <div className="min-h-screen">
    <div className="tatreez-band" aria-hidden />
    <header className="sticky top-0 z-[60] border-b border-[var(--heritage-line)] bg-[var(--heritage-cream)]">
      <div className="page-shell flex min-h-20 items-center justify-between gap-3 py-2 sm:gap-4">
        <Link to="/" className="flex min-w-0 items-center gap-3 no-underline">
          <BrandLogo className="size-12 sm:size-14 lg:size-16" />
          <strong className="min-w-0 text-lg font-extrabold leading-snug text-[var(--heritage-burgundy)] [overflow-wrap:anywhere]">{brandName}</strong>
        </Link>
        <nav aria-label={t('nav.primary')} className="hidden min-w-0 flex-wrap items-center justify-center gap-x-8 gap-y-2 lg:flex">{nav.map(([to, label]) => <NavLink className={({ isActive }) => `relative py-2 text-center font-semibold leading-snug no-underline transition-colors hover:text-[var(--heritage-burgundy)] ${isActive ? 'text-[var(--heritage-burgundy)] after:absolute after:inset-x-0 after:-bottom-0.5 after:h-0.5 after:rounded-full after:bg-[var(--heritage-gold)]' : 'text-[var(--heritage-charcoal)]'}`} end={to === '/'} key={to} to={to}>{label}</NavLink>)}</nav>
        <div className="relative flex shrink-0 items-center gap-2">
          <Link aria-label={t('nav.search')} className="button-secondary !size-11 !p-0 max-sm:hidden" to="/products"><Search size={19} /></Link>
          {isCustomerLoggedIn
            ? <CustomerHeaderMenu />
            : <Link aria-label={t('nav.account')} className="button-secondary !size-11 !p-0 max-sm:hidden" to="/account"><UserRound size={19} /></Link>}
          <Link aria-label={t('nav.cart')} className="button-secondary relative !size-11 !p-0" to="/cart"><ShoppingBag size={19} />{count > 0 && <span className="absolute -end-2 -top-2 grid min-h-5 min-w-5 place-items-center rounded-full bg-[var(--heritage-red)] px-1 text-[10px] leading-none text-white" dir="ltr">{count}</span>}</Link>
          <div className="hidden sm:block"><LanguageSwitcher /></div>
          <button
            aria-controls="storefront-mobile-menu"
            aria-expanded={open}
            aria-label={open ? t('nav.closeMenu') : t('nav.openMenu')}
            className="button-secondary !size-11 !p-0 lg:hidden"
            onClick={() => setOpen((current) => !current)}
            ref={menuButtonRef}
            type="button"
          >
            {open ? <X aria-hidden /> : <Menu aria-hidden />}
          </button>
        </div>
      </div>
    </header>
    {open && <div
      aria-hidden={false}
      className="fixed inset-x-0 bottom-0 top-[88px] z-50 bg-black/45 lg:hidden"
      onMouseDown={(event) => {
        if (event.target === event.currentTarget) closeMenu()
      }}
    >
      <nav
        aria-label={t('nav.mobile')}
        className="me-auto h-full w-[min(86vw,360px)] max-w-full overflow-y-auto bg-[var(--heritage-cream)] p-5 shadow-2xl"
        id="storefront-mobile-menu"
        ref={menuRef}
      >
        <div className="grid gap-1">
          {nav.map(([to, label]) => <NavLink className={({ isActive }) => `min-w-0 rounded-xl px-4 py-3 font-semibold leading-snug [overflow-wrap:anywhere] ${isActive ? 'bg-[var(--heritage-beige)] text-[var(--heritage-burgundy)]' : 'hover:bg-white'}`} end={to === '/'} key={to} onClick={() => setOpen(false)} to={to}>{label}</NavLink>)}
          <NavLink className="min-w-0 rounded-xl px-4 py-3 font-semibold leading-snug [overflow-wrap:anywhere] hover:bg-white" onClick={() => setOpen(false)} to="/account">{t('nav.account')}</NavLink>
          <NavLink className="min-w-0 rounded-xl px-4 py-3 font-semibold leading-snug [overflow-wrap:anywhere] hover:bg-white" onClick={() => setOpen(false)} to="/cart">{t('nav.cart')}</NavLink>
        </div>
        <div className="mt-5 border-t pt-5"><LanguageSwitcher /></div>
      </nav>
    </div>}
    <main><Outlet /></main>
    <footer className="mt-20 bg-[var(--heritage-charcoal)] text-white">
      <div className="tatreez-band tatreez-band-lg" aria-hidden />
      <div className="page-shell grid gap-10 py-12 sm:grid-cols-2 lg:grid-cols-[1.6fr_1fr_1fr_1.2fr]">
        <div className="flex min-w-0 items-start gap-4 sm:col-span-2 lg:col-span-1">
          <BrandLogo className="size-20" />
          <div className="min-w-0">
            <h2 className="text-xl font-extrabold [overflow-wrap:anywhere]">{brandName}</h2>
            <p className="mt-2 text-white/70 [overflow-wrap:anywhere]">{t('tagline')}</p>
          </div>
        </div>
        <nav aria-label={t('footer.shop')} className="min-w-0">
          <h3 className="font-bold text-[var(--heritage-gold)]">{t('footer.shop')}</h3>
          <ul className="mt-3 grid gap-2">
            <li><Link className="text-white/75 transition-colors hover:text-white" to="/products">{t('nav.products')}</Link></li>
            <li><Link className="text-white/75 transition-colors hover:text-white" to="/categories">{t('nav.categories')}</Link></li>
          </ul>
        </nav>
        <nav aria-label={t('footer.help')} className="min-w-0">
          <h3 className="font-bold text-[var(--heritage-gold)]">{t('footer.help')}</h3>
          <ul className="mt-3 grid gap-2">
            <li><Link className="text-white/75 transition-colors hover:text-white" to="/contact">{t('nav.contact')}</Link></li>
            <li><Link className="text-white/75 transition-colors hover:text-white" to="/order-lookup">{t('orders.lookupTitle')}</Link></li>
          </ul>
        </nav>
        <SocialLinks className="min-w-0" />
      </div>
      <div className="border-t border-white/10">
        <p className="page-shell py-4 text-sm text-white/55">© {new Date().getFullYear()} {brandName} · {t('footer.rights')}</p>
      </div>
    </footer>
  </div>
}
