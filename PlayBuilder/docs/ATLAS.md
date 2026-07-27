# Atlas

Atlas is PlayBuilder's deterministic, explainable game-file selection rule engine. It is not a weighted scoring system.

## Pipeline

Filename → FilenameTokenizer → FilenameToken collection → FilenameMetadataParser → FilenameMetadata → AtlasCandidateFactory → AtlasDecisionEngine

## Decision Model

Atlas compares two candidates using ordered rules. The first rule that distinguishes the candidates decides their order. No points are accumulated and later rules cannot secretly outweigh an earlier rule.

Current order:

1. Dump quality
2. Language priority
3. Region priority
4. Standard versus special release
5. Revision
6. Version
7. Stable filename tie-breaker

English-only mode removes candidates that cannot be identified as English before comparison.

Atlas receives candidates from scan groups that are already scoped by canonical system and normalized game title. Atlas still compares candidates deterministically within each group; it does not merge matching titles across different systems.

Collection Builder may filter which systems are sent to Atlas, but those filters do not change Atlas behavior or rule order. The UI stores stable system-aware selection keys for review and persistence; Atlas remains the deterministic comparison engine for each candidate group.

Distinct discs in a multi-disc set are grouped as required components of the same game, not as duplicate release alternatives. Duplicate selection can still occur between alternate copies of the same disc position.

## Explanations

The final decision contains structured reasons describing which rules favored the winner over the runner-up. The Collection Builder displays these reasons without changing the source archive.

## Migration

The live `ICollectionRuleService` now resolves to `AtlasCollectionRuleService`. The legacy `CollectionRuleService` remains registered as a concrete service for controlled comparison during the next milestone.
