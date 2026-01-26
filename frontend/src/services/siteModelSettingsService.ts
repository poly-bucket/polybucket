import { ApiClientFactory } from '../api/clientFactory';
import {
  UpdateSiteModelSettingsCommand,
  GetSiteModelSettingsResponse,
  UpdateSiteModelSettingsResponse,
  SiteModelSettingsData
} from '../api/client';

export type SiteModelSettings = SiteModelSettingsData;

const api = () => ApiClientFactory.getApiClient();

class SiteModelSettingsService {
  async getSettings(): Promise<GetSiteModelSettingsResponse> {
    try {
      return await api().getSiteModelSettings_GetSiteModelSettings();
    } catch (error) {
      console.error('Error fetching site model settings:', error);
      throw error;
    }
  }

  async updateSettings(settings: SiteModelSettings): Promise<UpdateSiteModelSettingsResponse> {
    try {
      const command = new UpdateSiteModelSettingsCommand(settings as any);
      return await api().updateSiteModelSettings_UpdateSiteModelSettings(command);
    } catch (error) {
      console.error('Error updating site model settings:', error);
      throw error;
    }
  }
}

const siteModelSettingsService = new SiteModelSettingsService();
export default siteModelSettingsService;
