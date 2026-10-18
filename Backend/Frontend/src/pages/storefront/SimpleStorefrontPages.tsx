import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQuery } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { Link, useParams } from 'react-router-dom'
import { z } from 'zod'
import { toast } from 'sonner'
import DOMPurify from 'dompurify'
import { useTranslation } from 'react-i18next'
import { api } from '@/lib/api/client'
import { parseApiProblem } from '@/lib/api/problemDetails'
import { formatDate, formatMoney } from '@/lib/utils'
import { EmptyState, ErrorState, LoadingState } from '@/components/ui/Feedback'
import { CategoryShowcaseCard } from '@/features/categories/CategoryShowcaseCard'
import { useStoreContact } from '@/features/settings/storeContact'
import { categoryIdPattern, categoryKeys, type BrowsableCategory, useCategoriesQuery } from '@/features/categories/categoryApi'
import { isSessionValid, useSessionStore } from '@/stores/sessionStore'
import type { Schema } from '@/types/models'
import { statusBadgeClass } from '@/lib/statusTone'

export function CategoriesPage() {
  const { t, i18n } = useTranslation()
  const { id } = useParams()
  const language: 'ar' | 'en' = i18n.language.startsWith('en') ? 'en' : 'ar'
  const listQuery = useCategoriesQuery({ enabled: !id })
  const detailQuery = useQuery({
    enabled: Boolean(id),
    queryKey: categoryKeys.detail(id ?? '', language),
    queryFn: async ({ signal }) => {
      const reference = id ?? ''
      const path = categoryIdPattern.test(reference)
        ? `/categories/${reference}`
        : `/categories/by-slug/${encodeURIComponent(reference)}`
      return api.get<Schema<'PublicCategoryDto'>>(path, {
        headers: { 'Accept-Language': language },
        signal,
      }).then((response) => response.data)
    },
  })
  const query = id ? detailQuery : listQuery
  const categories = id
    ? detailQuery.data?.id && detailQuery.data.name
      ? [detailQuery.data as BrowsableCategory]
      : []
    : listQuery.data ?? []

  return <div className="page-shell py-12">
    <h1 className="mb-8 max-w-2xl [overflow-wrap:anywhere] text-3xl font-extrabold sm:text-4xl">{t('categories.title')}</h1>
    {id && categories[0] && <div className="mb-6 flex min-w-0 flex-wrap items-center gap-2 rounded-2xl border border-[rgb(112_33_45_/_0.12)] bg-[var(--heritage-beige)]/70 p-4" role="status">
      <span className="muted text-sm">{t('products.selectedCategory')}:</span>
      <strong className="min-w-0 [overflow-wrap:anywhere] text-[var(--heritage-burgundy)]" dir="auto">{categories[0].name}</strong>
    </div>}
    {query.isLoading
      ? <div aria-busy="true" aria-live="polite" role="status">
          <span className="sr-only">{t('states.loading')}</span>
          <div aria-hidden className="grid min-w-0 grid-cols-2 gap-4 sm:grid-cols-3 sm:gap-5 lg:grid-cols-4">
            {Array.from({ length: id ? 1 : 8 }, (_, index) => <div className="skeleton aspect-[4/5] w-full rounded-2xl" key={index} />)}
          </div>
        </div>
      : query.isError
        ? <ErrorState error={query.error} message={t('categories.loadError')} retry={() => void query.refetch()} />
        : !categories.length
          ? <EmptyState message={t('categories.empty')} />
          : <div className="grid min-w-0 grid-cols-2 gap-4 sm:grid-cols-3 sm:gap-5 lg:grid-cols-4">
            {categories.map((category) => <CategoryShowcaseCard category={category} headingLevel="h2" key={category.id} />)}
          </div>}
  </div>
}

const contactSchema = z.object({
  name: z.string().trim().min(2),
  phoneNumber: z.string().trim().min(8),
  email: z.string().trim().email().or(z.literal('')),
  subject: z.string().trim().min(3),
  message: z.string().trim().min(10),
  website: z.string().max(0).optional(),
})
type ContactValues = z.infer<typeof contactSchema>

