# Backend Tests

Comprehensive integration tests for the Todo API endpoints using xUnit and WebApplicationFactory.

## Test Coverage

### TodoItemsController Tests

**GetAll Endpoint (9 tests)**
- Empty list when no items
- Paginated items with default paging
- Filtered by completion status
- Filtered by search term
- Filtered by due date ranges
- Sorted by different criteria (6 variants)
- Custom page size

**GetById Endpoint (3 tests)**
- Returns item when exists
- Returns 404 when not found
- Includes subscription emails

**Create Endpoint (4 tests)**
- Creates new item and returns 201
- Creates item with subscriptions
- Returns location header
- Sets CreatedAt to UTC now

**Update Endpoint (5 tests)**
- Updates existing item
- Returns 404 when item doesn't exist
- Returns 400 when ID mismatch
- Updates subscription emails
- Can clear subscription emails

**Delete Endpoint (3 tests)**
- Deletes item and returns 204
- Returns 404 when not found
- Cascades subscription emails

## Running Tests

```bash
cd backend.Tests
dotnet test
```

For detailed output:
```bash
dotnet test --logger "console;verbosity=detailed"
```

## Test Architecture

- **TestWebApplicationFactory**: Creates isolated in-memory database per test
- **Unique database names**: Each test gets a fresh database (no shared state)
- **Full integration**: Tests entire HTTP request/response cycle
- **In-memory database**: Fast test execution without external dependencies

## Test Isolation

Each test class instance creates its own factory and database using `IDisposable` pattern, ensuring complete isolation between tests.
