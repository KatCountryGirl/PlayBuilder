# PlayBuilder Documentation

Welcome to the PlayBuilder project documentation.

These documents define the product vision, user-experience philosophy, interface behavior, Atlas voice, engineering standards, technical architecture, release history, and future direction of PlayBuilder.

> **Document precedence**
>
> Read the core documents in order. Earlier documents define product intent; later documents define implementation guidance. When two documents appear to conflict, follow the earlier core document unless the Product Owner has approved a documented exception.

## Core Reading Order

1. [`architecture/00-PlayBuilder-Bible.md`](architecture/00-PlayBuilder-Bible.md) — what PlayBuilder is and why it exists
2. [`design/01-UX-Manifesto.md`](design/01-UX-Manifesto.md) — how PlayBuilder should feel
3. [`design/02-Design-System.md`](design/02-Design-System.md) — how the interface should look and behave
4. [`design/03-Atlas-Personality-Guide.md`](design/03-Atlas-Personality-Guide.md) — how Atlas should communicate
5. [`development/04-Developer-Standards.md`](development/04-Developer-Standards.md) — how PlayBuilder should be engineered and delivered
6. [`roadmap/05-Project-Roadmap.md`](roadmap/05-Project-Roadmap.md) — where the project is going

## Technical References

The [`reference`](reference/) folder contains implementation-specific documentation. These files support development but do not override the core product documents.

- [`reference/Architecture.md`](reference/Architecture.md)
- [`reference/Atlas-Engine.md`](reference/Atlas-Engine.md)
- [`reference/Changelog.md`](reference/Changelog.md)
- [`reference/Release-Notes.md`](reference/Release-Notes.md)

## Contributor Expectations

Before designing or implementing a feature:

1. Read the relevant core documents.
2. Confirm the proposed workflow supports the collector experience.
3. Preserve deterministic and explainable Atlas behavior.
4. Keep common workflows simple through progressive disclosure.
5. Update documentation alongside the implementation.

## Project Philosophy

PlayBuilder is built by collectors, for collectors.

Every design decision should reduce complexity, increase confidence, protect the source collection, and help people spend more time playing games instead of managing them.
