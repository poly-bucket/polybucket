import React from 'react';
import { Navigate } from 'react-router-dom';
import { useAppSelector } from '../utils/hooks';
import { useTokenValidation } from '../hooks/useTokenValidation';
// #region agent log
fetch('http://127.0.0.1:7242/ingest/fbd6cafe-c55d-4c1d-9edb-12a6a38267bf',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'ProtectedRoute.tsx:3',message:'ProtectedRoute module - useAppSelector imported',data:{hasUseAppSelector:typeof useAppSelector!=='undefined'},timestamp:Date.now(),runId:'run1',hypothesisId:'D'})}).catch(()=>{});
// #endregion

interface ProtectedRouteProps {
  children: React.ReactNode;
  requiredRole?: string;
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ children, requiredRole }) => {
  const { user, isInitialized, isLoading } = useAppSelector((state) => state.auth);
  useTokenValidation();

  // Show loading while initializing
  if (!isInitialized || isLoading) {
    return (
      <div className="min-h-screen flex flex-col items-center justify-center">
        <div className="animate-spin rounded-full h-32 w-32 border-t-2 border-b-2 border-indigo-500"></div>
        <h2 className="text-xl font-semibold mt-4">Loading...</h2>
      </div>
    );
  }

  // If no user, redirect to login
  if (!user) {
    return <Navigate to="/login" replace />;
  }

  // Check for required role if specified
  if (requiredRole) {
    console.log('ProtectedRoute: Checking required role:', requiredRole);
    console.log('ProtectedRoute: User roles:', user.roles);
    console.log('ProtectedRoute: User object:', user);
    
    const hasRequiredRole = user.roles?.some((role: string) => 
      role.toLowerCase() === requiredRole.toLowerCase()
    );
    
    console.log('ProtectedRoute: Has required role?', hasRequiredRole);
    
    if (!hasRequiredRole) {
      console.warn(`Access denied: User roles [${user.roles?.join(', ')}] do not include '${requiredRole}', required for this route.`);
      return <Navigate to="/dashboard" replace />;
    }
  }

  // Otherwise render the children
  return <>{children}</>;
};

export default ProtectedRoute; 