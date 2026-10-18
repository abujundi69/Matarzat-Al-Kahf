import type { Query, QueryClient } from '@tanstack/react-query'

const isActorScopedQuery = (query: Query) => {
  const scope = query.queryKey[0]
  return scope === 'cart'
    || scope === 'customer'
    || (typeof scope === 'string' && scope.startsWith('admin'))
}

export const clearActorQueryCaches = async (queryClient: QueryClient) => {
  const filters = { predicate: isActorScopedQuery }
  await queryClient.cancelQueries(filters)
  queryClient.removeQueries(filters)
}
