import { ApiClientFactory } from '../api/clientFactory';
import {
  UpdateModelConfigurationSettingsCommand,
  GetModelConfigurationSettingsResponse,
  UpdateModelConfigurationSettingsResponse,
  ModelConfigurationSettingsData
} from '../api/client';

export type ModelConfigurationSettings = ModelConfigurationSettingsData;

const api = () => ApiClientFactory.getApiClient();

class ModelConfigurationSettingsService {
  async getSettings(): Promise<GetModelConfigurationSettingsResponse> {
    try {
      return await api().getModelConfigurationSettings_GetModelConfigurationSettings();
    } catch (error) {
      console.error('Error fetching model configuration settings:', error);
      throw error;
    }
  }

  async updateSettings(settings: ModelConfigurationSettings): Promise<UpdateModelConfigurationSettingsResponse> {
    try {
      const command = new UpdateModelConfigurationSettingsCommand(settings as any);
      return await api().updateModelConfigurationSettings_UpdateModelConfigurationSettings(command);
    } catch (error) {
      console.error('Error updating model configuration settings:', error);
      throw error;
    }
  }
}

const modelConfigurationSettingsService = new ModelConfigurationSettingsService();
export default modelConfigurationSettingsService;
