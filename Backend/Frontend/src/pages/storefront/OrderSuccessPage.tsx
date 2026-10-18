import { useQuery } from '@tanstack/react-query'
import { CheckCircle2, Home, ReceiptText } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link, useLocation, useParams } from 'react-router-dom'
import { ErrorState, LoadingState } from '@/components/ui/Feedback'
import { api } from '@/lib/api/client'
import { formatMoney } from '@/lib/utils'
import type { Schema } from '@/types/models'

type SuccessResult = Schema<'PlaceOrderResponse'> & {
  paymentId?: string
  paymentMethod?: Schema<'PaymentMethodResponse'>['method']
  paymentInstructions?: string | null
  paymentRedirectUrl?: string | null
}

const validId = (value: string | undefined) =>
  Boolean(value && /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(value))

export function OrderSuccessPage() {
  const { t, i18n } = useTranslation()
  const { id } = useParams()
  const location = useLocation()
  const stateOrder = (location.state as { order?: SuccessResult } | null)?.order
  const idIsValid = validId(id)
  const order = useQuery({
    enabled: idIsValid,
    queryKey: ['customer', 'order', id, i18n.language],
    queryFn: () => api
      .get<Schema<'OrderDetailsResponse'>>(`/customer/orders/${id}`)
      .then((response) => response.data),
  })
  const paymentMethod = order.data?.paymentMethod ?? stateOrder?.paymentMethod
  const payment = order.data?.payments?.at(-1)
  const instructions = useQuery({
    enabled: idIsValid && (paymentMethod === 'BankTransfer' || paymentMethod === 'ElectronicWallet'),
    queryKey: ['customer', 'order', id, 'payment-instructions', i18n.language],
    queryFn: () => api
      .get<Schema<'PaymentMethodResponse'>>(`/customer/orders/${id}/payments/instructions`)
      .then((response) => response.data),
  })

  if (!idIsValid) return <div className="page-shell py-12"><ErrorState status={404} /></div>
  if (order.isLoading && !stateOrder) return <div className="page-shell py-12"><LoadingState cards={1} /></div>
  if (order.isError && !stateOrder) {
    return <div className="page-shell py-12"><ErrorState error={order.error} retry={() => void order.refetch()} /></div>
  }

  const orderNumber = order.data?.orderNumber ?? stateOrder?.orderNumber
  const status = order.data?.status ?? stateOrder?.status
  const paymentStatus = payment?.status ?? stateOrder?.paymentStatus
  const currency = order.data?.currency ?? stateOrder?.currency ?? 'ILS'
  const total = order.data?.total ?? stateOrder?.total ?? 0
  const paymentInstructions = stateOrder?.paymentInstructions ?? instructions.data?.instructions
  const locale = i18n.language.startsWith('ar') ? 'ar-PS-u-nu-latn' : 'en-US'

  return <div className="page-shell py-12 sm:py-16">
    <section aria-live="polite" className="card mx-auto min-w-0 max-w-3xl overflow-hidden text-center">
      <div className="heritage-pattern p-7 text-white sm:p-10">
        <CheckCircle2 className="mx-auto" size={64} />
        <h1 className="mt-5 [overflow-wrap:anywhere] text-2xl font-extrabold sm:text-3xl">{t('checkout.successTitle')}</h1>
      </div>
      <div className="p-5 sm:p-10">
        <div className="mx-auto grid max-w-xl gap-4 rounded-2xl bg-[var(--heritage-beige)] p-5 text-start sm:grid-cols-2 sm:p-6">
          <div className="sm:col-span-2">
            <span className="muted block text-sm">{t('checkout.orderNumber')}</span>
            <strong className="mt-1 block [overflow-wrap:anywhere] text-xl rtl:text-right" dir="ltr">{orderNumber ?? '—'}</strong>
          </div>
          {order.data && <>
            <div>
              <span className="muted block text-sm">{t('checkout.subtotal')}</span>
              <strong className="mt-1 block [overflow-wrap:anywhere] rtl:text-right" dir="ltr">{formatMoney(order.data.subtotal ?? 0, currency, locale)}</strong>
            </div>
            {(order.data.discount ?? 0) > 0 && <div>
              <span className="muted block text-sm">{t('checkout.discount')}</span>
              <strong className="mt-1 block [overflow-wrap:anywhere] rtl:text-right" dir="ltr">- {formatMoney(order.data.discount ?? 0, currency, locale)}</strong>
            </div>}
            <div>
              <span className="muted block text-sm">{t('orderDetails.deliveryArea')}</span>
              <strong className="mt-1 block [overflow-wrap:anywhere]">{order.data.deliveryAreaName ?? order.data.city ?? '—'}</strong>
            </div>
            <div>
              <span className="muted block text-sm">{t('checkout.delivery')}</span>
              <strong className="mt-1 block [overflow-wrap:anywhere] rtl:text-right" dir="ltr">{formatMoney(order.data.delivery ?? 0, currency, locale)}</strong>
            </div>
          </>}
          <div>
            <span className="muted block text-sm">{t('checkout.totalIncludingDelivery')}</span>
            <strong className="mt-1 block [overflow-wrap:anywhere] text-xl rtl:text-right" dir="ltr">{formatMoney(total, currency, locale)}</strong>
          </div>
          <div>
            <span className="muted block text-sm">{t('customer.fields.status')}</span>
            <strong className="mt-1 block [overflow-wrap:anywhere]">{t(`orderStatuses.${status}`, { defaultValue: status ?? '—' })}</strong>
          </div>
          <div>
            <span className="muted block text-sm">{t('customer.fields.paymentStatus')}</span>
            <strong className="mt-1 block [overflow-wrap:anywhere]">{t(`paymentStatuses.${paymentStatus}`, { defaultValue: paymentStatus ?? '—' })}</strong>
          </div>
          <div>
            <span className="muted block text-sm">{t('orders.paymentMethod')}</span>
            <strong className="mt-1 block [overflow-wrap:anywhere]">{t(`paymentMethods.${paymentMethod}`, { defaultValue: paymentMethod ?? '—' })}</strong>
          </div>
        </div>

        {paymentInstructions && <div className="mx-auto mt-5 max-w-xl rounded-2xl border-s-4 border-s-[var(--heritage-gold)] bg-stone-50 p-5 text-start">
          <h2 className="font-extrabold">{t('checkout.nextPaymentStep')}</h2>
          <p className="muted mt-2 whitespace-pre-wrap [overflow-wrap:anywhere]" dir="auto">{paymentInstructions}</p>
          {(paymentMethod === 'BankTransfer' || paymentMethod === 'ElectronicWallet') && <p className="mt-3 text-sm font-bold text-[var(--heritage-burgundy)]">{t('checkout.uploadProofHint')}</p>}
        </div>}

        {order.isError && <p className="alert alert-warning mx-auto mt-5 max-w-xl font-normal" role="status">{t('checkout.detailsRefreshFailed')}</p>}

        <div className="mt-7 flex flex-wrap justify-center gap-3">
          <Link className="button-primary max-[399px]:w-full no-underline" to={`/account/orders/${id}`}>
            <ReceiptText aria-hidden size={18} />
            {t('checkout.viewOrder')}
          </Link>
          <Link className="button-secondary max-[399px]:w-full no-underline" to="/">
            <Home aria-hidden size={18} />
            {t('checkout.returnHome')}
          </Link>
        </div>
      </div>
    </section>
  </div>
}
