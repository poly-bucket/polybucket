import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppSelector, useAppDispatch } from '../utils/hooks';
import UserAvatar from '../components/UserAvatar';
import NavigationBar from '../components/common/NavigationBar';
import AvatarRegeneration from '../components/AvatarRegeneration';
import { PrivacySettings } from '../services/api.client';
import { useUserSettings } from '../context/UserSettingsContext';
import { updateUserDetails } from '../store/slices/authSlice';
import { Tab } from '@headlessui/react';
import { ShieldCheckIcon, UserIcon } from '@heroicons/react/24/outline';
import TwoFactorAuth from '../components/auth/TwoFactorAuth';

interface UserSettings {
  language: string;
  theme: string;
  emailNotifications: boolean;
  defaultPrinterId: string;
  measurementSystem: string;
  timeZone: string;
  autoRotateModels: boolean;
  // Additional UI-focused settings for the future
  autoSaveInterval: number;
  showAdvancedOptions: boolean;
  defaultPrivacy: PrivacySettings;
  notificationSound: boolean;
}

const UserSettings: React.FC = () => {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { user } = useAppSelector((state) => state.auth);
  const { settings, loading, updateSettings } = useUserSettings();
  
  // Local state for editing
  const [localSettings, setLocalSettings] = useState<UserSettings>({
    language: 'en',
    theme: 'dark',
    emailNotifications: true,
    defaultPrinterId: '',
    measurementSystem: 'metric',
    timeZone: 'UTC',
    autoRotateModels: true,
    autoSaveInterval: 5,
    showAdvancedOptions: false,
    defaultPrivacy: PrivacySettings.Public,
    notificationSound: true,
  });

  const [isEditing, setIsEditing] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [selectedTab, setSelectedTab] = useState(0);

  // Update local settings when context settings change
  useEffect(() => {
    if (settings) {
      setLocalSettings({
        language: settings.language || 'en',
        theme: settings.theme || 'dark',
        emailNotifications: settings.emailNotifications ?? true,
        defaultPrinterId: settings.defaultPrinterId || '',
        measurementSystem: settings.measurementSystem || 'metric',
        timeZone: settings.timeZone || 'UTC',
        autoRotateModels: settings.autoRotateModels ?? true,
        autoSaveInterval: 5,
        showAdvancedOptions: false,
        defaultPrivacy: PrivacySettings.Public,
        notificationSound: true,
      });
    }
  }, [settings]);

  // Mock printer options
  const printerOptions = [
    { id: '', name: 'No default printer' },
    { id: 'printer-1', name: 'Ender 3 Pro' },
    { id: 'printer-2', name: 'Prusa i3 MK3S+' },
    { id: 'printer-3', name: 'Bambu Lab X1 Carbon' },
  ];

  const languageOptions = [
    { code: 'en', name: 'English' },
    { code: 'es', name: 'Español' },
    { code: 'fr', name: 'Français' },
    { code: 'de', name: 'Deutsch' },
    { code: 'zh', name: '中文' },
    { code: 'ja', name: '日本語' },
  ];

  const timeZoneOptions = [
    { code: 'UTC', name: 'UTC (Coordinated Universal Time)' },
    { code: 'America/New_York', name: 'Eastern Time (US & Canada)' },
    { code: 'America/Chicago', name: 'Central Time (US & Canada)' },
    { code: 'America/Denver', name: 'Mountain Time (US & Canada)' },
    { code: 'America/Los_Angeles', name: 'Pacific Time (US & Canada)' },
    { code: 'Europe/London', name: 'London' },
    { code: 'Europe/Berlin', name: 'Berlin' },
    { code: 'Asia/Tokyo', name: 'Tokyo' },
  ];

  const handleSettingChange = (key: keyof UserSettings, value: any) => {
    setLocalSettings(prev => ({
      ...prev,
      [key]: value
    }));
  };

  const handleSave = async () => {
    setIsSaving(true);
    try {
      const success = await updateSettings(localSettings);
      if (success) {
        setIsEditing(false);
        // Update the user's theme in Redux if it changed
        if (localSettings.theme !== user?.theme) {
          dispatch(updateUserDetails({ theme: localSettings.theme }));
        }
      }
    } catch (error) {
      console.error('Failed to save settings:', error);
    } finally {
      setIsSaving(false);
    }
  };

  const handleCancel = () => {
    setLocalSettings({
      language: settings?.language || 'en',
      theme: settings?.theme || 'dark',
      emailNotifications: settings?.emailNotifications ?? true,
      defaultPrinterId: settings?.defaultPrinterId || '',
      measurementSystem: settings?.measurementSystem || 'metric',
      timeZone: settings?.timeZone || 'UTC',
      autoRotateModels: settings?.autoRotateModels ?? true,
      autoSaveInterval: 5,
      showAdvancedOptions: false,
      defaultPrivacy: PrivacySettings.Public,
      notificationSound: true,
    });
    setIsEditing(false);
  };

  const handleAvatarUpdate = (newAvatar: string) => {
    // Handle avatar update if needed
    console.log('Avatar updated:', newAvatar);
  };

  const tabs = [
    {
      name: 'Account Preferences',
      icon: UserIcon,
      component: (
        <div className="space-y-6">
          {/* Avatar Customization */}
          <AvatarRegeneration onAvatarUpdate={handleAvatarUpdate} />
          
          {/* Language & Localization */}
          <div className="space-y-4">
            <h3 className="text-lg font-medium text-white">Language & Region</h3>
            
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-white/80 mb-2">
                  Language
                </label>
                <select
                  value={localSettings.language}
                  onChange={(e) => handleSettingChange('language', e.target.value)}
                  disabled={!isEditing}
                  className="lg-input disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {languageOptions.map(lang => (
                    <option key={lang.code} value={lang.code}>
                      {lang.name}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-white/80 mb-2">
                  Time Zone
                </label>
                <select
                  value={localSettings.timeZone}
                  onChange={(e) => handleSettingChange('timeZone', e.target.value)}
                  disabled={!isEditing}
                  className="lg-input disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {timeZoneOptions.map(tz => (
                    <option key={tz.code} value={tz.code}>
                      {tz.name}
                    </option>
                  ))}
                </select>
              </div>
            </div>
          </div>

          {/* Measurement System */}
          <div className="space-y-4">
            <h3 className="text-lg font-medium text-white">Measurement System</h3>
            
            <div>
              <label className="block text-sm font-medium text-white/80 mb-2">
                Default Measurement System
              </label>
              <select
                value={localSettings.measurementSystem}
                onChange={(e) => handleSettingChange('measurementSystem', e.target.value)}
                disabled={!isEditing}
                className="lg-input disabled:opacity-50 disabled:cursor-not-allowed"
              >
                <option value="metric">Metric (mm, kg, °C)</option>
                <option value="imperial">Imperial (in, lb, °F)</option>
              </select>
            </div>
          </div>

          {/* Appearance */}
          <div className="space-y-4">
            <h3 className="text-lg font-medium text-white">Appearance</h3>
            
            <div>
              <label className="block text-sm font-medium text-white/80 mb-2">
                Theme
              </label>
              <div className="space-y-2">
                {['light', 'dark', 'auto'].map((theme) => (
                  <label key={theme} className="flex items-center">
                    <input
                      type="radio"
                      name="theme"
                      value={theme}
                      checked={localSettings.theme === theme}
                      onChange={(e) => handleSettingChange('theme', e.target.value)}
                      disabled={!isEditing}
                      className="mr-2 disabled:opacity-50 disabled:cursor-not-allowed"
                    />
                    <span className="text-white capitalize">{theme}</span>
                  </label>
                ))}
              </div>
            </div>

            <div>
              <label className="flex items-center">
                <input
                  type="checkbox"
                  checked={localSettings.showAdvancedOptions}
                  onChange={(e) => handleSettingChange('showAdvancedOptions', e.target.checked)}
                  disabled={!isEditing}
                  className="mr-2 disabled:opacity-50 disabled:cursor-not-allowed"
                />
                <span className="text-white">Show advanced options in the interface</span>
              </label>
            </div>
          </div>

          {/* Notifications */}
          <div className="space-y-4">
            <h3 className="text-lg font-medium text-white">Notifications</h3>
            
            <div className="space-y-2">
              <label className="flex items-center">
                <input
                  type="checkbox"
                  checked={localSettings.emailNotifications}
                  onChange={(e) => handleSettingChange('emailNotifications', e.target.checked)}
                  disabled={!isEditing}
                  className="mr-2 disabled:opacity-50 disabled:cursor-not-allowed"
                />
                <span className="text-white">Email notifications</span>
              </label>
              
              <label className="flex items-center">
                <input
                  type="checkbox"
                  checked={localSettings.notificationSound}
                  onChange={(e) => handleSettingChange('notificationSound', e.target.checked)}
                  disabled={!isEditing}
                  className="mr-2 disabled:opacity-50 disabled:cursor-not-allowed"
                />
                <span className="text-white">Notification sounds</span>
              </label>
            </div>
          </div>
        </div>
      )
    },
    {
      name: 'Security',
      icon: ShieldCheckIcon,
      component: <SecurityTab />
    }
  ];

  if (loading) {
    return (
      <div className="lg-container min-h-screen">
        <NavigationBar
          title="User Settings"
          showSearch={false}
          showUploadButton={false}
          showHomeLink={true}
        />
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <div className="lg-card p-6">
            <div className="animate-pulse">
              <div className="h-4 bg-white/20 rounded w-1/4 mb-4"></div>
              <div className="h-4 bg-white/20 rounded w-1/2 mb-2"></div>
              <div className="h-4 bg-white/20 rounded w-3/4"></div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="lg-container min-h-screen">
      {/* Navigation Bar */}
      <NavigationBar
        title="User Settings"
        showSearch={false}
        showUploadButton={false}
        showHomeLink={true}
      />

      {/* Main Content */}
      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Profile Section */}
        <div className="lg-card mb-6 p-6">
          <div className="flex items-center">
            <UserAvatar 
              userId={user?.id || ''}
              username={user?.username || 'User'} 
              avatar={user?.avatar}
              profilePictureUrl={user?.profilePictureUrl}
              size="xl"
              className="mr-6"
            />
            <div>
              <h3 className="text-lg font-medium text-white">{user?.username}</h3>
              <p className="text-sm text-white/80">{user?.email}</p>
              <p className="text-xs text-white/60 mt-1">
                {user?.profilePictureUrl 
                  ? "Using your uploaded profile picture" 
                  : user?.avatar
                  ? "Using your custom generated avatar"
                  : "Your avatar is automatically generated from your username"
                }
              </p>
            </div>
          </div>
        </div>

        <div className="lg-card">
          {/* Header */}
          <div className="px-6 py-4 border-b border-white/10">
            <div className="flex items-center justify-between">
              <h2 className="text-lg font-medium text-white">Settings</h2>
              <div className="flex space-x-3">
                {!isEditing ? (
                  <button
                    onClick={() => setIsEditing(true)}
                    className="lg-button"
                  >
                    Edit Settings
                  </button>
                ) : (
                  <>
                    <button
                      onClick={handleCancel}
                      className="lg-button"
                    >
                      Cancel
                    </button>
                    <button
                      onClick={handleSave}
                      disabled={isSaving}
                      className="lg-button lg-button-primary"
                    >
                      {isSaving ? 'Saving...' : 'Save Changes'}
                    </button>
                  </>
                )}
              </div>
            </div>
          </div>

          {/* Tabbed Content */}
          <div className="p-6">
            <Tab.Group selectedIndex={selectedTab} onChange={setSelectedTab}>
              <Tab.List className="flex space-x-1 rounded-xl bg-white/10 p-1 mb-6">
                {tabs.map((tab, index) => (
                  <Tab
                    key={tab.name}
                    className={({ selected }) =>
                      `w-full rounded-lg py-2.5 text-sm font-medium leading-5 transition-all duration-200
                      ${selected 
                        ? 'bg-white text-gray-900 shadow'
                        : 'text-white/70 hover:text-white hover:bg-white/20'
                      }`
                    }
                  >
                    <div className="flex items-center justify-center space-x-2">
                      <tab.icon className="h-5 w-5" />
                      <span>{tab.name}</span>
                    </div>
                  </Tab>
                ))}
              </Tab.List>
              <Tab.Panels>
                {tabs.map((tab, index) => (
                  <Tab.Panel key={index}>
                    {tab.component}
                  </Tab.Panel>
                ))}
              </Tab.Panels>
            </Tab.Group>
          </div>
        </div>
      </div>
    </div>
  );
};

