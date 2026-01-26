import { ApiClientFactory } from '../api/clientFactory';
import { UpdatePluginSettingsRequest, UpdatePluginStatusRequest } from '../api/client';

const api = () => ApiClientFactory.getApiClient();

export interface PluginSummary {
  id: string;
  name: string;
  version: string;
  author: string;
  description: string;
  isLoaded: boolean;
  status: string;
  componentCount: number;
  hookCount: number;
  settingsCount: number;
  hasPermissions: boolean;
}

export interface PluginOverview {
  totalPlugins: number;
  activePlugins: number;
  pluginSummaries: PluginSummary[];
  lastRefresh: string;
}

export interface PluginComponent {
  id: string;
  name: string;
  componentPath: string;
  type: string;
  props: Record<string, any>;
  dependencies: string[];
  hooks: PluginHook[];
}

export interface PluginHook {
  hookName: string;
  componentId: string;
  priority: number;
  config: Record<string, any>;
}

export interface PluginMetadata {
  minimumAppVersion: string;
  maximumAppVersion: string;
  requiredPermissions: string[];
  optionalPermissions: string[];
  settings: Record<string, PluginSetting>;
  dependencies: string[];
  lifecycle: PluginLifecycle;
}

export interface PluginSetting {
  name: string;
  description: string;
  type: 'String' | 'Number' | 'Boolean' | 'Select' | 'MultiSelect' | 'Json';
  defaultValue: any;
  required: boolean;
  options: string[];
}

export interface PluginLifecycle {
  autoStart: boolean;
  canDisable: boolean;
  canUninstall: boolean;
  startupTasks: string[];
  shutdownTasks: string[];
}

export interface PluginDetails {
  id: string;
  name: string;
  version: string;
  author: string;
  description: string;
  isLoaded: boolean;
  metadata: PluginMetadata;
  components: PluginComponent[];
  loadedAt: string;
  status: string;
  dependencies: string[];
  hooks: PluginHook[];
  settings: PluginSetting[];
}

export interface PluginHookInfo {
  pluginId: string;
  pluginName: string;
  hookName: string;
  priority: number;
  configuration: Record<string, any>;
}

export interface PluginHooksResponse {
  extensionPoints: Record<string, PluginHookInfo[]>;
  totalExtensionPoints: number;
  totalHooks: number;
}

export interface PluginSettingSchema {
  type: string;
  description: string;
  required: boolean;
  defaultValue: any;
  options: string[];
}

export interface PluginSettingsResponse {
  pluginId: string;
  pluginName: string;
  settings: Record<string, PluginSetting>;
  currentValues: Record<string, any>;
  schema: Record<string, PluginSettingSchema>;
}

export interface UpdatePluginSettingsResponse {
  success: boolean;
  message: string;
  updatedSettings: Record<string, any>;
  validationErrors: string[];
  updatedAt: string;
}

export interface PluginStatusResponse {
  success: boolean;
  message: string;
  currentStatus: string;
  updatedAt: string;
}

class PluginAdminService {
  async getPluginsOverview(): Promise<PluginOverview> {
    const response = await api().getPluginDetails_GetPluginsOverview();
    return response as any as PluginOverview;
  }

  async getPluginDetails(pluginId: string): Promise<PluginDetails> {
    const response = await api().getPluginDetails_GetPluginDetails(pluginId);
    return response as any as PluginDetails;
  }

  async getPluginHooks(): Promise<PluginHooksResponse> {
    const response = await api().getPluginDetails_GetPluginHooks();
    return response as any as PluginHooksResponse;
  }

  async getPluginSettings(pluginId: string): Promise<PluginSettingsResponse> {
    const response = await api().updatePluginSettings_GetPluginSettings(pluginId);
    return response as any as PluginSettingsResponse;
  }

  async updatePluginSettings(
    pluginId: string,
    settings: Record<string, any>
  ): Promise<UpdatePluginSettingsResponse> {
    const request = new UpdatePluginSettingsRequest({ settings });
    const response = await api().updatePluginSettings_UpdatePluginSettings(pluginId, request);
    return response as any as UpdatePluginSettingsResponse;
  }

  async updatePluginStatus(pluginId: string, enabled: boolean): Promise<PluginStatusResponse> {
    const request = new UpdatePluginStatusRequest({ enabled });
    const response = await api().updatePluginSettings_UpdatePluginStatus(pluginId, request);
    return response as any as PluginStatusResponse;
  }

  async reloadPlugins(): Promise<void> {
    await api().reloadPlugins_ReloadPlugins();
  }

  async getPlugins(): Promise<any[]> {
    const response = await api().getPlugins_GetPlugins();
    return response as any as any[];
  }
}

export const pluginAdminService = new PluginAdminService();
export default pluginAdminService;
