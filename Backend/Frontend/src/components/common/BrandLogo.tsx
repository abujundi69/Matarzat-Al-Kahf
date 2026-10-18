import logoLarge from '@/assets/brand/logo.png'
import logoMedium from '@/assets/brand/logo-md.png'
import logoSmall from '@/assets/brand/logo-sm.png'
import { useStoreBranding } from '@/features/settings/storeBranding'

const renditions = {
  sm: { size: 192, src: logoSmall },
  md: { size: 384, src: logoMedium },
  lg: { size: 768, src: logoLarge },
} as const

type BrandLogoProps = {
  /**
   * Pick the smallest source that is still at least 2x the size the emblem is drawn at, so it is
   * sharp on high-density screens without downloading more than it needs:
   * `sm` (192px) up to about 96px: header, sidebar, menus, footer, sign-in on phones;
   * `md` (384px) up to about 190px: splash and the story band;
   * `lg` (768px) up to about 380px: the sign-in brand panel.
   */
  variant?: keyof typeof renditions
  /** Sizing only (for example `size-14`). The image is never cropped or stretched. */
  className?: string
}

/**
 * The store emblem. The bundled brand logo is the default; a logo uploaded under Settings
 * overrides it. Decorative on purpose: the brand name is always written out next to it.
 */
export function BrandLogo({ variant = 'sm', className = '' }: BrandLogoProps) {
  const { logoUrl } = useStoreBranding()
  const { size, src } = renditions[variant]

  return (
    <img
      alt=""
      className={`shrink-0 select-none object-contain ${className}`}
      decoding="async"
      draggable={false}
      height={size}
      src={logoUrl || src}
      width={size}
    />
  )
}
