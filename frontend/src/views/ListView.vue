<script setup lang="ts">
import { computed, onMounted, reactive, watch } from 'vue'
import { useRouter } from 'vue-router'
import type { PagedResult, TodoItem } from '../types'

const router = useRouter()

const state = reactive({
  items: [] as TodoItem[],
  totalCount: 0,
  page: 1,
  pageSize: 5,
  loading: false,
  error: '' as string | null,
  togglingSubscription: {} as Record<number, boolean>,
})

const filters = reactive({
  search: '',
  dueBefore: '',
  status: 'all' as 'all' | 'open' | 'done',
  sortBy: 'createdDesc' as 'createdDesc' | 'createdAsc' | 'dueDesc' | 'dueAsc' | 'titleAsc' | 'titleDesc',
})

const userEmail = computed(() => localStorage.getItem('userEmail') || '')

const isSubscribed = (item: TodoItem): boolean => {
  if (!userEmail.value) return false
  return item.subscriptionEmails?.some(s => s.email === userEmail.value) ?? false
}

const totalPages = computed(() => Math.max(1, Math.ceil(state.totalCount / state.pageSize)))

const fetchItems = async () => {
  state.loading = true
  state.error = null
  try {
    const params = new URLSearchParams()
    params.set('page', state.page.toString())
    params.set('pageSize', state.pageSize.toString())
    if (filters.search.trim()) params.set('search', filters.search.trim())
    if (filters.dueBefore) params.set('dueBefore', new Date(filters.dueBefore).toISOString())
    if (filters.status === 'open') params.set('isComplete', 'false')
    if (filters.status === 'done') params.set('isComplete', 'true')
    params.set('sortBy', filters.sortBy)

    const res = await fetch(`/api/todoitems?${params.toString()}`)
    if (!res.ok) throw new Error('Failed to load todo items')

    const data = (await res.json()) as PagedResult<TodoItem>
    state.items = data.items
    state.totalCount = data.totalCount
  } catch (err: any) {
    state.error = err?.message ?? 'Unexpected error'
  } finally {
    state.loading = false
  }
}

onMounted(fetchItems)

watch(() => [filters.search, filters.dueBefore, filters.status, filters.sortBy], () => {
  state.page = 1
  fetchItems()
})

watch(() => state.page, () => fetchItems())

const goToDetail = (id: number, event: Event) => {
  const target = event.target as HTMLElement
  if (target.closest('.bell-button')) return
  router.push({ name: 'detail', params: { id } })
}

const toggleSubscription = async (item: TodoItem, event: Event) => {
  event.stopPropagation()
  if (!userEmail.value || state.togglingSubscription[item.id]) return

  state.togglingSubscription[item.id] = true
  const currentlySubscribed = isSubscribed(item)

  try {
    let newEmails = [...(item.subscriptionEmails ?? [])]
    
    if (currentlySubscribed) {
      newEmails = newEmails.filter(s => s.email !== userEmail.value)
    } else {
      newEmails.push({ email: userEmail.value })
    }

    const payload = {
      id: item.id,
      title: item.title,
      isComplete: item.isComplete,
      createdAt: item.createdAt,
      dueDate: item.dueDate,
      subscriptionEmails: newEmails,
    }

    const res = await fetch(`/api/todoitems/${item.id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    })

    if (!res.ok) throw new Error('Failed to update subscription')

    item.subscriptionEmails = newEmails
    
    const message = currentlySubscribed ? 'Unsubscribed successfully' : 'Subscribed successfully'
    showNotification(message)
  } catch (err: any) {
    state.error = err?.message ?? 'Failed to update subscription'
  } finally {
    state.togglingSubscription[item.id] = false
  }
}

const showNotification = (message: string) => {
  const notification = document.createElement('div')
  notification.className = 'notification-toast'
  notification.textContent = message
  document.body.appendChild(notification)
  setTimeout(() => notification.remove(), 3000)
}

const createNew = async () => {
  try {
    const res = await fetch('/api/todoitems', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        title: 'New task',
        isComplete: false,
        dueDate: null,
        subscriptionEmails: [],
      }),
    })
    if (!res.ok) throw new Error('Failed to create item')
    const created = (await res.json()) as TodoItem
    router.push({ name: 'detail', params: { id: created.id } })
  } catch (err: any) {
    state.error = err?.message ?? 'Failed to create item'
  }
}

const nextPage = () => {
  if (state.page < totalPages.value) state.page += 1
}

const prevPage = () => {
  if (state.page > 1) state.page -= 1
}
</script>

<template>
  <section class="panel">
    <div class="panel-header">
      <div>
        <h1 class="title">Your tasks</h1>
        <p class="subtitle">Search, filter, and browse todo items</p>
      </div>
      <button class="primary" type="button" @click="createNew">+ New</button>
    </div>

    <div class="filters">
      <label class="field">
        <span>Search title</span>
        <input v-model="filters.search" type="search" placeholder="Find task" />
      </label>
      <label class="field">
        <span>Due before</span>
        <input v-model="filters.dueBefore" type="date" />
      </label>
      <label class="field">
        <span>Status</span>
        <select v-model="filters.status">
          <option value="all">All</option>
          <option value="open">Open</option>
          <option value="done">Done</option>
        </select>
      </label>
      <label class="field">
        <span>Sort by</span>
        <select v-model="filters.sortBy">
          <option value="createdDesc">Created (newest)</option>
          <option value="createdAsc">Created (oldest)</option>
          <option value="dueDesc">Due date (latest)</option>
          <option value="dueAsc">Due date (earliest)</option>
          <option value="titleAsc">Title (A-Z)</option>
          <option value="titleDesc">Title (Z-A)</option>
        </select>
      </label>
    </div>

    <div v-if="state.error" class="notice error">{{ state.error }}</div>
    <div v-else-if="state.loading" class="notice muted">Loading...</div>

    <ul v-else class="list">
      <li v-for="item in state.items" :key="item.id" class="card" @click="goToDetail(item.id, $event)">
        <div class="card-top">
          <div class="badge" :class="{ complete: item.isComplete }">
            {{ item.isComplete ? 'Done' : 'Open' }}
          </div>
          <button
            class="bell-button"
            :class="{ active: isSubscribed(item), disabled: !userEmail }"
            :disabled="!userEmail || state.togglingSubscription[item.id]"
            @click="toggleSubscription(item, $event)"
            :title="userEmail ? (isSubscribed(item) ? 'Unsubscribe' : 'Subscribe') : 'Login to subscribe'"
          >
            {{ isSubscribed(item) ? '🔔' : '🔕' }}
          </button>
        </div>
        <h3 class="card-title">{{ item.title }}</h3>
        <p class="meta">Created: {{ item.createdAt.split('T')[0] }}</p>
        <p class="meta" v-if="item.dueDate">Due: {{ item.dueDate.split('T')[0] }}</p>
      </li>

      <li v-if="!state.items.length" class="card muted">No items found.</li>
    </ul>

    <div class="pagination">
      <button type="button" @click="prevPage" :disabled="state.page === 1">Prev</button>
      <span>Page {{ state.page }} of {{ totalPages }}</span>
      <button type="button" @click="nextPage" :disabled="state.page >= totalPages">Next</button>
    </div>
  </section>
</template>
