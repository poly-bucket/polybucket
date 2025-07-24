import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppSelector, useAppDispatch } from '../utils/hooks';
import UserAvatar from '../components/UserAvatar';
import NavigationBar from '../components/common/NavigationBar';
import AvatarRegeneration from '../components/AvatarRegeneration';
import { PrivacySettings } from '../services/api.client';
import { useUserSettings } from '../context/UserSettingsContext';
import { updateUserDetails } from '../store/slices/authSlice';

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
      const success = await updateSettings({
        language: localSettings.language,
        theme: localSettings.theme,
        emailNotifications: localSettings.emailNotifications,
        defaultPrinterId: localSettings.defaultPrinterId,
        measurementSystem: localSettings.measurementSystem,
        timeZone: localSettings.timeZone,
        autoRotateModels: localSettings.autoRotateModels,
      });
      
      if (success) {
        setIsEditing(false);
      } else {
        console.error('Failed to save settings');
      }
    } catch (error) {
      console.error('Error saving settings:', error);
    } finally {
      setIsSaving(false);
    }
  };

  const handleCancel = () => {
    setIsEditing(false);
    // In real implementation, this would revert to original settings
  };

  const handleAvatarUpdate = (newAvatar: string) => {
    // Update the user's avatar in Redux store
    dispatch(updateUserDetails({ avatar: newAvatar }));
  };

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
              <h2 className="text-lg font-medium text-white">Account Preferences</h2>
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

          {/* Settings Content */}
          <div className="p-6 space-y-6">
            
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

                <div>
                  <label className="block text-sm font-medium text-white/80 mb-2">
                    Measurement System
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
            </div>

            {/* Appearance */}
            <div className="space-y-4">
              <h3 className="text-lg font-medium text-white">Appearance</h3>
              
              <div>
                <label className="block text-sm font-medium text-white/80 mb-2">
                  Theme
                </label>
                <div className="flex space-x-4">
                  {['light', 'dark', 'auto'].map(theme => (
                    <label key={theme} className="flex items-center">
                      <input
                        type="radio"
                        name="theme"
                        value={theme}
                        checked={localSettings.theme === theme}
                        onChange={(e) => handleSettingChange('theme', e.target.value)}
                        disabled={!isEditing}
                        className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 bg-transparent disabled:text-white/40"
                      />
                      <span className="ml-2 text-sm text-white/80 capitalize">{theme}</span>
                    </label>
                  ))}
                </div>
              </div>

              <div className="flex items-center">
                <input
                  type="checkbox"
                  id="showAdvancedOptions"
                  checked={localSettings.showAdvancedOptions}
                  onChange={(e) => handleSettingChange('showAdvancedOptions', e.target.checked)}
                  disabled={!isEditing}
                  className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent disabled:text-white/40"
                />
                <label htmlFor="showAdvancedOptions" className="ml-2 text-sm text-white/80">
                  Show advanced options in the interface
                </label>
              </div>

              <div className="flex items-center">
                <input
                  type="checkbox"
                  id="autoRotateModels"
                  checked={localSettings.autoRotateModels}
                  onChange={(e) => handleSettingChange('autoRotateModels', e.target.checked)}
                  disabled={!isEditing}
                  className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent disabled:text-white/40"
                />
                <label htmlFor="autoRotateModels" className="ml-2 text-sm text-white/80">
                  Auto-rotate models in preview
                </label>
              </div>
              <p className="text-xs text-white/60 ml-6">
                When enabled, 3D models will automatically rotate in the dashboard preview
              </p>
            </div>

            {/* Printing */}
            <div className="space-y-4">
              <h3 className="text-lg font-medium text-white">3D Printing</h3>
              
              <div>
                <label className="block text-sm font-medium text-white/80 mb-2">
                  Default Printer
                </label>
                <select
                  value={localSettings.defaultPrinterId}
                  onChange={(e) => handleSettingChange('defaultPrinterId', e.target.value)}
                  disabled={!isEditing}
                  className="lg-input max-w-md disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {printerOptions.map(printer => (
                    <option key={printer.id} value={printer.id}>
                      {printer.name}
                    </option>
                  ))}
                </select>
                <p className="mt-1 text-sm text-white/60">
                  This printer will be pre-selected when uploading models
                </p>
              </div>

              <div>
                <label className="block text-sm font-medium text-white/80 mb-2">
                  Default Privacy Level for Uploads
                </label>
                <select
                  value={localSettings.defaultPrivacy}
                  onChange={(e) => handleSettingChange('defaultPrivacy', Number(e.target.value) as PrivacySettings)}
                  disabled={!isEditing}
                  className="lg-input max-w-md disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  <option value={PrivacySettings.Public}>Public</option>
                  <option value={PrivacySettings.Unlisted}>Unlisted</option>
                  <option value={PrivacySettings.Private}>Private</option>
                </select>
              </div>
            </div>

            {/* Notifications */}
            <div className="space-y-4">
              <h3 className="text-lg font-medium text-white">Notifications</h3>
              
              <div className="space-y-3">
                <div className="flex items-center">
                  <input
                    type="checkbox"
                    id="emailNotifications"
                    checked={localSettings.emailNotifications}
                    onChange={(e) => handleSettingChange('emailNotifications', e.target.checked)}
                    disabled={!isEditing}
                    className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent disabled:text-white/40"
                  />
                  <label htmlFor="emailNotifications" className="ml-2 text-sm text-white/80">
                    Email notifications for comments, likes, and follows
                  </label>
                </div>

                <div className="flex items-center">
                  <input
                    type="checkbox"
                    id="notificationSound"
                    checked={localSettings.notificationSound}
                    onChange={(e) => handleSettingChange('notificationSound', e.target.checked)}
                    disabled={!isEditing}
                    className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent disabled:text-white/40"
                  />
                  <label htmlFor="notificationSound" className="ml-2 text-sm text-white/80">
                    Play notification sounds
                  </label>
                </div>
              </div>
            </div>

            {/* Editor */}
            <div className="space-y-4">
              <h3 className="text-lg font-medium text-white">Editor Preferences</h3>
              
              <div>
                <label className="block text-sm font-medium text-white/80 mb-2">
                  Auto-save interval (minutes)
                </label>
                <select
                                      value={localSettings.autoSaveInterval}
                    onChange={(e) => handleSettingChange('autoSaveInterval', parseInt(e.target.value))}
                  disabled={!isEditing}
                  className="lg-input max-w-md disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  <option value={1}>1 minute</option>
                  <option value={5}>5 minutes</option>
                  <option value={10}>10 minutes</option>
                  <option value={15}>15 minutes</option>
                  <option value={0}>Disabled</option>
                </select>
              </div>
            </div>

          </div>
        </div>
      </div>
    </div>
  );
};

export default UserSettings; 