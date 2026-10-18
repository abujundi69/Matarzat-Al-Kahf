import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { useState } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { z } from 'zod'
import { toast } from 'sonner'
import { useTranslation } from 'react-i18next'
import { BrandLogo } from '@/components/common/BrandLogo'
import { api } from '@/lib/api/client'
import { parseApiProblem } from '@/lib/api/problemDetails'
import { mergeGuestCart } from '@/lib/cartMerge'
import { clearActorQueryCaches } from '@/lib/sessionCache'
import { useSessionStore } from '@/stores/sessionStore'
import type { Schema } from '@/types/models'

const phoneSchema = z.object({ phoneNumber: z.string().trim().min(8) })
const codeSchema = phoneSchema.extend({ code: z.string().trim().min(4).max(10) })
const passwordRule = z
  .string()
  .min(12)
  .regex(/[a-z]/)
  .regex(/[A-Z]/)
  .regex(/[0-9]/)
  .regex(/[^A-Za-z0-9]/)
const registrationSchema = z
  .object({
    fullName: z.string().trim().min(2),
    email: z.string().trim().email().or(z.literal('')),
    password: passwordRule,
    confirmPassword: z.string(),
  })
  .refine((values) => values.password === values.confirmPassword, {
    path: ['confirmPassword'],
    message: 'passwordsDoNotMatch',
  })
const resetSchema = z
  .object({ newPassword: passwordRule, confirmPassword: z.string() })
  .refine((values) => values.newPassword === values.confirmPassword, {
    path: ['confirmPassword'],
    message: 'passwordsDoNotMatch',
  })

type Step = 'phone' | 'code' | 'details' | 'success'
type RegistrationValues = z.infer<typeof registrationSchema>
type ResetValues = z.infer<typeof resetSchema>

