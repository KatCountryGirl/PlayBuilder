# PlayBuilder Developer Standards

**Version:** 1.1
**Status:** Living document
**Maintainer:** Wine Country Flix

---

## Purpose

This document defines how PlayBuilder is engineered, tested, documented, and delivered.

Every contributor, whether human or AI-assisted, should follow these standards so the project remains stable, maintainable, and consistent with the Product Owner's vision.

## Project Philosophy

PlayBuilder is a long-term project.

- Favor maintainability over cleverness.
- Favor readability over brevity.
- Favor consistency over personal preference.
- Leave the project buildable after every milestone.
- Treat documentation as part of the implementation.

Code should feel like it was written by one team regardless of who authored it.

## Product Ownership

The Product Owner defines:

- Vision.
- Feature priorities.
- UX decisions.
- Workflows.
- Acceptance criteria.
- Browser verification.
- Final approval.

Contributors implement the vision. They must not silently redefine it.

## Source of Truth

Before implementing relevant work, read:

1. PlayBuilder Bible.
2. UX Manifesto.
3. Design System.
4. Atlas Personality Guide.
5. Developer Standards.
6. Project Roadmap.
7. Relevant technical references.

If implementation conflicts with these documents, redesign before writing code or document the issue for Product Owner approval.

## Milestone Workflow

Every milestone follows this lifecycle:

1. Create a feature branch.
2. Implement the feature.
3. Add or update tests.
4. Update documentation.
5. Update the changelog.
6. Build the solution.
7. Run the full test suite.
8. Commit the changes.
9. Push the feature branch.
10. Open a Pull Request.
11. Provide a browser-verification checklist.
12. Merge only after Product Owner approval.

Never commit milestone work directly to `main`.

## Definition of Done

A milestone is complete only when:

- The solution builds successfully.
- Automated tests pass.
- The feature is fully implemented.
- No placeholder code remains.
- Existing functionality is preserved unless an approved change says otherwise.
- Documentation and changelog entries are updated.
- A Pull Request exists.
- Manual browser-verification steps are provided.

## Build Quality

Every milestone must leave PlayBuilder in a buildable state.

Do not merge:

- Compilation errors.
- Failing tests.
- `TODO` implementations.
- `NotImplementedException` placeholders.
- Fake production data.
- Empty methods presented as finished functionality.
- Intentionally broken features.

Partial work belongs on the feature branch until complete.

## Browser Verification

Browser verification is performed by the Product Owner.

Development agents should:

- Build the solution.
- Run automated tests.
- State the exact command or Visual Studio action used.
- Provide a concise manual checklist.
- Stop any application process launched for a smoke test.
- Never wait indefinitely for browser interaction they cannot perform.

## Architecture

- Business logic belongs in services or domain components.
- UI concerns belong in Razor components and presentation models.
- Persistence belongs in repositories, data services, or the data layer.
- Razor components must not directly implement complex Atlas rules.
- Atlas core logic remains independent from Blazor UI components.
- Long-lived Blazor services must use safe database lifetimes.

## C# Standards

- Keep nullable reference types enabled.
- Use file-scoped namespaces.
- Use explicit, descriptive names.
- Prefer immutable records for value-like data.
- Prefer sealed classes for services unless inheritance is required.
- Validate public method arguments.
- Prefer collection expressions when they improve readability.
- Keep methods focused on one responsibility.
- Comments should explain why, not restate what the code already says.

## Dependency Injection

- Constructor-inject collaborators.
- Avoid service-locator patterns.
- Register interfaces where an abstraction provides real value.
- Keep services safe for their registered lifetime.
- Do not resolve scoped services from singletons without an appropriate factory or scope.

## Asynchronous Code

- Use asynchronous APIs for I/O and database work.
- Pass cancellation tokens through long-running operations.
- Do not use `async void` except framework event handlers that require it.
- Avoid blocking async work with `.Result`, `.Wait()`, or equivalent calls.
- Keep the UI responsive during scans, builds, imports, and conversions.

## Entity Framework Core

- Use `IDbContextFactory<PlayBuilderDbContext>` from long-lived Blazor services.
- Keep `DbContext` instances short-lived.
- Use no-tracking reads when practical.
- Keep database operations cancellable when practical.
- Treat migrations and schema changes as explicit milestone work.
- Avoid destructive catalog changes without confirmation and tests.

## Atlas Engineering

- Atlas uses ordered deterministic comparison rules, not hidden point accumulation.
- The first rule that distinguishes candidates determines their ordering.
- Atlas output must include structured, human-readable reasons.
- The same inputs and settings must produce the same result.
- Atlas groups must remain scoped by canonical system and normalized game title.
- Multi-disc components must not compete merely because they share a title.
- UI filtering must not silently alter Atlas rule order.

