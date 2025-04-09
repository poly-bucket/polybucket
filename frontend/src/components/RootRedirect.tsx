import React, { useEffect, useState } from 'react';
import { Navigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../utils/hooks';
import { checkFirstRun, getSetupStatus } from '../store/slices/authSlice';

const RootRedirect: React.FC = () => {
  const dispatch = useAppDispatch();
  const [isChecking, setIsChecking] = useState(true);
  const { isFirstRun, user, isLoading, setupStatus } = useAppSelector((state) => state.auth);

  useEffect(() => {
    const checkStatus = async () => {
      try {
        // First check if it's the first run
        await dispatch(checkFirstRun());
        
        // Also get detailed setup status
        await dispatch(getSetupStatus());
        
        setIsChecking(false);
      } catch (error) {
        console.error('Error checking application status:', error);
        setIsChecking(false);
      }
    };
    checkStatus();
  }, [dispatch]);

  // Show loading state while checking
  if (isChecking || isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-indigo-500"></div>
      </div>
    );
  }

  // Check setup status to determine where to redirect
  if (setupStatus) {
    // If admin is not configured, go to admin setup
    if (!setupStatus.isAdminConfigured) {
      return <Navigate to="/admin-setup" replace />;
    }
    
    // If admin is configured but role is not, go to role setup
    if (!setupStatus.isRoleConfigured) {
      return <Navigate to="/custom-role-setup" replace />;
    }
  } else if (isFirstRun) {
    // Fallback to first run check if setup status is not available
    return <Navigate to="/admin-setup" replace />;
  }

  // If not first run but no user, go to login
  if (!user && !isFirstRun) {
    return <Navigate to="/login" replace />;
  }

  // If user exists, go to dashboard
  return <Navigate to="/dashboard" replace />;
};

export default RootRedirect; 