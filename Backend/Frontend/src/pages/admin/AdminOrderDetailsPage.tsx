import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Download, ShieldCheck, XCircle } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router-dom'
import { toast } from 'sonner'
import { EmptyState, ErrorState, ForbiddenState, InlineLoader, LoadingState } from '@/components/ui/Feedback'
import { adminPermissions } from '@/config/adminResources'
import { api, downloadFile } from '@/lib/api/client'
import { parseApiProblem, type ApiProblem } from '@/lib/api/problemDetails'
import { OrderDetailsView } from '@/pages/orders/OrderDetailsView'
import { useSessionStore } from '@/stores/sessionStore'
import type { Schema } from '@/types/models'

type OrderDetails = Schema<'OrderDetailsResponse'>
type ChangeOrderStatusRequest = Schema<'ChangeOrderStatusRequest'>
type OrderStatus = NonNullable<ChangeOrderStatusRequest['status']>

const validId = (value: string | undefined) =>
  Boolean(value && /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(value))

const allowedTransitions: Record<OrderStatus, readonly OrderStatus[]> = {
  New: ['UnderReview', 'Confirmed', 'Cancelled', 'PaymentFailed'],
  UnderReview: ['Confirmed', 'Cancelled'],
  Confirmed: ['Preparing', 'Cancelled'],
  Preparing: ['ReadyForShipping', 'Cancelled'],
  ReadyForShipping: ['Shipped', 'Cancelled'],
  Shipped: ['Delivered', 'Returned'],
  Delivered: ['Returned'],
  Cancelled: [],
  Returned: [],
  PaymentFailed: ['Cancelled'],
}

const fieldMessages = (problem: ApiProblem | null, ...names: string[]) =>
  names.flatMap((name) => problem?.errors[name] ?? [])

