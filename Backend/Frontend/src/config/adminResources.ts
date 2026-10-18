export const adminPermissions = {
  dashboard: 'reports.sales.read',
  productsRead: 'catalog.products.read',
  productsWrite: 'catalog.products.write',
  categoriesRead: 'catalog.categories.read',
  categoriesWrite: 'catalog.categories.write',
  inventoryRead: 'catalog.inventory.read',
  inventoryWrite: 'catalog.inventory.write',
  ordersRead: 'orders.read',
  ordersExport: 'orders.export',
  paymentsRead: 'payments.read',
  paymentsWrite: 'payments.write',
  paymentsVerify: 'payments.verify',
  customersRead: 'customers.read',
  customersWrite: 'customers.write',
  customersStatusWrite: 'customers.status.write',
  usersRead: 'admin.users.read',
  usersWrite: 'admin.users.write',
  rolesRead: 'admin.roles.read',
  rolesWrite: 'admin.roles.write',
  couponsRead: 'coupons.read',
  couponsWrite: 'coupons.write',
  deliveryRead: 'delivery.read',
  deliveryWrite: 'delivery.write',
  settingsRead: 'content.settings.read',
  settingsWrite: 'content.settings.write',
  socialRead: 'content.social.read',
  socialWrite: 'content.social.write',
  contactRead: 'contact.messages.read',
  contactWrite: 'contact.messages.write',
  notificationsRead: 'notifications.read',
  reportsSalesRead: 'reports.sales.read',
  reportsProductsRead: 'reports.products.read',
  reportsCustomersRead: 'reports.customers.read',
  reportsOrdersRead: 'reports.orders.read',
  reportsInventoryRead: 'reports.inventory.read',
  reportsExport: 'reports.export',
  importsProducts: 'imports.products',
  importsInventory: 'imports.inventory',
  auditRead: 'audit.read',
  seoRead: 'seo.read',
  seoWrite: 'seo.write',
  filesUpload: 'files.upload',
  bannersRead: 'content.banners.read',
  bannersWrite: 'content.banners.write',
} as const

export type AdminResource = {
  titleKey: string
  endpoint: string
  readPermissions: readonly string[]
  writePermission?: string
  statusWritePermission?: string
  exportEndpoint?: string
  exportPermission?: string
  supportsSearch?: boolean
  /** Fields present in the API response that should never render in the table or the details dialog (e.g. internal IDs). */
  hiddenFields?: readonly string[]
  /** Fixed, ordered list columns for catalog resources; everything else stays in the details dialog. */
  listColumns?: readonly string[]
}

export type WritableAdminResource = 'products' | 'categories' | 'inventory' | 'coupons' | 'banners'

export const isWritableAdminResource = (value: string): value is WritableAdminResource =>
  value === 'products' || value === 'categories' || value === 'inventory' || value === 'coupons' || value === 'banners'

