import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { MapPin, PackageCheck } from 'lucide-react'
import { useForm } from 'react-hook-form'
import { Link, Navigate, useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useEffect, useMemo, useRef, useState } from 'react'
import { z } from 'zod'
import { toast } from 'sonner'
import { api } from '@/lib/api/client'
import { createIdempotencyKey, formatMoney } from '@/lib/utils'
import { parseApiProblem } from '@/lib/api/problemDetails'
import { isSessionValid, useSessionStore } from '@/stores/sessionStore'
import { EmptyState, ErrorState, LoadingState } from '@/components/ui/Feedback'
import type { Cart, Schema } from '@/types/models'

const schema = z.object({
  customerName: z.string().trim().min(2),
  phoneNumber: z.string().trim().min(8),
  deliveryLocationId: z.string().uuid(),
  area: z.string().trim().min(2),
  addressLine: z.string().trim().min(5),
  taxNumber: z.string().trim().optional(),
  deliveryNotes: z.string().trim().optional(),
  paymentMethod: z.enum(['CreditCard', 'BankTransfer', 'CashOnDelivery', 'ElectronicWallet']),
  paymentReference: z.string().trim().max(250).optional(),
})
type Values = z.infer<typeof schema>
type Submission = { addressId: string; values: Values; cartSignature: string }
type Address = Schema<'CustomerAddressResponse'>
type PlaceOrderResult = Schema<'PlaceOrderResponse'> & {
  paymentId?: string
  paymentMethod?: Values['paymentMethod']
  paymentInstructions?: string | null
  paymentRedirectUrl?: string | null
}

const draftKey = 'matarzat-checkout-draft'
const draftSchema = z.object({
  ownerId: z.string().min(1),
  values: z.object({
    customerName: z.string(),
    phoneNumber: z.string(),
    deliveryLocationId: z.string(),
    area: z.string(),
    addressLine: z.string(),
    taxNumber: z.string(),
    deliveryNotes: z.string(),
    paymentMethod: z.enum(['CreditCard', 'BankTransfer', 'CashOnDelivery', 'ElectronicWallet']),
    paymentReference: z.string(),
  }).partial(),
  selectedAddressId: z.string(),
  idempotencyKey: z.string().min(8).max(200),
  reviewRequested: z.boolean(),
})
type CheckoutDraft = z.infer<typeof draftSchema>

const loadDraft = (): CheckoutDraft | undefined => {
  try {
    const raw = sessionStorage.getItem(draftKey)
    if (!raw) return undefined
    const parsed = draftSchema.safeParse(JSON.parse(raw))
    return parsed.success ? parsed.data : undefined
  } catch {
    return undefined
  }
}

const fieldForApiError = (field: string): keyof Values | undefined => {
  const normalized = field.toLowerCase()
  return (Object.keys(schema.shape) as (keyof Values)[])
    .find((candidate) => normalized.includes(candidate.toLowerCase()))
}

const addressLabel = (address: Address) =>
  [address.city, address.area, address.addressLine].filter(Boolean).join('، ')

const textDirection = (value: string): 'ltr' | 'rtl' => {
  const leftToRight = value.search(/[A-Za-z]/)
  const rightToLeft = value.search(/[\u0590-\u08ff]/)
  return rightToLeft >= 0 && (leftToRight < 0 || rightToLeft < leftToRight) ? 'rtl' : 'ltr'
}

export function CheckoutPage() {
  const customerSession = useSessionStore((state) => state.customer)
  const guestCartToken = useSessionStore((state) => state.guestCartToken)
  return guestCartToken && isSessionValid(customerSession)
    ? <Navigate replace to="/cart" />
    : <CheckoutFlow />
}

