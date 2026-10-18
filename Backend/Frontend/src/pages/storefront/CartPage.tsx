import { useState } from 'react'
import { Minus, Plus, Tag, Trash2, X } from 'lucide-react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { ImagePlaceholder } from '@/components/common/ImagePlaceholder'
import { api } from '@/lib/api/client'
import { parseApiProblem } from '@/lib/api/problemDetails'
import { mergeGuestCart } from '@/lib/cartMerge'
import { resolveLoginDestination, safeReturnPath } from '@/lib/authRouting'
import { formatMoney } from '@/lib/utils'
import { isSessionValid, useSessionStore } from '@/stores/sessionStore'
import type { Cart } from '@/types/models'
import { EmptyState, ErrorState, LoadingState } from '@/components/ui/Feedback'

export function CartPage() {
  const { t, i18n } = useTranslation()
  const location = useLocation()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [couponCode, setCouponCode] = useState('')
  const customerSession = useSessionStore((state) => state.customer)
  const guestCartToken = useSessionStore((state) => state.guestCartToken)
  const pendingGuestCartMerge = Boolean(guestCartToken && isSessionValid(customerSession))
  const mergeReturnState = (location.state as { cartMergeReturnTo?: unknown } | null)?.cartMergeReturnTo
  const requestedMergeReturn = typeof mergeReturnState === 'string'
    ? safeReturnPath(mergeReturnState)
    : undefined
  const mergeReturnTo = requestedMergeReturn
    ? resolveLoginDestination('customer', [], requestedMergeReturn)
    : undefined
  const queryKey = ['cart', i18n.language] as const
  const query = useQuery({
    queryKey,
    queryFn: () => api.get<Cart>('/cart').then((response) => response.data),
    enabled: !pendingGuestCartMerge,
  })
  const showCartError = (error: unknown, operation: 'quantity' | 'coupon' | 'removeCoupon') => {
    const problem = parseApiProblem(error)
    const description = operation === 'coupon'
      ? problem.status === 400 || problem.status === 409
        ? t('cart.couponInvalid')
        : t('cart.couponError')
      : problem.status === 409
        ? t('cart.quantityUnavailable')
        : problem.status === 404
          ? t('cart.changed')
          : operation === 'quantity' && problem.status === 400
            ? t('cart.quantityInvalid')
            : t('cart.operationError')
    toast.error(problem.title, { description })
  }
  const update = useMutation({
    mutationFn: ({ id, quantity }: { id: string; quantity: number }) =>
      quantity > 0
        ? api.put<Cart>(`/cart/items/${id}`, { quantity }).then((response) => response.data)
        : api.delete<Cart>(`/cart/items/${id}`).then((response) => response.data),
    onSuccess: (cart) => {
      queryClient.setQueryData(queryKey, cart)
      void queryClient.invalidateQueries({ queryKey: ['cart'] })
    },
    onError: (error) => showCartError(error, 'quantity'),
  })
  const applyCoupon = useMutation({
    mutationFn: () => api
      .post<Cart>('/cart/coupon', { code: couponCode.trim() })
      .then((response) => response.data),
    onSuccess: (cart) => {
      queryClient.setQueryData(queryKey, cart)
      setCouponCode('')
      toast.success(t('cart.couponApplied'))
      void queryClient.invalidateQueries({ queryKey: ['cart'] })
    },
    onError: (error) => showCartError(error, 'coupon'),
  })
  const removeCoupon = useMutation({
    mutationFn: () => api.delete<Cart>('/cart/coupon').then((response) => response.data),
    onSuccess: (cart) => {
      queryClient.setQueryData(queryKey, cart)
      toast.success(t('cart.couponRemoved'))
      void queryClient.invalidateQueries({ queryKey: ['cart'] })
    },
    onError: (error) => showCartError(error, 'removeCoupon'),
  })
  const retryMerge = useMutation({
    mutationFn: () => mergeGuestCart(guestCartToken, queryClient),
    onSuccess: () => {
      toast.success(t('cart.mergeSuccess'))
      if (mergeReturnTo) void navigate(mergeReturnTo, { replace: true })
    },
    onError: () => toast.error(t('cart.mergeError')),
  })

  if (pendingGuestCartMerge) {
    return <div className="page-shell py-12">
      <h1 className="mb-8 [overflow-wrap:anywhere] text-3xl font-extrabold sm:text-4xl">{t('cart.title')}</h1>
      <section
        aria-live="polite"
        className="card mx-auto max-w-2xl border-s-4 border-s-[var(--heritage-gold)] p-5 sm:p-8"
        role="alert"
      >
        <h2 className="[overflow-wrap:anywhere] text-xl font-extrabold text-[var(--heritage-burgundy)]">
          {t('cart.mergePendingTitle')}
        </h2>
        <p className="muted mt-3 [overflow-wrap:anywhere]">{t('cart.mergePendingDescription')}</p>
        <button
          className="button-primary mt-6 max-w-full whitespace-normal text-center"
          disabled={retryMerge.isPending}
          onClick={() => retryMerge.mutate()}
          type="button"
        >
          {retryMerge.isPending ? t('cart.merging') : t('cart.retryMerge')}
        </button>
      </section>
    </div>
  }

  if (query.isLoading) return <div className="page-shell py-12"><LoadingState /></div>
  if (query.isError) {
    return <div className="page-shell py-12">
      <ErrorState error={query.error} retry={() => void query.refetch()} />
    </div>
  }

  const cart = query.data
  const locale = i18n.language === 'ar' ? 'ar-PS-u-nu-latn' : 'en-US'
  const changingItem = update.isPending

  return <div className="page-shell py-12">
    <h1 className="mb-8 [overflow-wrap:anywhere] text-3xl font-extrabold sm:text-4xl">{t('cart.title')}</h1>
    {!cart?.items?.length
      ? <EmptyState message={t('cart.empty')} />
      : <div className="grid min-w-0 gap-8 lg:grid-cols-[minmax(0,1fr)_minmax(18rem,360px)]">
        <div className="grid min-w-0 content-start gap-4">
          {cart.items.map((item) => <article className="card flex min-w-0 flex-wrap items-center gap-4 p-4 sm:flex-nowrap" key={item.id}>
            <div aria-hidden className="size-20 shrink-0 overflow-hidden rounded-xl"><ImagePlaceholder /></div>
            <div className="min-w-0 flex-1">
              <h2 className="[overflow-wrap:anywhere] text-start font-bold" dir="auto">{item.name}</h2>
              <p className="muted [overflow-wrap:anywhere] text-start text-sm" dir="auto">{[item.size, item.color, item.sku].filter(Boolean).join(' · ')}</p>
              <strong className="mt-2 block [overflow-wrap:anywhere]" dir="ltr">{formatMoney(item.total ?? 0, cart.currency ?? 'ILS', locale)}</strong>
            </div>
            <div className="flex items-center rounded-lg border border-[var(--heritage-line-strong)] bg-white">
              <button
                aria-label={t('cart.decreaseQuantity', { name: item.name })}
                className="grid size-10 place-items-center rounded-s-lg transition-colors hover:bg-[var(--heritage-beige)] disabled:opacity-40"
                disabled={changingItem}
                onClick={() => update.mutate({ id: item.id ?? '', quantity: (item.quantity ?? 1) - 1 })}
                type="button"
              >
                <Minus size={15} />
              </button>
              <span aria-live="polite" className="min-w-8 text-center">{item.quantity}</span>
              <button
                aria-label={t('cart.increaseQuantity', { name: item.name })}
                className="grid size-10 place-items-center rounded-e-lg transition-colors hover:bg-[var(--heritage-beige)] disabled:opacity-40"
                disabled={changingItem || (item.quantity ?? 0) >= (item.availableQuantity ?? 0)}
                onClick={() => update.mutate({ id: item.id ?? '', quantity: (item.quantity ?? 0) + 1 })}
                type="button"
              >
                <Plus size={15} />
              </button>
            </div>
            <button
              aria-label={t('cart.removeItem', { name: item.name })}
              className="grid size-10 shrink-0 place-items-center rounded-lg text-[var(--state-danger-text)] transition-colors hover:bg-[var(--state-danger-surface)] disabled:opacity-40"
              disabled={changingItem}
              onClick={() => update.mutate({ id: item.id ?? '', quantity: 0 })}
              type="button"
            >
              <Trash2 size={18} />
            </button>
          </article>)}
        </div>

        <aside className="card h-fit min-w-0 p-5 sm:p-6">
          <h2 className="[overflow-wrap:anywhere] text-xl font-bold">{t('cart.summary')}</h2>
          <dl className="mt-5 grid gap-3">
            <div className="flex flex-wrap items-baseline gap-x-4 gap-y-1"><dt className="[overflow-wrap:anywhere]">{t('cart.subtotal')}</dt><dd className="ms-auto [overflow-wrap:anywhere]" dir="ltr">{formatMoney(cart.subtotal ?? 0, cart.currency ?? 'ILS', locale)}</dd></div>
            <div className="flex flex-wrap items-baseline gap-x-4 gap-y-1 text-[var(--state-success-text)]"><dt className="[overflow-wrap:anywhere]">{t('cart.discount')}</dt><dd className="ms-auto [overflow-wrap:anywhere]" dir="ltr">- {formatMoney(cart.discount ?? 0, cart.currency ?? 'ILS', locale)}</dd></div>
            <div className="mt-3 flex flex-wrap items-baseline gap-x-4 gap-y-1 border-t pt-4 text-xl font-extrabold"><dt className="[overflow-wrap:anywhere]">{t('cart.total')}</dt><dd className="ms-auto [overflow-wrap:anywhere]" dir="ltr">{formatMoney(cart.total ?? 0, cart.currency ?? 'ILS', locale)}</dd></div>
          </dl>

          {cart.couponCode
            ? <div className="mt-5 flex min-w-0 items-center justify-between gap-2 rounded-xl bg-[var(--heritage-beige)] p-3">
              <span className="flex min-w-0 flex-1 items-center gap-2 [overflow-wrap:anywhere] font-bold" dir="ltr"><Tag className="shrink-0" size={17} />{cart.couponCode}</span>
              <button
                aria-label={t('cart.removeCoupon')}
                className="shrink-0 p-1 text-[var(--state-danger-text)]"
                disabled={removeCoupon.isPending}
                onClick={() => removeCoupon.mutate()}
                type="button"
              >
                <X size={18} />
              </button>
            </div>
            : <div className="mt-5">
              <label className="text-sm font-bold" htmlFor="coupon-code">{t('cart.coupon')}</label>
              <div className="mt-2 flex min-w-0 flex-col gap-2 min-[400px]:flex-row">
                <input
                  className="field min-w-0"
                  dir="ltr"
                  id="coupon-code"
                  value={couponCode}
                  onChange={(event) => setCouponCode(event.target.value)}
                />
                <button
                  className="button-secondary w-full shrink-0 !whitespace-nowrap text-center min-[400px]:w-auto"
                  disabled={!couponCode.trim() || applyCoupon.isPending}
                  onClick={() => applyCoupon.mutate()}
                  type="button"
                >
                  {t('cart.applyCoupon')}
                </button>
              </div>
            </div>}

          <Link className="button-primary mt-6 w-full whitespace-normal text-center no-underline" to="/checkout">{t('actions.checkout')}</Link>
        </aside>
      </div>}
  </div>
}
