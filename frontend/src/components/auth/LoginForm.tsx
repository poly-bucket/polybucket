import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../../store';
import { loginUser } from '../../store/thunks/authThunks';
import { reset } from '../../store/slices/authSlice';
import { EyeIcon, EyeSlashIcon } from '@heroicons/react/24/outline';
import authSettingsService from '../../services/authSettingsService';
import TwoFactorAuthInput from './TwoFactorAuthInput';

const LoginForm: React.FC = () => {
  const [formData, setFormData] = useState({
    email: '',
    password: '',
  });
  const [showPassword, setShowPassword] = useState(false);
  const [loginPlaceholder, setLoginPlaceholder] = useState('Email or Username');
  const [showTwoFactorInput, setShowTwoFactorInput] = useState(false);
  const [twoFactorError, setTwoFactorError] = useState<string | null>(null);
  const [isTwoFactorLoading, setIsTwoFactorLoading] = useState(false);
  const { email, password } = formData;

  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { user, isLoading, isSuccess, isError, errorMessage } = useAppSelector(
    (state) => state.auth
  );

  // Load authentication settings
  useEffect(() => {
    const loadAuthSettings = async () => {
      try {
        await authSettingsService.getSettings();
        setLoginPlaceholder(authSettingsService.getLoginPlaceholder());
      } catch (error) {
        console.warn('Failed to load auth settings:', error);
      }
    };
    
    loadAuthSettings();
  }, []);

  // Handle redirects after successful login
  useEffect(() => {
    console.log('=== REDIRECT EFFECT ===');
    console.log('user:', user);
    console.log('isSuccess:', isSuccess);
    console.log('isError:', isError);
    console.log('errorMessage:', errorMessage);
    console.log('isLoading:', isLoading);
    
    // Only redirect if:
    // 1. We have a valid user object
    // 2. The user has a valid access token
    // 3. We're not in an error state
    // 4. We're not loading
    // 5. The user doesn't require 2FA
    // 6. We have a successful login state
    // 7. The user has a valid ID (from JWT)
    if (user && 
        user.accessToken && 
        user.accessToken.length > 0 && 
        !isError && 
        !isLoading && 
        !user.requiresTwoFactor && 
        isSuccess &&
        user.id && 
        user.id.length > 0) {
      console.log('Redirecting to dashboard/setup');
      // Check if user requires first-time setup
      if (user.requiresFirstTimeSetup) {
        navigate('/setup');
      } else {
        navigate('/dashboard');
      }
    } else {
      console.log('Not redirecting - conditions not met:');
      console.log('- user exists:', !!user);
      console.log('- has access token:', !!(user?.accessToken && user.accessToken.length > 0));
      console.log('- no error:', !isError);
      console.log('- not loading:', !isLoading);
      console.log('- no 2FA required:', !user?.requiresTwoFactor);
      console.log('- is success:', isSuccess);
      console.log('- has valid ID:', !!(user?.id && user.id.length > 0));
    }
  }, [user, isSuccess, navigate, isError, errorMessage, isLoading]);

  // Handle 2FA requirement
  useEffect(() => {
    console.log('=== 2FA EFFECT ===');
    console.log('user?.requiresTwoFactor:', user?.requiresTwoFactor);
    
    if (user?.requiresTwoFactor) {
      setShowTwoFactorInput(true);
    }
  }, [user?.requiresTwoFactor]);

  // Debug error state changes
  useEffect(() => {
    console.log('=== ERROR STATE CHANGE ===');
    console.log('isError:', isError);
    console.log('errorMessage:', errorMessage);
    console.log('isLoading:', isLoading);
  }, [isError, errorMessage, isLoading]);

  const onChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData((prevState) => ({
      ...prevState,
      [e.target.name]: e.target.value,
    }));
  };

  const onSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    
    // Prevent multiple submissions
    if (isLoading) {
      console.log('Form submission blocked - already loading');
      return;
    }
    
    console.log('=== FORM SUBMITTED ===');
    console.log('Email:', email);
    console.log('Password length:', password.length);

    const credentials = {
      emailOrUsername: email,
      email: email,
      password: password,
    };

    console.log('Dispatching login with credentials:', credentials);
    
    dispatch(loginUser(credentials))
      .unwrap()
      .then(() => {
        console.log('Login successful!');
        // The slice handles the redirect logic now
      })
      .catch((error: unknown) => {
        console.error('Login failed:', error);
        // Error is already handled by the Redux slice, no need to do anything here
        // The error will be displayed via the isError and errorMessage state
      });
  };

  const handleTwoFactorSubmit = async (token: string, isBackupCode: boolean) => {
    setIsTwoFactorLoading(true);
    setTwoFactorError(null);

    try {
      const credentials = {
        emailOrUsername: email,
        email: email,
        password: password,
        twoFactorToken: isBackupCode ? undefined : token,
        backupCode: isBackupCode ? token : undefined,
      };

      await dispatch(loginUser(credentials)).unwrap();
      console.log('2FA verification successful!');
    } catch (error: any) {
      console.error('2FA verification failed:', error);
      setTwoFactorError(error.message || 'Invalid 2FA code. Please try again.');
    } finally {
      setIsTwoFactorLoading(false);
    }
  };

  const handleTwoFactorCancel = () => {
    setShowTwoFactorInput(false);
    setTwoFactorError(null);
    dispatch(reset());
  };

  return (
    <div className="lg-container min-h-screen flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-md w-full space-y-8">
        <div className="text-center">
          <h2 className="text-3xl font-extrabold text-white mb-2">
            {showTwoFactorInput ? 'Two-Factor Authentication' : 'Welcome Back'}
          </h2>
        </div>

        {showTwoFactorInput ? (
          <div className="lg-card p-8">
            <TwoFactorAuthInput
              onSubmit={handleTwoFactorSubmit}
              onCancel={handleTwoFactorCancel}
              loading={isTwoFactorLoading}
              error={twoFactorError || undefined}
            />
          </div>
        ) : (
          <div className="lg-card p-8">
            <form className="space-y-6" onSubmit={onSubmit}>
              <div className="space-y-4">
                <div>
                  <input
                    id="email"
                    name="email"
                    type="text"
                    required
                    className="lg-input"
                    placeholder={loginPlaceholder}
                    value={email}
                    onChange={onChange}
                  />
                </div>
                
                <div>
                  <div className="relative">
                    <input
                      id="password"
                      name="password"
                      type={showPassword ? 'text' : 'password'}
                      required
                      className="lg-input pr-10"
                      placeholder="Enter your password"
                      value={password}
                      onChange={onChange}
                    />
                    <button
                      type="button"
                      className="absolute inset-y-0 right-0 pr-3 flex items-center"
                      onClick={() => setShowPassword(!showPassword)}
                    >
                      {showPassword ? (
                        <EyeSlashIcon className="h-5 w-5 text-gray-400 hover:text-white transition-colors" />
                      ) : (
                        <EyeIcon className="h-5 w-5 text-gray-400 hover:text-white transition-colors" />
                      )}
                    </button>
                  </div>
                </div>
              </div>

              <div>
                <button
                  type="submit"
                  disabled={isLoading}
                  className="lg-button lg-button-primary w-full"
                >
                  {isLoading ? (
                    <div className="flex items-center justify-center">
                      <div className="lg-spinner w-4 h-4 mr-2"></div>
                      Signing in...
                    </div>
                  ) : (
                    'Sign in'
                  )}
                </button>
              </div>

              {isError && (
                <div className="lg-badge lg-badge-error w-full justify-center">
                  {errorMessage}
                </div>
              )}

              <div className="text-center">
                <p className="text-sm text-gray-300">
                  Don't have an account?{' '}
                  <a href="/register" className="font-medium text-blue-400 hover:text-blue-300 transition-colors">
                    Sign up
                  </a>
                </p>
              </div>
            </form>
          </div>
        )}
      </div>
    </div>
  );
};

export default LoginForm; 