function StoreContactDetails() {
  const { t } = useTranslation()
  const { phone, whatsapp, email, address } = useStoreContact()
  const whatsappNumber = whatsapp.replace(/\D/g, '')
  const rows: { key: string; label: string; value: string; href?: string; direction: 'ltr' | 'auto' }[] = []
  if (phone) rows.push({ key: 'phone', label: t('contact.storePhone'), value: phone, href: `tel:${phone.replace(/[^\d+]/g, '')}`, direction: 'ltr' })
  if (whatsapp) rows.push({ key: 'whatsapp', label: t('contact.storeWhatsapp'), value: whatsapp, href: whatsappNumber ? `https://wa.me/${whatsappNumber}` : undefined, direction: 'ltr' })
  if (email) rows.push({ key: 'email', label: t('contact.storeEmail'), value: email, href: `mailto:${email}`, direction: 'ltr' })
  if (address) rows.push({ key: 'address', label: t('contact.storeAddress'), value: address, direction: 'auto' })
  if (!rows.length) return null

  return <dl className="mt-6 grid max-w-md gap-4">
    {rows.map((row) => <div className="min-w-0" key={row.key}>
      <dt className="muted text-sm">{row.label}</dt>
      <dd className="[overflow-wrap:anywhere] font-bold" dir={row.direction}>
        {row.href
          ? <a className="text-[var(--heritage-burgundy)]" href={row.href} rel="noreferrer" target={row.key === 'whatsapp' ? '_blank' : undefined}>{row.value}</a>
          : row.value}
      </dd>
    </div>)}
  </dl>
}

export function ContactPage() {
  const { t, i18n } = useTranslation()
  const isCustomer = isSessionValid(useSessionStore((state) => state.customer))
  const form = useForm<ContactValues>({
    resolver: zodResolver(contactSchema),
    defaultValues: { name: '', phoneNumber: '', email: '', subject: '', message: '', website: '' },
  })
  const mutation = useMutation({
    mutationFn: (data: ContactValues) => api.post('/contact', {
      ...data,
      language: i18n.language.startsWith('ar') ? 'Arabic' : 'English',
    }),
    onSuccess: () => {
      form.reset()
      toast.success(t('contact.sent'))
    },
    onError: (error) => {
      const problem = parseApiProblem(error)
      for (const [field, messages] of Object.entries(problem.errors)) {
        const normalized = field.toLowerCase()
        const formField = (Object.keys(contactSchema.shape) as (keyof ContactValues)[])
          .find((candidate) => normalized.includes(candidate.toLowerCase()))
        if (formField) form.setError(formField, { type: 'server', message: messages[0] })
      }
      toast.error(problem.title)
    },
  })
  const fieldError = (field: keyof ContactValues) => {
    const error = form.formState.errors[field]
    if (!error) return null

    const fallback = field === 'phoneNumber'
      ? 'validation.phone'
      : field === 'email'
        ? 'validation.email'
        : 'validation.required'
    return <small className="field-error">{t(fallback)}</small>
  }

  return <div className="page-shell grid min-w-0 gap-10 py-12 lg:grid-cols-[minmax(0,.8fr)_minmax(0,1.2fr)]">
    <div className="min-w-0">
      <h1 className="[overflow-wrap:anywhere] text-3xl font-extrabold sm:text-4xl">{t('contact.title')}</h1>
      <StoreContactDetails />
      {isCustomer && (
        <Link className="mt-4 block max-w-md font-semibold text-[var(--heritage-burgundy)] [overflow-wrap:anywhere] leading-7" to="/account/messages">
          {t('customer.resources.messages')}
        </Link>
      )}
    </div>
    <form className="card grid min-w-0 gap-4 p-4 sm:p-6" noValidate onSubmit={form.handleSubmit((data) => mutation.mutate(data))}>
      <div className="grid gap-4 sm:grid-cols-2">
        <label>
          {t('contact.name')}
          <input aria-invalid={Boolean(form.formState.errors.name)} autoComplete="name" className="field mt-2" {...form.register('name')} />
          {fieldError('name')}
        </label>
        <label>
          {t('auth.phone')}
          <input aria-invalid={Boolean(form.formState.errors.phoneNumber)} autoComplete="tel" className="field mt-2" dir="ltr" {...form.register('phoneNumber')} />
          {fieldError('phoneNumber')}
        </label>
      </div>
      <label>
        {t('contact.email')}
        <input aria-invalid={Boolean(form.formState.errors.email)} autoComplete="email" className="field mt-2" dir="ltr" type="email" {...form.register('email')} />
        {fieldError('email')}
      </label>
      <label>
        {t('contact.subject')}
        <input aria-invalid={Boolean(form.formState.errors.subject)} className="field mt-2" {...form.register('subject')} />
        {fieldError('subject')}
      </label>
      <label>
        {t('contact.message')}
        <textarea aria-invalid={Boolean(form.formState.errors.message)} className="field mt-2 min-h-36" {...form.register('message')} />
        {fieldError('message')}
      </label>
      <input aria-hidden className="hidden" tabIndex={-1} {...form.register('website')} />
      <button className="button-primary max-w-full whitespace-normal text-center" disabled={mutation.isPending} type="submit">
        {mutation.isPending ? t('states.sending') : t('contact.send')}
      </button>
    </form>
  </div>
}

