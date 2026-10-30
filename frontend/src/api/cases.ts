import { api, API_BASE_URL, ApiError } from './client'
import type { CaseDetail, CaseSummary, PagedResult } from '../types/case'

export interface CaseListParams {
  search?: string
  status?: string
  priority?: string
  departmentId?: number
  mine?: boolean
  sort?: string
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

function buildQuery(params: CaseListParams): string {
  const usp = new URLSearchParams()
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') {
      usp.set(key, String(value))
    }
  })
  const qs = usp.toString()
  return qs ? `?${qs}` : ''
}

export interface CreateCasePayload {
  title: string
  description: string
  customerId: number
  departmentId?: number
  priority: string
  deadline?: string | null
}

export interface UpdateCasePayload {
  title: string
  description: string
  status: string
  priority: string
  assignedToUserId: number | null
  departmentId: number
  deadline?: string | null
}

export const casesApi = {
  list: (params: CaseListParams = {}) =>
    api.get<PagedResult<CaseSummary>>(`/api/v2/cases${buildQuery(params)}`),
  get: (id: number) => api.get<CaseDetail>(`/api/v2/cases/${id}`),
  create: (payload: CreateCasePayload) => api.post<CaseDetail>('/api/v2/cases', payload),
  update: (id: number, payload: UpdateCasePayload) =>
    api.put<CaseDetail>(`/api/v2/cases/${id}`, payload),
  sendReminder: (id: number) => api.post<{ message: string }>(`/api/v2/cases/${id}/send-reminder`),
  assign: (id: number, assignedToUserId: number) =>
    api.post<CaseDetail>(`/api/v2/cases/${id}/assign`, { assignedToUserId }),
  approve: (id: number) => api.get<CaseDetail>(`/api/v2/cases/${id}/approve`),
  close: (id: number, decisionText?: string) =>
    api.post<CaseDetail>(`/api/v2/cases/${id}/close`, { decisionText }),
  exportHistory: (id: number) => api.get<unknown>(`/api/v2/cases/${id}/export`),
  exportPdf: async (id: number): Promise<Blob> => {
    const res = await fetch(`${API_BASE_URL}/api/v2/cases/${id}/export/pdf`, {
      method: 'GET',
      credentials: 'include',
    })
    if (!res.ok) {
      throw new ApiError(res.status, `Kunne ikke eksportere PDF (${res.status})`)
    }
    return res.blob()
  },
  importHistory: (id: number, json: string) =>
    api.post<{ message: string; count: number }>(`/api/v2/cases/${id}/import`, { json }),
}
