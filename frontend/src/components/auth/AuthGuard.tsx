import React, { useEffect } from 'react';
import { useNavigate, Outlet } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../../utils/hooks';
import { checkFirstRun, getSetupStatus } from '../../store/slices/authSlice';

const AuthGuard: React.FC = () => {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { isFirstRun, user, setupStatus } = useAppSelector((state) => state.auth);

  useEffect(() => {
    const checkApplicationStatus = async () => {
      await dispatch(checkFirstRun());
      await dispatch(getSetupStatus());
    };
    checkApplicationStatus();
  }, [dispatch]);

  useEffect(() => {
    if (setupStatus) {
      // If admin is not configured, go to admin setup
      if (!setupStatus.isAdminConfigured) {
        navigate('/admin-setup');
        return;
      }
      
      // If admin is configured but role is not, go to role setup
      if (!setupStatus.isRoleConfigured) {
        navigate('/custom-role-setup');
        return;
      }
    } else if (isFirstRun) {
      // Fallback to first run check if setup status is not available
      navigate('/admin-setup');
      return;
    }
    
    // If not first run but no user, go to login
    if (!user) {
      navigate('/login');
    }
  }, [isFirstRun, user, navigate, setupStatus]);

  return <Outlet />;
};

export default AuthGuard; 