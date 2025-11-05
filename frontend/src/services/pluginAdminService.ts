import { API_CONFIG } from '../api/config';
import { AxiosHttpClient } from '../api/axiosAdapter';
import {
  GetPluginsClient,
  GetPluginDetailsClient,
  ReloadPluginsClient,
  UpdatePluginSettingsClient,
  UpdatePluginSettingsRequest
} from './api.client';
import api from '../utils/axiosConfig';

const sharedHttpClient = new AxiosHttpClient(API_CONFIG.baseUrl);

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

export interface UpdatePluginSettingsRequest {
  settings: Record<string, any>;
}

export interface UpdatePluginSettingsResponse {
  success: boolean;
  message: string;
  updatedSettings: Record<string, any>;
  validationErrors: string[];
  updatedAt: string;
}

export interface UpdatePluginStatusRequest {
  enabled: boolean;
}

export interface PluginStatusResponse {
  success: boolean;
  message: string;
  currentStatus: string;
  updatedAt: string;
}

class PluginAdminService {
  // SKIPPED: /api/plugins/overview endpoint not found in generated client
  async getPluginsOverview(): Promise<PluginOverview> {
    const response = await api.get('/api/plugins/overview');
    return response.data;
  }

  async getPluginDetails(pluginId: string): Promise<PluginDetails> {
    const client = new GetPluginDetailsClient(API_CONFIG.baseUrl, sharedHttpClient);
    const response = await client.getPluginDetails(pluginId);
    return response as any as PluginDetails;
  }

  // SKIPPED: /api/plugins/hooks endpoint not found in generated client
  async getPluginHooks(): Promise<PluginHooksResponse> {
    const response = await api.get('/api/plugins/hooks');
    return response.data;
  }

  // SKIPPED: GET /api/plugins/{pluginId}/settings endpoint not found in generated client
  async getPluginSettings(pluginId: string): Promise<PluginSettingsResponse> {
    const response = await api.get(`/api/plugins/${pluginId}/settings`);
    return response.data;
  }

  async updatePluginSettings(
    pluginId: string,
    settings: Record<string, any>
  ): Promise<UpdatePluginSettingsResponse> {
    const client = new UpdatePluginSettingsClient(API_CONFIG.baseUrl, sharedHttpClient);
    const request = new UpdatePluginSettingsRequest({
      settings: settings
    });
    const response = await client.updatePluginSettings(pluginId, request);
    return response;
  }

  // SKIPPED: /api/plugins/{pluginId}/status endpoint not found in generated client
  async updatePluginStatus(
    pluginId: string,
    enabled: boolean
  ): Promise<PluginStatusResponse> {
    const response = await api.put(`/api/plugins/${pluginId}/status`, {
      enabled
    });
    return response.data;
  }

  async reloadPlugins(): Promise<void> {
    const client = new ReloadPluginsClient(API_CONFIG.baseUrl, sharedHttpClient);
    await client.reloadPlugins();
  }

  async getPlugins(): Promise<any[]> {
    const client = new GetPluginsClient(API_CONFIG.baseUrl, sharedHttpClient);
    const response = await client.getPlugins();
    return response as any as any[];
  }
}

export const pluginAdminService = new PluginAdminService();
export default pluginAdminService;
