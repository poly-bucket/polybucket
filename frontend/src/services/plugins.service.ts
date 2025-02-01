import api from './api';

export interface Plugin {
  id: string;
  name: string;
  version: string;
  author: string;
  description: string;
}

class PluginsService {
  async getPlugins(): Promise<Plugin[]> {
    const response = await api.get<Plugin[]>('/plugins');
    return response.data;
  }

  async reloadPlugins(): Promise<void> {
    await api.post('/plugins/reload');
  }
}

export const pluginsService = new PluginsService(); 