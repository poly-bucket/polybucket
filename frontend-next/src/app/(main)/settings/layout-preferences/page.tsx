"use client";

import { useState, useEffect } from "react";
import { toast } from "sonner";
import { LayoutGrid, List, Maximize2, Square, Columns } from "lucide-react";
import { useUserSettings } from "@/contexts/UserSettingsContext";
import { SettingsSection } from "@/components/settings/settings-section";
import { Button } from "@/components/primitives/button";
import { cn } from "@/lib/utils";
import { LayoutPreferencesSkeleton } from "@/components/ui/skeletons";

const VIEW_TYPE_OPTIONS = [
  { value: "grid", label: "Grid", icon: LayoutGrid, description: "Card grid on browse pages" },
  { value: "list", label: "List", icon: List, description: "Compact rows on browse pages" },
];

const CARD_SIZE_OPTIONS = [
  { value: "small", label: "Small", icon: Square, description: "Compact" },
  { value: "medium", label: "Medium", icon: LayoutGrid, description: "Standard" },
  { value: "large", label: "Large", icon: Maximize2, description: "Spacious" },
];

const SPACING_OPTIONS = [
  { value: "compact", label: "Compact" },
  { value: "normal", label: "Normal" },
  { value: "spacious", label: "Spacious" },
];

const COLUMN_OPTIONS = [2, 3, 4, 5, 6];

export default function LayoutPreferencesPage() {
  const { settings, updateSettings } = useUserSettings();
  const [local, setLocal] = useState({
    dashboardViewType: "grid",
    cardSize: "medium",
    cardSpacing: "normal",
    gridColumns: 4,
  });
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (settings) {
      setLocal({
        dashboardViewType: settings.dashboardViewType ?? "grid",
        cardSize: settings.cardSize ?? "medium",
        cardSpacing: settings.cardSpacing ?? "normal",
        gridColumns: settings.gridColumns ?? 4,
      });
    }
  }, [settings]);

  const handleChange = (key: string, value: string | number) => {
    setLocal((prev) => ({ ...prev, [key]: value }));
  };

  const handleSave = async () => {
    setSaving(true);
    try {
      const success = await updateSettings({ ...local });
      if (success) {
        toast.success("Layout preferences updated");
      } else {
        toast.error("Failed to update layout preferences");
      }
    } catch {
      toast.error("Failed to update layout preferences");
    } finally {
      setSaving(false);
    }
  };

  const isListView = local.dashboardViewType === "list";

  if (!settings) {
    return <LayoutPreferencesSkeleton />;
  }

  return (
    <div className="space-y-8">
      <SettingsSection
        title="Model browse layout"
        description="Customize how models appear on the dashboard, search, my models, profiles, and collections."
      >
        <div className="space-y-6">
          <div>
            <label className="mb-2 block text-sm font-medium text-white">
              View type
            </label>
            <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
              {VIEW_TYPE_OPTIONS.map((opt) => {
                const Icon = opt.icon;
                return (
                  <button
                    key={opt.value}
                    type="button"
                    onClick={() => handleChange("dashboardViewType", opt.value)}
                    className={cn(
                      "flex items-center gap-3 rounded-lg border-2 p-4 transition-colors",
                      local.dashboardViewType === opt.value
                        ? "border-primary bg-primary/10"
                        : "border-white/20 bg-white/5 hover:border-white/40"
                    )}
                  >
                    <Icon className="h-5 w-5 text-white" />
                    <div className="text-left">
                      <p className="font-medium text-white">{opt.label}</p>
                      <p className="text-xs text-white/60">{opt.description}</p>
                    </div>
                  </button>
                );
              })}
            </div>
          </div>

          <div>
            <label className="mb-2 block text-sm font-medium text-white">
              {isListView ? "Row density" : "Card size"}
            </label>
            <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
              {CARD_SIZE_OPTIONS.map((opt) => {
                const Icon = opt.icon;
                return (
                  <button
                    key={opt.value}
                    type="button"
                    onClick={() => handleChange("cardSize", opt.value)}
                    className={cn(
                      "flex items-center gap-3 rounded-lg border-2 p-4 transition-colors",
                      local.cardSize === opt.value
                        ? "border-primary bg-primary/10"
                        : "border-white/20 bg-white/5 hover:border-white/40"
                    )}
                  >
                    <Icon className="h-5 w-5 text-white" />
                    <div className="text-left">
                      <p className="font-medium text-white">{opt.label}</p>
                      <p className="text-xs text-white/60">{opt.description}</p>
                    </div>
                  </button>
                );
              })}
            </div>
          </div>

          <div>
            <label className="mb-2 block text-sm font-medium text-white">
              Spacing
            </label>
            <div className="flex flex-wrap gap-2">
              {SPACING_OPTIONS.map((opt) => (
                <button
                  key={opt.value}
                  type="button"
                  onClick={() => handleChange("cardSpacing", opt.value)}
                  className={cn(
                    "rounded-md border-2 px-4 py-2 text-sm transition-colors",
                    local.cardSpacing === opt.value
                      ? "border-primary bg-primary/10 text-white"
                      : "border-white/20 text-white/80 hover:border-white/40"
                  )}
                >
                  {opt.label}
                </button>
              ))}
            </div>
          </div>

          {!isListView && (
            <div>
              <label className="mb-2 block text-sm font-medium text-white">
                Grid Columns
              </label>
              <p className="mb-2 text-xs text-white/50">
                Applies on large screens; smaller viewports use fewer columns automatically.
              </p>
              <div className="flex flex-wrap gap-2">
                {COLUMN_OPTIONS.map((n) => (
                  <button
                    key={n}
                    type="button"
                    onClick={() => handleChange("gridColumns", n)}
                    className={cn(
                      "flex items-center gap-2 rounded-md border-2 px-4 py-2 text-sm transition-colors",
                      local.gridColumns === n
                        ? "border-primary bg-primary/10 text-white"
                        : "border-white/20 text-white/80 hover:border-white/40"
                    )}
                  >
                    <Columns className="h-4 w-4" />
                    {n} Columns
                  </button>
                ))}
              </div>
            </div>
          )}

          <div className="border-t border-white/10 pt-4">
            <Button onClick={handleSave} disabled={saving} size="sm">
              {saving ? "Saving..." : "Save Layout Preferences"}
            </Button>
          </div>
        </div>
      </SettingsSection>
    </div>
  );
}