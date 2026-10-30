import { useEffect, useState, type FormEvent } from 'react'
import { adminApi } from '../api/admin'
import { ApiError } from '../api/client'
import { referenceApi, type NamedOption } from '../api/reference'
import { USER_ROLE_OPTIONS, type UserDetail } from '../types/user'

export function AdminUsersPage() {
  const [users, setUsers] = useState<UserDetail[]>([])
  const [departments, setDepartments] = useState<NamedOption[]>([])
  const [error, setError] = useState<string | null>(null)

  const [username, setUsername] = useState('')
  const [fullName, setFullName] = useState('')
  const [email, setEmail] = useState('')
  const [phone, setPhone] = useState('')
  const [role, setRole] = useState<string>('Saksbehandler')
  const [departmentId, setDepartmentId] = useState('')
  const [password, setPassword] = useState('')
  const [creating, setCreating] = useState(false)

  function reload() {
    adminApi.listUsers().then(setUsers)
  }

  useEffect(() => {
    reload()
    referenceApi.departments().then(setDepartments)
  }, [])

  async function handleRoleChange(idHash: string, newRole: string) {
    setError(null)
    try {
      await adminApi.updateRole(idHash, newRole)
      reload()
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Kunne ikke endre rolle.')
    }
  }

  async function handleStatusToggle(idHash: string, isActive: boolean) {
    setError(null)
    try {
      await adminApi.updateStatus(idHash, !isActive)
      reload()
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Kunne ikke endre status.')
    }
  }

  async function handleCreate(e: FormEvent) {
    e.preventDefault()
    setError(null)
    setCreating(true)
    try {
      await adminApi.createUser({
        username,
        fullName,
        email,
        phone,
        role,
        departmentId: Number(departmentId),
        password,
      })
      setUsername('')
      setFullName('')
      setEmail('')
      setPhone('')
      setPassword('')
      reload()
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Kunne ikke opprette bruker.')
    } finally {
      setCreating(false)
    }
  }

  return (
    <div className="admin-users-page">
      <h1>Administrasjon — Brukere</h1>
      {error && <p className="form-error">{error}</p>}

      <table className="case-table">
        <thead>
          <tr>
            <th>Brukernavn</th>
            <th>Navn</th>
            <th>E-post</th>
            <th>Avdeling</th>
            <th>Rolle</th>
            <th>Status</th>
            <th>Sist innlogget</th>
          </tr>
        </thead>
        <tbody>
          {users.map((u) => (
            <tr key={u.id}>
              <td>{u.username}</td>
              <td>{u.fullName}</td>
              <td>{u.email}</td>
              <td>{u.departmentName}</td>
              <td>
                <select value={u.role} onChange={(e) => handleRoleChange(u.idHash, e.target.value)}>
                  {USER_ROLE_OPTIONS.map((r) => (
                    <option key={r} value={r}>
                      {r}
                    </option>
                  ))}
                </select>
              </td>
              <td>
                <button onClick={() => handleStatusToggle(u.idHash, u.isActive)}>
                  {u.isActive ? 'Aktiv' : 'Deaktivert'}
                </button>
              </td>
              <td>{u.lastLoginAt ? new Date(u.lastLoginAt).toLocaleString('nb-NO') : '—'}</td>
            </tr>
          ))}
        </tbody>
      </table>

      <h2>Ny bruker</h2>
      <form onSubmit={handleCreate} className="case-form">
        <label>
          Brukernavn
          <input value={username} onChange={(e) => setUsername(e.target.value)} required />
        </label>
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
        <label>
          Avdeling
          <select value={departmentId} onChange={(e) => setDepartmentId(e.target.value)} required>
            <option value="">Velg avdeling…</option>
            {departments.map((d) => (
              <option key={d.id} value={d.id}>
                {d.name}
              </option>
            ))}
          </select>
        </label>
        <label>
          Rolle
          <select value={role} onChange={(e) => setRole(e.target.value)}>
            {USER_ROLE_OPTIONS.map((r) => (
              <option key={r} value={r}>
                {r}
              </option>
            ))}
          </select>
        </label>
        <label>
          Passord
          <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
        </label>
        <button type="submit" disabled={creating}>
          {creating ? 'Oppretter…' : 'Opprett bruker'}
        </button>
      </form>
    </div>
  )
}
