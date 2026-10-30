import { useEffect, useState, type FormEvent } from 'react'
import { usersApi } from '../api/users'
import { ApiError } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import type { UserDetail } from '../types/user'

export function ProfilePage() {
  const { user: authUser } = useAuth()
  const [profile, setProfile] = useState<UserDetail | null>(null)
  const [fullName, setFullName] = useState('')
  const [email, setEmail] = useState('')
  const [phone, setPhone] = useState('')
  const [message, setMessage] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    if (!authUser) return
    usersApi.get(authUser.id).then((p) => {
      setProfile(p)
      setFullName(p.fullName)
      setEmail(p.email)
      setPhone(p.phone)
    })
  }, [authUser])

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    if (!profile) return
    setError(null)
    setMessage(null)
    setSubmitting(true)
    try {
      const updated = await usersApi.update(profile.id, {
        fullName,
        email,
        phone,
        // Role/department/status aren't editable here - carried through
        // unchanged from what was already on the account.
        role: profile.role,
        departmentId: profile.departmentId,
        isActive: profile.isActive,
      })
      setProfile(updated)
      setMessage('Profilen er oppdatert.')
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Oppdatering feilet.')
    } finally {
      setSubmitting(false)
    }
  }

  if (!profile) {
    return <p>Laster…</p>
  }

  return (
    <div className="profile-page">
      <h1>Min profil</h1>
      <form onSubmit={handleSubmit} className="case-form">
        <label>
          Navn
          <input value={fullName} onChange={(e) => setFullName(e.target.value)} required />
        </label>
        <label>
          E-post
          <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
        </label>
        <label>
          Telefon
          <input value={phone} onChange={(e) => setPhone(e.target.value)} />
        </label>
        <dl className="case-detail-fields">
          <dt>Brukernavn</dt>
          <dd>{profile.username}</dd>
          <dt>Rolle</dt>
          <dd>{profile.role}</dd>
          <dt>Avdeling</dt>
          <dd>{profile.departmentName}</dd>
        </dl>
        {message && <p className="form-message">{message}</p>}
        {error && <p className="form-error">{error}</p>}
        <button type="submit" disabled={submitting}>
          {submitting ? 'Lagrer…' : 'Lagre'}
        </button>
      </form>
    </div>
  )
}
