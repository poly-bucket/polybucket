"use client";

import { useEffect } from "react";
import "@/lib/plugins/config";
import { AuthProvider, useAuth } from "@/contexts/AuthContext";
import {
  getLayoutProviders,
  getPlugins,
} from "@/lib/plugins/registry";
import { PluginBoundary } from "@/lib/plugins/plugin-boundary";

function ComposeLayoutProviders({
  children,
}: {
  children: React.ReactNode;
}) {
  const providers = getLayoutProviders().sort((a, b) => a.order - b.order);
  return providers.reduce(
    (acc, { id, component: C }) => (
      <PluginBoundary pluginId={id} key={id}>
        <C>{acc}</C>
      </PluginBoundary>
    ),
    children
  );
}

function PluginLifecycleRunner({ children }: { children: React.ReactNode }) {
  const { user, isAuthenticated } = useAuth();
  const plugins = getPlugins();

  useEffect(() => {
    const context = {
      isAuthenticated,
      userId: user?.id,
      roles: user?.roles,
    };
    for (const plugin of plugins) {
      plugin.onInit?.(context);
    }
    return () => {
      for (const plugin of plugins) {
        plugin.onDestroy?.();
      }
    };
  }, [plugins, isAuthenticated, user?.id, user?.roles]);

  return <>{children}</>;
}

export function Providers({ children }: { children: React.ReactNode }) {
  return (
    <AuthProvider>
      <PluginLifecycleRunner>
        <ComposeLayoutProviders>{children}</ComposeLayoutProviders>
      </PluginLifecycleRunner>
    </AuthProvider>
  );
}
