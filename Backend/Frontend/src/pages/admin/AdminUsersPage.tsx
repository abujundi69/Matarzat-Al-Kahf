import { useEffect, useMemo, useRef, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Edit3, KeyRound, Plus, Search, ShieldCheck, UserCheck, UserMinus, UserX, X } from 'lucide-react'
import { useForm, type FieldPath, type UseFormReturn } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { adminPermissions } from '@/config/adminResources'
import { EmptyState, ErrorState, ForbiddenState, InlineLoader, LoadingState } from '@/components/ui/Feedback'
import { api } from '@/lib/api/client'
import { parseApiProblem, type ApiProblem } from '@/lib/api/problemDetails'
import { formatDate } from '@/lib/utils'
import { useSessionStore } from '@/stores/sessionStore'
import type { Schema } from '@/types/models'
import { statusBadgeClass } from '@/lib/statusTone'

type AdminUser = Schema<'AdminUserResponse'>
type AdminUserPage = Schema<'AdminUserResponsePagedResult'>
type UserStatus = Schema<'UserStatus'>
type LanguageCode = Schema<'LanguageCode'>
type StatusAction = 'activate' | 'suspend' | 'disable'

type UserFormValues = {
  fullName: string
  phoneNumber: string
  email: string
  preferredLanguage: LanguageCode
  password: string
}

type PasswordFormValues = {
  newPassword: string
  confirmPassword: string
}

const pageSize = 20
const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
const phonePattern = /^\+?[\d\s().-]{7,30}$/
const strongPassword = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\p{L}\p{N}]).{12,128}$/u

const roleNamed = (value: string | null | undefined, expected: string) =>
  value?.replace(/[\s_-]+/g, '').toLowerCase() === expected.toLowerCase()

const isSuperAdminUser = (user: AdminUser) =>
  user.roles?.some((role) => roleNamed(role.name, 'SuperAdmin')) ?? false

const fieldError = (message: string | undefined) =>
  message ? <small className="field-error font-medium">{message}</small> : null

const messagesFor = (problem: ApiProblem, ...names: string[]) => {
  const normalized = names.map((name) => name.toLowerCase())
  return Object.entries(problem.errors)
    .filter(([key]) => normalized.includes(key.toLowerCase()))
    .flatMap(([, messages]) => messages)
}

const setServerField = <T extends Record<string, unknown>>(
  form: UseFormReturn<T>,
  field: FieldPath<T>,
  messages: string[],
) => {
  if (messages.length) form.setError(field, { message: messages.join(' ') })
  return messages.length > 0
}

function DialogShell({
  children,
  labelId,
  onClose,
}: {
  children: React.ReactNode
  labelId: string
  onClose: () => void
}) {
  const dialogRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    const previouslyFocused = document.activeElement as HTMLElement | null
    const previousOverflow = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        event.preventDefault()
        onClose()
        return
      }
      if (event.key !== 'Tab') return
      const focusable = Array.from(
        dialogRef.current?.querySelectorAll<HTMLElement>(
          'button:not([disabled]), input:not([disabled]), textarea:not([disabled]), select:not([disabled]), [href], [tabindex]:not([tabindex="-1"])',
        ) ?? [],
      )
      if (!focusable.length) return
      const first = focusable[0]
      const last = focusable[focusable.length - 1]
      if (event.shiftKey && document.activeElement === first) {
        event.preventDefault()
        last.focus()
      } else if (!event.shiftKey && document.activeElement === last) {
        event.preventDefault()
        first.focus()
      }
    }
    document.addEventListener('keydown', onKeyDown)
    return () => {
      document.removeEventListener('keydown', onKeyDown)
      document.body.style.overflow = previousOverflow
      previouslyFocused?.focus()
    }
  }, [onClose])

  return (
    <div
      aria-labelledby={labelId}
      aria-modal="true"
      className="fixed inset-0 z-80 flex items-start justify-center overflow-y-auto bg-black/45 p-3 sm:p-4"
      onMouseDown={(event) => {
        if (event.target === event.currentTarget) onClose()
      }}
      ref={dialogRef}
      role="dialog"
    >
      {children}
    </div>
  )
}

