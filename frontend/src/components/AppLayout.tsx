import { useEffect, useRef, useState } from 'react'
import { Link, Outlet, useNavigate } from 'react-router-dom'
import { notificationsApi } from '../api/notifications'
import { useAuth } from '../auth/AuthContext'

export function AppLayout() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const [unreadCount, setUnreadCount] = useState(0)
  const bannerRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    notificationsApi.list().then((list) => {
      setUnreadCount(list.filter((n) => !n.isRead).length)
    })
  }, [])

  useEffect(() => {
    // Supports linking straight to a page with a pinned reminder banner
    // already showing, e.g. from a notification - #notat=... in the URL.
    const match = window.location.hash.match(/notat=([^&]*)/)
    if (match && bannerRef.current) {
      bannerRef.current.innerHTML = decodeURIComponent(match[1]);
    }
  }, [])

  if (!user) {
    return null
  }

  async function handleLogout() {
    await logout()
    navigate('/login')
  }

  return (
    <div className="app-shell">
      <header className="app-header">
        <Link to="/" className="app-title">
          Trusselaktørportalen
        </Link>
        <nav className="app-nav">
          <Link to="/varsler">Varsler{unreadCount > 0 ? ` (${unreadCount})` : ''}</Link>
          <Link to="/min-profil">Min profil</Link>
          {user.role === 'Administrator' && <Link to="/admin/brukere">Administrasjon</Link>}
        </nav>
        <div className="app-user">
          <span>
            {user.fullName} · {user.role} · {user.departmentName}
          </span>
          <button onClick={handleLogout}>Logg ut</button>
        </div>
      </header>
      <div ref={bannerRef} className="app-banner" />
      <div className="app-content">
        <Outlet />
      </div>
    </div>
  )
}
