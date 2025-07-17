namespace PolyBucket.Api.Features.ACL.Domain
{
    public static class PermissionConstants
    {
        // System Administration
        public const string ADMIN_SYSTEM_SETTINGS = "admin.system.settings";
        public const string ADMIN_VIEW_AUDIT_LOGS = "admin.view.audit_logs";
        public const string ADMIN_MANAGE_ROLES = "admin.manage.roles";
        public const string ADMIN_MANAGE_PERMISSIONS = "admin.manage.permissions";
        public const string ADMIN_MANAGE_USERS = "admin.manage.users";
        public const string ADMIN_DELETE_ANY_USER = "admin.delete.any_user";
        public const string ADMIN_BAN_USERS = "admin.ban.users";
        public const string ADMIN_VIEW_USER_DETAILS = "admin.view.user_details";

        // User Management
        public const string USER_VIEW_PROFILE = "user.view.profile";
        public const string USER_EDIT_PROFILE = "user.edit.profile";
        public const string USER_DELETE_ACCOUNT = "user.delete.account";
        public const string USER_CHANGE_PASSWORD = "user.change.password";
        public const string USER_MANAGE_SETTINGS = "user.manage.settings";
        public const string USER_VIEW_PRIVATE_MODELS = "user.view.private_models";

        // Model Management
        public const string MODEL_CREATE = "model.create";
        public const string MODEL_VIEW_PUBLIC = "model.view.public";
        public const string MODEL_VIEW_PRIVATE = "model.view.private";
        public const string MODEL_EDIT_OWN = "model.edit.own";
        public const string MODEL_EDIT_ANY = "model.edit.any";
        public const string MODEL_DELETE_OWN = "model.delete.own";
        public const string MODEL_DELETE_ANY = "model.delete.any";
        public const string MODEL_UPLOAD_FILES = "model.upload.files";
        public const string MODEL_DOWNLOAD = "model.download";
        public const string MODEL_LIKE = "model.like";
        public const string MODEL_FEATURE = "model.feature";
        public const string MODEL_VIEW_ANALYTICS = "model.view.analytics";

        // Moderation
        public const string MODERATION_VIEW_QUEUE = "moderation.view.queue";
        public const string MODERATION_APPROVE_MODELS = "moderation.approve.models";
        public const string MODERATION_REJECT_MODELS = "moderation.reject.models";
        public const string MODERATION_EDIT_MODELS = "moderation.edit.models";
        public const string MODERATION_FLAG_CONTENT = "moderation.flag.content";
        public const string MODERATION_VIEW_REPORTS = "moderation.view.reports";
        public const string MODERATION_HANDLE_REPORTS = "moderation.handle.reports";
        public const string MODERATION_MODERATE_COMMENTS = "moderation.moderate.comments";
        public const string MODERATION_MODERATE_USERS = "moderation.moderate.users";
        public const string MODERATION_VIEW_AUDIT_LOG = "moderation.view.audit_log";

        // Collections
        public const string COLLECTION_CREATE = "collection.create";
        public const string COLLECTION_VIEW_PUBLIC = "collection.view.public";
        public const string COLLECTION_VIEW_PRIVATE = "collection.view.private";
        public const string COLLECTION_EDIT_OWN = "collection.edit.own";
        public const string COLLECTION_EDIT_ANY = "collection.edit.any";
        public const string COLLECTION_DELETE_OWN = "collection.delete.own";
        public const string COLLECTION_DELETE_ANY = "collection.delete.any";
        public const string COLLECTION_ADD_MODELS = "collection.add.models";
        public const string COLLECTION_REMOVE_MODELS = "collection.remove.models";

        // Comments
        public const string COMMENT_CREATE = "comment.create";
        public const string COMMENT_VIEW = "comment.view";
        public const string COMMENT_EDIT_OWN = "comment.edit.own";
        public const string COMMENT_EDIT_ANY = "comment.edit.any";
        public const string COMMENT_DELETE_OWN = "comment.delete.own";
        public const string COMMENT_DELETE_ANY = "comment.delete.any";
        public const string COMMENT_REPORT = "comment.report";

        // Reports
        public const string REPORT_CREATE = "report.create";
        public const string REPORT_VIEW_OWN = "report.view.own";
        public const string REPORT_VIEW_ALL = "report.view.all";
        public const string REPORT_HANDLE = "report.handle";
        public const string REPORT_DELETE = "report.delete";

        // Plugins
        public const string PLUGIN_INSTALL = "plugin.install";
        public const string PLUGIN_MANAGE = "plugin.manage";
        public const string PLUGIN_CONFIGURE = "plugin.configure";
        public const string PLUGIN_VIEW_LOGS = "plugin.view.logs";

        // API Access
        public const string API_READ_ACCESS = "api.read.access";
        public const string API_WRITE_ACCESS = "api.write.access";
        public const string API_ADMIN_ACCESS = "api.admin.access";
        public const string API_RATE_LIMIT_OVERRIDE = "api.rate_limit.override";

        // Storage & Files
        public const string STORAGE_UPLOAD = "storage.upload";
        public const string STORAGE_DELETE_OWN = "storage.delete.own";
        public const string STORAGE_DELETE_ANY = "storage.delete.any";
        public const string STORAGE_VIEW_USAGE = "storage.view.usage";
        public const string STORAGE_MANAGE_QUOTAS = "storage.manage.quotas";

        // Categories for organization
        public static class Categories
        {
            public const string ADMINISTRATION = "Administration";
            public const string USER_MANAGEMENT = "User Management";
            public const string MODEL_MANAGEMENT = "Model Management";
            public const string MODERATION = "Moderation";
            public const string COLLECTIONS = "Collections";
            public const string COMMENTS = "Comments";
            public const string REPORTS = "Reports";
            public const string PLUGINS = "Plugins";
            public const string API_ACCESS = "API Access";
            public const string STORAGE = "Storage & Files";
        }

        // Default role permissions
        public static class DefaultRoles
        {
            public static readonly Dictionary<string, string[]> USER = new()
            {
                [Categories.USER_MANAGEMENT] = new[]
                {
                    USER_VIEW_PROFILE,
                    USER_EDIT_PROFILE,
                    USER_DELETE_ACCOUNT,
                    USER_CHANGE_PASSWORD,
                    USER_MANAGE_SETTINGS
                },
                [Categories.MODEL_MANAGEMENT] = new[]
                {
                    MODEL_CREATE,
                    MODEL_VIEW_PUBLIC,
                    MODEL_EDIT_OWN,
                    MODEL_DELETE_OWN,
                    MODEL_UPLOAD_FILES,
                    MODEL_DOWNLOAD,
                    MODEL_LIKE
                },
                [Categories.COLLECTIONS] = new[]
                {
                    COLLECTION_CREATE,
                    COLLECTION_VIEW_PUBLIC,
                    COLLECTION_EDIT_OWN,
                    COLLECTION_DELETE_OWN,
                    COLLECTION_ADD_MODELS,
                    COLLECTION_REMOVE_MODELS
                },
                [Categories.COMMENTS] = new[]
                {
                    COMMENT_CREATE,
                    COMMENT_VIEW,
                    COMMENT_EDIT_OWN,
                    COMMENT_DELETE_OWN,
                    COMMENT_REPORT
                },
                [Categories.REPORTS] = new[]
                {
                    REPORT_CREATE,
                    REPORT_VIEW_OWN
                },
                [Categories.API_ACCESS] = new[]
                {
                    API_READ_ACCESS
                },
                [Categories.STORAGE] = new[]
                {
                    STORAGE_UPLOAD,
                    STORAGE_DELETE_OWN,
                    STORAGE_VIEW_USAGE
                }
            };

            public static readonly Dictionary<string, string[]> MODERATOR = new()
            {
                [Categories.MODERATION] = new[]
                {
                    MODERATION_VIEW_QUEUE,
                    MODERATION_APPROVE_MODELS,
                    MODERATION_REJECT_MODELS,
                    MODERATION_EDIT_MODELS,
                    MODERATION_FLAG_CONTENT,
                    MODERATION_VIEW_REPORTS,
                    MODERATION_HANDLE_REPORTS,
                    MODERATION_MODERATE_COMMENTS,
                    MODERATION_VIEW_AUDIT_LOG
                },
                [Categories.MODEL_MANAGEMENT] = new[]
                {
                    MODEL_VIEW_PRIVATE,
                    MODEL_EDIT_ANY,
                    MODEL_FEATURE
                },
                [Categories.COMMENTS] = new[]
                {
                    COMMENT_EDIT_ANY,
                    COMMENT_DELETE_ANY
                },
                [Categories.REPORTS] = new[]
                {
                    REPORT_VIEW_ALL,
                    REPORT_HANDLE
                },
                [Categories.USER_MANAGEMENT] = new[]
                {
                    USER_VIEW_PRIVATE_MODELS
                }
            };

            public static readonly Dictionary<string, string[]> ADMIN = new()
            {
                [Categories.ADMINISTRATION] = new[]
                {
                    ADMIN_SYSTEM_SETTINGS,
                    ADMIN_VIEW_AUDIT_LOGS,
                    ADMIN_MANAGE_ROLES,
                    ADMIN_MANAGE_PERMISSIONS,
                    ADMIN_MANAGE_USERS,
                    ADMIN_DELETE_ANY_USER,
                    ADMIN_BAN_USERS,
                    ADMIN_VIEW_USER_DETAILS
                },
                [Categories.MODEL_MANAGEMENT] = new[]
                {
                    MODEL_DELETE_ANY,
                    MODEL_VIEW_ANALYTICS
                },
                [Categories.COLLECTIONS] = new[]
                {
                    COLLECTION_VIEW_PRIVATE,
                    COLLECTION_EDIT_ANY,
                    COLLECTION_DELETE_ANY
                },
                [Categories.REPORTS] = new[]
                {
                    REPORT_DELETE
                },
                [Categories.PLUGINS] = new[]
                {
                    PLUGIN_INSTALL,
                    PLUGIN_MANAGE,
                    PLUGIN_CONFIGURE,
                    PLUGIN_VIEW_LOGS
                },
                [Categories.API_ACCESS] = new[]
                {
                    API_WRITE_ACCESS,
                    API_ADMIN_ACCESS,
                    API_RATE_LIMIT_OVERRIDE
                },
                [Categories.STORAGE] = new[]
                {
                    STORAGE_DELETE_ANY,
                    STORAGE_MANAGE_QUOTAS
                },
                [Categories.MODERATION] = new[]
                {
                    MODERATION_MODERATE_USERS
                }
            };
        }
    }
} 