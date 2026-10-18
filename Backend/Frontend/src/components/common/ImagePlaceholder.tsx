import logoSmall from '@/assets/brand/logo-sm.png'

/**
 * Stand-in for a product, category, or cart-line photo that does not exist yet: warm cream with a
 * faint stitch field and the emblem as a quiet watermark. It always fills its parent, which sets
 * the size and shape.
 */
export function ImagePlaceholder({ className = '' }: { className?: string }) {
  return (
    <div aria-hidden className={`placeholder-art grid size-full place-items-center overflow-hidden ${className}`}>
      <img alt="" className="size-[46%] max-w-24 select-none object-contain opacity-35" draggable={false} src={logoSmall} />
    </div>
  )
}
