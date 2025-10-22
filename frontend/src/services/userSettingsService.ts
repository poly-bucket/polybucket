import { UserSettings } from './api.client';
import api from '../utils/axiosConfig';

class UserSettingsService {
  async getUserSettings(accessToken: string): Promise<UserSettings | null> {
    try {
      const response = await api.get('/users/settings');
      return response.data.settings || null;
    } catch (error) {
      console.error('Error fetching user settings:', error);
      return null;
    }
  }

  async updateUserSettings(
    userId: string, 
    settings: Partial<UserSettings>, 
    accessToken: string
  ): Promise<boolean> {
    try {
      const request = {
        userId,
        language: settings.language,
        theme: settings.theme,
        emailNotifications: settings.emailNotifications,
        defaultPrinterId: settings.defaultPrinterId,
        measurementSystem: settings.measurementSystem,
        timeZone: settings.timeZone,
        autoRotateModels: settings.autoRotateModels,
        dashboardViewType: settings.dashboardViewType,
        cardSize: settings.cardSize,
        cardSpacing: settings.cardSpacing,
        gridColumns: settings.gridColumns,
        customSettings: settings.customSettings
      };

      const response = await api.put('/users/settings', request);
      return response.status >= 200 && response.status < 300;
    } catch (error) {
      console.error('Error updating user settings:', error);
      return false;
    }
  }

  async updateAutoRotateSetting(userId: string, autoRotate: boolean, accessToken: string): Promise<boolean> {
    try {
      const request = {
        userId,
        autoRotateModels: autoRotate
      };

      const response = await api.put('/users/settings', request);
      return response.status >= 200 && response.status < 300;
    } catch (error) {
      console.error('Error updating auto-rotate setting:', error);
      return false;
    }
  }

  getDefaultSettings() {
    return {
      language: 'en',
      theme: 'dark',
      emailNotifications: true,
      measurementSystem: 'metric',
      timeZone: 'UTC',
      autoRotateModels: true,
      dashboardViewType: 'grid',
      cardSize: 'medium',
      cardSpacing: 'normal',
      gridColumns: 4,
      customSettings: {}
    };
  }
}

const userSettingsService = new UserSettingsService();
export default userSettingsService; 