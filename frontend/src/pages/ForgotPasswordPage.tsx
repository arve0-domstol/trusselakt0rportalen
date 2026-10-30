import { useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { api, ApiError } from '../api/client'

export function ForgotPasswordPage() {
  const [step, setStep] = useState<'request' | 'confirm'>('request')
  const [usernameOrEmail, setUsernameOrEmail] = useState('')
  const [token, setToken] = useState('')
  const [newPassword, setNewPassword] = useState('')
  const [message, setMessage] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)

  async function handleRequest(e: FormEvent) {
    e.preventDefault()
    setError(null)
    const res = await api.post<{ message: string }>('/api/auth/reset-password/request', {
      usernameOrEmail,
    })
    setMessage(res.message)
    setStep('confirm')
  }

  async function handleConfirm(e: FormEvent) {
    e.preventDefault()
    setError(null)
    setMessage(null)
    try {
      const res = await api.post<{ message: string }>('/api/auth/reset-password/confirm', {
        token,
        newPassword,
      })
      setMessage(res.message)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Noe gikk galt.')
    }
  }

  return (
    <main className="auth-page">
      <div className="auth-form">
        <h1>Glemt passord</h1>
        {step === 'request' && (
          <form onSubmit={handleRequest}>
            <label>
              Brukernavn eller e-post
              <input value={usernameOrEmail} onChange={(e) => setUsernameOrEmail(e.target.value)} autoFocus />
            </label>
            <button type="submit">Send</button>
          </form>
        )}
        {step === 'confirm' && (
          <form onSubmit={handleConfirm}>
            <label>
              Engangskode
              <input value={token} onChange={(e) => setToken(e.target.value)} autoFocus />
            </label>
            <label>
              Nytt passord
              <input type="password" value={newPassword} onChange={(e) => setNewPassword(e.target.value)} />
            </label>
            <button type="submit">Bytt passord</button>
          </form>
        )}
        {message && <p className="form-message">{message}</p>}
        {error && <p className="form-error">{error}</p>}
        <p>
          <Link to="/login">Tilbake til innlogging</Link>
        </p>
      </div>
    </main>
  )
}
