import axios from 'axios'
import {
  CheckCircle2,
  CircleDollarSign,
  ContactRound,
  Globe2,
  Link2,
  LoaderCircle,
  MapPinned,
  Plus,
  Save,
  SearchCheck,
  Settings2,
  Trash2,
} from 'lucide-react'
import {
  useEffect,
  useMemo,
  useState,
  type FormEvent,
  type KeyboardEvent,
  type ReactNode,
} from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import type { TFunction } from 'i18next'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import brandLogo from '@/assets/brand/logo-sm.png'
import { ErrorState, ForbiddenState, LoadingState } from '@/components/ui/Feedback'
import { ImageFilePicker } from '@/components/admin/ImageFilePicker'
import { adminPermissions } from '@/config/adminResources'
import { env } from '@/config/env'
import { api } from '@/lib/api/client'
import { uploadAdminImage, validateAdminImage } from '@/lib/media'
import { parseApiProblem } from '@/lib/api/problemDetails'
import { useSessionStore } from '@/stores/sessionStore'
import type { components } from '@/types/api.generated'

type Schema<Name extends keyof components['schemas']> = components['schemas'][Name]
type StoreSetting = Schema<'StoreSettingResponse'>
type SaveSetting = Schema<'SaveSettingRequest'>
type SocialLink = Schema<'SocialLinkResponse'>
type SocialPlatform = Schema<'SocialPlatform'>
type SaveSocialLink = Schema<'SaveSocialLinkRequest'>
type PaymentMethod = Schema<'PaymentMethod'>
type PaymentMethodResponse = Schema<'PaymentMethodResponse'>
type UpdatePaymentMethod = Schema<'UpdatePaymentMethodRequest'>
type DeliveryLocation = Schema<'AdminDeliveryLocationResponse'>
type SaveDeliveryLocation = Schema<'SaveDeliveryLocationRequest'>
type SeoSettings = Schema<'SeoSettingsResponse'>
type SaveSeoSettings = Schema<'SaveSeoSettingsRequest'>
type LanguageCode = Schema<'LanguageCode'>
type FieldErrors = Record<string, string>

const tx = (
  t: TFunction,
  key: string,
  defaultValue: string,
  values: Record<string, unknown> = {},
) => String(t(key, { defaultValue, ...values }))

const hasPermission = (permissions: readonly string[], permission: string) =>
  permissions.includes(permission)

const httpUrl = (value: string) => {
  try {
    const url = new URL(value)
    return url.protocol === 'http:' || url.protocol === 'https:'
  } catch {
    return false
  }
}

const settingByKey = (settings: readonly StoreSetting[], key: string) =>
  settings.find((setting) => setting.key?.toLowerCase() === key.toLowerCase())

const settingValue = (settings: readonly StoreSetting[], key: string) =>
  settingByKey(settings, key)?.value ?? ''

const localizedSettingValue = (
  settings: readonly StoreSetting[],
  key: string,
  language: LanguageCode,
) => settingByKey(settings, key)?.translations?.find(
  (translation) => translation.language === language,
)?.value ?? ''

const toSaveSetting = (setting: StoreSetting): SaveSetting => ({
  key: setting.key ?? '',
  value: setting.value ?? null,
  description: setting.description ?? null,
  isPublic: setting.isPublic ?? false,
  isLocalized: setting.isLocalized ?? false,
  translations: setting.translations?.map((translation) => ({
    language: translation.language,
    value: translation.value ?? '',
  })) ?? [],
})

const replaceSetting = (
  settings: readonly SaveSetting[],
  key: string,
  next: SaveSetting,
) => {
  const index = settings.findIndex((setting) => setting.key?.toLowerCase() === key.toLowerCase())
  if (index < 0) return [...settings, next]
  return settings.map((setting, itemIndex) => itemIndex === index ? next : setting)
}

const upsertPlainSetting = (
  settings: readonly SaveSetting[],
  key: string,
  value: string,
  defaults: { description: string; isPublic: boolean },
) => {
  const current = settings.find((setting) => setting.key?.toLowerCase() === key.toLowerCase())
  return replaceSetting(settings, key, {
    ...current,
    key: current?.key ?? key,
    value,
    description: current?.description ?? defaults.description,
    isPublic: current?.isPublic ?? defaults.isPublic,
    isLocalized: false,
    translations: [],
  })
}

const upsertLocalizedSetting = (
  settings: readonly SaveSetting[],
  key: string,
  arabicValue: string,
  englishValue: string,
  defaults: { description: string; isPublic: boolean },
) => {
  const current = settings.find((setting) => setting.key?.toLowerCase() === key.toLowerCase())
  return replaceSetting(settings, key, {
    ...current,
    key: current?.key ?? key,
    value: null,
    description: current?.description ?? defaults.description,
    isPublic: current?.isPublic ?? defaults.isPublic,
    isLocalized: true,
    translations: [
      { language: 'Arabic', value: arabicValue },
      { language: 'English', value: englishValue },
    ],
  })
}

const removeSetting = (settings: readonly SaveSetting[], key: string) =>
  settings.filter((setting) => setting.key?.toLowerCase() !== key.toLowerCase())

function FieldMessage({ message }: { message?: string }) {
  return message ? <p className="field-error" role="alert">{message}</p> : null
}

function MutationError({ error }: { error: unknown }) {
  const { t } = useTranslation()
  if (!error) return null
  const problem = parseApiProblem(error)
  const details = Object.values(problem.errors).flat()
  return (
    <div className="alert alert-error flex-col p-4 font-normal" role="alert">
      <p className="font-bold">{problem.title}</p>
      {problem.detail && <p className="mt-1">{problem.detail}</p>}
      {details.length > 0 && (
        <ul className="mt-2 list-inside list-disc">
          {details.map((detail, index) => <li key={`${detail}-${index}`}>{detail}</li>)}
        </ul>
      )}
      {problem.traceId && (
        <p className="mt-2 break-all text-start text-xs" dir="ltr">
          {tx(t, 'settings.errors.traceId', 'Trace ID')}: {problem.traceId}
        </p>
      )}
    </div>
  )
}

function SavedMessage() {
  const { t } = useTranslation()
  return (
    <p className="alert alert-success" role="status">
      <CheckCircle2 aria-hidden className="mt-0.5 shrink-0" size={18} />
      <span className="break-words [overflow-wrap:anywhere]">{tx(t, 'settings.feedback.saved', 'Changes were saved successfully.')}</span>
    </p>
  )
}

function ReadOnlyNotice() {
  const { t } = useTranslation()
  return (
    <p className="alert alert-warning font-normal">
      {tx(t, 'settings.feedback.readOnly', 'You can view this section, but you do not have permission to change it.')}
    </p>
  )
}

function SectionHeading({
  icon,
  title,
}: {
  icon: ReactNode
  title: string
}) {
  return (
    <header className="mb-6 flex min-w-0 items-center gap-3">
      <span className="grid size-11 shrink-0 place-items-center rounded-xl bg-[var(--heritage-beige)] text-[var(--heritage-burgundy)]">
        {icon}
      </span>
      <h2 className="min-w-0 break-words text-xl font-extrabold [overflow-wrap:anywhere]">{title}</h2>
    </header>
  )
}

function SaveButton({ disabled, saving }: { disabled?: boolean; saving: boolean }) {
  const { t } = useTranslation()
  return (
    <button className="button-primary w-full whitespace-normal sm:w-auto" disabled={disabled || saving} type="submit">
      {saving ? <LoaderCircle aria-hidden className="animate-spin" size={18} /> : <Save aria-hidden size={18} />}
      {saving
        ? tx(t, 'states.saving', 'Saving…')
        : tx(t, 'actions.save', 'Save')}
    </button>
  )
}

const settingGroupQueryKey = (group: string) => ['admin-settings-group', group] as const

const getSettingGroup = (group: string) => api
  .get<StoreSetting[]>(`/admin/settings/${encodeURIComponent(group)}`)
  .then((response) => response.data)

const updateSettingGroup = (group: string, settings: SaveSetting[]) => api
  .put<StoreSetting[]>(`/admin/settings/${encodeURIComponent(group)}`, { settings })
  .then((response) => response.data)