export function ContentPage() {
  const { t, i18n } = useTranslation()
  const { slug = '' } = useParams()
  const query = useQuery({
    queryKey: ['page', slug, i18n.language],
    queryFn: () => api
      .get<Schema<'ContentPageResponse'>>(`/pages/${encodeURIComponent(slug)}`)
      .then((response) => response.data),
  })
  return <div className="page-shell py-12">
    {query.isLoading
      ? <LoadingState cards={1} />
      : query.isError
        ? <ErrorState error={query.error} retry={() => void query.refetch()} />
        : !query.data
          ? <EmptyState message={t('states.empty')} />
          : <article className="card mx-auto min-w-0 max-w-4xl p-5 sm:p-12">
            <h1 className="[overflow-wrap:anywhere] text-start text-3xl font-extrabold sm:text-4xl" dir="auto">{query.data.title}</h1>
            <div
              className="prose mt-8 min-w-0 max-w-none [overflow-wrap:anywhere] leading-8 [&_a]:break-words [&_code]:[overflow-wrap:anywhere] [&_iframe]:max-w-full [&_img]:h-auto [&_img]:max-w-full [&_pre]:max-w-full [&_pre]:overflow-x-auto [&_table]:block [&_table]:max-w-full [&_table]:overflow-x-auto"
              dangerouslySetInnerHTML={{ __html: DOMPurify.sanitize(query.data.content ?? '') }}
            />
          </article>}
  </div>
}

const lookupSchema = z.object({
  orderNumber: z.string().trim().min(3),
  phoneNumber: z.string().trim().min(8),
})
type LookupValues = z.infer<typeof lookupSchema>

