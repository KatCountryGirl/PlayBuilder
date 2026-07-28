# PlayBuilder Project Roadmap

**Version:** 1.1
**Status:** Living document
**Maintainer:** Wine Country Flix

---

## Purpose

This roadmap communicates PlayBuilder's planned evolution and helps ensure that each milestone supports the long-term product vision.

It is not a promise of delivery dates. Priorities may change when testing reveals a more important collector problem.

## Vision

PlayBuilder will become the easiest way to build, organize, and maintain curated game libraries from preservation archives and existing collections.

The goal is not to make users manage more ROM details. The goal is to help them build the libraries they actually want to play.

## Guiding Priorities

Every milestone should improve at least one of these areas:

- Simplicity.
- Reliability.
- Safety.
- Performance.
- Collector experience.
- Discoverability.
- Trust.

Features that increase complexity without meaningful collector benefit should be reconsidered.

## Current Phase — User Experience

PlayBuilder has a working foundation: catalog persistence, scanning, Atlas selection, collection building, tests, and technical documentation.

The current priority is transforming the functional application into a coherent, welcoming product through progressive disclosure, human language, and reliable end-to-end workflows.

### Current Milestone

## 2.12 — The Great UX Refactor

Status: Implemented for Product Owner review.

Goals:

- Simplify Collection Builder.
- Introduce clear workflows for 1G1R All Games, 1G1R English Only, Favorites, and Custom.
- Replace exposed Atlas terminology with collector language.
- Introduce simple preference choices such as English First, USA First, Europe First, and Japanese First.
- Move advanced Atlas controls into Custom or Advanced Options.
- Replace disruptive Inspect behavior with inline **Why?** explanations.
- Clarify Needs Review, Excluded, Showing, and Selected states.
- Make every visible action produce an immediate result.
- Make Review Build preserve the current plan and navigate to a meaningful Build Preview.
- Ensure Build Preview reflects the active working plan.
- Update automated tests and Product Owner browser-verification guidance.

## Planned Milestones

### 2.13 — Atlas Becomes Your Guide

- Expand collector-friendly explanations.
- Improve uncertainty and conflict messaging.
- Add clearer recommendation summaries.
- Improve the Needs Review workflow.
- Apply the Atlas Personality Guide consistently across the application.

### 2.14 — Library Renaissance

- Improve large-library browsing.
- Improve search and filtering.
- Improve system navigation.
- Improve release inspection and duplicate review.
- Strengthen favorite and collection-management workflows.

### 2.15 — Build Pipeline

- Improve build reliability.
- Improve progress reporting and cancellation.
- Improve file-operation diagnostics.
- Improve collision and overwrite handling.
- Strengthen reviewed execution and recovery behavior.

### 2.16 — Docker and Unraid Experience

- Provide a dependable container deployment path.
- Document persistent storage and permissions.
- Improve configuration and upgrade workflows.
- Provide Unraid-focused setup and troubleshooting guidance.

## Future Phases

### Collection Intelligence

- Improved language detection.
- Improved revision and special-release handling.
- Evidence-based confidence indicators.
- Collection health reports.
- Smart repair suggestions.
- Profile import and export.

### Library Management

- Tags and notes.
- Custom collections.
- Smart collections and playlists.
- Collection comparison.
- Recently added views.
- Statistics and collection summaries.
- Multiple libraries.

### Preservation and Metadata

- DAT ingestion for No-Intro, Redump, TOSEC, GoodTools, and MAME.
- Archive-aware verification.
- Additional metadata providers.
- Artwork management using owned or permissively licensed assets.
- Multi-disc preservation and playlist generation.

### Product Polish

- Better onboarding.
- Contextual help.
- Keyboard shortcuts.
- Accessibility improvements.
- Theme and visual refinements.
- Performance optimization.

### Community

- Contribution guide.
- Localization.
- Example collections.
- Sample datasets.
- Community collection templates.
- Carefully designed extension or plugin architecture.

## Future Ideas

Ideas intentionally not scheduled:

- Steam Deck optimization.
- RetroArch integration.
- Emulator-launcher profiles.
- Save-management integration.
- Achievement integration.
- Family mode.
- Parental controls.
- Portable mode.
- Import and export wizards.
- Collection sharing.
- Cross-platform launcher integrations.

## Dream Features

Ideas worth exploring after the core product is mature:

- Atlas learning from explicit collector choices without becoming unpredictable.
- Recommendation features that remain explainable.
- Community-curated collection templates.
- Game-history timelines.
- Rich collection-health visualization.

## Out of Scope

PlayBuilder is not intended to become:

- A ROM downloader.
- A torrent client.
- An emulator.
- A storefront.
- A replacement for every launcher.

Supporting integrations may exist, but PlayBuilder remains focused on curated library construction and maintenance.

## Completed Foundation

### Project Infrastructure

- Established milestone and documentation workflow.
- Established Git and Pull Request delivery practices.

### Atlas Parsing and Deterministic Selection

- Added tokenizer, parser, metadata, candidate, and rule foundations.
- Replaced inherited point accumulation with ordered deterministic comparisons.
- Connected Atlas to the live 1G1R preview.
- Added human-readable decision reasons and automated rule tests.

### Catalog and Collection Foundation

- Restored Scan Games and background scan status.
- Added additive catalog scanning and explicit catalog replacement.
- Scoped duplicate groups by canonical system and normalized title.
- Preserved multi-disc components.
- Added system-aware collection selection and stable recommendation identity.
- Added early Favorites collection-building support.

## Success Measures

PlayBuilder succeeds when collectors can:

- Scan a library quickly.
- Understand Atlas recommendations.
- Select systems and games without losing their work.
- Review exactly what will happen.
- Build a playable collection safely.
- Return later without relearning the application.

## Milestone Completion Checklist

- Solution builds successfully.
- Automated tests pass.
- Documentation is updated.
- Changelog is updated.
- Existing working behavior is preserved or intentionally changed.
- UX is clearer than before.
- Product Owner browser-verification checklist is provided.
- No unnecessary complexity is introduced.

## The North Star

Whenever the team must choose between adding another feature and making an existing feature dramatically easier to use, choose simplicity.

The greatest feature PlayBuilder can offer is confidence.

Every collector should finish building a library feeling:

> I spent my time choosing games, not fighting software.
