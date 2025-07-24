import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAppSelector } from '../utils/hooks';

const RootRedirect: React.FC = () => {
  const { user, isInitialized, isLoading } = useAppSelector((state) => state.auth);
  const location = useLocation();

  // Show loading while initializing
  if (!isInitialized || isLoading) {
    return (
      <div className="min-h-screen flex flex-col items-center justify-center">
        <div className="animate-spin rounded-full h-32 w-32 border-t-2 border-b-2 border-indigo-500"></div>
        <h2 className="text-xl font-semibold mt-4">Loading...</h2>
      </div>
    );
  }

  // Allow access to specific routes without authentication
  const publicRoutes = ['/dashboard', '/public-dashboard', '/test-models'];
  if (publicRoutes.includes(location.pathname)) {
    console.log('Allowing access to public route:', location.pathname);
    return <Navigate to={location.pathname} replace />;
  }

  // TEMPORARY: Allow unauthenticated users to access dashboard for testing
  // TODO: Remove this after fixing the thumbnail issue
  if (!user) {
    console.log('Allowing unauthenticated access to dashboard for testing');
    return <Navigate to="/dashboard" replace />;
  }

  // If user is logged in, go to dashboard
  if (user) {
    return <Navigate to="/dashboard" replace />;
  }

  // If no user, go to login (this should not be reached with the above change)
  return <Navigate to="/login" replace />;
};

export default RootRedirect; 