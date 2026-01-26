import { ApiClientFactory } from '../api/clientFactory';
import { UserSettings, UpdateUserSettingsRequest } from '../api/client';

const api = () => ApiClientFactory.getApiClient();

class UserSettingsService {
  async getUserSettings(accessToken: string): Promise<UserSettings | null> {
    try {
      const response = await api().getUserSettings_GetUserSettings();
      return (response as any) || null;
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
      const request = new UpdateUserSettingsRequest({
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
      await api().updateUserSettings_UpdateUserSettings(request);
      return true;
    } catch (error) {
      console.error('Error updating user settings:', error);
      return false;
    }
  }

  async updateAutoRotateSetting(userId: string, autoRotate: boolean, accessToken: string): Promise<boolean> {
    try {
      const request = new UpdateUserSettingsRequest({ autoRotateModels: autoRotate });
      await api().updateUserSettings_UpdateUserSettings(request);
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
