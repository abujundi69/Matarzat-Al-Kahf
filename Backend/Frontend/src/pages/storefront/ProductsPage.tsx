import { useDeferredValue } from 'react'
import { useSearchParams } from 'react-router-dom'
import { LayoutGrid, SlidersHorizontal } from 'lucide-react'
import { useQuery } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { api } from '@/lib/api/client'
import type { ProductPage } from '@/types/models'
import { EmptyState, ErrorState, LoadingState } from '@/components/ui/Feedback'
import { CategoryFilterThumb } from '@/features/categories/CategoryFilterThumb'
import { useCategoriesQuery } from '@/features/categories/categoryApi'
import { ProductCard } from '@/features/products/ProductCard'

export function ProductsPage() {
  const { t, i18n } = useTranslation()
  const [params, setParams] = useSearchParams()
  const search = params.get('search') ?? ''
  const deferredSearch = useDeferredValue(search)
  const rawPage = Number(params.get('page') ?? 1)
  const page = Number.isSafeInteger(rawPage) && rawPage > 0 ? rawPage : 1
  const allowedSorts = ['newest', 'price-asc', 'price-desc', 'name'] as const
  const requestedSort = params.get('sort') ?? 'newest'
  const sort = allowedSorts.includes(requestedSort as typeof allowedSorts[number]) ? requestedSort : 'newest'
  const categoryId = params.get('categoryId') || undefined
  const categories = useCategoriesQuery()
  const selectedCategoryMissing = Boolean(categoryId) && !categories.data?.some((category) => category.id === categoryId)
  const selectedCategory = categories.data?.find((category) => category.id === categoryId)
  const query = useQuery({
    queryKey: ['products', i18n.language, page, deferredSearch, sort, categoryId],
    queryFn: () => api
      .get<ProductPage>('/products', {
        params: {
          page,
          pageSize: 12,
          search: deferredSearch || undefined,
          sort,
          categoryId,
        },
      })
      .then((response) => response.data),
  })
  const update = (key: string, value: string) => {
    const next = new URLSearchParams(params)
    if (value) next.set(key, value)
    else next.delete(key)
    if (key !== 'page') next.delete('page')
    setParams(next, { replace: key === 'search' })
  }
  return <div className="page-shell py-12">
    <h1 className="mb-8 [overflow-wrap:anywhere] text-4xl font-extrabold">{t('products.title')}</h1>
    <div className="card mb-8 grid min-w-0 gap-3 p-4 md:grid-cols-2 lg:grid-cols-[minmax(0,1fr)_minmax(11rem,15rem)_minmax(11rem,15rem)_auto]">
      <label className="min-w-0">
        <span className="sr-only">{t('products.search')}</span>
        <input
          className="field"
          placeholder={t('products.search')}
          type="search"
          value={search}
          onChange={(event) => update('search', event.target.value)}
        />
      </label>
      <select aria-label={t('products.sort')} className="field min-w-0" value={sort} onChange={(event) => update('sort', event.target.value)}>
        <option value="newest">{t('products.sortNewest')}</option>
        <option value="price-asc">{t('products.sortPriceLow')}</option>
        <option value="price-desc">{t('products.sortPriceHigh')}</option>
        <option value="name">{t('products.sortName')}</option>
      </select>
      <select
        aria-label={t('products.category')}
        className="field min-w-0"
        disabled={categories.isLoading}
        value={categoryId ?? ''}
        onChange={(event) => update('categoryId', event.target.value)}
      >
        <option value="">{t('products.allCategories')}</option>
        {selectedCategoryMissing && <option value={categoryId}>{t('products.selectedCategory')}</option>}
        {categories.data?.map((category) => <option key={category.id} value={category.id}>{category.name}</option>)}
      </select>
      <button className="button-secondary max-w-full whitespace-normal text-center" disabled={!params.size} onClick={() => setParams({})} type="button">
        <SlidersHorizontal className="shrink-0" size={18} />{t('actions.clear')}
      </button>
      {categories.isError && <div className="flex min-w-0 flex-wrap items-center justify-between gap-3 rounded-xl bg-[var(--heritage-beige)] px-3 py-2 text-sm md:col-span-2 lg:col-span-4" role="alert">
        <span className="min-w-0 [overflow-wrap:anywhere] text-[var(--heritage-burgundy)]">{t('products.categoriesUnavailable')}</span>
        <button className="button-secondary !min-h-9 !px-3 !py-1" onClick={() => void categories.refetch()} type="button">{t('actions.retry')}</button>
      </div>}
    </div>
    {!categories.isError && <nav aria-label={t('products.category')} className="category-filter-rail mb-8">
      <button
        aria-pressed={!categoryId}
        className="category-filter-chip"
        data-active={!categoryId}
        disabled={categories.isLoading}
        onClick={() => update('categoryId', '')}
        type="button"
      >
        <span aria-hidden className="grid size-6 shrink-0 place-items-center rounded-full bg-[var(--heritage-beige)] ring-1 ring-black/5">
          <LayoutGrid className="text-[var(--heritage-burgundy)]" size={13} strokeWidth={2} />
        </span>
        {t('products.allCategories')}
      </button>
      {categories.isLoading
        ? Array.from({ length: 5 }, (_, index) => <span aria-hidden className="skeleton h-11 w-32 shrink-0 rounded-full" key={index} />)
        : categories.data?.map((category) => <button
            aria-pressed={category.id === categoryId}
            className="category-filter-chip"
            data-active={category.id === categoryId}
            key={category.id}
            onClick={() => update('categoryId', category.id)}
            type="button"
          >
            <CategoryFilterThumb imageUrl={category.imageUrl} />
            <span className="clip-safe min-w-0 truncate" dir="auto" title={category.name}>{category.name}</span>
            {category.productCount != null && <span className="category-filter-count" dir="ltr">{category.productCount}</span>}
          </button>)}
    </nav>}
    {selectedCategory && <div className="mb-6 flex min-w-0 flex-wrap items-center gap-2" role="status">
      <span className="muted text-sm">{t('products.selectedCategory')}:</span>
      <strong className="badge max-w-full whitespace-normal text-start" dir="auto">{selectedCategory.name}</strong>
    </div>}
    {query.isLoading
      ? <LoadingState cards={8} />
      : query.isError
        ? <ErrorState error={query.error} retry={() => void query.refetch()} />
        : !query.data?.items?.length
          ? <EmptyState message={t('products.empty')} />
          : <>
            <div className="grid grid-cols-2 gap-3 sm:gap-4 md:grid-cols-3 lg:grid-cols-4">
              {query.data.items.map((product) => <ProductCard key={product.id} product={product} />)}
            </div>
            <nav aria-label={t('pagination.label')} className="mt-10 flex flex-wrap items-center justify-center gap-3">
              <button className="button-secondary max-w-full whitespace-normal text-center" disabled={page <= 1 || query.isFetching} onClick={() => update('page', String(page - 1))} type="button">{t('pagination.previous')}</button>
              <span className="badge max-w-full whitespace-normal text-center">{t('pagination.pageOf', { page, total: Math.max(1, query.data.totalPages ?? 1) })}</span>
              <button className="button-secondary max-w-full whitespace-normal text-center" disabled={page >= (query.data.totalPages ?? 1) || query.isFetching} onClick={() => update('page', String(page + 1))} type="button">{t('pagination.next')}</button>
            </nav>
          </>}
  </div>
}
