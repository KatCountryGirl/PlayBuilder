# PlayBuilder Architecture

## Application

PlayBuilder is a .NET 10 Blazor Server application using SQLite through Entity Framework Core.

## Atlas Boundary

Atlas is isolated under `Services/Atlas` and has no dependency on Blazor UI components. The collection preview adapter lives in `Services/AtlasCollectionRuleService.cs` and translates Atlas decisions into UI-facing preview models.

## Dependency Flow

Blazor Components → Application service interfaces → Atlas adapter / collection services → Atlas engine and parsing → data or scan models

The UI does not call individual Atlas rules directly.

## Migration State

`ICollectionRuleService` is implemented by `AtlasCollectionRuleService`. The former `CollectionRuleService` remains available by concrete type so Milestone 3 can generate controlled comparison reports before deletion or archival.
