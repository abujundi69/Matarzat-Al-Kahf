import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { ImagePlaceholder } from '@/components/common/ImagePlaceholder'
import { isGenericPlaceholderUrl, resolveMediaUrl } from '@/lib/media'
import { formatMoney } from '@/lib/utils'
import type { ProductSummary } from '@/types/models'

export function ProductCard({ product }: { product: ProductSummary }) {
  const { t, i18n } = useTranslation()
  const target = `/products/${product.slug || product.id}`
  return <article className="group relative flex h-full min-w-0 flex-col gap-3">
    {/* Decorative: the name link below is the card's single, full-card link target. */}
    <div aria-hidden className="aspect-[4/5] overflow-hidden rounded-xl bg-[var(--heritage-beige)]">
      {product.primaryImageUrl && !isGenericPlaceholderUrl(product.primaryImageUrl)
        ? <img alt="" className="size-full object-cover transition duration-500 group-hover:scale-[1.03] motion-reduce:transform-none motion-reduce:transition-none" loading="lazy" src={resolveMediaUrl(product.primaryImageUrl)} />
        : <ImagePlaceholder />}
    </div>
    <div className="flex min-w-0 flex-1 flex-col gap-1.5">
      <Link className="rounded-sm [overflow-wrap:anywhere] text-start text-sm font-bold leading-snug text-[var(--heritage-charcoal)] no-underline transition-colors after:absolute after:inset-0 after:content-[''] group-hover:text-[var(--heritage-burgundy)] sm:text-base" dir="auto" to={target}>{product.name}</Link>
      <div className="mt-auto flex flex-wrap items-baseline gap-x-3 gap-y-0.5">
        <strong className="min-w-0 [overflow-wrap:anywhere] text-base font-extrabold leading-snug text-[var(--heritage-burgundy)] sm:text-lg" dir="ltr">{formatMoney(product.minimumEffectivePrice ?? product.basePrice ?? 0, product.currency ?? 'ILS', i18n.language === 'ar' ? 'ar-PS-u-nu-latn' : 'en-US')}</strong>
        {!product.inStock && <span className="muted text-xs font-semibold">{t('products.outOfStock')}</span>}
      </div>
    </div>
  </article>
}
