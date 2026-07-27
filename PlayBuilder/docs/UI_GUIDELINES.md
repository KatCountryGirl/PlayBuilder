# PlayBuilder UI Guidelines

## Product qualities

The UI should feel friendly, understandable, and safe for users who do not know game-file management terminology.

## Rules

- Explain actions before they modify files.
- Prefer previews and summaries before execution.
- Use plain language first; technical details may be secondary.
- Use Game or Game file in user-facing text. Keep established technical internals unchanged when renaming would add risk.
- Keep primary actions visually obvious.
- Show why Atlas selected a release and preserve alternatives for review.
- Never present an Atlas total as an unexplained quality score.
- Keep collection mode, language preference, region preference, summary, search, basic filters, selection controls, and build actions visible in the default Collection Builder workflow.
- Keep Atlas profiles, rule switches, developer diagnostics, and legacy comparison tools inside collapsed Advanced Options sections.
- Use real form controls for user choices; selected recommendations must use checkboxes rather than decorative status icons.
- Ask Atlas help should be page-specific and explain purpose, controls, next steps, safety information, and common questions in practical language.
- Scan progress should remain visible after navigation or refresh when work continues in the background.
- Library browsing should keep system navigation readable and provide equivalent filter/search access for large catalogs.
- Prefer a stable Library content layout with a readable Systems column over drawer behavior that can overlap the main app navigation.
- Duplicate indicators should explain that they mean same-system alternate releases, not byte-for-byte identical files.
- Default scanning language should say Add or Update Games. Catalog replacement must be advanced, confirmed, and clear that original game files are not changed.
- Collection Builder bulk actions should describe and affect the currently filtered result set.
- System search in Collection Builder should be alias-aware, partial-match capable, and visibility-only until the user chooses Select Matching, Select All, Select None, or an individual checkbox.
- System selection controls should remain visible above a bounded scrollable system list, with selected counts updating immediately.
- Favorites collections should be described as saved build plans created from explicitly selected favorite games, while Library remains the place to manage the scanned catalog and favorite flags.
- Future cleanup or quarantine controls must preserve source directory structure and must never imply destructive source-file changes without an explicit reviewed plan.
- Future system artwork should use owned or permissively licensed media-category icons and should never hide the checkbox or system name.
- Maintain the existing PlayBuilder visual language unless a redesign is approved.

## Accessibility

- Use semantic controls and labels.
- Preserve keyboard navigation.
- Do not rely on color alone to communicate status.
- Keep status and error messages specific and actionable.
