import { api } from './client'
import type { AuditEvent } from '../types/auditEvent'

export const historyApi = {
  list: (caseId: number) => api.get<AuditEvent[]>(`/api/v2/cases/${caseId}/history`),
}
