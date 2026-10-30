import { api } from './client'
import type { Comment } from '../types/comment'

export const commentsApi = {
  list: (caseId: number) => api.get<Comment[]>(`/api/v2/cases/${caseId}/comments`),
  create: (caseId: number, payload: { text: string; isInternal: boolean }) =>
    api.post<Comment>(`/api/v2/cases/${caseId}/comments`, payload),
}
