import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../../utils/hooks';
import { login, reset, getSetupStatus } from '../../store/slices/authSlice';
import { LoginRequest } from '../../services/api.client';
import { EyeIcon, EyeSlashIcon } from '@heroicons/react/24/outline';
import { store } from '../../store/store';

const LoginForm: React.FC = () => {
  const [formData, setFormData] = useState({
    email: '',
    password: '',
  });
  const [showPassword, setShowPassword] = useState(false);
  const { email, password } = formData;

  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { user, isLoading, isSuccess, isError, errorMessage, setupStatus } = useAppSelector(
    (state) => state.auth
  );

  useEffect(() => {
    if (user) {
      // If we have a user, fetch the setup status
      dispatch(getSetupStatus())
        .unwrap()
        .then(() => {
          // Check for redirect in the next effect
        })
        .catch((error: unknown) => {
          console.error('Failed to fetch setup status:', error);
        });
    }
  }, [user, dispatch]);
  
  // Handle redirects based on setup status
  useEffect(() => {
    if (user && setupStatus) {
      if (!setupStatus.isAdminSetupComplete) {
        navigate('/admin-setup');
      } else if (!setupStatus.isRoleSetupComplete) {
        navigate('/custom-role-setup');
      } else {
        navigate('/dashboard');
      }
    } else if (user && isSuccess) {
      // Fallback if setupStatus isn't available yet
      navigate('/dashboard');
    }
  }, [user, isSuccess, setupStatus, navigate]);

  const onChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData((prevState) => ({
      ...prevState,
      [e.target.name]: e.target.value,
    }));
  };

  const onSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    const credentials = new LoginRequest({
      email: email,
      password: password,
    });

    dispatch(login(credentials))
      .unwrap()
      .then(() => {
        // The slice handles the redirect logic now
      })
      .catch((error: unknown) => {
        console.error('Login failed:', error);
      });
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-md w-full space-y-8">
        <div>
          <h2 className="mt-6 text-center text-3xl font-extrabold text-gray-900">
            Sign in to your account
          </h2>
        </div>

        {/* Add test localStorage button */}
        <div className="flex justify-center">
          <button 
            type="button"
            onClick={() => {
              const testData = {
                id: 'test-id',
                username: 'test-user',
                email: 'test@example.com',
                accessToken: 'test-token-' + Date.now(),
                refreshToken: 'test-refresh-token',
                roles: ['User']
              };
              
              // Test storing in localStorage
              localStorage.setItem('user', JSON.stringify(testData));
              console.log('TEST: Data written to localStorage');
              
              // Verify it was stored
              setTimeout(() => {
                const storedData = localStorage.getItem('user');
                console.log('TEST: Data read from localStorage:', storedData);
              }, 100);
            }}
            className="px-4 py-2 bg-green-500 text-white rounded hover:bg-green-600"
          >
            Test localStorage
          </button>
        </div>

        <form className="mt-8 space-y-6" onSubmit={onSubmit}>
          <div className="rounded-md shadow-sm -space-y-px">
            <div>
              <label htmlFor="email" className="sr-only">
                Email
              </label>
              <input
                id="email"
                name="email"
                type="email"
                autoComplete="email"
                required
                className="appearance-none rounded-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-t-md focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 focus:z-10 sm:text-sm"
                placeholder="Email"
                value={email}
                onChange={onChange}
              />
            </div>
            <div className="relative">
              <label htmlFor="password" className="sr-only">
                Password
              </label>
              <input
                id="password"
                name="password"
                type={showPassword ? 'text' : 'password'}
                autoComplete="current-password"
                required
                className="appearance-none rounded-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-b-md focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 focus:z-10 sm:text-sm"
                placeholder="Password"
                value={password}
                onChange={onChange}
              />
              <button
                type="button"
                className="absolute inset-y-0 right-0 pr-3 flex items-center"
                onClick={() => setShowPassword(!showPassword)}
              >
                {showPassword ? (
                  <EyeSlashIcon className="h-5 w-5 text-gray-400" />
                ) : (
                  <EyeIcon className="h-5 w-5 text-gray-400" />
                )}
              </button>
            </div>
          </div>

          {isError && (
            <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded relative">
              <span className="block sm:inline">{errorMessage}</span>
            </div>
          )}

          <div>
            <button
              type="submit"
              className="group relative w-full flex justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
              disabled={isLoading}
            >
              {isLoading ? (
                <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
              ) : 'Sign in'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default LoginForm; 