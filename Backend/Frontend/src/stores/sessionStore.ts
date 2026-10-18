import { create } from 'zustand'
import { persist } from 'zustand/middleware'

type SessionKind = 'customer' | 'admin'
export type SessionActor = {
  id?: string
  fullName?: string
  phoneNumber?: string
  email?: string | null
  preferredLanguage?: string
  actorType: string
  roles: string[]
}
export type Session = {
  token: string
  expiresAt?: string
  permissions: string[]
  actor?: SessionActor
}
type SessionState = {
  customer: Session | null
  admin: Session | null
  guestCartToken: string
  setSession: (kind: SessionKind, session: Session | null) => void
  setAuthenticatedSession: (kind: SessionKind, session: Session) => void
  clearSessions: () => void
  setGuestCartToken: (token: string) => void
}
type PersistedSessionState = Pick<SessionState, 'customer' | 'admin' | 'guestCartToken'>

export const isSessionValid = (session: Session | null | undefined) => {
  if (!session?.token) return false
  if (!session.expiresAt) return true

  const expiration = Date.parse(session.expiresAt)
  return Number.isFinite(expiration) && expiration > Date.now()
}

export const useSessionStore = create<SessionState>()(
  persist<SessionState, [], [], PersistedSessionState>(
    (set) => ({
      customer: null,
      admin: null,
      // The backend creates the opaque guest-cart token. Inventing a client-side
      // token makes the first cart request look like a lookup for a missing cart.
      guestCartToken: '',
      setSession: (kind, session) => set({ [kind]: session }),
      // A credential belongs to exactly one backend actor. Keeping the other
      // slot empty prevents a stale token from influencing route guards or API
      // token selection after a different role signs in.
      setAuthenticatedSession: (kind, session) => set({
        admin: kind === 'admin' ? session : null,
        customer: kind === 'customer' ? session : null,
      }),
      clearSessions: () => set({ admin: null, customer: null }),
      setGuestCartToken: (guestCartToken) => set({ guestCartToken }),
    }),
    {
      name: 'matarzat-session',
      version: 2,
      migrate: (persistedState, version) => {
        const state = persistedState as Partial<PersistedSessionState>
        return {
          customer: state.customer ?? null,
          admin: state.admin ?? null,
          guestCartToken: version < 2 ? '' : (state.guestCartToken ?? ''),
        }
      },
      partialize: ({ customer, admin, guestCartToken }) => ({ customer, admin, guestCartToken }),
    },
  ),
)
