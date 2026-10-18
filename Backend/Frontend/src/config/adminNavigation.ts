import {
  BarChart3,
  FileSpreadsheet,
  LayoutDashboard,
  Package,
  Images,
  ScrollText,
  Settings,
  ShieldCheck,
  ShoppingCart,
  Tags,
  TicketPercent,
  UserCog,
  Users,
  Warehouse,
  type LucideIcon,
} from 'lucide-react'
import {
  adminPermissions,
  adminResources,
  hasAnyPermission,
} from '@/config/adminResources'

export type AdminNavigationItem = {
  icon: LucideIcon
  key: string
  path: string
  permissions: readonly string[]
}

export type AdminNavigationGroup = {
  collapsible: boolean
  icon: LucideIcon
  key: string
  items: readonly AdminNavigationItem[]
}

const dashboardItem: AdminNavigationItem = {
  path: '/admin/dashboard',
  key: 'admin.resources.dashboard',
  icon: LayoutDashboard,
  permissions: [adminPermissions.dashboard],
}

const storeItems: readonly AdminNavigationItem[] = [
  { path: '/admin/orders', key: 'admin.resources.operations', icon: ShoppingCart, permissions: [adminPermissions.ordersRead, adminPermissions.paymentsRead] },
  { path: '/admin/products', key: adminResources.products.titleKey, icon: Package, permissions: adminResources.products.readPermissions },
  { path: '/admin/categories', key: adminResources.categories.titleKey, icon: Tags, permissions: adminResources.categories.readPermissions },
  { path: '/admin/banners', key: adminResources.banners.titleKey, icon: Images, permissions: adminResources.banners.readPermissions },
  { path: '/admin/inventory', key: adminResources.inventory.titleKey, icon: Warehouse, permissions: adminResources.inventory.readPermissions },
  { path: '/admin/customers', key: adminResources.customers.titleKey, icon: Users, permissions: adminResources.customers.readPermissions },
  { path: '/admin/coupons', key: adminResources.coupons.titleKey, icon: TicketPercent, permissions: adminResources.coupons.readPermissions },
]

const reportItems: readonly AdminNavigationItem[] = [
  {
    path: '/admin/reports',
    key: 'admin.resources.reports',
    icon: BarChart3,
    permissions: [
      adminPermissions.reportsSalesRead,
      adminPermissions.reportsProductsRead,
      adminPermissions.reportsCustomersRead,
      adminPermissions.reportsOrdersRead,
      adminPermissions.reportsInventoryRead,
    ],
  },
  { path: '/admin/imports', key: 'admin.resources.imports', icon: FileSpreadsheet, permissions: [adminPermissions.importsProducts, adminPermissions.importsInventory] },
]

const accessItems: readonly AdminNavigationItem[] = [
  { path: '/admin/users', key: adminResources.users.titleKey, icon: UserCog, permissions: adminResources.users.readPermissions },
  { path: '/admin/audit-logs', key: adminResources['audit-logs'].titleKey, icon: ScrollText, permissions: adminResources['audit-logs'].readPermissions },
]

const settingsItem: AdminNavigationItem = {
  path: '/admin/settings',
  key: adminResources.settings.titleKey,
  icon: Settings,
  permissions: [
    adminPermissions.settingsRead,
    adminPermissions.socialRead,
    adminPermissions.paymentsRead,
    adminPermissions.deliveryRead,
    adminPermissions.seoRead,
  ],
}

export const adminNavigationGroups: readonly AdminNavigationGroup[] = [
  { key: 'admin.groups.main', icon: LayoutDashboard, collapsible: false, items: [dashboardItem] },
  { key: 'admin.groups.store', icon: ShoppingCart, collapsible: true, items: storeItems },
  { key: 'admin.groups.reports', icon: BarChart3, collapsible: true, items: reportItems },
  { key: 'admin.groups.access', icon: ShieldCheck, collapsible: true, items: accessItems },
  { key: 'admin.groups.settings', icon: Settings, collapsible: false, items: [settingsItem] },
]

export const adminNavigation: readonly AdminNavigationItem[] =
  adminNavigationGroups.flatMap((group) => group.items)

export const getAdminLandingPath = (permissions: readonly string[]) => {
  if (permissions.includes(adminPermissions.dashboard)) return '/admin/dashboard'
  if (permissions.includes(adminPermissions.ordersRead)) return '/admin/orders'
  const navigationPath = adminNavigation.find((item) =>
    hasAnyPermission(permissions, item.permissions))?.path
  if (navigationPath) return navigationPath
  if (permissions.includes(adminPermissions.notificationsRead)) return '/admin/notifications'
  if (permissions.includes(adminPermissions.contactRead)) return '/admin/messages'
  return undefined
}
