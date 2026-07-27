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

## Migration State

`ICollectionRuleService` is implemented by `AtlasCollectionRuleService`. The former `CollectionRuleService` remains available by concrete type so Milestone 3 can generate controlled comparison reports before deletion or archival.
