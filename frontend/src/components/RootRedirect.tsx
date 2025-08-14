import React, { useEffect, useState } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAppSelector } from '../utils/hooks';
import api from '../utils/axiosConfig';

const RootRedirect: React.FC = () => {
  const { user, isInitialized, isLoading } = useAppSelector((state) => state.auth);
  const location = useLocation();
  const [setupStatus, setSetupStatus] = useState<any>(null);
  const [isCheckingSetup, setIsCheckingSetup] = useState(false);

  useEffect(() => {
    const checkSetupStatus = async () => {
      if (user && user.accessToken && user.roles?.includes('Admin')) {
        console.log('RootRedirect: Checking setup status for admin user');
        setIsCheckingSetup(true);
        try {
          const response = await api.get('/SystemSetup/status');

          if (response.status === 200) {
            const status = response.data;
            console.log('RootRedirect: Setup status received:', status);
            setSetupStatus(status);
          }
        } catch (error) {
          console.error('RootRedirect: Error checking setup status:', error);
        } finally {
          setIsCheckingSetup(false);
        }
      } else {
        console.log('RootRedirect: Not checking setup status - user:', user?.username, 'roles:', user?.roles);
      }
    };

    checkSetupStatus();
  }, [user]);

  // Show loading while initializing or checking setup
  if (!isInitialized || isLoading || isCheckingSetup) {
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

  // If user is logged in, check for setup requirements
  if (user && user.accessToken && user.accessToken.length > 0) {
    // Check if user requires password change
    if (user.requiresPasswordChange) {
      console.log('RootRedirect: User requires password change, redirecting to setup');
      return <Navigate to="/setup" replace />;
    }

    // Check if user requires first-time setup
    if (user.requiresFirstTimeSetup) {
      console.log('RootRedirect: User requires first-time setup, redirecting to setup');
      return <Navigate to="/setup" replace />;
    }

    // Check if admin user needs to complete system setup (only for root route)
    if (location.pathname === '/' && user.roles?.includes('Admin') && setupStatus?.isFirstTimeSetup) {
      console.log('RootRedirect: Admin user needs to complete system setup, redirecting to setup');
      console.log('RootRedirect: Setup status:', setupStatus);
      return <Navigate to="/setup" replace />;
    }

    // Only redirect to dashboard if we're actually on the root route
    if (location.pathname === '/') {
      console.log('RootRedirect: User is authenticated, redirecting to dashboard');
      return <Navigate to="/dashboard" replace />;
    }
    
    // For any other route, don't redirect - let the normal routing handle it
    console.log('RootRedirect: User authenticated, allowing access to:', location.pathname);
    return null;
  }

  // If no user or no valid token, go to login
  console.log('User is not authenticated, redirecting to login');
  return <Navigate to="/login" replace />;
};

export default RootRedirect; 