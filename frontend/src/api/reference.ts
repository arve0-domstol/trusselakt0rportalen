import { api } from './client'

export interface NamedOption {
  id: number
  name: string
}

export interface UserOption {
  id: number
  fullName: string
  departmentId: number
  role: string
}

export const referenceApi = {
  customers: () => api.get<NamedOption[]>('/api/v2/customers'),
  departments: () => api.get<NamedOption[]>('/api/v2/departments'),
  users: (departmentId?: number) =>
    api.get<UserOption[]>(`/api/v2/users${departmentId ? `?departmentId=${departmentId}` : ''}`),
}
