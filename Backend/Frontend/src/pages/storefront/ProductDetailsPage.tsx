import { useState } from 'react'
import { useParams } from 'react-router-dom'
import { Check, Minus, Plus, ShoppingBag } from 'lucide-react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { useTranslation } from 'react-i18next'
import { ImagePlaceholder } from '@/components/common/ImagePlaceholder'
import { api } from '@/lib/api/client'
import { parseApiProblem } from '@/lib/api/problemDetails'
import { isGenericPlaceholderUrl, resolveMediaUrl } from '@/lib/media'
import { formatMoney } from '@/lib/utils'
import type { Cart, ProductDetails } from '@/types/models'
import { ErrorState, LoadingState } from '@/components/ui/Feedback'

const guidPattern = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i

export function ProductDetailsPage() {
  const { id = '' } = useParams()
  const { t, i18n } = useTranslation()
  const [variantId, setVariantId] = useState('')
  const [imageId, setImageId] = useState('')
  const [quantity, setQuantity] = useState(1)
  const client = useQueryClient()
  const query = useQuery({
    queryKey: ['product', id, i18n.language],
    queryFn: () => api
      .get<ProductDetails>(`/products/${guidPattern.test(id) ? id : `by-slug/${encodeURIComponent(id)}`}`)
      .then((response) => response.data),
  })
  const selectedVariant = query.data?.variants?.find((variant) => variant.id === variantId)
    ?? query.data?.variants?.find((variant) => (variant.availableQuantity ?? 0) > 0)
    ?? query.data?.variants?.[0]
  // The generic development placeholder means "no photo yet", so it is never offered as one.
  const images = (query.data?.images ?? []).filter((image) => !isGenericPlaceholderUrl(image.imageUrl))
  const selectedImage = images.find((image) => image.id === imageId) ?? images[0]
  const add = useMutation({
    mutationFn: () => api
      .post<Cart>('/cart/items', { productVariantId: selectedVariant?.id, quantity })
      .then((response) => response.data),
    onSuccess: () => {
      void client.invalidateQueries({ queryKey: ['cart'] })
      toast.success(t('products.addedToCart'))
    },
    onError: (error) => {
      const problem = parseApiProblem(error)
      const description = problem.status === 409
        ? t('products.addErrorStock')
        : problem.status === 404
          ? t('products.addErrorUnavailable')
          : problem.status === 400
            ? t('products.addErrorQuantity')
            : t('products.addError')
      toast.error(problem.title, { description })
    },
  })
  if (query.isLoading) return <div className="page-shell py-12"><LoadingState cards={2} /></div>
  if (query.isError || !query.data) {
    return <div className="page-shell py-12">
      <ErrorState error={query.error} retry={() => void query.refetch()} />
    </div>
  }
  const product = query.data
  const availableQuantity = selectedVariant?.availableQuantity ?? 0
  return <div className="page-shell grid min-w-0 gap-10 py-12 lg:grid-cols-2">
    <div className={`grid min-w-0 gap-3 ${images.length > 1 ? 'sm:grid-cols-[90px_minmax(0,1fr)]' : ''}`}>
      {images.length > 1 && <div className="order-2 flex gap-2 overflow-x-auto sm:order-1 sm:grid sm:self-start sm:overflow-visible">
        {images.map((image) => <button
          aria-label={t('products.selectImage', { alt: image.altText ?? product.name })}
          className={`aspect-square w-20 shrink-0 overflow-hidden rounded-xl border-2 ${selectedImage?.id === image.id ? 'border-[var(--heritage-burgundy)]' : 'border-transparent'}`}
          key={image.id}
          onClick={() => setImageId(image.id ?? '')}
          type="button"
        >
          <img alt="" className="size-full object-cover" src={resolveMediaUrl(image.thumbnailUrl ?? image.imageUrl)} />
        </button>)}
      </div>}
      <div className="order-1 aspect-[4/5] overflow-hidden rounded-2xl bg-[var(--heritage-beige)] sm:order-2">
        {selectedImage?.imageUrl
          ? <img className="size-full object-cover" alt={selectedImage.altText ?? product.name ?? ''} src={resolveMediaUrl(selectedImage.imageUrl)} />
          : <ImagePlaceholder />}
      </div>
    </div>
    <div className="min-w-0 lg:py-8">
      <span className="badge max-w-full whitespace-normal text-start" dir="auto">{product.category?.name}</span>
      <h1 className="mt-4 [overflow-wrap:anywhere] text-start text-4xl font-extrabold text-[var(--heritage-burgundy)]" dir="auto">{product.name}</h1>
      <p className="muted mt-4 [overflow-wrap:anywhere] text-start leading-8" dir="auto">{product.shortDescription}</p>
      <p className="mt-6 [overflow-wrap:anywhere] text-3xl font-extrabold text-[var(--heritage-burgundy)]" dir="ltr">{formatMoney(selectedVariant?.effectivePrice ?? product.basePrice ?? 0, product.currency ?? 'ILS', i18n.language === 'ar' ? 'ar-PS-u-nu-latn' : 'en-US')}</p>
      {!!product.variants?.length && <div className="mt-8">
        <h2 className="mb-3 font-bold">{t('products.variants')}</h2>
        <div className="flex flex-wrap gap-2">
          {product.variants.map((variant) => <button
            className={`button-secondary max-w-full whitespace-normal [overflow-wrap:anywhere] text-start ${selectedVariant?.id === variant.id ? '!border-[var(--heritage-burgundy)] !bg-[var(--heritage-beige)]' : ''}`}
            dir="auto"
            key={variant.id}
            onClick={() => {
              setVariantId(variant.id ?? '')
              setQuantity(1)
            }}
            type="button"
          >
            {variant.size || variant.color || variant.sku}
            {selectedVariant?.id === variant.id && <Check aria-hidden size={15} />}
          </button>)}
        </div>
      </div>}
      <p className={`mt-4 [overflow-wrap:anywhere] text-sm font-semibold ${availableQuantity > 0 ? 'text-[var(--state-success-text)]' : 'text-[var(--state-danger-text)]'}`}>
        {availableQuantity > 0
          ? t('products.stockAvailable', { count: availableQuantity })
          : t('products.outOfStock')}
      </p>
      <div className="mt-8 flex flex-wrap gap-3">
        <div className="flex shrink-0 items-center rounded-xl border bg-white">
          <button aria-label={t('products.decreaseQuantity')} className="p-3" disabled={quantity <= 1 || add.isPending} onClick={() => setQuantity(Math.max(1, quantity - 1))} type="button"><Minus size={17} /></button>
          <span aria-live="polite" className="min-w-10 text-center font-bold">{quantity}</span>
          <button aria-label={t('products.increaseQuantity')} className="p-3" disabled={quantity >= availableQuantity || add.isPending} onClick={() => setQuantity(Math.min(availableQuantity, quantity + 1))} type="button"><Plus size={17} /></button>
        </div>
        <button className="button-primary min-w-0 basis-48 flex-1 whitespace-normal text-center" disabled={availableQuantity <= 0 || add.isPending} onClick={() => add.mutate()} type="button">
          <ShoppingBag className="shrink-0" size={19} />{add.isPending ? t('states.adding') : t('actions.add')}
        </button>
      </div>
      <div className="mt-10 border-t pt-8">
        <h2 className="text-xl font-bold">{t('products.description')}</h2>
        <p className="muted mt-3 whitespace-pre-wrap [overflow-wrap:anywhere] text-start leading-8" dir="auto">{product.description}</p>
        {product.story && <>
          <h2 className="mt-8 text-xl font-bold">{t('products.story')}</h2>
          <p className="muted mt-3 whitespace-pre-wrap [overflow-wrap:anywhere] text-start leading-8" dir="auto">{product.story}</p>
        </>}
      </div>
    </div>
  </div>
}
