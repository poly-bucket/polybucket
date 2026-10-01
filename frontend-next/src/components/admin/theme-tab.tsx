"use client";

import { useState, useEffect, useCallback } from "react";
import { SettingsSection } from "@/components/settings/settings-section";
import { Button } from "@/components/primitives/button";
import { Input } from "@/components/primitives/input";
import {
  getPresetThemes,
  activatePresetTheme,
  getThemeConfiguration,
  updateThemeConfiguration,
} from "@/lib/services/adminService";
import type { ThemeDto } from "@/lib/api/client";
import { useAdminQuery } from "@/lib/hooks/use-admin-query";
import { useAdminMutation } from "@/lib/hooks/use-admin-mutation";
import {
  applySiteThemeColors,
  extensibleConfigToSiteTheme,
  siteThemeToExtensibleConfig,
  themeColorsFromDto,
} from "@/lib/theme/apply-site-theme";

const COLOR_CONFIG_LABELS: Record<string, string> = {
  primaryColor: "Primary",
  primaryLightColor: "Primary light",
  primaryDarkColor: "Primary dark",
  secondaryColor: "Secondary",
  secondaryLightColor: "Secondary light",
  secondaryDarkColor: "Secondary dark",
  accentColor: "Accent",
  accentLightColor: "Accent light",
  accentDarkColor: "Accent dark",
  backgroundPrimaryColor: "Background primary",
  backgroundSecondaryColor: "Background secondary",
  backgroundTertiaryColor: "Background tertiary",
};

export function ThemeTab() {
  const {
    data: themeListResponse,
    isLoading: loadingThemes,
    error: themesError,
    refetch: refetchThemes,
  } = useAdminQuery(getPresetThemes);

  const { data: config, refetch: refetchConfig } = useAdminQuery(
    getThemeConfiguration
  );

  const themeList = themeListResponse?.themes ?? [];
  const activeTheme = themeListResponse?.activeTheme ?? null;

  const setThemeMutation = useAdminMutation(
    async (theme: ThemeDto) => {
      if (theme.id === undefined) {
        throw new Error("Theme ID is missing");
      }
      await activatePresetTheme(theme.id);
      const siteColors = themeColorsFromDto(theme.colors);
      if (siteColors) {
        applySiteThemeColors(siteColors);
      }
    },
    {
      onSuccess: () => {
        refetchThemes();
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

  const resetMutation = useAdminMutation(
    async () => {
      const defaultTheme =
        themeList.find((t) => t.isDefault) ?? themeList[0] ?? null;
      if (defaultTheme?.id === undefined) {
        throw new Error("No default theme found");
      }
      await activatePresetTheme(defaultTheme.id);
      const siteColors = themeColorsFromDto(defaultTheme.colors);
      if (siteColors) {
        applySiteThemeColors(siteColors);
      }
    },
    {
      onSuccess: () => {
        refetchThemes();
        refetchConfig();
      },
      successMessage: "Theme reset to defaults",
    }
  );

  const [localConfig, setLocalConfig] = useState<Record<string, string>>({});

  useEffect(() => {
    const fromExtensible = config && typeof config === "object"
      ? extensibleConfigToSiteTheme(config)
      : null;
    if (fromExtensible) {
      setLocalConfig(siteThemeToExtensibleConfig(fromExtensible));
      return;
    }
    const fromActive = themeColorsFromDto(activeTheme?.colors);
    if (fromActive) {
      setLocalConfig(siteThemeToExtensibleConfig(fromActive));
    }
  }, [config, activeTheme]);

  const handleColorChange = useCallback((key: string, hex: string) => {
    setLocalConfig((p) => {
      const next = { ...p, [key]: hex };
      const site = extensibleConfigToSiteTheme(next);
      if (site) {
        applySiteThemeColors(site);
      }
      return next;
    });
  }, []);

  const handleSaveColors = useCallback(() => {
    updateConfigMutation.mutate(localConfig as Record<string, unknown>);
  }, [localConfig, updateConfigMutation]);

  if (loadingThemes) {
    return (
      <div className="space-y-6">
        <h2 className="text-2xl font-bold text-white">Theme</h2>
        <div className="text-center text-white/60 py-12">
          Loading theme settings...
        </div>
      </div>
    );
  }

  const currentName = activeTheme?.name ?? "Default";

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold text-white">Theme</h2>

      {themesError && (
        <div className="rounded-lg border border-red-500/50 bg-red-500/10 px-4 py-3 text-red-400">
          {themesError}
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
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {themeList.map((t) => {
            const isActive = t.isActive ?? activeTheme?.id === t.id;
            return (
              <button
                key={t.id ?? t.name ?? "unknown"}
                type="button"
                onClick={() => setThemeMutation.mutate(t)}
                disabled={setThemeMutation.isLoading || t.id === undefined}
                className={`rounded-lg border p-4 text-left transition-all ${
                  isActive
                    ? "border-white/40 glass-bg ring-2 ring-white/30"
                    : "border-white/10 glass-bg hover:border-white/20"
                }`}
              >
                <div className="flex items-center gap-3 mb-2">
                  <div className="flex gap-1">
                    <div
                      className="h-4 w-4 rounded"
                      style={{
                        backgroundColor: t.colors?.primary ?? "#6366f1",
                      }}
                    />
                    <div
                      className="h-4 w-4 rounded"
                      style={{
                        backgroundColor: t.colors?.secondary ?? "#8b5cf6",
                      }}
                    />
                    <div
                      className="h-4 w-4 rounded"
                      style={{
                        backgroundColor: t.colors?.accent ?? "#06b6d4",
                      }}
                    />
                  </div>
                  <span className="font-medium text-white">{t.name}</span>
                  {isActive && (
                    <span className="text-xs rounded bg-white/20 px-2 py-0.5 text-white">
                      Active
                    </span>
                  )}
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
                <div key={key} className="flex items-center gap-4">
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
                    {COLOR_CONFIG_LABELS[key] ?? key}
                  </span>
                </div>
              );
            })}
        </div>
        {Object.keys(localConfig).filter((k) =>
          k.toLowerCase().includes("color")
        ).length === 0 && (
          <p className="text-white/60 text-sm">
            Select a preset theme to customize colors.
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
