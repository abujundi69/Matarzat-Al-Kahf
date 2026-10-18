import { useQuery } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { api } from '@/lib/api/client'
import type { Category } from '@/types/models'

export type BrowsableCategory = Category & { id: string; name: string }

export const categoryIdPattern = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i

const hasRequiredCategoryData = (category: Category): category is BrowsableCategory =>
  typeof category.id === 'string'
  && categoryIdPattern.test(category.id)
  && typeof category.name === 'string'
  && category.name.trim().length > 0

export const categoryKeys = {
  all: ['categories'] as const,
  list: (language: string) => [...categoryKeys.all, 'list', language] as const,
  detail: (reference: string, language: string) => [...categoryKeys.all, 'detail', reference, language] as const,
}

export const getCategories = (language: 'ar' | 'en', signal?: AbortSignal) =>
  api.get<Category[]>('/categories', {
    headers: { 'Accept-Language': language },
    signal,
  }).then((response) => response.data)

export function useCategoriesQuery({ enabled = true }: { enabled?: boolean } = {}) {
  const { i18n } = useTranslation()
  const language: 'ar' | 'en' = i18n.language.startsWith('en') ? 'en' : 'ar'

  return useQuery({
    enabled,
    queryKey: categoryKeys.list(language),
    queryFn: ({ signal }) => getCategories(language, signal),
    select: (categories) => categories.filter(hasRequiredCategoryData),
  })
}