type GeneralDraft = {
  nameArabic: string
  nameEnglish: string
  descriptionArabic: string
  descriptionEnglish: string
  timezone: string
  defaultLanguage: LanguageCode
  logoUrl: string
  faviconUrl: string
}

const blankGeneralDraft: GeneralDraft = {
  nameArabic: '',
  nameEnglish: '',
  descriptionArabic: '',
  descriptionEnglish: '',
  timezone: 'Asia/Hebron',
  defaultLanguage: 'Arabic',
  logoUrl: '',
  faviconUrl: '',
}

const normalizeLanguage = (value: string): LanguageCode =>
  value.toLowerCase().startsWith('en') ? 'English' : 'Arabic'

function GeneralSettingsSection({ permissions }: { permissions: readonly string[] }) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const canWrite = hasPermission(permissions, adminPermissions.settingsWrite)
  const canUpload = canWrite && hasPermission(permissions, adminPermissions.filesUpload)
  const [draft, setDraft] = useState<GeneralDraft>(blankGeneralDraft)
  const [errors, setErrors] = useState<FieldErrors>({})
  const [saved, setSaved] = useState(false)
  const [uploading, setUploading] = useState<'logoUrl' | 'faviconUrl' | null>(null)
  const [uploadError, setUploadError] = useState<unknown>(null)

  const query = useQuery({
    queryKey: settingGroupQueryKey('General'),
    queryFn: () => getSettingGroup('General'),
  })
  const mutation = useMutation({
    mutationFn: (settings: SaveSetting[]) => updateSettingGroup('General', settings),
    onSuccess: (data) => {
      queryClient.setQueryData(settingGroupQueryKey('General'), data)
      void queryClient.invalidateQueries({ queryKey: ['store-branding'] })
      setSaved(true)
      toast.success(tx(t, 'settings.feedback.saved', 'Changes were saved successfully.'))
    },
  })

  useEffect(() => {
    if (!query.data) return
    setDraft({
      nameArabic: localizedSettingValue(query.data, 'store.name', 'Arabic'),
      nameEnglish: localizedSettingValue(query.data, 'store.name', 'English'),
      descriptionArabic: localizedSettingValue(query.data, 'store.description', 'Arabic'),
      descriptionEnglish: localizedSettingValue(query.data, 'store.description', 'English'),
      timezone: settingValue(query.data, 'store.timezone') || 'Asia/Hebron',
      defaultLanguage: normalizeLanguage(settingValue(query.data, 'store.defaultLanguage') || 'Arabic'),
      logoUrl: settingValue(query.data, 'store.logoUrl'),
      faviconUrl: settingValue(query.data, 'store.faviconUrl'),
    })
  }, [query.data])

  const change = <Key extends keyof GeneralDraft>(key: Key, value: GeneralDraft[Key]) => {
    setDraft((current) => ({ ...current, [key]: value }))
    setErrors((current) => ({ ...current, [key]: '' }))
    setSaved(false)
    mutation.reset()
  }

  const validate = () => {
    const next: FieldErrors = {}
    if (!draft.nameArabic.trim()) next.nameArabic = tx(t, 'settings.validation.required', 'This field is required.')
    if (!draft.nameEnglish.trim()) next.nameEnglish = tx(t, 'settings.validation.required', 'This field is required.')
    const hasDescription = Boolean(draft.descriptionArabic.trim() || draft.descriptionEnglish.trim())
    if (hasDescription && !draft.descriptionArabic.trim()) next.descriptionArabic = tx(t, 'settings.validation.bothLanguages', 'Enter both Arabic and English values.')
    if (hasDescription && !draft.descriptionEnglish.trim()) next.descriptionEnglish = tx(t, 'settings.validation.bothLanguages', 'Enter both Arabic and English values.')
    if (!draft.timezone.trim()) next.timezone = tx(t, 'settings.validation.required', 'This field is required.')
    setErrors(next)
    return Object.keys(next).length === 0
  }

  const save = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!canWrite || !query.data || !validate()) return
    let settings = query.data.map(toSaveSetting)
    settings = upsertLocalizedSetting(settings, 'store.name', draft.nameArabic.trim(), draft.nameEnglish.trim(), {
      description: 'Store display name.',
      isPublic: true,
    })
    if (draft.descriptionArabic.trim() || draft.descriptionEnglish.trim()) {
      settings = upsertLocalizedSetting(settings, 'store.description', draft.descriptionArabic.trim(), draft.descriptionEnglish.trim(), {
        description: 'Store description.',
        isPublic: true,
      })
    } else {
      settings = removeSetting(settings, 'store.description')
    }
    settings = upsertPlainSetting(settings, 'store.currency', 'ILS', {
      description: 'Default store currency.',
      isPublic: true,
    })
    settings = upsertPlainSetting(settings, 'store.timezone', draft.timezone.trim(), {
      description: 'Store time zone.',
      isPublic: false,
    })
    settings = upsertPlainSetting(settings, 'store.defaultLanguage', draft.defaultLanguage, {
      description: 'Default store language.',
      isPublic: true,
    })
    settings = upsertPlainSetting(settings, 'store.supportedCurrencies', 'ILS', {
      description: 'Currencies supported by the storefront.',
      isPublic: true,
    })
    settings = upsertPlainSetting(settings, 'store.logoUrl', draft.logoUrl.trim(), {
      description: 'Store logo URL.',
      isPublic: true,
    })
    settings = upsertPlainSetting(settings, 'store.faviconUrl', draft.faviconUrl.trim(), {
      description: 'Store favicon URL.',
      isPublic: true,
    })
    setSaved(false)
    mutation.mutate(settings)
  }

  const upload = async (field: 'logoUrl' | 'faviconUrl', file?: File) => {
    if (!file || !canUpload) return
    const validation = validateAdminImage(file)
    if (validation) {
      setUploadError(new Error(t(validation === 'size' ? 'uploads.errors.size' : 'uploads.errors.type')))
      return
    }
    setUploading(field)
    setUploadError(null)
    try {
      const response = await uploadAdminImage(file, 'logos')
      change(field, response.url)
      toast.success(tx(t, 'settings.branding.uploaded', 'Image uploaded. Save the section to apply it.'))
    } catch (error) {
      setUploadError(error)
    } finally {
      setUploading(null)
    }
  }

  if (query.isLoading) return <LoadingState />
  if (query.isError) return <ErrorState error={query.error} retry={() => void query.refetch()} />

  return (
    <form className="card min-w-0 p-4 sm:p-6" onSubmit={save}>
      <SectionHeading
        icon={<Settings2 aria-hidden size={22} />}
        title={tx(t, 'settings.tabs.general', 'General')}
      />
      {!canWrite && <div className="mb-5"><ReadOnlyNotice /></div>}
      <fieldset className="grid min-w-0 gap-6" disabled={!canWrite || mutation.isPending}>
        <div>
          <h3 className="font-bold">{tx(t, 'settings.general.storeInformation', 'Store information')}</h3>
          <div className="mt-3 grid min-w-0 gap-4 md:grid-cols-2 [&>label]:min-w-0">
            <label>
              <span className="mb-1 block text-sm font-semibold">{tx(t, 'settings.fields.storeNameArabic', 'Store name (Arabic)')}</span>
              <input className="field" dir="rtl" maxLength={500} onChange={(event) => change('nameArabic', event.target.value)} value={draft.nameArabic} />
              <FieldMessage message={errors.nameArabic} />
            </label>
            <label>
              <span className="mb-1 block text-sm font-semibold">{tx(t, 'settings.fields.storeNameEnglish', 'Store name (English)')}</span>
              <input className="field" dir="ltr" maxLength={500} onChange={(event) => change('nameEnglish', event.target.value)} value={draft.nameEnglish} />
              <FieldMessage message={errors.nameEnglish} />
            </label>
            <label>
              <span className="mb-1 block text-sm font-semibold">{tx(t, 'settings.fields.storeDescriptionArabic', 'Description (Arabic, optional)')}</span>
              <textarea className="field min-h-28" dir="rtl" maxLength={2000} onChange={(event) => change('descriptionArabic', event.target.value)} value={draft.descriptionArabic} />
              <FieldMessage message={errors.descriptionArabic} />
            </label>
            <label>
              <span className="mb-1 block text-sm font-semibold">{tx(t, 'settings.fields.storeDescriptionEnglish', 'Description (English, optional)')}</span>
              <textarea className="field min-h-28" dir="ltr" maxLength={2000} onChange={(event) => change('descriptionEnglish', event.target.value)} value={draft.descriptionEnglish} />
              <FieldMessage message={errors.descriptionEnglish} />
            </label>
          </div>
        </div>

        <div className="border-t pt-5">
          <h3 className="font-bold">{tx(t, 'settings.general.localization', 'Localization')}</h3>
          <div className="mt-3 grid min-w-0 gap-4 md:grid-cols-2 [&>label]:min-w-0">
            <label>
              <span className="mb-1 block text-sm font-semibold">{tx(t, 'settings.fields.timezone', 'Time zone')}</span>
              <input className="field" dir="ltr" maxLength={100} onChange={(event) => change('timezone', event.target.value)} placeholder="Asia/Hebron" value={draft.timezone} />
              <FieldMessage message={errors.timezone} />
            </label>
            <label>
              <span className="mb-1 block text-sm font-semibold">{tx(t, 'settings.fields.defaultLanguage', 'Default language')}</span>
              <select className="field" onChange={(event) => change('defaultLanguage', event.target.value as LanguageCode)} value={draft.defaultLanguage}>
                <option value="Arabic">{tx(t, 'settings.languages.Arabic', 'Arabic')}</option>
                <option value="English">{tx(t, 'settings.languages.English', 'English')}</option>
              </select>
            </label>
          </div>
        </div>

        <div className="border-t pt-5">
          <h3 className="font-bold">{tx(t, 'settings.general.branding', 'Branding')}</h3>
          <div className="mt-3 grid min-w-0 gap-5 md:grid-cols-2">
            {([
              ['logoUrl', 'settings.fields.logo', 'Store logo'],
              ['faviconUrl', 'settings.fields.favicon', 'Site icon'],
            ] as const).map(([field, labelKey, label]) => (
              <ImageFilePicker
                currentUrl={draft[field]}
                defaultPreviewSrc={brandLogo}
                disabled={!canUpload || Boolean(uploading)}
                error={errors[field]}
                key={field}
                label={tx(t, labelKey, label)}
                onError={(message) => setErrors((current) => ({ ...current, [field]: message }))}
                onSelect={(file) => void upload(field, file ?? undefined)}
              />
            ))}
          </div>
        </div>
      </fieldset>
      <div className="mt-6 grid min-w-0 gap-3">
        {uploadError && <MutationError error={uploadError} />}
        {mutation.isError && <MutationError error={mutation.error} />}
        {saved && <SavedMessage />}
        {canWrite && <div><SaveButton disabled={Boolean(uploading)} saving={mutation.isPending} /></div>}
      </div>
    </form>
  )
}

