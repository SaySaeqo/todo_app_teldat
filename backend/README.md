# Backend quickstart

This backend now uses a local SQLite database stored at `backend/todo.db`.

## Prerequisites
- .NET 8 SDK

## Configure
- Connection string lives in `appsettings.Development.json` under `ConnectionStrings:DefaultConnection`.
- Default: `Data Source=todo.db` (relative to the backend folder). Override with `DOTNET_ConnectionStrings__DefaultConnection` if needed.

## Run
```bash
cd backend
dotnet restore
dotnet run
```
On startup the app runs `Database.Migrate()`, so the database file is created and the latest migrations are applied automatically.

## Background Services

### DailyReminderService
Automated email reminder system that runs daily at midnight UTC.

**Behavior:**
- Checks for incomplete tasks with due dates within 3 days (today through day +3)
- Sends reminder emails to all subscribers of qualifying tasks
- Uses `MockEmailService` that logs emails instead of sending (see console output)
- Gracefully handles errors and logs all activity

**Email Logic:**
- Tasks must be incomplete (`IsComplete = false`)
- Tasks must have a due date (`DueDate != null`)
- Due date must be between today and 3 days from now
- Only sends to tasks with subscribers

**Logs Example:**
```
[MOCK EMAIL] Sending reminder to user@example.com: Task 'Review PR' is due on 2025-12-12
```

To implement real email sending, replace `MockEmailService` with an SMTP or cloud email service implementation of `IEmailService`.

## API
Base URL while running locally: `https://localhost:7030` (or the HTTP port shown on startup).
- `GET /api/todoitems` — list todos (newest first)
- `GET /api/todoitems/{id}` — fetch one
- `POST /api/todoitems` — create `{ "title": "My task", "isComplete": false }`
- `PUT /api/todoitems/{id}` — update title/completion
- `DELETE /api/todoitems/{id}` — remove

## Migrations
- Tooling: install once `dotnet tool install -g dotnet-ef --version 8.0.11` (already used for the initial migration).
- Add a migration: `dotnet ef migrations add MeaningfulName`
- Apply migrations manually (optional): `dotnet ef database update`
- Initial migration `InitialCreate` is already included and will be applied on run.
