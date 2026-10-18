import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { Eye, EyeOff, Home } from 'lucide-react'
import { useRef, useState } from 'react'
import { useForm } from 'react-hook-form'
import { Link, Navigate, useLocation, useNavigate, useSearchParams } from 'react-router-dom'
import { z } from 'zod'
import { toast } from 'sonner'
import { useTranslation } from 'react-i18next'
import { BrandLogo } from '@/components/common/BrandLogo'
import { LanguageSwitcher } from '@/components/common/LanguageSwitcher'
import { api } from '@/lib/api/client'
import { resolveLoginDestination, safeReturnPath } from '@/lib/authRouting'
import { parseApiProblem } from '@/lib/api/problemDetails'
import { mergeGuestCart } from '@/lib/cartMerge'
import { clearActorQueryCaches } from '@/lib/sessionCache'
import { isSessionValid, useSessionStore, type SessionActor } from '@/stores/sessionStore'

const loginSchema = z.object({
  phoneNumber: z.string().trim().min(8),
  password: z.string().min(1),
})
type LoginValues = z.infer<typeof loginSchema>

type UnifiedLoginResponse = {
  accessToken?: string | null
  tokenType?: string | null
  expiresAtUtc?: string
  actor?: Partial<SessionActor> & { actorType?: string | null; permissions?: string[] }
}

const stringList = (value: unknown) => Array.isArray(value)
  ? value.filter((item): item is string => typeof item === 'string' && Boolean(item.trim()))
  : []

