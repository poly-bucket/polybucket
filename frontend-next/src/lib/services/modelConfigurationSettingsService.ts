import { ApiClientFactory } from "@/lib/api/clientFactory";
import type {
  GetModelConfigurationSettingsResponse,
  ModelConfigurationSettingsData,
} from "@/lib/api/client";

export type { ModelConfigurationSettingsData };

const client = () => ApiClientFactory.getApiClient();

export async function getModelConfigurationSettings(): Promise<GetModelConfigurationSettingsResponse> {
  return client().getModelConfigurationSettings_GetModelConfigurationSettings();
}
