import { UserSettings, UpdateUserSettingsRequest } from './api.client';
import { API_CONFIG } from '../api/config';
import { AxiosHttpClient } from '../api/axiosAdapter';
import { GetUserSettingsClient, UpdateUserSettingsClient } from './api.client';

const sharedHttpClient = new AxiosHttpClient(API_CONFIG.baseUrl);

class UserSettingsService {
  async getUserSettings(accessToken: string): Promise<UserSettings | null> {
    try {
      const client = new GetUserSettingsClient(API_CONFIG.baseUrl, sharedHttpClient);
      const response = await client.getUserSettings();
      return response || null;
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
      const client = new UpdateUserSettingsClient(API_CONFIG.baseUrl, sharedHttpClient);
      const request = new UpdateUserSettingsRequest({
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
      });

      await client.updateUserSettings(request);
      return true;
    } catch (error) {
      console.error('Error updating user settings:', error);
      return false;
    }
  }

  async updateAutoRotateSetting(userId: string, autoRotate: boolean, accessToken: string): Promise<boolean> {
    try {
      const client = new UpdateUserSettingsClient(API_CONFIG.baseUrl, sharedHttpClient);
      const request = new UpdateUserSettingsRequest({
        userId,
        autoRotateModels: autoRotate
      });

      await client.updateUserSettings(request);
      return true;
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
