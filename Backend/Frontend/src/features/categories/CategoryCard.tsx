import { useState, type ElementType } from 'react'
import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { ImagePlaceholder } from '@/components/common/ImagePlaceholder'
import { isGenericPlaceholderUrl, resolveMediaUrl } from '@/lib/media'
import type { BrowsableCategory } from './categoryApi'

type CategoryCardProps = {
  category: BrowsableCategory
  headingLevel?: 'h2' | 'h3'
}

export function CategoryCard({ category, headingLevel = 'h3' }: CategoryCardProps) {
  const { t } = useTranslation()
  const [failedImageUrl, setFailedImageUrl] = useState<string>()
  const Heading = headingLevel as ElementType
  const target = `/products?categoryId=${encodeURIComponent(category.id)}`
  const hasImage = Boolean(category.imageUrl) && !isGenericPlaceholderUrl(category.imageUrl) && failedImageUrl !== category.imageUrl

  return (
    <article className="category-card group flex h-full min-w-0">
      <Link
        aria-label={t('categories.browseNamed', { name: category.name })}
        className="flex min-w-0 flex-1 flex-col items-center gap-3 rounded-xl p-2 text-center no-underline"
        to={target}
      >
        <span className="relative grid shrink-0 place-items-center">
          <span className="size-24 overflow-hidden rounded-full bg-[var(--heritage-beige)] sm:size-28 lg:size-32">
            {hasImage
              ? <img
                  alt=""
                  className="size-full object-cover transition duration-500 group-hover:scale-105 motion-reduce:transform-none motion-reduce:transition-none"
                  loading="lazy"
                  onError={() => setFailedImageUrl(category.imageUrl ?? '')}
                  src={resolveMediaUrl(category.imageUrl) || undefined}
                />
              : <div
                  aria-label={t('categories.imageFallback', { name: category.name })}
                  className="size-full"
                  data-testid="category-image-fallback"
                  role="img"
                >
                  <ImagePlaceholder />
                </div>}
          </span>
        </span>
        <Heading className="max-w-full [overflow-wrap:anywhere] text-sm font-bold leading-snug text-[var(--heritage-charcoal)] transition-colors group-hover:text-[var(--heritage-burgundy)] group-focus-within:text-[var(--heritage-burgundy)] sm:text-base" dir="auto">
          {category.name}
        </Heading>
      </Link>
    </article>
  )
}
