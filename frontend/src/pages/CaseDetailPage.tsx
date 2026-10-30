import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { casesApi } from '../api/cases'
import { ApiError } from '../api/client'
import { CaseComments } from '../components/CaseComments'
import { CaseDocuments } from '../components/CaseDocuments'
import { CaseExportImport } from '../components/CaseExportImport'
import { CaseHistory } from '../components/CaseHistory'
import { CaseWorkflow } from '../components/CaseWorkflow'
import type { CaseDetail } from '../types/case'

const STATUS_LABELS: Record<string, string> = {
  Ny: 'Ny',
  UnderBehandling: 'Under behandling',
  VenterPaaKunde: 'Venter på kunde',
  TilGodkjenning: 'Til godkjenning',
  Godkjent: 'Godkjent',
  Lukket: 'Lukket',
  Avvist: 'Avvist',
}

const PRIORITY_LABELS: Record<string, string> = {
  Lav: 'Lav',
  Normal: 'Normal',
  Hoy: 'Høy',
  Kritisk: 'Kritisk',
}

export function CaseDetailPage() {
  const { id } = useParams()
  const [caseData, setCaseData] = useState<CaseDetail | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [reminderStatus, setReminderStatus] = useState<string | null>(null)
  const [sendingReminder, setSendingReminder] = useState(false)
  const [exportingPdf, setExportingPdf] = useState(false)

  useEffect(() => {
    if (!id) return
    setError(null)
    setCaseData(null)
    casesApi
      .get(Number(id))
      .then(setCaseData)
      .catch((err) => setError(err instanceof ApiError ? err.message : 'Kunne ikke laste saken.'))
  }, [id])

  async function handleSendReminder() {
    if (!id) return
    setSendingReminder(true)
    setReminderStatus(null)
    try {
      const res = await casesApi.sendReminder(Number(id))
      setReminderStatus(res.message)
    } catch (err) {
      setReminderStatus(err instanceof ApiError ? err.message : 'Kunne ikke sende påminnelse.')
    } finally {
      setSendingReminder(false)
    }
  }

  async function handleExportPdf() {
    if (!id) return
    setExportingPdf(true)
    setReminderStatus(null)
    try {
      const blob = await casesApi.exportPdf(Number(id))
      const url = URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = `sak-${id}.pdf`
      link.click()
      URL.revokeObjectURL(url)
    } catch (err) {
      setReminderStatus(err instanceof ApiError ? err.message : 'Kunne ikke eksportere PDF.')
    } finally {
      setExportingPdf(false)
    }
  }

  if (error) return <p className="form-error">{error}</p>
  if (!caseData) return <p>Laster…</p>

  return (
    <div className="case-detail-page">
      <div className="case-detail-header">
        <div>
          <span className="case-number">{caseData.caseNumber}</span>
          <h1>{caseData.title}</h1>
        </div>
        <div className="case-detail-actions">
          <button onClick={handleSendReminder} disabled={sendingReminder}>
            {sendingReminder ? 'Sender…' : 'Send påminnelse'}
          </button>
          <button onClick={handleExportPdf} disabled={exportingPdf}>
            {exportingPdf ? 'Eksporterer…' : 'Eksporter som PDF'}
          </button>
          <Link to={`/saker/${caseData.id}/rediger`} className="button-link">
            Rediger
          </Link>
        </div>
      </div>
      {reminderStatus && <p className="form-message">{reminderStatus}</p>}

      <section className="case-summary-card">
        <dl className="case-detail-fields">
          <dt>Status</dt>
          <dd>
            <span className={`status-badge status-${caseData.status}`}>
              {STATUS_LABELS[caseData.status] ?? caseData.status}
            </span>
          </dd>
          <dt>Prioritet</dt>
          <dd>
            <span className={`priority-badge priority-${caseData.priority}`}>
              {PRIORITY_LABELS[caseData.priority] ?? caseData.priority}
            </span>
          </dd>
          <dt>Kunde</dt>
          <dd>{caseData.customerName}</dd>
          <dt>Avdeling</dt>
          <dd>{caseData.departmentName}</dd>
          <dt>Saksbehandler</dt>
          <dd>{caseData.assignedToName ?? '—'}</dd>
          <dt>Godkjent av</dt>
          <dd>{caseData.approvedByName ?? '—'}</dd>
          <dt>Opprettet</dt>
          <dd>{new Date(caseData.createdAt).toLocaleString('nb-NO')}</dd>
          <dt>Sist oppdatert</dt>
          <dd>{new Date(caseData.updatedAt).toLocaleString('nb-NO')}</dd>
          <dt>Frist</dt>
          <dd>{caseData.deadline ? new Date(caseData.deadline).toLocaleDateString('nb-NO') : '—'}</dd>
        </dl>

        <h2>Beskrivelse</h2>
        <p className="case-description">{caseData.description}</p>
      </section>

      <CaseWorkflow caseData={caseData} onChanged={setCaseData} />
      <CaseDocuments caseId={caseData.id} />
      <CaseComments caseId={caseData.id} />
      <CaseHistory caseId={caseData.id} />
      <CaseExportImport caseId={caseData.id} />
    </div>
  )
}
