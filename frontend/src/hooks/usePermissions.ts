import { useState, useEffect, useCallback } from 'react';
import { useAuth } from '../context/AuthContext';
import axios from 'axios';

export interface UserPermissions {
  effectivePermissions: string[];
  role: {
    id: string;
    name: string;
    priority: number;
  };
  permissionOverrides: PermissionOverride[];
}

export interface PermissionOverride {
  id: string;
  permissionName: string;
  isGranted: boolean;
  reason?: string;
  expiresAt?: string;
  grantedByUsername?: string;
  createdAt: string;
}

export interface Permission {
  name: string;
  category: string;
  description: string;
}

export const usePermissions = () => {
  const { user } = useAuth();
  const [permissions, setPermissions] = useState<UserPermissions | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchUserPermissions = useCallback(async () => {
    if (!user?.id || !user.accessToken) {
      setPermissions(null);
      setLoading(false);
      return;
    }

    try {
      setLoading(true);
      const response = await axios.get(`/api/admin/user-permissions/me/permissions`, {
        headers: {
          Authorization: `Bearer ${user.accessToken}`
        }
      });
      
      console.log('usePermissions - API Response:', response.data);
      setPermissions(response.data);
      setError(null);
    } catch (err) {
      console.error('Error fetching user permissions:', err);
      setError('Failed to load permissions');
      setPermissions(null);
    } finally {
      setLoading(false);
    }
  }, [user?.id, user?.accessToken]);

  useEffect(() => {
    fetchUserPermissions();
  }, [fetchUserPermissions]);

  const hasPermission = useCallback((permission: string): boolean => {
    if (!permissions || !permissions.effectivePermissions) return false;
    return permissions.effectivePermissions.includes(permission);
  }, [permissions]);

  const hasAnyPermission = useCallback((permissionList: string[]): boolean => {
    if (!permissions || !permissions.effectivePermissions) return false;
    return permissionList.some(permission => permissions.effectivePermissions.includes(permission));
  }, [permissions]);

  const hasAllPermissions = useCallback((permissionList: string[]): boolean => {
    if (!permissions || !permissions.effectivePermissions) return false;
    return permissionList.every(permission => permissions.effectivePermissions.includes(permission));
  }, [permissions]);

  const hasRole = useCallback((roleName: string): boolean => {
    if (!permissions || !permissions.role) return false;
    return permissions.role.name.toLowerCase() === roleName.toLowerCase();
  }, [permissions]);

  const hasMinimumRole = useCallback((roleName: string, roleHierarchy: Record<string, number>): boolean => {
    if (!permissions || !permissions.role) return false;
    const userRolePriority = roleHierarchy[permissions.role.name.toLowerCase()] || 0;
    const requiredRolePriority = roleHierarchy[roleName.toLowerCase()] || 0;
    return userRolePriority >= requiredRolePriority;
  }, [permissions]);

  const isAdmin = useCallback((): boolean => {
    if (!permissions || !permissions.role) return false;
    return permissions.role.name.toLowerCase() === 'admin' || permissions.role.priority >= 1000;
  }, [permissions]);

  const isModerator = useCallback((): boolean => {
    if (!permissions || !permissions.role) return false;
    return isAdmin() || permissions.role.name.toLowerCase() === 'moderator';
  }, [permissions, isAdmin]);

  const canModerate = useCallback((targetUserId?: string): boolean => {
    if (!permissions) return false;
    
    // Admins can moderate anyone except other admins would need backend check
    if (isAdmin()) return true;
    
    // Check for moderation permissions
    return hasPermission('moderation.moderate.users');
  }, [permissions, hasPermission, isAdmin]);

  const refreshPermissions = useCallback(() => {
    fetchUserPermissions();
  }, [fetchUserPermissions]);

  return {
    permissions,
    loading,
    error,
    hasPermission,
    hasAnyPermission,
    hasAllPermissions,
    hasRole,
    hasMinimumRole,
    isAdmin,
    isModerator,
    canModerate,
    refreshPermissions
  };
};

// Default role hierarchy for convenience
export const DEFAULT_ROLE_HIERARCHY: Record<string, number> = {
  'user': 100,
  'moderator': 500,
  'admin': 1000
};

