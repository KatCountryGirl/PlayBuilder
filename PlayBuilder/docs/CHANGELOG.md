# Changelog

## [Unreleased]

### Added
- 1G1R scan groups now include every normalized title, including single-ROM titles, with compact Collection Builder diagnostics.
- Atlas Profiles with JSON-backed create, rename, delete, duplicate, active-profile switching, and saved Atlas preference options.
- Atlas Inspector in Collection Builder with read-only decision details, ordered candidates, and parsed metadata for each recommendation.
- Atlas comparison service for side-by-side legacy-versus-Atlas migration reports without changing live Collection Builder behavior.
- Comprehensive dedicated unit test coverage for every Atlas deterministic comparison rule.

### Changed
- Collection Builder, legacy comparison, and Atlas preview now use the full 1G1R group set while duplicate reporting remains duplicate-only.
- Atlas explanations now identify the first deterministic rule that selected the winner before listing any supporting rule matches.

### Fixed
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
