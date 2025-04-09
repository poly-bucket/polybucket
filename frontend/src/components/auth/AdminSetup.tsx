import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../../utils/hooks';
import { register, reset, login, setAdminConfigured, getSetupStatus } from '../../store/slices/authSlice';
import { EyeIcon, EyeSlashIcon } from '@heroicons/react/24/outline';

interface ValidationErrors {
  [key: string]: string[];
}

const AdminSetup: React.FC = () => {
  const [formData, setFormData] = useState({
    username: '',
    email: '',
    password: '',
    confirmPassword: '',
    firstName: '',
    lastName: '',
  });
  const [passwordMismatch, setPasswordMismatch] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [validationErrors, setValidationErrors] = useState<ValidationErrors>({});
  const { username, email, password, confirmPassword, firstName, lastName } = formData;

  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { user, isLoading, isSuccess, isError, errorMessage, setupStatus } = useAppSelector(
    (state) => state.auth
  );

  useEffect(() => {
    // If we don't have setup status, fetch it
    if (!setupStatus) {
      dispatch(getSetupStatus());
    } 
    // If we have setup status and admin is already configured, redirect to role setup
    else if (setupStatus.isAdminConfigured) {
      navigate('/custom-role-setup');
    }
    
    // Clean up on unmount
    return () => {
      dispatch(reset());
    };
  }, [setupStatus, dispatch, navigate]);

  const onChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData((prevState) => ({
      ...prevState,
      [e.target.name]: e.target.value,
    }));
    
    // Clear password mismatch error when user types
    if (e.target.name === 'password' || e.target.name === 'confirmPassword') {
      setPasswordMismatch(false);
    }
    
    // Clear validation error for this field
    if (validationErrors[e.target.name]) {
      setValidationErrors(prev => {
        const newErrors = { ...prev };
        delete newErrors[e.target.name];
        return newErrors;
      });
    }
  };

  const onSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setValidationErrors({});

    if (password !== confirmPassword) {
      setPasswordMismatch(true);
      return;
    }

    const userData = {
      username,
      email,
      password,
      confirmPassword,
      firstName: firstName || 'User', // Use non-empty default instead of empty string
      lastName: lastName || 'Account', // Use non-empty default instead of empty string
      isAdmin: true, // This is the admin setup, so we set isAdmin to true
    };

    console.log('AdminSetup - Submitting registration data:', userData);
    
    dispatch(register(userData))
      .unwrap()
      .then((response: any) => {
        console.log('Registration successful:', response);
        
        // Mark admin setup as configured
        dispatch(setAdminConfigured(true))
          .unwrap()
          .then(() => {
            console.log('Admin setup marked as configured');
          })
          .catch((err: any) => {
            console.error('Failed to mark admin as configured:', err);
          });
        
        // Check if we have authentication tokens in the response
        if (response.accessToken) {
          // We already have auth tokens, navigate to role setup
          console.log('Registration returned auth tokens, proceeding to role setup');
          navigate('/custom-role-setup');
        } else {
          // No auth tokens in response, try explicit login
          console.log('Registration successful but no auth tokens, attempting login');
          
          // After successful registration, explicitly log in with the same credentials
          const loginData = {
            emailOrUsername: username,
            password: password
          };
          
          dispatch(login(loginData))
            .unwrap()
            .then((loginResponse: any) => {
              console.log('Auto-login successful:', loginResponse);
              navigate('/custom-role-setup');
            })
            .catch((loginError: any) => {
              console.error('Auto-login failed:', loginError);
              // Even if auto-login fails, we can still try to continue
              // At this point we should show a specific message to the user
              console.log('Proceeding to role setup despite login failure');
              navigate('/custom-role-setup');
            });
        }
      })
      .catch((error: any) => {
        console.error('Registration failed:', error);
        
        // Handle validation errors
        if (error.validationErrors) {
          // Convert backend field names to our frontend field names
          const mappedErrors: ValidationErrors = {};
          
          Object.entries(error.validationErrors).forEach(([key, messages]: [string, any]) => {
            // Convert backend field names to lowercase to match our frontend naming
            const fieldName = key.toLowerCase();
            mappedErrors[fieldName] = messages;
          });
          
          setValidationErrors(mappedErrors);
        }
      });
  };

  // Helper to get field error message
  const getFieldError = (fieldName: string): string => {
    const errors = validationErrors[fieldName.toLowerCase()];
    if (errors && errors.length > 0) {
      return errors[0];
    }
    return '';
  };

  return (
    <div className="flex flex-col items-center justify-center min-h-screen bg-gray-50">
      <div className="w-full max-w-md">
        <div className="text-center mb-10">
          <h1 className="text-3xl font-bold">Welcome to PolyBucket</h1>
          <p className="mt-2 text-gray-600">Let's set up your admin account</p>
        </div>

        <form onSubmit={onSubmit} className="space-y-6 bg-white p-8 rounded-lg shadow">
          <div>
            <label htmlFor="username" className="block text-sm font-medium text-gray-700">
              Username
            </label>
            <input
              type="text"
              name="username"
              id="username"
              value={username}
              onChange={onChange}
              required
              className={`mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm ${
                getFieldError('username') ? 'border-red-500' : ''
              }`}
              placeholder="Choose a username"
            />
            {getFieldError('username') && (
              <p className="mt-1 text-sm text-red-600">{getFieldError('username')}</p>
            )}
          </div>

          <div>
            <label htmlFor="email" className="block text-sm font-medium text-gray-700">
              Email
            </label>
            <input
              type="email"
              name="email"
              id="email"
              value={email}
              onChange={onChange}
              required
              className={`mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm ${
                getFieldError('email') ? 'border-red-500' : ''
              }`}
              placeholder="Enter your email"
            />
            {getFieldError('email') && (
              <p className="mt-1 text-sm text-red-600">{getFieldError('email')}</p>
            )}
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label htmlFor="firstName" className="block text-sm font-medium text-gray-700">
                First Name
              </label>
              <input
                type="text"
                name="firstName"
                id="firstName"
                value={firstName}
                onChange={onChange}
                className={`mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm ${
                  getFieldError('firstname') ? 'border-red-500' : ''
                }`}
                placeholder="First Name"
              />
              {getFieldError('firstname') && (
                <p className="mt-1 text-sm text-red-600">{getFieldError('firstname')}</p>
              )}
            </div>
            <div>
              <label htmlFor="lastName" className="block text-sm font-medium text-gray-700">
                Last Name
              </label>
              <input
                type="text"
                name="lastName"
                id="lastName"
                value={lastName}
                onChange={onChange}
                className={`mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm ${
                  getFieldError('lastname') ? 'border-red-500' : ''
                }`}
                placeholder="Last Name"
              />
              {getFieldError('lastname') && (
                <p className="mt-1 text-sm text-red-600">{getFieldError('lastname')}</p>
              )}
            </div>
          </div>

          <div className="relative">
            <label htmlFor="password" className="block text-sm font-medium text-gray-700">
              Password
            </label>
            <div className="mt-1 relative rounded-md shadow-sm">
              <input
                type={showPassword ? 'text' : 'password'}
                name="password"
                id="password"
                value={password}
                onChange={onChange}
                required
                className={`block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm pr-10 ${
                  getFieldError('password') ? 'border-red-500' : ''
                }`}
                placeholder="Create a password"
              />
              <button
                type="button"
                onClick={() => setShowPassword(!showPassword)}
                className="absolute inset-y-0 right-0 pr-3 flex items-center"
              >
                {showPassword ? (
                  <EyeSlashIcon className="h-5 w-5 text-gray-400" />
                ) : (
                  <EyeIcon className="h-5 w-5 text-gray-400" />
                )}
              </button>
            </div>
            {getFieldError('password') && (
              <p className="mt-1 text-sm text-red-600">{getFieldError('password')}</p>
            )}
          </div>

          <div className="relative">
            <label htmlFor="confirmPassword" className="block text-sm font-medium text-gray-700">
              Confirm Password
            </label>
            <div className="mt-1 relative rounded-md shadow-sm">
              <input
                type={showConfirmPassword ? 'text' : 'password'}
                name="confirmPassword"
                id="confirmPassword"
                value={confirmPassword}
                onChange={onChange}
                required
                className="block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm pr-10"
                placeholder="Confirm your password"
              />
              <button
                type="button"
                onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                className="absolute inset-y-0 right-0 pr-3 flex items-center"
              >
                {showConfirmPassword ? (
                  <EyeSlashIcon className="h-5 w-5 text-gray-400" />
                ) : (
                  <EyeIcon className="h-5 w-5 text-gray-400" />
                )}
              </button>
            </div>
          </div>

          {passwordMismatch && (
            <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded relative" role="alert">
              <span className="block sm:inline">Passwords do not match</span>
            </div>
          )}

          {isError && Object.keys(validationErrors).length === 0 && (
            <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded relative" role="alert">
              <span className="block sm:inline">{errorMessage}</span>
            </div>
          )}

          <button
            type="submit"
            disabled={isLoading}
            className={`w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 ${
              isLoading ? 'opacity-50 cursor-not-allowed' : ''
            }`}
          >
            {isLoading ? 'Creating Account...' : 'Create Admin Account'}
          </button>
        </form>
      </div>
    </div>
  );
};

export default AdminSetup; 