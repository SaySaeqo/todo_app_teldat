# Frontend - Todo App

Vue 3 + TypeScript + Vite SPA for task management with real-time filtering, pagination, and email subscriptions.

## Features

- **Task List View**: Browse, search, filter, and sort tasks with pagination
- **Task Detail View**: View and edit tasks with full CRUD operations
- **Email Subscriptions**: Subscribe/unsubscribe to task reminders with bell icon
- **Filtering Options**: Status (all/open/done), due date, search by title
- **Sorting**: By creation date, due date, or title (ascending/descending)

## Tech Stack

- Vue 3 with Composition API (`<script setup>`)
- TypeScript with strict type checking
- Vue Router for navigation
- Axios for HTTP requests with global `/api` baseURL
- Vite for fast development and optimized builds

## Setup & Run

Install dependencies:
```bash
npm install
```

Start development server:
```bash
npm run dev
```

Build for production:
```bash
npm run build
```

Preview production build:
```bash
npm preview
```