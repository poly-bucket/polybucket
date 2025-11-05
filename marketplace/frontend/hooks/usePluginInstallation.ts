import { useState, useCallback } from 'react';
import { apiClient } from '@/lib/api';
import { 
  InstallationState, 
  InstallationStatus, 
  PluginInstallationRequest,
  PluginDownloadInfo 
} from '@/types/plugin';

interface UsePluginInstallationOptions {
  onSuccess?: (downloadUrl: string, version: string) => void;
  onError?: (error: string) => void;
  onProgress?: (progress: number) => void;
}

export function usePluginInstallation(options: UsePluginInstallationOptions = {}) {
  const [state, setState] = useState<InstallationState>({
    status: 'idle',
    progress: 0,
  });

  const updateState = useCallback((updates: Partial<InstallationState>) => {
    setState(prev => ({ ...prev, ...updates }));
  }, []);

  const installPlugin = useCallback(async (
    pluginId: string, 
    version?: string,
    installationMethod: 'marketplace' | 'direct' | 'cli' = 'marketplace'
  ) => {
    try {
      updateState({ status: 'downloading', progress: 0, error: undefined });

      // Step 1: Get download information
      updateState({ progress: 20 });
      const downloadInfo: PluginDownloadInfo = await apiClient.getPluginDownload(pluginId, version);
      
      updateState({ 
        progress: 40, 
        downloadUrl: downloadInfo.downloadUrl,
        version: downloadInfo.version 
      });

      // Step 2: Record installation for analytics
      updateState({ progress: 60 });
      const installationRequest: PluginInstallationRequest = {
        pluginId,
        version: downloadInfo.version,
        installationMethod,
        userAgent: navigator.userAgent,
      };

      const installationResponse = await apiClient.recordInstallation(installationRequest);
      
      if (!installationResponse.success) {
        throw new Error(installationResponse.message || 'Installation recording failed');
      }

      // Step 3: Complete installation
      updateState({ progress: 80 });
      
      // Simulate download progress
      await new Promise(resolve => setTimeout(resolve, 500));
      
      updateState({ 
        status: 'success', 
        progress: 100,
        downloadUrl: downloadInfo.downloadUrl,
        version: downloadInfo.version
      });

      options.onSuccess?.(downloadInfo.downloadUrl, downloadInfo.version);

    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Installation failed';
      updateState({ 
        status: 'error', 
        progress: 0, 
        error: errorMessage 
      });
      options.onError?.(errorMessage);
    }
  }, [updateState, options]);

  const reset = useCallback(() => {
    setState({
      status: 'idle',
      progress: 0,
    });
  }, []);

  const downloadDirectly = useCallback(async (downloadUrl: string) => {
    try {
      // Create a temporary link to trigger download
      const link = document.createElement('a');
      link.href = downloadUrl;
      link.download = '';
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      
      return true;
    } catch (error) {
      console.error('Direct download failed:', error);
      return false;
    }
  }, []);

  const openRepository = useCallback((repositoryUrl: string) => {
    window.open(repositoryUrl, '_blank', 'noopener,noreferrer');
  }, []);

  return {
    state,
    installPlugin,
    reset,
    downloadDirectly,
    openRepository,
    isInstalling: state.status === 'downloading' || state.status === 'installing',
    isSuccess: state.status === 'success',
    isError: state.status === 'error',
    isIdle: state.status === 'idle',
  };
}
