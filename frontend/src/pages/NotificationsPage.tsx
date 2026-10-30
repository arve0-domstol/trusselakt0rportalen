import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { notificationsApi } from '../api/notifications'
import type { Notification } from '../types/notification'

export function NotificationsPage() {
  const [notifications, setNotifications] = useState<Notification[]>([])

  function reload() {
    notificationsApi.list().then(setNotifications)
  }

  useEffect(reload, [])

  async function markRead(id: number) {
    await notificationsApi.markRead(id)
    reload()
  }

  return (
    <div className="notifications-page">
      <h1>Varsler</h1>
      <ul className="notification-list">
        {notifications.map((n) => (
          <li key={n.id} className={n.isRead ? 'notification' : 'notification notification-unread'}>
            <span className="notification-message">{n.message}</span>
            <span className="notification-time">{new Date(n.createdAt).toLocaleString('nb-NO')}</span>
            {n.relatedCaseId && <Link to={`/saker/${n.relatedCaseId}`}>Gå til sak</Link>}
            {!n.isRead && <button onClick={() => markRead(n.id)}>Merk som lest</button>}
          </li>
        ))}
        {notifications.length === 0 && <li>Ingen varsler.</li>}
      </ul>
    </div>
  )
}
