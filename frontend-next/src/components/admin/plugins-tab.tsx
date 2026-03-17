"use client";

import { Puzzle, ChevronDown, ChevronUp } from "lucide-react";
import { SettingsSection } from "@/components/settings/settings-section";
import { Button } from "@/components/primitives/button";
import { getPlugins } from "@/lib/plugins/registry";
import { useState } from "react";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/primitives/tabs";

export function PluginsTab() {
  const plugins = getPlugins();
  const [expandedId, setExpandedId] = useState<string | null>(null);

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold text-white">Plugins</h2>

      <Tabs defaultValue="installed" className="w-full">
        <TabsList variant="glass">
          <TabsTrigger variant="glass" value="installed">
            Installed
          </TabsTrigger>
          <TabsTrigger variant="glass" value="marketplace">
            Marketplace
          </TabsTrigger>
        </TabsList>
        <TabsContent variant="glass" value="installed">
          <SettingsSection
            title="Installed Plugins"
            description="Plugins loaded in this instance"
          >
            {plugins.length === 0 ? (
              <div className="rounded-lg border border-white/10 glass-bg p-8 text-center text-white/60">
                No plugins installed. The core plugin provides default
                functionality.
              </div>
            ) : (
              <div className="space-y-4">
                {plugins.map((plugin) => {
                  const isExpanded = expandedId === plugin.id;
                  return (
                    <div
                      key={plugin.id}
                      className="rounded-lg border border-white/10 glass-bg overflow-hidden"
                    >
                      <div className="flex items-center justify-between p-4">
                        <div className="flex items-center gap-3">
                          <Puzzle className="h-8 w-8 text-white/80" />
                          <div>
                            <p className="font-medium text-white">
                              {plugin.name}
                            </p>
                            <p className="text-sm text-white/50">
                              {plugin.id}
                              {plugin.version && ` v${plugin.version}`}
                            </p>
                          </div>
                        </div>
                        <div className="flex items-center gap-2">
                          <span className="rounded px-2 py-1 text-xs bg-green-500/20 text-green-400">
                            Active
                          </span>
                          <Button
                            variant="ghost"
                            size="icon"
                            onClick={() =>
                              setExpandedId(isExpanded ? null : plugin.id)
                            }
                          >
                            {isExpanded ? (
                              <ChevronUp className="h-4 w-4" />
                            ) : (
                              <ChevronDown className="h-4 w-4" />
                            )}
                          </Button>
                        </div>
                      </div>
                      {isExpanded && (
                        <div className="border-t border-white/10 px-4 py-3 text-sm text-white/70">
                          <p className="mb-2">
                            Plugins are loaded at startup. Enable/disable
                            requires a server restart.
                          </p>
                          {plugin.contributions.adminNavItems &&
                            plugin.contributions.adminNavItems.length > 0 && (
                              <p>
                                Contributes{" "}
                                {plugin.contributions.adminNavItems.length} admin
                                nav item(s).
                              </p>
                            )}
                        </div>
                      )}
                    </div>
                  );
                })}
              </div>
            )}
          </SettingsSection>
        </TabsContent>
        <TabsContent variant="glass" value="marketplace">
          <SettingsSection
            title="Plugin Marketplace"
            description="Browse and install plugins"
          >
            <div className="rounded-lg border border-white/10 glass-bg p-12 text-center">
              <Puzzle className="h-16 w-16 mx-auto mb-4 text-white/40" />
              <p className="text-white/80 mb-2 font-medium">
                Plugin marketplace coming soon
              </p>
              <p className="text-sm text-white/50 max-w-md mx-auto">
                Search, install from URL or GitHub, and browse featured plugins
                will be available when the marketplace API is ready.
              </p>
            </div>
          </SettingsSection>
        </TabsContent>
      </Tabs>
    </div>
  );
}
