import { useEffect, useState } from 'react'
import { historyApi } from '../api/history'
import type { AuditEvent } from '../types/auditEvent'

export function CaseHistory({ caseId }: { caseId: number }) {
  const [events, setEvents] = useState<AuditEvent[]>([])

  useEffect(() => {
    historyApi.list(caseId).then(setEvents)
  }, [caseId])

  return (
    <section className="case-history">
      <h2>Historikk</h2>
      <ul className="history-list">
        {events.map((e) => (
          <li key={e.id}>
            <span className="history-time">{new Date(e.timestamp).toLocaleString('nb-NO')}</span>
            <span className="history-action">{e.action}</span>
            {e.userName && <span className="history-user">{e.userName}</span>}
            {e.details && <span className="history-details">{e.details}</span>}
          </li>
        ))}
        {events.length === 0 && <li>Ingen hendelser registrert.</li>}
      </ul>
    </section>
  )
}
