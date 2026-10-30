export interface CaseSummary {
  id: number
  caseNumber: string
  title: string
  status: string
  priority: string
  customerName: string
  departmentName: string
  assignedToName: string | null
  createdAt: string
  deadline: string | null
}

export interface CaseDetail {
  id: number
  caseNumber: string
  title: string
  description: string
  status: string
  priority: string
  customerId: number
  customerName: string
  departmentId: number
  departmentName: string
  assignedToUserId: number | null
  assignedToName: string | null
  approvedByUserId: number | null
  approvedByName: string | null
  createdAt: string
  updatedAt: string
  deadline: string | null
  closedAt: string | null
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}

export const CASE_STATUS_OPTIONS = [
  'Ny',
  'UnderBehandling',
  'VenterPaaKunde',
  'TilGodkjenning',
  'Godkjent',
  'Lukket',
  'Avvist',
] as const

export const CASE_PRIORITY_OPTIONS = ['Lav', 'Normal', 'Hoy', 'Kritisk'] as const
