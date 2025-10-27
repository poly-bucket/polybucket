import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../../store';
import { registerUser } from '../../store/thunks/authThunks';
import { reset } from '../../store/slices/authSlice';
import { EyeIcon, EyeSlashIcon } from '@heroicons/react/24/outline';

interface RegisterFormProps {
  isAdminSetup?: boolean;
}

const RegisterForm: React.FC<RegisterFormProps> = ({ isAdminSetup = false }) => {
  const [formData, setFormData] = useState({
    username: '',
    email: '',
    password: '',
    confirmPassword: '',
  });
  const [showPassword, setShowPassword] = useState(false);
  const [passwordMatchError, setPasswordMatchError] = useState('');

  const { username, email, password, confirmPassword } = formData;

  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { user, isLoading, isSuccess, isError, errorMessage } = useAppSelector(
    (state) => state.auth
  );

  useEffect(() => {
    if (isSuccess || user) {
      navigate('/dashboard');
    }

    return () => {
      dispatch(reset());
    };
  }, [user, isSuccess, navigate, dispatch]);

  const onChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData((prevState) => ({
      ...prevState,
      [e.target.name]: e.target.value,
    }));

    // Clear password match error when typing in password fields
    if (e.target.name === 'password' || e.target.name === 'confirmPassword') {
      setPasswordMatchError('');
    }
  };

  const validateForm = (): boolean => {
    if (password !== confirmPassword) {
      setPasswordMatchError('Passwords do not match');
      return false;
    }
    return true;
  };

  const onSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    const userData = {
      username,
      email,
      password,
      confirmPassword,
      isAdmin: isAdminSetup
    };

    dispatch(registerUser(userData));
  };

  return (
    <div className="lg-container min-h-screen flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-md w-full space-y-8">
        <div>
          <h2 className="mt-6 text-center text-3xl font-extrabold text-white">
            {isAdminSetup ? 'Create Admin Account' : 'Create an account'}
          </h2>
          {isAdminSetup && (
            <p className="mt-2 text-center text-sm text-white/60">
              This is the first time setup. This account will have administrator privileges.
            </p>
          )}
        </div>
        <div className="lg-card p-8">
          <form className="space-y-6" onSubmit={onSubmit}>
            <div className="space-y-4">
              <div>
                <label htmlFor="username" className="block text-sm font-medium text-white/80 mb-1">
                  Username
                </label>
                <input
                  id="username"
                  name="username"
                  type="text"
                  autoComplete="username"
                  required
                  className="lg-input"
                  placeholder="Username"
                  value={username}
                  onChange={onChange}
                />
              </div>
              <div>
                <label htmlFor="email" className="block text-sm font-medium text-white/80 mb-1">
                  Email address
                </label>
                <input
                  id="email"
                  name="email"
                  type="email"
                  autoComplete="email"
                  required
                  className="lg-input"
                  placeholder="Email address"
                  value={email}
                  onChange={onChange}
                />
              </div>
              <div className="relative">
                <label htmlFor="password" className="block text-sm font-medium text-white/80 mb-1">
                  Password
                </label>
                <input
                  id="password"
                  name="password"
                  type={showPassword ? 'text' : 'password'}
                  autoComplete="new-password"
                  required
                  className="lg-input pr-10"
                  placeholder="Password"
                  value={password}
                  onChange={onChange}
                />
                <button
                  type="button"
                  className="absolute inset-y-0 right-0 pr-3 flex items-center top-6"
                  onClick={() => setShowPassword(!showPassword)}
                >
                  {showPassword ? (
                    <EyeSlashIcon className="h-5 w-5 text-white/40" />
                  ) : (
                    <EyeIcon className="h-5 w-5 text-white/40" />
                  )}
                </button>
              </div>
              <div>
                <label htmlFor="confirmPassword" className="block text-sm font-medium text-white/80 mb-1">
                  Confirm Password
                </label>
                <input
                  id="confirmPassword"
                  name="confirmPassword"
                  type={showPassword ? 'text' : 'password'}
                  autoComplete="new-password"
                  required
                  className="lg-input"
                  placeholder="Confirm Password"
                  value={confirmPassword}
                  onChange={onChange}
                />
              </div>
            </div>

            {passwordMatchError && (
              <div className="bg-red-500/20 border border-red-500/50 text-red-200 px-4 py-3 rounded">
                <span className="block sm:inline">{passwordMatchError}</span>
              </div>
            )}

            {isError && (
              <div className="bg-red-500/20 border border-red-500/50 text-red-200 px-4 py-3 rounded">
                <span className="block sm:inline">{errorMessage}</span>
              </div>
            )}

            <div>
              <button
                type="submit"
                className="w-full lg-button lg-button-primary disabled:opacity-50 disabled:cursor-not-allowed"
                disabled={isLoading}
              >
                {isLoading ? (
                  <div className="flex items-center justify-center">
                    <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                    </svg>
                    Creating account...
                  </div>
                ) : isAdminSetup ? 'Create Admin Account' : 'Sign up'}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};

export default RegisterForm; 