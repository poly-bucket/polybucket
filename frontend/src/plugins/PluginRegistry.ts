import React from 'react';

export interface PluginComponent {
  id: string;
  name: string;
  componentPath: string;
  type: ComponentType;
  props?: Record<string, any>;
  dependencies?: string[];
  hooks?: PluginHook[];
}

export interface PluginHook {
  hookName: string;
  componentId: string;
  priority: number;
  config?: Record<string, any>;
}

export enum ComponentType {
  Page = 'Page',
  Widget = 'Widget',
  Modal = 'Modal',
  Sidebar = 'Sidebar',
  Toolbar = 'Toolbar',
  Overlay = 'Overlay',
  Embed = 'Embed'
}

export interface PluginMetadata {
  minimumAppVersion: string;
  maximumAppVersion?: string;
  requiredPermissions: string[];
  optionalPermissions: string[];
  settings: Record<string, PluginSetting>;
  dependencies: string[];
  lifecycle: PluginLifecycle;
}

export interface PluginSetting {
  name: string;
  description: string;
  type: PluginSettingType;
  defaultValue: any;
  required: boolean;
  options?: string[];
}

export enum PluginSettingType {
  String = 'String',
  Number = 'Number',
  Boolean = 'Boolean',
  Select = 'Select',
  MultiSelect = 'MultiSelect',
  Json = 'Json'
}

export interface PluginLifecycle {
  autoStart: boolean;
  canDisable: boolean;
  canUninstall: boolean;
  startupTasks: string[];
  shutdownTasks: string[];
}

export interface Plugin {
  id: string;
  name: string;
  version: string;
  author: string;
  description: string;
  frontendComponents: PluginComponent[];
  metadata: PluginMetadata;
}

export interface LoadedComponent {
  id: string;
  component: React.ComponentType<any>;
  plugin: Plugin;
  metadata: PluginComponent;
}

export interface HookSubscription {
  hookName: string;
  components: Array<{
    componentId: string;
    component: React.ComponentType<any>;
    priority: number;
    config: Record<string, any>;
    plugin: Plugin;
  }>;
}

class PluginRegistry {
  private plugins: Map<string, Plugin> = new Map();
  private loadedComponents: Map<string, LoadedComponent> = new Map();
  private hooks: Map<string, HookSubscription> = new Map();
  private componentCache: Map<string, React.ComponentType<any>> = new Map();

  /**
   * Register a plugin with the registry
   */
  registerPlugin(plugin: Plugin): void {
    if (this.plugins.has(plugin.id)) {
      console.warn(`Plugin ${plugin.id} is already registered`);
      return;
    }

    this.plugins.set(plugin.id, plugin);
    
    // Load all frontend components for this plugin
    plugin.frontendComponents.forEach(componentDef => {
      this.loadComponent(plugin, componentDef);
    });

    console.log(`Plugin ${plugin.name} (${plugin.id}) registered successfully`);
  }

  /**
   * Unregister a plugin
   */
  unregisterPlugin(pluginId: string): void {
    const plugin = this.plugins.get(pluginId);
    if (!plugin) {
      console.warn(`Plugin ${pluginId} not found`);
      return;
    }

    // Remove all components for this plugin
    plugin.frontendComponents.forEach(componentDef => {
      this.unloadComponent(componentDef.id);
    });

    // Remove from hooks
    this.hooks.forEach((subscription, hookName) => {
      subscription.components = subscription.components.filter(
        hookComponent => hookComponent.plugin.id !== pluginId
      );
      if (subscription.components.length === 0) {
        this.hooks.delete(hookName);
      }
    });

    this.plugins.delete(pluginId);
    console.log(`Plugin ${pluginId} unregistered`);
  }

  /**
   * Load a React component dynamically
   */
  private async loadComponent(plugin: Plugin, componentDef: PluginComponent): Promise<void> {
    try {
      // Dynamic import of the component
      const componentModule = await import(`../plugins/${componentDef.componentPath}`);
      const Component = componentModule.default || componentModule[componentDef.name];

      if (!Component) {
        throw new Error(`Component ${componentDef.name} not found in ${componentDef.componentPath}`);
      }

      const loadedComponent: LoadedComponent = {
        id: componentDef.id,
        component: Component,
        plugin,
        metadata: componentDef
      };

      this.loadedComponents.set(componentDef.id, loadedComponent);
      this.componentCache.set(componentDef.id, Component);

      // Register component hooks
      componentDef.hooks?.forEach(hook => {
        this.registerHook(hook.hookName, {
          componentId: componentDef.id,
          component: Component,
          priority: hook.priority,
          config: hook.config || {},
          plugin
        });
      });

      console.log(`Component ${componentDef.name} loaded successfully`);
    } catch (error) {
      console.error(`Failed to load component ${componentDef.name}:`, error);
    }
  }

