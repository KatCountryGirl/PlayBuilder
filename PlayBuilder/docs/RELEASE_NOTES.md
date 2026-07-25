# Release Notes

## v0.3.0 — Deterministic Atlas Collection Integration

Atlas now powers the live 1G1R recommendation preview. Every selection is made by an ordered set of deterministic rules rather than a hidden point total. Expanding a recommendation shows the rules that favored the selected ROM.

Collection Builder now receives the full 1G1R title-group set from scans, including single-ROM titles. Duplicate reporting remains duplicate-only, and compact diagnostics show how many ROMs, title groups, exclusions, and final recommendations moved through the preview pipeline.

The Scan Games page is available again from the sidebar. New users can choose a source ROM folder, destination folder, and frontend, run or repeat a scan, then continue to Collection Builder. Advanced Atlas profile and rule controls remain available but are collapsed by default.

Collection Builder now uses real checkboxes for reviewed recommendations. Every Atlas recommendation starts selected, users can exclude individual games, selection state survives filtering, and Build Preview uses the checked set without changing Atlas decisions. Summary cards now filter confident choices, needs-review items, and extra versions for faster review.

Ask Atlas now gives page-specific help across the main app. The guidance explains what each page is for, the main controls, next steps, safety notes, and common confusion points in beginner-friendly language.

The source archive remains untouched. This release only previews recommendations.

### Validation

- Rebuild the PlayBuilder project.
- Run the application.
- Open Scan Games from the sidebar and confirm the page loads.
- Rescan the archive, then open Collection Builder.
- Confirm the heading identifies Atlas and expanded recommendations show decision reasons.
- Confirm Collection Builder selections can be checked, unchecked, reset, and sent to Build Preview.
- Confirm Ask Atlas content changes as the user moves between the main navigation pages.
- Confirm singleton title groups appear as automatic recommendations and multi-disc titles remain separate candidates.
