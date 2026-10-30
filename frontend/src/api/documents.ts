import { api, ApiError } from './client'
import type { CaseDocument } from '../types/document'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5080'

export const documentsApi = {
  list: (caseId: number) => api.get<CaseDocument[]>(`/api/v2/cases/${caseId}/documents`),

  importFromUrl: (caseId: number, url: string) =>
    api.post<CaseDocument>(`/api/v2/cases/${caseId}/documents/import-url`, { url }),

  downloadUrl: (id: number) => `${API_BASE_URL}/api/documents/${id}`,
  previewUrl: (id: number) => `${API_BASE_URL}/api/documents/${id}/preview`,
}
