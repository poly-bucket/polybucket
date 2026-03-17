# Plugin Architecture Decisions

**Last Updated:** 2025-02-27

**Do not change these decisions without team review.** This document exists to hold us accountable and prevent scope creep or inconsistent implementation.

---

## Recorded Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| **v1 Scope** | Option A: Minimal (typed contributions + refactors only) | Fastest path to extensibility; no backend for settings, no asset loading. |
| **Loading Model** | Build-time only | Plugins are npm packages imported in `config.ts`. No runtime discovery or marketplace install in v1. |
| **Theme Support** | `layoutProviders` in v1 | Enables theme plugins (e.g. DarkThemeProvider) with minimal code; small, contained addition. |
| **Manifest Alignment** | Define contract now | `polybucket-plugin.json` and `PluginDefinition` must map cleanly so CLI validation and marketplace packaging work. |
| **Deferred to v2** | Settings, assets, compatibility, permissions, runtime loading | Reduces v1 scope; can add incrementally. |

---

## v1 In Scope

- Typed contribution interfaces: `navItems`, `userMenuItems`, `settingsNavItems`, `dashboardSections`, `modelDetailTabs`, `modelSidebarCards`, `profileTabs`, `routes`, `layoutProviders`
- Static plugin registration via `config.ts`
- Module-scope registry with extension hooks
- Error-boundary isolation around plugin contributions
- `layoutProviders` for theme and layout wrapping (e.g. DarkThemeProvider)
- Manifest contract between `polybucket-plugin.json` and `PluginDefinition` (see [PLUGIN_MANIFEST_CONTRACT.md](PLUGIN_MANIFEST_CONTRACT.md))

## v1 Out of Scope (Deferred to v2)

- Plugin settings schema and backend storage
- Asset loading (CSS, JS, images) at runtime
- Compatibility version checks (minVersion, maxVersion)
- Permission model
- Runtime plugin discovery or marketplace install
