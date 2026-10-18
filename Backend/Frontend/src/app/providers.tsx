import { useState, type ReactNode } from 'react'
import { QueryClientProvider } from '@tanstack/react-query'
import { ReactQueryDevtools } from '@tanstack/react-query-devtools'
import { RouterProvider } from 'react-router-dom'
import { Toaster } from 'sonner'
import { queryClient } from './queryClient'
import { router } from './router'
import { ErrorBoundary } from '@/components/common/ErrorBoundary'
import { SplashScreen } from '@/components/common/SplashScreen'

const splashSeenKey = 'matarzat-splash-seen'

/**
 * The intro belongs to arriving at the store, not to every page load. Replaying it after a
 * refresh would make an administrator wait through branding before each piece of work, so it is
 * shown once per browser tab. Storage can be unavailable (private windows, blocked site data),
 * in which case the intro simply plays and the app still works.
 */
const shouldShowSplash = () => {
  try {
    if (sessionStorage.getItem(splashSeenKey)) return false
    sessionStorage.setItem(splashSeenKey, '1')
  } catch {
    // Ignore: an unavailable store only means the intro may replay.
  }

  return true
}

export function AppProviders({ children }: { children?: ReactNode }) {
  // Mounted once per real page load; React Router navigation never remounts this tree,
  // so the splash never replays when the user just moves between pages.
  const [showSplash, setShowSplash] = useState(shouldShowSplash)

  return <ErrorBoundary><QueryClientProvider client={queryClient}>
    {/*
      The intro is a visual overlay, never an input blocker. Marking the application inert while
      it played meant the first keystrokes of anyone who started typing straight away were
      silently dropped; the overlay already absorbs pointer events, and the first interaction
      dismisses it, so that keystroke now reaches the field it was meant for.
    */}
    <div className="contents">{children ?? <RouterProvider router={router} />}</div>
    {showSplash && <SplashScreen onFinished={() => setShowSplash(false)} />}
    <Toaster richColors position="top-center" />
    <ReactQueryDevtools initialIsOpen={false} />
  </QueryClientProvider></ErrorBoundary>
}
