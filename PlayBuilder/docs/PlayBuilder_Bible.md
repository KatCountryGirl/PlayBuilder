# PlayBuilder Project Bible

**Version:** 1.1  
**Current application version:** 0.2.0 development

## Product

PlayBuilder is a Windows-focused game-file management application built with .NET 10, Blazor Server, C#, Entity Framework Core, and SQLite.

## Roles

The Product Owner controls features, UI, workflow, priorities, and acceptance testing. The Lead Software Engineer owns architecture, implementation, maintainability, documentation, build quality, and milestone packaging.

## Delivery rules

Every coding milestone must:

1. Update relevant documentation.
2. Update `CHANGELOG.md`.
3. Update `ROADMAP.md`.
4. provide complete changed files in a ZIP.
5. contain no placeholder implementations.
6. leave the project buildable.
7. clearly state whether Visual Studio should build, rebuild, run, or wait.

## Engineering principles

- Prefer steady, buildable progress over disruptive rewrites.
- Keep the Atlas core independent from the Blazor UI.
- Use deterministic rules with human-readable explanations.
- Prefer tokenizer and parser pipelines over giant regex-only parsers.
- Preserve working legacy behavior until its replacement is tested and integrated.
- Treat duplicate groups as same-system alternate releases, not matching titles across different systems.
- Standard scans must be non-destructive catalog additions or updates unless the user explicitly confirms catalog replacement.
- Library manages the scanned catalog, release inspection, duplicate review, metadata, catalog removal, and favorite flags.
- Collections > Favorites builds a playable collection from explicitly selected games; Atlas must not decide personal favorites.
- Collection Builder system selection must support alias-aware search and explicit Select Matching behavior without silently changing selection on search.
- Future quarantine must preserve source directory structure below a configured quarantine root, detect collisions, and store restoration records before any file move occurs.
