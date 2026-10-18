/**
 * One place that decides what a status looks like, so "Active" is the same green and "Cancelled"
 * the same red in every table, card, and detail view. Colour is only a hint: the badge always
 * carries the status as text too.
 *
 * Values mirror the backend status enums (order, payment, product, user, coupon, contact message,
 * notification, import, cart, OTP). Anything unknown stays neutral rather than guessing.
 */
export type StatusTone = 'success' | 'warning' | 'danger' | 'neutral'

const tones: Readonly<Record<string, StatusTone>> = {
  // Healthy, finished, or live.
  Active: 'success',
  Confirmed: 'success',
  Delivered: 'success',
  Paid: 'success',
  Verified: 'success',
  Completed: 'success',
  Replied: 'success',
  Sent: 'success',
  Converted: 'success',
  // Waiting on someone, or in progress.
  Pending: 'warning',
  Draft: 'warning',
  New: 'warning',
  UnderReview: 'warning',
  Processing: 'warning',
  Preparing: 'warning',
  ReadyForShipping: 'warning',
  Shipped: 'warning',
  Suspended: 'warning',
  OutOfStock: 'warning',
  Committing: 'warning',
  Previewed: 'warning',
  PartiallyRefunded: 'warning',
  // Failed or stopped.
  Failed: 'danger',
  PaymentFailed: 'danger',
  Cancelled: 'danger',
  Disabled: 'danger',
  Spam: 'danger',
  Returned: 'danger',
  // Switched off or over, on purpose.
  Refunded: 'neutral',
  Expired: 'neutral',
  Closed: 'neutral',
  Hidden: 'neutral',
  Discontinued: 'neutral',
  Abandoned: 'neutral',
  Archived: 'neutral',
}

export const statusTone = (status: unknown): StatusTone =>
  (typeof status === 'string' ? tones[status] : undefined) ?? 'neutral'

/** `badge` plus the tone class, ready to drop into a className. */
export const statusBadgeClass = (status: unknown) => `badge badge-${statusTone(status)}`
