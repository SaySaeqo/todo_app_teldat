<script setup lang="ts">
import { computed, onBeforeMount, reactive } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import axios from '@/axios'
import type { SubscriptionEmail, TodoItem } from '@/types'

const props = defineProps<{
  item?: TodoItem | null
}>()

const route = useRoute()
const router = useRouter()
const id = Number(route.params.id)

const state = reactive({
  item: null as TodoItem | null,
  loading: false,
  saving: false,
  error: '' as string | null,
  hasChanges: false,
})

const form = reactive({
  title: '',
  isComplete: false,
  dueDate: '' as string | null,
  subscriptionEmails: [] as SubscriptionEmail[],
})

const original = reactive({
  title: '',
  isComplete: false,
  dueDate: '' as string | null,
  subscriptionEmails: [] as SubscriptionEmail[],
})

const loadItem = (data: TodoItem) => {
  state.item = data
  form.title = data.title
  form.isComplete = data.isComplete
  form.dueDate = data.dueDate ? data.dueDate.substring(0, 10) : ''
  form.subscriptionEmails = [...(data.subscriptionEmails ?? [])]
  
  original.title = form.title
  original.isComplete = form.isComplete
  original.dueDate = form.dueDate
  original.subscriptionEmails = JSON.parse(JSON.stringify(form.subscriptionEmails))
  
  state.hasChanges = false
}

const fetchItem = async () => {
  state.loading = true
  state.error = null
  try {
    const { data } = await axios.get<TodoItem>(`/todoitems/${id}`)
    loadItem(data)
  } catch (err: any) {
    state.error = err?.response?.data?.message || err?.message || 'Unexpected error'
  } finally {
    state.loading = false
  }
}

onBeforeMount(fetchItem)

const hasUnsavedChanges = computed(() => {
  return (
    form.title !== original.title ||
    form.isComplete !== original.isComplete ||
    form.dueDate !== original.dueDate ||
    JSON.stringify(form.subscriptionEmails) !== JSON.stringify(original.subscriptionEmails)
  )
})

const goBack = () => {
  if (hasUnsavedChanges.value) {
    const ok = window.confirm('You have unsaved changes. Discard them?')
    if (!ok) return
  }
  router.push({ name: 'list' })
}

const addEmail = () => {
  form.subscriptionEmails.push({ email: '' })
}

const removeEmail = (idx: number) => {
  form.subscriptionEmails.splice(idx, 1)
}

const save = async () => {
  if (!state.item) return
  state.saving = true
  state.error = null
  try {
    const payload: TodoItem = {
      id: state.item.id,
      title: form.title.trim() || state.item.title,
      isComplete: form.isComplete,
      createdAt: state.item.createdAt,
      dueDate: form.dueDate ? new Date(form.dueDate).toISOString() : null,
      subscriptionEmails: form.subscriptionEmails.filter(e => e.email.trim()),
    }

    await axios.put(`/todoitems/${state.item.id}`, payload)
    await fetchItem()
  } catch (err: any) {
    state.error = err?.response?.data?.message || err?.message || 'Save failed'
  } finally {
    state.saving = false
  }
}

const confirmDelete = async () => {
  if (!state.item) return
  const ok = window.confirm('Delete this todo item?')
  if (!ok) return

  try {
    await axios.delete(`/todoitems/${state.item.id}`)
    router.push({ name: 'list' })
  } catch (err: any) {
    state.error = err?.response?.data?.message || err?.message || 'Delete failed'
  }
}

const dueDateDisplay = computed(() => state.item?.dueDate?.split('T')[0] ?? 'Not set')
</script>

<template>
  <section class="panel">
    <div class="panel-header">
      <div>
        <h1 class="title">Task details</h1>
        <p class="subtitle">View and edit this todo item</p>
      </div>
      <div class="actions">
        <button class="ghost" type="button" @click="goBack">← Back</button>
        <button class="danger" type="button" @click="confirmDelete">🗑 Delete</button>
      </div>
    </div>

    <div v-if="state.error" class="notice error">{{ state.error }}</div>
    <div v-else-if="state.loading" class="notice muted">Loading...</div>
    <div v-else-if="!state.item" class="notice error">Not found</div>

    <form v-else class="detail" @submit.prevent="save">
      <label class="field">
        <span>Title</span>
        <input v-model="form.title" required />
      </label>

      <label class="field checkbox">
        <input v-model="form.isComplete" type="checkbox" />
        <span>Completed</span>
      </label>

      <label class="field date-input">
        <span>Due date</span>
        <input v-model="form.dueDate" type="date" class="date-field" />
        <small v-if="state.item">Current: {{ dueDateDisplay }}</small>
      </label>

      <div class="emails">
        <div class="emails-header">
          <span>Subscription emails</span>
          <button class="ghost" type="button" @click="addEmail">+ Add email</button>
        </div>
        <div v-if="!form.subscriptionEmails.length" class="notice muted">No subscriptions</div>
        <div v-for="(email, idx) in form.subscriptionEmails" :key="idx" class="email-row">
          <input v-model="email.email" type="email" placeholder="name@example.com" />
          <button class="ghost" type="button" @click="removeEmail(idx)">Remove</button>
        </div>
      </div>

      <div class="form-actions">
        <button class="primary" type="submit" :disabled="state.saving || !hasUnsavedChanges">
          {{ state.saving ? 'Saving...' : 'Save changes' }}
        </button>
        <span v-if="hasUnsavedChanges" class="unsaved-indicator">Unsaved changes</span>
      </div>
    </form>
  </section>
</template>
