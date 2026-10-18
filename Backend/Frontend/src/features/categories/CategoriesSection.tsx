import { ArrowLeft, ArrowRight } from 'lucide-react'
import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { EmptyState, ErrorState } from '@/components/ui/Feedback'
import { CategoryCard } from './CategoryCard'
import { useCategoriesQuery } from './categoryApi'

function CategoryGridSkeleton() {
  const { t } = useTranslation()

  return (
    <div aria-busy="true" aria-live="polite" role="status">
      <span className="sr-only">{t('states.loading')}</span>
      <div aria-hidden className="category-rail">
        {Array.from({ length: 8 }, (_, index) => (
          <div className="flex flex-col items-center gap-3 p-3" key={index}>
            <div className="skeleton size-24 rounded-full sm:size-28 lg:size-32" />
            <div className="skeleton h-4 w-4/5 max-w-24" />
          </div>
        ))}
      </div>
    </div>
  )
}

export function CategoriesSection() {
  const { t, i18n } = useTranslation()
  const query = useCategoriesQuery()
  const Arrow = i18n.dir() === 'rtl' ? ArrowLeft : ArrowRight

  return (
    <section aria-labelledby="home-categories-title" className="relative overflow-hidden border-y border-[var(--heritage-line)] bg-white py-16 sm:py-20">
      <div className="page-shell relative">
        <div className="mb-9 flex min-w-0 flex-wrap items-end justify-between gap-5">
          <h2 className="min-w-0 max-w-2xl [overflow-wrap:anywhere] text-3xl font-extrabold sm:text-4xl" id="home-categories-title">{t('home.categoriesTitle')}</h2>
          <Link className="button-secondary shrink-0 no-underline" to="/categories">
            {t('categories.viewAll')}
            <Arrow aria-hidden size={17} />
          </Link>
        </div>
        {query.isLoading
          ? <CategoryGridSkeleton />
          : query.isError
            ? <ErrorState error={query.error} message={t('categories.loadError')} retry={() => void query.refetch()} />
            : !query.data?.length
              ? <EmptyState message={t('categories.empty')} />
              : <div className="category-rail">
                  {query.data.map((category) => <CategoryCard category={category} key={category.id} />)}
                </div>}
      </div>
    </section>
  )
}
