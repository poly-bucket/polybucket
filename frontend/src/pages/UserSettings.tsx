import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppSelector } from '../utils/hooks';
import UserAvatar from '../components/UserAvatar';

interface UserSettings {
  language: string;
  theme: string;
  emailNotifications: boolean;
  defaultPrinterId: string;
  measurementSystem: string;
  timeZone: string;
  // Additional UI-focused settings for the future
  autoSaveInterval: number;
  showAdvancedOptions: boolean;
  defaultPrivacy: string;
  notificationSound: boolean;
}

const UserSettings: React.FC = () => {
  const navigate = useNavigate();
  const { user } = useAppSelector((state) => state.auth);
  
  // Mock user settings data
  const [settings, setSettings] = useState<UserSettings>({
    language: 'en',
    theme: 'dark',
    emailNotifications: true,
    defaultPrinterId: '',
    measurementSystem: 'metric',
    timeZone: 'UTC',
    autoSaveInterval: 5,
    showAdvancedOptions: false,
    defaultPrivacy: 'public',
    notificationSound: true,
  });

  const [isEditing, setIsEditing] = useState(false);
  const [isSaving, setIsSaving] = useState(false);

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
    setSettings(prev => ({
      ...prev,
      [key]: value
    }));
  };

  const handleSave = async () => {
    setIsSaving(true);
    // Simulate API call
    await new Promise(resolve => setTimeout(resolve, 1000));
    setIsSaving(false);
    setIsEditing(false);
    // In real implementation, this would save to the backend
    console.log('Settings saved:', settings);
  };

  const handleCancel = () => {
    setIsEditing(false);
    // In real implementation, this would revert to original settings
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Navigation */}
      <nav className="bg-white shadow-sm border-b">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center h-16">
            <button
              onClick={() => navigate('/dashboard')}
              className="flex items-center text-gray-600 hover:text-gray-900"
            >
              <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
              </svg>
              Back to Dashboard
            </button>
            <div className="ml-6">
              <h1 className="text-xl font-semibold text-gray-900">User Settings</h1>
            </div>
          </div>
        </div>
      </nav>

      {/* Main Content */}
      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Profile Section */}
        <div className="bg-white shadow rounded-lg mb-6 p-6">
          <div className="flex items-center">
            <UserAvatar 
              username={user?.username || 'User'} 
              profilePictureUrl={user?.profilePictureUrl}
              size="xl"
              className="mr-6"
            />
            <div>
              <h3 className="text-lg font-medium text-gray-900">{user?.username}</h3>
              <p className="text-sm text-gray-600">{user?.email}</p>
              <p className="text-xs text-gray-500 mt-1">
                {user?.profilePictureUrl 
                  ? "Using your uploaded profile picture" 
                  : "Your avatar is automatically generated from your username"
                }
              </p>
            </div>
          </div>
        </div>

        <div className="bg-white shadow rounded-lg">
          {/* Header */}
          <div className="px-6 py-4 border-b border-gray-200">
            <div className="flex items-center justify-between">
              <h2 className="text-lg font-medium text-gray-900">Account Preferences</h2>
              <div className="flex space-x-3">
                {!isEditing ? (
                  <button
                    onClick={() => setIsEditing(true)}
                    className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                  >
                    Edit Settings
                  </button>
                ) : (
                  <>
                    <button
                      onClick={handleCancel}
                      className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                    >
                      Cancel
                    </button>
                    <button
                      onClick={handleSave}
                      disabled={isSaving}
                      className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed"
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
            
            {/* Language & Localization */}
            <div className="space-y-4">
              <h3 className="text-lg font-medium text-gray-900">Language & Region</h3>
              
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Language
                  </label>
                  <select
                    value={settings.language}
                    onChange={(e) => handleSettingChange('language', e.target.value)}
                    disabled={!isEditing}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 disabled:bg-gray-50 disabled:text-gray-500"
                  >
                    {languageOptions.map(lang => (
                      <option key={lang.code} value={lang.code}>
                        {lang.name}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Time Zone
                  </label>
                  <select
                    value={settings.timeZone}
                    onChange={(e) => handleSettingChange('timeZone', e.target.value)}
                    disabled={!isEditing}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 disabled:bg-gray-50 disabled:text-gray-500"
                  >
                    {timeZoneOptions.map(tz => (
                      <option key={tz.code} value={tz.code}>
                        {tz.name}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Measurement System
                  </label>
                  <select
                    value={settings.measurementSystem}
                    onChange={(e) => handleSettingChange('measurementSystem', e.target.value)}
                    disabled={!isEditing}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 disabled:bg-gray-50 disabled:text-gray-500"
                  >
                    <option value="metric">Metric (mm, kg, °C)</option>
                    <option value="imperial">Imperial (in, lb, °F)</option>
                  </select>
                </div>
              </div>
            </div>

            {/* Appearance */}
            <div className="space-y-4">
              <h3 className="text-lg font-medium text-gray-900">Appearance</h3>
              
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Theme
                </label>
                <div className="flex space-x-4">
                  {['light', 'dark', 'auto'].map(theme => (
                    <label key={theme} className="flex items-center">
                      <input
                        type="radio"
                        name="theme"
                        value={theme}
                        checked={settings.theme === theme}
                        onChange={(e) => handleSettingChange('theme', e.target.value)}
                        disabled={!isEditing}
                        className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 disabled:text-gray-400"
                      />
                      <span className="ml-2 text-sm text-gray-700 capitalize">{theme}</span>
                    </label>
                  ))}
                </div>
              </div>

              <div className="flex items-center">
                <input
                  type="checkbox"
                  id="showAdvancedOptions"
                  checked={settings.showAdvancedOptions}
                  onChange={(e) => handleSettingChange('showAdvancedOptions', e.target.checked)}
                  disabled={!isEditing}
                  className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded disabled:text-gray-400"
                />
                <label htmlFor="showAdvancedOptions" className="ml-2 text-sm text-gray-700">
                  Show advanced options in the interface
                </label>
              </div>
            </div>

            {/* Printing */}
            <div className="space-y-4">
              <h3 className="text-lg font-medium text-gray-900">3D Printing</h3>
              
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Default Printer
                </label>
                <select
                  value={settings.defaultPrinterId}
                  onChange={(e) => handleSettingChange('defaultPrinterId', e.target.value)}
                  disabled={!isEditing}
                  className="w-full max-w-md px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 disabled:bg-gray-50 disabled:text-gray-500"
                >
                  {printerOptions.map(printer => (
                    <option key={printer.id} value={printer.id}>
                      {printer.name}
                    </option>
                  ))}
                </select>
                <p className="mt-1 text-sm text-gray-500">
                  This printer will be pre-selected when uploading models
                </p>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Default Privacy Level for Uploads
                </label>
                <select
                  value={settings.defaultPrivacy}
                  onChange={(e) => handleSettingChange('defaultPrivacy', e.target.value)}
                  disabled={!isEditing}
                  className="w-full max-w-md px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 disabled:bg-gray-50 disabled:text-gray-500"
                >
                  <option value="public">Public</option>
                  <option value="unlisted">Unlisted</option>
                  <option value="private">Private</option>
                </select>
              </div>
            </div>

            {/* Notifications */}
            <div className="space-y-4">
              <h3 className="text-lg font-medium text-gray-900">Notifications</h3>
              
              <div className="space-y-3">
                <div className="flex items-center">
                  <input
                    type="checkbox"
                    id="emailNotifications"
                    checked={settings.emailNotifications}
                    onChange={(e) => handleSettingChange('emailNotifications', e.target.checked)}
                    disabled={!isEditing}
                    className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded disabled:text-gray-400"
                  />
                  <label htmlFor="emailNotifications" className="ml-2 text-sm text-gray-700">
                    Email notifications for comments, likes, and follows
                  </label>
                </div>

                <div className="flex items-center">
                  <input
                    type="checkbox"
                    id="notificationSound"
                    checked={settings.notificationSound}
                    onChange={(e) => handleSettingChange('notificationSound', e.target.checked)}
                    disabled={!isEditing}
                    className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded disabled:text-gray-400"
                  />
                  <label htmlFor="notificationSound" className="ml-2 text-sm text-gray-700">
                    Play notification sounds
                  </label>
                </div>
              </div>
            </div>

            {/* Editor */}
            <div className="space-y-4">
              <h3 className="text-lg font-medium text-gray-900">Editor Preferences</h3>
              
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Auto-save interval (minutes)
                </label>
                <select
                  value={settings.autoSaveInterval}
                  onChange={(e) => handleSettingChange('autoSaveInterval', parseInt(e.target.value))}
                  disabled={!isEditing}
                  className="w-full max-w-md px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 disabled:bg-gray-50 disabled:text-gray-500"
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