function UserEditorDialog({
  onClose,
  user,
}: {
  onClose: () => void
  user: AdminUser | null
}) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const [problem, setProblem] = useState<ApiProblem | null>(null)
  const editing = Boolean(user?.id)
  const form = useForm<UserFormValues>({
    defaultValues: {
      fullName: user?.fullName ?? '',
      phoneNumber: user?.phoneNumber ?? '',
      email: user?.email ?? '',
      preferredLanguage: user?.preferredLanguage ?? 'Arabic',
      password: '',
    },
  })
  const titleId = `admin-user-editor-${user?.id ?? 'new'}`

  const mutation = useMutation({
    mutationFn: async (values: UserFormValues) => {
      if (editing && user?.id) {
        const request = {
          fullName: values.fullName.trim(),
          phoneNumber: values.phoneNumber.trim(),
          email: values.email.trim() || null,
          preferredLanguage: values.preferredLanguage,
        } satisfies Schema<'UpdateAdminUserRequest'>
        const updated = await api.put<AdminUser>(`/admin/users/${user.id}`, request)
        return updated.data
      }

      const request = {
        fullName: values.fullName.trim(),
        phoneNumber: values.phoneNumber.trim(),
        email: values.email.trim() || null,
        password: values.password,
        preferredLanguage: values.preferredLanguage,
      } satisfies Schema<'CreateAdminUserRequest'>
      return api.post<AdminUser>('/admin/users', request).then((response) => response.data)
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['admin-access-users'] })
      setProblem(null)
      toast.success(t(editing ? 'access.users.success.updated' : 'access.users.success.created'))
      onClose()
    },
    onError: (error) => {
      const nextProblem = parseApiProblem(error)
      const mapped = [
        setServerField(form, 'fullName', messagesFor(nextProblem, 'fullname')),
        setServerField(form, 'phoneNumber', messagesFor(nextProblem, 'phonenumber')),
        setServerField(form, 'email', messagesFor(nextProblem, 'email')),
        setServerField(form, 'preferredLanguage', messagesFor(nextProblem, 'preferredlanguage')),
        setServerField(form, 'password', messagesFor(nextProblem, 'password', 'newpassword')),
      ].some(Boolean)
      setProblem(mapped ? null : nextProblem)
      toast.error(nextProblem.title)
    },
  })

  const submit = form.handleSubmit((values) => {
    form.clearErrors()
    setProblem(null)
    let valid = true
    const fullName = values.fullName.trim()
    const phone = values.phoneNumber.trim()
    const email = values.email.trim()

    if (!fullName || fullName.length > 200) {
      form.setError('fullName', { message: t('access.validation.fullName') })
      valid = false
    }
    if (!phonePattern.test(phone)) {
      form.setError('phoneNumber', { message: t('access.validation.phone') })
      valid = false
    }
    if (email && (email.length > 320 || !emailPattern.test(email))) {
      form.setError('email', { message: t('validation.email') })
      valid = false
    }
    if (!editing && !strongPassword.test(values.password)) {
      form.setError('password', { message: t('validation.passwordPolicy') })
      valid = false
    }
    if (valid) mutation.mutate(values)
  })

  return (
    <DialogShell labelId={titleId} onClose={() => !mutation.isPending && onClose()}>
      <form className="card my-auto grid max-h-[calc(100dvh-1.5rem)] w-full max-w-2xl gap-5 overflow-y-auto overscroll-contain p-4 sm:p-6" noValidate onSubmit={submit}>
        <div className="flex min-w-0 items-start justify-between gap-4">
          <div className="min-w-0">
            <h2 className="break-words text-xl font-extrabold [overflow-wrap:anywhere]" id={titleId}>
              {t(editing ? 'access.users.editTitle' : 'access.users.createTitle')}
            </h2>
            {!editing && <p className="muted mt-1 break-words text-sm [overflow-wrap:anywhere]">{t('access.users.formHelp')}</p>}
          </div>
          <button
            aria-label={t('actions.close')}
            className="button-secondary min-h-10 shrink-0 px-3"
            disabled={mutation.isPending}
            onClick={onClose}
            type="button"
          >
            <X aria-hidden size={18} />
          </button>
        </div>

        <div className="grid min-w-0 gap-4 sm:grid-cols-2">
          <label className="grid min-w-0 gap-2 font-bold">
            <span>{t('access.fields.fullName')}</span>
            <input
              aria-invalid={Boolean(form.formState.errors.fullName)}
              autoComplete="name"
              autoFocus
              className="field"
              disabled={mutation.isPending}
              maxLength={200}
              {...form.register('fullName')}
            />
            {fieldError(form.formState.errors.fullName?.message)}
          </label>

          <label className="grid min-w-0 gap-2 font-bold">
            <span>{t('access.fields.phoneNumber')}</span>
            <input
              aria-invalid={Boolean(form.formState.errors.phoneNumber)}
              autoComplete="tel"
              className="field"
              dir="ltr"
              disabled={mutation.isPending}
              maxLength={30}
              {...form.register('phoneNumber')}
            />
            {fieldError(form.formState.errors.phoneNumber?.message)}
          </label>

          <label className="grid min-w-0 gap-2 font-bold">
            <span>{t('access.fields.email')}</span>
            <input
              aria-invalid={Boolean(form.formState.errors.email)}
              autoComplete="email"
              className="field"
              dir="ltr"
              disabled={mutation.isPending}
              maxLength={320}
              type="email"
              {...form.register('email')}
            />
            {fieldError(form.formState.errors.email?.message)}
          </label>

          <label className="grid min-w-0 gap-2 font-bold">
            <span>{t('access.fields.preferredLanguage')}</span>
            <select className="field" disabled={mutation.isPending} {...form.register('preferredLanguage')}>
              <option value="Arabic">{t('profile.languages.Arabic')}</option>
              <option value="English">{t('profile.languages.English')}</option>
            </select>
            {fieldError(form.formState.errors.preferredLanguage?.message)}
          </label>

          {!editing && (
            <label className="grid min-w-0 gap-2 font-bold sm:col-span-2">
              <span>{t('access.fields.temporaryPassword')}</span>
              <input
                aria-describedby={`${titleId}-password-help`}
                aria-invalid={Boolean(form.formState.errors.password)}
                autoComplete="new-password"
                className="field"
                disabled={mutation.isPending}
                maxLength={128}
                type="password"
                {...form.register('password')}
              />
              <small className="muted break-words [overflow-wrap:anywhere]" id={`${titleId}-password-help`}>{t('validation.passwordPolicy')}</small>
              {fieldError(form.formState.errors.password?.message)}
            </label>
          )}
        </div>

        {problem && (
          <p className="alert alert-error font-bold" role="alert">
            {problem.title}{problem.detail ? `: ${problem.detail}` : ''}
          </p>
        )}

        <div className="flex flex-col-reverse gap-3 sm:flex-row sm:flex-wrap sm:justify-end">
          <button className="button-secondary w-full whitespace-normal sm:w-auto" disabled={mutation.isPending} onClick={onClose} type="button">
            {t('access.actions.cancel')}
          </button>
          <button
            className="button-primary w-full whitespace-normal sm:w-auto"
            disabled={mutation.isPending}
            type="submit"
          >
            {mutation.isPending ? <><InlineLoader /> {t('states.saving')}</> : t('access.actions.save')}
          </button>
        </div>
      </form>
    </DialogShell>
  )
}

