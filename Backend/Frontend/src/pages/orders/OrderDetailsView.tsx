import {
  ArrowLeft,
  Clock3,
  CreditCard,
  MapPin,
  PackageCheck,
  UserRound,
} from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { EmptyState } from '@/components/ui/Feedback'
import { formatDate, formatMoney } from '@/lib/utils'
import type { Schema } from '@/types/models'

type OrderDetails = Schema<'OrderDetailsResponse'>

const valueOrDash = (value: string | null | undefined) => value?.trim() || '—'

export function OrderDetailsView({
  backTo,
  order,
  showInternalNotes = false,
}: {
  backTo: string
  order: OrderDetails
  showInternalNotes?: boolean
}) {
  const { t, i18n } = useTranslation()
  const locale = i18n.language.startsWith('ar') ? 'ar-PS-u-nu-latn' : 'en-US'
  const currency = order.currency ?? 'ILS'
  const items = order.items ?? []
  const history = order.statusHistory ?? []
  const payments = order.payments ?? []
  const latestPayment = payments.at(-1)
  const address = [order.deliveryAreaName ?? order.city, order.area, order.addressLine].filter(Boolean).join('، ')

  return (
    <section className="grid min-w-0 gap-6">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div className="min-w-0">
          <Link className="mb-3 inline-flex max-w-full items-center gap-2 [overflow-wrap:anywhere] text-sm font-bold text-[var(--heritage-burgundy)]" to={backTo}>
            <ArrowLeft aria-hidden className="shrink-0 rtl:rotate-180" size={17} />
            {t('orderDetails.backToOrders')}
          </Link>
          <p className="muted text-sm">{t('orderDetails.title')}</p>
          <h1 className="mt-1 [overflow-wrap:anywhere] text-3xl font-extrabold" dir="ltr">
            {valueOrDash(order.orderNumber)}
          </h1>
        </div>
        <span className="badge max-w-full whitespace-normal [overflow-wrap:anywhere] text-start text-sm" dir="auto">
          {t(`orderStatuses.${order.status}`, { defaultValue: order.status ?? '—' })}
        </span>
      </div>

      <div className="grid gap-4 lg:grid-cols-3">
        <article className="card min-w-0 p-5">
          <h2 className="flex min-w-0 items-start gap-2 [overflow-wrap:anywhere] font-extrabold">
            <UserRound aria-hidden className="mt-0.5 shrink-0" size={19} />
            {t('orderDetails.customer')}
          </h2>
          <dl className="mt-4 grid gap-3">
            <div>
              <dt className="muted text-sm">{t('customer.fields.customerName')}</dt>
              <dd className="[overflow-wrap:anywhere] text-start font-bold" dir="auto">{valueOrDash(order.customerName)}</dd>
            </div>
            <div>
              <dt className="muted text-sm">{t('customer.fields.phoneNumber')}</dt>
              <dd className="[overflow-wrap:anywhere] text-start font-bold" dir="ltr">{valueOrDash(order.phoneNumber)}</dd>
            </div>
            <div>
              <dt className="muted text-sm">{t('orders.date')}</dt>
              <dd className="[overflow-wrap:anywhere] font-bold">{formatDate(order.createdAtUtc, locale)}</dd>
            </div>
          </dl>
        </article>

        <article className="card min-w-0 p-5">
          <h2 className="flex min-w-0 items-start gap-2 [overflow-wrap:anywhere] font-extrabold">
            <MapPin aria-hidden className="mt-0.5 shrink-0" size={19} />
            {t('orderDetails.shipping')}
          </h2>
          <dl className="mt-4 grid gap-3">
            <div>
              <dt className="muted text-sm">{t('orderDetails.deliveryArea')}</dt>
              <dd className="[overflow-wrap:anywhere] text-start font-bold" dir="auto">{valueOrDash(order.deliveryAreaName ?? order.city)}</dd>
            </div>
            <div>
              <dt className="muted text-sm">{t('orders.deliveryAddress')}</dt>
              <dd className="[overflow-wrap:anywhere] text-start font-bold" dir="auto">{valueOrDash(address)}</dd>
            </div>
            {order.taxNumber && (
              <div>
                <dt className="muted text-sm">{t('orderDetails.taxNumber')}</dt>
                <dd className="[overflow-wrap:anywhere] text-start font-bold" dir="ltr">{order.taxNumber}</dd>
              </div>
            )}
            {order.deliveryNotes && (
              <div>
                <dt className="muted text-sm">{t('orderDetails.deliveryNotes')}</dt>
                <dd className="whitespace-pre-wrap [overflow-wrap:anywhere] text-start font-bold" dir="auto">{order.deliveryNotes}</dd>
              </div>
            )}
          </dl>
        </article>

        <article className="card min-w-0 p-5">
          <h2 className="flex min-w-0 items-start gap-2 [overflow-wrap:anywhere] font-extrabold">
            <CreditCard aria-hidden className="mt-0.5 shrink-0" size={19} />
            {t('orderDetails.payment')}
          </h2>
          <dl className="mt-4 grid gap-3">
            <div>
              <dt className="muted text-sm">{t('orders.paymentMethod')}</dt>
              <dd className="[overflow-wrap:anywhere] text-start font-bold" dir="auto">
                {t(`paymentMethods.${order.paymentMethod}`, { defaultValue: order.paymentMethod ?? '—' })}
              </dd>
            </div>
            <div>
              <dt className="muted text-sm">{t('customer.fields.paymentStatus')}</dt>
              <dd className="mt-1">
                <span className="badge max-w-full whitespace-normal [overflow-wrap:anywhere]">
                  {t(`paymentStatuses.${latestPayment?.status}`, { defaultValue: latestPayment?.status ?? '—' })}
                </span>
              </dd>
            </div>
            <div>
              <dt className="muted text-sm">{t('checkout.totalIncludingDelivery')}</dt>
              <dd className="[overflow-wrap:anywhere] text-xl font-extrabold text-[var(--heritage-burgundy)]" dir="ltr">
                {formatMoney(order.total ?? 0, currency, locale)}
              </dd>
            </div>
          </dl>
        </article>
      </div>

      <article className="card min-w-0">
        <div className="flex min-w-0 items-start gap-2 border-b p-5">
          <PackageCheck aria-hidden className="mt-0.5 shrink-0" size={20} />
          <h2 className="[overflow-wrap:anywhere] font-extrabold">{t('orderDetails.items')}</h2>
        </div>
        {!items.length
          ? <EmptyState message={t('orderDetails.emptyItems')} />
          : (
            <div className="min-w-0 sm:overflow-x-auto">
              <table className="block w-full text-start text-sm sm:table sm:min-w-[680px]">
                <thead className="hidden bg-[var(--heritage-beige)]/60 sm:table-header-group">
                  <tr>
                    <th className="px-5 py-3 text-start">{t('orderDetails.product')}</th>
                    <th className="px-5 py-3 text-start">{t('orderDetails.variant')}</th>
                    <th className="px-5 py-3 text-start">{t('orderDetails.unitPrice')}</th>
                    <th className="px-5 py-3 text-start">{t('orderDetails.quantity')}</th>
                    <th className="px-5 py-3 text-start">{t('cart.total')}</th>
                  </tr>
                </thead>
                <tbody className="grid gap-3 p-4 sm:table-row-group sm:divide-y sm:p-0">
                  {items.map((item, index) => (
                    <tr className="grid min-w-0 gap-2 rounded-xl border p-4 sm:table-row sm:rounded-none sm:border-0 sm:p-0" key={`${item.productVariantId ?? item.productId ?? 'item'}-${index}`}>
                      <td
                        className="grid min-w-0 grid-cols-[minmax(0,8rem)_minmax(0,1fr)] gap-3 py-1 sm:table-cell sm:px-5 sm:py-4"
                        data-label={t('orderDetails.product')}
                      >
                        <span className="muted [overflow-wrap:anywhere] font-semibold sm:hidden">{t('orderDetails.product')}</span>
                        <div className="min-w-0">
                          <p className="[overflow-wrap:anywhere] text-start font-bold" dir="auto">{valueOrDash(item.name)}</p>
                          <p className="muted mt-1 [overflow-wrap:anywhere] text-start text-xs" dir="ltr">{valueOrDash(item.sku)}</p>
                        </div>
                      </td>
                      <td
                        className="grid min-w-0 grid-cols-[minmax(0,8rem)_minmax(0,1fr)] gap-3 py-1 sm:table-cell sm:px-5 sm:py-4"
                        data-label={t('orderDetails.variant')}
                      >
                        <span className="muted [overflow-wrap:anywhere] font-semibold sm:hidden">{t('orderDetails.variant')}</span>
                        <span className="[overflow-wrap:anywhere] text-start" dir="auto">{valueOrDash(item.variant)}</span>
                      </td>
                      <td
                        className="grid min-w-0 grid-cols-[minmax(0,8rem)_minmax(0,1fr)] gap-3 py-1 sm:table-cell sm:px-5 sm:py-4"
                        data-label={t('orderDetails.unitPrice')}
                      >
                        <span className="muted [overflow-wrap:anywhere] font-semibold sm:hidden">{t('orderDetails.unitPrice')}</span>
                        <span className="[overflow-wrap:anywhere] text-start" dir="ltr">{formatMoney(item.unitPrice ?? 0, currency, locale)}</span>
                      </td>
                      <td
                        className="grid min-w-0 grid-cols-[minmax(0,8rem)_minmax(0,1fr)] gap-3 py-1 sm:table-cell sm:px-5 sm:py-4"
                        data-label={t('orderDetails.quantity')}
                      >
                        <span className="muted [overflow-wrap:anywhere] font-semibold sm:hidden">{t('orderDetails.quantity')}</span>
                        <span className="[overflow-wrap:anywhere] text-start" dir="ltr">{new Intl.NumberFormat(locale).format(item.quantity ?? 0)}</span>
                      </td>
                      <td
                        className="grid min-w-0 grid-cols-[minmax(0,8rem)_minmax(0,1fr)] gap-3 py-1 sm:table-cell sm:px-5 sm:py-4"
                        data-label={t('cart.total')}
                      >
                        <span className="muted [overflow-wrap:anywhere] font-semibold sm:hidden">{t('cart.total')}</span>
                        <span className="[overflow-wrap:anywhere] text-start font-extrabold" dir="ltr">{formatMoney(item.total ?? 0, currency, locale)}</span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        <dl className="ms-auto grid w-full max-w-md gap-2 border-t p-5">
          {[
            ['checkout.subtotal', order.subtotal],
            ['checkout.discount', order.discount],
            ['checkout.delivery', order.delivery],
          ].map(([key, amount]) => (
            <div className="flex flex-wrap items-baseline gap-x-4 gap-y-1" key={String(key)}>
              <dt className="muted [overflow-wrap:anywhere]">{t(String(key))}</dt>
              <dd className="ms-auto [overflow-wrap:anywhere] font-bold" dir="ltr">{formatMoney(Number(amount ?? 0), currency, locale)}</dd>
            </div>
          ))}
          <div className="mt-1 flex flex-wrap items-baseline gap-x-4 gap-y-1 border-t pt-3">
            <dt className="[overflow-wrap:anywhere] font-extrabold">{t('checkout.totalIncludingDelivery')}</dt>
            <dd className="ms-auto [overflow-wrap:anywhere] text-lg font-extrabold text-[var(--heritage-burgundy)]" dir="ltr">
              {formatMoney(order.total ?? 0, currency, locale)}
            </dd>
          </div>
        </dl>
      </article>

      <div className="grid gap-4 xl:grid-cols-2">
        <article className="card min-w-0 p-5">
          <h2 className="flex min-w-0 items-start gap-2 [overflow-wrap:anywhere] font-extrabold">
            <Clock3 aria-hidden className="mt-0.5 shrink-0" size={19} />
            {t('orderDetails.history')}
          </h2>
          {!history.length
            ? <p className="muted mt-4">{t('orderDetails.emptyHistory')}</p>
            : (
              <ol className="mt-5 grid gap-4">
                {history.map((entry, index) => (
                  <li className="relative border-s-2 border-[var(--heritage-gold)] ps-4" key={`${entry.changedAtUtc ?? index}-${index}`}>
                    <p className="[overflow-wrap:anywhere] text-start font-extrabold" dir="auto">
                      {t(`orderStatuses.${entry.status}`, { defaultValue: entry.status ?? '—' })}
                    </p>
                    <p className="muted [overflow-wrap:anywhere] text-xs">{formatDate(entry.changedAtUtc, locale)}</p>
                    {entry.note && <p className="mt-2 whitespace-pre-wrap [overflow-wrap:anywhere] text-start text-sm" dir="auto">{entry.note}</p>}
                  </li>
                ))}
              </ol>
            )}
        </article>

        <article className="card min-w-0 p-5">
          <h2 className="flex min-w-0 items-start gap-2 [overflow-wrap:anywhere] font-extrabold">
            <CreditCard aria-hidden className="mt-0.5 shrink-0" size={19} />
            {t('orderDetails.payments')}
          </h2>
          {!payments.length
            ? <p className="muted mt-4">{t('orderDetails.emptyPayments')}</p>
            : (
              <div className="mt-5 grid gap-3">
                {payments.map((payment, index) => (
                  <div className="min-w-0 rounded-xl border p-4" key={payment.id ?? index}>
                    <div className="flex flex-wrap items-center justify-between gap-2">
                      <p className="min-w-0 [overflow-wrap:anywhere] text-start font-extrabold" dir="auto">
                        {t(`paymentMethods.${payment.method}`, { defaultValue: payment.method ?? '—' })}
                      </p>
                      <span className="badge max-w-full whitespace-normal [overflow-wrap:anywhere] text-start" dir="auto">
                        {t(`paymentStatuses.${payment.status}`, { defaultValue: payment.status ?? '—' })}
                      </span>
                    </div>
                    <p className="mt-2 [overflow-wrap:anywhere] font-bold" dir="ltr">
                      {formatMoney(payment.amount ?? 0, payment.currency ?? currency, locale)}
                    </p>
                    {payment.transactionReference && (
                      <p className="muted mt-1 break-all text-start text-xs" dir="ltr">{payment.transactionReference}</p>
                    )}
                    <p className="muted mt-1 text-xs">{formatDate(payment.createdAtUtc, locale)}</p>
                  </div>
                ))}
              </div>
            )}
        </article>
      </div>

      {showInternalNotes && order.internalNotes && (
        <article className="card border-s-4 border-s-[var(--heritage-gold)] p-5">
          <h2 className="font-extrabold">{t('orderDetails.internalNotes')}</h2>
          <p className="mt-3 whitespace-pre-wrap [overflow-wrap:anywhere] text-start" dir="auto">{order.internalNotes}</p>
        </article>
      )}
    </section>
  )
}
