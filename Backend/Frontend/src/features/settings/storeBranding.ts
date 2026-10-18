import { useEffect } from 'react'
import { useQuery } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { api } from '@/lib/api/client'
import { isGenericPlaceholderUrl, resolveMediaUrl } from '@/lib/media'
import type { Schema } from '@/types/models'

type StoreSetting = Schema<'StoreSettingResponse'>

const valueFor = (settings: readonly StoreSetting[] | undefined, key: string) =>
  settings?.find((setting) => setting.key?.toLowerCase() === key.toLowerCase())?.value ?? ''

export const storeBrandingQueryKey = ['store-branding'] as const

const languageChoiceKey = 'matarzat-language-chosen'

/** Records that the visitor picked a language, so the store default no longer overrides it. */
export const markLanguageChosen = () => {
  try {
    localStorage.setItem(languageChoiceKey, '1')
  } catch {
    // Storage can be unavailable (private mode); the store default then keeps applying.
  }
}

const hasChosenLanguage = () => {
  try {
    return localStorage.getItem(languageChoiceKey) === '1'
  } catch {
    return false
  }
}

export function useStoreBranding() {
  const { i18n } = useTranslation()
  const query = useQuery({
    // The store name is localized by the API, so the cache is kept per language.
    queryKey: [...storeBrandingQueryKey, i18n.language],
    queryFn: () => api.get<StoreSetting[]>('/settings/General').then((response) => response.data),
    staleTime: 5 * 60_000,
  })
  const settings = Array.isArray(query.data) ? query.data : []
  // An owner-uploaded logo or icon overrides the bundled brand; the generic placeholder does not count.
  const configured = (key: string) => {
    const url = resolveMediaUrl(valueFor(settings, key))
    return isGenericPlaceholderUrl(url) ? '' : url
  }
  const logoUrl = configured('store.logoUrl')
  const faviconUrl = configured('store.faviconUrl')
  const storeName = valueFor(settings, 'store.name').trim()
  const defaultLanguage = valueFor(settings, 'store.defaultLanguage')

  useEffect(() => {
    if (!faviconUrl) return
    let icon = document.querySelector<HTMLLinkElement>('link[rel~="icon"]')
    if (!icon) {
      icon = document.createElement('link')
      icon.rel = 'icon'
      document.head.append(icon)
    }
    icon.href = faviconUrl
  }, [faviconUrl])

  useEffect(() => {
    // The owner's default language only applies until the visitor chooses one themselves.
    if (!defaultLanguage || hasChosenLanguage()) return
    const code = defaultLanguage.toLowerCase().startsWith('en') ? 'en' : 'ar'
    if (!i18n.language.startsWith(code)) void i18n.changeLanguage(code)
  }, [defaultLanguage, i18n])

  return { faviconUrl, logoUrl, storeName }
}
