export interface UserDetail {
  id: number
  idHash: string
  username: string
  fullName: string
  email: string
  phone: string
  role: string
  departmentId: number
  departmentName: string
  isActive: boolean
  createdAt: string
  lastLoginAt: string | null
}

export const USER_ROLE_OPTIONS = ['Saksbehandler', 'SeniorSaksbehandler', 'Teamleder', 'Administrator'] as const
