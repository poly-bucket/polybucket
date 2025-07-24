import React from 'react';
import store from '../store';
import { useAppDispatch, useAppSelector } from '../utils/hooks';
import { useAuth } from '../context/AuthContext';
import modelsService from '../services/modelsService';
import collectionsService from '../services/collectionsService';

/**
 * Plugin API that provides access to core application functionality
 */
export class PluginAPI {
  private static instance: PluginAPI;

  public static getInstance(): PluginAPI {
    if (!PluginAPI.instance) {
      PluginAPI.instance = new PluginAPI();
    }
    return PluginAPI.instance;
  }

  /**
   * Get the Redux store
   */
  getStore() {
    return store;
  }

  /**
   * Get current application state
   */
  getState() {
    return store.getState();
  }

  /**
   * Dispatch Redux actions
   */
  dispatch(action: any) {
    return store.dispatch(action);
  }

  /**
   * Get available services
   */
  getServices() {
    return {
      models: modelsService,
      collections: collectionsService
    };
  }

  /**
   * Make HTTP requests to the backend
   */
  async makeRequest(endpoint: string, options: RequestInit = {}): Promise<Response> {
    const token = this.getState().auth?.user?.accessToken;
    const headers = {
      'Content-Type': 'application/json',
      ...(token && { Authorization: `Bearer ${token}` }),
      ...options.headers
    };

    return fetch(`/api${endpoint}`, {
      ...options,
      headers
    });
  }

  /**
   * Show notifications/toasts
   */
  showNotification(message: string, type: 'success' | 'error' | 'info' | 'warning' = 'info') {
    // For now, we'll use console.log, but this should integrate with a toast system
    console.log(`[${type.toUpperCase()}] ${message}`);
    
    // TODO: Integrate with actual notification system
    // This could dispatch a Redux action to show notifications
  }

  /**
   * Navigate to a different page
   */
  navigate(path: string) {
    // This would need to be integrated with React Router
    window.location.href = path;
  }

  /**
   * Get current user information
   */
  getCurrentUser() {
    return this.getState().auth?.user;
  }

  /**
   * Check if user has specific permissions
   */
  hasPermission(permission: string): boolean {
    const user = this.getCurrentUser();
    // TODO: Implement proper permission checking
    return user?.roles?.includes('Admin') || false;
  }

  /**
   * Subscribe to state changes
   */
  subscribe(callback: () => void) {
    return store.subscribe(callback);
  }

  /**
   * Plugin-specific storage
   */
  getPluginStorage(pluginId: string) {
    return {
      get: (key: string) => {
        const storageKey = `plugin_${pluginId}_${key}`;
        const value = localStorage.getItem(storageKey);
        try {
          return value ? JSON.parse(value) : null;
        } catch {
          return value;
        }
      },
      set: (key: string, value: any) => {
        const storageKey = `plugin_${pluginId}_${key}`;
        localStorage.setItem(storageKey, JSON.stringify(value));
      },
      remove: (key: string) => {
        const storageKey = `plugin_${pluginId}_${key}`;
        localStorage.removeItem(storageKey);
      },
      clear: () => {
        const prefix = `plugin_${pluginId}_`;
        Object.keys(localStorage)
          .filter(key => key.startsWith(prefix))
          .forEach(key => localStorage.removeItem(key));
      }
    };
  }
}

/**
 * React hook for accessing the Plugin API
 */
export function usePluginAPI() {
  return PluginAPI.getInstance();
}

/**
 * React hook for plugin-specific functionality
 */
export function usePlugin(pluginId: string) {
  const api = usePluginAPI();
  const dispatch = useAppDispatch();
  const state = useAppSelector(state => state);
  const auth = useAuth();

  return {
    api,
    dispatch,
    state,
    auth,
    storage: api.getPluginStorage(pluginId),
    services: api.getServices(),
    showNotification: api.showNotification,
    hasPermission: api.hasPermission,
    getCurrentUser: api.getCurrentUser
  };
}

/**
 * Base props that all plugin components receive
 */
export interface PluginComponentProps {
  pluginId: string;
  componentId: string;
  [key: string]: any;
}

/**
 * Higher-order component that provides plugin API to components
 */
export function withPluginAPI<P extends PluginComponentProps>(
  Component: React.ComponentType<P & { pluginAPI: PluginAPI }>
): React.ComponentType<P> {
  return function PluginWrappedComponent(props: P) {
    const pluginAPI = usePluginAPI();
    return React.createElement(Component, { ...props, pluginAPI } as P & { pluginAPI: PluginAPI });
  };
} 