type ContactDraft = {
  phone: string
  whatsapp: string
  email: string
  addressArabic: string
  addressEnglish: string
}

function ContactSettingsSection({ permissions }: { permissions: readonly string[] }) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const canWrite = hasPermission(permissions, adminPermissions.settingsWrite)
  const [draft, setDraft] = useState<ContactDraft>({ phone: '', whatsapp: '', email: '', addressArabic: '', addressEnglish: '' })
  const [errors, setErrors] = useState<FieldErrors>({})
  const [saved, setSaved] = useState(false)
  const query = useQuery({
    queryKey: settingGroupQueryKey('Contact'),
    queryFn: () => getSettingGroup('Contact'),
  })
  const mutation = useMutation({
    mutationFn: (settings: SaveSetting[]) => updateSettingGroup('Contact', settings),
    onSuccess: (data) => {
      queryClient.setQueryData(settingGroupQueryKey('Contact'), data)
      void queryClient.invalidateQueries({ queryKey: ['store-contact'] })
      setSaved(true)
      toast.success(tx(t, 'settings.feedback.saved', 'Changes were saved successfully.'))
    },
  })

  useEffect(() => {
    if (!query.data) return
    setDraft({
      phone: settingValue(query.data, 'contact.phone'),
      whatsapp: settingValue(query.data, 'contact.whatsapp'),
      email: settingValue(query.data, 'contact.email'),
      addressArabic: localizedSettingValue(query.data, 'contact.address', 'Arabic'),
      addressEnglish: localizedSettingValue(query.data, 'contact.address', 'English'),
    })
  }, [query.data])

  const change = (key: keyof ContactDraft, value: string) => {
    setDraft((current) => ({ ...current, [key]: value }))
    setErrors((current) => ({ ...current, [key]: '' }))
    setSaved(false)
    mutation.reset()
  }

  const save = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!canWrite || !query.data) return
    const next: FieldErrors = {}
    if (!draft.phone.trim()) next.phone = tx(t, 'settings.validation.required', 'This field is required.')
    if (!/^\+?[0-9 ()-]{7,30}$/.test(draft.whatsapp.trim())) next.whatsapp = tx(t, 'settings.validation.phone', 'Enter a valid phone number.')
    if (!draft.email.trim()) next.email = tx(t, 'settings.validation.required', 'This field is required.')
    else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(draft.email.trim())) next.email = tx(t, 'settings.validation.email', 'Enter a valid email address.')
    if (!draft.addressArabic.trim()) next.addressArabic = tx(t, 'settings.validation.required', 'This field is required.')
    if (!draft.addressEnglish.trim()) next.addressEnglish = tx(t, 'settings.validation.required', 'This field is required.')
    setErrors(next)
    if (Object.keys(next).length) return

    let settings = query.data.map(toSaveSetting)
    settings = upsertPlainSetting(settings, 'contact.phone', draft.phone.trim(), {
      description: 'Public contact phone.',
      isPublic: true,
    })
    settings = upsertPlainSetting(settings, 'contact.whatsapp', draft.whatsapp.trim(), {
      description: 'Public WhatsApp number.',
      isPublic: true,
    })
    settings = upsertPlainSetting(settings, 'contact.email', draft.email.trim(), {
      description: 'Public contact email.',
      isPublic: true,
    })
    settings = upsertLocalizedSetting(settings, 'contact.address', draft.addressArabic.trim(), draft.addressEnglish.trim(), {
      description: 'Public contact address.',
      isPublic: true,
    })
    setSaved(false)
    mutation.mutate(settings)
  }

  if (query.isLoading) return <LoadingState />
  if (query.isError) return <ErrorState error={query.error} retry={() => void query.refetch()} />

  return (
    <form className="card min-w-0 p-4 sm:p-6" onSubmit={save}>
      <SectionHeading
        icon={<ContactRound aria-hidden size={22} />}
        title={tx(t, 'settings.tabs.contact', 'Contact')}
      />
      {!canWrite && <div className="mb-5"><ReadOnlyNotice /></div>}
      <fieldset className="grid min-w-0 gap-4 md:grid-cols-2 [&>label]:min-w-0" disabled={!canWrite || mutation.isPending}>
        <label>
          <span className="mb-1 block text-sm font-semibold">{tx(t, 'settings.fields.phone', 'Phone number')}</span>
          <input className="field" dir="ltr" maxLength={30} onChange={(event) => change('phone', event.target.value)} value={draft.phone} />
          <FieldMessage message={errors.phone} />
        </label>
        <label>
          <span className="mb-1 block text-sm font-semibold">{tx(t, 'settings.fields.whatsapp', 'WhatsApp number')}</span>
          <input className="field" dir="ltr" maxLength={30} onChange={(event) => change('whatsapp', event.target.value)} value={draft.whatsapp} />
          <FieldMessage message={errors.whatsapp} />
        </label>
        <label>
          <span className="mb-1 block text-sm font-semibold">{tx(t, 'settings.fields.email', 'Email address')}</span>
          <input className="field" dir="ltr" maxLength={320} onChange={(event) => change('email', event.target.value)} type="email" value={draft.email} />
          <FieldMessage message={errors.email} />
        </label>
        <label>
          <span className="mb-1 block text-sm font-semibold">{tx(t, 'settings.fields.addressArabic', 'Address (Arabic)')}</span>
          <textarea className="field min-h-28" dir="rtl" maxLength={1000} onChange={(event) => change('addressArabic', event.target.value)} value={draft.addressArabic} />
          <FieldMessage message={errors.addressArabic} />
        </label>
        <label>
          <span className="mb-1 block text-sm font-semibold">{tx(t, 'settings.fields.addressEnglish', 'Address (English)')}</span>
          <textarea className="field min-h-28" dir="ltr" maxLength={1000} onChange={(event) => change('addressEnglish', event.target.value)} value={draft.addressEnglish} />
          <FieldMessage message={errors.addressEnglish} />
        </label>
      </fieldset>
      <div className="mt-6 grid min-w-0 gap-3">
        {mutation.isError && <MutationError error={mutation.error} />}
        {saved && <SavedMessage />}
        {canWrite && <div><SaveButton saving={mutation.isPending} /></div>}
      </div>
    </form>
  )
}

