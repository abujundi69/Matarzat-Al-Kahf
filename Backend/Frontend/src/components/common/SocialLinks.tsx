import { Globe } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api/client'
import { safeExternalUrl } from '@/lib/utils'
import type { Schema } from '@/types/models'

type SocialLink = Schema<'SocialLinkResponse'>
type SocialPlatform = Schema<'SocialPlatform'>

const iconProps = { 'aria-hidden': true, focusable: false, height: 19, viewBox: '0 0 24 24', width: 19 } as const

function BrandIcon({ platform }: { platform: SocialPlatform }) {
  switch (platform) {
    case 'Facebook':
      return (
        <svg {...iconProps} fill="currentColor">
          <path d="M22 12.06C22 6.505 17.523 2 12 2S2 6.505 2 12.06c0 5.02 3.657 9.184 8.438 9.94v-7.03H7.898v-2.91h2.54V9.845c0-2.522 1.492-3.915 3.777-3.915 1.094 0 2.238.197 2.238.197v2.475h-1.26c-1.243 0-1.63.775-1.63 1.57v1.888h2.773l-.443 2.91h-2.33V22c4.78-.756 8.437-4.92 8.437-9.94Z" />
        </svg>
      )
    case 'Instagram':
      return (
        <svg {...iconProps} fill="none" stroke="currentColor" strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.8}>
          <rect height="18" rx="5" width="18" x="3" y="3" />
          <circle cx="12" cy="12" r="4.2" />
          <circle cx="17.3" cy="6.7" fill="currentColor" r="1.1" stroke="none" />
        </svg>
      )
    case 'WhatsApp':
      return (
        <svg {...iconProps} fill="currentColor">
          <path d="M12.01 2C6.49 2 2 6.48 2 12c0 1.86.5 3.6 1.38 5.1L2 22l5.06-1.33A9.94 9.94 0 0 0 12.01 22C17.53 22 22 17.52 22 12S17.53 2 12.01 2Zm5.63 14.16c-.24.68-1.4 1.3-1.93 1.35-.5.05-1.02.24-3.42-.72-2.9-1.16-4.77-4.07-4.92-4.26-.14-.19-1.17-1.56-1.17-2.98 0-1.42.74-2.11 1-2.4.26-.29.57-.36.76-.36.19 0 .38 0 .55.01.18.01.42-.07.66.5.24.58.83 2 .9 2.14.07.15.12.32.02.51-.1.19-.15.31-.3.48-.14.17-.31.38-.44.51-.15.15-.3.31-.13.6.17.29.76 1.25 1.63 2.02 1.12 1 2.06 1.31 2.35 1.46.29.15.46.13.63-.05.17-.19.72-.84.91-1.12.19-.29.38-.24.63-.14.26.1 1.65.78 1.93.92.29.15.48.22.55.34.07.13.07.72-.17 1.4Z" />
        </svg>
      )
    case 'TikTok':
      return (
        <svg {...iconProps} fill="currentColor">
          <path d="M16.5 2h-3.2v13.6a2.7 2.7 0 1 1-2.7-2.9c.24 0 .48.02.7.07V9.5a6.1 6.1 0 1 0 5.2 6.04V8.62a7.9 7.9 0 0 0 4.5 1.4V6.85a4.6 4.6 0 0 1-4.5-4.5Z" />
        </svg>
      )
    case 'YouTube':
      return (
        <svg {...iconProps} fill="currentColor">
          <path d="M21.6 7.2s-.21-1.49-.87-2.15c-.83-.87-1.76-.87-2.18-.92C15.6 4 12 4 12 4h-.01s-3.6 0-6.55.13c-.42.05-1.35.05-2.18.92C2.6 5.71 2.4 7.2 2.4 7.2S2.19 8.94 2.19 10.68v1.63c0 1.74.21 3.48.21 3.48s.21 1.49.86 2.15c.83.87 1.92.84 2.4.94C7.4 19.03 12 19.07 12 19.07s3.61-.01 6.55-.13c.42-.05 1.35-.05 2.18-.92.65-.66.87-2.15.87-2.15s.21-1.74.21-3.48v-1.63c0-1.74-.21-3.48-.21-3.48ZM9.96 14.5V8.9l5.4 2.81-5.4 2.8Z" />
        </svg>
      )
    case 'X':
      return (
        <svg {...iconProps} fill="currentColor">
          <path d="M13.6 10.6 20.4 3h-2.1l-5.9 6.6L7.9 3H2.5l7.1 10.1L2.5 21h2.1l6.2-6.9L15.9 21h5.4l-7.7-10.4Zm-2.2 2.5-.7-1L5.1 4.5h2l4.6 6.5.7 1 6 8.4h-2l-4.9-6.9Z" />
        </svg>
      )
    case 'LinkedIn':
      return (
        <svg {...iconProps} fill="currentColor">
          <path d="M6.94 8.5H3.56V20h3.38V8.5ZM5.25 3.5a1.96 1.96 0 1 0 0 3.92 1.96 1.96 0 0 0 0-3.92ZM20.45 20h-3.37v-5.6c0-1.34-.03-3.06-1.87-3.06-1.87 0-2.16 1.46-2.16 2.96V20H9.68V8.5h3.24v1.57h.05c.45-.85 1.55-1.75 3.2-1.75 3.42 0 4.05 2.25 4.05 5.18V20Z" />
        </svg>
      )
    case 'Telegram':
      return (
        <svg {...iconProps} fill="currentColor">
          <path d="m21.4 3.5-3.02 15.13c-.23 1.02-.83 1.27-1.68.79l-4.64-3.42-2.24 2.16c-.25.25-.46.46-.94.46l.34-4.78 8.7-7.86c.38-.34-.08-.53-.58-.19L7.02 12.5l-4.6-1.44c-1-.31-1.02-1 .21-1.48l17.98-6.93c.83-.31 1.56.2 1.79.85Z" />
        </svg>
      )
    default:
      return <Globe {...iconProps} fill="none" stroke="currentColor" strokeWidth={1.8} />
  }
}

export function SocialLinks({ className }: { className?: string }) {
  const { t } = useTranslation()
  const query = useQuery({
    queryKey: ['social-links'],
    queryFn: () => api.get<SocialLink[]>('/social-links').then((response) => response.data),
    staleTime: 5 * 60_000,
  })
  const links = (Array.isArray(query.data) ? query.data : []).flatMap((link) => {
    if (!link.url || !link.platform) return []
    try {
      return [{ ...link, url: safeExternalUrl(link.url) }]
    } catch {
      return []
    }
  })

  if (!links.length) return null

  return (
    <div className={className}>
      <h3 className="font-bold text-[var(--heritage-gold)]">{t('footer.social.title')}</h3>
      <ul className="mt-3 flex flex-wrap gap-2">
        {links.map((link, index) => {
          const platformName = t(`settings.social.platforms.${link.platform}`, { defaultValue: link.platform })
          return (
            <li key={link.id ?? `${link.platform}-${index}`}>
              <a
                aria-label={t('footer.social.visit', { platform: platformName })}
                className="grid size-11 place-items-center rounded-full bg-white/10 text-white/80 transition-colors hover:bg-[var(--heritage-gold)] hover:text-[var(--heritage-charcoal)]"
                href={link.url}
                rel="noopener noreferrer"
                target="_blank"
                title={platformName}
              >
                <BrandIcon platform={link.platform as SocialPlatform} />
              </a>
            </li>
          )
        })}
      </ul>
    </div>
  )
}
