import React from 'react';
import { Navigate } from 'react-router-dom';
import { useAppSelector } from '../utils/hooks';

interface ProtectedRouteProps {
  children: React.ReactNode;
  requiredRole?: string;
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ children, requiredRole }) => {
  const { user, isFirstRun, setupStatus } = useAppSelector((state) => state.auth);

  // Always prioritize first run setup
  if (isFirstRun) {
    return <Navigate to="/admin-setup" replace />;
  }

  // Check if roles need to be configured (when admin exists but roles don't)
  if (setupStatus && setupStatus.isAdminConfigured && !setupStatus.isRoleConfigured) {
    return <Navigate to="/custom-role-setup" replace />;
  }

  // If no user, redirect to login
  if (!user) {
    return <Navigate to="/login" replace />;
  }

  // Check for required role if specified
  if (requiredRole === 'admin' && !user.roles?.includes('Admin')) {
    // Redirect to dashboard or an unauthorized page if the role doesn't match
    console.warn(`Access denied: User role does not include 'Admin', required for this route.`);
    return <Navigate to="/dashboard" replace />;
  }

  // Otherwise render the children
  return <>{children}</>;
};

export default ProtectedRoute; 