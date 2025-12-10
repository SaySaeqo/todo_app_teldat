<script setup lang="ts">
import { onMounted, reactive } from 'vue'
import { RouterView } from 'vue-router'

const auth = reactive({
  email: '',
  loginInput: '',
})

const loadAuth = () => {
  const stored = localStorage.getItem('userEmail')
  if (stored) {
    auth.email = stored
  }
}

const handleLogin = () => {
  const trimmed = auth.loginInput.trim()
  if (!trimmed || !trimmed.includes('@')) {
    alert('Please enter a valid email address')
    return
  }
  auth.email = trimmed
  localStorage.setItem('userEmail', trimmed)
  auth.loginInput = ''
}

const handleLogout = () => {
  auth.email = ''
  auth.loginInput = ''
  localStorage.removeItem('userEmail')
}

onMounted(loadAuth)
</script>

<template>
  <div class="app-shell">
    <header class="app-header">
      <router-link to="/" class="brand">TODO APP - TelDat Entry Excercise</router-link>
      <div class="auth">
        <div v-if="!auth.email" class="login-form">
          <input
            v-model="auth.loginInput"
            type="email"
            placeholder="Enter your email"
            @keyup.enter="handleLogin"
            class="login-input"
          />
          <button class="ghost" type="button" @click="handleLogin">Login</button>
        </div>
        <div v-else class="logged-in">
          <span class="logged-text">Logged as <strong>{{ auth.email }}</strong></span>
          <button class="ghost" type="button" @click="handleLogout">Logout</button>
        </div>
      </div>
    </header>

    <main class="app-main">
      <RouterView />
    </main>

    <footer class="app-footer">
        <span>Author: Tomasz Piwowski</span>
        <a href="https://saysaeqo-home-website-94ac6.ondigitalocean.app/#/" target="_blank" rel="noreferrer">Author's homepage</a>
    </footer>
  </div>
</template>
