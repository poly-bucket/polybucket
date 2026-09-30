# Endpoint test coverage

Legend: **Behavioral** = Moq/xUnit controller or service tests with AAA and meaningful assertions. **Smoke** = attribute/route/reflection-only checks. **Integration** = `BaseIntegrationTest` / HTTP against test host. **—** = no dedicated test class yet.

| Feature slice | Controller / action | Behavioral | Smoke | Integration |
|---------------|----------------------|:----------:|:-----:|:-------------:|
| **ACL** | RoleManagement | ✓ | | |
| | UserPermissionManagement | ✓ | | |
| **Admin** | GetModerationAuditLogs | ✓ | | |
| **Authentication** | Login, Register, Refresh, OAuth, Me, Verify/Forgot/Reset password, Email change, Danger zone | ✓ (most) | ✓ (some) | ✓ |
| | Account (export, delete, sessions) | ✓ | | |
| **Categories** | Create, Update, Delete, GetCategories | ✓ | | ✓ |
| **Collections** | Create, Update, Delete, Add/Remove model, Access, GetById, GetUser, Favorite | ✓ | ✓ | ✓ |
| **Comments** | CreateComment | ✓ | | |
| | AddComment (legacy) | ✓ | | |
| | GetComment, GetCommentsForTarget, Update, Delete | ✓ | | |
| | Like/Dislike/Remove reactions | ✓ | | |
| | ModerateComment, ModerateAllUserComments, ReportComment | ✓ | | |
| | GetCommentStatistics, GetUserCommentStatistics, GetModeratedComments | ✓ | | |
| | DeleteAllCommentsForTarget | ✓ | | |
| | CommentsController (aggregate smoke) | | ✓ | |
| **Email** | Get/Update settings, Preview template, Test configuration, Outbox | ✓ | | |
| **Federation** | Instances, catalog, health, models, token exchange | | ✓ | ✓ |
| **Files** | StreamFile | ✓ | ✓ | |
| | supported extensions | | ✓ | |
| **Filaments** | CRUD, GetAll, GetById | ✓ | | |
| **ModelModeration** | ApproveModel | ✓ | ✓ (aggregate) | ✓ |
| | RejectModel | ✓ | ✓ | ✓ |
| | GetModelsAwaitingModeration | ✓ | ✓ | ✓ |
| | GetModerationSettings | ✓ | ✓ | |
| | UpdateModerationSettings | ✓ | ✓ | |
| | ModeratorEditModel (GET/PUT) | ✓ | ✓ | ✓ |
| **Models** | GetModels | ✓ | ✓ | ✓ |
| | GetModelById | ✓ | ✓ | ✓ |
| | DownloadModel | ✓ | ✓ | |
| | DeleteAllModels | ✓ | ✓ | |
| | GetModelPreview | ✓ | ✓ | |
| | GenerateModelPreview | ✓ | ✓ | ✓ |
| | GenerateCustomThumbnail | ✓ | ✓ | |
| | Create/Update/Delete model & versions | ✓ (many) | ✓ | ✓ |
| | Like, tags, categories | ✓ | ✓ | ✓ |
| | GetModelVersions, GetModelByUserId | ✓ | ✓ | |
| **Notifications** | GetNotifications | ✓ | | ✓ |
| | MarkNotificationRead | ✓ | | |
| | MarkAllNotificationsRead | ✓ | | |
| | GetUnreadNotificationCount | ✓ | | ✓ |
| **Plugins** | GetPlugins | ✓ | ✓ | |
| | ReloadPlugins | ✓ | ✓ | |
| | GetPluginDetails (details, overview, hooks) | ✓ | | |
| | UpdatePluginSettings (settings, status) | ✓ | | |
| | OAuth, Theme, Metadata, Marketplace, PluginManagement HTTP | ✓ | | |
| **Printers** | GetPrinters | ✓ | ✓ | |
| **Reports** | GetReport | ✓ | ✓ | |
| | SubmitReport | ✓ | ✓ | |
| | GetAllReports | ✓ | ✓ | |
| | GetUnresolvedReports | ✓ | ✓ | |
| | GetReportsForTarget | ✓ | ✓ | |
| | ResolveReport | ✓ | ✓ | |
| | GetReportAnalytics (analytics, tops, trends, moderator activity) | ✓ | ✓ | |
| **Search** | Search | ✓ (`SearchControllerUnitTests`) | | ✓ (`SearchControllerTests`) |
| **SystemSettings** | SystemSetup (status, site-settings, complete) | ✓ | ✓ | |
| | Get/Update model configuration | ✓ | ✓ | |
| | Get/Update site model settings | ✓ | ✓ | |
| | Get/Update file settings | ✓ | ✓ | |
| | AuthenticationSettings | ✓ | ✓ | |
| | ThemeSettings, FontAwesome, Token, ExtensibleTheme | ✓ (FontAwesome, ExtensibleTheme, Theme, Token getters) | ✓ | |
| **ThemeManagement** | GetActiveTheme | ✓ | | |
| | GetThemes | ✓ | | |
| | CreateTheme | ✓ | | |
| | SetActiveTheme | ✓ | | |
| **Users** | BanUser | ✓ | | |
| | UnbanUser | ✓ | | |
| | BanUserService / UnbanUserService | ✓ | | |
| | GetUserById | ✓ | | |
| | GetUserSettings / UpdateUserSettings | ✓ | | |
| | RegenerateAvatar | ✓ | | |
| | CreateUser, GetUsers, GetUserProfile, UpdateUserProfile | ✓ | ✓ | |
| | GetBannedUsers, MarkEmailVerified, GeneratePasswordResetLink | ✓ | ✓ | |
| | GetUserModels, GetUserLikedModels, GetPublicUserCollections, GetUserComments, GetUserPrinters | ✓ | ✓ | |
| | UsersController (aggregate smoke) | | ✓ | |

## Phase mapping (test rollout)

| Phase | Area | Status |
|------:|------|--------|
| 11 | ModelModeration (6 slices) | Behavioral per slice |
| 12 | Models (listed actions) | Behavioral expanded |
| 13 | Notifications (4 controllers) | Behavioral |
| 14 | Plugins (core CRUD/management) | Behavioral split from aggregate smoke |
| 15 | Printers | Behavioral + smoke |
| 16 | Reports (7 slices) | Behavioral per slice |
| 17 | Search | Unit Moq (`SearchControllerUnitTests`) + integration (`SearchControllerTests`) |
| 18 | SystemSettings (primary HTTP controllers) | Behavioral Moq |
| 19 | ThemeManagement (4 controllers) | Behavioral |
| 20 | Users (ban/unban + settings + avatar) | Behavioral + service tests |

## Notes

- Shared helpers: `ControllerTestExtensions`, `MediatorTestExtensions`, `NotificationControllerTestContext`, `PluginManagerTestHelper`.
- Prefer **behavioral** Moq tests for controller branching; keep **integration** tests for DB/search/plugins that need a real host.
- Update this table when adding slices or changing test type.
