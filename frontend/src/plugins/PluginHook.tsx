import React from 'react';
import { useHookComponents } from './PluginRegistry';

interface PluginHookProps {
  hookName: string;
  context?: Record<string, any>;
  fallback?: React.ReactNode;
  className?: string;
  style?: React.CSSProperties;
}

/**
 * PluginHook component that renders all plugins registered to a specific hook point
 */
const PluginHook: React.FC<PluginHookProps> = ({
  hookName,
  context = {},
  fallback = null,
  className,
  style
}) => {
  const hookComponents = useHookComponents(hookName);

  if (hookComponents.length === 0) {
    return <>{fallback}</>;
  }

  return (
    <div className={className} style={style}>
      {hookComponents.map((hookComponent, index) => {
        const Component = hookComponent.component;
        const combinedProps = {
          ...hookComponent.config,
          ...context,
          pluginId: hookComponent.plugin.id,
          componentId: hookComponent.componentId
        };

        return (
          <React.Fragment key={`${hookComponent.componentId}-${index}`}>
            <Component {...combinedProps} />
          </React.Fragment>
        );
      })}
    </div>
  );
};

export default PluginHook; 