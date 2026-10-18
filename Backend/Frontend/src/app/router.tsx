import { Suspense, type ReactNode } from 'react'
import { createBrowserRouter, Navigate } from 'react-router-dom'
import { StorefrontLayout } from '@/layouts/StorefrontLayout'
import { CustomerLayout } from '@/layouts/CustomerLayout'
import { AdminLandingPage, AdminLayout } from '@/layouts/AdminLayout'
import { AdminGuard, CustomerGuard } from '@/app/guards/AuthGuards'
import { LoadingState } from '@/components/ui/Feedback'
import {
  AdminDashboardPage,
  AdminImportsPage,
  AdminNotificationsPage,
  AdminOperationsPage,
  AdminOrderDetailsPage,
  AdminProfilePage,
  AdminReportsPage,
  AdminResourcePage,
  AdminSettingsPage,
  AdminUsersPage,
  CartPage,
  CategoriesPage,
  CheckoutPage,
  ContactPage,
  ContentPage,
  CustomerOrderDetailsPage,
  CustomerOrdersPage,
  CustomerProfilePage,
  CustomerResourcePage,
  HomePage,
  LoginPage,
  NotFoundPage,
  OrderLookupPage,
  OrderSuccessPage,
  OtpFlowPage,
  ProductDetailsPage,
  ProductsPage,
} from '@/app/lazyPages'

const pending = <div className="page-shell py-12"><LoadingState /></div>
const wrap = (element: ReactNode) => <Suspense fallback={pending}>{element}</Suspense>

export const router = createBrowserRouter([
  {
    element: <StorefrontLayout />,
    children: [
      { index: true, element: wrap(<HomePage />) },
      { path: 'products', element: wrap(<ProductsPage />) },
      { path: 'search', element: <Navigate replace to="/products" /> },
      { path: 'products/:id', element: wrap(<ProductDetailsPage />) },
      { path: 'categories', element: wrap(<CategoriesPage />) },
      { path: 'categories/:id', element: wrap(<CategoriesPage />) },
      { path: 'cart', element: wrap(<CartPage />) },
      { path: 'order-lookup', element: wrap(<OrderLookupPage />) },
      { path: 'payment-methods', element: <Navigate replace to="/checkout" /> },
      { path: 'pages/:slug', element: wrap(<ContentPage />) },
      { path: 'contact', element: wrap(<ContactPage />) },
      {
        element: <CustomerGuard />,
        children: [
          { path: 'checkout', element: wrap(<CheckoutPage />) },
          { path: 'order-success/:id', element: wrap(<OrderSuccessPage />) },
          {
            path: 'account',
            element: <CustomerLayout />,
            children: [
              { index: true, element: <Navigate replace to="profile" /> },
              { path: 'profile', element: wrap(<CustomerProfilePage />) },
              { path: 'addresses', element: wrap(<CustomerResourcePage resource="addresses" />) },
              { path: 'orders', element: wrap(<CustomerOrdersPage />) },
              { path: 'orders/:id', element: wrap(<CustomerOrderDetailsPage />) },
              { path: 'messages', element: wrap(<CustomerResourcePage resource="messages" />) },
              { path: 'payments', element: wrap(<CustomerResourcePage resource="payments" />) },
              { path: 'notifications', element: wrap(<CustomerResourcePage resource="notifications" />) },
            ],
          },
        ],
      },
      { path: '*', element: wrap(<NotFoundPage />) },
    ],
  },
  { path: '/login', element: wrap(<LoginPage />) },
  { path: '/register', element: wrap(<OtpFlowPage purpose="register" />) },
  { path: '/forgot-password', element: wrap(<OtpFlowPage purpose="forgot-password" />) },
  { path: '/reset-password', element: wrap(<OtpFlowPage purpose="forgot-password" />) },
  {
    element: <AdminGuard />,
    children: [{
      path: '/admin',
      element: <AdminLayout />,
      children: [
        { index: true, element: <AdminLandingPage /> },
        { path: 'dashboard', element: wrap(<AdminDashboardPage />) },
        { path: 'orders', element: wrap(<AdminOperationsPage />) },
        { path: 'payments', element: <Navigate replace to="/admin/orders?tab=payments" /> },
        { path: 'reports', element: wrap(<AdminReportsPage />) },
        { path: 'imports', element: wrap(<AdminImportsPage />) },
        { path: 'notifications', element: wrap(<AdminNotificationsPage />) },
        { path: 'messages', element: wrap(<AdminResourcePage resource="contact" />) },
        { path: 'contact', element: <Navigate replace to="/admin/messages" /> },
        { path: 'templates', element: <Navigate replace to="/admin/notifications?tab=templates" /> },
        { path: 'users', element: wrap(<AdminUsersPage />) },
        { path: 'roles', element: <Navigate replace to="/admin/users" /> },
        { path: 'permissions', element: <Navigate replace to="/admin/users" /> },
        { path: 'settings', element: wrap(<AdminSettingsPage />) },
        { path: 'settings/general', element: <Navigate replace to="/admin/settings?tab=general" /> },
        { path: 'settings/contact', element: <Navigate replace to="/admin/settings?tab=contact" /> },
        { path: 'settings/social', element: <Navigate replace to="/admin/settings?tab=social" /> },
        { path: 'settings/payments', element: <Navigate replace to="/admin/settings?tab=payments" /> },
        { path: 'settings/shipping', element: <Navigate replace to="/admin/settings?tab=shipping" /> },
        { path: 'settings/seo', element: <Navigate replace to="/admin/settings?tab=seo" /> },
        { path: 'social-links', element: <Navigate replace to="/admin/settings?tab=social" /> },
        { path: 'payment-methods', element: <Navigate replace to="/admin/settings?tab=payments" /> },
        { path: 'delivery', element: <Navigate replace to="/admin/settings?tab=shipping" /> },
        { path: 'seo', element: <Navigate replace to="/admin/settings?tab=seo" /> },
        { path: 'profile', element: wrap(<AdminProfilePage />) },
        { path: 'orders/:id', element: wrap(<AdminOrderDetailsPage />) },
        { path: ':resource', element: wrap(<AdminResourcePage />) },
      ],
    }],
  },
])
