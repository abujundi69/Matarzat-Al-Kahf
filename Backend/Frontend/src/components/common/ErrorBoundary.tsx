import { Component, type ErrorInfo, type ReactNode } from 'react'
import i18n from '@/i18n'

export class ErrorBoundary extends Component<{ children: ReactNode }, { failed: boolean }> {
  state = { failed: false }

  static getDerivedStateFromError() {
    return { failed: true }
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    if (import.meta.env.DEV) {
      console.error('Unexpected React rendering error', error, info)
    }
  }

  render() {
    if (!this.state.failed) return this.props.children

    return (
      <main className="page-shell grid min-h-screen min-h-dvh place-items-center py-6">
        <div className="card min-w-0 max-w-2xl p-6 text-center sm:p-10" role="alert">
          <h1 className="text-2xl font-bold [overflow-wrap:anywhere]">{i18n.t('errors.render.title')}</h1>
          <p className="muted mt-3 [overflow-wrap:anywhere]">{i18n.t('errors.render.message')}</p>
          <button className="button-primary mt-5" onClick={() => window.location.reload()} type="button">
            {i18n.t('actions.reload')}
          </button>
        </div>
      </main>
    )
  }
}
