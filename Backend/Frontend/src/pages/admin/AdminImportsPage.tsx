import { useEffect, useMemo, useRef, useState } from 'react'
import { Download, FileSpreadsheet, Upload } from 'lucide-react'
import { useMutation } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { api, downloadFile } from '@/lib/api/client'
import { parseApiProblem } from '@/lib/api/problemDetails'
import { createIdempotencyKey } from '@/lib/utils'
import type { Schema } from '@/types/models'
import { adminPermissions } from '@/config/adminResources'
import { ForbiddenState } from '@/components/ui/Feedback'
import { useSessionStore } from '@/stores/sessionStore'

type ImportType = 'products' | 'inventory'

export function AdminImportsPage() {
  const { t, i18n } = useTranslation()
  const permissions = useSessionStore((state) => state.admin?.permissions ?? [])
  const allowedTypes = useMemo<ImportType[]>(() => {
    const result: ImportType[] = []
    if (permissions.includes(adminPermissions.importsProducts)) result.push('products')
    if (permissions.includes(adminPermissions.importsInventory)) result.push('inventory')
    return result
  }, [permissions])
  const [type, setType] = useState<ImportType>(allowedTypes[0] ?? 'products')
  const [file, setFile] = useState<File | null>(null)
  const [preview, setPreview] = useState<Schema<'ImportPreviewResponse'> | null>(null)
  const [downloading, setDownloading] = useState(false)
  const fileInputRef = useRef<HTMLInputElement>(null)
  const commitIdempotencyKey = useRef(createIdempotencyKey())

  useEffect(() => {
    if (allowedTypes.length && !allowedTypes.includes(type)) {
      setType(allowedTypes[0])
      setFile(null)
      setPreview(null)
    }
  }, [allowedTypes, type])

  const upload = useMutation({
    mutationFn: async () => {
      if (!file) throw new Error(t('admin.imports.fileRequired'))
      const data = new FormData()
      data.append('file', file)
      return api
        .post<Schema<'ImportPreviewResponse'>>(
          `/admin/imports/${type}/preview`,
          data,
          { params: type === 'products' ? { allowNewProducts: true } : undefined },
        )
        .then((response) => response.data)
    },
    onSuccess: (result) => {
      commitIdempotencyKey.current = createIdempotencyKey()
      setPreview(result)
    },
    onError: (error) => {
      const problem = parseApiProblem(error)
      toast.error(t('admin.imports.previewFailed'), { description: problem.title })
    },
  })
  const commit = useMutation({
    mutationFn: () => api
      .post<Schema<'ImportCommitResponse'>>(
        `/admin/imports/${type}/commit`,
        { confirm: true },
        {
          params: { importId: preview?.importId },
          headers: { 'Idempotency-Key': commitIdempotencyKey.current },
        },
      )
      .then((response) => response.data),
    onSuccess: () => {
      toast.success(t('admin.imports.committed'))
      setPreview(null)
      setFile(null)
      commitIdempotencyKey.current = createIdempotencyKey()
      if (fileInputRef.current) fileInputRef.current.value = ''
    },
    onError: (error) => {
      const problem = parseApiProblem(error)
      toast.error(t('admin.imports.commitFailed'), { description: problem.title })
    },
  })

  if (!allowedTypes.length) return <ForbiddenState />

  const downloadTemplate = async () => {
    if (downloading) return
    setDownloading(true)
    try {
      const language = i18n.language.startsWith('ar') ? 'Arabic' : 'English'
      await downloadFile(
        `/admin/imports/${type}/template?language=${language}`,
        `${type}-template.xlsx`,
      )
      toast.success(t('admin.imports.templateReady'))
    } catch (error) {
      const problem = parseApiProblem(error)
      toast.error(t('admin.imports.templateFailed'), { description: problem.title })
    } finally {
      setDownloading(false)
    }
  }

  return (
    <section className="min-w-0">
      <h1 className="break-words text-3xl font-extrabold [overflow-wrap:anywhere]">{t('admin.imports.title')}</h1>
      <div className="card mt-6 min-w-0 p-4 sm:p-6">
        <div className="flex min-w-0 flex-wrap gap-3">
          <label className="w-full min-w-0 sm:w-auto">
            <span className="sr-only">{t('admin.imports.type')}</span>
            <select
              className="field w-full sm:min-w-56"
              value={type}
              onChange={(event) => {
                setType(event.target.value as ImportType)
                setFile(null)
                setPreview(null)
                if (fileInputRef.current) fileInputRef.current.value = ''
              }}
            >
              {allowedTypes.map((item) => <option key={item} value={item}>{t(`admin.imports.types.${item}`)}</option>)}
            </select>
          </label>
          <button className="button-secondary w-full min-w-0 whitespace-normal sm:w-auto" disabled={downloading} onClick={() => void downloadTemplate()} type="button">
            <Download aria-hidden className="shrink-0" size={17} />
            {downloading ? t('admin.imports.downloading') : t('admin.imports.template')}
          </button>
        </div>
        <label className="mt-6 grid min-h-48 min-w-0 cursor-pointer place-items-center gap-3 rounded-2xl border-2 border-dashed border-[var(--heritage-sand)] bg-[var(--heritage-beige)]/30 p-4 text-center sm:p-6">
          <FileSpreadsheet aria-hidden className="text-[var(--heritage-burgundy)]" size={40} />
          <span className="max-w-full break-words font-bold [overflow-wrap:anywhere]" dir="auto">{file?.name ?? t('admin.imports.chooseFile')}</span>
          <input
            accept=".xlsx"
            className="sr-only"
            onChange={(event) => {
              setFile(event.target.files?.[0] ?? null)
              setPreview(null)
            }}
            ref={fileInputRef}
            type="file"
          />
        </label>
        <button
          className="button-primary mt-5 w-full min-w-0 whitespace-normal sm:w-auto"
          disabled={!file || upload.isPending}
          onClick={() => upload.mutate()}
          type="button"
        >
          <Upload aria-hidden className="shrink-0" size={17} />
          {upload.isPending ? t('admin.imports.uploading') : t('admin.imports.previewAction')}
        </button>
      </div>
      {preview && (
        <div className="card mt-6 min-w-0 p-4 sm:p-6">
          <h2 className="break-words text-xl font-bold [overflow-wrap:anywhere]">{t('admin.imports.previewTitle')}</h2>
          <div className="mt-4 flex flex-wrap gap-3">
            <span className="badge max-w-full whitespace-normal break-words [overflow-wrap:anywhere]">{t('admin.imports.totalRows', { count: preview.totalRows ?? 0 })}</span>
            <span className="badge max-w-full whitespace-normal break-words [overflow-wrap:anywhere]">{t('admin.imports.validRows', { count: preview.validRows ?? 0 })}</span>
            <span className="badge max-w-full whitespace-normal break-words [overflow-wrap:anywhere]">{t('admin.imports.invalidRows', { count: preview.invalidRows ?? 0 })}</span>
          </div>
          {!!preview.errors?.length && (
            <div className="mt-5 min-w-0">
              <div className="grid gap-3 md:hidden">
                {preview.errors.map((error, index) => (
                  <article className="min-w-0 rounded-xl border bg-stone-50 p-4" key={`${error.rowNumber ?? index}-${error.field ?? ''}`}>
                    <dl className="grid min-w-0 gap-3">
                      <div className="grid min-w-0 gap-1">
                        <dt className="muted text-xs font-semibold">{t('admin.imports.row')}</dt>
                        <dd className="break-words text-sm font-semibold [overflow-wrap:anywhere]" dir="ltr">{error.rowNumber}</dd>
                      </div>
                      <div className="grid min-w-0 gap-1">
                        <dt className="muted text-xs font-semibold">{t('admin.imports.field')}</dt>
                        <dd className="break-words text-sm font-semibold [overflow-wrap:anywhere]" dir="auto">{error.field}</dd>
                      </div>
                      <div className="grid min-w-0 gap-1">
                        <dt className="muted text-xs font-semibold">{t('admin.imports.error')}</dt>
                        <dd className="break-words text-sm [overflow-wrap:anywhere]" dir="auto">{error.message}</dd>
                      </div>
                    </dl>
                  </article>
                ))}
              </div>
              <div className="hidden overflow-x-auto md:block">
              <table className="w-full min-w-[560px] table-auto text-sm">
                <thead>
                  <tr>
                    <th className="p-3 text-start">{t('admin.imports.row')}</th>
                    <th className="p-3 text-start">{t('admin.imports.field')}</th>
                    <th className="p-3 text-start">{t('admin.imports.error')}</th>
                  </tr>
                </thead>
                <tbody>
                  {preview.errors.map((error, index) => (
                    <tr className="border-t" key={`${error.rowNumber ?? index}-${error.field ?? ''}`}>
                      <td className="break-words p-3 [overflow-wrap:anywhere]" dir="ltr">{error.rowNumber}</td>
                      <td className="max-w-52 break-words p-3 [overflow-wrap:anywhere]" dir="auto">{error.field}</td>
                      <td className="max-w-96 break-words p-3 [overflow-wrap:anywhere]" dir="auto">{error.message}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
              </div>
            </div>
          )}
          <button
            className="button-primary mt-5 w-full min-w-0 whitespace-normal sm:w-auto"
            disabled={commit.isPending || Boolean(preview.invalidRows)}
            onClick={() => commit.mutate()}
            type="button"
          >
            {commit.isPending ? t('admin.imports.committing') : t('admin.imports.commit')}
          </button>
        </div>
      )}
    </section>
  )
}
