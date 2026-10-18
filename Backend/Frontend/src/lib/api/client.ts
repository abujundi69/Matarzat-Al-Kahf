import axios, { type InternalAxiosRequestConfig } from 'axios'
import i18n from '@/i18n'
import { env } from '@/config/env'
import { isSessionValid, useSessionStore } from '@/stores/sessionStore'

type RetryableRequestConfig = InternalAxiosRequestConfig & {
  cartTokenRetry?: boolean
  authKind?: 'admin' | 'customer'
}

const requestPath = (url: unknown) => String(url ?? '')
const isAdminRequest = (url: unknown) => /^\/admin(?:\/|$)/.test(requestPath(url))
const isSharedFileRequest = (url: unknown) => /^\/(?:api\/v1\/)?files(?:\/|$|\?)/.test(requestPath(url))
const isAuthenticationRequest = (url: unknown) => /^\/auth\/login(?:\?|$)/.test(requestPath(url))
const isCartRequest = (url: unknown) => /^\/cart(?:\/|$|\?)/.test(requestPath(url))
const isPasswordVerification = (url: unknown) =>
  /^\/(?:admin|customer)\/profile\/change-password(?:\?|$)/.test(requestPath(url))

export const api = axios.create({
  baseURL: env.VITE_API_BASE_URL,
  timeout: env.VITE_REQUEST_TIMEOUT_MS,
  headers: { Accept: 'application/json' },
})

api.interceptors.request.use((config) => {
  const state = useSessionStore.getState()
  const requestConfig = config as RetryableRequestConfig
  const kind = isAdminRequest(config.url)
    || (isSharedFileRequest(config.url) && isSessionValid(state.admin))
    ? 'admin'
    : 'customer'
  requestConfig.authKind = kind
  const session = state[kind]
  if (session && !isSessionValid(session)) {
    state.setSession(kind, null)
  }

  const token = !isAuthenticationRequest(config.url) && isSessionValid(session) ? session?.token : undefined
  if (token) config.headers.Authorization = `Bearer ${token}`
  else delete config.headers.Authorization

  config.headers['Accept-Language'] ??= i18n.language.startsWith('en') ? 'en' : 'ar'
  if (isCartRequest(config.url) && state.guestCartToken) {
    config.headers['X-Cart-Token'] = state.guestCartToken
  } else {
    delete config.headers['X-Cart-Token']
  }
  if (config.data && !(config.data instanceof FormData)) {
    config.headers['Content-Type'] = 'application/json'
  }

  return config
})

api.interceptors.response.use(
  (response) => {
    if (isCartRequest(response.config.url)) {
      const cartToken = (response.data as { cartToken?: unknown } | undefined)?.cartToken
      if (typeof cartToken === 'string' && cartToken) {
        useSessionStore.getState().setGuestCartToken(cartToken)
      }
    }
    return response
  },
  async (error: unknown) => {
    if (!axios.isAxiosError(error)) return Promise.reject(error)

    const config = error.config as RetryableRequestConfig | undefined
    const status = error.response?.status
    const state = useSessionStore.getState()
    const method = config?.method?.toLowerCase()
    const canSafelyRecreateCart =
      isCartRequest(config?.url)
      && (method === 'get' || (method === 'post' && /\/items(?:\?|$)/.test(requestPath(config?.url))))

    if (status === 404 && config && state.guestCartToken && !config.cartTokenRetry && canSafelyRecreateCart) {
      state.setGuestCartToken('')
      config.cartTokenRetry = true
      delete config.headers['X-Cart-Token']
      return api.request(config)
    }

    if (status === 401 && !isPasswordVerification(config?.url)) {
      const kind = config?.authKind ?? (isAdminRequest(config?.url) ? 'admin' : 'customer')
      const hadSession = Boolean(state[kind]?.token)
      state.setSession(kind, null)

      if (hadSession && !isAuthenticationRequest(config?.url) && typeof window !== 'undefined') {
        const loginPath = '/login'
        const currentPath = `${window.location.pathname}${window.location.search}`
        if (window.location.pathname !== loginPath) {
          window.location.assign(`${loginPath}?returnUrl=${encodeURIComponent(currentPath)}`)
        }
      }
    }

    if (import.meta.env.DEV && !axios.isCancel(error)) {
      const data = error.response?.data as Record<string, unknown> | undefined
      console.error('[Matarzat API request failed]', {
        method: config?.method?.toUpperCase(),
        url: config ? api.getUri(config) : undefined,
        status,
        code: error.code,
        traceId: data?.traceId,
        serverTitle: data?.title,
        serverDetail: data?.detail,
      })
    }

    return Promise.reject(error)
  }
)

export const downloadFile = async (url: string, fallbackName: string) => {
  const response = await api.get<Blob>(url, { responseType: 'blob' })
  const disposition = String(response.headers['content-disposition'] ?? '')
  const name = /filename\*?=(?:UTF-8'')?"?([^";]+)/i.exec(disposition)?.[1]
  const objectUrl = URL.createObjectURL(response.data)
  const anchor = document.createElement('a')
  anchor.href = objectUrl
  anchor.download = name ? decodeURIComponent(name) : fallbackName
  anchor.click()
  window.setTimeout(() => URL.revokeObjectURL(objectUrl), 0)
}
