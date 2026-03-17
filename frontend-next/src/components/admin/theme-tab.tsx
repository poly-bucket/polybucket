"use client";

import { useState, useEffect, useCallback } from "react";
import { Palette } from "lucide-react";
import { SettingsSection } from "@/components/settings/settings-section";
import { Button } from "@/components/primitives/button";
import { Input } from "@/components/primitives/input";
import {
  getAvailableThemes,
  getActiveTheme,
  setActiveTheme,
  getThemeConfiguration,
  updateThemeConfiguration,
  resetThemeToDefault,
} from "@/lib/services/adminService";
import { useAdminQuery } from "@/lib/hooks/use-admin-query";
import { useAdminMutation } from "@/lib/hooks/use-admin-mutation";
import { toast } from "sonner";

export function ThemeTab() {
  const {
    data: themes,
    isLoading: loadingThemes,
    error: themesError,
    refetch: refetchThemes,
  } = useAdminQuery(getAvailableThemes);

  const {
    data: activeTheme,
    isLoading: loadingActive,
    refetch: refetchActive,
  } = useAdminQuery(getActiveTheme);

  const {
    data: config,
    refetch: refetchConfig,
  } = useAdminQuery(getThemeConfiguration);

  const setThemeMutation = useAdminMutation(
    (id: string) => setActiveTheme(id),
    {
      onSuccess: () => {
        refetchActive();
        refetchConfig();
      },
      successMessage: "Theme applied",
    }
  );

  const updateConfigMutation = useAdminMutation(
    (c: Record<string, unknown>) => updateThemeConfiguration(c),
    {
      onSuccess: () => {
        refetchConfig();
      },
      successMessage: "Theme configuration updated",
    }
  );

  const resetMutation = useAdminMutation(() => resetThemeToDefault(), {
    onSuccess: () => {
      refetchActive();
      refetchConfig();
    },
    successMessage: "Theme reset to defaults",
  });

  const [localConfig, setLocalConfig] = useState<Record<string, string>>({});

  useEffect(() => {
    if (!config || typeof config !== "object") return;
    const flattened: Record<string, string> = {};
    for (const [k, v] of Object.entries(config)) {
      if (typeof v === "string") flattened[k] = v;
    }
    setLocalConfig(flattened);
  }, [config]);

  const handleColorChange = useCallback(
    (key: string, hex: string) => {
      setLocalConfig((p) => ({ ...p, [key]: hex }));
      document.documentElement.style.setProperty(key, hex);
    },
    []
  );

  const handleSaveColors = useCallback(() => {
    updateConfigMutation.mutate(localConfig as Record<string, unknown>);
  }, [localConfig, updateConfigMutation]);

  const error = themesError;

  if (loadingThemes && loadingActive) {
    return (
      <div className="space-y-6">
        <h2 className="text-2xl font-bold text-white">Theme</h2>
        <div className="text-center text-white/60 py-12">
          Loading theme settings...
        </div>
      </div>
    );
  }

  const themeList = themes ?? [];
  const currentName = activeTheme?.name ?? "Default";

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold text-white">Theme</h2>

      {error && (
        <div className="rounded-lg border border-red-500/50 bg-red-500/10 px-4 py-3 text-red-400">
          {error}
        </div>
      )}

      <SettingsSection
        title="Active Theme"
        description="Current theme and reset options"
      >
        <div className="flex items-center justify-between py-4">
          <div>
            <p className="font-medium text-white">{currentName}</p>
            <p className="text-sm text-white/60">
              {activeTheme?.description ?? "Site appearance"}
            </p>
          </div>
          <Button
            variant="glass"
            onClick={() => resetMutation.mutate(undefined)}
            disabled={resetMutation.isLoading}
          >
            {resetMutation.isLoading ? "Resetting..." : "Reset to Defaults"}
          </Button>
        </div>
      </SettingsSection>

      <SettingsSection
        title="Preset Themes"
        description="Select a theme to apply"
      >
        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-4">
          {themeList.map((t) => {
            const isActive = activeTheme?.id === t.id || activeTheme?.name === t.name;
            return (
              <button
                key={t.id ?? t.name ?? "unknown"}
                type="button"
                onClick={() =>
                  t.id && setThemeMutation.mutate(t.id)
                }
                disabled={setThemeMutation.isLoading || !t.id}
                className={`rounded-lg border p-4 text-left transition-all ${
                  isActive
                    ? "border-white/40 glass-bg ring-2 ring-white/30"
                    : "border-white/10 glass-bg hover:border-white/20"
                }`}
              >
                <div className="flex items-center gap-2 mb-2">
                  <Palette className="h-4 w-4 text-white/80" />
                  <span className="font-medium text-white">{t.name}</span>
                </div>
                {t.description && (
                  <p className="text-xs text-white/60 line-clamp-2">
                    {t.description}
                  </p>
                )}
              </button>
            );
          })}
        </div>
        {themeList.length === 0 && (
          <div className="rounded-lg border border-white/10 glass-bg p-6 text-center text-white/60">
            No themes available.
          </div>
        )}
      </SettingsSection>

      <SettingsSection
        title="Custom Colors"
        description="Override theme colors (live preview)"
      >
        <div className="space-y-4">
          {Object.entries(localConfig)
            .filter(([k]) => k.toLowerCase().includes("color"))
            .map(([key, value]) => {
              const hex = value.startsWith("#") ? value : `#${value}`;
              return (
                <div
                  key={key}
                  className="flex items-center gap-4"
                >
                  <input
                    type="color"
                    value={hex}
                    onChange={(e) => handleColorChange(key, e.target.value)}
                    className="h-10 w-14 rounded cursor-pointer border-0 bg-transparent"
                  />
                  <Input
                    variant="glass"
                    value={hex}
                    onChange={(e) => handleColorChange(key, e.target.value)}
                    className="text-white font-mono flex-1 max-w-[140px]"
                  />
                  <span className="text-sm text-white/50 truncate max-w-[200px]">
                    {key}
                  </span>
                </div>
              );
            })}
        </div>
        {Object.keys(localConfig).filter((k) =>
          k.toLowerCase().includes("color")
        ).length === 0 && (
          <p className="text-white/60 text-sm">
            No customizable color variables in the current theme.
          </p>
        )}
        {Object.keys(localConfig).filter((k) =>
          k.toLowerCase().includes("color")
        ).length > 0 && (
          <div className="mt-4">
            <Button
              variant="glass"
              onClick={handleSaveColors}
              disabled={updateConfigMutation.isLoading}
            >
              {updateConfigMutation.isLoading ? "Saving..." : "Save Colors"}
            </Button>
          </div>
        )}
      </SettingsSection>
    </div>
  );
}
