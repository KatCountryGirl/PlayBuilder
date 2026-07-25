# Release Notes

## v0.3.0 — Deterministic Atlas Collection Integration

Atlas now powers the live 1G1R recommendation preview. Every selection is made by an ordered set of deterministic rules rather than a hidden point total. Expanding a recommendation shows the rules that favored the selected ROM.

The Atlas Comparison Report lets maintainers compare legacy CollectionRuleService output against Atlas side by side before the legacy engine is retired. The report is read-only and can export filtered results to CSV.

The source archive remains untouched. This release only previews recommendations.

### Validation

- Rebuild the PlayBuilder project.
- Run the application.
- Open Collection Builder after loading a scan report.
- Confirm the heading identifies Atlas and expanded recommendations show decision reasons.
- Open the Atlas Comparison Report from Collection Builder and confirm agreement summary, filters, search, and CSV export are available.
