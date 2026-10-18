import { Bell, FileText } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { useSearchParams } from 'react-router-dom'
import { AdminResourcePage } from '@/pages/admin/AdminResourcePage'

type NotificationTab = 'inbox' | 'templates'

export function AdminNotificationsPage() {
  const { t } = useTranslation()
  const [searchParams, setSearchParams] = useSearchParams()
  const tab: NotificationTab = searchParams.get('tab') === 'templates' ? 'templates' : 'inbox'

  const selectTab = (next: NotificationTab) => {
    const updated = new URLSearchParams(searchParams)
    if (next === 'inbox') updated.delete('tab')
    else updated.set('tab', next)
    setSearchParams(updated, { replace: true })
  }

  return (
    <section className="min-w-0">
      <div className="card mb-6 flex min-w-0 flex-wrap gap-2 p-2" role="tablist" aria-label={t('admin.notificationTabs.label')}>
        <button
          aria-selected={tab === 'inbox'}
          className={`${tab === 'inbox' ? 'button-primary' : 'button-secondary'} min-w-0 flex-1 whitespace-normal sm:flex-none`}
          onClick={() => selectTab('inbox')}
          role="tab"
          type="button"
        >
          <Bell aria-hidden className="shrink-0" size={17} />
          {t('admin.notificationTabs.inbox')}
        </button>
        <button
          aria-selected={tab === 'templates'}
          className={`${tab === 'templates' ? 'button-primary' : 'button-secondary'} min-w-0 flex-1 whitespace-normal sm:flex-none`}
          onClick={() => selectTab('templates')}
          role="tab"
          type="button"
        >
          <FileText aria-hidden className="shrink-0" size={17} />
          {t('admin.notificationTabs.templates')}
        </button>
      </div>
      <AdminResourcePage resource={tab === 'templates' ? 'templates' : 'notifications'} />
    </section>
  )
}
