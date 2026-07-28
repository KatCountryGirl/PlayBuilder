# PlayBuilder Design System

**Version:** 1.1
**Status:** Living document
**Maintainer:** Wine Country Flix

---

## Purpose

This document defines how PlayBuilder should look, behave, and interact.

It exists to ensure that every screen feels like part of the same application regardless of who implements it. When a design decision is unclear, consult this document before writing code.

If an implementation conflicts with this document, redesign the implementation unless the Product Owner approves a documented exception.

## Design Goals

PlayBuilder should feel:

- Friendly.
- Modern.
- Fast.
- Predictable.
- Collector-focused.
- Understandable.
- Safe.

The interface should disappear behind the workflow. The user should think about games, not software.

## Core Questions

Every screen should answer three questions immediately:

1. Where am I?
2. What can I do?
3. What should I do next?

If any answer is unclear, the page needs redesign.

## Progressive Disclosure

Show only the controls needed for the current workflow.

- Common workflows remain simple.
- Advanced workflows remain powerful.
- Atlas profiles, rule switches, diagnostics, and migration tools belong in Advanced Options.
- Custom mode may reveal detailed language, region, revision, translation, and Atlas controls.

Complexity should be available, not unavoidable.

## Navigation

Navigation must always make the current location obvious and use collector-friendly labels.

- Use consistent labels across navigation, headings, and actions.
- Do not hide primary destinations behind ambiguous icons.
- Navigation actions should not resemble destructive or form-submission actions.

## Page Layout

Use a consistent page structure:

1. Page title.
2. Short description.
3. Primary action or next step.
4. Main content.
5. Secondary tools.
6. Status or summary.

Users should never hunt for the primary action.

## Language and Terminology

Use collector language in the default interface.

Prefer:

- Game.
- Game file.
- Library.
- Collection.
- Build.
- Review.
- Why this version?
- English First.
- USA First.

Avoid in the default workflow:

- Candidate.
- Eligibility.
- Evaluation.
- Heuristic.
- Priority rule.
- Engine terminology.

Technical terminology may appear in Advanced mode or technical documentation when it adds value.

Use **Add or Update Games** for the normal scan action. Catalog replacement must be advanced, confirmed, and explicit that original files are unchanged.

## Buttons and Actions

Button appearance communicates importance.

- **Primary:** Scan, Build, Save, Review Build.
- **Secondary:** Cancel, Reset, Back, Select None.
- **Navigation:** View, Open Library, Continue.
- **Dangerous:** Delete, Remove, Clear Database, Replace Entire Catalog.
- **Atlas assistance:** Ask Atlas, Explain Decision, Why this version?

Every button must produce visible feedback by changing the UI, changing selection, navigating, updating status, or revealing information.

Never allow dead clicks or silent failures.

Explain actions before they modify files. Dangerous actions must state what will change and what will remain untouched.

## Cards

Use cards for selecting meaningful concepts such as collection type or frontend.

- A card may look clickable only when clicking performs an action.
- Hover, focus, and selected states must accurately represent behavior.
- Non-interactive summaries must not mimic selectable cards.
- Primary choices should remain understandable without relying on icons.

## Search

Every search field should behave consistently.

- Typing filters results immediately when practical.
- Search changes visibility, not selection.
- Selections survive filtering.
- Search should support natural names and known system aliases.
- Partial matching should be supported where useful.
- Search should display visible and selected counts separately.

Example:

```text
Showing: 128 games
Selected: 34,220 games
```

System search remains visibility-only until the user chooses Select Matching, Select All, Select None, or changes an individual checkbox.

## Lists and Tables

- Use alphabetical order unless another order clearly benefits the collector.
- Never expose random or implementation order.
- Keep spacing, alignment, labels, and row actions consistent.
- Provide sorting when it improves large-library browsing.
- Use sticky headers only when they improve usability.
- Keep system navigation readable and stable on desktop.
- Avoid drawers or panels that overlap primary navigation.

## Selection

Checkboxes represent inclusion and must not carry multiple meanings.

- Use real form controls, not decorative status icons.
- Update selection counts immediately.
- Preserve hidden selections while filtering.
- Bulk actions affect the currently visible or clearly described result set.
- Button labels must make bulk-action scope obvious.
- Keep system selection controls visible above a bounded, scrollable system list.

## Counts and Status

Always distinguish:

- Showing.
- Selected.
- Excluded.
- Needs Review.
- Total Library.

Never use one number for multiple concepts.

Do not present an Atlas total as an unexplained quality score.

