import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'

export function NotFoundPage() {
  const { t } = useTranslation()

  return <main className="page-shell grid min-h-[70vh] min-w-0 place-items-center py-16 text-center">
    <div className="min-w-0 max-w-xl">
      <p className="text-6xl font-extrabold text-[var(--heritage-sand)] sm:text-8xl" dir="ltr">404</p>
      <h1 className="mt-3 [overflow-wrap:anywhere] text-3xl font-extrabold">{t('errors.notFound.title')}</h1>
      <Link className="button-primary mt-6 max-w-full whitespace-normal text-center no-underline" to="/">{t('nav.home')}</Link>
    </div>
  </main>
}
