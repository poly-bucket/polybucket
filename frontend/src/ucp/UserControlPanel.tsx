import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppSelector, useAppDispatch } from '../utils/hooks';
import { 
  Person, 
  Security, 
  Folder, 
  Upload,
  Settings,
  Visibility,
  VisibilityOff,
  GridView
} from '@mui/icons-material';
import UserAvatar from './UserAvatar';
import NavigationBar from '../components/common/NavigationBar';
import AvatarRegeneration from './AvatarRegeneration';
import { PrivacySettings } from '../services/api.client';
import { useUserSettings } from '../context/UserSettingsContext';
import { updateUserDetails } from '../store/slices/authSlice';
import TwoFactorAuth from '../components/auth/TwoFactorAuth';
import {
  AccountPreferencesTab,
  CollectionSettingsTab,
  ModelUploadSettingsTab,
  DashboardLayoutSettingsTab
} from './tabs';

interface UserSettings {
  language: string;
  theme: string;
  emailNotifications: boolean;
  defaultPrinterId: string;
  measurementSystem: string;
  timeZone: string;
  autoRotateModels: boolean;
  autoSaveInterval: number;
  showAdvancedOptions: boolean;
  defaultPrivacy: PrivacySettings;
  notificationSound: boolean;
}

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`user-tabpanel-${index}`}
      aria-labelledby={`user-tab-${index}`}
      {...other}
    >
      {value === index && (
        <div>
          {children}
        </div>
      )}
    </div>
  );
}

const UserControlPanel: React.FC = () => {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { user } = useAppSelector((state) => state.auth);
  const { settings, loading, updateSettings } = useUserSettings();
  const [tabValue, setTabValue] = useState(0);
  
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

  const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

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
      id: 'account',
      label: 'Account Preferences',
      icon: Person,
      component: <AccountPreferencesTab />
    },
    {
      id: 'security',
      label: 'Security',
      icon: Security,
      component: <SecurityTab />
    },
    {
      id: 'collections',
      label: 'Collection Settings',
      icon: Folder,
      component: <CollectionSettingsTab />
    },
    {
      id: 'uploads',
      label: 'Model Upload Settings',
      icon: Upload,
      component: <ModelUploadSettingsTab />
    },
    {
      id: 'dashboard-layout',
      label: 'Dashboard Layout',
      icon: GridView,
      component: <DashboardLayoutSettingsTab />
    }
  ];

  if (loading) {
    return (
      <div className="lg-container min-h-screen flex flex-col">
        <NavigationBar
          title="User Control Panel"
          icon={<Settings className="w-6 h-6" />}
          showSearch={false}
          showUploadButton={false}
          showHomeLink={true}
        />
        <div className="flex-1 pt-20">
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
      </div>
    );
  }

  return (
    <div className="lg-container min-h-screen flex flex-col">
      {/* Navigation Bar */}
      <NavigationBar
        title="User Control Panel"
        icon={<Settings className="w-6 h-6" />}
        description=""
        showSearch={true}
        showUploadButton={true}
        showHomeLink={true}
      />

      {/* Main Content - Padding for fixed navbar */}
      <div className="flex-1 pt-20">

        {/* Navigation Tabs */}
        <div className="lg-tabs">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <nav className="flex space-x-8">
              {tabs.map((tab, index) => {
                const Icon = tab.icon;
                return (
                  <button
                    key={tab.id}
                    onClick={(e) => handleTabChange(e, index)}
                    className={`lg-tab-button ${
                      tabValue === index ? 'active' : ''
                    }`}
                  >
                    <Icon className="w-5 h-5" />
                    {tab.label}
                  </button>
                );
              })}
            </nav>
          </div>
        </div>

        {/* Tab Content */}
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          {tabs.map((tab, index) => (
            <TabPanel key={index} value={tabValue} index={index}>
              <div className="lg-card">
                <div className="p-6">
                  {tab.component}
                </div>
              </div>
            </TabPanel>
          ))}
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

export default UserControlPanel;