export const adminResources: Record<string, AdminResource> = {
  products: {
    titleKey: 'admin.resources.products',
    endpoint: '/admin/products',
    readPermissions: [adminPermissions.productsRead],
    writePermission: adminPermissions.productsWrite,
    exportEndpoint: '/admin/exports/products',
    exportPermission: adminPermissions.reportsExport,
    listColumns: ['imageUrl', 'name', 'categoryName', 'basePrice', 'status'],
  },
  categories: {
    titleKey: 'admin.resources.categories',
    endpoint: '/admin/categories',
    readPermissions: [adminPermissions.categoriesRead],
    writePermission: adminPermissions.categoriesWrite,
    listColumns: ['imageUrl', 'name', 'isActive'],
  },
  banners: {
    titleKey: 'admin.resources.banners',
    endpoint: '/admin/banners',
    readPermissions: [adminPermissions.bannersRead],
    writePermission: adminPermissions.bannersWrite,
  },
  inventory: {
    titleKey: 'admin.resources.inventory',
    endpoint: '/admin/inventory',
    readPermissions: [adminPermissions.inventoryRead],
    writePermission: adminPermissions.inventoryWrite,
    exportEndpoint: '/admin/exports/inventory',
    exportPermission: adminPermissions.reportsExport,
    hiddenFields: ['productVariantId', 'productId'],
  },
  orders: {
    titleKey: 'admin.resources.orders',
    endpoint: '/admin/orders',
    readPermissions: [adminPermissions.ordersRead],
    exportEndpoint: '/admin/exports/orders',
    exportPermission: adminPermissions.ordersExport,
    supportsSearch: true,
  },
  payments: {
    titleKey: 'admin.resources.payments',
    endpoint: '/admin/payments',
    readPermissions: [adminPermissions.paymentsRead],
  },
  'payment-methods': {
    titleKey: 'admin.resources.paymentMethods',
    endpoint: '/admin/payment-methods',
    readPermissions: [adminPermissions.paymentsRead],
    writePermission: adminPermissions.paymentsWrite,
  },
  customers: {
    titleKey: 'admin.resources.customers',
    endpoint: '/admin/customers',
    readPermissions: [adminPermissions.customersRead],
    writePermission: adminPermissions.customersWrite,
    statusWritePermission: adminPermissions.customersStatusWrite,
    exportEndpoint: '/admin/exports/customers',
    exportPermission: adminPermissions.reportsExport,
    supportsSearch: true,
  },
  users: {
    titleKey: 'admin.resources.users',
    endpoint: '/admin/users',
    readPermissions: [adminPermissions.usersRead],
    writePermission: adminPermissions.usersWrite,
    supportsSearch: true,
  },
  roles: {
    titleKey: 'admin.resources.roles',
    endpoint: '/admin/roles',
    readPermissions: [adminPermissions.rolesRead],
    writePermission: adminPermissions.rolesWrite,
  },
  permissions: {
    titleKey: 'admin.resources.permissions',
    endpoint: '/admin/permissions',
    readPermissions: [adminPermissions.rolesRead],
    writePermission: adminPermissions.rolesWrite,
  },
  coupons: {
    titleKey: 'admin.resources.coupons',
    endpoint: '/admin/coupons',
    readPermissions: [adminPermissions.couponsRead],
    writePermission: adminPermissions.couponsWrite,
    supportsSearch: true,
  },
  delivery: {
    titleKey: 'admin.resources.delivery',
    endpoint: '/admin/delivery/locations',
    readPermissions: [adminPermissions.deliveryRead],
    writePermission: adminPermissions.deliveryWrite,
  },
  settings: {
    titleKey: 'admin.resources.settings',
    endpoint: '/admin/settings/general',
    readPermissions: [adminPermissions.settingsRead],
    writePermission: adminPermissions.settingsWrite,
  },
  'social-links': {
    titleKey: 'admin.resources.socialLinks',
    endpoint: '/admin/social-links',
    readPermissions: [adminPermissions.socialRead],
    writePermission: adminPermissions.socialWrite,
  },
  contact: {
    titleKey: 'admin.resources.contact',
    endpoint: '/admin/contact-messages',
    readPermissions: [adminPermissions.contactRead],
    writePermission: adminPermissions.contactWrite,
    supportsSearch: true,
  },
  notifications: {
    titleKey: 'admin.resources.notifications',
    endpoint: '/admin/notifications',
    readPermissions: [adminPermissions.notificationsRead],
  },
  templates: {
    titleKey: 'admin.resources.templates',
    endpoint: '/admin/notifications/templates',
    readPermissions: [adminPermissions.notificationsRead],
  },
  'audit-logs': {
    titleKey: 'admin.resources.auditLogs',
    endpoint: '/admin/audit-logs',
    readPermissions: [adminPermissions.auditRead],
    hiddenFields: ['actorId'],
  },
  seo: {
    titleKey: 'admin.resources.seo',
    endpoint: '/admin/seo',
    readPermissions: [adminPermissions.seoRead],
    writePermission: adminPermissions.seoWrite,
  },
}

export const hasAnyPermission = (
  permissions: readonly string[],
  required: readonly string[],
) => required.length === 0 || required.some((permission) => permissions.includes(permission))

export const getAdminResource = (key: string) =>
  (adminResources as Record<string, AdminResource | undefined>)[key]
