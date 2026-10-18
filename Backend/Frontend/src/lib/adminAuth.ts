import { useLogoutFlow } from '@/lib/logoutFlow'

/**
 * Clears the admin/super-admin session/queries the same way everywhere, then navigates to
 * `redirectTo`. The server-side call revokes the administrative session so the presented token
 * stops being accepted immediately instead of staying valid until it expires.
 */
export function useAdminLogout(redirectTo: string) {
  return useLogoutFlow({
    apiLogoutPath: '/admin/auth/logout',
    matchesProtectedQuery: (queryKey) => String(queryKey[0] ?? '').startsWith('admin'),
    redirectTo,
  })
}
