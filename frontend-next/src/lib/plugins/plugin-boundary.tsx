"use client";

import React from "react";

interface PluginBoundaryProps {
  pluginId: string;
  fallback?: React.ReactNode;
  children: React.ReactNode;
}

interface PluginBoundaryState {
  hasError: boolean;
}

export class PluginBoundary extends React.Component<
  PluginBoundaryProps,
  PluginBoundaryState
> {
  state: PluginBoundaryState = { hasError: false };

  static getDerivedStateFromError(): PluginBoundaryState {
    return { hasError: true };
  }

  componentDidCatch(error: Error): void {
    console.error(`Plugin "${this.props.pluginId}" crashed:`, error);
  }

  render(): React.ReactNode {
    if (this.state.hasError) {
      return this.props.fallback ?? null;
    }
    return this.props.children;
  }
}
