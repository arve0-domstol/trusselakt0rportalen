import { useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { ApiError } from '../api/client'
import { useAuth } from '../auth/AuthContext'

export function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [rememberMe, setRememberMe] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setError(null)
    setSubmitting(true)
    try {
      await login(username, password, rememberMe)
      navigate('/')
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Innlogging feilet.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <main className="auth-page">
      <form onSubmit={handleSubmit} className="auth-form">
        <h1>Trusselaktørportalen</h1>
        <label>
          Brukernavn
          <input value={username} onChange={(e) => setUsername(e.target.value)} autoFocus />
        </label>
        <label>
          Passord
          <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} />
        </label>
        <label className="checkbox-label">
          <input type="checkbox" checked={rememberMe} onChange={(e) => setRememberMe(e.target.checked)} />
          Husk meg
        </label>
        {error && <p className="form-error">{error}</p>}
        <button type="submit" disabled={submitting}>
          {submitting ? 'Logger inn…' : 'Logg inn'}
        </button>
        <p>
          <Link to="/glemt-passord">Glemt passord?</Link>
        </p>
      </form>
    </main>
  )
}
