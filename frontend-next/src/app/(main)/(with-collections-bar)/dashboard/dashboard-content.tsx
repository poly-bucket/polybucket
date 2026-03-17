"use client";

import { useDashboardSections, PluginBoundary } from "@/lib/plugins";

export function DashboardContent() {
  const sections = useDashboardSections();

  return (
    <div className="mx-auto max-w-6xl px-4 py-8 sm:px-6 lg:px-8">
      <h1 className="mb-8 text-2xl font-semibold text-white">Dashboard</h1>
      <div className="space-y-6">
        {sections.map((section) => {
          const Component = section.component;
          return (
            <PluginBoundary key={section.id} pluginId={section.id}>
              <Component />
            </PluginBoundary>
          );
        })}
      </div>
    </div>
  );
}
