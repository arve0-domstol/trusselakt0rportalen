import { useEffect, useState, type FormEvent } from 'react'
import { casesApi } from '../api/cases'
import { ApiError } from '../api/client'
import { referenceApi, type UserOption } from '../api/reference'
import type { CaseDetail } from '../types/case'

interface CaseWorkflowProps {
  caseData: CaseDetail
  onChanged: (updated: CaseDetail) => void
}

export function CaseWorkflow({ caseData, onChanged }: CaseWorkflowProps) {
  const [users, setUsers] = useState<UserOption[]>([])
  const [assignToUserId, setAssignToUserId] = useState('')
  const [decisionText, setDecisionText] = useState('')
  const [message, setMessage] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)

  useEffect(() => {
    referenceApi.users(caseData.departmentId).then(setUsers)
  }, [caseData.departmentId])

  async function handleAssign(e: FormEvent) {
    e.preventDefault()
    if (!assignToUserId) return
    setError(null)
    setMessage(null)
    setBusy(true)
    try {
      const updated = await casesApi.assign(caseData.id, Number(assignToUserId))
      onChanged(updated)
      setMessage('Sak tildelt.')
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Tildeling feilet.')
    } finally {
      setBusy(false)
    }
  }

  async function handleApprove() {
    setError(null)
    setMessage(null)
    setBusy(true)
    try {
      const updated = await casesApi.approve(caseData.id)
      onChanged(updated)
      setMessage('Sak godkjent.')
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Godkjenning feilet.')
    } finally {
      setBusy(false)
    }
  }

  async function handleClose(e: FormEvent) {
    e.preventDefault()
    setError(null)
    setMessage(null)
    setBusy(true)
    try {
      const updated = await casesApi.close(caseData.id, decisionText || undefined)
      onChanged(updated)
      setMessage('Sak lukket.')
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Lukking feilet.')
    } finally {
      setBusy(false)
    }
  }

  return (
    <section className="case-workflow">
      <h2>Saksbehandling</h2>

      <form onSubmit={handleAssign} className="case-workflow-row">
        <select value={assignToUserId} onChange={(e) => setAssignToUserId(e.target.value)}>
          <option value="">Velg saksbehandler…</option>
          {users.map((u) => (
            <option key={u.id} value={u.id}>
              {u.fullName}
            </option>
          ))}
        </select>
        <button type="submit" disabled={busy || !assignToUserId}>
          Tildel
        </button>
      </form>

      <div className="case-workflow-row">
        <button type="button" onClick={handleApprove} disabled={busy}>
          Godkjenn
        </button>
      </div>

      <form onSubmit={handleClose} className="case-workflow-row">
        <input
          type="text"
          placeholder="Vedtakstekst (valgfritt)"
          value={decisionText}
          onChange={(e) => setDecisionText(e.target.value)}
        />
        <button type="submit" disabled={busy}>
          Lukk sak
        </button>
      </form>

      {message && <p className="form-message">{message}</p>}
      {error && <p className="form-error">{error}</p>}
    </section>
  )
}
