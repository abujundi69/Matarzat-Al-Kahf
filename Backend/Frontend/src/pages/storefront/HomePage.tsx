import { useState } from 'react'
import { ArrowLeft, ArrowRight } from 'lucide-react'
import { Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { api } from '@/lib/api/client'
import { isGenericPlaceholderUrl, resolveMediaUrl } from '@/lib/media'
import type { HomeData } from '@/types/models'
import { BrandLogo } from '@/components/common/BrandLogo'
import { EmptyState, ErrorState, LoadingState } from '@/components/ui/Feedback'
import { CategoriesSection } from '@/features/categories/CategoriesSection'
import { ProductCard } from '@/features/products/ProductCard'
import heroImage from '@/assets/hero-heritage.jpg'

export function HomePage() {
  const { t, i18n } = useTranslation()
  const [failedBannerUrl, setFailedBannerUrl] = useState<string>()
  const query = useQuery({ queryKey: ['home', i18n.language], queryFn: () => api.get<HomeData>('/home').then((response) => response.data) })
  const Arrow = i18n.dir() === 'rtl' ? ArrowLeft : ArrowRight
  const banner = query.data?.mainBanners?.[0]
  const bannerUrl = banner?.desktopImageUrl ?? ''
  const useRemoteBanner = Boolean(bannerUrl) && !isGenericPlaceholderUrl(bannerUrl) && failedBannerUrl !== bannerUrl
  const products = query.data?.featuredProducts?.slice(0, 8) ?? []
  return <>
    <section className="relative overflow-hidden bg-[var(--heritage-beige)]">
      <div className="page-shell relative grid min-w-0 items-center gap-12 py-12 sm:py-16 lg:min-h-[600px] lg:grid-cols-[minmax(0,1.05fr)_minmax(0,.95fr)]">
        <div className="min-w-0">
          <p className="mb-5 flex items-center gap-3 [overflow-wrap:anywhere] font-bold uppercase tracking-[.2em] text-[var(--heritage-red)] rtl:tracking-normal"><span aria-hidden className="h-0.5 w-10 shrink-0 bg-[var(--heritage-gold)]" />{t('home.eyebrow')}</p>
          <h1 className="max-w-3xl [overflow-wrap:anywhere] text-4xl font-extrabold leading-[1.2] text-[var(--heritage-burgundy)] sm:text-5xl lg:text-6xl">{t('home.title')}</h1>
          <div className="mt-8 grid gap-3 sm:flex sm:flex-wrap">
            <Link className="button-primary max-w-full whitespace-normal text-center no-underline" to="/products">{t('actions.explore')}<Arrow className="shrink-0" size={18} /></Link>
            <Link className="button-secondary max-w-full whitespace-normal text-center no-underline" to="/categories">{t('categories.viewAll')}</Link>
          </div>
        </div>
        <div className="relative mx-auto w-full max-w-md sm:before:absolute sm:before:inset-0 sm:before:translate-x-3 sm:before:translate-y-3 sm:before:rounded-2xl sm:before:border-2 sm:before:border-[var(--heritage-gold)] sm:rtl:before:-translate-x-3">
          <div className="relative aspect-[4/5] overflow-hidden rounded-2xl shadow-[0_18px_44px_rgb(35_28_24_/_0.18)]">
            <picture className="block size-full">
              {useRemoteBanner && banner?.mobileImageUrl && <source media="(max-width: 639px)" srcSet={resolveMediaUrl(banner.mobileImageUrl)} />}
              <img
                alt={banner?.altText ?? t('home.eyebrow')}
                className="size-full object-cover"
                onError={useRemoteBanner ? () => setFailedBannerUrl(bannerUrl) : undefined}
                src={useRemoteBanner ? resolveMediaUrl(bannerUrl) : heroImage}
              />
            </picture>
          </div>
        </div>
      </div>
    </section>
    <CategoriesSection />
    <section className="page-shell py-16 sm:py-20">
      <div className="mb-9 flex flex-wrap items-end justify-between gap-4"><h2 className="min-w-0 [overflow-wrap:anywhere] text-3xl font-extrabold">{t('home.featuredTitle')}</h2><Link className="button-secondary shrink-0 no-underline" to="/products">{t('actions.explore')}<Arrow aria-hidden size={17} /></Link></div>
      {query.isLoading ? <LoadingState cards={4} /> : query.isError ? <ErrorState error={query.error} retry={() => void query.refetch()} /> : products.length ? <div className="grid grid-cols-2 gap-3 sm:gap-4 md:grid-cols-3 lg:grid-cols-4">{products.map((product) => <ProductCard key={product.id} product={product} />)}</div> : <EmptyState />}
    </section>
    {/* The footer keeps a margin of its own; pulling this band down by the same amount lets the red story band meet the footer ribbon instead of leaving a stripe of page colour between them. */}
    <section className="heritage-pattern -mb-20 text-white">
      <div className="tatreez-band tatreez-band-lg" aria-hidden />
      <div className="page-shell grid min-w-0 items-center gap-8 py-14 md:grid-cols-[auto_minmax(0,1fr)] md:gap-12 lg:py-16">
        <BrandLogo className="size-32 drop-shadow-[0_14px_30px_rgb(0_0_0_/_0.35)] max-md:mx-auto md:size-44" variant="md" />
        <div className="min-w-0 max-md:text-center">
          <p className="[overflow-wrap:anywhere] text-sm font-bold uppercase tracking-widest text-[var(--heritage-sand)] rtl:tracking-normal">{t('home.heritageEyebrow')}</p>
          <h2 className="mt-3 [overflow-wrap:anywhere] text-3xl font-extrabold sm:text-4xl">{t('home.heritageTitle')}</h2>
        </div>
      </div>
    </section>
  </>
}