export function AdminOrderDetailsPage() {
  const { t, i18n } = useTranslation()
  const { id } = useParams()
  const queryClient = useQueryClient()
  const permissions = useSessionStore((state) => state.admin?.permissions ?? [])
  const canRead = permissions.includes(adminPermissions.ordersRead)
  const canChangeStatus = permissions.includes('orders.status.write')
  const canVerifyPayment = permissions.includes(adminPermissions.paymentsVerify)
  const idIsValid = validId(id)
  const queryKey = useMemo(() => ['admin-order', id, i18n.language] as const, [i18n.language, id])
  const query = useQuery({
    enabled: canRead && idIsValid,
    queryKey,
    queryFn: () => api
      .get<OrderDetails>(`/admin/orders/${id}`)
      .then((response) => response.data),
  })
  const currentStatus = query.data?.status
  const latestPayment = useMemo(
    () => [...(query.data?.payments ?? [])].reverse().find((payment) => payment.id),
    [query.data?.payments],
  )
  const paymentMethod = latestPayment?.method ?? query.data?.paymentMethod
  const paymentAllowsConfirmation = paymentMethod === 'CashOnDelivery' || latestPayment?.status === 'Paid'
  const transitions = useMemo(
    () => currentStatus
      ? allowedTransitions[currentStatus].filter(
          (transition) => transition !== 'Confirmed' || paymentAllowsConfirmation,
        )
      : [],
    [currentStatus, paymentAllowsConfirmation],
  )
  const reviewablePayment = latestPayment
    && latestPayment.status === 'Processing'
    && (latestPayment.method === 'BankTransfer' || latestPayment.method === 'ElectronicWallet')
    ? latestPayment
    : undefined
  const [status, setStatus] = useState<OrderStatus | ''>('')
  const [note, setNote] = useState('')
  const [submissionProblem, setSubmissionProblem] = useState<ApiProblem | null>(null)
  const [paymentReference, setPaymentReference] = useState('')
  const [paymentNote, setPaymentNote] = useState('')
  const [downloadingProof, setDownloadingProof] = useState(false)

  useEffect(() => {
    setStatus(transitions[0] ?? '')
    setNote('')
    setSubmissionProblem(null)
  }, [transitions])

  useEffect(() => {
    setPaymentReference(reviewablePayment?.transactionReference ?? '')
    setPaymentNote('')
  }, [reviewablePayment?.id, reviewablePayment?.transactionReference])

  const mutation = useMutation({
    mutationFn: (request: ChangeOrderStatusRequest) => api
      .post<OrderDetails>(`/admin/orders/${id}/status`, request)
      .then((response) => response.data),
    onSuccess: async (updatedOrder) => {
      queryClient.setQueryData(queryKey, updatedOrder)
      setSubmissionProblem(null)
      toast.success(t('orderDetails.statusUpdated'))
      await queryClient.invalidateQueries({ queryKey: ['admin-resource', 'orders'] })
    },
    onError: (error) => {
      const problem = parseApiProblem(error)
      setSubmissionProblem(problem)
      toast.error(problem.title)
    },
  })
  const paymentMutation = useMutation({
    mutationFn: ({ action, paymentId }: { action: 'verify' | 'reject'; paymentId: string }) => api
      .post<Schema<'PaymentResponse'>>(
        `/admin/payments/${paymentId}/${action}`,
        action === 'verify'
          ? { transactionReference: paymentReference.trim() || null, note: paymentNote.trim() || null }
          : { reason: paymentNote.trim() },
      )
      .then((response) => response.data),
    onSuccess: async (_, variables) => {
      toast.success(t(variables.action === 'verify'
        ? 'orderDetails.paymentVerified'
        : 'orderDetails.paymentRejected'))
      await Promise.all([
        query.refetch(),
        queryClient.invalidateQueries({ queryKey: ['admin-resource', 'orders'] }),
        queryClient.invalidateQueries({ queryKey: ['admin-resource', 'payments'] }),
      ])
    },
    onError: (error) => toast.error(parseApiProblem(error).title),
  })

  const reviewPayment = (action: 'verify' | 'reject') => {
    if (!reviewablePayment?.id || paymentMutation.isPending) return
    if (action === 'reject' && !paymentNote.trim()) {
      toast.error(t('orderDetails.rejectionReasonRequired'))
      return
    }
    paymentMutation.mutate({ action, paymentId: reviewablePayment.id })
  }

  const openProof = async () => {
    if (!reviewablePayment?.proofFilePath || downloadingProof) return
    setDownloadingProof(true)
    try {
      await downloadFile(reviewablePayment.proofFilePath, `payment-proof-${reviewablePayment.id}`)
    } catch (error) {
      toast.error(parseApiProblem(error).title)
    } finally {
      setDownloadingProof(false)
    }
  }

  const submitStatus = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setSubmissionProblem(null)
    if (!status) {
      setSubmissionProblem({
        title: t('errors.badRequest.title'),
        errors: { status: [t('orderDetails.statusRequired')] },
      })
      return
    }
    mutation.mutate({ status, note: note.trim() || null })
  }

  if (!canRead) return <ForbiddenState />
  if (!idIsValid) return <ErrorState status={404} />
  if (query.isLoading) return <LoadingState />
  if (query.isError) return <ErrorState error={query.error} retry={() => void query.refetch()} />
  if (!query.data?.id) return <EmptyState message={t('orderDetails.empty')} />

  const statusErrors = fieldMessages(submissionProblem, 'status', 'Status')
  const noteErrors = fieldMessages(submissionProblem, 'note', 'Note')

  return (
    <div className="grid min-w-0 gap-6">
      {canVerifyPayment && reviewablePayment && (
        <form className="card grid min-w-0 gap-4 border-s-4 border-s-[var(--heritage-gold)] p-4 sm:p-5" onSubmit={(event) => {
          event.preventDefault()
          reviewPayment('verify')
        }}>
          <h2 className="min-w-0 [overflow-wrap:anywhere] font-extrabold">{t('orderDetails.reviewPayment')}</h2>
          <div className="grid min-w-0 gap-4 lg:grid-cols-2">
            <label className="grid min-w-0 gap-2 font-bold">
              <span>{t('customer.fields.transactionReference')}</span>
              <input className="field" dir="ltr" disabled={paymentMutation.isPending} maxLength={250} onChange={(event) => setPaymentReference(event.target.value)} value={paymentReference} />
            </label>
            <label className="grid min-w-0 gap-2 font-bold">
              <span>{t('orderDetails.verificationNote')}</span>
              <textarea className="field min-h-24" disabled={paymentMutation.isPending} maxLength={1000} onChange={(event) => setPaymentNote(event.target.value)} value={paymentNote} />
            </label>
          </div>
          <div className="flex min-w-0 flex-wrap gap-3">
            {reviewablePayment.proofFilePath && <button className="button-secondary min-w-0 flex-1 sm:flex-none" disabled={downloadingProof || paymentMutation.isPending} onClick={() => void openProof()} type="button">
              <Download aria-hidden size={18} />
              {downloadingProof ? t('states.loading') : t('orderDetails.viewProof')}
            </button>}
            <button className="button-primary min-w-0 flex-1 sm:flex-none" disabled={paymentMutation.isPending} type="submit">
              <ShieldCheck aria-hidden size={18} />
              {paymentMutation.isPending ? t('states.saving') : t('orderDetails.verifyPayment')}
            </button>
            <button className="button-secondary min-w-0 flex-1 !border-[var(--heritage-red)] !text-[var(--heritage-red)] sm:flex-none" disabled={paymentMutation.isPending} onClick={() => reviewPayment('reject')} type="button">
              <XCircle aria-hidden size={18} />
              {t('orderDetails.rejectPayment')}
            </button>
          </div>
        </form>
      )}
      {canChangeStatus && (
        <form className="card grid min-w-0 gap-4 p-4 sm:p-5" onSubmit={submitStatus}>
          <h2 className="min-w-0 break-words font-extrabold [overflow-wrap:anywhere]">{t('orderDetails.changeStatus')}</h2>
          {!transitions.length
            ? <p className="muted break-words rounded-xl bg-[var(--heritage-beige)] p-4 [overflow-wrap:anywhere]">{t('orderDetails.noTransitions')}</p>
            : (
              <div className="grid min-w-0 gap-4 xl:grid-cols-[minmax(0,0.7fr)_minmax(0,1.3fr)_auto] xl:items-start">
                <label className="grid min-w-0 gap-2 font-bold">
                  <span className="break-words [overflow-wrap:anywhere]">{t('orderDetails.newStatus')}</span>
                  <select
                    aria-invalid={statusErrors.length > 0}
                    className="field"
                    disabled={mutation.isPending}
                    onChange={(event) => setStatus(event.target.value as OrderStatus)}
                    value={status}
                  >
                    {transitions.map((option) => (
                      <option key={option} value={option}>{t(`orderStatuses.${option}`)}</option>
                    ))}
                  </select>
                  {statusErrors.map((error) => <span className="break-words text-sm text-[var(--heritage-red)] [overflow-wrap:anywhere]" key={error}>{error}</span>)}
                </label>
                <label className="grid min-w-0 gap-2 font-bold">
                  <span className="break-words [overflow-wrap:anywhere]">{t('orderDetails.statusNote')}</span>
                  <textarea
                    aria-invalid={noteErrors.length > 0}
                    className="field min-h-24 resize-y"
                    disabled={mutation.isPending}
                    dir="auto"
                    maxLength={1000}
                    onChange={(event) => setNote(event.target.value)}
                    value={note}
                  />
                  {noteErrors.map((error) => <span className="break-words text-sm text-[var(--heritage-red)] [overflow-wrap:anywhere]" key={error}>{error}</span>)}
                </label>
                <button className="button-primary w-full whitespace-normal xl:mt-8 xl:w-auto" disabled={mutation.isPending} type="submit">
                  {mutation.isPending ? <><InlineLoader /> {t('states.saving')}</> : t('orderDetails.updateStatus')}
                </button>
              </div>
            )}
          {submissionProblem && !statusErrors.length && !noteErrors.length && (
            <p className="alert alert-error font-bold" role="alert">
              {submissionProblem.title}
            </p>
          )}
        </form>
      )}
      <OrderDetailsView backTo="/admin/orders" order={query.data} showInternalNotes />
    </div>
  )
}
