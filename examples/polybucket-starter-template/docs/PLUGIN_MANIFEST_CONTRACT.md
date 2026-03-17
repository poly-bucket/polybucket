# Plugin Manifest Contract

This document defines the contract between `polybucket-plugin.json` (JSON manifest for CLI/marketplace) and the frontend `PluginDefinition` (TypeScript).

---

## Field Mapping

| polybucket-plugin.json | PluginDefinition / Contributions | Notes |
|------------------------|----------------------------------|-------|
| `id`, `name`, `version`, `author` | `PluginDefinition.id`, `.name`, `.version` | Direct map. `author` metadata only. |
| `type` | `PluginDefinition.type` | Optional. Values: `"theme"`, `"extension"`, `"tool"`. |
| `hooks` | `layoutProviders` | `hooks` with `name: "theme-override"` or `"layout-provider"` maps to `layoutProviders: [{ component, order: priority }]`. Component names resolve to React components at build time. |
| `contributions.navItems` | `navItems` | Same shape; order field. |
| `contributions.userMenuItems` | `userMenuItems` | Same shape. |
| `contributions.settingsNavItems` | `settingsNavItems` | Same shape; matches existing `SettingsNavItem`. |
| `contributions.dashboardSections` | `dashboardSections` | `{ id, order, component }`. |
| `contributions.modelDetailTabs` | `modelDetailTabs` | Same shape. |
| `contributions.modelSidebarCards` | `modelSidebarCards` | Same shape. |
| `contributions.profileTabs` | `profileTabs` | Same shape. |
| `contributions.routes` | `routes` | `path` is relative to `/ext/`. |
| `contributions.layoutProviders` | `layoutProviders` | Canonical frontend field. Maps from `hooks` in JSON. |
| `settings` | Deferred v2 | Not in v1. |
| `assets` | Deferred v2 | Not in v1. |
| `compatibility` | Deferred v2 | Not in v1. |
| `permissions` | Deferred v2 | Not in v1. |

---

## Build-Time Flow

```
polybucket-plugin.json (optional)  -->  Plugin authors can provide JSON for CLI/marketplace
         |
         v
PluginDefinition (TypeScript)     -->  Canonical form; config.ts imports and registers
         |
         v
Registry                           -->  Frontend consumes via hooks
```

Plugins can be authored as:

- **TypeScript only**: Export `PluginDefinition` from index; import in host `config.ts`.
- **Hybrid**: `polybucket-plugin.json` for CLI/validation; package also exports `PluginDefinition` for frontend. `validate-plugin` checks JSON; frontend uses TS export.

---

## v1 Contract Summary

- Frontend accepts `PluginDefinition` (TypeScript) with: `navItems`, `userMenuItems`, `settingsNavItems`, `dashboardSections`, `modelDetailTabs`, `modelSidebarCards`, `profileTabs`, `routes`, `layoutProviders`.
- `polybucket-plugin.json` `hooks` with `theme-override` or `layout-provider` map to `layoutProviders`. CLI validation can check for these hooks.
- `settings`, `assets`, `compatibility`, `permissions` in JSON are valid but ignored by frontend in v1.
