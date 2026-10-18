import { Languages } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { markLanguageChosen } from '@/features/settings/storeBranding'

export function LanguageSwitcher({ compact = false }: { compact?: boolean }) {
  const { i18n, t } = useTranslation()
  return <button
    aria-label={t('languageSwitch')}
    className={`${compact ? '!size-11 !p-0 sm:!w-auto sm:!px-3' : '!min-h-10 max-w-full !px-3'} button-secondary shrink-0`}
    onClick={() => {
      markLanguageChosen()
      void i18n.changeLanguage(i18n.language.startsWith('ar') ? 'en' : 'ar')
    }}
    type="button"
  >
    <Languages aria-hidden className="shrink-0" size={compact ? 20 : 17} />
    <span className={`${compact ? 'sr-only sm:not-sr-only' : ''} min-w-0 leading-snug [overflow-wrap:anywhere]`}>{t('language')}</span>
  </button>
}
