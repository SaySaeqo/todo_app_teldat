# Todo App Teldat

Full-stack todo application with automated email reminders.

## Modules

### Backend
ASP.NET Core 8 API with SQLite database, pagination, filtering, and daily reminder service that sends mock emails for tasks due within 3 days.

**Run:** `cd backend && dotnet run`

### Backend Tests
Comprehensive xUnit integration tests (35 tests) covering all API endpoints and the reminder service.

**Run:** `cd backend.Tests && dotnet test`

### Frontend
Vue 3 + TypeScript + Vite SPA with task management, filtering, and email subscription features.

**Run:** `cd frontend && npm install && npm run dev`

## Quick Start

1. **Start backend:** `cd backend && dotnet run`
2. **Start frontend:** `cd frontend && npm run dev`
3. **Run tests:** `cd backend.Tests && dotnet test`

Backend runs on `https://localhost:7030`, frontend on `http://localhost:5173`.
