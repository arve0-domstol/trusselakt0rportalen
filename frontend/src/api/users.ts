import { api } from './client'
import type { UserDetail } from '../types/user'

export interface UpdateProfilePayload {
  fullName: string
  email: string
  phone: string
  role: string
  departmentId: number
  isActive: boolean
}

export const usersApi = {
  get: (id: number) => api.get<UserDetail>(`/api/v2/users/${id}`),
  update: (id: number, payload: UpdateProfilePayload) => api.put<UserDetail>(`/api/v2/users/${id}`, payload),
}
