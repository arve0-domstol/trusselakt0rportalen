import { api } from './client'
import type { UserDetail } from '../types/user'

export interface CreateUserPayload {
  username: string
  fullName: string
  email: string
  phone: string
  role: string
  departmentId: number
  password: string
}

export const adminApi = {
  listUsers: () => api.get<UserDetail[]>('/api/admin/users'),
  getUser: (idHash: string) => api.get<UserDetail>(`/api/admin/users/${idHash}`),
  createUser: (payload: CreateUserPayload) => api.post<UserDetail>('/api/admin/users', payload),
  updateRole: (idHash: string, role: string) =>
    api.put<{ message: string }>(`/api/admin/users/${idHash}/role`, { role }),
  updateStatus: (idHash: string, isActive: boolean) =>
    api.put<{ message: string }>(`/api/admin/users/${idHash}/status`, { isActive }),
}
