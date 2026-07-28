# PlayBuilder Architecture

## Application

PlayBuilder is a .NET 10 Blazor Server application using SQLite through Entity Framework Core.

## Atlas Boundary

Atlas is isolated under `Services/Atlas` and has no dependency on Blazor UI components. The collection preview adapter lives in `Services/AtlasCollectionRuleService.cs` and translates Atlas decisions into UI-facing preview models.

## Dependency Flow

Blazor Components → Application service interfaces → Atlas adapter / collection services → Atlas engine and parsing → data or scan models

The UI does not call individual Atlas rules directly.

## Duplicate Identity

Duplicate and 1G1R scan groups are derived from canonical system identity plus normalized game title. The same title on separate systems is not a duplicate group. System aliases that refer to the same actual platform resolve to one canonical key before grouping.

Saved scan reports store derived duplicate-group summaries. When an older report lacks system-scoped group keys, PlayBuilder repairs those derived summaries from the SQLite game catalog during load without deleting source files or unrelated settings.

## Catalog Persistence

Standard scans add or update catalog records by canonical source path. Missing files are not removed merely because a later scan targets a different folder or system. Replace Entire Catalog is an explicit advanced scan mode that clears PlayBuilder catalog records before importing the selected source folder; it never deletes original game files.

System removal is handled as catalog maintenance through `ICatalogService`. It deletes selected system records from PlayBuilder's SQLite catalog only and relies on normal collection-game cascade behavior to remove saved collection links.

Saved collections store reusable system scope in `Collection.RuleJson` so 1G1R, Favorites, and future collection types can share the same selected-system model without a database migration.

Collection Builder uses `SystemSelectionState` and `SystemIdentity` to keep system search, alias matching, selected counts, and Select Matching behavior outside the Razor markup. System search changes visibility only; recommendation previews are recalculated when system selection changes.

Recommendation preview rows expose a stable `SelectionKey` composed from system identity, title, and selected file. This prevents duplicate filenames from producing duplicate Blazor render keys and lets Review Build preserve the exact checked recommendation plan.

Collection workflow presets and Atlas explanation translation live in application services rather than individual Razor event handlers. The UI selects collector-facing workflows and release preferences, while `CollectionWorkflowPresets` maps those choices onto existing deterministic `CollectionRuleOptions`. `AtlasExplanationTranslator` turns stored preview and decision data into interface language without calling Atlas rules from the UI.

Favorites remains separate from Library management. Library owns catalog browsing, release inspection, duplicate review, catalog entry removal, and favorite flags. Collections > Favorites uses catalog search plus explicit selected game IDs to save a playable Favorites build plan.

Future quarantine work must preserve the source path below a configured quarantine root, detect collisions, avoid overwrites, and store restoration records. A source such as `/games/Sony/PSP/Game Name.iso` should quarantine under `/quarantine/games/Sony/PSP/Game Name.iso`, not into a flat folder.

System card media categories are resolved as lightweight metadata only. Later artwork must use locally owned or permissively licensed icons and must keep the checkbox and system name usable when no icon exists.

## Migration State

`ICollectionRuleService` is implemented by `AtlasCollectionRuleService`. The former `CollectionRuleService` remains available by concrete type so Milestone 3 can generate controlled comparison reports before deletion or archival.
