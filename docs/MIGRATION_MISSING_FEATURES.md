# Migration: Missing Features (Old Frontend → frontend-next)

This document tracks feature gaps between the legacy Vite/React frontend and the new Next.js frontend (frontend-next). Use for future implementation planning.

## Auth & Setup

| Feature | Old | New | Notes |
|---------|-----|-----|-------|
| User registration | `/register` (RegisterForm) | None | API supports it; no public registration UI |
| Setup: Email settings | Dedicated EmailSettingsStep (SMTP) | Absent | Old allows configure/skip; new has Security + Site essentials only |
| Setup: Moderation step | ModerationSettingsStep | Absent | Links to admin moderation; new setup lacks this step |
| 2FA in setup | TwoFactorAuthStep | In SecurityStep | New has it; verify UX parity |

## Dashboard

| Feature | Old | New | Notes |
|---------|-----|-----|-------|
| Featured tab | Tab with `isFeatured` filter | None | Only "Recent" section |
| Popular tab | Tab with download-count filter | None | Popular models by downloads |
| Federated-only filter | `showFederatedOnly` toggle | None | Filter for federated content |
| Inline search + filters | SearchBar, SearchResults, SearchType | SearchCommand (⌘K), /search | Different UX; verify type filters |
| Layout controls | LayoutControls, LayoutToggle | Unclear | Verify layout switching |

## Models

| Feature | Old | New | Notes |
|---------|-----|-----|-------|
| PrintSettingsEditor | Full editor with presets | BillOfMaterialsManager | Verify print settings parity (layer height, infill, temps, etc.) |
| VersionEditor | With PrintSettings tab | VersionEditor exists | Confirm print settings in versions |
| MiniModelViewer | Compact 3D preview | ModelViewer | Check for mini/compact variant |

## Admin Panel

| Feature | Old | New | Notes |
|---------|-----|-----|-------|
| Tokens tab | TokensTab | None | Admin API tokens management |
| FontAwesome tab | FontAwesomeTab | None | Admin FontAwesome icon config |
| Admin Models tab | File Types, Categories, Upload, Config, Moderation actions | ModelsTab | Verify sub-sections and behavior |
| `/admin/moderation-settings` | Linked from setup | Unclear | May live under System or Auth |

## Moderation

| Feature | Old | New | Notes |
|---------|-----|-----|-------|
| Pending models workflow | `/moderation` – approve/reject queue | Redirects to Reports | No dedicated pending-model approval UI |
| ModeratorEditForm | Edit pending model before approve | None | Inline edit for moderation |
| ModerationDashboard | `/moderation-dashboard` overview | Reports as default | Analytics/overview may differ |

## User & Profile

| Feature | Old | New | Notes |
|---------|-----|-----|-------|
| AvatarRegeneration | Regenerate avatar | Unclear | Check settings/profile |
| Profile likes tab | User likes | ProfileModelsTab, ProfileCollectionsTab | Verify likes support |
| `/profile` (current user) | Redirect or same as `/profile/:id` | User menu "My Profile" → `/profile` | Confirm routing for "my profile" |

## Plugins & Extensions

| Feature | Old | New | Notes |
|---------|-----|-----|-------|
| Comments plugin | EnhancedCommentsWidget | Core plugin only | Check if comments are plugin or core |
| Printing metadata | PrintingMetadataDisplay/Form | Core? | Verify model detail printing metadata |
| Discord OAuth | DiscordLoginButton, DiscordUserProfile | None | OAuth integration |
| Dark theme plugin | DarkThemeProvider | Built-in dark | Theme handling differs |

## Misc

| Feature | Old | New | Notes |
|---------|-----|-----|-------|
| Redux + persist | authSlice, roleSlice, fileTypeSettings | AuthContext | State management change; ensure auth persistence works |
| MUI theme | liquidGlassTheme | shadcn/Tailwind | Theming differs |
| Recharts | Admin dashboard charts | StatCard, etc. | Verify admin dashboard charts |
