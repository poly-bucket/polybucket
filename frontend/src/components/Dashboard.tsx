import React, { useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../utils/hooks';
import { logout, getSetupStatus } from '../store/slices/authSlice';

const Dashboard: React.FC = () => {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { user, setupStatus } = useAppSelector((state) => state.auth);

  // Check setup status and redirect if needed
  useEffect(() => {
    // If we don't have setup status, fetch it
    if (!setupStatus) {
      dispatch(getSetupStatus());
    } else if (setupStatus && !setupStatus.isRoleConfigured) {
      // If roles are not configured, redirect to role setup
      navigate('/custom-role-setup');
    }
  }, [setupStatus, dispatch, navigate]);

  const handleLogout = () => {
    dispatch(logout());
    navigate('/login');
  };

  return (
    <div className="min-h-screen bg-gray-100">
      <nav className="bg-white shadow-sm">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex">
              <div className="flex-shrink-0 flex items-center">
                <span className="text-xl font-bold text-indigo-600">PolyBucket</span>
              </div>
            </div>
            <div className="flex items-center">
              <div className="ml-3 relative">
                <div className="flex items-center">
                  <span className="mr-4 text-sm font-medium text-gray-700">
                    {user?.username}
                  </span>
                  {user?.roles?.includes('Admin') && (
                    <Link 
                      to="/admin"
                      className="mr-4 px-3 py-2 rounded-md text-sm font-medium text-gray-700 hover:text-gray-900 hover:bg-gray-50"
                    >
                      Admin Panel
                    </Link>
                  )}
                  <button
                    onClick={handleLogout}
                    className="px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                  >
                    Logout
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </nav>

      <div className="py-10">
        <header>
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <h1 className="text-3xl font-bold leading-tight text-gray-900">Dashboard</h1>
          </div>
        </header>
        <main>
          <div className="max-w-7xl mx-auto sm:px-6 lg:px-8">
            <div className="px-4 py-8 sm:px-0">
              <div className="border-4 border-dashed border-gray-200 rounded-lg p-6">
                <p className="text-lg text-gray-700">
                  Welcome to your dashboard, {user?.username}! You are now logged in.
                </p>
                <p className="mt-4 text-sm text-gray-500">
                  Email: {user?.email}
                </p>
                <div className="mt-6">
                  <h2 className="text-lg font-medium text-gray-900">Roles:</h2>
                  <ul className="mt-2 list-disc list-inside">
                    {user?.roles && user.roles.length > 0 ? (
                      user.roles.map((role: string, index: number) => (
                        <li key={index} className="text-sm text-gray-600">{role}</li>
                      ))
                    ) : (
                      <li className="text-sm text-gray-600">No roles assigned</li>
                    )}
                  </ul>
                </div>
              </div>
            </div>
          </div>
        </main>
      </div>
    </div>
  );
};

export default Dashboard; 