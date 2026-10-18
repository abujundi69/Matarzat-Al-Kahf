import { useEffect, useId, useState } from 'react'
import { ImageUp, RotateCcw } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { resolveMediaUrl, validateAdminImage } from '@/lib/media'
import { AdminImagePreview } from '@/components/admin/AdminImagePreview'

type Props = {
  currentUrl?: string | null
  /** What visitors see while nothing is uploaded. */
  defaultPreviewSrc?: string
  disabled?: boolean
  error?: string
  label: string
  onError?: (message: string) => void
  onSelect: (file: File | null) => void
  selectedFile?: File | null
}

export function ImageFilePicker({
  currentUrl,
  defaultPreviewSrc,
  disabled,
  error,
  label,
  onError,
  onSelect,
  selectedFile,
}: Props) {
  const { t } = useTranslation()
  const inputId = useId()
  const [objectUrl, setObjectUrl] = useState('')

  useEffect(() => {
    if (!selectedFile || typeof URL.createObjectURL !== 'function') {
      setObjectUrl('')
      return
    }
    const next = URL.createObjectURL(selectedFile)
    setObjectUrl(next)
    return () => URL.revokeObjectURL(next)
  }, [selectedFile])

  const previewUrl = objectUrl || resolveMediaUrl(currentUrl)
  const choose = (file?: File) => {
    if (!file) return
    const validation = validateAdminImage(file)
    if (validation) {
      const message = t(validation === 'size' ? 'uploads.errors.size' : 'uploads.errors.type')
      onError?.(message)
      return
    }
    onError?.('')
    onSelect(file)
  }

  return (
    <div className="grid min-w-0 gap-2 rounded-xl border border-stone-200 bg-white p-3">
      <span className="break-words text-sm font-semibold [overflow-wrap:anywhere]">{label}</span>
      {previewUrl ? (
        <div className="grid min-h-36 place-items-center overflow-hidden rounded-xl bg-stone-100 p-2">
          <AdminImagePreview alt={t('uploads.previewAlt', { label })} className="h-44 w-full" src={previewUrl} />
        </div>
      ) : defaultPreviewSrc ? (
        <div className="grid min-h-36 place-items-center overflow-hidden rounded-xl bg-[var(--heritage-beige)] p-3">
          <img alt="" className="size-24 object-contain" src={defaultPreviewSrc} />
        </div>
      ) : (
        <div className="grid min-h-28 place-items-center rounded-xl border border-dashed border-stone-300 bg-stone-50 p-4 text-center text-sm text-[var(--heritage-muted)]">
          {t('uploads.noImage')}
        </div>
      )}
      <div className="flex min-w-0 flex-wrap items-center gap-2">
        <label className="button-secondary max-w-full cursor-pointer whitespace-normal" htmlFor={inputId}>
          <ImageUp aria-hidden size={18} />
          {t(currentUrl || selectedFile ? 'uploads.replace' : 'uploads.choose')}
        </label>
        <input
          accept=".jpg,.jpeg,.png,.webp,image/jpeg,image/png,image/webp"
          className="sr-only"
          disabled={disabled}
          id={inputId}
          onChange={(event) => {
            choose(event.target.files?.[0])
            event.target.value = ''
          }}
          type="file"
        />
        {selectedFile && (
          <button className="button-secondary whitespace-normal" disabled={disabled} onClick={() => onSelect(null)} type="button">
            <RotateCcw aria-hidden size={16} />
            {t('uploads.restore')}
          </button>
        )}
      </div>
      {selectedFile && <span className="break-all text-xs text-[var(--heritage-muted)]" dir="auto">{selectedFile.name}</span>}
      <span className="text-xs text-[var(--heritage-muted)]">{t('uploads.help')}</span>
      {error && <span className="break-words text-xs text-[var(--state-danger-text)] [overflow-wrap:anywhere]" role="alert">{error}</span>}
    </div>
  )
}