const socialPlatforms = ['Facebook', 'Instagram', 'WhatsApp', 'TikTok', 'YouTube', 'X'] as const satisfies readonly SocialPlatform[]
type ManagedSocialPlatform = typeof socialPlatforms[number]
type SocialDraft = Record<ManagedSocialPlatform, {
  id?: string
  url: string
  isActive: boolean
  sortOrder: number
}>

const displaySocialUrl = (platform: ManagedSocialPlatform, value: string) => {
  if (platform !== 'WhatsApp') return value
  const match = /^https?:\/\/(?:api\.)?wa\.me\/(\d+)/i.exec(value)
  return match ? `+${match[1]}` : value
}

const apiSocialUrl = (platform: ManagedSocialPlatform, value: string) => {
  const trimmed = value.trim()
  if (platform !== 'WhatsApp' || httpUrl(trimmed)) return trimmed
  const digits = trimmed.replace(/\D/g, '')
  return digits ? `https://wa.me/${digits}` : ''
}

const emptySocialDraft = (): SocialDraft => Object.fromEntries(
  socialPlatforms.map((platform, index) => [platform, {
    url: '',
    isActive: false,
    sortOrder: index + 1,
  }]),
) as SocialDraft

function SocialSettingsSection({ permissions }: { permissions: readonly string[] }) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const canWrite = hasPermission(permissions, adminPermissions.socialWrite)
  const [draft, setDraft] = useState<SocialDraft>(emptySocialDraft)
  const [errors, setErrors] = useState<FieldErrors>({})
  const [saved, setSaved] = useState(false)
  const query = useQuery({
    queryKey: ['admin-social-links'],
    queryFn: () => api.get<SocialLink[]>('/admin/social-links').then((response) => response.data),
  })
  const mutation = useMutation({
    mutationFn: async (values: SocialDraft) => {
      const requests = socialPlatforms.flatMap((platform) => {
        const item = values[platform]
        const url = apiSocialUrl(platform, item.url)
        if (!item.id && !url) return []
        const body: SaveSocialLink = {
          platform,
          url,
          isActive: item.isActive,
          sortOrder: item.sortOrder,
        }
        return [item.id
          ? api.put<SocialLink>(`/admin/social-links/${item.id}`, body)
          : api.post<SocialLink>('/admin/social-links', body)]
      })
      await Promise.all(requests)
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['admin-social-links'] })
      setSaved(true)
      toast.success(tx(t, 'settings.feedback.saved', 'Changes were saved successfully.'))
    },
  })

  useEffect(() => {
    if (!query.data) return
    const next = emptySocialDraft()
    for (const platform of socialPlatforms) {
      const item = query.data.find((link) => link.platform === platform)
      if (!item) continue
      next[platform] = {
        id: item.id,
        url: displaySocialUrl(platform, item.url ?? ''),
        isActive: item.isActive ?? false,
        sortOrder: item.sortOrder ?? next[platform].sortOrder,
      }
    }
    setDraft(next)
  }, [query.data])

  const change = (platform: ManagedSocialPlatform, values: Partial<SocialDraft[ManagedSocialPlatform]>) => {
    setDraft((current) => ({
      ...current,
      [platform]: { ...current[platform], ...values },
    }))
    setErrors((current) => ({ ...current, [platform]: '' }))
    setSaved(false)
    mutation.reset()
  }

  const save = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!canWrite) return
    const next: FieldErrors = {}
    for (const platform of socialPlatforms) {
      const item = draft[platform]
      const normalized = apiSocialUrl(platform, item.url)
      if ((item.id || item.isActive) && !normalized) {
        next[platform] = tx(t, 'settings.validation.required', 'This field is required.')
      } else if (normalized && !httpUrl(normalized)) {
        next[platform] = tx(t, 'settings.validation.httpUrl', 'Enter a valid HTTP(S) URL.')
      }
    }
    setErrors(next)
    if (Object.keys(next).length) return
    setSaved(false)
    mutation.mutate(draft)
  }

  if (query.isLoading) return <LoadingState />
  if (query.isError) return <ErrorState error={query.error} retry={() => void query.refetch()} />

  return (
    <form className="card min-w-0 p-4 sm:p-6" onSubmit={save}>
      <SectionHeading
        icon={<Link2 aria-hidden size={22} />}
        title={tx(t, 'settings.tabs.social', 'Social media')}
      />
      {!canWrite && <div className="mb-5"><ReadOnlyNotice /></div>}
      <fieldset className="grid min-w-0 gap-4 md:grid-cols-2" disabled={!canWrite || mutation.isPending}>
        {socialPlatforms.map((platform) => (
          <div className="min-w-0 rounded-xl border p-4" key={platform}>
            <div className="mb-3 flex min-w-0 flex-wrap items-center justify-between gap-3">
              <label className="min-w-0 break-words font-bold [overflow-wrap:anywhere]" htmlFor={`social-${platform}`}>{tx(t, `settings.social.platforms.${platform}`, platform)}</label>
              <label className="flex min-w-0 items-center gap-2 text-sm">
                <input checked={draft[platform].isActive} className="shrink-0" onChange={(event) => change(platform, { isActive: event.target.checked })} type="checkbox" />
                {tx(t, 'settings.fields.active', 'Active')}
              </label>
            </div>
            <input
              className="field"
              dir="ltr"
              id={`social-${platform}`}
              maxLength={2000}
              onChange={(event) => change(platform, { url: event.target.value })}
              placeholder={platform === 'WhatsApp' ? '+970590000000' : 'https://…'}
              value={draft[platform].url}
            />
            <FieldMessage message={errors[platform]} />
          </div>
        ))}
      </fieldset>
      <div className="mt-6 grid min-w-0 gap-3">
        {mutation.isError && <MutationError error={mutation.error} />}
        {saved && <SavedMessage />}
        {canWrite && <div><SaveButton saving={mutation.isPending} /></div>}
      </div>
    </form>
  )
}

const paymentMethods = ['CreditCard', 'BankTransfer', 'CashOnDelivery', 'ElectronicWallet'] as const satisfies readonly PaymentMethod[]
type ConfigurablePaymentMethod = typeof paymentMethods[number]
type PaymentDraft = Record<ConfigurablePaymentMethod, {
  isEnabled: boolean
  arabicInstructions: string
  englishInstructions: string
}>

const emptyPaymentDraft = (): PaymentDraft => Object.fromEntries(
  paymentMethods.map((method) => [method, {
    isEnabled: false,
    arabicInstructions: '',
    englishInstructions: '',
  }]),
) as PaymentDraft

