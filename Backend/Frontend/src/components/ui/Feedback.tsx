import { AlertTriangle, Inbox, LoaderCircle, LockKeyhole } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { parseApiProblem } from '@/lib/api/problemDetails'

export function LoadingState({ cards = 3 }: { cards?: number }) {
  const { t } = useTranslation()
  return (
    <div aria-busy="true" aria-live="polite" role="status">
      <span className="sr-only">{t('states.loading')}</span>
      <div aria-hidden className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {Array.from({ length: cards }, (_, index) => <div className="skeleton h-64" key={index} />)}
      </div>
    </div>
  )
}

export function EmptyState({ message }: { message?: string }) {
  const { t } = useTranslation()
  return (
    <div className="card grid min-w-0 place-items-center gap-3 px-4 py-12 text-center sm:px-6 sm:py-16" role="status">
      <Inbox aria-hidden className="text-[var(--heritage-gold)]" size={38} />
      <p className="muted max-w-full [overflow-wrap:anywhere]">{message ?? t('states.empty')}</p>
    </div>
  )
}

type ErrorStateProps = {
  error?: unknown
  message?: string
  retry?: () => void
  status?: number
  title?: string
}

const statusKey = (status?: number) => {
  if (status === 400) return 'badRequest'
  if (status === 401) return 'unauthorized'
  if (status === 403) return 'forbidden'
  if (status === 404) return 'notFound'
  if (status === 409) return 'conflict'
  if (status === 429) return 'rateLimit'
  if (status === 503) return 'unavailable'
  if (status && status >= 500) return 'server'
  return 'network'
}

// Retrying only helps when the failure may be transient. A missing page (404), a rejected request
// (400/409) or a permission failure (401/403) answers the same way every time.
const isRetryable = (status?: number) =>
  status === undefined || status === 408 || status === 429 || status >= 500

export function ErrorState({ error, message, retry, status, title }: ErrorStateProps) {
  const { t } = useTranslation()
  const problem = error === undefined ? undefined : parseApiProblem(error)
  const resolvedStatus = status ?? problem?.status
  const key = statusKey(resolvedStatus)
  const resolvedTitle = title ?? problem?.title ?? t(`errors.${key}.title`)
  const resolvedMessage = message ?? t(`errors.${key}.message`)

  return (
    <div className="card grid min-w-0 place-items-center gap-3 px-4 py-12 text-center sm:px-6 sm:py-16" role="alert">
      {resolvedStatus === 403
        ? <LockKeyhole aria-hidden className="text-[var(--heritage-red)]" size={38} />
        : <AlertTriangle aria-hidden className="text-[var(--heritage-red)]" size={38} />}
      <div className="min-w-0 max-w-full">
        <h2 className="font-bold [overflow-wrap:anywhere]">{resolvedTitle}</h2>
        <p className="muted mt-2 max-w-xl [overflow-wrap:anywhere]">{resolvedMessage}</p>
      </div>
      {resolvedStatus && <span className="badge" dir="ltr">HTTP {resolvedStatus}</span>}
      {retry && isRetryable(resolvedStatus) && <button className="button-secondary" onClick={retry} type="button">{t('actions.retry')}</button>}
    </div>
  )
}

export function ForbiddenState() {
  const { t } = useTranslation()
  return <ErrorState message={t('errors.forbidden.message')} status={403} title={t('errors.forbidden.title')} />
}

export function InlineLoader() {
  const { t } = useTranslation()
  return <LoaderCircle aria-label={t('states.loading')} className="animate-spin" size={18} />
}
