import { useState } from 'react'
import { useQueryClient, type QueryKey } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { api } from '@/lib/api/client'
import { parseApiProblem } from '@/lib/api/problemDetails'
import { useSessionStore } from '@/stores/sessionStore'

type LogoutFlowOptions = {
  apiLogoutPath?: string
  extraCleanup?: () => void
  matchesProtectedQuery: (queryKey: QueryKey) => boolean
  redirectTo: string
}

/**
 * Shared confirm-then-clear-session flow behind every "Logout" action (customer, admin,
 * super admin all funnel through here). `requestLogout` only opens the confirmation dialog;
 * the session is only cleared from `confirmLogout`.
 */
export function useLogoutFlow({ apiLogoutPath, extraCleanup, matchesProtectedQuery, redirectTo }: LogoutFlowOptions) {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const clearSessions = useSessionStore((state) => state.clearSessions)
  const [loggingOut, setLoggingOut] = useState(false)
  const [confirmOpen, setConfirmOpen] = useState(false)

  const requestLogout = () => setConfirmOpen(true)
  const cancelLogout = () => setConfirmOpen(false)

  const confirmLogout = async () => {
    if (loggingOut) return
    setLoggingOut(true)
    try {
      if (apiLogoutPath) await api.post(apiLogoutPath)
    } catch (error) {
      if (import.meta.env.DEV) {
        const problem = parseApiProblem(error)
        console.warn('Logout endpoint failed; clearing the local session.', {
          status: problem.status,
          title: problem.technicalTitle ?? problem.title,
          traceId: problem.traceId,
        })
      }
    } finally {
      setConfirmOpen(false)
      clearSessions()
      await queryClient.cancelQueries({ predicate: ({ queryKey }) => matchesProtectedQuery(queryKey) })
      queryClient.removeQueries({ predicate: ({ queryKey }) => matchesProtectedQuery(queryKey) })
      extraCleanup?.()
      setLoggingOut(false)
      void navigate(redirectTo, { replace: true })
    }
  }

  return { cancelLogout, confirmLogout, confirmOpen, loggingOut, requestLogout }
}
