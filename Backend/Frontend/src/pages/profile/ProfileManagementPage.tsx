import { useEffect, useRef, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useForm, type FieldPath, type UseFormReturn } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { EmptyState, ErrorState, InlineLoader, LoadingState } from '@/components/ui/Feedback'
import { api } from '@/lib/api/client'
import { parseApiProblem, type ApiProblem } from '@/lib/api/problemDetails'
import { localizeRoleName } from '@/lib/roles'
import type { Schema } from '@/types/models'

type ProfileKind = 'admin' | 'customer'
type LanguageCode = Schema<'LanguageCode'>
type AdminProfile = Schema<'AdminProfileDto'>
type CustomerProfile = Schema<'CustomerProfileResponse'>
type ProfileResponse = AdminProfile | CustomerProfile
type ProfileValues = {
  fullName: string
  phoneNumber: string
  email: string
  preferredLanguage: LanguageCode
}
type PasswordValues = {
  currentPassword: string
  newPassword: string
  confirmPassword: string
}

const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
const phonePattern = /^\+?[\d\s().-]+$/
const strongPassword = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\p{L}\p{N}]).{12,128}$/u

const fieldError = (message: string | undefined) =>
  message ? <small className="field-error">{message}</small> : null

const serverField = <T extends Record<string, unknown>>(
  form: UseFormReturn<T>,
  field: FieldPath<T>,
  messages: string[],
) => {
  if (messages.length) form.setError(field, { message: messages.join(' ') })
  return messages.length > 0
}

const messagesFor = (problem: ApiProblem, ...names: string[]) =>
  Object.entries(problem.errors)
    .filter(([key]) => names.includes(key.toLowerCase()))
    .flatMap(([, messages]) => messages)

