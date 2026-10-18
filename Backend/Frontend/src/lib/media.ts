import { env } from '@/config/env'
import { api } from '@/lib/api/client'

export const MAX_ADMIN_IMAGE_BYTES = 5 * 1024 * 1024

export type AdminImagePurpose =
  | 'logos'
  | 'products'
  | 'categories'
  | 'banners'
  | 'content'
  | 'other'

type UploadedFile = {
  id?: string
  url?: string | null
}

const absoluteMediaPattern = /^(?:https?:|data:|blob:)/i
const uploadedMediaPattern = /^\/?uploads\//i
const supportedImageTypes = new Set(['image/jpeg', 'image/png', 'image/webp'])
const supportedImageExtension = /\.(?:jpe?g|png|webp)$/i

const apiOrigin = () => {
  const fallbackOrigin = typeof window === 'undefined' ? 'http://localhost' : window.location.origin
  return new URL(env.VITE_API_BASE_URL, fallbackOrigin).origin
}

/**
 * Uploaded media is served outside /api/v1. When the API is hosted on a
 * different origin, a root-relative /uploads URL must therefore be anchored
 * to the API origin rather than the frontend origin.
 */
export const resolveMediaUrl = (value: string | null | undefined) => {
  const url = value?.trim()
  if (!url || absoluteMediaPattern.test(url)) return url ?? ''
  const isApiPath = url.startsWith('/api/')
  if (!isApiPath && !uploadedMediaPattern.test(url)) return url

  const mediaPath = url.startsWith('/') ? url : `/${url}`
  return new URL(mediaPath, `${apiOrigin()}/`).toString()
}

/**
 * The development seed points banners and the store logo at the generic product placeholder.
 * That file stands in for "no image yet", so it must never be shown as if it were real content.
 */
export const isGenericPlaceholderUrl = (value: string | null | undefined) =>
  /\/images\/products\/heritage-placeholder\.svg(?:\?|$)/i.test(value ?? '')

export const validateAdminImage = (file: File) => {
  if (file.size > MAX_ADMIN_IMAGE_BYTES) return 'size' as const
  if (!supportedImageTypes.has(file.type) || !supportedImageExtension.test(file.name)) return 'type' as const
  return null
}

export const uploadAdminImage = async (file: File, purpose: AdminImagePurpose) => {
  const body = new FormData()
  body.append('file', file)
  const response = await api.post<UploadedFile>('/admin/files', body, {
    params: { isPublic: true, purpose },
  })
  const url = response.data.url?.trim()
  if (!url) throw new Error('The upload response did not include a media URL.')
  return { ...response.data, url }
}