const loadLocalizedPaymentMethods = async (token: string) => {
  const request = (language: 'ar' | 'en') => axios.get<PaymentMethodResponse[]>(
    `${env.VITE_API_BASE_URL}/admin/payment-methods`,
    {
      headers: {
        Accept: 'application/json',
        'Accept-Language': language,
        Authorization: `Bearer ${token}`,
      },
      timeout: env.VITE_REQUEST_TIMEOUT_MS,
    },
  ).then((response) => response.data)
  try {
    const [arabic, english] = await Promise.all([request('ar'), request('en')])
    return { arabic, english }
  } catch (error) {
    if (axios.isAxiosError(error) && error.response?.status === 401) {
      useSessionStore.getState().setSession('admin', null)
    }
    throw error
  }
}

function PaymentSettingsSection({ permissions }: { permissions: readonly string[] }) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const token = useSessionStore((state) => state.admin?.token ?? '')
  const canWrite = hasPermission(permissions, adminPermissions.paymentsWrite)
  const [draft, setDraft] = useState<PaymentDraft>(emptyPaymentDraft)
  const [errors, setErrors] = useState<FieldErrors>({})
  const [saved, setSaved] = useState(false)
  const query = useQuery({
    queryKey: ['admin-payment-method-settings'],
    queryFn: () => loadLocalizedPaymentMethods(token),
    enabled: Boolean(token),
  })
  const mutation = useMutation({
    mutationFn: (values: PaymentDraft) => Promise.all(paymentMethods.map((method) => {
      const body: UpdatePaymentMethod = values[method]
      return api.put<PaymentMethodResponse>(`/admin/payment-methods/${method}`, body)
    })),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['admin-payment-method-settings'] })
      setSaved(true)
      toast.success(tx(t, 'settings.feedback.saved', 'Changes were saved successfully.'))
    },
  })

  useEffect(() => {
    if (!query.data) return
    const next = emptyPaymentDraft()
    for (const method of paymentMethods) {
      const arabic = query.data.arabic.find((item) => item.method === method)
      const english = query.data.english.find((item) => item.method === method)
      next[method] = {
        isEnabled: arabic?.isEnabled ?? english?.isEnabled ?? false,
        arabicInstructions: arabic?.instructions ?? '',
        englishInstructions: english?.instructions ?? '',
      }
    }
    setDraft(next)
  }, [query.data])

  const change = (method: ConfigurablePaymentMethod, values: Partial<PaymentDraft[ConfigurablePaymentMethod]>) => {
    setDraft((current) => ({ ...current, [method]: { ...current[method], ...values } }))
    setErrors((current) => ({ ...current, [`${method}.arabic`]: '', [`${method}.english`]: '' }))
    setSaved(false)
    mutation.reset()
  }

  const save = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!canWrite) return
    const next: FieldErrors = {}
    for (const method of paymentMethods) {
      if (!draft[method].arabicInstructions.trim()) next[`${method}.arabic`] = tx(t, 'settings.validation.required', 'This field is required.')
      if (!draft[method].englishInstructions.trim()) next[`${method}.english`] = tx(t, 'settings.validation.required', 'This field is required.')
      if (draft[method].arabicInstructions.length > 2000) next[`${method}.arabic`] = tx(t, 'settings.validation.maxLength', 'Must be no more than {{count}} characters.', { count: 2000 })
      if (draft[method].englishInstructions.length > 2000) next[`${method}.english`] = tx(t, 'settings.validation.maxLength', 'Must be no more than {{count}} characters.', { count: 2000 })
    }
    setErrors(next)
    if (Object.keys(next).length) return
    setSaved(false)
    mutation.mutate(draft)
  }

  if (query.isLoading || (!query.data && query.fetchStatus !== 'idle')) return <LoadingState />
  if (query.isError) return <ErrorState error={query.error} retry={() => void query.refetch()} />

  return (
    <form className="card min-w-0 p-4 sm:p-6" onSubmit={save}>
      <SectionHeading
        icon={<CircleDollarSign aria-hidden size={22} />}
        title={tx(t, 'settings.tabs.payments', 'Payments')}
      />
      {!canWrite && <div className="mb-5"><ReadOnlyNotice /></div>}
      <fieldset className="grid min-w-0 gap-5" disabled={!canWrite || mutation.isPending}>
        {paymentMethods.map((method) => (
          <section className="min-w-0 rounded-xl border p-4" key={method}>
            <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
              <h3 className="min-w-0 break-words font-bold [overflow-wrap:anywhere]">{tx(t, `settings.payments.methods.${method}`, method)}</h3>
              <label className="flex min-w-0 items-center gap-2 text-sm font-semibold">
                <input checked={draft[method].isEnabled} className="shrink-0" onChange={(event) => change(method, { isEnabled: event.target.checked })} type="checkbox" />
                {tx(t, 'settings.fields.enabled', 'Enabled')}
              </label>
            </div>
            <div className="grid min-w-0 gap-4 md:grid-cols-2 [&>label]:min-w-0">
              <label>
                <span className="mb-1 block text-sm font-semibold">{tx(t, 'settings.fields.instructionsArabic', 'Instructions (Arabic)')}</span>
                <textarea className="field min-h-28" dir="rtl" maxLength={2000} onChange={(event) => change(method, { arabicInstructions: event.target.value })} value={draft[method].arabicInstructions} />
                <FieldMessage message={errors[`${method}.arabic`]} />
              </label>
              <label>
                <span className="mb-1 block text-sm font-semibold">{tx(t, 'settings.fields.instructionsEnglish', 'Instructions (English)')}</span>
                <textarea className="field min-h-28" dir="ltr" maxLength={2000} onChange={(event) => change(method, { englishInstructions: event.target.value })} value={draft[method].englishInstructions} />
                <FieldMessage message={errors[`${method}.english`]} />
              </label>
            </div>
          </section>
        ))}
        <section className="min-w-0 rounded-xl border bg-stone-50 p-4">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <h3 className="min-w-0 break-words font-bold [overflow-wrap:anywhere]">
              {t('paymentMethods.ElectronicPayment')} — {t('checkout.unavailablePayment')}
            </h3>
            <input aria-label={t('paymentMethods.ElectronicPayment')} checked={false} disabled type="checkbox" />
          </div>
        </section>
      </fieldset>
      <div className="mt-6 grid min-w-0 gap-3">
        {mutation.isError && <MutationError error={mutation.error} />}
        {saved && <SavedMessage />}
        {canWrite && <div><SaveButton saving={mutation.isPending} /></div>}
      </div>
    </form>
  )
}

type DeliveryDraft = {
  nameArabic: string
  nameEnglish: string
  additionalDeliveryCost: number | ''
  isActive: boolean
}

const blankDeliveryLocation: DeliveryDraft = {
  nameArabic: '',
  nameEnglish: '',
  isActive: true,
  additionalDeliveryCost: '',
}

const deliveryRequest = (location?: DeliveryLocation): DeliveryDraft => location ? {
  nameArabic: location.nameArabic ?? '',
  nameEnglish: location.nameEnglish ?? '',
  isActive: location.isActive ?? false,
  additionalDeliveryCost: location.additionalDeliveryCost ?? 0,
} : { ...blankDeliveryLocation }