  /**
   * Unload a component
   */
  private unloadComponent(componentId: string): void {
    const loadedComponent = this.loadedComponents.get(componentId);
    if (loadedComponent) {
      // Remove from hooks
      loadedComponent.metadata.hooks?.forEach(hook => {
        this.unregisterHook(hook.hookName, componentId);
      });

      this.loadedComponents.delete(componentId);
      this.componentCache.delete(componentId);
    }
  }

  /**
   * Register a component to a hook
   */
  private registerHook(hookName: string, hookComponent: {
    componentId: string;
    component: React.ComponentType<any>;
    priority: number;
    config: Record<string, any>;
    plugin: Plugin;
  }): void {
    if (!this.hooks.has(hookName)) {
      this.hooks.set(hookName, {
        hookName,
        components: []
      });
    }

    const subscription = this.hooks.get(hookName)!;
    subscription.components.push(hookComponent);
    
    // Sort by priority (lower number = higher priority)
    subscription.components.sort((a, b) => a.priority - b.priority);
  }

  /**
   * Unregister a component from a hook
   */
  private unregisterHook(hookName: string, componentId: string): void {
    const subscription = this.hooks.get(hookName);
    if (subscription) {
      subscription.components = subscription.components.filter(
        component => component.componentId !== componentId
      );
      
      if (subscription.components.length === 0) {
        this.hooks.delete(hookName);
      }
    }
  }

  /**
   * Get all components registered to a specific hook
   */
  getHookComponents(hookName: string): Array<{
    componentId: string;
    component: React.ComponentType<any>;
    priority: number;
    config: Record<string, any>;
    plugin: Plugin;
  }> {
    const subscription = this.hooks.get(hookName);
    return subscription ? subscription.components : [];
  }

  /**
   * Get a loaded component by ID
   */
  getComponent(componentId: string): LoadedComponent | undefined {
    return this.loadedComponents.get(componentId);
  }

  /**
   * Get all registered plugins
   */
  getPlugins(): Plugin[] {
    return Array.from(this.plugins.values());
  }

  /**
   * Get a specific plugin by ID
   */
  getPlugin(pluginId: string): Plugin | undefined {
    return this.plugins.get(pluginId);
  }

  /**
   * Get all loaded components
   */
  getLoadedComponents(): LoadedComponent[] {
    return Array.from(this.loadedComponents.values());
  }

  /**
   * Check if a plugin is registered
   */
  isPluginRegistered(pluginId: string): boolean {
    return this.plugins.has(pluginId);
  }

  /**
   * Get all available hooks
   */
  getAvailableHooks(): string[] {
    return Array.from(this.hooks.keys());
  }

  /**
   * Initialize the plugin system with plugins from the backend
   */
  async initializeFromBackend(): Promise<void> {
    try {
      const response = await fetch('/api/plugins');
      const plugins: Plugin[] = await response.json();

      plugins.forEach(plugin => {
        this.registerPlugin(plugin);
      });

      console.log(`Initialized ${plugins.length} plugins from backend`);
    } catch (error) {
      console.error('Failed to initialize plugins from backend:', error);
    }
  }
}

// Export singleton instance
export const pluginRegistry = new PluginRegistry();

// Export React hook for using the plugin system
export function usePluginRegistry() {
  return pluginRegistry;
}

// Export React hook for accessing hook components
export function useHookComponents(hookName: string) {
  const registry = usePluginRegistry();
  const [components, setComponents] = React.useState(
    registry.getHookComponents(hookName)
  );

  React.useEffect(() => {
    // Re-fetch components if plugins change
    const updatedComponents = registry.getHookComponents(hookName);
    setComponents(updatedComponents);
  }, [hookName, registry]);

  return components;
} 