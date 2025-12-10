export interface SubscriptionEmail {
  id?: number
  email: string
}

export interface TodoItem {
  id: number
  title: string
  isComplete: boolean
  createdAt: string
  dueDate?: string | null
  subscriptionEmails?: SubscriptionEmail[]
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}