export function OtpFlowPage({ purpose }: { purpose: 'register' | 'forgot-password' }) {
  const { t, i18n } = useTranslation()
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const queryClient = useQueryClient()
  const setAuthenticatedSession = useSessionStore((state) => state.setAuthenticatedSession)
  const guestCartToken = useSessionStore((state) => state.guestCartToken)
  const [step, setStep] = useState<Step>('phone')
  const [phone, setPhone] = useState('')
  const [verificationToken, setVerificationToken] = useState('')
  const phoneForm = useForm<z.infer<typeof phoneSchema>>({
    resolver: zodResolver(phoneSchema),
    defaultValues: { phoneNumber: '' },
  })
  const codeForm = useForm<z.infer<typeof codeSchema>>({
    resolver: zodResolver(codeSchema),
    defaultValues: { phoneNumber: '', code: '' },
  })
  const registrationForm = useForm<RegistrationValues>({
    resolver: zodResolver(registrationSchema),
    defaultValues: { fullName: '', email: '', password: '', confirmPassword: '' },
  })
  const resetForm = useForm<ResetValues>({
    resolver: zodResolver(resetSchema),
    defaultValues: { newPassword: '', confirmPassword: '' },
  })

  const requestOtp = useMutation({
    mutationFn: (data: { phoneNumber: string }) => api.post(
      `/customer/auth/${purpose}/request-otp`,
      {
        ...data,
        language: i18n.language.startsWith('ar') ? 'Arabic' : 'English',
      },
    ),
    onSuccess: (_, values) => {
      setPhone(values.phoneNumber)
      codeForm.reset({ phoneNumber: values.phoneNumber, code: '' })
      setStep('code')
    },
    onError: (error) => toast.error(parseApiProblem(error).title),
  })

  const verifyOtp = useMutation({
    mutationFn: (data: z.infer<typeof codeSchema>) => api
      .post<Schema<'OtpVerificationResponse'>>(
        `/customer/auth/${purpose}/verify-otp`,
        data,
      )
      .then((response) => response.data),
    onSuccess: (data) => {
      if (!data.verificationToken) {
        toast.error(t('errors.invalidResponse'))
        return
      }
      setVerificationToken(data.verificationToken)
      setStep('details')
    },
    onError: (error) => toast.error(parseApiProblem(error).title),
  })

  const completeRegistration = useMutation({
    mutationFn: (values: RegistrationValues) => api
      .post<Schema<'CustomerLoginResponse'>>('/customer/auth/register/complete', {
        verificationToken,
        fullName: values.fullName,
        phoneNumber: phone,
        email: values.email || null,
        password: values.password,
        preferredLanguage: i18n.language.startsWith('ar') ? 'Arabic' : 'English',
      })
      .then((response) => response.data),
    onSuccess: async (data) => {
      if (!data.accessToken) {
        toast.error(t('errors.invalidResponse'))
        return
      }
      await clearActorQueryCaches(queryClient)
      setAuthenticatedSession('customer', {
        token: data.accessToken,
        expiresAt: data.expiresAtUtc,
        permissions: [],
        actor: {
          id: data.customer?.id,
          fullName: data.customer?.fullName,
          phoneNumber: data.customer?.phoneNumber,
          email: data.customer?.email,
          preferredLanguage: data.customer?.preferredLanguage,
          actorType: 'Customer',
          roles: ['Customer'],
        },
      })
      let mergeFailed = false
      if (guestCartToken) {
        try {
          await mergeGuestCart(guestCartToken, queryClient)
        } catch (error) {
          mergeFailed = true
          toast.error(t('cart.mergeError'))
          if (import.meta.env.DEV) console.error('[Cart merge failed after registration]', parseApiProblem(error))
        }
      }
      toast.success(t('auth.registrationComplete'))
      const requested = searchParams.get('returnUrl')
      const destination = requested?.startsWith('/') && !requested.startsWith('//') && !requested.startsWith('/admin')
        ? requested
        : '/'
      void navigate(mergeFailed ? '/cart' : destination, {
        replace: true,
        state: mergeFailed ? { cartMergeReturnTo: destination } : undefined,
      })
    },
    onError: (error) => toast.error(parseApiProblem(error).title),
  })

  const resetPassword = useMutation({
    mutationFn: (values: ResetValues) => api.post('/customer/auth/reset-password', {
      phoneNumber: phone,
      verificationToken,
      newPassword: values.newPassword,
    }),
    onSuccess: () => {
      setStep('success')
      toast.success(t('auth.passwordResetComplete'))
    },
    onError: (error) => toast.error(parseApiProblem(error).title),
  })

  const steps: Step[] = ['phone', 'code', 'details', 'success']
  const currentStep = steps.indexOf(step)
  const passwordMessage = t('validation.passwordPolicy')
  const returnUrl = searchParams.get('returnUrl')
  const loginPath = returnUrl?.startsWith('/') && !returnUrl.startsWith('//')
    ? `/login?returnUrl=${encodeURIComponent(returnUrl)}`
    : '/login'

  return <main className="min-w-0 bg-[var(--heritage-cream)]">
    <div aria-hidden className="tatreez-band" />
    <div className="grid min-h-[calc(100dvh-10px)] min-w-0 place-items-center p-4 py-8">
    <div className="w-full min-w-0 max-w-md">
    <Link aria-label={t('auth.homeLinkLabel')} className="mx-auto mb-5 flex w-fit max-w-full flex-col items-center gap-2 no-underline" to="/">
      <BrandLogo className="size-20" />
      <span className="text-lg font-extrabold text-[var(--heritage-burgundy)] [overflow-wrap:anywhere]">{t('brand')}</span>
    </Link>
    <div className="card min-w-0 p-5 sm:p-8 [&_small]:[overflow-wrap:anywhere]">
      <h1 className="[overflow-wrap:anywhere] text-3xl font-extrabold text-[var(--heritage-burgundy)]">
        {purpose === 'register' ? t('auth.registerTitle') : t('auth.resetTitle')}
      </h1>
      <div aria-hidden className="mt-4 flex gap-2">
        {steps.slice(0, 3).map((item, index) =>
          <span
            key={item}
            className={`h-1 flex-1 rounded-full ${currentStep >= index ? 'bg-[var(--heritage-burgundy)]' : 'bg-stone-200'}`}
          />,
        )}
      </div>

      {step === 'phone' && <form className="mt-7 grid gap-4" noValidate onSubmit={phoneForm.handleSubmit((values) => requestOtp.mutate(values))}>
        <label>
          {t('auth.phone')}
          <input
            aria-invalid={Boolean(phoneForm.formState.errors.phoneNumber)}
            autoComplete="tel"
            className="field mt-2"
            dir="ltr"
            {...phoneForm.register('phoneNumber')}
          />
          {phoneForm.formState.errors.phoneNumber && <small className="field-error">{t('validation.phone')}</small>}
        </label>
        <button className="button-primary max-w-full whitespace-normal text-center" disabled={requestOtp.isPending} type="submit">
          {requestOtp.isPending ? t('states.sending') : t('auth.sendCode')}
        </button>
      </form>}

      {step === 'code' && <form className="mt-7 grid gap-4" noValidate onSubmit={codeForm.handleSubmit((values) => verifyOtp.mutate(values))}>
        <p className="muted [overflow-wrap:anywhere]">{t('auth.codeSent', { phone })}</p>
        <input type="hidden" {...codeForm.register('phoneNumber')} />
        <label>
          {t('auth.code')}
          <input
            aria-invalid={Boolean(codeForm.formState.errors.code)}
            autoComplete="one-time-code"
            className="field mt-2 text-center tracking-[.4em]"
            dir="ltr"
            inputMode="numeric"
            {...codeForm.register('code')}
          />
          {codeForm.formState.errors.code && <small className="field-error">{t('validation.code')}</small>}
        </label>
        <button className="button-primary max-w-full whitespace-normal text-center" disabled={verifyOtp.isPending} type="submit">
          {verifyOtp.isPending ? t('states.verifying') : t('auth.verifyCode')}
        </button>
        <button className="button-secondary max-w-full whitespace-normal text-center" disabled={verifyOtp.isPending} onClick={() => setStep('phone')} type="button">
          {t('actions.back')}
        </button>
      </form>}

      {step === 'details' && purpose === 'register' && <form className="mt-7 grid gap-4" noValidate onSubmit={registrationForm.handleSubmit((values) => completeRegistration.mutate(values))}>
        <label>
          {t('auth.name')}
          <input className="field mt-2" autoComplete="name" {...registrationForm.register('fullName')} />
          {registrationForm.formState.errors.fullName && <small className="field-error">{t('validation.required')}</small>}
        </label>
        <label>
          {t('auth.emailOptional')}
          <input className="field mt-2" autoComplete="email" dir="ltr" type="email" {...registrationForm.register('email')} />
          {registrationForm.formState.errors.email && <small className="field-error">{t('validation.email')}</small>}
        </label>
        <label>
          {t('auth.password')}
          <input className="field mt-2" autoComplete="new-password" dir="ltr" type="password" {...registrationForm.register('password')} />
          {registrationForm.formState.errors.password && <small className="field-error">{passwordMessage}</small>}
        </label>
        <label>
          {t('auth.confirmPassword')}
          <input className="field mt-2" autoComplete="new-password" dir="ltr" type="password" {...registrationForm.register('confirmPassword')} />
          {registrationForm.formState.errors.confirmPassword && <small className="field-error">{t('validation.passwordMatch')}</small>}
        </label>
        <button className="button-primary max-w-full whitespace-normal text-center" disabled={completeRegistration.isPending} type="submit">
          {completeRegistration.isPending ? t('states.saving') : t('auth.completeRegistration')}
        </button>
      </form>}

      {step === 'details' && purpose === 'forgot-password' && <form className="mt-7 grid gap-4" noValidate onSubmit={resetForm.handleSubmit((values) => resetPassword.mutate(values))}>
        <label>
          {t('auth.newPassword')}
          <input className="field mt-2" autoComplete="new-password" dir="ltr" type="password" {...resetForm.register('newPassword')} />
          {resetForm.formState.errors.newPassword && <small className="field-error">{passwordMessage}</small>}
        </label>
        <label>
          {t('auth.confirmPassword')}
          <input className="field mt-2" autoComplete="new-password" dir="ltr" type="password" {...resetForm.register('confirmPassword')} />
          {resetForm.formState.errors.confirmPassword && <small className="field-error">{t('validation.passwordMatch')}</small>}
        </label>
        <button className="button-primary max-w-full whitespace-normal text-center" disabled={resetPassword.isPending} type="submit">
          {resetPassword.isPending ? t('states.saving') : t('auth.resetPassword')}
        </button>
      </form>}

      {step === 'success' && <div className="mt-7">
        <p className="[overflow-wrap:anywhere] font-bold">{t('auth.passwordResetComplete')}</p>
        <Link className="button-primary mt-5 w-full whitespace-normal text-center no-underline" to={loginPath}>{t('actions.login')}</Link>
      </div>}

      {step !== 'success' && <Link className="mt-6 block [overflow-wrap:anywhere] text-center text-sm" to={loginPath}>{t('actions.backToLogin')}</Link>}
    </div>
    </div>
    </div>
  </main>
}
