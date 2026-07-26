# Release Notes

## v0.3.0 — Deterministic Atlas Collection Integration

Atlas now powers the live 1G1R recommendation preview. Every selection is made by an ordered set of deterministic rules rather than a hidden point total. Expanding a recommendation shows the rules that favored the selected game file.

Collection Builder now receives the full 1G1R title-group set from scans, including single-game titles. Duplicate reporting remains duplicate-only, and compact diagnostics show how many game files, title groups, exclusions, and final recommendations moved through the preview pipeline.

The Scan Games page is available again from the sidebar. New users can choose a source Game folder, destination folder, and frontend, run or repeat a scan, then continue to Collection Builder. Advanced Atlas profile and rule controls remain available but are collapsed by default.

Scans now continue in the background if the user leaves or refreshes Scan Games. Returning to the page shows the current status, progress, cancellation control, and last completed summary. Browse buttons help choose source and destination folders while keeping manual path entry available.

The Library Systems panel now uses a stable wider desktop column instead of a collapsible drawer. The Library filters include System and duplicate status, and search covers titles, filenames, and system names. On narrower browser widths, the Systems section stacks above the game table so it does not slide under the main application navigation.

Duplicate groups now mean groups of two or more releases that appear to represent the same game on the same canonical system. Matching titles on different systems, such as the same game name on SNES and Genesis, are kept separate. Older saved scan reports that do not include system-scoped group keys are repaired from the SQLite catalog when loaded.

Collection Builder now uses real checkboxes for reviewed recommendations. Every Atlas recommendation starts selected, users can exclude individual games, selection state survives filtering, and Build Preview uses the checked set without changing Atlas decisions. Summary cards now filter confident choices, needs-review items, and extra versions for faster review.

Ask Atlas now gives page-specific help across the main app. The guidance explains what each page is for, the main controls, next steps, safety notes, and common confusion points in beginner-friendly language.

The source archive remains untouched. This release only previews recommendations.

### Validation

- Rebuild the PlayBuilder project.
- Run the application.
- Open Scan Games from the sidebar and confirm the page loads.
- Rescan the archive, then open Collection Builder.
- Confirm a scan continues after navigating away from Scan Games and returning.
- Confirm Scan Games Browse buttons open a folder picker on Windows.
- Confirm Library system filtering and system-name search work alongside the Systems panel.
- Confirm matching titles on different systems are not counted as duplicate groups.
- Confirm the Library duplicate-status filter only shows same-system alternate releases.
- Confirm the heading identifies Atlas and expanded recommendations show decision reasons.
- Confirm Collection Builder selections can be checked, unchecked, reset, and sent to Build Preview.
- Confirm Ask Atlas content changes as the user moves between the main navigation pages.
- Confirm singleton title groups appear as automatic recommendations and multi-disc titles remain separate candidates.
