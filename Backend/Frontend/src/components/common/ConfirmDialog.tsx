import { useEffect, useId, useRef, useState } from 'react'
import { createPortal } from 'react-dom'
import { useTranslation } from 'react-i18next'

// Matches the .dialog-panel-exit CSS animation duration, plus a small buffer. Acts as a
// fallback in case the animationend event never fires (throttled tab, unsupported environment).
const EXIT_ANIMATION_MS = 220

type ConfirmDialogProps = {
  cancelLabel: string
  confirmLabel: string
  message: string
  onCancel: () => void
  onConfirm: () => void
  open: boolean
  pending?: boolean
  title: string
  tone?: 'default' | 'danger'
}

/** Animated, accessible replacement for window.confirm(). Stays mounted briefly to play its close animation. */
export function ConfirmDialog({
  cancelLabel,
  confirmLabel,
  message,
  onCancel,
  onConfirm,
  open,
  pending = false,
  title,
  tone = 'default',
}: ConfirmDialogProps) {
  const { t } = useTranslation()
  const [rendered, setRendered] = useState(open)
  const [closing, setClosing] = useState(false)
  const panelRef = useRef<HTMLDivElement>(null)
  const cancelRef = useRef<HTMLButtonElement>(null)
  const headingId = useId()
  const messageId = useId()

  useEffect(() => {
    if (open) {
      setRendered(true)
      setClosing(false)
    } else if (rendered) {
      setClosing(true)
    }
  }, [open, rendered])

  useEffect(() => {
    if (!closing) return
    const timer = window.setTimeout(() => setRendered(false), EXIT_ANIMATION_MS)
    return () => window.clearTimeout(timer)
  }, [closing])

  useEffect(() => {
    if (!rendered) return

    const previouslyFocused = document.activeElement as HTMLElement | null
    const previousOverflow = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    cancelRef.current?.focus()

    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        event.preventDefault()
        onCancel()
        return
      }
      if (event.key !== 'Tab') return

      const focusable = Array.from(
        panelRef.current?.querySelectorAll<HTMLElement>('button:not([disabled])') ?? [],
      )
      if (!focusable.length) return
      const first = focusable[0]
      const last = focusable[focusable.length - 1]
      if (event.shiftKey && document.activeElement === first) {
        event.preventDefault()
        last.focus()
      } else if (!event.shiftKey && document.activeElement === last) {
        event.preventDefault()
        first.focus()
      }
    }
    document.addEventListener('keydown', onKeyDown)
    return () => {
      document.removeEventListener('keydown', onKeyDown)
      document.body.style.overflow = previousOverflow
      previouslyFocused?.focus()
    }
  }, [rendered, onCancel])

  if (!rendered) return null

  // Portalled to <body> so it always covers the full viewport, regardless of whether an
  // ancestor (e.g. the sticky header's backdrop-blur) would otherwise create a containing
  // block that traps this "fixed inset-0" overlay inside a smaller box.
  return createPortal(
    <div
      className={`fixed inset-0 z-[90] grid place-items-center bg-black/55 p-4 ${closing ? 'dialog-backdrop-exit' : 'dialog-backdrop-enter'}`}
      onMouseDown={(event) => {
        if (event.target === event.currentTarget) onCancel()
      }}
      role="presentation"
    >
      <div
        aria-describedby={messageId}
        aria-labelledby={headingId}
        aria-modal="true"
        className={`card w-full max-w-sm p-6 ${closing ? 'dialog-panel-exit' : 'dialog-panel-enter'}`}
        onAnimationEnd={() => {
          if (closing) setRendered(false)
        }}
        ref={panelRef}
        role="alertdialog"
      >
        <h2 className="[overflow-wrap:anywhere] text-lg font-extrabold text-[var(--heritage-burgundy)]" id={headingId}>{title}</h2>
        <p className="muted mt-2 [overflow-wrap:anywhere] leading-7" id={messageId}>{message}</p>
        <div className="mt-6 flex flex-wrap justify-end gap-3">
          <button className="button-secondary max-w-full whitespace-normal text-center" disabled={pending} onClick={onCancel} ref={cancelRef} type="button">
            {cancelLabel}
          </button>
          <button
            className={`max-w-full whitespace-normal text-center ${tone === 'danger' ? 'button-danger' : 'button-primary'}`}
            disabled={pending}
            onClick={onConfirm}
            type="button"
          >
            {pending ? t('states.loading') : confirmLabel}
          </button>
        </div>
      </div>
    </div>,
    document.body,
  )
}
