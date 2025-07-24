import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { useAuth } from './AuthContext';
import userSettingsService from '../services/userSettingsService';

interface UserSettingsData {
  language: string;
  theme: string;
  emailNotifications: boolean;
  defaultPrinterId?: string;
  measurementSystem: string;
  timeZone: string;
  autoRotateModels: boolean;
  customSettings?: { [key: string]: string };
}

interface UserSettingsContextType {
  settings: UserSettingsData | null;
  loading: boolean;
  updateSettings: (newSettings: Partial<UserSettingsData>) => Promise<boolean>;
  updateAutoRotate: (autoRotate: boolean) => Promise<boolean>;
  refreshSettings: () => Promise<void>;
}

const UserSettingsContext = createContext<UserSettingsContextType | undefined>(undefined);

interface UserSettingsProviderProps {
  children: ReactNode;
}

export const UserSettingsProvider: React.FC<UserSettingsProviderProps> = ({ children }) => {
  const { user } = useAuth();
  const [settings, setSettings] = useState<UserSettingsData | null>(null);
  const [loading, setLoading] = useState(false);

  const loadSettings = async () => {
    if (!user?.accessToken) {
      setSettings(null);
      return;
    }

    setLoading(true);
    try {
      const userSettings = await userSettingsService.getUserSettings(user.accessToken);
      if (userSettings) {
        setSettings({
          language: userSettings.language || 'en',
          theme: userSettings.theme || 'dark',
          emailNotifications: userSettings.emailNotifications ?? true,
          defaultPrinterId: userSettings.defaultPrinterId,
          measurementSystem: userSettings.measurementSystem || 'metric',
          timeZone: userSettings.timeZone || 'UTC',
          autoRotateModels: userSettings.autoRotateModels ?? true,
          customSettings: userSettings.customSettings
        });
      } else {
        // Use default settings if none exist
        setSettings(userSettingsService.getDefaultSettings());
      }
    } catch (error) {
      console.error('Error loading user settings:', error);
      // Use default settings on error
      setSettings(userSettingsService.getDefaultSettings());
    } finally {
      setLoading(false);
    }
  };

  const updateSettings = async (newSettings: Partial<UserSettingsData>): Promise<boolean> => {
    if (!user?.accessToken || !user.id) {
      return false;
    }

    try {
      const success = await userSettingsService.updateUserSettings(
        user.id,
        newSettings,
        user.accessToken
      );
      
      if (success && settings) {
        setSettings({ ...settings, ...newSettings });
      }
      
      return success;
    } catch (error) {
      console.error('Error updating user settings:', error);
      return false;
    }
  };

  const updateAutoRotate = async (autoRotate: boolean): Promise<boolean> => {
    if (!user?.accessToken || !user.id) {
      return false;
    }

    try {
      const success = await userSettingsService.updateAutoRotateSetting(
        user.id,
        autoRotate,
        user.accessToken
      );
      
      if (success && settings) {
        setSettings({ ...settings, autoRotateModels: autoRotate });
      }
      
      return success;
    } catch (error) {
      console.error('Error updating auto-rotate setting:', error);
      return false;
    }
  };

  const refreshSettings = async () => {
    await loadSettings();
  };

  useEffect(() => {
    loadSettings();
  }, [user?.accessToken, user?.id]);

  const value: UserSettingsContextType = {
    settings,
    loading,
    updateSettings,
    updateAutoRotate,
    refreshSettings
  };

  return (
    <UserSettingsContext.Provider value={value}>
      {children}
    </UserSettingsContext.Provider>
  );
};

export const useUserSettings = (): UserSettingsContextType => {
  const context = useContext(UserSettingsContext);
  if (context === undefined) {
    throw new Error('useUserSettings must be used within a UserSettingsProvider');
  }
  return context;
}; 