function DeliveryLocationEditor({
  canWrite,
  location,
  onSaved,
}: {
  canWrite: boolean
  location?: DeliveryLocation
  onSaved: () => void
}) {
  const { t } = useTranslation()
  const [draft, setDraft] = useState<DeliveryDraft>(() => deliveryRequest(location))
  const [errors, setErrors] = useState<FieldErrors>({})
  const [saved, setSaved] = useState(false)
  const [confirmingDelete, setConfirmingDelete] = useState(false)
  const mutation = useMutation({
    mutationFn: (body: SaveDeliveryLocation) => location?.id
      ? api.put<DeliveryLocation>(`/admin/delivery/locations/${location.id}`, body)
      : api.post<DeliveryLocation>('/admin/delivery/locations', body),
    onSuccess: () => {
      setSaved(true)
      toast.success(tx(t, 'settings.feedback.saved', 'Changes were saved successfully.'))
      onSaved()
    },
  })
  const deleteMutation = useMutation({
    mutationFn: () => api.delete(`/admin/delivery/locations/${location?.id}`),
    onSuccess: () => {
      toast.success(tx(t, 'settings.shipping.deleted', 'The delivery location was deleted.'))
      onSaved()
    },
    onError: () => setConfirmingDelete(false),
  })

  useEffect(() => setDraft(deliveryRequest(location)), [location])

  const change = <Key extends keyof DeliveryDraft>(key: Key, value: DeliveryDraft[Key]) => {
    setDraft((current) => ({ ...current, [key]: value }))
    setErrors((current) => ({ ...current, [key]: '' }))
    setSaved(false)
    mutation.reset()
  }

  const save = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!canWrite) return
    const next: FieldErrors = {}
    for (const field of ['nameArabic', 'nameEnglish'] as const) {
      if (!draft[field]?.trim()) next[field] = tx(t, 'settings.validation.required', 'This field is required.')
    }
    if (draft.additionalDeliveryCost === '' || !Number.isFinite(draft.additionalDeliveryCost)) next.additionalDeliveryCost = tx(t, 'settings.validation.required', 'This field is required.')
    else if (draft.additionalDeliveryCost < 0) next.additionalDeliveryCost = tx(t, 'settings.validation.nonNegative', 'Enter zero or a positive value.')
    setErrors(next)
    if (Object.keys(next).length) return
    setSaved(false)
    mutation.mutate({
      isActive: draft.isActive,
      additionalDeliveryCost: Number(draft.additionalDeliveryCost),
      nameArabic: draft.nameArabic?.trim(),
      nameEnglish: draft.nameEnglish?.trim(),
    })
  }

  return (
    <form className="min-w-0 rounded-xl border p-4" noValidate onSubmit={save}>
      <div className="grid min-w-0 gap-4 md:grid-cols-2 lg:grid-cols-3 [&>label]:min-w-0">
        <label>
          <span className="mb-1 block text-sm font-semibold">{tx(t, 'settings.shipping.locationArabic', 'Location (Arabic)')}</span>
          <input className="field" disabled={!canWrite || mutation.isPending} dir="rtl" maxLength={200} onChange={(event) => change('nameArabic', event.target.value)} required value={draft.nameArabic} />
          <FieldMessage message={errors.nameArabic} />
        </label>
        <label>
          <span className="mb-1 block text-sm font-semibold">{tx(t, 'settings.shipping.locationEnglish', 'Location (English)')}</span>
          <input className="field" disabled={!canWrite || mutation.isPending} dir="ltr" maxLength={200} onChange={(event) => change('nameEnglish', event.target.value)} required value={draft.nameEnglish} />
          <FieldMessage message={errors.nameEnglish} />
        </label>
        <label>
          <span className="mb-1 block text-sm font-semibold">{tx(t, 'settings.shipping.additionalCost', 'Additional delivery cost')}</span>
          <input className="field" dir="ltr" disabled={!canWrite || mutation.isPending} min={0} onChange={(event) => change('additionalDeliveryCost', event.target.value === '' ? '' : Number(event.target.value))} required step="0.01" type="number" value={draft.additionalDeliveryCost} />
          <FieldMessage message={errors.additionalDeliveryCost} />
        </label>
      </div>
      <div className="mt-4 flex min-w-0 flex-wrap gap-4">
        <label className="flex min-h-11 min-w-0 items-center gap-2 rounded-xl border px-4">
          <input checked={draft.isActive} className="shrink-0" disabled={!canWrite || mutation.isPending} onChange={(event) => change('isActive', event.target.checked)} type="checkbox" />
          {tx(t, 'settings.fields.active', 'Active')}
        </label>
      </div>
      <div className="mt-4 grid gap-3">
        {mutation.isError && <MutationError error={mutation.error} />}
        {deleteMutation.isError && <MutationError error={deleteMutation.error} />}
        {saved && <SavedMessage />}
        {canWrite && (
          <div className="flex min-w-0 flex-wrap items-center gap-3">
            <SaveButton saving={mutation.isPending} />
            {location?.id && !confirmingDelete && (
              <button
                className="button-secondary min-w-0 whitespace-normal !border-[var(--heritage-red)] !text-[var(--heritage-red)]"
                onClick={() => setConfirmingDelete(true)}
                type="button"
              >
                <Trash2 aria-hidden size={17} />
                {tx(t, 'settings.shipping.delete', 'Delete location')}
              </button>
            )}
          </div>
        )}
        {canWrite && location?.id && confirmingDelete && (
          <div className="alert alert-error flex-wrap items-center gap-3 font-normal">
            <p className="min-w-0 flex-1 break-words text-sm font-semibold text-[var(--heritage-red)] [overflow-wrap:anywhere]">
              {tx(t, 'settings.shipping.deleteConfirm', 'Delete this delivery location? This cannot be undone.')}
            </p>
            <button
              className="button-secondary min-w-0 whitespace-normal !border-[var(--heritage-red)] !bg-[var(--heritage-red)] !text-white"
              disabled={deleteMutation.isPending}
              onClick={() => deleteMutation.mutate()}
              type="button"
            >
              {deleteMutation.isPending ? tx(t, 'states.saving', 'Saving…') : tx(t, 'settings.shipping.deleteConfirmAction', 'Confirm delete')}
            </button>
            <button
              className="button-secondary min-w-0 whitespace-normal"
              disabled={deleteMutation.isPending}
              onClick={() => setConfirmingDelete(false)}
              type="button"
            >
              {tx(t, 'actions.cancel', 'Cancel')}
            </button>
          </div>
        )}
      </div>
    </form>
  )
}

function ShippingSettingsSection({ permissions }: { permissions: readonly string[] }) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const canWrite = hasPermission(permissions, adminPermissions.deliveryWrite)
  const [adding, setAdding] = useState(false)
  const query = useQuery({
    queryKey: ['admin-delivery-locations'],
    queryFn: () => api.get<DeliveryLocation[]>('/admin/delivery/locations').then((response) => response.data),
  })
  const refresh = () => {
    void queryClient.invalidateQueries({ queryKey: ['admin-delivery-locations'] })
  }

  if (query.isLoading) return <LoadingState />
  if (query.isError) return <ErrorState error={query.error} retry={() => void query.refetch()} />

  return (
    <section className="card min-w-0 p-4 sm:p-6">
      <div className="flex min-w-0 flex-wrap items-start justify-between gap-4">
        <SectionHeading
          icon={<MapPinned aria-hidden size={22} />}
          title={tx(t, 'settings.tabs.shipping', 'Shipping')}
        />
        {canWrite && (
          <button className="button-secondary w-full whitespace-normal sm:w-auto" onClick={() => setAdding((value) => !value)} type="button">
            <Plus aria-hidden className="shrink-0" size={18} />
            {tx(t, 'settings.shipping.addLocation', 'Add location')}
          </button>
        )}
      </div>
      {!canWrite && <div className="mb-5"><ReadOnlyNotice /></div>}
      <div className="grid min-w-0 gap-5">
        {adding && (
          <section>
            <h3 className="mb-3 break-words font-bold [overflow-wrap:anywhere]">{tx(t, 'settings.shipping.newLocation', 'New delivery location')}</h3>
            <DeliveryLocationEditor
              canWrite={canWrite}
              onSaved={() => {
                setAdding(false)
                refresh()
              }}
            />
          </section>
        )}
        {(query.data ?? []).map((location) => (
          <section key={location.id}>
            <h3 className="mb-3 break-words font-bold [overflow-wrap:anywhere]" dir="auto">{location.nameArabic || location.nameEnglish || tx(t, 'settings.shipping.unnamedLocation', 'Delivery location')}</h3>
            <DeliveryLocationEditor canWrite={canWrite} location={location} onSaved={refresh} />
          </section>
        ))}
        {!query.data?.length && !adding && (
          <p className="break-words rounded-xl border border-dashed p-6 text-center text-sm text-[var(--heritage-muted)] [overflow-wrap:anywhere] sm:p-8" role="status">
            {tx(t, 'settings.shipping.empty', 'No delivery locations have been configured.')}
          </p>
        )}
      </div>
    </section>
  )
}

const blankSeo: SaveSeoSettings = {
  baseUrl: '',
  arabicTitle: '',
  englishTitle: '',
  arabicDescription: '',
  englishDescription: '',
  arabicOpenGraphImageUrl: null,
  englishOpenGraphImageUrl: null,
  robotsAdditionalRules: null,
}

