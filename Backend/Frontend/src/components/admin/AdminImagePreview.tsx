import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ImagePlaceholder } from '@/components/common/ImagePlaceholder'
import { resolveMediaUrl } from '@/lib/media'

export function AdminImagePreview({
  alt,
  className = 'size-20',
  compact = false,
  src,
}: {
  alt: string
  className?: string
  /** List-row thumbnail: cropped to fill, no border, no caption on the placeholder. */
  compact?: boolean
  src?: string | null
}) {
  const { t } = useTranslation()
  const [failedUrl, setFailedUrl] = useState('')
  const url = resolveMediaUrl(src)

  return (
    <div className={`grid overflow-hidden ${compact ? 'rounded-lg bg-[var(--heritage-beige)]' : 'rounded-xl border bg-stone-50'} ${className}`}>
      {url && failedUrl !== url ? (
        <img alt={alt} className={`size-full min-h-0 ${compact ? 'object-cover' : 'object-contain'}`} onError={() => setFailedUrl(url)} src={url} />
      ) : (
        <div aria-label={t('uploads.noImage')} className={`relative size-full ${compact ? '' : 'min-h-20'}`} role="img">
          <ImagePlaceholder />
          {!compact && <span className="absolute inset-x-1 bottom-1 rounded bg-white/90 px-1 text-center text-xs font-normal text-[var(--heritage-muted)]">
            {t('uploads.noImage')}
          </span>}
        </div>
      )}
    </div>
  )
}
