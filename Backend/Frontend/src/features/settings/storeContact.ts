import { useQuery } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { api } from '@/lib/api/client'
import type { Schema } from '@/types/models'

type StoreSetting = Schema<'StoreSettingResponse'>

export const storeContactQueryKey = ['store-contact'] as const

const valueFor = (settings: readonly StoreSetting[], key: string) =>
  settings.find((setting) => setting.key?.toLowerCase() === key.toLowerCase())?.value?.trim() ?? ''

/** The public contact details an administrator saves under Settings > Contact. */
export function useStoreContact() {
  const { i18n } = useTranslation()
  const query = useQuery({
    // The address is localized by the API, so the cache is kept per language.
    queryKey: [...storeContactQueryKey, i18n.language],
    queryFn: () => api.get<StoreSetting[]>('/settings/Contact').then((response) => response.data),
    staleTime: 5 * 60_000,
  })
  const settings = Array.isArray(query.data) ? query.data : []
  return {
    phone: valueFor(settings, 'contact.phone'),
    whatsapp: valueFor(settings, 'contact.whatsapp'),
    email: valueFor(settings, 'contact.email'),
    address: valueFor(settings, 'contact.address'),
  }
}
