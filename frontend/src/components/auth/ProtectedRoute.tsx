import React, { useEffect } from 'react';
import { Navigate, Outlet, useNavigate } from 'react-router-dom';
import { useAppSelector, useAppDispatch } from '../../utils/hooks';
import { checkFirstRun } from '../../store/slices/authSlice';

const ProtectedRoute: React.FC = () => {
  const { user, isLoading } = useAppSelector((state) => state.auth);
  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  useEffect(() => {
    // If we don't have a user, check if it's first run
    if (!user && !isLoading) {
      dispatch(checkFirstRun())
        .unwrap()
        .then((result: { isFirstRun: boolean }) => {
          if (result.isFirstRun) {
            navigate('/');
          }
        })
        .catch(() => {
          navigate('/login');
        });
    }
  }, [user, isLoading, dispatch, navigate]);

  // Show loading indicator while checking authentication
  if (isLoading) {
    return (
      <div className="min-h-screen flex flex-col items-center justify-center">
        <div className="animate-spin rounded-full h-32 w-32 border-t-2 border-b-2 border-indigo-500"></div>
        <h2 className="text-xl font-semibold mt-4">Verifying authentication...</h2>
      </div>
    );
  }

  // If user is not authenticated, redirect to login
  if (!user) {
    return <Navigate to="/login" replace />;
  }

  // If user is authenticated, render the child routes
  return <Outlet />;
};

export default ProtectedRoute; 