Duplicate indicators must explain that they refer to same-system alternate releases, not necessarily byte-for-byte identical files.

## Empty States

Do not display vague messages such as **No Items**.

Explain what happened and what the user can do next.

Examples:

- **No games matched your search.** Try removing a filter.
- **No systems selected.** Choose one or more systems to continue.
- **No scan results yet.** Add or update games to begin.

## Loading and Long Operations

Never make the application appear frozen.

Long-running operations should show:

- Current operation.
- Progress.
- Meaningful status text.
- Cancellation when practical.
- Estimated remaining work only when the estimate is trustworthy.

Background scan progress should remain available after navigation or refresh.

## Errors

Never expose raw exception text as the primary user message.

A user-facing error should explain:

1. What PlayBuilder was doing.
2. Whether anything changed.
3. What the user can do next.

Example:

> PlayBuilder encountered an unexpected problem while building this collection. The operation stopped safely and no source files were changed.

Log the technical details separately for diagnosis.

## Atlas Messages and Ask Atlas

Atlas should explain decisions in collector language.

Good:

> I chose this version because it is the newest English release.

Avoid:

> Candidate evaluation complete.

Ask Atlas help should be page-specific and explain:

- The purpose of the page.
- The primary controls.
- The next step.
- Safety information.
- Common points of confusion.

Atlas explanations should preserve alternatives for review and clearly state uncertainty when a decision is not confident.

## Tooltips

Tooltips explain purpose, not implementation.

Good:

> **English First** — Prefer English releases whenever one exists.

Avoid tooltips that merely repeat an internal label.

## Icons

- Icons reinforce labels; they do not replace them for important actions.
- Use a consistent icon style.
- Avoid icons with ambiguous meaning.
- System artwork must be owned or permissively licensed.
- Artwork must never hide the system name or selection control.

## Color

Color reinforces meaning but is never the only signal.

Every state communicated through color must also use text, shape, iconography, or another accessible indicator.

## Dialogs and Inline Disclosure

Dialogs interrupt the workflow and should be reserved for confirmations, focused decisions, or information that cannot remain inline.

Prefer inline expansion for explanations such as **Why this version?**

Use confirmation dialogs for destructive, catalog-replacing, or irreversible actions.

## Collection Builder

The default Collection Builder workflow should focus on:

- Collection type.
- Simple release preference.
- System selection.
- Game search.
- Game selection.
- Review Build.

Advanced Atlas controls belong behind Custom or Advanced Options.

Favorites must be described as a user-selected playable collection. Atlas must not imply that it decides personal favorites.

## Review Build

Review Build must immediately preserve the current working plan and proceed to a meaningful preview.

It should summarize exactly what will happen:

- Games.
- Systems.
- Destination.
- Output layout.
- Exclusions.
- Needs Review.
- Estimated size when available.

Nothing important should be hidden.

## Build Preview

Build Preview must reflect the active working plan from Collection Builder.

It must not require the user to recreate or re-save the collection before reviewing it.

Prefer previews and summaries before execution.

## Cleanup and Quarantine

Future cleanup and quarantine controls must:

- Avoid implying source deletion unless deletion is truly intended.
- Preserve source-relative directory structure.
- Detect destination collisions.
- Require an explicit reviewed plan.
- Support restoration records where practical.

## Accessibility

- Use semantic controls and labels.
- Preserve keyboard navigation.
- Provide visible focus states.
- Do not rely on color alone.
- Use scalable text and readable contrast.
- Keep status and error messages specific and actionable.
- Support screen readers where practical.

## Responsiveness

Desktop is the primary experience.

Tablet should remain usable. Mobile support is welcome but must not compromise the desktop workflow.

On narrower screens, content should stack predictably rather than slide beneath navigation or become unreachable.

## Consistency Rules

- Controls performing the same task must behave identically.
- Pages solving the same problem should use the same interaction pattern.
- Do not invent a second pattern without a compelling reason.
- Maintain the established PlayBuilder visual language unless a redesign is approved.

## Simplicity Budget

Every new control must justify its presence.

When a feature introduces significant complexity, consider whether another control can be simplified, combined, moved to Advanced Options, or removed.

PlayBuilder should continuously become easier to use, not merely more powerful.

## The Collector Test

Before implementing a screen, ask:

> Would someone who loves classic games but has never heard of No-Intro understand what to do next?

If not, simplify the workflow before adding more features.

Complexity belongs inside Atlas, not on the screen.

## Final Rule

PlayBuilder should never impress users with complexity.

It should impress them with how little complexity they had to learn.
