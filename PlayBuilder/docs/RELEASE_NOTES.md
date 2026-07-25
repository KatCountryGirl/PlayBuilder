# Release Notes

## v0.3.0 — Deterministic Atlas Collection Integration

Atlas now powers the live 1G1R recommendation preview. Every selection is made by an ordered set of deterministic rules rather than a hidden point total. Expanding a recommendation shows the rules that favored the selected ROM.

Collection Builder now receives the full 1G1R title-group set from scans, including single-ROM titles. Duplicate reporting remains duplicate-only, and compact diagnostics show how many ROMs, title groups, exclusions, and final recommendations moved through the preview pipeline.

The source archive remains untouched. This release only previews recommendations.

### Validation

- Rebuild the PlayBuilder project.
- Run the application.
- Rescan the archive, then open Collection Builder.
- Confirm the heading identifies Atlas and expanded recommendations show decision reasons.
- Confirm singleton title groups appear as automatic recommendations and multi-disc titles remain separate candidates.
