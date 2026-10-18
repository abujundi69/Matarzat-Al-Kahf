import { adminNavigation, getAdminLandingPath } from '@/config/adminNavigation'
import { adminPermissions, hasAnyPermission } from '@/config/adminResources'

export const safeReturnPath = (value: string | null | undefined) =>
  value?.startsWith('/') && !value.startsWith('//') ? value : undefined

export const resolveLoginDestination = (
  kind: 'customer' | 'admin',
  permissions: readonly string[],
  requestedPath?: string,
) => {
  const requested = safeReturnPath(requestedPath)
  if (kind === 'admin') {
    const requestedItem = requested && adminNavigation
      .filter((item) => requested === item.path || requested.startsWith(`${item.path}/`) || requested.startsWith(`${item.path}?`))
      .sort((left, right) => right.path.length - left.path.length)[0]
    const canOpenStandalone = requested === '/admin/profile'
      || (requested?.startsWith('/admin/notifications') && permissions.includes(adminPermissions.notificationsRead))
      || (requested?.startsWith('/admin/messages') && permissions.includes(adminPermissions.contactRead))
    const canOpenRequested = requestedItem && hasAnyPermission(permissions, requestedItem.permissions)
    return requested && (canOpenStandalone || canOpenRequested)
      ? requested
      : (getAdminLandingPath(permissions) ?? '/admin')
  }

  const isCustomerDestination = requested
    && !requested.startsWith('/admin')
    && !['/login', '/register', '/forgot-password', '/reset-password'].includes(requested)
  return isCustomerDestination ? requested : '/'
}