export function LoginPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const location = useLocation()
  const [searchParams] = useSearchParams()
  const queryClient = useQueryClient()
  const customerSession = useSessionStore((state) => state.customer)
  const adminSession = useSessionStore((state) => state.admin)
  const setAuthenticatedSession = useSessionStore((state) => state.setAuthenticatedSession)
  const guestCartToken = useSessionStore((state) => state.guestCartToken)
  const loginCompletionPending = useRef(false)
  const [showPassword, setShowPassword] = useState(false)
  const form = useForm<LoginValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: { phoneNumber: '', password: '' },
  })
  const requestedPath = searchParams.get('returnUrl')
    ?? (location.state as { from?: string } | null)?.from
  const authFlowPath = (path: string) => {
    const requested = safeReturnPath(requestedPath)
    return requested ? `${path}?returnUrl=${encodeURIComponent(requested)}` : path
  }

  const mutation = useMutation({
    mutationFn: (values: LoginValues) => api
      .post<UnifiedLoginResponse>('/auth/login', values)
      .then((response) => response.data),
    onSuccess: async (data) => {
      const actorType = data.actor?.actorType?.trim().toLowerCase()
      const permissions = stringList(data.actor?.permissions)
      const roles = stringList(data.actor?.roles)
      const kind = actorType === 'customer'
        ? 'customer'
        : actorType === 'admin' || permissions.length > 0
          ? 'admin'
          : undefined

      if (!data.accessToken || !kind || !data.actor?.actorType) {
        toast.error(t('errors.invalidResponse'))
        return
      }

      loginCompletionPending.current = true
      await clearActorQueryCaches(queryClient)
      setAuthenticatedSession(kind, {
        token: data.accessToken,
        expiresAt: data.expiresAtUtc,
        permissions,
        actor: {
          id: data.actor.id,
          fullName: data.actor.fullName,
          phoneNumber: data.actor.phoneNumber,
          email: data.actor.email,
          preferredLanguage: data.actor.preferredLanguage,
          actorType: data.actor.actorType,
          roles,
        },
      })

      const destination = resolveLoginDestination(kind, permissions, requestedPath)
      if (kind === 'customer' && guestCartToken) {
        try {
          await mergeGuestCart(guestCartToken, queryClient)
        } catch (error) {
          toast.error(t('cart.mergeError'))
          if (import.meta.env.DEV) console.error('[Cart merge failed after login]', parseApiProblem(error))
          void navigate('/cart', {
            replace: true,
            state: { cartMergeReturnTo: destination },
          })
          return
        }
      }

      void navigate(destination, { replace: true })
    },
    onError: (error) => {
      const problem = parseApiProblem(error)
      for (const [field, messages] of Object.entries(problem.errors)) {
        const normalized = field.toLowerCase()
        if (normalized.includes('password')) form.setError('password', { type: 'server', message: messages[0] })
        if (normalized.includes('phone')) {
          form.setError('phoneNumber', { type: 'server', message: messages[0] })
        }
      }
      toast.error(problem.status === 401 ? t('auth.invalidCredentials') : problem.title)
    },
  })
  const validationMessage = (field: keyof LoginValues, fallback: string) => {
    const error = form.formState.errors[field]
    return error ? t(fallback) : ''
  }

  if (!loginCompletionPending.current && isSessionValid(adminSession)) {
    return <Navigate replace to={resolveLoginDestination('admin', adminSession.permissions, requestedPath)} />
  }
  if (!loginCompletionPending.current && isSessionValid(customerSession)) {
    return <Navigate replace to={resolveLoginDestination('customer', customerSession.permissions, requestedPath)} />
  }

  return <main className="relative grid min-h-screen min-w-0 bg-[var(--heritage-cream)] lg:grid-cols-2">
    <div aria-hidden className="stitch-field relative hidden overflow-hidden bg-[var(--heritage-charcoal)] lg:flex lg:flex-col lg:items-center lg:justify-center lg:p-14">
      <div className="tatreez-band tatreez-band-lg absolute inset-x-0 top-0" />
      <div className="tatreez-band tatreez-band-lg absolute inset-x-0 bottom-0" />
      <div className="relative z-10 flex max-w-sm flex-col items-center text-center">
        <BrandLogo className="size-60 drop-shadow-[0_20px_44px_rgb(0_0_0_/_0.5)] xl:size-72" variant="lg" />
        <p className="mt-9 text-sm font-bold text-[var(--heritage-gold)]">{t('home.heritageEyebrow')}</p>
        <h2 className="mt-2 [overflow-wrap:anywhere] text-3xl font-extrabold leading-tight text-white">{t('home.heritageTitle')}</h2>
      </div>
    </div>
    <div className="relative grid min-w-0 place-items-center p-4 py-8 sm:p-8">
      <div className="w-full max-w-md">
        <div className="mb-4 flex min-w-0 items-center justify-between gap-3">
          <Link className="button-secondary min-w-0 no-underline" to="/">
            <Home aria-hidden size={18} />
            <span>{t('auth.continueBrowsing')}</span>
          </Link>
          <LanguageSwitcher compact />
        </div>
        <div className="card min-w-0 overflow-hidden">
          <div aria-hidden className="tatreez-band" />
          <div className="p-5 sm:p-10">
            <Link aria-label={t('auth.homeLinkLabel')} className="mx-auto block w-fit no-underline lg:hidden" to="/">
              <BrandLogo className="size-24" />
            </Link>
            <h1 className="mt-5 [overflow-wrap:anywhere] text-center text-3xl font-extrabold text-[var(--heritage-burgundy)] lg:mt-0">{t('auth.title')}</h1>
            <form
              className="mt-7 grid gap-5"
              noValidate
              onSubmit={form.handleSubmit((values) => {
                if (!mutation.isPending) mutation.mutate(values)
              })}
            >
              <label>
                {t('auth.phone')}
                <input
                  aria-invalid={Boolean(form.formState.errors.phoneNumber)}
                  autoComplete="tel"
                  className="field mt-2"
                  dir="ltr"
                  {...form.register('phoneNumber')}
                />
                {form.formState.errors.phoneNumber && <small className="field-error">
                  {validationMessage('phoneNumber', 'validation.phone')}
                </small>}
              </label>
              <label>
                {t('auth.password')}
                <div className="relative mt-2">
                  <input
                    aria-invalid={Boolean(form.formState.errors.password)}
                    autoComplete="current-password"
                    className="field field-icon-trailing"
                    dir="ltr"
                    type={showPassword ? 'text' : 'password'}
                    {...form.register('password')}
                  />
                  <button
                    aria-label={showPassword ? t('auth.hidePassword') : t('auth.showPassword')}
                    className="absolute inset-y-0 end-0 grid w-11 place-items-center text-[var(--heritage-muted)] transition-colors hover:text-[var(--heritage-burgundy)]"
                    dir="ltr"
                    onClick={() => setShowPassword((value) => !value)}
                    type="button"
                  >
                    {showPassword ? <EyeOff aria-hidden size={19} /> : <Eye aria-hidden size={19} />}
                  </button>
                </div>
                {form.formState.errors.password && <small className="field-error">
                  {validationMessage('password', 'validation.required')}
                </small>}
              </label>
              <button className="button-primary max-w-full whitespace-normal text-center" disabled={mutation.isPending} type="submit">
                {mutation.isPending ? t('auth.signingIn') : t('actions.login')}
              </button>
            </form>
            <div className="mt-6 flex min-w-0 flex-wrap justify-between gap-3 text-sm">
              <Link className="[overflow-wrap:anywhere]" to={authFlowPath('/register')}>{t('auth.createAccount')}</Link>
              <Link className="[overflow-wrap:anywhere]" to={authFlowPath('/forgot-password')}>{t('auth.forgotPassword')}</Link>
            </div>
          </div>
        </div>
      </div>
    </div>
  </main>
}