export function OrderLookupPage() {
  const { t, i18n } = useTranslation()
  const form = useForm<LookupValues>({
    resolver: zodResolver(lookupSchema),
    defaultValues: { orderNumber: '', phoneNumber: '' },
  })
  const mutation = useMutation({
    mutationFn: (values: LookupValues) => api
      .get<Schema<'OrderDetailsResponse'>>('/orders/lookup', { params: values })
      .then((response) => response.data),
  })
  const order = mutation.data
  const locale = i18n.language === 'ar' ? 'ar-PS-u-nu-latn' : 'en-US'
  return <div className="page-shell py-12">
    <form className="card mx-auto grid min-w-0 max-w-xl gap-4 p-5 sm:p-7" noValidate onSubmit={form.handleSubmit((values) => mutation.mutate(values))}>
      <h1 className="[overflow-wrap:anywhere] text-3xl font-extrabold">{t('orders.lookupTitle')}</h1>
      <label>
        {t('orders.orderNumber')}
        <input aria-invalid={Boolean(form.formState.errors.orderNumber)} className="field mt-2" dir="ltr" {...form.register('orderNumber')} />
        {form.formState.errors.orderNumber && <small className="field-error">{t('validation.required')}</small>}
      </label>
      <label>
        {t('auth.phone')}
        <input aria-invalid={Boolean(form.formState.errors.phoneNumber)} className="field mt-2" dir="ltr" {...form.register('phoneNumber')} />
        {form.formState.errors.phoneNumber && <small className="field-error">{t('validation.phone')}</small>}
      </label>
      <button className="button-primary max-w-full whitespace-normal text-center" disabled={mutation.isPending} type="submit">
        {mutation.isPending ? t('states.loading') : t('orders.lookupAction')}
      </button>
    </form>
    {mutation.isError && <div className="mx-auto mt-6 max-w-xl">
      <ErrorState error={mutation.error} retry={() => mutation.mutate(form.getValues())} />
    </div>}
    {order && <article className="card mx-auto mt-6 min-w-0 max-w-3xl p-5 sm:p-8">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div className="min-w-0">
          <span className="muted text-sm">{t('orders.orderNumber')}</span>
          <h2 className="mt-1 [overflow-wrap:anywhere] text-2xl font-extrabold" dir="ltr">{order.orderNumber}</h2>
        </div>
        <span className={statusBadgeClass(order.status) + ' max-w-full whitespace-normal text-start'}>{t(`orderStatuses.${order.status}`)}</span>
      </div>
      <dl className="mt-6 grid gap-4 sm:grid-cols-2">
        <div className="min-w-0"><dt className="muted text-sm">{t('orders.date')}</dt><dd className="[overflow-wrap:anywhere] font-bold">{order.createdAtUtc ? formatDate(order.createdAtUtc, locale) : '—'}</dd></div>
        <div className="min-w-0"><dt className="muted text-sm">{t('orders.paymentMethod')}</dt><dd className="[overflow-wrap:anywhere] font-bold">{t(`paymentMethods.${order.paymentMethod}`)}</dd></div>
        <div className="min-w-0"><dt className="muted text-sm">{t('orders.deliveryAddress')}</dt><dd className="[overflow-wrap:anywhere] text-start font-bold" dir="auto">{[order.city, order.area, order.addressLine].filter(Boolean).join('، ')}</dd></div>
        <div className="min-w-0"><dt className="muted text-sm">{t('cart.total')}</dt><dd className="[overflow-wrap:anywhere] font-bold" dir="ltr">{formatMoney(order.total ?? 0, order.currency ?? 'ILS', locale)}</dd></div>
      </dl>
      <h3 className="mt-7 border-t pt-6 text-lg font-bold">{t('orders.items')}</h3>
      <div className="mt-3 grid gap-3">
        {order.items?.map((item, index) => <div className="grid min-w-0 gap-2 rounded-xl bg-stone-50 p-3 sm:grid-cols-[minmax(0,1fr)_auto] sm:gap-4" key={`${item.productVariantId ?? item.productId}-${index}`}>
          <div className="min-w-0"><strong className="block [overflow-wrap:anywhere] text-start" dir="auto">{item.name}</strong><span className="muted mt-1 block [overflow-wrap:anywhere] text-start text-sm" dir="auto">{[item.variant, item.sku].filter(Boolean).join(' · ')}</span></div>
          <span className="[overflow-wrap:anywhere] sm:justify-self-end" dir="ltr">{item.quantity} × {formatMoney(item.unitPrice ?? 0, order.currency ?? 'ILS', locale)}</span>
        </div>)}
      </div>
    </article>}
  </div>
}
