# PlayBuilder Coding Standards

## C#

- Nullable reference types remain enabled.
- Use file-scoped namespaces.
- Use explicit, descriptive names.
- Prefer immutable records for value-like data and sealed classes for services unless inheritance is required.
- Validate public method arguments.
- Do not add `TODO`, `NotImplementedException`, fake data, or empty placeholder methods.
- Prefer collection expressions where they improve readability.

## Dependency injection

- Constructor-inject collaborators.
- Avoid service-locator patterns.
- Register interfaces where an abstraction is useful.
- Keep long-lived services safe for their registered lifetime.

## Async

- Use asynchronous APIs for I/O and database work.
- Pass cancellation tokens through long-running operations.
- Do not use `async void` except framework event handlers that require it.

## Entity Framework Core

- Use `IDbContextFactory<PlayBuilderDbContext>` from long-lived Blazor services.
- Keep DbContext instances short-lived.
- Avoid tracking for read-only queries when practical.

## Testing

- Parsing and rule behavior require automated tests.
- Tests must be deterministic and must not depend on the user's game library.
- Every bug fix should add a regression test when practical.
