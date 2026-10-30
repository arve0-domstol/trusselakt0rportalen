export interface Notification {
  id: number
  message: string
  isRead: boolean
  createdAt: string
  relatedCaseId: number | null
}