function ResetPasswordDialog({ onClose, user }: { onClose: () => void; user: AdminUser }) {
  const { t } = useTranslation()
  const [problem, setProblem] = useState<ApiProblem | null>(null)
  const form = useForm<PasswordFormValues>({ defaultValues: { newPassword: '', confirmPassword: '' } })
  const titleId = `admin-user-password-${user.id}`
  const mutation = useMutation({
    mutationFn: (values: PasswordFormValues) => {
      const request = { newPassword: values.newPassword } satisfies Schema<'ResetAdminPasswordRequest'>
      return api.post(`/admin/users/${user.id}/reset-password`, request)
    },
    onSuccess: () => {
      toast.success(t('access.users.success.passwordReset'))
      onClose()
    },
    onError: (error) => {
      const nextProblem = parseApiProblem(error)
      const messages = messagesFor(nextProblem, 'newpassword', 'password')
      setServerField(form, 'newPassword', messages)
      setProblem(messages.length ? null : nextProblem)
      toast.error(nextProblem.title)
    },
  })
  const submit = form.handleSubmit((values) => {
    form.clearErrors()
    setProblem(null)
    let valid = true
    if (!strongPassword.test(values.newPassword)) {
      form.setError('newPassword', { message: t('validation.passwordPolicy') })
      valid = false
    }
    if (values.newPassword !== values.confirmPassword) {
      form.setError('confirmPassword', { message: t('validation.passwordMatch') })
      valid = false
    }
    if (valid) mutation.mutate(values)
  })

  return (
    <DialogShell labelId={titleId} onClose={() => !mutation.isPending && onClose()}>
      <form className="card my-auto grid max-h-[calc(100dvh-1.5rem)] w-full max-w-lg gap-4 overflow-y-auto overscroll-contain p-4 sm:p-6" noValidate onSubmit={submit}>
        <div className="flex min-w-0 items-start justify-between gap-4">
          <h2 className="min-w-0 break-words text-xl font-extrabold [overflow-wrap:anywhere]" id={titleId}>{t('access.users.resetPasswordTitle')}</h2>
          <button aria-label={t('actions.close')} className="button-secondary min-h-10 shrink-0 px-3" disabled={mutation.isPending} onClick={onClose} type="button">
            <X aria-hidden size={18} />
          </button>
        </div>
        <label className="grid min-w-0 gap-2 font-bold">
          <span>{t('access.fields.newPassword')}</span>
          <input
            aria-invalid={Boolean(form.formState.errors.newPassword)}
            autoComplete="new-password"
            autoFocus
            className="field"
            disabled={mutation.isPending}
            maxLength={128}
            type="password"
            {...form.register('newPassword')}
          />
          <small className="muted break-words [overflow-wrap:anywhere]">{t('validation.passwordPolicy')}</small>
          {fieldError(form.formState.errors.newPassword?.message)}
        </label>
        <label className="grid min-w-0 gap-2 font-bold">
          <span>{t('access.fields.confirmPassword')}</span>
          <input
            aria-invalid={Boolean(form.formState.errors.confirmPassword)}
            autoComplete="new-password"
            className="field"
            disabled={mutation.isPending}
            maxLength={128}
            type="password"
            {...form.register('confirmPassword')}
          />
          {fieldError(form.formState.errors.confirmPassword?.message)}
        </label>
        {problem && <p className="alert alert-error font-bold" role="alert">{problem.title}</p>}
        <div className="flex flex-col-reverse gap-3 sm:flex-row sm:flex-wrap sm:justify-end">
          <button className="button-secondary w-full whitespace-normal sm:w-auto" disabled={mutation.isPending} onClick={onClose} type="button">{t('access.actions.cancel')}</button>
          <button className="button-primary w-full whitespace-normal sm:w-auto" disabled={mutation.isPending} type="submit">
            {mutation.isPending ? <><InlineLoader /> {t('states.saving')}</> : t('access.users.actions.resetPassword')}
          </button>
        </div>
      </form>
    </DialogShell>
  )
}

