# PlayBuilder Roadmap

## Current Milestone

### Milestone 3 — Atlas Profiles and Comparison Safety

- Persist Atlas rule preferences as named profiles.
- Add a legacy-versus-Atlas comparison report for migration validation.
- Expand integration coverage with real scan-report fixtures.
- Validate full-library 1G1R grouping after the singleton recommendation fix.
- Keep advanced Atlas controls available without making them part of the default beginner workflow.
- Improve the Collection Builder review workflow with persistent user selections, summary filtering, and page-specific Ask Atlas help.
- Keep scans running in the background and improve Library browsing for large catalogs.
- Correct duplicate reporting so alternate releases are grouped within the same canonical system only.
- Preserve catalog entries across additive scans and repair the Collection Builder to Build Preview workflow.

## Next Milestone

### Milestone 4 — Multi-disc Preservation

- Identify disc sets as one playable game.
- Prevent individual discs from competing against one another.
- Explain multi-disc grouping decisions.

## Future Ideas

- Rule editor and profile import/export.
- Evidence-based confidence indicators.
- DAT ingestion for No-Intro, Redump, TOSEC, GoodTools, and MAME.
- Archive-aware duplicate verification.
- Smart collections, playlists, and custom collections.
- Controlled dependency-security updates.

## Completed

### Milestone 2 — Deterministic Atlas Collection Integration (v0.3.0)

- Replaced inherited point accumulation with ordered deterministic comparisons.
- Connected Atlas to the live 1G1R preview.
- Added user-visible decision explanations.
- Fixed full-library 1G1R input so singleton title groups are recommended automatically.
- Restored the dedicated Scan Games page and simplified the default collection-building workflow.
- Added real Collection Builder selection controls, basic review filters, extra-version review, and page-specific Ask Atlas guidance.
- Added background scanning, Scan Games folder browsing, clearer Game-file terminology, and improved Library system browsing.
- Replaced the Library drawer-style Systems panel with a stable wider column and corrected duplicate grouping by canonical system.
- Added additive catalog scanning, explicit catalog replacement, system removal, system-scoped collections, and Build Preview support for current saved collection types.
- Added English-only and duplicate-group integration tests.

### Milestone 1 — Atlas Parsing Foundation (v0.2.0)

- Added tokenizer, parser, metadata, candidates, initial rules, and tests.

### Milestone 0 — Project Infrastructure (v0.1.0)

- Established documentation and milestone workflow.
