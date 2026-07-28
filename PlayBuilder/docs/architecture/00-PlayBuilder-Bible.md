# PlayBuilder Bible

**Version:** 2.0
**Status:** Living document
**Maintainer:** Wine Country Flix

---

## Purpose

PlayBuilder is a collector-first application for building, organizing, and maintaining curated game libraries from preservation archives and existing game collections.

It helps collectors reduce clutter, choose the releases they actually want, and create clean playable libraries without requiring them to understand complicated ROM-management terminology.

PlayBuilder is not defined by how many technical controls it exposes. It is defined by how confidently a collector can move from a large archive to a library they are excited to play.

## Mission

PlayBuilder should be the easiest game collection manager to begin using while remaining powerful enough for advanced collectors as their needs grow.

It should be:

- Beginner first and expert capable.
- Safe by default.
- Deterministic and explainable.
- Powerful without being intimidating.
- Focused on real collector problems.

## Built by a Collector, for Collectors

PlayBuilder began as a practical way to reduce a large game archive into a clean, playable collection.

Every feature should solve a real collector problem. Features must not add complexity merely because the underlying engine can support it.

When a feature makes PlayBuilder harder to understand without meaningfully improving the collector's experience, redesign the feature.

## Product Roles

### Product Owner

The Product Owner controls:

- Product vision.
- Feature priorities.
- User workflows.
- UX decisions.
- Acceptance criteria.
- Browser verification.
- Final approval.

### Engineering

Engineering owns:

- Architecture.
- Implementation quality.
- Maintainability.
- Automated testing.
- Documentation updates.
- Build quality.
- Milestone packaging.

Engineering implements the product vision; it does not silently redefine it.

## Core Workflow

```text
Scan Games
    ↓
Library
    ↓
Collection Builder
    ↓
Build Preview
    ↓
Execute Build
```

Supporting tools may assist this workflow, but they must not obscure it.

## Core Product Areas

### 1. Scan Games

Scan Games discovers and catalogs the user's existing files.

The common workflow should require only:

- Source game folder.
- Destination folder.
- Frontend or output layout.
- A clear Add or Update Games action.

Scanning must be additive and non-destructive by default. Replacing the entire catalog must remain an explicit advanced action with confirmation.

### 2. Library

Library displays and manages the scanned PlayBuilder catalog.

It should support:

- Clear system browsing.
- Fast search and filtering.
- Release inspection.
- Duplicate review.
- Favorite flags.
- Catalog maintenance.

Removing catalog entries must never imply deleting the user's source files.

### 3. Collection Builder

Collection Builder is the heart of PlayBuilder.

Primary workflows include:

- 1G1R All Games.
- 1G1R English Only.
- Favorites.
- Custom collections.

The default workflow should emphasize:

- Collection type.
- Simple release preference.
- System selection.
- Game search.
- Game selection.
- Review Build.

Advanced Atlas rules belong behind Custom or Advanced Options.

### 4. Build Preview

Build Preview must show exactly what PlayBuilder intends to do before files are changed.

It should summarize:

- Selected games.
- Selected systems.
- Exclusions.
- Items needing review.
- Destination.
- Output layout.
- Estimated size when available.
- File operation mode.

The active Collection Builder plan should flow directly into Build Preview without requiring the user to recreate it.

### 5. Execute Build

Supported build strategies may include:

- Copy.
- Move.
- Hardlink.

Execution must be predictable, reviewed, and reversible where practical. Overwrite behavior must never be enabled silently.

### 6. Conversion

Conversion may support collector workflows such as:

- Individual file conversion.
- Batch or system conversion.
- Multi-disc playlist generation.
- Frontend-aware recommendations.

Conversion is a supporting capability, not the primary identity of PlayBuilder.

### 7. Metadata

Metadata may enrich the library with:

- Genres.
- Ratings.
- Franchises.
- Release information.
- Artwork references.
- Recommendations.

Metadata must supplement deterministic file selection rather than hide it behind an unexplained score.

### 8. Review, Cleanup, and Quarantine

Cleanup features must protect the source collection.

PlayBuilder should prefer quarantine over deletion and must preserve source-relative paths, detect collisions, and store enough information to support restoration.

### 9. Ask Atlas

Ask Atlas provides practical, page-specific guidance.

Atlas should explain:

- What the current page is for.
- What the primary controls do.
- What happens next.
- Why a release was selected.
- What remains safe and unchanged.

## Atlas Principles

Atlas is PlayBuilder's deterministic and explainable selection engine.

- Atlas uses ordered rules, not hidden accumulated scores.
- Atlas decisions must be repeatable.
- Atlas explanations must be human-readable.
- Atlas must admit uncertainty.
- Atlas must never choose personal favorites for the user.
- Atlas core logic remains independent from Blazor UI components.

## Safety Principles

- Never surprise the user.
- Never delete source files automatically.
- Preview before execution.
- Explain destructive or catalog-replacing actions before confirmation.
- Preserve alternatives for review.
- Make important actions predictable, explainable, reviewable, and reversible.

## Engineering Principles

- Prefer steady, buildable progress over disruptive rewrites.
- Preserve working behavior until its replacement is tested.
- Keep business rules out of Razor components.
- Keep Atlas independent from UI concerns.
- Prefer tokenizer and parser pipelines over giant regex-only parsers.
- Treat duplicates as alternate releases of the same normalized game on the same canonical system.
- Keep matching titles on different systems separate.
- Keep standard scans additive unless explicit replacement is confirmed.

## Supported Frontends

PlayBuilder may support layouts for:

- RetroBat.
- EmulationStation.
- Batocera.
- ES-DE.
- LaunchBox.
- HyperSpin.
- Personal folder structures.

Frontend support should be introduced only when the generated layout can be explained, previewed, and tested.

## Out of Scope

PlayBuilder is not intended to become:

- A ROM downloader.
- A torrent client.
- An emulator.
- A storefront.
- A replacement for every game launcher.

Integrations may support the collection-building workflow, but PlayBuilder's primary identity remains curated library construction and maintenance.

## Delivery Rules

Every coding milestone must:

1. Update relevant documentation.
2. Update the changelog.
3. Update the roadmap when priorities or milestone status change.
4. Contain no placeholder implementation.
5. Leave the project buildable.
6. Pass the automated test suite.
7. Provide a Product Owner browser-verification checklist.
8. Clearly state whether Visual Studio should build, rebuild, run, or wait.

## Guiding Promise

PlayBuilder should help collectors build the exact library they want without forcing them to become experts in game-file preservation terminology.

The user should think about games.

PlayBuilder should handle the complexity.
