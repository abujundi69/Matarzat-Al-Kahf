import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { BrandLogo } from '@/components/common/BrandLogo'

const HOLD_MS = 900
const EXIT_MS = 320

/**
 * One-time branded intro shown while the app first mounts. Renders as a full-viewport
 * overlay on top of the router (which mounts and starts fetching immediately underneath),
 * so there is never a layout shift or a flash of page content before it fades in.
 *
 * The overlay makes the application inert while it is up, so it is deliberately short and any
 * key press, tap, or scroll dismisses it at once: someone who starts typing their phone number
 * straight away must not have those keystrokes swallowed.
 */
export function SplashScreen({ onFinished }: { onFinished: () => void }) {
  const { t } = useTranslation()
  const [exiting, setExiting] = useState(false)

  useEffect(() => {
    const holdTimer = window.setTimeout(() => setExiting(true), HOLD_MS)
    const skip = () => setExiting(true)
    const events = ['keydown', 'pointerdown', 'wheel', 'touchstart'] as const
    for (const event of events) window.addEventListener(event, skip, { passive: true })
    return () => {
      window.clearTimeout(holdTimer)
      for (const event of events) window.removeEventListener(event, skip)
    }
  }, [])

  useEffect(() => {
    if (!exiting) return
    const exitTimer = window.setTimeout(onFinished, EXIT_MS)
    return () => window.clearTimeout(exitTimer)
  }, [exiting, onFinished])

  return (
    <div aria-live="polite" className={`splash-screen ${exiting ? 'splash-screen-exit' : ''}`} role="status">
      <div aria-hidden className="stitch-field absolute inset-0" />
      <div aria-hidden className="tatreez-band tatreez-band-lg absolute inset-x-0 top-0" />
      <div aria-hidden className="tatreez-band tatreez-band-lg absolute inset-x-0 bottom-0" />
      <div className="relative flex min-w-0 flex-col items-center gap-6 px-6 text-center">
        <BrandLogo className="splash-logo size-36 drop-shadow-[0_18px_40px_rgb(0_0_0_/_0.45)] sm:size-44" variant="md" />
        <div className="min-w-0">
          <p className="splash-brand-text [overflow-wrap:anywhere] text-2xl font-extrabold text-white sm:text-3xl">{t('brand')}</p>
          <p className="splash-tagline-text mt-2 [overflow-wrap:anywhere] text-sm text-[var(--heritage-sand)] sm:text-base">{t('tagline')}</p>
        </div>
        <span className="sr-only">{t('splash.loading')}</span>
        <div className="splash-progress-track mt-2 h-1 w-44 rounded-full bg-white/15 sm:w-52">
          <div className="splash-progress-fill h-full rounded-full" />
        </div>
      </div>
    </div>
  )
}
