import type { TFunction } from 'i18next'

type RoleValue = string | null | undefined
type LocalizedRoleValue = {
  displayNameArabic?: RoleValue
  displayNameEnglish?: RoleValue
  name?: RoleValue
}

const roleParts = (role: RoleValue | LocalizedRoleValue) => {
  if (typeof role === 'string') {
    return { displayNameArabic: undefined, displayNameEnglish: undefined, name: role.trim() }
  }
  if (!role) {
    return { displayNameArabic: undefined, displayNameEnglish: undefined, name: undefined }
  }
  return {
    displayNameArabic: role.displayNameArabic?.trim(),
    displayNameEnglish: role.displayNameEnglish?.trim(),
    name: role.name?.trim(),
  }
}

/**
 * Keeps the canonical role identifier supplied by the API separate from its
 * visible label. System identifiers use the application dictionary; custom
 * roles use their bilingual display-name fields and never leak the other
 * language as a fallback.
 */
export const localizeRoleName = (
  t: TFunction,
  role: RoleValue | LocalizedRoleValue,
  language: string,
) => {
  const { displayNameArabic, displayNameEnglish, name } = roleParts(role)
  if (!name) return t('access.roles.unnamed')

  const targetLanguage = language.startsWith('ar') ? 'ar' : 'en'
  const systemName = t(`access.roleNames.${name}`, { defaultValue: '', lng: targetLanguage }).trim()
  if (systemName) return systemName

  const localizedName = targetLanguage === 'ar' ? displayNameArabic : displayNameEnglish
  return localizedName || t('access.roles.missingTranslation', { lng: targetLanguage })
}
