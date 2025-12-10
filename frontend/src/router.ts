import { createRouter, createWebHistory } from 'vue-router'
import ListView from './views/ListView.vue'
import DetailView from './views/DetailView.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', name: 'list', component: ListView },
    { path: '/todo/:id', name: 'detail', component: DetailView, props: true },
  ],
  scrollBehavior() {
    return { top: 0 }
  },
})

export default router