function SeoSettingsSection({ permissions }: { permissions: readonly string[] }) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const canWrite = hasPermission(permissions, adminPermissions.seoWrite)
  const canUpload = canWrite && hasPermission(permissions, adminPermissions.filesUpload)
  const [draft, setDraft] = useState<SaveSeoSettings>(blankSeo)
  const [errors, setErrors] = useState<FieldErrors>({})
  const [saved, setSaved] = useState(false)
  const [uploading, setUploading] = useState<'arabicOpenGraphImageUrl' | 'englishOpenGraphImageUrl' | null>(null)
  const [uploadError, setUploadError] = useState<unknown>(null)
  const query = useQuery({
    queryKey: ['admin-seo-settings'],
    queryFn: () => api.get<SeoSettings>('/admin/seo').then((response) => response.data),
  })
  const mutation = useMutation({
    mutationFn: (body: SaveSeoSettings) => api.put<SeoSettings>('/admin/seo', body).then((response) => response.data),
    onSuccess: (data) => {
      queryClient.setQueryData(['admin-seo-settings'], data)
      setSaved(true)
      toast.success(tx(t, 'settings.feedback.saved', 'Changes were saved successfully.'))
    },
  })

  useEffect(() => {
    if (!query.data) return
    setDraft({
      baseUrl: query.data.baseUrl ?? '',
      arabicTitle: query.data.arabicTitle ?? '',
      englishTitle: query.data.englishTitle ?? '',
      arabicDescription: query.data.arabicDescription ?? '',
      englishDescription: query.data.englishDescription ?? '',
      arabicOpenGraphImageUrl: query.data.arabicOpenGraphImageUrl ?? null,
      englishOpenGraphImageUrl: query.data.englishOpenGraphImageUrl ?? null,
      robotsAdditionalRules: query.data.robotsAdditionalRules ?? null,
    })
  }, [query.data])

  const change = <Key extends keyof SaveSeoSettings>(key: Key, value: SaveSeoSettings[Key]) => {
    setDraft((current) => ({ ...current, [key]: value }))
    setErrors((current) => ({ ...current, [key]: '' }))
    setSaved(false)
    mutation.reset()
  }

  const upload = async (
    field: 'arabicOpenGraphImageUrl' | 'englishOpenGraphImageUrl',
    file?: File,
  ) => {
    if (!file || !canUpload) return
    const validation = validateAdminImage(file)
    if (validation) {
      setErrors((current) => ({
        ...current,
        [field]: t(validation === 'size' ? 'uploads.errors.size' : 'uploads.errors.type'),
      }))
      return
    }
    setUploading(field)
    setUploadError(null)
    try {
      const response = await uploadAdminImage(file, 'content')
      change(field, response.url)
    } catch (error) {
      setUploadError(error)
    } finally {
      setUploading(null)
    }
  }

  const save = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!canWrite) return
    const next: FieldErrors = {}
    if (!draft.baseUrl?.trim() || !httpUrl(draft.baseUrl.trim())) next.baseUrl = tx(t, 'settings.validation.httpUrl', 'Enter a valid HTTP(S) URL.')
    if (!draft.arabicTitle?.trim()) next.arabicTitle = tx(t, 'settings.validation.required', 'This field is required.')
    if (!draft.englishTitle?.trim()) next.englishTitle = tx(t, 'settings.validation.required', 'This field is required.')
    if ((draft.arabicTitle?.length ?? 0) > 300) next.arabicTitle = tx(t, 'settings.validation.maxLength', 'Must be no more than {{count}} characters.', { count: 300 })
    if ((draft.englishTitle?.length ?? 0) > 300) next.englishTitle = tx(t, 'settings.validation.maxLength', 'Must be no more than {{count}} characters.', { count: 300 })
    if ((draft.arabicDescription?.length ?? 0) > 1000) next.arabicDescription = tx(t, 'settings.validation.maxLength', 'Must be no more than {{count}} characters.', { count: 1000 })
    if ((draft.englishDescription?.length ?? 0) > 1000) next.englishDescription = tx(t, 'settings.validation.maxLength', 'Must be no more than {{count}} characters.', { count: 1000 })
    if ((draft.robotsAdditionalRules?.length ?? 0) > 4000) next.robotsAdditionalRules = tx(t, 'settings.validation.maxLength', 'Must be no more than {{count}} characters.', { count: 4000 })
    setErrors(next)
    if (Object.keys(next).length) return
    setSaved(false)
    mutation.mutate({
      ...draft,
      baseUrl: draft.baseUrl?.trim(),
      arabicTitle: draft.arabicTitle?.trim(),
      englishTitle: draft.englishTitle?.trim(),
      arabicDescription: draft.arabicDescription?.trim() ?? '',
      englishDescription: draft.englishDescription?.trim() ?? '',
      arabicOpenGraphImageUrl: draft.arabicOpenGraphImageUrl?.trim() || null,
      englishOpenGraphImageUrl: draft.englishOpenGraphImageUrl?.trim() || null,
      robotsAdditionalRules: draft.robotsAdditionalRules?.trim() || null,
    })
  }

  if (query.isLoading) return <LoadingState />
  if (query.isError) return <ErrorState error={query.error} retry={() => void query.refetch()} />

  return (
    <form className="card min-w-0 p-4 sm:p-6" onSubmit={save}>
      <SectionHeading
        icon={<SearchCheck aria-hidden size={22} />}
        title={tx(t, 'settings.tabs.seo', 'SEO')}
      />
      {!canWrite && <div className="mb-5"><ReadOnlyNotice /></div>}
      <fieldset className="grid min-w-0 gap-4 md:grid-cols-2 [&>label]:min-w-0" disabled={!canWrite || mutation.isPending}>
        <label className="md:col-span-2">
          <span className="mb-1 block text-sm font-semibold">{tx(t, 'settings.seo.baseUrl', 'Store base URL')}</span>
          <input className="field" dir="ltr" maxLength={1000} onChange={(event) => change('baseUrl', event.target.value)} placeholder="https://example.com" value={draft.baseUrl ?? ''} />
          <FieldMessage message={errors.baseUrl} />
        </label>
        <label>
          <span className="mb-1 block text-sm font-semibold">{tx(t, 'settings.seo.titleArabic', 'Default title (Arabic)')}</span>
          <input className="field" dir="rtl" maxLength={300} onChange={(event) => change('arabicTitle', event.target.value)} value={draft.arabicTitle ?? ''} />
          <FieldMessage message={errors.arabicTitle} />
        </label>
        <label>
          <span className="mb-1 block text-sm font-semibold">{tx(t, 'settings.seo.titleEnglish', 'Default title (English)')}</span>
          <input className="field" dir="ltr" maxLength={300} onChange={(event) => change('englishTitle', event.target.value)} value={draft.englishTitle ?? ''} />
          <FieldMessage message={errors.englishTitle} />
        </label>
        <label>
          <span className="mb-1 block text-sm font-semibold">{tx(t, 'settings.seo.descriptionArabic', 'Default description (Arabic)')}</span>
          <textarea className="field min-h-28" dir="rtl" maxLength={1000} onChange={(event) => change('arabicDescription', event.target.value)} value={draft.arabicDescription ?? ''} />
          <FieldMessage message={errors.arabicDescription} />
        </label>
        <label>
          <span className="mb-1 block text-sm font-semibold">{tx(t, 'settings.seo.descriptionEnglish', 'Default description (English)')}</span>
          <textarea className="field min-h-28" dir="ltr" maxLength={1000} onChange={(event) => change('englishDescription', event.target.value)} value={draft.englishDescription ?? ''} />
          <FieldMessage message={errors.englishDescription} />
        </label>
        <ImageFilePicker
          currentUrl={draft.arabicOpenGraphImageUrl}
          disabled={!canUpload || Boolean(uploading)}
          error={errors.arabicOpenGraphImageUrl}
          label={tx(t, 'settings.seo.ogImageArabic', 'Arabic sharing image')}
          onError={(message) => setErrors((current) => ({ ...current, arabicOpenGraphImageUrl: message }))}
          onSelect={(file) => void upload('arabicOpenGraphImageUrl', file ?? undefined)}
        />
        <ImageFilePicker
          currentUrl={draft.englishOpenGraphImageUrl}
          disabled={!canUpload || Boolean(uploading)}
          error={errors.englishOpenGraphImageUrl}
          label={tx(t, 'settings.seo.ogImageEnglish', 'English sharing image')}
          onError={(message) => setErrors((current) => ({ ...current, englishOpenGraphImageUrl: message }))}
          onSelect={(file) => void upload('englishOpenGraphImageUrl', file ?? undefined)}
        />
        <label className="md:col-span-2">
          <span className="mb-1 block text-sm font-semibold">{tx(t, 'settings.seo.robotsRules', 'Additional robots.txt rules')}</span>
          <textarea className="field min-h-36 font-mono text-sm" dir="ltr" maxLength={4000} onChange={(event) => change('robotsAdditionalRules', event.target.value)} value={draft.robotsAdditionalRules ?? ''} />
          <FieldMessage message={errors.robotsAdditionalRules} />
        </label>
      </fieldset>
      <div className="mt-6 grid min-w-0 gap-3">
        {uploadError && <MutationError error={uploadError} />}
        {mutation.isError && <MutationError error={mutation.error} />}
        {saved && <SavedMessage />}
        {canWrite && <div><SaveButton disabled={Boolean(uploading)} saving={mutation.isPending} /></div>}
      </div>
    </form>
  )
}