// Security Tab Component
const SecurityTab: React.FC = () => {
  const { user } = useAppSelector((state) => state.auth);
  const [isChangingPassword, setIsChangingPassword] = useState(false);
  const [passwordForm, setPasswordForm] = useState({
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  });
  const [showPasswords, setShowPasswords] = useState({
    current: false,
    new: false,
    confirm: false
  });
  const [errors, setErrors] = useState<{[key: string]: string}>({});
  const [success, setSuccess] = useState('');

  const handlePasswordChange = (field: string, value: string) => {
    setPasswordForm(prev => ({ ...prev, [field]: value }));
    // Clear errors when user starts typing
    if (errors[field]) {
      setErrors(prev => ({ ...prev, [field]: '' }));
    }
  };

  const validatePasswordForm = () => {
    const newErrors: {[key: string]: string} = {};

    if (!passwordForm.currentPassword) {
      newErrors.currentPassword = 'Current password is required';
    }

    if (!passwordForm.newPassword) {
      newErrors.newPassword = 'New password is required';
    } else if (passwordForm.newPassword.length < 8) {
      newErrors.newPassword = 'Password must be at least 8 characters long';
    }

    if (!passwordForm.confirmPassword) {
      newErrors.confirmPassword = 'Please confirm your new password';
    } else if (passwordForm.newPassword !== passwordForm.confirmPassword) {
      newErrors.confirmPassword = 'Passwords do not match';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleChangePassword = async (e: React.FormEvent) => {
    e.preventDefault();
    setSuccess('');

    if (!validatePasswordForm()) {
      return;
    }

    setIsChangingPassword(true);
    try {
      const response = await fetch('/api/ChangePassword/change', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${user?.accessToken}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          currentPassword: passwordForm.currentPassword,
          newPassword: passwordForm.newPassword,
          confirmPassword: passwordForm.confirmPassword
        })
      });

      if (response.ok) {
        setSuccess('Password changed successfully!');
        setPasswordForm({
          currentPassword: '',
          newPassword: '',
          confirmPassword: ''
        });
        setShowPasswords({
          current: false,
          new: false,
          confirm: false
        });
      } else {
        const errorData = await response.json();
        setErrors({ submit: errorData.message || 'Failed to change password' });
      }
    } catch (error) {
      setErrors({ submit: 'An error occurred while changing password' });
    } finally {
      setIsChangingPassword(false);
    }
  };

  const togglePasswordVisibility = (field: 'current' | 'new' | 'confirm') => {
    setShowPasswords(prev => ({ ...prev, [field]: !prev[field] }));
  };

  return (
    <div className="space-y-6">
      <div className="space-y-4">
        <h3 className="text-lg font-medium text-white">Change Password</h3>
        <p className="text-sm text-white/70">
          Update your password to keep your account secure. Make sure to use a strong password that you haven't used elsewhere.
        </p>
      </div>

      {success && (
        <div className="bg-green-500/20 border border-green-500/30 rounded-lg p-4">
          <p className="text-green-400 text-sm">{success}</p>
        </div>
      )}

      {errors.submit && (
        <div className="bg-red-500/20 border border-red-500/30 rounded-lg p-4">
          <p className="text-red-400 text-sm">{errors.submit}</p>
        </div>
      )}

      <form onSubmit={handleChangePassword} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-white/80 mb-2">
            Current Password
          </label>
          <div className="relative">
            <input
              type={showPasswords.current ? 'text' : 'password'}
              value={passwordForm.currentPassword}
              onChange={(e) => handlePasswordChange('currentPassword', e.target.value)}
              className={`lg-input pr-10 ${errors.currentPassword ? 'border-red-500' : ''}`}
              placeholder="Enter your current password"
            />
            <button
              type="button"
              onClick={() => togglePasswordVisibility('current')}
              className="absolute inset-y-0 right-0 pr-3 flex items-center text-white/60 hover:text-white"
            >
              {showPasswords.current ? (
                <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.878 9.878L3 3m6.878 6.878L21 21" />
                </svg>
              ) : (
                <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                </svg>
              )}
            </button>
          </div>
          {errors.currentPassword && (
            <p className="text-red-400 text-sm mt-1">{errors.currentPassword}</p>
          )}
        </div>

        <div>
          <label className="block text-sm font-medium text-white/80 mb-2">
            New Password
          </label>
          <div className="relative">
            <input
              type={showPasswords.new ? 'text' : 'password'}
              value={passwordForm.newPassword}
              onChange={(e) => handlePasswordChange('newPassword', e.target.value)}
              className={`lg-input pr-10 ${errors.newPassword ? 'border-red-500' : ''}`}
              placeholder="Enter your new password"
            />
            <button
              type="button"
              onClick={() => togglePasswordVisibility('new')}
              className="absolute inset-y-0 right-0 pr-3 flex items-center text-white/60 hover:text-white"
            >
              {showPasswords.new ? (
                <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.878 9.878L3 3m6.878 6.878L21 21" />
                </svg>
              ) : (
                <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                </svg>
              )}
            </button>
          </div>
          {errors.newPassword && (
            <p className="text-red-400 text-sm mt-1">{errors.newPassword}</p>
          )}
        </div>

        <div>
          <label className="block text-sm font-medium text-white/80 mb-2">
            Confirm New Password
          </label>
          <div className="relative">
            <input
              type={showPasswords.confirm ? 'text' : 'password'}
              value={passwordForm.confirmPassword}
              onChange={(e) => handlePasswordChange('confirmPassword', e.target.value)}
              className={`lg-input pr-10 ${errors.confirmPassword ? 'border-red-500' : ''}`}
              placeholder="Confirm your new password"
            />
            <button
              type="button"
              onClick={() => togglePasswordVisibility('confirm')}
              className="absolute inset-y-0 right-0 pr-3 flex items-center text-white/60 hover:text-white"
            >
              {showPasswords.confirm ? (
                <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.878 9.878L3 3m6.878 6.878L21 21" />
                </svg>
              ) : (
                <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                </svg>
              )}
            </button>
          </div>
          {errors.confirmPassword && (
            <p className="text-red-400 text-sm mt-1">{errors.confirmPassword}</p>
          )}
        </div>

        <div className="pt-4">
          <button
            type="submit"
            disabled={isChangingPassword}
            className="lg-button lg-button-primary w-full"
          >
            {isChangingPassword ? 'Changing Password...' : 'Change Password'}
          </button>
        </div>
      </form>

      <div className="border-t border-white/10 pt-6">
        <div className="space-y-4">
          <h4 className="text-md font-medium text-white">Security Tips</h4>
          <ul className="text-sm text-white/70 space-y-2">
            <li>• Use a strong password with at least 8 characters</li>
            <li>• Include a mix of uppercase, lowercase, numbers, and symbols</li>
            <li>• Don't reuse passwords from other accounts</li>
            <li>• Consider using a password manager</li>
          </ul>
        </div>
      </div>

      <div className="border-t border-white/10 pt-6">
        <div className="space-y-4">
          <h4 className="text-md font-medium text-white">Two-Factor Authentication</h4>
          <TwoFactorAuth />
        </div>
      </div>
    </div>
  );
};

export default UserSettings; 