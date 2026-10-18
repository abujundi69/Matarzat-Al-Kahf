import {
  useCallback,
  useEffect,
  useId,
  useLayoutEffect,
  useRef,
  useState,
  type CSSProperties,
  type FocusEventHandler,
  type MouseEventHandler,
  type ReactNode,
} from 'react'
import { createPortal } from 'react-dom'

type TooltipTriggerProps = {
  'aria-describedby'?: string
  onBlur: FocusEventHandler<HTMLElement>
  onFocus: FocusEventHandler<HTMLElement>
  onMouseEnter: MouseEventHandler<HTMLElement>
  onMouseLeave: MouseEventHandler<HTMLElement>
}

type TooltipProps = {
  children: (triggerProps: TooltipTriggerProps) => ReactNode
  content: ReactNode
  disabled?: boolean
}

type TooltipPosition = {
  corrected?: boolean
  left?: number
  right?: number
  top: number
}

const viewportEdge = 8
const triggerGap = 10

/**
 * A portal-based tooltip that remains visible outside scroll and overflow
 * containers. It follows the trigger's writing direction, supports keyboard
 * focus, and corrects its final rendered position to stay in the viewport.
 */
export function Tooltip({ children, content, disabled = false }: TooltipProps) {
  const tooltipId = useId()
  const tooltipRef = useRef<HTMLSpanElement>(null)
  const triggerRef = useRef<HTMLElement>(null)
  const [position, setPosition] = useState<TooltipPosition | null>(null)

  const positionFromTrigger = useCallback(() => {
    const trigger = triggerRef.current
    if (!trigger || disabled) return

    const bounds = trigger.getBoundingClientRect()
    const direction = getComputedStyle(trigger).direction || document.documentElement.dir
    const top = Math.min(
      Math.max(bounds.top + bounds.height / 2, viewportEdge),
      window.innerHeight - viewportEdge,
    )

    if (direction === 'rtl') {
      setPosition({
        right: Math.max(window.innerWidth - bounds.left + triggerGap, viewportEdge),
        top,
      })
    } else {
      setPosition({
        left: Math.max(bounds.right + triggerGap, viewportEdge),
        top,
      })
    }
  }, [disabled])

  const show: MouseEventHandler<HTMLElement> & FocusEventHandler<HTMLElement> = (event) => {
    triggerRef.current = event.currentTarget
    positionFromTrigger()
  }

  const hide = () => {
    triggerRef.current = null
    setPosition(null)
  }

  useEffect(() => {
    if (!position) return

    const dismissWithEscape = (event: KeyboardEvent) => {
      if (event.key === 'Escape') setPosition(null)
    }
    window.addEventListener('resize', positionFromTrigger)
    window.addEventListener('scroll', positionFromTrigger, true)
    document.addEventListener('keydown', dismissWithEscape)
    return () => {
      window.removeEventListener('resize', positionFromTrigger)
      window.removeEventListener('scroll', positionFromTrigger, true)
      document.removeEventListener('keydown', dismissWithEscape)
    }
  }, [position, positionFromTrigger])

  useEffect(() => {
    if (disabled) hide()
  }, [disabled])

  useLayoutEffect(() => {
    const tooltip = tooltipRef.current
    if (!tooltip || !position || position.corrected) return

    const bounds = tooltip.getBoundingClientRect()
    const maxRight = window.innerWidth - viewportEdge
    const maxBottom = window.innerHeight - viewportEdge
    const horizontalShift = bounds.left < viewportEdge
      ? viewportEdge - bounds.left
      : bounds.right > maxRight
        ? maxRight - bounds.right
        : 0
    const verticalShift = bounds.top < viewportEdge
      ? viewportEdge - bounds.top
      : bounds.bottom > maxBottom
        ? maxBottom - bounds.bottom
        : 0

    if (Math.abs(horizontalShift) < .5 && Math.abs(verticalShift) < .5) return

    setPosition((current) => {
      if (!current) return current
      return {
        left: current.left === undefined ? undefined : current.left + horizontalShift,
        right: current.right === undefined ? undefined : current.right - horizontalShift,
        top: current.top + verticalShift,
        corrected: true,
      }
    })
  }, [position])

  const triggerProps: TooltipTriggerProps = {
    'aria-describedby': disabled ? undefined : tooltipId,
    onBlur: hide,
    onFocus: show,
    onMouseEnter: show,
    onMouseLeave: hide,
  }
  const tooltipStyle: CSSProperties | undefined = position
    ? { left: position.left, right: position.right, top: position.top }
    : undefined

  return (
    <>
      {children(triggerProps)}
      {!disabled && createPortal(
        <span className="app-tooltip" hidden={!position} id={tooltipId} ref={tooltipRef} role="tooltip" style={tooltipStyle}>
          {content}
        </span>,
        document.body,
      )}
    </>
  )
}
