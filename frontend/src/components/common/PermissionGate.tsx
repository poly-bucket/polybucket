import React, { ReactNode } from 'react';
import { usePermissions, DEFAULT_ROLE_HIERARCHY } from '../../hooks/usePermissions';
import { Alert, CircularProgress, Box } from '@mui/material';

interface PermissionGateProps {
  children: ReactNode;
  fallback?: ReactNode;
  loadingFallback?: ReactNode;
  
  // Permission-based access
  permission?: string;
  permissions?: string[];
  requireAll?: boolean; // true = all permissions required, false = any permission required
  
  // Role-based access
  role?: string;
  roles?: string[];
  minimumRole?: string;
  roleHierarchy?: Record<string, number>;
  
  // Custom validation
  customCheck?: (permissions: any) => boolean;
  
  // Admin/Moderator shortcuts
  adminOnly?: boolean;
  moderatorOnly?: boolean;
  
  // Resource ownership (for "own" permissions)
  resourceOwnerId?: string;
  currentUserId?: string;
  
  // Error handling
  showErrorMessage?: boolean;
  errorMessage?: string;
}

const PermissionGate: React.FC<PermissionGateProps> = ({
  children,
  fallback = null,
  loadingFallback,
  permission,
  permissions,
  requireAll = true,
  role,
  roles,
  minimumRole,
  roleHierarchy = DEFAULT_ROLE_HIERARCHY,
  customCheck,
  adminOnly,
  moderatorOnly,
  resourceOwnerId,
  currentUserId,
  showErrorMessage = false,
  errorMessage = "You don't have permission to access this content."
}) => {
  const {
    permissions: userPermissions,
    loading,
    error,
    hasPermission,
    hasAnyPermission,
    hasAllPermissions,
    hasRole,
    hasMinimumRole,
    isAdmin,
    isModerator
  } = usePermissions();

  // Show loading state
  if (loading) {
    return (
      <div>
        {loadingFallback || (
          <Box sx={{ display: 'flex', justifyContent: 'center', p: 2 }}>
            <CircularProgress size={24} />
          </Box>
        )}
      </div>
    );
  }

  // Show error state
  if (error) {
    return showErrorMessage ? (
      <Alert severity="error" sx={{ mt: 1 }}>
        {error}
      </Alert>
    ) : (
      <div>{fallback}</div>
    );
  }

  // No permissions loaded
  if (!userPermissions) {
    return <div>{fallback}</div>;
  }

  let hasAccess = true;

  // Admin-only check
  if (adminOnly) {
    hasAccess = isAdmin();
  }
  // Moderator-only check (includes admins)
  else if (moderatorOnly) {
    hasAccess = isModerator();
  }
  // Permission-based checks
  else if (permission) {
    hasAccess = hasPermission(permission);
  }
  else if (permissions && permissions.length > 0) {
    hasAccess = requireAll 
      ? hasAllPermissions(permissions)
      : hasAnyPermission(permissions);
  }
  // Role-based checks
  else if (role) {
    hasAccess = hasRole(role);
  }
  else if (roles && roles.length > 0) {
    hasAccess = roles.some(r => hasRole(r));
  }
  else if (minimumRole) {
    hasAccess = hasMinimumRole(minimumRole, roleHierarchy);
  }
  // Custom validation
  else if (customCheck) {
    hasAccess = customCheck(userPermissions);
  }

  // Resource ownership check for "own" permissions
  if (hasAccess && resourceOwnerId && currentUserId) {
    // For "own" permissions, also check ownership
    if (permission?.includes('.own') || permissions?.some(p => p.includes('.own'))) {
      hasAccess = hasAccess && (resourceOwnerId === currentUserId || isAdmin());
    }
  }

  if (!hasAccess) {
    return showErrorMessage ? (
      <Alert severity="warning" sx={{ mt: 1 }}>
        {errorMessage}
      </Alert>
    ) : (
      <div>{fallback}</div>
    );
  }

  return <>{children}</>;
};

export default PermissionGate;

// Convenience components for common use cases
export const AdminOnly: React.FC<{ children: ReactNode; fallback?: ReactNode }> = ({ children, fallback }) => (
  <PermissionGate adminOnly={true} fallback={fallback}>
    {children}
  </PermissionGate>
);

export const ModeratorOnly: React.FC<{ children: ReactNode; fallback?: ReactNode }> = ({ children, fallback }) => (
  <PermissionGate moderatorOnly={true} fallback={fallback}>
    {children}
  </PermissionGate>
);

export const RequirePermission: React.FC<{ 
  permission: string; 
  children: ReactNode; 
  fallback?: ReactNode;
  resourceOwnerId?: string;
  currentUserId?: string;
}> = ({ permission, children, fallback, resourceOwnerId, currentUserId }) => (
  <PermissionGate 
    permission={permission} 
    fallback={fallback}
    resourceOwnerId={resourceOwnerId}
    currentUserId={currentUserId}
  >
    {children}
  </PermissionGate>
);

export const RequireAnyPermission: React.FC<{ 
  permissions: string[]; 
  children: ReactNode; 
  fallback?: ReactNode;
}> = ({ permissions, children, fallback }) => (
  <PermissionGate permissions={permissions} requireAll={false} fallback={fallback}>
    {children}
  </PermissionGate>
);

export const RequireAllPermissions: React.FC<{ 
  permissions: string[]; 
  children: ReactNode; 
  fallback?: ReactNode;
}> = ({ permissions, children, fallback }) => (
  <PermissionGate permissions={permissions} requireAll={true} fallback={fallback}>
    {children}
  </PermissionGate>
);

export const RequireRole: React.FC<{ 
  role: string; 
  children: ReactNode; 
  fallback?: ReactNode;
}> = ({ role, children, fallback }) => (
  <PermissionGate role={role} fallback={fallback}>
    {children}
  </PermissionGate>
);

export const RequireMinimumRole: React.FC<{ 
  minimumRole: string; 
  children: ReactNode; 
  fallback?: ReactNode;
  roleHierarchy?: Record<string, number>;
}> = ({ minimumRole, children, fallback, roleHierarchy }) => (
  <PermissionGate minimumRole={minimumRole} fallback={fallback} roleHierarchy={roleHierarchy}>
    {children}
  </PermissionGate>
); 