import { useState } from 'react'
import { isGenericPlaceholderUrl, resolveMediaUrl } from '@/lib/media'

type CategoryFilterThumbProps = {
  imageUrl?: string | null
}

/** Small decorative circular thumbnail used beside category names in the products filter. */
export function CategoryFilterThumb({ imageUrl }: CategoryFilterThumbProps) {
  const [failedImageUrl, setFailedImageUrl] = useState<string>()
  const hasImage = Boolean(imageUrl) && !isGenericPlaceholderUrl(imageUrl) && failedImageUrl !== imageUrl

  return (
    <span className="size-6 shrink-0 overflow-hidden rounded-full bg-[var(--heritage-beige)] ring-1 ring-[var(--heritage-line-strong)]">
      {hasImage
        ? <img
            alt=""
            className="size-full object-cover"
            loading="lazy"
            onError={() => setFailedImageUrl(imageUrl ?? '')}
            src={resolveMediaUrl(imageUrl) || undefined}
          />
        : <span aria-hidden className="placeholder-art block size-full" />}
    </span>
  )
}
