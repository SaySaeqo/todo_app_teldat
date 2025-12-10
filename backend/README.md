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

## API
Base URL while running locally: `https://localhost:5001` (or the HTTP port shown on startup).
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
