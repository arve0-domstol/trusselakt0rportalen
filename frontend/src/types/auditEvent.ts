export interface AuditEvent {
  id: number
  action: string
  details: string | null
  userName: string | null
  timestamp: string
}