function ProfileManagementPage({ kind }: { kind: ProfileKind }) {
  const { i18n, t } = useTranslation()
  const queryClient = useQueryClient()
  const endpoint = `/${kind}/profile`
  const queryKey = [kind, 'profile'] as const
  const [profileProblem, setProfileProblem] = useState<ApiProblem | null>(null)
  const [passwordProblem, setPasswordProblem] = useState<ApiProblem | null>(null)
  const profileForm = useForm<ProfileValues>({
    defaultValues: {
      fullName: '',
      phoneNumber: '',
      email: '',
      preferredLanguage: 'Arabic',
    },
  })
  const passwordForm = useForm<PasswordValues>({
    defaultValues: {
      currentPassword: '',
      newPassword: '',
      confirmPassword: '',
    },
  })
  const query = useQuery({
    queryKey,
    queryFn: () => api.get<ProfileResponse>(endpoint).then((response) => response.data),
  })
  const profile = query.data
  const resetProfile = profileForm.reset
  const initializedProfileId = useRef<string | null>(null)

  useEffect(() => {
    if (!profile?.id || initializedProfileId.current === profile.id) return
    initializedProfileId.current = profile.id
    resetProfile({
      fullName: profile.fullName ?? '',
      phoneNumber: profile.phoneNumber ?? '',
      email: profile.email ?? '',
      preferredLanguage: profile.preferredLanguage ?? 'Arabic',
    })
  }, [profile, resetProfile])

  const updateProfile = useMutation({
    mutationFn: (values: ProfileValues) => {
      const request = kind === 'admin'
        ? {
            fullName: values.fullName.trim(),
            phoneNumber: values.phoneNumber.trim(),
            email: values.email.trim() || null,
            preferredLanguage: values.preferredLanguage,
          } satisfies Schema<'UpdateAdminUserRequest'>
        : {
            fullName: values.fullName.trim(),
            email: values.email.trim() || null,
            preferredLanguage: values.preferredLanguage,
          } satisfies Schema<'UpdateCustomerProfileRequest'>

      return api.put<ProfileResponse>(endpoint, request).then((response) => response.data)
    },
    onSuccess: (updated) => {
      initializedProfileId.current = null
      queryClient.setQueryData(queryKey, updated)
      setProfileProblem(null)
      toast.success(t('profile.saved'))
    },
    onError: (error) => {
      const problem = parseApiProblem(error)
      const fullName = messagesFor(problem, 'fullname')
      const phone = messagesFor(problem, 'phonenumber', 'value')
      const email = messagesFor(problem, 'email')
      const language = messagesFor(problem, 'preferredlanguage')
      const mapped = [
        serverField(profileForm, 'fullName', fullName),
        serverField(profileForm, 'phoneNumber', phone),
        serverField(profileForm, 'email', email),
        serverField(profileForm, 'preferredLanguage', language),
      ].some(Boolean)
      setProfileProblem(mapped ? null : problem)
      toast.error(problem.title)
    },
  })

  const changePassword = useMutation({
    mutationFn: (values: PasswordValues) => api.post(`${endpoint}/change-password`, {
      currentPassword: values.currentPassword,
      newPassword: values.newPassword,
    }),
    onSuccess: () => {
      passwordForm.reset()
      setPasswordProblem(null)
      toast.success(t('profile.passwordChanged'))
    },
    onError: (error) => {
      const problem = parseApiProblem(error)
      const currentPassword = messagesFor(problem, 'currentpassword')
      const newPassword = messagesFor(problem, 'newpassword', 'password')
      const mappedCurrent = serverField(
        passwordForm,
        'currentPassword',
        problem.status === 401 ? [t('profile.currentPasswordIncorrect')] : currentPassword,
      )
      const mappedNew = serverField(passwordForm, 'newPassword', newPassword)
      setPasswordProblem(mappedCurrent || mappedNew ? null : problem)
      toast.error(problem.status === 401 ? t('profile.currentPasswordIncorrect') : problem.title)
    },
  })

  const submitProfile = profileForm.handleSubmit((values) => {
    profileForm.clearErrors()
    setProfileProblem(null)
    let valid = true
    const fullName = values.fullName.trim()
    const email = values.email.trim()
    if (!fullName || fullName.length > 200) {
      profileForm.setError('fullName', { message: t('profile.validation.fullName') })
      valid = false
    }
    if (email && (email.length > 320 || !emailPattern.test(email))) {
      profileForm.setError('email', { message: t('validation.email') })
      valid = false
    }
    if (kind === 'admin') {
      const phone = values.phoneNumber.trim()
      const normalizedLength = phone.replace(/[^\d+]/g, '').length
      if (!phone || !phonePattern.test(phone) || normalizedLength > 30) {
        profileForm.setError('phoneNumber', { message: t('profile.validation.phone') })
        valid = false
      }
    }
    if (valid) updateProfile.mutate(values)
  })

  const submitPassword = passwordForm.handleSubmit((values) => {
    passwordForm.clearErrors()
    setPasswordProblem(null)
    let valid = true
    if (!values.currentPassword) {
      passwordForm.setError('currentPassword', { message: t('validation.required') })
      valid = false
    }
    if (!strongPassword.test(values.newPassword)) {
      passwordForm.setError('newPassword', { message: t('validation.passwordPolicy') })
      valid = false
    }
    if (values.newPassword !== values.confirmPassword) {
      passwordForm.setError('confirmPassword', { message: t('validation.passwordMatch') })
      valid = false
    }
    if (valid) changePassword.mutate(values)
  })

  if (query.isLoading) return <LoadingState />
  if (query.isError) return <ErrorState error={query.error} retry={() => void query.refetch()} />
  if (!profile?.id) return <EmptyState message={t('profile.empty')} />

  const adminProfile = kind === 'admin' ? profile as AdminProfile : null
  const adminRoles = adminProfile?.roleSummaries?.length
    ? adminProfile.roleSummaries
    : (adminProfile?.roles ?? [])

  return (
    <section className="min-w-0">
      <h1 className="[overflow-wrap:anywhere] text-3xl font-extrabold">{t('profile.title')}</h1>

      <div className="mt-6 grid min-w-0 gap-6 xl:grid-cols-[minmax(0,1fr)_minmax(0,1fr)] xl:items-start">
        <form className="card grid min-w-0 gap-4 p-5 sm:p-6" noValidate onSubmit={submitProfile}>
          <h2 className="min-w-0 [overflow-wrap:anywhere] text-xl font-extrabold">{t('profile.personalInformation')}</h2>

          <label className="grid gap-2 font-bold">
            <span>{t('profile.fullName')}</span>
            <input
              aria-invalid={Boolean(profileForm.formState.errors.fullName)}
              autoComplete="name"
              className="field"
              disabled={updateProfile.isPending}
              maxLength={200}
              {...profileForm.register('fullName')}
            />
            {fieldError(profileForm.formState.errors.fullName?.message)}
          </label>

          <div className="grid gap-2 font-bold">
            <label htmlFor={`${kind}-profile-phone`}>{t('profile.phoneNumber')}</label>
            <input
              aria-invalid={Boolean(profileForm.formState.errors.phoneNumber)}
              aria-describedby={kind === 'customer' ? `${kind}-profile-phone-help` : undefined}
              autoComplete="tel"
              className="field"
              dir="ltr"
              disabled={updateProfile.isPending}
              id={`${kind}-profile-phone`}
              readOnly={kind === 'customer'}
              {...profileForm.register('phoneNumber')}
            />
            {kind === 'customer' && <small className="muted block [overflow-wrap:anywhere]" id={`${kind}-profile-phone-help`}>{t('profile.phoneReadOnly')}</small>}
            {fieldError(profileForm.formState.errors.phoneNumber?.message)}
          </div>

          <label className="grid gap-2 font-bold">
            <span>{t('profile.email')}</span>
            <input
              aria-invalid={Boolean(profileForm.formState.errors.email)}
              autoComplete="email"
              className="field"
              dir="ltr"
              disabled={updateProfile.isPending}
              maxLength={320}
              type="email"
              {...profileForm.register('email')}
            />
            {fieldError(profileForm.formState.errors.email?.message)}
          </label>

          <label className="grid gap-2 font-bold">
            <span>{t('profile.preferredLanguage')}</span>
            <select
              aria-invalid={Boolean(profileForm.formState.errors.preferredLanguage)}
              className="field"
              disabled={updateProfile.isPending}
              {...profileForm.register('preferredLanguage')}
            >
              <option value="Arabic">{t('profile.languages.Arabic')}</option>
              <option value="English">{t('profile.languages.English')}</option>
            </select>
            {fieldError(profileForm.formState.errors.preferredLanguage?.message)}
          </label>

          {profileProblem && (
            <p className="alert alert-error font-bold" role="alert">
              {profileProblem.title}
            </p>
          )}
          <button className="button-primary max-w-full justify-self-stretch whitespace-normal text-center sm:justify-self-start" disabled={updateProfile.isPending} type="submit">
            {updateProfile.isPending ? <><InlineLoader /> {t('states.saving')}</> : t('profile.save')}
          </button>
        </form>

        <form className="card grid min-w-0 gap-4 p-5 sm:p-6" noValidate onSubmit={submitPassword}>
          <h2 className="min-w-0 [overflow-wrap:anywhere] text-xl font-extrabold">{t('profile.changePassword')}</h2>

          <label className="grid gap-2 font-bold">
            <span>{t('profile.currentPassword')}</span>
            <input
              aria-invalid={Boolean(passwordForm.formState.errors.currentPassword)}
              autoComplete="current-password"
              className="field"
              dir="ltr"
              disabled={changePassword.isPending}
              type="password"
              {...passwordForm.register('currentPassword')}
            />
            {fieldError(passwordForm.formState.errors.currentPassword?.message)}
          </label>

          <div className="grid gap-2 font-bold">
            <label htmlFor={`${kind}-profile-new-password`}>{t('profile.newPassword')}</label>
            <input
              aria-describedby={`${kind}-profile-new-password-help`}
              aria-invalid={Boolean(passwordForm.formState.errors.newPassword)}
              autoComplete="new-password"
              className="field"
              dir="ltr"
              disabled={changePassword.isPending}
              id={`${kind}-profile-new-password`}
              maxLength={128}
              type="password"
              {...passwordForm.register('newPassword')}
            />
            <small className="muted block [overflow-wrap:anywhere]" id={`${kind}-profile-new-password-help`}>{t('validation.passwordPolicy')}</small>
            {fieldError(passwordForm.formState.errors.newPassword?.message)}
          </div>

          <label className="grid gap-2 font-bold">
            <span>{t('profile.confirmPassword')}</span>
            <input
              aria-invalid={Boolean(passwordForm.formState.errors.confirmPassword)}
              autoComplete="new-password"
              className="field"
              dir="ltr"
              disabled={changePassword.isPending}
              maxLength={128}
              type="password"
              {...passwordForm.register('confirmPassword')}
            />
            {fieldError(passwordForm.formState.errors.confirmPassword?.message)}
          </label>

          {passwordProblem && (
            <p className="alert alert-error font-bold" role="alert">
              {passwordProblem.title}
            </p>
          )}
          <button className="button-primary max-w-full justify-self-stretch whitespace-normal text-center sm:justify-self-start" disabled={changePassword.isPending} type="submit">
            {changePassword.isPending ? <><InlineLoader /> {t('states.saving')}</> : t('profile.updatePassword')}
          </button>
        </form>

        {adminProfile && (
          <article className="card p-5 sm:p-6 xl:col-span-2">
            <h2 className="[overflow-wrap:anywhere] text-xl font-extrabold">{t('profile.access')}</h2>
            <div className="mt-4 flex flex-wrap gap-2">
              {adminRoles.map((role) => {
                const key = typeof role === 'string' ? role : (role.id ?? role.name ?? '')
                return <span className="badge max-w-full whitespace-normal [overflow-wrap:anywhere] text-start" dir="auto" key={key}>{localizeRoleName(t, role, i18n.language)}</span>
              })}
              {!adminRoles.length && <span className="muted">{t('profile.noRoles')}</span>}
            </div>
            <p className="muted mt-4 text-sm">
              {t('profile.permissionCount', { count: adminProfile.permissions?.length ?? 0 })}
            </p>
          </article>
        )}
      </div>
    </section>
  )
}

export function AdminProfilePage() {
  return <ProfileManagementPage kind="admin" />
}

export function CustomerProfilePage() {
  return <ProfileManagementPage kind="customer" />
}
