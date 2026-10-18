import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Upload } from 'lucide-react'
import { useRef, useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router-dom'
import { toast } from 'sonner'
import { EmptyState, ErrorState, LoadingState } from '@/components/ui/Feedback'
import { api } from '@/lib/api/client'
import { parseApiProblem } from '@/lib/api/problemDetails'
import { OrderDetailsView } from '@/pages/orders/OrderDetailsView'
import type { Schema } from '@/types/models'

type OrderDetails = Schema<'OrderDetailsResponse'>

const validId = (value: string | undefined) =>
  Boolean(value && /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(value))

export function CustomerOrderDetailsPage() {
  const { t, i18n } = useTranslation()
  const { id } = useParams()
  const queryClient = useQueryClient()
  const [proofFile, setProofFile] = useState<File | null>(null)
  const fileInputRef = useRef<HTMLInputElement>(null)
  const idIsValid = validId(id)
  const query = useQuery({
    enabled: idIsValid,
    queryKey: ['customer', 'order', id, i18n.language],
    queryFn: () => api
      .get<OrderDetails>(`/customer/orders/${id}`)
      .then((response) => response.data),
  })
  const manualPayment = [...(query.data?.payments ?? [])].reverse().find((payment) =>
    payment.id
    && (payment.method === 'BankTransfer' || payment.method === 'ElectronicWallet'))
  const canUploadProof = manualPayment?.status === 'Pending' || manualPayment?.status === 'Failed'
  const instructions = useQuery({
    enabled: idIsValid && Boolean(manualPayment),
    queryKey: ['customer', 'order', id, 'payment-instructions', i18n.language],
    queryFn: () => api
      .get<Schema<'PaymentMethodResponse'>>(`/customer/orders/${id}/payments/instructions`)
      .then((response) => response.data),
  })
  const uploadProof = useMutation({
    mutationFn: (file: File) => {
      const body = new FormData()
      body.append('file', file)
      return api
        .post<Schema<'PaymentResponse'>>(`/customer/orders/${id}/payments/${manualPayment?.id}/proof`, body)
        .then((response) => response.data)
    },
    onSuccess: async () => {
      setProofFile(null)
      if (fileInputRef.current) fileInputRef.current.value = ''
      toast.success(t('orderDetails.proofUploaded'))
      await Promise.all([
        query.refetch(),
        queryClient.invalidateQueries({ queryKey: ['customer', 'orders'] }),
        queryClient.invalidateQueries({ queryKey: ['customer', 'payments'] }),
      ])
    },
    onError: (error) => toast.error(parseApiProblem(error).title),
  })

  const submitProof = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!proofFile || uploadProof.isPending) return
    const accepted = ['image/jpeg', 'image/png', 'image/webp', 'application/pdf']
    if (proofFile.size <= 0 || proofFile.size > 10 * 1024 * 1024 || !accepted.includes(proofFile.type)) {
      toast.error(t('orderDetails.invalidProofFile'))
      return
    }
    uploadProof.mutate(proofFile)
  }

  if (!idIsValid) return <ErrorState status={404} />
  if (query.isLoading) return <LoadingState />
  if (query.isError) return <ErrorState error={query.error} retry={() => void query.refetch()} />
  if (!query.data?.id) return <EmptyState message={t('orderDetails.empty')} />

  return <div className="grid min-w-0 gap-6">
    {manualPayment && <section className="card min-w-0 border-s-4 border-s-[var(--heritage-gold)] p-5 sm:p-6">
      <h2 className="[overflow-wrap:anywhere] text-xl font-extrabold">{t('orderDetails.paymentProof')}</h2>
      {instructions.data?.instructions && <p className="muted mt-2 whitespace-pre-wrap [overflow-wrap:anywhere] text-start" dir="auto">{instructions.data.instructions}</p>}
      {canUploadProof
        ? <form className="mt-5 grid min-w-0 gap-3 sm:grid-cols-[minmax(0,1fr)_auto] sm:items-end" onSubmit={submitProof}>
          <label className="min-w-0 font-bold">
            <span className="block [overflow-wrap:anywhere]">{t('orderDetails.chooseProof')}</span>
            <input
              accept="image/jpeg,image/png,image/webp,application/pdf"
              aria-label={t('orderDetails.chooseProof')}
              className="field mt-2 file:me-3 file:rounded-xl file:border-0 file:bg-[var(--heritage-beige)] file:px-3 file:py-1 file:font-bold"
              disabled={uploadProof.isPending}
              onChange={(event) => setProofFile(event.target.files?.[0] ?? null)}
              ref={fileInputRef}
              type="file"
            />
            <span className="muted mt-1 block text-xs">{t('orderDetails.proofRequirements')}</span>
          </label>
          <button className="button-primary w-full sm:w-auto" disabled={!proofFile || uploadProof.isPending} type="submit">
            <Upload aria-hidden size={18} />
            {uploadProof.isPending ? t('orderDetails.uploadingProof') : t('orderDetails.uploadProof')}
          </button>
        </form>
        : <p className="mt-4 rounded-xl bg-[var(--heritage-beige)] p-3 font-bold" role="status">
          {manualPayment.status === 'Processing'
            ? t('orderDetails.proofUnderReview')
            : t('orderDetails.proofNotRequired')}
        </p>}
    </section>}
    <OrderDetailsView backTo="/account/orders" order={query.data} />
  </div>
}
