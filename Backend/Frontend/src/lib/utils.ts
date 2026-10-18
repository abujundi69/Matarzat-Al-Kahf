export const formatMoney = (value: number, _legacyCurrency = 'ILS', locale = 'ar-PS-u-nu-latn') =>
  new Intl.NumberFormat(locale, { style: 'currency', currency: 'ILS' }).format(value)

/**
 * The Arabic locales wrap the slashes of a numeric date in invisible direction marks (U+200F) and
 * separate date from time with an Arabic comma. Inside a table cell those marks reorder the
 * pieces, so "11/10/2026" can render as "10/2026/11". Plain digits, slashes and spaces read the
 * same in either direction.
 */
const withoutDirectionMarks = (formatted: string) =>
  formatted.replace(/[‎‏؜]/g, '').replace(/،\s*/g, ' ')

export const formatDate = (value: string | null | undefined, locale = 'ar-PS-u-nu-latn') =>
  value
    ? withoutDirectionMarks(new Intl.DateTimeFormat(locale, { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(value)))
    : '—'

export const formatDateCompact = (value: string | null | undefined, locale = 'ar-PS-u-nu-latn') =>
  value
    ? withoutDirectionMarks(new Intl.DateTimeFormat(locale, { dateStyle: 'medium' }).format(new Date(value)))
    : '—'

const relativeTimeUnits = [
  ['year', 365 * 24 * 60 * 60 * 1000],
  ['month', 30 * 24 * 60 * 60 * 1000],
  ['week', 7 * 24 * 60 * 60 * 1000],
  ['day', 24 * 60 * 60 * 1000],
  ['hour', 60 * 60 * 1000],
  ['minute', 60 * 1000],
  ['second', 1000],
] as const

export const formatRelativeTime = (
  value: string | null | undefined,
  locale = 'ar-PS-u-nu-latn',
  now = Date.now(),
) => {
  if (!value) return '—'
  const timestamp = Date.parse(value)
  if (!Number.isFinite(timestamp)) return '—'

  const difference = timestamp - now
  const absoluteDifference = Math.abs(difference)
  const [unit, milliseconds] = relativeTimeUnits.find(([, size]) => absoluteDifference >= size)
    ?? relativeTimeUnits.at(-1)!
  const amount = Math.round(difference / milliseconds)

  return new Intl.RelativeTimeFormat(locale, { numeric: 'auto' }).format(amount, unit)
}

export const createIdempotencyKey = () => crypto.randomUUID()

export const safeExternalUrl = (value: string) => {
  const url = new URL(value)
  if (!['https:', 'http:'].includes(url.protocol)) throw new Error('Unsupported URL')
  return url.toString()
}
