import { useLogoutFlow } from '@/lib/logoutFlow'

/** Clears the customer session/queries the same way everywhere, then navigates to `redirectTo`. */
export function useCustomerLogout(redirectTo: string) {
  return useLogoutFlow({
    apiLogoutPath: '/customer/auth/logout',
    extraCleanup: () => sessionStorage.removeItem('matarzat-checkout-draft'),
    matchesProtectedQuery: (queryKey) => queryKey[0] === 'customer' || queryKey[0] === 'cart',
    redirectTo,
  })
}