function CheckoutFlow() {
  const { t, i18n } = useTranslation()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const sessionActor = useSessionStore((state) => state.customer?.actor)
  const storedDraft = useRef(loadDraft()).current
  const initialDraft = storedDraft?.ownerId === sessionActor?.id ? storedDraft : undefined
  const setGuestCartToken = useSessionStore((state) => state.setGuestCartToken)
  const [preview, setPreview] = useState<Schema<'CheckoutPreviewResponse'> | null>(null)
  const [previewSignature, setPreviewSignature] = useState('')
  const [selectedAddressId, setSelectedAddressId] = useState(initialDraft?.selectedAddressId ?? '')
  const idempotencyKey = useRef(initialDraft?.idempotencyKey ?? createIdempotencyKey())
  const placementLocked = useRef(false)
  const didPrefill = useRef(Boolean(initialDraft))
  const restoreReview = useRef(Boolean(initialDraft?.reviewRequested))
  const draftCleared = useRef(false)
  const language = i18n.language.startsWith('ar') ? 'Arabic' : 'English'
  const locale = i18n.language.startsWith('ar') ? 'ar-PS-u-nu-latn' : 'en-US'

  const cart = useQuery({
    queryKey: ['cart', i18n.language],
    queryFn: () => api.get<Cart>('/cart').then((response) => response.data),
  })
  const locations = useQuery({
    queryKey: ['delivery-locations', i18n.language],
    queryFn: () => api
      .get<Schema<'DeliveryLocationResponse'>[]>('/delivery/locations')
      .then((response) => response.data),
  })
  const paymentMethods = useQuery({
    queryKey: ['payment-methods', i18n.language],
    queryFn: () => api
      .get<Schema<'PaymentMethodResponse'>[]>('/payment-methods')
      .then((response) => response.data),
  })
  const profile = useQuery({
    queryKey: ['customer', 'profile'],
    queryFn: () => api
      .get<Schema<'CustomerProfileResponse'>>('/customer/profile')
      .then((response) => response.data),
  })
  const addresses = useQuery({
    queryKey: ['customer', 'addresses'],
    queryFn: () => api
      .get<Address[]>('/customer/addresses')
      .then((response) => response.data),
  })
  const selectedAddress = addresses.data?.find((address) => address.id === selectedAddressId)
  const form = useForm<Values>({
    resolver: zodResolver(schema),
    defaultValues: {
      customerName: sessionActor?.fullName ?? '',
      phoneNumber: sessionActor?.phoneNumber ?? '',
      deliveryLocationId: '',
      area: '',
      addressLine: '',
      taxNumber: '',
      deliveryNotes: '',
      paymentMethod: 'CashOnDelivery',
      paymentReference: '',
      ...initialDraft?.values,
    },
  })
  const values = form.watch()
  const enabledMethods = useMemo(
    () => paymentMethods.data?.filter((item) => item.isEnabled && item.method && item.method !== 'ElectronicPayment') ?? [],
    [paymentMethods.data],
  )
  const selectedLocation = locations.data?.find((location) => location.id === values.deliveryLocationId)
  const estimatedDelivery = selectedLocation
    ? selectedLocation.additionalDeliveryCost ?? 0
    : null
  const estimatedTotal = estimatedDelivery != null && cart.data
    ? (cart.data.subtotal ?? 0) - (cart.data.discount ?? 0) + estimatedDelivery
    : null
  const availableMethods = useMemo(
    () => enabledMethods.filter(
      (method) => method.method !== 'CashOnDelivery' || selectedLocation?.cashOnDeliveryAllowed !== false,
    ),
    [enabledMethods, selectedLocation?.cashOnDeliveryAllowed],
  )
  const selectedMethod = availableMethods.find((item) => item.method === values.paymentMethod)
  const isManualPayment = values.paymentMethod === 'BankTransfer' || values.paymentMethod === 'ElectronicWallet'
  const cartSignature = JSON.stringify({
    subtotal: cart.data?.subtotal,
    discount: cart.data?.discount,
    items: cart.data?.items,
  })
  const currentSignature = JSON.stringify({ address: selectedAddressId, values, cartSignature })
  const currentPreview = preview && previewSignature === currentSignature ? preview : null

  useEffect(() => {
    if (didPrefill.current || profile.isLoading || addresses.isLoading) return
    if (form.formState.isDirty) {
      didPrefill.current = true
      return
    }
    didPrefill.current = true
    const savedAddress = addresses.data?.find((address) => address.isDefault) ?? addresses.data?.[0]
    form.setValue('customerName', profile.data?.fullName ?? sessionActor?.fullName ?? '')
    form.setValue('phoneNumber', profile.data?.phoneNumber ?? sessionActor?.phoneNumber ?? '')
    if (!savedAddress?.id) return
    setSelectedAddressId(savedAddress.id)
    form.setValue('deliveryLocationId', savedAddress.deliveryLocationId ?? '')
    form.setValue('area', savedAddress.area ?? savedAddress.city ?? '')
    form.setValue('addressLine', savedAddress.addressLine ?? '')
    form.setValue('taxNumber', savedAddress.taxNumber ?? '')
    form.setValue('deliveryNotes', savedAddress.deliveryNotes ?? '')
  }, [addresses.data, addresses.isLoading, form, form.formState.isDirty, profile.data, profile.isLoading, sessionActor])

  useEffect(() => {
    if (
      availableMethods.length > 0
      && !availableMethods.some((method) => method.method === values.paymentMethod)
    ) {
      form.setValue(
        'paymentMethod',
        availableMethods[0].method as Values['paymentMethod'],
        { shouldValidate: true },
      )
    }
  }, [availableMethods, form, values.paymentMethod])

  const selectAddress = (id: string) => {
    setSelectedAddressId(id)
    const address = addresses.data?.find((item) => item.id === id)
    if (!address) return
    form.setValue('deliveryLocationId', address.deliveryLocationId ?? '', { shouldValidate: true })
    form.setValue('area', address.area ?? address.city ?? '', { shouldValidate: true })
    form.setValue('addressLine', address.addressLine ?? '', { shouldValidate: true })
    form.setValue('taxNumber', address.taxNumber ?? '')
    form.setValue('deliveryNotes', address.deliveryNotes ?? '')
  }

  const requestBody = (checkoutValues: Values, addressId: string) => ({
    ...checkoutValues,
    paymentReference: checkoutValues.paymentReference?.trim() || null,
    customerAddressId: addressId || null,
    guestCartToken: null,
    guestPhoneVerificationToken: null,
    language,
  })

  const showApiError = (error: unknown) => {
    const problem = parseApiProblem(error)
    for (const [field, messages] of Object.entries(problem.errors)) {
      const formField = fieldForApiError(field)
      if (formField) form.setError(formField, { type: 'server', message: messages[0] })
    }
    const diagnostic = `${problem.technicalTitle ?? ''} ${problem.detail ?? ''}`
    const key = /stock|inventory/i.test(diagnostic)
      ? 'checkout.errors.outOfStock'
      : /idempot|already.*process|duplicate/i.test(diagnostic)
        ? 'checkout.errors.duplicate'
        : problem.status === 400
          ? 'checkout.errors.invalidData'
          : problem.status === 503
            ? 'checkout.errors.unavailable'
            : undefined
    toast.error(key ? t(key) : problem.title)
  }
  const validationMessage = (field: keyof Values, fallback: string) => {
    const error = form.formState.errors[field]
    return error?.type === 'server' ? t('checkout.errors.invalidField') : t(fallback)
  }

  const previewMutation = useMutation({
    mutationFn: (submission: Submission) => api
      .post<Schema<'CheckoutPreviewResponse'>>('/checkout/preview', requestBody(submission.values, submission.addressId))
      .then((response) => response.data),
    onSuccess: (data, submission) => {
      setPreview(data)
      setPreviewSignature(JSON.stringify({ address: submission.addressId, values: submission.values, cartSignature: submission.cartSignature }))
      restoreReview.current = false
    },
    onError: showApiError,
  })
  const place = useMutation({
    mutationFn: (submission: Submission) => api
      .post<PlaceOrderResult>(
        '/checkout/orders',
        requestBody(submission.values, submission.addressId),
        { headers: { 'Idempotency-Key': idempotencyKey.current } },
      )
      .then((response) => response.data),
    onSuccess: async (result) => {
      if (!result.orderId || !result.orderNumber) {
        placementLocked.current = false
        toast.error(t('errors.invalidResponse'))
        return
      }

      setGuestCartToken('')
      draftCleared.current = true
      sessionStorage.removeItem(draftKey)
      queryClient.setQueriesData<Cart>({ queryKey: ['cart'] }, (existing) => existing
        ? { ...existing, couponCode: null, discount: 0, items: [], subtotal: 0, total: 0 }
        : existing)
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['cart'] }),
        queryClient.invalidateQueries({ queryKey: ['customer', 'orders'] }),
      ])

      toast.success(result.isDuplicate
        ? t('checkout.duplicateRecovered', { orderNumber: result.orderNumber })
        : t('checkout.orderPlaced', { orderNumber: result.orderNumber }))

      if (result.paymentRedirectUrl) {
        try {
          const redirect = new URL(result.paymentRedirectUrl, window.location.origin)
          if (!['http:', 'https:'].includes(redirect.protocol)) throw new Error('Unsupported payment redirect protocol')
          window.location.assign(redirect.href)
          return
        } catch (error) {
          if (import.meta.env.DEV) console.error('[Invalid payment redirect URL]', error)
        }
      }

      void navigate(`/order-success/${result.orderId}`, {
        replace: true,
        state: { order: result },
      })
    },
    onError: (error) => {
      placementLocked.current = false
      showApiError(error)
    },
  })

  const submit = (checkoutValues: Values) => {
    if (!availableMethods.some((method) => method.method === checkoutValues.paymentMethod)) {
      form.setError('paymentMethod', { type: 'server', message: t('checkout.noPaymentMethods') })
      return
    }
    const submission = { addressId: selectedAddressId, values: checkoutValues, cartSignature }
    if (currentPreview) {
      if (placementLocked.current || place.isPending) return
      placementLocked.current = true
      place.mutate(submission)
      return
    }
    if (!previewMutation.isPending) previewMutation.mutate(submission)
  }

  useEffect(() => {
    if (storedDraft && !initialDraft) sessionStorage.removeItem(draftKey)
  }, [initialDraft, storedDraft])

  useEffect(() => {
    if (draftCleared.current) return
    if (!sessionActor?.id) return
    sessionStorage.setItem(draftKey, JSON.stringify({
      ownerId: sessionActor.id,
      values,
      selectedAddressId,
      idempotencyKey: idempotencyKey.current,
      reviewRequested: Boolean(currentPreview) || previewMutation.isPending || restoreReview.current,
    } satisfies CheckoutDraft))
  }, [currentPreview, currentSignature, previewMutation.isPending, selectedAddressId, sessionActor?.id, values])

  useEffect(() => {
    if (
      !restoreReview.current
      || cart.isLoading
      || locations.isLoading
      || paymentMethods.isLoading
      || previewMutation.isPending
    ) return
    restoreReview.current = false
    void form.trigger().then((valid) => {
      if (!valid) return
      previewMutation.mutate({ addressId: selectedAddressId, values: form.getValues(), cartSignature })
    })
  }, [cart.isLoading, cartSignature, form, locations.isLoading, paymentMethods.isLoading, previewMutation, selectedAddressId])

  const blockingQueries = [cart, locations, paymentMethods]
  if (blockingQueries.some((query) => query.isLoading)) {
    return <div className="page-shell py-12"><LoadingState /></div>
  }
  const failedQuery = blockingQueries.find((query) => query.isError)
  if (failedQuery) {
    return <div className="page-shell py-12">
      <ErrorState
        error={failedQuery.error}
        retry={() => {
          void cart.refetch()
          void locations.refetch()
          void paymentMethods.refetch()
        }}
      />
    </div>
  }
  if (!cart.data?.items?.length) {
    return <div className="page-shell py-12">
      <EmptyState message={t('checkout.emptyCart')} />
      <div className="mt-5 text-center">
        <Link className="button-primary no-underline" to="/products">{t('actions.continueShopping')}</Link>
      </div>
    </div>
  }

  return <div className="page-shell py-12">
    <h1 className="mb-8 [overflow-wrap:anywhere] text-3xl font-extrabold sm:text-4xl">{t('checkout.title')}</h1>
    <div className="grid min-w-0 gap-8 lg:grid-cols-[minmax(0,1fr)_minmax(18rem,380px)]">
      <form className="card grid min-w-0 gap-5 p-4 sm:p-6" noValidate onSubmit={form.handleSubmit(submit)}>
        <fieldset disabled={place.isPending || previewMutation.isPending} className="contents">
          <div className="grid gap-4 sm:grid-cols-2">
            <label>
              {t('checkout.customerName')}
              <input aria-invalid={Boolean(form.formState.errors.customerName)} className="field mt-2" autoComplete="name" {...form.register('customerName', { onChange: () => setSelectedAddressId('') })} />
              {form.formState.errors.customerName && <small className="field-error">
                {validationMessage('customerName', 'validation.required')}
              </small>}
            </label>
            <label>
              {t('auth.phone')}
              <input aria-invalid={Boolean(form.formState.errors.phoneNumber)} className="field mt-2" autoComplete="tel" dir="ltr" {...form.register('phoneNumber', { onChange: () => setSelectedAddressId('') })} />
              {form.formState.errors.phoneNumber && <small className="field-error">
                {validationMessage('phoneNumber', 'validation.phone')}
              </small>}
            </label>
          </div>

          {Boolean(addresses.data?.length) && <label>
            {t('checkout.savedAddress')}
            <select
              className="field mt-2"
              dir={selectedAddress ? textDirection(addressLabel(selectedAddress)) : i18n.dir()}
              onChange={(event) => selectAddress(event.target.value)}
              value={selectedAddressId}
            >
              <option value="">{t('checkout.enterNewAddress')}</option>
              {addresses.data?.map((address) => <option key={address.id} value={address.id}>{addressLabel(address)}</option>)}
            </select>
          </label>}

          <label>
            {t('checkout.deliveryLocation')}
            <select aria-invalid={Boolean(form.formState.errors.deliveryLocationId)} className="field mt-2" {...form.register('deliveryLocationId', { onChange: () => setSelectedAddressId('') })}>
              <option value="">{t('checkout.selectLocation')}</option>
              {locations.data?.map((location) => <option key={location.id} value={location.id}>{location.name}</option>)}
            </select>
            {form.formState.errors.deliveryLocationId && <small className="field-error">
              {validationMessage('deliveryLocationId', 'validation.required')}
            </small>}
          </label>
          <div className="grid gap-4 sm:grid-cols-2">
            <label>
              {t('checkout.area')}
              <input aria-invalid={Boolean(form.formState.errors.area)} className="field mt-2" autoComplete="address-level2" {...form.register('area', { onChange: () => setSelectedAddressId('') })} />
              {form.formState.errors.area && <small className="field-error">
                {validationMessage('area', 'validation.required')}
              </small>}
            </label>
            <label>
              {t('checkout.address')}
              <input aria-invalid={Boolean(form.formState.errors.addressLine)} className="field mt-2" autoComplete="street-address" {...form.register('addressLine', { onChange: () => setSelectedAddressId('') })} />
              {form.formState.errors.addressLine && <small className="field-error">
                {validationMessage('addressLine', 'validation.required')}
              </small>}
            </label>
          </div>
          <label>
            {t('checkout.taxNumber')}
            <input className="field mt-2" dir="ltr" {...form.register('taxNumber', { onChange: () => setSelectedAddressId('') })} />
          </label>
          <label>
            {t('checkout.deliveryNotes')}
            <textarea className="field mt-2 min-h-28" {...form.register('deliveryNotes', { onChange: () => setSelectedAddressId('') })} />
          </label>
          <fieldset>
            <legend className="mb-3 font-bold">{t('checkout.paymentMethod')}</legend>
            <div className="grid gap-3 sm:grid-cols-2">
              {availableMethods.map((method) => <label className={`card flex min-w-0 cursor-pointer items-start gap-2 p-4 transition ${values.paymentMethod === method.method ? '!border-[var(--heritage-burgundy)] bg-[var(--heritage-beige)]/45' : ''}`} key={method.method}>
                <input className="mt-1 shrink-0" type="radio" value={method.method} {...form.register('paymentMethod')} />
                <span className="min-w-0 [overflow-wrap:anywhere] font-bold">{t(`paymentMethods.${method.method}`)}</span>
              </label>)}
              <label className="card flex min-w-0 cursor-not-allowed items-start gap-2 p-4 opacity-60">
                <input className="mt-1 shrink-0" disabled name="paymentMethod" type="radio" value="ElectronicPayment" />
                <span className="min-w-0 [overflow-wrap:anywhere] font-bold">{t('paymentMethods.ElectronicPayment')} — {t('checkout.unavailablePayment')}</span>
              </label>
            </div>
            {availableMethods.length === 0 && <p className="field-error mt-3">{t('checkout.noPaymentMethods')}</p>}
            {form.formState.errors.paymentMethod && <p className="field-error mt-3">{t('checkout.noPaymentMethods')}</p>}
            {(selectedMethod?.instructions || currentPreview?.paymentInstructions) && <div className="mt-4 rounded-xl border-s-4 border-s-[var(--heritage-gold)] bg-[var(--heritage-beige)]/50 p-4">
              <p className="font-bold">{t('checkout.paymentInstructions')}</p>
              <p className="muted mt-1 whitespace-pre-wrap [overflow-wrap:anywhere] text-start text-sm" dir="auto">
                {currentPreview?.paymentInstructions ?? selectedMethod?.instructions}
              </p>
            </div>}
            {isManualPayment && <label className="mt-4 block">
              {t('checkout.paymentReference')}
              <input
                aria-invalid={Boolean(form.formState.errors.paymentReference)}
                className="field mt-2"
                dir="ltr"
                maxLength={250}
                {...form.register('paymentReference')}
              />
              {form.formState.errors.paymentReference && <small className="field-error">
                {validationMessage('paymentReference', 'validation.paymentReference')}
              </small>}
            </label>}
          </fieldset>
        </fieldset>
        <button className="button-primary max-w-full whitespace-normal text-center" disabled={previewMutation.isPending || place.isPending || availableMethods.length === 0} type="submit">
          {place.isPending
            ? t('checkout.placingOrder')
            : previewMutation.isPending
              ? t('checkout.calculating')
              : currentPreview
                ? t('checkout.confirmOrder')
                : t('checkout.reviewOrder')}
        </button>
      </form>

      <aside className="card h-fit min-w-0 overflow-hidden">
        <div className="flex min-w-0 items-center gap-2 border-b p-5 sm:p-6">
          <PackageCheck aria-hidden size={20} />
          <h2 className="[overflow-wrap:anywhere] text-xl font-bold">{t('checkout.summary')}</h2>
        </div>
        <div className="grid gap-3 p-5 sm:p-6">
          {cart.data.items.map((item) => <div className="flex min-w-0 items-start justify-between gap-3 border-b pb-3 last:border-0" key={item.id}>
            <div className="min-w-0">
              <p className="[overflow-wrap:anywhere] text-start font-bold" dir="auto">{item.name}</p>
              <p className="muted text-xs" dir="ltr">{t('orderDetails.quantity')}: {item.quantity ?? 0}</p>
            </div>
            <span className="shrink-0 text-sm font-bold" dir="ltr">{formatMoney(item.total ?? 0, cart.data.currency ?? 'ILS', locale)}</span>
          </div>)}
        </div>
        {currentPreview ? <dl className="grid gap-3 border-t p-5 sm:p-6">
          <div className="flex flex-wrap items-baseline gap-x-4 gap-y-1"><dt>{t('checkout.subtotal')}</dt><dd className="ms-auto" dir="ltr">{formatMoney(currentPreview.subtotal ?? 0, currentPreview.currency ?? 'ILS', locale)}</dd></div>
          <div className="flex flex-wrap items-baseline gap-x-4 gap-y-1"><dt>{t('checkout.discount')}</dt><dd className="ms-auto" dir="ltr">- {formatMoney(currentPreview.discount ?? 0, currentPreview.currency ?? 'ILS', locale)}</dd></div>
          <div className="flex flex-wrap items-baseline gap-x-4 gap-y-1"><dt>{t('checkout.delivery')}</dt><dd className="ms-auto" dir="ltr">{formatMoney(currentPreview.delivery ?? 0, currentPreview.currency ?? 'ILS', locale)}</dd></div>
          <div className="flex flex-wrap items-baseline gap-x-4 gap-y-1 border-t pt-4 text-xl font-extrabold"><dt>{t('checkout.totalIncludingDelivery')}</dt><dd className="ms-auto" dir="ltr">{formatMoney(currentPreview.total ?? 0, currentPreview.currency ?? 'ILS', locale)}</dd></div>
        </dl> : estimatedTotal != null ? <dl className="grid gap-3 border-t p-5 sm:p-6">
          <div className="flex flex-wrap items-baseline gap-x-4 gap-y-1"><dt>{t('checkout.subtotal')}</dt><dd className="ms-auto" dir="ltr">{formatMoney(cart.data.subtotal ?? 0, cart.data.currency ?? 'ILS', locale)}</dd></div>
          {(cart.data.discount ?? 0) > 0 && <div className="flex flex-wrap items-baseline gap-x-4 gap-y-1"><dt>{t('checkout.discount')}</dt><dd className="ms-auto" dir="ltr">- {formatMoney(cart.data.discount ?? 0, cart.data.currency ?? 'ILS', locale)}</dd></div>}
          <div className="flex flex-wrap items-baseline gap-x-4 gap-y-1"><dt>{t('checkout.delivery')}</dt><dd className="ms-auto" dir="ltr">{formatMoney(estimatedDelivery ?? 0, cart.data.currency ?? 'ILS', locale)}</dd></div>
          <div className="flex flex-wrap items-baseline gap-x-4 gap-y-1 border-t pt-4 text-xl font-extrabold"><dt>{t('checkout.totalIncludingDelivery')}</dt><dd className="ms-auto" dir="ltr">{formatMoney(estimatedTotal, cart.data.currency ?? 'ILS', locale)}</dd></div>
        </dl> : <div className="border-t p-5 sm:p-6">
          <dl className="mb-3"><div className="flex flex-wrap items-baseline gap-x-4 gap-y-1"><dt>{t('checkout.subtotal')}</dt><dd className="ms-auto" dir="ltr">{formatMoney(cart.data.subtotal ?? 0, cart.data.currency ?? 'ILS', locale)}</dd></div></dl>
          <p className="muted [overflow-wrap:anywhere]">{t('checkout.summaryHelp')}</p>
        </div>}
        {selectedLocation && <div className="flex min-w-0 items-start gap-2 border-t bg-[var(--heritage-beige)]/35 p-5 text-sm">
          <MapPin aria-hidden className="mt-0.5" size={17} />
          <dl className="min-w-0"><dt className="font-bold">{t('orderDetails.deliveryArea')}</dt><dd className="[overflow-wrap:anywhere]">{selectedLocation.name}</dd></dl>
        </div>}
      </aside>
    </div>
  </div>
}
