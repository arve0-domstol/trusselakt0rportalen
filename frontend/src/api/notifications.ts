import { api } from './client'
import type { Notification } from '../types/notification'

export const notificationsApi = {
  list: () => api.get<Notification[]>('/api/v2/notifications'),
  markRead: (id: number) => api.post<void>(`/api/v2/notifications/${id}/read`),
}
