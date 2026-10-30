import { useEffect, useState, type FormEvent } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { casesApi } from '../api/cases'
import { ApiError } from '../api/client'
import { referenceApi, type NamedOption, type UserOption } from '../api/reference'
import { CASE_PRIORITY_OPTIONS, CASE_STATUS_OPTIONS } from '../types/case'

export function CaseFormPage({ mode }: { mode: 'create' | 'edit' }) {
  const { id } = useParams()
  const navigate = useNavigate()

  const [customers, setCustomers] = useState<NamedOption[]>([])
  const [departments, setDepartments] = useState<NamedOption[]>([])
  const [users, setUsers] = useState<UserOption[]>([])

  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [customerId, setCustomerId] = useState('')
  const [departmentId, setDepartmentId] = useState('')
  const [priority, setPriority] = useState<string>('Normal')
  const [status, setStatus] = useState<string>('Ny')
  const [assignedToUserId, setAssignedToUserId] = useState('')
  const [deadline, setDeadline] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)
  const [loaded, setLoaded] = useState(mode === 'create')

  useEffect(() => {
    referenceApi.customers().then(setCustomers)
    referenceApi.departments().then(setDepartments)
    referenceApi.users().then(setUsers)
  }, [])

  useEffect(() => {
    if (mode === 'edit' && id) {
      casesApi.get(Number(id)).then((c) => {
        setTitle(c.title)
        setDescription(c.description)
        setDepartmentId(String(c.departmentId))
        setPriority(c.priority)
        setStatus(c.status)
        setAssignedToUserId(c.assignedToUserId ? String(c.assignedToUserId) : '')
        setDeadline(c.deadline ? c.deadline.slice(0, 10) : '')
        setLoaded(true)
      })
    }
  }, [mode, id])

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setError(null)
    setSubmitting(true)
    try {
      if (mode === 'create') {
        const created = await casesApi.create({
          title,
          description,
          customerId: Number(customerId),
          departmentId: departmentId ? Number(departmentId) : undefined,
          priority,
          deadline: deadline || undefined,
        })
        navigate(`/saker/${created.id}`)
      } else if (id) {
        await casesApi.update(Number(id), {
          title,
          description,
          status,
          priority,
          assignedToUserId: assignedToUserId ? Number(assignedToUserId) : null,
          departmentId: Number(departmentId),
          deadline: deadline || null,
        })
        navigate(`/saker/${id}`)
      }
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Lagring feilet.')
    } finally {
      setSubmitting(false)
    }
  }

  if (!loaded) {
    return <p>Laster…</p>
  }

  const usersInDepartment = departmentId
    ? users.filter((u) => u.departmentId === Number(departmentId))
    : users

  return (
    <form className="case-form" onSubmit={handleSubmit}>
      <h1>{mode === 'create' ? 'Ny sak' : 'Rediger sak'}</h1>

      <label>
        Tittel
        <input value={title} onChange={(e) => setTitle(e.target.value)} required />
      </label>

      <label>
        Beskrivelse
        <textarea value={description} onChange={(e) => setDescription(e.target.value)} rows={4} />
      </label>

      {mode === 'create' && (
        <label>
          Kunde
          <select value={customerId} onChange={(e) => setCustomerId(e.target.value)} required>
            <option value="">Velg kunde…</option>
            {customers.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </select>
        </label>
      )}

      <label>
        Avdeling
        <select value={departmentId} onChange={(e) => setDepartmentId(e.target.value)}>
          <option value="">(min avdeling)</option>
          {departments.map((d) => (
            <option key={d.id} value={d.id}>
              {d.name}
            </option>
          ))}
        </select>
      </label>

      <label>
        Prioritet
        <select value={priority} onChange={(e) => setPriority(e.target.value)}>
          {CASE_PRIORITY_OPTIONS.map((p) => (
            <option key={p} value={p}>
              {p}
            </option>
          ))}
        </select>
      </label>

      {mode === 'edit' && (
        <>
          <label>
            Status
            <select value={status} onChange={(e) => setStatus(e.target.value)}>
              {CASE_STATUS_OPTIONS.map((s) => (
                <option key={s} value={s}>
                  {s}
                </option>
              ))}
            </select>
          </label>

          <label>
            Saksbehandler
            <select value={assignedToUserId} onChange={(e) => setAssignedToUserId(e.target.value)}>
              <option value="">(ikke tildelt)</option>
              {usersInDepartment.map((u) => (
                <option key={u.id} value={u.id}>
                  {u.fullName}
                </option>
              ))}
            </select>
          </label>
        </>
      )}

      <label>
        Frist
        <input type="date" value={deadline} onChange={(e) => setDeadline(e.target.value)} />
      </label>

      {error && <p className="form-error">{error}</p>}
      <button type="submit" disabled={submitting}>
        {submitting ? 'Lagrer…' : 'Lagre'}
      </button>
    </form>
  )
}