type SettingsTab = 'general' | 'contact' | 'social' | 'payments' | 'shipping' | 'seo'
type TabDefinition = {
  id: SettingsTab
  labelKey: string
  fallback: string
  icon: ReactNode
  readPermission: string
}

const tabs: readonly TabDefinition[] = [
  { id: 'general', labelKey: 'settings.tabs.general', fallback: 'General', icon: <Settings2 aria-hidden size={17} />, readPermission: adminPermissions.settingsRead },
  { id: 'contact', labelKey: 'settings.tabs.contact', fallback: 'Contact', icon: <ContactRound aria-hidden size={17} />, readPermission: adminPermissions.settingsRead },
  { id: 'social', labelKey: 'settings.tabs.social', fallback: 'Social media', icon: <Link2 aria-hidden size={17} />, readPermission: adminPermissions.socialRead },
  { id: 'payments', labelKey: 'settings.tabs.payments', fallback: 'Payments', icon: <CircleDollarSign aria-hidden size={17} />, readPermission: adminPermissions.paymentsRead },
  { id: 'shipping', labelKey: 'settings.tabs.shipping', fallback: 'Shipping', icon: <MapPinned aria-hidden size={17} />, readPermission: adminPermissions.deliveryRead },
  { id: 'seo', labelKey: 'settings.tabs.seo', fallback: 'SEO', icon: <Globe2 aria-hidden size={17} />, readPermission: adminPermissions.seoRead },
]

const urlTab = () => {
  if (typeof window === 'undefined') return undefined
  const value = (
    new URLSearchParams(window.location.search).get('tab')
    ?? window.location.hash.replace(/^#/, '')
  ).toLowerCase()
  return tabs.some((tab) => tab.id === value) ? value as SettingsTab : undefined
}

export function AdminSettingsPage() {
  const { i18n, t } = useTranslation()
  const permissions = useSessionStore((state) => state.admin?.permissions ?? [])
  const visibleTabs = useMemo(
    () => tabs.filter((tab) => hasPermission(permissions, tab.readPermission)),
    [permissions],
  )
  const [activeTab, setActiveTab] = useState<SettingsTab>(() => urlTab() ?? 'general')

  useEffect(() => {
    const selectUrlTab = () => {
      const requested = urlTab()
      if (requested) setActiveTab(requested)
    }
    window.addEventListener('hashchange', selectUrlTab)
    window.addEventListener('popstate', selectUrlTab)
    return () => {
      window.removeEventListener('hashchange', selectUrlTab)
      window.removeEventListener('popstate', selectUrlTab)
    }
  }, [])

  useEffect(() => {
    if (visibleTabs.some((tab) => tab.id === activeTab)) return
    if (visibleTabs[0]) setActiveTab(visibleTabs[0].id)
  }, [activeTab, visibleTabs])

  if (!visibleTabs.length) return <ForbiddenState />
  const activeDefinition = visibleTabs.find((tab) => tab.id === activeTab) ?? visibleTabs[0]
  const selectTab = (tab: SettingsTab) => {
    setActiveTab(tab)
    if (typeof window !== 'undefined') {
      const params = new URLSearchParams(window.location.search)
      params.delete('tab')
      const query = params.size ? `?${params.toString()}` : ''
      const nextUrl = `${window.location.pathname}${query}#${tab}`
      window.history.replaceState(window.history.state, '', nextUrl)
    }
  }
  const handleTabKeyDown = (event: KeyboardEvent<HTMLButtonElement>, index: number) => {
    let nextIndex: number | undefined
    if (event.key === 'Home') nextIndex = 0
    else if (event.key === 'End') nextIndex = visibleTabs.length - 1
    else if (event.key === 'ArrowRight') {
      nextIndex = (index + (i18n.dir() === 'rtl' ? -1 : 1) + visibleTabs.length) % visibleTabs.length
    } else if (event.key === 'ArrowLeft') {
      nextIndex = (index + (i18n.dir() === 'rtl' ? 1 : -1) + visibleTabs.length) % visibleTabs.length
    }
    if (nextIndex === undefined) return
    event.preventDefault()
    const next = visibleTabs[nextIndex]
    selectTab(next.id)
    window.requestAnimationFrame(() => document.getElementById(`settings-tab-${next.id}`)?.focus())
  }

  return (
    <section className="min-w-0">
      <h1 className="mb-6 min-w-0 break-words text-3xl font-extrabold [overflow-wrap:anywhere]">{tx(t, 'settings.title', 'Settings')}</h1>
      <div className="mb-6 min-w-0 pb-1">
        <div aria-label={tx(t, 'settings.tabs.label', 'Settings sections')} className="flex min-w-0 flex-wrap gap-2" role="tablist">
          {visibleTabs.map((tab, index) => (
            <button
              aria-controls={`settings-panel-${tab.id}`}
              aria-selected={activeDefinition.id === tab.id}
              className={`inline-flex min-h-11 min-w-0 items-center gap-2 whitespace-normal rounded-xl border px-4 text-sm font-bold transition-colors ${
                activeDefinition.id === tab.id
                  ? 'border-[var(--heritage-burgundy)] bg-[var(--heritage-burgundy)] text-white'
                  : 'border-stone-200 bg-white text-[var(--heritage-burgundy)] hover:border-[var(--heritage-burgundy)]'
              }`}
              id={`settings-tab-${tab.id}`}
              key={tab.id}
              onClick={() => selectTab(tab.id)}
              onKeyDown={(event) => handleTabKeyDown(event, index)}
              role="tab"
              tabIndex={activeDefinition.id === tab.id ? 0 : -1}
              type="button"
            >
              {tab.icon}
              {tx(t, tab.labelKey, tab.fallback)}
            </button>
          ))}
        </div>
      </div>
      <div
        aria-labelledby={`settings-tab-${activeDefinition.id}`}
        className="min-w-0"
        id={`settings-panel-${activeDefinition.id}`}
        role="tabpanel"
      >
        {activeDefinition.id === 'general' && <GeneralSettingsSection permissions={permissions} />}
        {activeDefinition.id === 'contact' && <ContactSettingsSection permissions={permissions} />}
        {activeDefinition.id === 'social' && <SocialSettingsSection permissions={permissions} />}
        {activeDefinition.id === 'payments' && <PaymentSettingsSection permissions={permissions} />}
        {activeDefinition.id === 'shipping' && <ShippingSettingsSection permissions={permissions} />}
        {activeDefinition.id === 'seo' && <SeoSettingsSection permissions={permissions} />}
      </div>
    </section>
  )
}
