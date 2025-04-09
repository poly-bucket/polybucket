import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../../utils/hooks';
import { checkFirstRun, getSetupStatus } from '../../store/slices/authSlice';
import AdminSetup from './AdminSetup';

const FirstTimeSetup: React.FC = () => {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const [isChecking, setIsChecking] = useState(true);
  const { isFirstRun, isLoading, setupStatus } = useAppSelector((state) => state.auth);

  useEffect(() => {
    const checkStatus = async () => {
      // First check the first run status
      await dispatch(checkFirstRun());
      
      // Also get detailed setup status
      await dispatch(getSetupStatus());
      
      setIsChecking(false);
    };
    checkStatus();
  }, [dispatch]);

  useEffect(() => {
    // If not checking and not loading
    if (!isChecking && !isLoading) {
      // If not first run at all, redirect to login
      if (isFirstRun === false) {
        navigate('/login');
        return;
      }
      
      // If we have setup status and admin is already configured, redirect to role setup
      if (setupStatus && setupStatus.isAdminConfigured) {
        navigate('/custom-role-setup');
        return;
      }
    }
  }, [isFirstRun, isLoading, isChecking, setupStatus, navigate]);

  // Still loading, show loading state
  if (isLoading || isChecking) {
    return (
      <div className="min-h-screen flex flex-col items-center justify-center">
        <div className="animate-spin rounded-full h-32 w-32 border-t-2 border-b-2 border-indigo-500"></div>
        <h2 className="text-xl font-semibold mt-4">Checking system status...</h2>
      </div>
    );
  }

  // Show admin setup only if admin is not configured
  return <AdminSetup />;
};

export default FirstTimeSetup; 