// Permission constants for frontend use
export const PERMISSIONS = {
  // Administration
  ADMIN_SYSTEM_SETTINGS: 'admin.system.settings',
  ADMIN_VIEW_AUDIT_LOGS: 'admin.view.audit_logs',
  ADMIN_MANAGE_ROLES: 'admin.manage.roles',
  ADMIN_MANAGE_PERMISSIONS: 'admin.manage.permissions',
  ADMIN_MANAGE_USERS: 'admin.manage.users',
  ADMIN_DELETE_ANY_USER: 'admin.delete.any_user',
  ADMIN_BAN_USERS: 'admin.ban.users',
  ADMIN_VIEW_USER_DETAILS: 'admin.view.user_details',

  // User Management
  USER_VIEW_PROFILE: 'user.view.profile',
  USER_EDIT_PROFILE: 'user.edit.profile',
  USER_DELETE_ACCOUNT: 'user.delete.account',
  USER_CHANGE_PASSWORD: 'user.change.password',
  USER_MANAGE_SETTINGS: 'user.manage.settings',
  USER_VIEW_PRIVATE_MODELS: 'user.view.private_models',

  // Model Management
  MODEL_CREATE: 'model.create',
  MODEL_VIEW_PUBLIC: 'model.view.public',
  MODEL_VIEW_PRIVATE: 'model.view.private',
  MODEL_EDIT_OWN: 'model.edit.own',
  MODEL_EDIT_ANY: 'model.edit.any',
  MODEL_DELETE_OWN: 'model.delete.own',
  MODEL_DELETE_ANY: 'model.delete.any',
  MODEL_UPLOAD_FILES: 'model.upload.files',
  MODEL_DOWNLOAD: 'model.download',
  MODEL_LIKE: 'model.like',
  MODEL_FEATURE: 'model.feature',
  MODEL_VIEW_ANALYTICS: 'model.view.analytics',

  // Moderation
  MODERATION_VIEW_QUEUE: 'moderation.view.queue',
  MODERATION_APPROVE_MODELS: 'moderation.approve.models',
  MODERATION_REJECT_MODELS: 'moderation.reject.models',
  MODERATION_EDIT_MODELS: 'moderation.edit.models',
  MODERATION_FLAG_CONTENT: 'moderation.flag.content',
  MODERATION_VIEW_REPORTS: 'moderation.view.reports',
  MODERATION_HANDLE_REPORTS: 'moderation.handle.reports',
  MODERATION_MODERATE_COMMENTS: 'moderation.moderate.comments',
  MODERATION_MODERATE_USERS: 'moderation.moderate.users',
  MODERATION_VIEW_AUDIT_LOG: 'moderation.view.audit_log',

  // Collections
  COLLECTION_CREATE: 'collection.create',
  COLLECTION_VIEW_PUBLIC: 'collection.view.public',
  COLLECTION_VIEW_PRIVATE: 'collection.view.private',
  COLLECTION_EDIT_OWN: 'collection.edit.own',
  COLLECTION_EDIT_ANY: 'collection.edit.any',
  COLLECTION_DELETE_OWN: 'collection.delete.own',
  COLLECTION_DELETE_ANY: 'collection.delete.any',
  COLLECTION_ADD_MODELS: 'collection.add.models',
  COLLECTION_REMOVE_MODELS: 'collection.remove.models',

  // Comments
  COMMENT_CREATE: 'comment.create',
  COMMENT_VIEW: 'comment.view',
  COMMENT_EDIT_OWN: 'comment.edit.own',
  COMMENT_EDIT_ANY: 'comment.edit.any',
  COMMENT_DELETE_OWN: 'comment.delete.own',
  COMMENT_DELETE_ANY: 'comment.delete.any',
  COMMENT_REPORT: 'comment.report',

  // Reports
  REPORT_CREATE: 'report.create',
  REPORT_VIEW_OWN: 'report.view.own',
  REPORT_VIEW_ALL: 'report.view.all',
  REPORT_HANDLE: 'report.handle',
  REPORT_DELETE: 'report.delete',

  // API Access
  API_READ_ACCESS: 'api.read.access',
  API_WRITE_ACCESS: 'api.write.access',
  API_ADMIN_ACCESS: 'api.admin.access',
  API_RATE_LIMIT_OVERRIDE: 'api.rate_limit.override',

  // Storage
  STORAGE_UPLOAD: 'storage.upload',
  STORAGE_DELETE_OWN: 'storage.delete.own',
  STORAGE_DELETE_ANY: 'storage.delete.any',
  STORAGE_VIEW_USAGE: 'storage.view.usage',
  STORAGE_MANAGE_QUOTAS: 'storage.manage.quotas'
} as const; 