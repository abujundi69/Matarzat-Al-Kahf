import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { isSessionValid, useSessionStore } from '@/stores/sessionStore'

export function AdminGuard() {
  const adminSession = useSessionStore((state) => state.admin)
  const customerSession = useSessionStore((state) => state.customer)
  const location = useLocation()
  if (isSessionValid(adminSession)) return <Outlet />
  if (isSessionValid(customerSession)) return <Navigate replace to="/" />
  return <Navigate replace state={{ from: `${location.pathname}${location.search}` }} to="/login" />
}

export function CustomerGuard() {
  const customerSession = useSessionStore((state) => state.customer)
  const adminSession = useSessionStore((state) => state.admin)
  const location = useLocation()
  if (isSessionValid(customerSession)) return <Outlet />
  if (isSessionValid(adminSession)) return <Navigate replace to="/admin" />
  return <Navigate replace state={{ from: `${location.pathname}${location.search}` }} to="/login" />
}
