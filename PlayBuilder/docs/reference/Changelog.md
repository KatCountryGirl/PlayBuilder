# Changelog

## [Unreleased]

### Added
- Collection Builder now presents four clear workflows: 1G1R All Games, 1G1R English Only, Favorites, and Custom.
- 1G1R workflows now include simple release preference presets for English First, USA First, Europe First, Japanese First, and Custom.
- Collection Builder recommendations now include inline collector-friendly explanations instead of the fixed Atlas Inspector dialog.
- Saved collection rule state now records workflow, release preference, excluded count, and needs-review count for Build Preview summaries.
- Build Preview now summarizes the active working plan with collection workflow, selected systems, selected games, excluded games, needs-review count, and output information.
- Atlas recommendation explanations now translate deterministic decision data into collector-facing language for the UI.
- Collection Builder now has alias-aware system search, including `snes`, `nes`, `genesis`, `megadrive`, and `psp`.
- Collection Builder system selection now includes Select Matching for intentionally choosing only the visible system search results.
- Favorites now includes a collection-building search and selection area for finding catalog games, marking favorites, removing favorite flags, and saving a Favorites build plan.
- System cards now expose a lightweight media category resolver for later owned or permissively licensed artwork.
- Standard scans now add or update catalog entries instead of replacing unrelated systems.
- Scan Games now separates the default Add or Update Games action from an advanced, confirmed Replace Entire Catalog action.
- Library now includes Manage Systems catalog removal with confirmation and affected release counts.
- Collection Builder now includes reusable system selection for 1G1R and Favorites collections.
- Build Preview now includes system filtering, per-view counts, selected-system context, and saved collection deletion.
- Library duplicate status filtering now narrows results to same-system duplicate groups, needs-review releases, or healthy releases.
- Scans now continue in an application-level background session when users leave or refresh the Scan Games page.
- Scan Games now has Browse buttons for the source Game folder and destination folder.
- Library now has a wider stable Systems panel plus a System filter and system-name search.
- Collection Builder now supports real recommendation selection checkboxes with selected/excluded counts, Select All, Select None, Invert Selection, and Reset to Atlas Recommendations actions.
- Collection Builder summary cards now filter confident choices, needs-review recommendations, and extra versions without rerunning Atlas.
- Collection Builder now includes beginner-friendly search, language, region, selected-only, excluded-only, and needs-review filters.
- Ask Atlas now provides page-specific practical help for every main navigation page, including Collection Builder, Scan Games, and Build Preview guidance.
- Restored the Scan Games page at `/scan` with folder setup, frontend selection, scan progress, Add or Update Games, and scan summary.
- 1G1R scan groups now include every normalized title, including single-game titles, with compact Collection Builder diagnostics.
- Atlas Profiles with JSON-backed create, rename, delete, duplicate, active-profile switching, and saved Atlas preference options.
- Atlas Inspector in Collection Builder with read-only decision details, ordered candidates, and parsed metadata for each recommendation.
- Atlas comparison service for side-by-side legacy-versus-Atlas migration reports without changing live Collection Builder behavior.
- Comprehensive dedicated unit test coverage for every Atlas deterministic comparison rule.

### Changed
- Collection Builder now hides Atlas profiles, rule switches, detailed language and region ordering, and diagnostics unless the Custom workflow or Custom release preference is selected.
- Needs Review now represents genuine uncertainty or incomplete metadata rather than single-game recommendations.
- Collection Builder summary filters now use clear Recommended, Needs Review, and Excluded categories without duplicate checkbox filters.
- Review Build now persists the current checked plan with workflow metadata before navigating to Build Preview.
- Recommendation row keys and selection state now use stable system-aware identifiers so duplicate filenames across systems do not break rendering or saved build plans.
- Library and Favorites wording now distinguishes catalog management from building a playable Favorites collection.
- Multi-disc sets no longer count distinct required discs as duplicate releases.
- Collection Builder selection actions now apply to the currently filtered recommendation list and preserve selections across filters.
- Favorites collection wording now clarifies that saved Favorites collections are built from games marked as favorites in Library.
- Duplicate groups are now scoped by canonical system plus normalized game title, so matching titles on different systems are no longer counted or displayed as duplicates.
- Library now uses a stable wider Systems column instead of the collapsible drawer-style panel.
- Older saved scan reports with title-only duplicate groups are repaired from the SQLite catalog when loaded, without changing source game files.
- User-facing copy now uses Game or Game file instead of legacy technical wording where technical naming is not required.
- Scan Games now explains Frontend as the destination layout choice for finished collections.
- Build Preview creation from Collection Builder now uses the user's checked recommendation selections while leaving Atlas recommendations unchanged.
- Collection Builder keeps collection mode, language preference, region preference, summary, recommendation list, search, and basic filters in the default view while keeping advanced Atlas controls collapsed.
- Collection Builder now presents the simple 1G1R choices first and keeps profiles, rule switches, priority controls, and diagnostics collapsed under Advanced Options.
- Collection Builder, legacy comparison, and Atlas preview now use the full 1G1R group set while duplicate reporting remains duplicate-only.
- Atlas explanations now identify the first deterministic rule that selected the winner before listing any supporting rule matches.

### Fixed
- Fixed a Blazor unhandled-error banner in Collection Builder caused by duplicate `@key` values when multiple recommendation rows shared the same filename.
- Fixed system search using only plain display-name matching instead of canonical aliases and whitespace-tolerant partial matching.
- Fixed recommendation search so it also matches system names and stable normalized titles.
- Fixed Favorites search so metadata fields that do not exist yet do not break keyword matching.
- Fixed the Scan Games navigation link returning Not Found.
- Fixed 1G1R previews only showing duplicate title groups instead of recommending singleton title groups automatically.

## [0.3.0] - 2026-07-24

### Added
- Atlas-backed collection preview service.
- Detailed Atlas decision reasons in the Collection Builder UI.
- Integration tests for duplicate groups and English-only filtering.

### Changed
- Atlas now uses ordered deterministic comparisons instead of accumulated scores.
- The live 1G1R preview now uses Atlas while the legacy service remains available for migration comparison.

### Fixed
- Corrected the inherited scoring architecture that conflicted with the PlayBuilder Project Bible.

### Refactored
- Atlas rules now compare candidates directly and return human-readable outcomes.

## [0.2.0] - 2026-07-24

### Added
- Atlas parsing foundation and automated tests.

## [0.1.0] - 2026-07-24

### Added
- Project documentation infrastructure.
