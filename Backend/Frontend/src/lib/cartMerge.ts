import type { QueryClient } from '@tanstack/react-query'
import i18n from '@/i18n'
import { api } from '@/lib/api/client'
import { parseApiProblem } from '@/lib/api/problemDetails'
import { useSessionStore } from '@/stores/sessionStore'
import type { Cart } from '@/types/models'

const concurrencyConflictTitle = 'concurrency conflict'

const isConcurrencyConflict = (error: unknown) =>
  parseApiProblem(error).technicalTitle?.trim().toLowerCase() === concurrencyConflictTitle

const requestMerge = async (guestCartToken: string) => {
  const response = await api.post<Cart>('/cart/merge', { guestCartToken })
  if (!response.data || !Array.isArray(response.data.items)) {
    throw new Error('Invalid cart merge response')
  }
  return response.data
}

export const mergeGuestCart = async (guestCartToken: string, queryClient: QueryClient) => {
  await queryClient.cancelQueries({ queryKey: ['cart'] })

  let cart: Cart
  try {
    cart = await requestMerge(guestCartToken)
  } catch (error) {
    // A conflict means the backend rejected the transaction before committing,
    // so one immediate retry is safe. Network failures can be lost successful
    // responses; those stay recoverable through the explicit cart-page action,
    // where the backend treats this same opaque token idempotently.
    if (!isConcurrencyConflict(error)) throw error
    cart = await requestMerge(guestCartToken)
  }

  queryClient.setQueriesData<Cart>({ queryKey: ['cart'] }, cart)
  queryClient.setQueryData<Cart>(['cart', i18n.language], cart)
  useSessionStore.getState().setGuestCartToken('')
  await queryClient.invalidateQueries({ queryKey: ['cart'] })
  return cart
}
