import { QueryClient } from '@tanstack/react-query'
import axios from 'axios'

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: { staleTime: 30_000, retry: (count, error) => count < 2 && !(axios.isAxiosError(error) && [401, 403, 404].includes(error.response?.status ?? 0)) },
    mutations: { retry: false },
  },
})
