# PlayBuilder Repository Rules

These rules apply to every implementation, refactoring, testing, cleanup, and UX task in this repository.

## Product Principle

PlayBuilder must be:

> Simple by default. Powerful when needed.

A first-time user should be able to complete the normal workflow without understanding ROM-management terminology, Atlas internals, database behavior, or advanced selection rules.

Complexity belongs behind clearly labeled advanced or custom options.

## Grandma Test

Every user-facing label, explanation, warning, and action must pass this test:

> Could a non-technical first-time user understand what this means and what will happen next?

When plain language and technical language compete, use plain language.

Do not expose internal terms merely because they match the implementation.

## Five-Second Rule

Within five seconds of opening a page, a first-time user should understand:

1. Where they are
2. What the page does
3. What they should do next

Each page should have one clear primary purpose and one obvious next action.

## Collection Wizard

The user-facing collection workflow is called:

> Collection Wizard

The Collection Wizard should guide the user step by step.

Its primary collection paths are:

### 1G1R

Goal:

> Build one unique version of every selected game.

Keep the normal 1G1R workflow simple. Atlas should make the detailed version-selection decisions automatically.

Advanced region, revision, language, hack, translation, and variant controls must not dominate the default workflow.

### Favorites

Goal:

> Build a collection around what the user enjoys.

Favorites may guide the user through choices such as:

- genres
- franchises
- characters
- years
- multiplayer
- family-friendly games
- personal favorites
- other approachable interests

Use friendly language rather than database or rule-engine terminology.

### Custom

Goal:

> Provide the full advanced rule and filtering system when the user needs complete control.

Detailed Atlas rules, priorities, filters, overrides, and technical controls belong here.

Do not move Custom complexity into the default 1G1R or Favorites paths.

## Build Preview

Build Preview must make the result immediately understandable.

Clearly separate:

- Included
- Needs Review
- Excluded

The user should be able to:

- understand which games will be built
- expand a section to inspect it
- check or uncheck individual games when appropriate
- build the collection without understanding Atlas internals

Lists should be collapsed by default unless opening one is necessary to resolve a decision.

The page should answer:

> Are you happy with what will be included?

It should not require the user to understand how the algorithm works.

## Atlas

Atlas may perform complex reasoning internally, but its explanations must be simple and reassuring.

Explain decisions in plain language.

Prefer:

> Atlas chose this version because it matches your preferred region.

Avoid:

> Selected through regional weighting and revision precedence.

Technical detail may be available through an optional explanation, but it must not dominate the default experience.

## Documentation Protection

The directory below is controlled by the Product Owner and is part of the project architecture:

```text
PlayBuilder/docs/