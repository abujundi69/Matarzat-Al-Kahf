import { useState, type ElementType } from 'react'
import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { ImagePlaceholder } from '@/components/common/ImagePlaceholder'
import { isGenericPlaceholderUrl, resolveMediaUrl } from '@/lib/media'
import type { BrowsableCategory } from './categoryApi'

type CategoryShowcaseCardProps = {
  category: BrowsableCategory
  headingLevel?: 'h2' | 'h3'
}

/**
 * Grid card for the dedicated Categories page only: same image-over-name shape as ProductCard.
 * The Home page keeps the separate circular CategoryCard — do not merge these.
 */
export function CategoryShowcaseCard({ category, headingLevel = 'h2' }: CategoryShowcaseCardProps) {
  const { t } = useTranslation()
  const [failedImageUrl, setFailedImageUrl] = useState<string>()
  const Heading = headingLevel as ElementType
  const target = `/products?categoryId=${encodeURIComponent(category.id)}`
  const hasImage = Boolean(category.imageUrl) && !isGenericPlaceholderUrl(category.imageUrl) && failedImageUrl !== category.imageUrl

  return (
    <article className="category-showcase-card group flex h-full min-w-0">
      <Link
        aria-label={t('categories.browseNamed', { name: category.name })}
        className="flex min-w-0 flex-1 flex-col gap-3 rounded-xl no-underline"
        to={target}
      >
        <span className="block aspect-[4/5] shrink-0 overflow-hidden rounded-xl bg-[var(--heritage-beige)]">
          {hasImage
            ? <img
                alt=""
                className="size-full object-cover transition duration-500 group-hover:scale-[1.03] motion-reduce:transform-none motion-reduce:transition-none"
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
        <Heading className="max-w-full [overflow-wrap:anywhere] text-start text-sm font-bold leading-snug text-[var(--heritage-charcoal)] transition-colors group-hover:text-[var(--heritage-burgundy)] group-focus-within:text-[var(--heritage-burgundy)] sm:text-base" dir="auto">
          {category.name}
        </Heading>
      </Link>
    </article>
  )
}