## Error Handling and Logging

- Never swallow exceptions silently.
- Log enough technical context to diagnose failures.
- Do not expose stack traces as the primary user message.
- User-facing errors must state what stopped, whether data changed, and what to do next.
- Protect source files and catalog integrity during failure paths.

## Performance

- Prefer responsive user experience over raw throughput.
- Measure before optimizing.
- Avoid blocking the UI thread.
- Report progress for long-running work.
- Make cancellation available when practical.
- Avoid loading an entire large catalog into memory unless necessary and measured.

## Testing

- Parsing and rule behavior require automated tests.
- Tests must be deterministic.
- Tests must not depend on the Product Owner's personal game library.
- Every bug fix should add a regression test when practical.
- New features should include meaningful behavior coverage.
- Avoid tests that exist only to increase a count.
- Integration tests should validate important service boundaries and persistence behavior.

## UX Before Code

Before implementing a feature, ask:

- Does it improve the collector's experience?
- Does it pass the Collector Test?
- Is the common workflow still simple?
- Is advanced complexity properly disclosed?
- Does every new control justify its presence?

If not, redesign before implementation.

## Backward Compatibility and Migration

Preserve existing collections and settings whenever practical.

When a breaking change is necessary:

- Explain it.
- Document it.
- Provide migration or repair behavior where practical.
- Test older saved data when relevant.
- Never silently discard user-created plans.

## Dependencies

- Prefer the .NET platform before adding external packages.
- Add dependencies only when they provide clear value.
- Document why a significant dependency is needed.
- Consider maintenance, licensing, security, and upgrade cost.

## Security and Privacy

- Never commit secrets, credentials, tokens, or personal paths.
- Use configuration and secret storage appropriately.
- Treat user data and source paths with care.
- Avoid sending library information to external services without clear user intent.

## Documentation

Significant feature work should update the appropriate documents:

- Architecture changes → `reference/Architecture.md`.
- Atlas behavior changes → `reference/Atlas-Engine.md`.
- Product or UX changes → core documents where approved.
- User-visible changes → `reference/Changelog.md`.
- Release validation → `reference/Release-Notes.md`.
- Milestone direction → `roadmap/05-Project-Roadmap.md`.

Documentation is part of the feature, not an afterthought.

## Pull Requests

Pull Requests should include:

- Purpose.
- Summary.
- Important files changed.
- Architecture or UX decisions.
- Build result.
- Test result and count.
- Known limitations.
- Product Owner browser-verification checklist.

Prefer focused PRs over unrelated large refactors.

## Commit Messages

Use clear, descriptive commit messages.

Good examples:

- `Simplify Collection Builder workflow`
- `Fix duplicate SelectionKey rendering issue`
- `Add inline Atlas decision explanations`

Avoid:

- `Update`
- `Changes`
- `Fix stuff`
- `Misc`

## AI-Assisted Development

AI is a development partner, not the Product Owner.

AI-assisted contributors should:

- Read the project documents first.
- Explain meaningful trade-offs.
- Avoid inventing requirements.
- Avoid silently changing product direction.
- Produce complete, buildable implementations.
- Run builds and tests before declaring completion.
- Leave browser verification to the Product Owner.

## Visual Studio Support

Instructions for the Product Owner should be beginner-friendly and Visual Studio aware.

Provide:

- Menu paths when useful.
- Exact build or rebuild guidance.
- Git branch and pull instructions.
- Clear guidance about whether to run or stop the application.
- Command-line instructions only when they provide real value or no equivalent workflow exists.

## Git Standards

- Default branch: `main`.
- Feature branches: `feature/<milestone-or-feature-name>`.
- Never rewrite published shared history.
- Merge through Pull Requests.
- Do not automatically merge on behalf of the Product Owner.
- Remove remote feature branches after an approved merge when appropriate.

## Accessibility

Accessibility is required engineering quality.

- Preserve keyboard access.
- Use semantic labels.
- Provide visible focus.
- Do not rely on color alone.
- Keep messages clear and actionable.

## Leave It Better Than You Found It

Small related improvements are encouraged:

- Clarify a confusing name.
- Remove dead code.
- Simplify an unnecessarily complex method.
- Add a missing regression test.
- Improve nearby documentation.

Avoid unrelated large refactors, but do not ignore a safe opportunity to make the touched area cleaner.

## Final Standard

Every line of code should make PlayBuilder easier to use, easier to maintain, safer, or easier to extend.

If it does none of those things, it does not belong in the project.