export function AdminUsersPage() {
  const { i18n, t } = useTranslation()
  const queryClient = useQueryClient()
  const session = useSessionStore((state) => state.admin)
  const permissions = session?.permissions ?? []
  const canRead = permissions.includes(adminPermissions.usersRead)
  const canWrite = permissions.includes(adminPermissions.usersWrite)
  const currentIsSuperAdmin = session?.actor?.roles.some((role) => roleNamed(role, 'SuperAdmin')) ?? false
  const [draftSearch, setDraftSearch] = useState('')
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState<UserStatus | ''>('')
  const [page, setPage] = useState(1)
  const [editingUser, setEditingUser] = useState<AdminUser | null | undefined>(undefined)
  const [passwordUser, setPasswordUser] = useState<AdminUser | null>(null)

  const query = useQuery({
    queryKey: ['admin-access-users', page, search, status],
    queryFn: () => api.get<AdminUserPage>('/admin/users', {
      params: {
        Page: page,
        PageSize: pageSize,
        ...(search ? { Search: search } : {}),
        ...(status ? { Status: status } : {}),
      },
    }).then((response) => response.data),
    enabled: canRead,
    placeholderData: (previous) => previous,
  })
  const users = query.data?.items ?? []
  const totalPages = Math.max(query.data?.totalPages ?? 1, 1)

  const statusMutation = useMutation({
    mutationFn: ({ action, id }: { action: StatusAction; id: string }) =>
      api.post(`/admin/users/${id}/${action}`),
    onSuccess: async (_response, variables) => {
      await queryClient.invalidateQueries({ queryKey: ['admin-access-users'] })
      toast.success(t(`access.users.success.${variables.action}`))
    },
    onError: (error) => toast.error(parseApiProblem(error).title),
  })

  const statusOptions = useMemo<UserStatus[]>(() => ['Active', 'Suspended', 'Disabled'], [])
  const submitSearch = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setPage(1)
    setSearch(draftSearch.trim())
  }
  const changeStatusFilter = (next: UserStatus | '') => {
    setStatus(next)
    setPage(1)
  }
  const runStatusAction = (user: AdminUser, action: StatusAction) => {
    if (!canWrite || !user.id) return
    statusMutation.mutate({ action, id: user.id })
  }
  const userActions = (user: AdminUser) => {
    if (isSuperAdminUser(user) && !currentIsSuperAdmin) {
      return (
        <p className="alert alert-warning">
          <ShieldCheck aria-hidden className="mt-0.5 shrink-0" size={17} />
          <span className="break-words [overflow-wrap:anywhere]">{t('access.users.protectedSuperAdmin')}</span>
        </p>
      )
    }
    // The API refuses to let an administrator remove their own access, so those actions are not offered.
    const isSelf = Boolean(user.id) && user.id === session?.actor?.id
    return <div className="flex min-w-0 flex-wrap gap-2 max-sm:flex-col">
      <button className="button-secondary min-h-10 min-w-0 whitespace-normal px-3 max-sm:w-full" onClick={() => setEditingUser(user)} type="button">
        <Edit3 aria-hidden className="shrink-0" size={16} /> {t('access.users.actions.edit')}
      </button>
      <button className="button-secondary min-h-10 min-w-0 whitespace-normal px-3 max-sm:w-full" onClick={() => setPasswordUser(user)} type="button">
        <KeyRound aria-hidden className="shrink-0" size={16} /> {t('access.users.actions.resetPassword')}
      </button>
      {user.status !== 'Active' && (
        <button
          className="button-secondary min-h-10 min-w-0 whitespace-normal px-3 max-sm:w-full"
          disabled={statusMutation.isPending}
          onClick={() => runStatusAction(user, 'activate')}
          type="button"
        >
          <UserCheck aria-hidden className="shrink-0" size={16} /> {t('access.users.actions.activate')}
        </button>
      )}
      {!isSelf && user.status !== 'Suspended' && (
        <button
          className="button-secondary min-h-10 min-w-0 whitespace-normal px-3 max-sm:w-full"
          disabled={statusMutation.isPending}
          onClick={() => runStatusAction(user, 'suspend')}
          type="button"
        >
          <UserMinus aria-hidden className="shrink-0" size={16} /> {t('access.users.actions.suspend')}
        </button>
      )}
      {!isSelf && user.status !== 'Disabled' && (
        <button
          className="button-secondary min-h-10 min-w-0 whitespace-normal px-3 text-[var(--state-danger-text)] max-sm:w-full"
          disabled={statusMutation.isPending}
          onClick={() => runStatusAction(user, 'disable')}
          type="button"
        >
          <UserX aria-hidden className="shrink-0" size={16} /> {t('access.users.actions.disable')}
        </button>
      )}
    </div>
  }

  if (!canRead) return <ForbiddenState />
  if (query.isLoading) return <LoadingState />
  if (query.isError) return <ErrorState error={query.error} retry={() => void query.refetch()} />

  return (
    <section>
      <div className="flex min-w-0 flex-wrap items-center justify-between gap-4">
        <h1 className="min-w-0 break-words text-3xl font-extrabold [overflow-wrap:anywhere]">{t('access.users.title')}</h1>
        {canWrite && (
          <button className="button-primary w-full whitespace-normal sm:w-auto" onClick={() => setEditingUser(null)} type="button">
            <Plus aria-hidden size={18} /> {t('access.users.actions.create')}
          </button>
        )}
      </div>

      <form className="card mt-6 grid min-w-0 gap-3 p-4 md:grid-cols-[minmax(0,1fr)_minmax(10rem,12rem)_auto]" onSubmit={submitSearch}>
        <label className="grid min-w-0 gap-2 font-bold">
          <span>{t('access.users.searchLabel')}</span>
          <input
            className="field"
            onChange={(event) => setDraftSearch(event.target.value)}
            placeholder={t('access.users.searchPlaceholder')}
            type="search"
            value={draftSearch}
          />
        </label>
        <label className="grid min-w-0 gap-2 font-bold">
          <span>{t('access.fields.status')}</span>
          <select className="field" onChange={(event) => changeStatusFilter(event.target.value as UserStatus | '')} value={status}>
            <option value="">{t('access.users.allStatuses')}</option>
            {statusOptions.map((option) => <option key={option} value={option}>{t(`admin.values.${option}`)}</option>)}
          </select>
        </label>
        <button className="button-secondary self-end whitespace-normal" disabled={query.isFetching} type="submit">
          <Search aria-hidden size={18} /> {t('admin.search')}
        </button>
      </form>

      {query.isFetching && !query.isLoading && <p className="muted mt-3 text-sm" role="status">{t('states.loading')}</p>}

      {users.length === 0 ? (
        <div className="mt-6"><EmptyState message={t('access.users.empty')} /></div>
      ) : (
        <div className="mt-6 min-w-0">
          <div className="grid gap-4 lg:hidden">
            {users.map((user) => (
              <article className="card min-w-0 p-4" key={user.id}>
                <header className="min-w-0 border-b pb-3">
                  <h2 className="break-words font-extrabold [overflow-wrap:anywhere]" dir="auto">{user.fullName}</h2>
                  <p className="muted mt-1 break-all text-start text-sm" dir="ltr">{user.phoneNumber}</p>
                  {user.email && <p className="muted mt-1 break-all text-start text-sm" dir="ltr">{user.email}</p>}
                </header>
                <dl className="grid min-w-0 gap-0 sm:grid-cols-2">
                  <div className="border-b py-3 sm:px-2">
                    <dt className="muted text-xs font-semibold">{t('access.fields.status')}</dt>
                    <dd className="mt-1"><span className={statusBadgeClass(user.status ?? 'Pending') + ' max-w-full whitespace-normal break-words [overflow-wrap:anywhere]'}>{t(`admin.values.${user.status ?? 'Pending'}`)}</span></dd>
                  </div>
                  <div className="border-b py-3 sm:px-2">
                    <dt className="muted text-xs font-semibold">{t('access.fields.lastLogin')}</dt>
                    <dd className="mt-1 break-words text-sm font-semibold [overflow-wrap:anywhere]" dir="ltr">{formatDate(user.lastLoginAtUtc, i18n.language.startsWith('en') ? 'en' : 'ar-PS-u-nu-latn')}</dd>
                  </div>
                  <div className="min-w-0 py-3 sm:col-span-2 sm:px-2">
                    <dt className="muted text-xs font-semibold">{t('access.fields.accountType')}</dt>
                    <dd className="mt-2 flex min-w-0 flex-wrap gap-2">
                      <span className="badge max-w-full whitespace-normal break-words [overflow-wrap:anywhere]">
                        {t(`access.roleNames.${isSuperAdminUser(user) ? 'SuperAdmin' : 'Admin'}`)}
                      </span>
                    </dd>
                  </div>
                </dl>
                {canWrite && (
                  <div className="border-t pt-3">
                    <p className="muted mb-2 text-xs font-semibold">{t('access.fields.actions')}</p>
                    {userActions(user)}
                  </div>
                )}
              </article>
            ))}
          </div>
          <div className="card hidden min-w-0 overflow-x-auto lg:block">
          <table className="w-full min-w-[860px] table-auto border-collapse text-sm">
            <thead className="bg-[var(--heritage-beige)] text-start">
              <tr>
                <th className="p-4 text-start">{t('access.fields.user')}</th>
                <th className="p-4 text-start">{t('access.fields.status')}</th>
                <th className="p-4 text-start">{t('access.fields.accountType')}</th>
                <th className="p-4 text-start">{t('access.fields.lastLogin')}</th>
                {canWrite && <th className="p-4 text-start">{t('access.fields.actions')}</th>}
              </tr>
            </thead>
            <tbody>
              {users.map((user) => (
                <tr className="border-t border-stone-200 align-top" key={user.id}>
                  <td className="p-4">
                    <p className="break-words font-extrabold [overflow-wrap:anywhere]" dir="auto">{user.fullName}</p>
                    <p className="muted mt-1 break-all text-start" dir="ltr">{user.phoneNumber}</p>
                    {user.email && <p className="muted mt-1 break-all text-start" dir="ltr">{user.email}</p>}
                  </td>
                  <td className="p-4"><span className={statusBadgeClass(user.status ?? 'Pending')}>{t(`admin.values.${user.status ?? 'Pending'}`)}</span></td>
                  <td className="p-4">
                    <span className="badge max-w-full whitespace-normal break-words [overflow-wrap:anywhere]">
                      {t(`access.roleNames.${isSuperAdminUser(user) ? 'SuperAdmin' : 'Admin'}`)}
                    </span>
                  </td>
                  <td className="break-words p-4 [overflow-wrap:anywhere]" dir="ltr">{formatDate(user.lastLoginAtUtc, i18n.language.startsWith('en') ? 'en' : 'ar-PS-u-nu-latn')}</td>
                  {canWrite && (
                    <td className="p-4">
                      {userActions(user)}
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
          </div>
        </div>
      )}

      {users.length > 0 && (
        <nav aria-label={t('pagination.label')} className="mt-5 grid gap-3 sm:flex sm:flex-wrap sm:items-center sm:justify-between">
          <span className="muted break-words text-center text-sm [overflow-wrap:anywhere] sm:text-start">{t('access.users.total', { count: query.data?.totalItems ?? users.length })}</span>
          <div className="flex min-w-0 flex-wrap items-center justify-center gap-2">
            <button className="button-secondary min-w-0 flex-1 whitespace-normal sm:flex-none" disabled={page <= 1 || query.isFetching} onClick={() => setPage((value) => value - 1)} type="button">
              {t('pagination.previous')}
            </button>
            <span className="order-first w-full break-words text-center font-bold [overflow-wrap:anywhere] sm:order-none sm:w-auto">{t('pagination.pageOf', { page, total: totalPages })}</span>
            <button className="button-secondary min-w-0 flex-1 whitespace-normal sm:flex-none" disabled={page >= totalPages || query.isFetching} onClick={() => setPage((value) => value + 1)} type="button">
              {t('pagination.next')}
            </button>
          </div>
        </nav>
      )}

      {editingUser !== undefined && (
        <UserEditorDialog
          onClose={() => setEditingUser(undefined)}
          user={editingUser}
        />
      )}
      {passwordUser && <ResetPasswordDialog onClose={() => setPasswordUser(null)} user={passwordUser} />}
    </section>
  )
}
