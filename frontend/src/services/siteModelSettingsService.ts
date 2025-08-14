import { 
  GetSiteModelSettingsClient, 
  UpdateSiteModelSettingsClient, 
  UpdateSiteModelSettingsCommand,
  GetSiteModelSettingsResponse,
  UpdateSiteModelSettingsResponse,
  SiteModelSettingsData
} from './api.client';

export type SiteModelSettings = SiteModelSettingsData;

class SiteModelSettingsService {
  private getSiteModelSettingsClient(): GetSiteModelSettingsClient {
    return new GetSiteModelSettingsClient();
  }

  private getUpdateSiteModelSettingsClient(): UpdateSiteModelSettingsClient {
    return new UpdateSiteModelSettingsClient();
  }

  async getSettings(): Promise<GetSiteModelSettingsResponse> {
    try {
      const client = this.getSiteModelSettingsClient();
      return await client.getSiteModelSettings();
    } catch (error) {
      console.error('Error fetching site model settings:', error);
      throw error;
    }
  }

  async updateSettings(settings: SiteModelSettings): Promise<UpdateSiteModelSettingsResponse> {
    try {
      const client = this.getUpdateSiteModelSettingsClient();
      const command = new UpdateSiteModelSettingsCommand();
      
      // Map the settings to the command
      command.maxFileSizeBytes = settings.maxFileSizeBytes;
      command.allowedFileTypes = settings.allowedFileTypes;
      command.maxFilesPerUpload = settings.maxFilesPerUpload;
      command.enableFileCompression = settings.enableFileCompression;
      command.autoGeneratePreviews = settings.autoGeneratePreviews;
      command.defaultModelPrivacy = settings.defaultModelPrivacy;
      command.autoApproveModels = settings.autoApproveModels;
      command.requireModeration = settings.requireModeration;
      command.requireLoginForUpload = settings.requireLoginForUpload;
      command.allowPublicBrowsing = settings.allowPublicBrowsing;

      return await client.updateSiteModelSettings(command);
    } catch (error) {
      console.error('Error updating site model settings:', error);
      throw error;
    }
  }
}

const siteModelSettingsService = new SiteModelSettingsService();
export default siteModelSettingsService;
