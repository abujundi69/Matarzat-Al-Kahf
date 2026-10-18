import axios from 'axios'
import i18n from '@/i18n'

export type ApiProblem = {
  title: string
  detail?: string
  status?: number
  errors: Record<string, string[]>
  traceId?: string
  technicalTitle?: string
}

const translatedTitle = (status?: number, code?: string) => {
  if (code === 'ERR_CANCELED') {
    return i18n.t('errors.canceled.title')
  }

  if (code === 'ECONNABORTED') {
    return i18n.t('errors.timeout.title')
  }

  switch (status) {
    case 400:
      return i18n.t('errors.badRequest.title')
    case 401:
      return i18n.t('errors.unauthorized.title')
    case 403:
      return i18n.t('errors.forbidden.title')
    case 404:
      return i18n.t('errors.notFound.title')
    case 409:
      return i18n.t('errors.conflict.title')
    case 429:
      return i18n.t('errors.rateLimit.title')
    case 500:
      return i18n.t('errors.server.title')
    case 503:
      return i18n.t('errors.unavailable.title')
    default:
      return i18n.t('errors.network.title')
  }
}

export const parseApiProblem = (error: unknown): ApiProblem => {
  if (!axios.isAxiosError(error)) {
    return {
      title: i18n.t('errors.render.title'),
      detail: error instanceof Error ? error.message : undefined,
      errors: {},
    }
  }

  const data = error.response?.data as Record<string, unknown> | undefined
  const rawErrors = data?.errors
  const errors =
    rawErrors && typeof rawErrors === 'object'
      ? Object.fromEntries(
          Object.entries(rawErrors).map(([key, value]) => [
            key,
            Array.isArray(value) ? value.map(String) : [String(value)],
          ]),
        )
      : {}

  const status = error.response?.status
  return {
    title: translatedTitle(status, error.code),
    detail: data?.detail ? String(data.detail) : error.message,
    status,
    errors,
    traceId: data?.traceId ? String(data.traceId) : undefined,
    technicalTitle: data?.title ? String(data.title) : undefined,
  }
}
