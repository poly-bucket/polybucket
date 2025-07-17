import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../../utils/hooks';
import { checkFirstRun, getSetupStatus, login, setAdminConfigured } from '../../store/slices/authSlice';
import AdminSetup from './AdminSetup';

const FirstTimeSetup: React.FC = () => {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const [isChecking, setIsChecking] = useState(true);
  const [showAdminSetup, setShowAdminSetup] = useState(false);
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

  const handleUseDefaultAdmin = async () => {
    try {
      // Login with default admin credentials
      await dispatch(login({ email: 'admin@polybucket.com', password: 'admin' }));
      
      // Mark admin setup as configured since we're using the default admin
      await dispatch(setAdminConfigured(true));
      
      // Redirect to dashboard or next setup step
      navigate('/dashboard');
    } catch (error) {
      console.error('Failed to login with default admin:', error);
    }
  };

  const handleCreateCustomAdmin = () => {
    setShowAdminSetup(true);
  };

  // Still loading, show loading state
  if (isLoading || isChecking) {
    return (
      <div className="min-h-screen flex flex-col items-center justify-center">
        <div className="animate-spin rounded-full h-32 w-32 border-t-2 border-b-2 border-indigo-500"></div>
        <h2 className="text-xl font-semibold mt-4">Checking system status...</h2>
      </div>
    );
  }

  // Show admin setup if user chose to create custom admin
  if (showAdminSetup) {
    return <AdminSetup />;
  }

  // Show choice screen
  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-md w-full space-y-8">
        <div>
          <h2 className="mt-6 text-center text-3xl font-extrabold text-gray-900">
            Welcome to PolyBucket
          </h2>
          <p className="mt-2 text-center text-sm text-gray-600">
            Choose how you'd like to get started
          </p>
        </div>
        
        <div className="mt-8 space-y-4">
          <div className="bg-white p-6 rounded-lg border border-gray-200 shadow-sm">
            <h3 className="text-lg font-medium text-gray-900 mb-2">Quick Start</h3>
            <p className="text-sm text-gray-600 mb-4">
              Use the default admin account and get started immediately. You can change the password and email later.
            </p>
            <button
              onClick={handleUseDefaultAdmin}
              className="w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
            >
              Continue with Default Admin
            </button>
          </div>
          
          <div className="bg-white p-6 rounded-lg border border-gray-200 shadow-sm">
            <h3 className="text-lg font-medium text-gray-900 mb-2">Custom Setup</h3>
            <p className="text-sm text-gray-600 mb-4">
              Create your own admin account with custom credentials. The default admin account will be removed.
            </p>
            <button
              onClick={handleCreateCustomAdmin}
              className="w-full flex justify-center py-2 px-4 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
            >
              Create Custom Admin Account
            </button>
          </div>
        </div>
        
        <div className="mt-6 text-center">
          <p className="text-xs text-gray-500">
            Default admin credentials: admin@polybucket.com / admin
          </p>
        </div>
      </div>
    </div>
  );
};

export default FirstTimeSetup; 