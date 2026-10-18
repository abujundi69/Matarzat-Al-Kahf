import type { components } from './api.generated'

export type Schema<K extends keyof components['schemas']> = components['schemas'][K]
export type ProductSummary = Schema<'PublicProductListItemDto'>
export type ProductDetails = Schema<'PublicProductDetailsDto'>
export type ProductPage = Schema<'PublicProductListItemDtoPagedResult'>
export type Category = Schema<'PublicCategoryDto'>
export type HomeData = Schema<'HomeResponse'>
export type Cart = Schema<'CartResponse'>
export type Dashboard = Schema<'DashboardResponse'>
