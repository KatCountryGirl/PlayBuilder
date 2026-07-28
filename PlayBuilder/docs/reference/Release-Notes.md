# Release Notes

## v0.3.0 — Deterministic Atlas Collection Integration

Atlas now powers the live 1G1R recommendation preview. Every selection is made by an ordered set of deterministic rules rather than a hidden point total. Expanding a recommendation shows the rules that favored the selected game file.

Collection Builder now receives the full 1G1R title-group set from scans, including single-game titles. Duplicate reporting remains duplicate-only, and compact diagnostics show how many game files, title groups, exclusions, and final recommendations moved through the preview pipeline.

The Scan Games page is available again from the sidebar. New users can choose a source Game folder, destination folder, and frontend, run or repeat a scan, then continue to Collection Builder. Advanced Atlas profile and rule controls remain available but are collapsed by default.

Scans now continue in the background if the user leaves or refreshes Scan Games. Returning to the page shows the current status, progress, cancellation control, and last completed summary. Browse buttons help choose source and destination folders while keeping manual path entry available.

The Library Systems panel now uses a stable wider desktop column instead of a collapsible drawer. The Library filters include System and duplicate status, and search covers titles, filenames, and system names. On narrower browser widths, the Systems section stacks above the game table so it does not slide under the main application navigation.

Duplicate groups now mean groups of two or more releases that appear to represent the same game on the same canonical system. Matching titles on different systems, such as the same game name on SNES and Genesis, are kept separate. Older saved scan reports that do not include system-scoped group keys are repaired from the SQLite catalog when loaded.

Standard scans now add or update PlayBuilder catalog entries instead of replacing unrelated systems. Scan Games exposes Replace Entire Catalog only as an advanced confirmed action, and Library adds Manage Systems for intentionally removing catalog systems without touching original game files.

Multi-disc sets are no longer counted as duplicate release groups simply because Disc 1 and Disc 2 share a title. Collection Builder and Build Preview keep current saved collection types flowing through one review path, with selected-system scope and system filtering.

Collection Builder now uses real checkboxes for reviewed recommendations. Every Atlas recommendation starts selected, users can exclude individual games, selection state survives filtering, and Build Preview uses the checked set without changing Atlas decisions. Summary cards now filter confident choices, needs-review items, and extra versions for faster review.

Collection Builder system selection has been repaired with alias-aware search and explicit Select Matching behavior. Searches such as `snes`, `super`, `nes`, `genesis`, `megadrive`, and `psp` can find the expected systems, and changing search text no longer silently changes selected systems.

The Collection Builder unhandled-error banner caused by duplicate recommendation render keys has been corrected. Recommendation rows now use stable system-aware selection identity, so duplicate filenames across systems can render and save safely.

Favorites now has a collection-building foundation inside Collections. Users can search selected systems for games, select matching results, add or remove favorite flags, and save a Favorites collection plan without treating Atlas as a personal-favorites chooser.

Ask Atlas now gives page-specific help across the main app. The guidance explains what each page is for, the main controls, next steps, safety notes, and common confusion points in beginner-friendly language.

The source archive remains untouched. This release only previews recommendations.

## Milestone 2.12 — The Great UX Refactor

Collection Builder now starts from four collector-facing workflows: 1G1R All Games, 1G1R English Only, Favorites, and Custom. The normal 1G1R flow focuses on collection type, release preference, systems, search, game selection, and Review Build. Atlas profiles, deterministic rule switches, detailed priority editors, and diagnostics remain available through Custom or Custom preference controls instead of appearing in the default path.

Release preferences now use simple presets: English First, USA First, Europe First, Japanese First, and Custom. These presets adjust existing deterministic Atlas options; they do not add scoring or change Atlas into a weighted engine.

The disruptive Inspect action has been replaced by inline "Why this version?" explanations. Atlas still keeps structured decision data internally, while the page translates the selected reasoning into calm collector-facing language and explains that alternate files stay in the library unless selected later.

Needs Review now indicates genuine uncertainty, such as incomplete language or region information or an unresolved tie. Single-game 1G1R groups that Atlas can recommend clearly are no longer presented as needing review merely because there was only one candidate.

Review Build now saves the active checked plan, selected systems, workflow, release preference, excluded count, and needs-review count before navigating to Build Preview. Build Preview displays that working-plan context with destination and frontend details while remaining preview-only.

### Validation

- Rebuild the PlayBuilder project.
- Run the application.
- Confirm Collection Builder shows the four workflows and the selected workflow is visually obvious.
- Confirm normal 1G1R workflows show release preferences, system selection, game search, game selection, counts, and Review Build without Atlas rule controls.
- Confirm Custom exposes advanced Atlas controls and diagnostics, while simple workflows do not.
- Confirm English First, USA First, Europe First, and Japanese First visibly refresh the recommendation plan.
- Confirm Custom release preference reveals detailed language and region priority controls.
- Confirm game search changes visible rows without changing selected games.
- Confirm Recommended, Needs Review, and Excluded summary filters visibly toggle and change the result count.
- Confirm each recommendation expands inline with collector-friendly "Why this version?" or needs-review wording.
- Confirm Review Build saves the checked recommendations and opens Build Preview.
- Confirm Build Preview shows the active workflow, selected systems, selected games, excluded count, needs-review count, destination, and frontend.
- Open Scan Games from the sidebar and confirm the page loads.
- Add or update games from the archive, then open Collection Builder.
- Confirm a scan continues after navigating away from Scan Games and returning.
- Confirm Scan Games Browse buttons open a folder picker on Windows.
- Confirm Library system filtering and system-name search work alongside the Systems panel.
- Confirm matching titles on different systems are not counted as duplicate groups.
- Confirm the Library duplicate-status filter only shows same-system alternate releases.
- Confirm scanning one system and then another keeps both systems in the catalog.
- Confirm Replace Entire Catalog requires confirmation and changes only PlayBuilder catalog records.
- Confirm multi-disc games are not counted as duplicate groups when the only multiple files are required discs.
- Confirm the heading identifies Atlas and expanded recommendations show decision reasons.
- Confirm Collection Builder selections can be checked, unchecked, reset, and sent to Build Preview.
- Confirm Collection Builder system search supports `snes`, `super`, `nes`, `genesis`, `megadrive`, and `psp`.
- Confirm Select All, Select None, Select Matching, and individual system toggles update counts without a Blazor unhandled-error banner.
- Confirm recommendation filters and selection buttons update the visible recommendation list and selected/excluded counts.
- Confirm Favorites search can find games such as Mario from selected systems, and selected Favorites can be added, removed, saved, and reviewed.
- Confirm Ask Atlas content changes as the user moves between the main navigation pages.
- Confirm singleton title groups appear as automatic recommendations and multi-disc titles remain separate candidates.
