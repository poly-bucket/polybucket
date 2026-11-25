"use client";

import { useEffect, useState } from "react";
import { toast } from "sonner";
import { useUserSettings } from "@/contexts/UserSettingsContext";
import { SettingsSection } from "@/components/settings/settings-section";
import { SettingsField } from "@/components/settings/settings-field";
import { SettingsToggle } from "@/components/settings/settings-toggle";
import { SettingsFooter } from "@/components/settings/settings-footer";
import { LayoutPreferencesSkeleton } from "@/components/ui/skeletons";
import { ApiClientFactory } from "@/lib/api/clientFactory";
import type { PrinterListItemDto } from "@/lib/api/client";

const LANGUAGE_OPTIONS = [{ value: "en", label: "English" }];

const THEME_OPTIONS = [
  { value: "dark", label: "Dark" },
  { value: "light", label: "Light" },
  { value: "system", label: "System" },
];

const MEASUREMENT_OPTIONS = [
  { value: "metric", label: "Metric (mm)" },
  { value: "imperial", label: "Imperial (in)" },
];

const TIME_ZONE_OPTIONS = [
  "UTC",
  "America/New_York",
  "America/Chicago",
  "America/Denver",
  "America/Los_Angeles",
  "Europe/London",
  "Europe/Paris",
  "Asia/Tokyo",
  "Australia/Sydney",
];

const selectClassName =
  "flex h-10 w-full rounded-md border border-white/20 bg-white/5 px-3 py-2 text-sm text-white";

export default function PreferencesPage() {
  const { settings, updateSettings } = useUserSettings();
  const [printers, setPrinters] = useState<PrinterListItemDto[]>([]);
  const [local, setLocal] = useState({
    language: "en",
    theme: "dark",
    timeZone: "UTC",
    measurementSystem: "metric",
    defaultPrinterId: "" as string,
    autoRotateModels: true,
  });
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (!settings) {
      return;
    }
    setLocal({
      language: settings.language ?? "en",
      theme: settings.theme ?? "dark",
      timeZone: settings.timeZone ?? "UTC",
      measurementSystem: settings.measurementSystem ?? "metric",
      defaultPrinterId: settings.defaultPrinterId ?? "",
      autoRotateModels: settings.autoRotateModels ?? true,
    });
  }, [
    settings?.language,
    settings?.theme,
    settings?.timeZone,
    settings?.measurementSystem,
    settings?.defaultPrinterId,
    settings?.autoRotateModels,
  ]);

  useEffect(() => {
  let cancelled = false;
  (async () => {
    try {
      const response = await ApiClientFactory.getApiClient().getPrinters_GetPrinters();
      if (!cancelled) {
        setPrinters(response.printers ?? []);
      }
    } catch {
      if (!cancelled) setPrinters([]);
    }
  })();
  return () => {
    cancelled = true;
  };
}, []);

  const handleSave = async () => {
    setSaving(true);
    try {
      const success = await updateSettings({
        language: local.language,
        theme: local.theme,
        timeZone: local.timeZone,
        measurementSystem: local.measurementSystem,
        defaultPrinterId: local.defaultPrinterId,
        autoRotateModels: local.autoRotateModels,
      });
      if (success) {
        toast.success("Preferences updated");
      } else {
        toast.error("Failed to update preferences");
      }
    } catch {
      toast.error("Failed to update preferences");
    } finally {
      setSaving(false);
    }
  };

  if (!settings) {
    return <LayoutPreferencesSkeleton />;
  }

  return (
    <div className="space-y-8">
      <SettingsSection title="Regional" description="Language, time zone, and units">
        <SettingsField label="Language" description="Site language preference (translations coming later)">
          <select
            className={selectClassName}
            value={local.language}
            onChange={(e) => setLocal((p) => ({ ...p, language: e.target.value }))}
          >
            {LANGUAGE_OPTIONS.map((opt) => (
              <option key={opt.value} value={opt.value} className="bg-zinc-900">
                {opt.label}
              </option>
            ))}
          </select>
        </SettingsField>
        <SettingsField label="Time zone" description="Used when showing dates and times">
          <select
            className={selectClassName}
            value={local.timeZone}
            onChange={(e) => setLocal((p) => ({ ...p, timeZone: e.target.value }))}
          >
            {TIME_ZONE_OPTIONS.map((tz) => (
              <option key={tz} value={tz} className="bg-zinc-900">
                {tz.replace(/_/g, " ")}
              </option>
            ))}
          </select>
        </SettingsField>
        <SettingsField label="Measurement system" description="How lengths and build volumes are shown">
          <select
            className={selectClassName}
            value={local.measurementSystem}
            onChange={(e) => setLocal((p) => ({ ...p, measurementSystem: e.target.value }))}
          >
            {MEASUREMENT_OPTIONS.map((opt) => (
              <option key={opt.value} value={opt.value} className="bg-zinc-900">
                {opt.label}
              </option>
            ))}
          </select>
        </SettingsField>
      </SettingsSection>

      <SettingsSection title="Display" description="Appearance and model viewer">
        <SettingsField label="Theme" description="Light or dark mode (site colors still come from admin theme)">
          <select
            className={selectClassName}
            value={local.theme}
            onChange={(e) => setLocal((p) => ({ ...p, theme: e.target.value }))}
          >
            {THEME_OPTIONS.map((opt) => (
              <option key={opt.value} value={opt.value} className="bg-zinc-900">
                {opt.label}
              </option>
            ))}
          </select>
        </SettingsField>
        <SettingsToggle
          label="Auto-rotate models"
          description="Spin models by default in the 3D viewer"
          checked={local.autoRotateModels}
          onCheckedChange={(checked) => setLocal((p) => ({ ...p, autoRotateModels: checked }))}
        />
      </SettingsSection>

      <SettingsSection title="Printing" description="Default hardware for your workflow">
        <SettingsField label="Default printer" description="Used to prefill print-related UI">
          <select
            className={selectClassName}
            value={local.defaultPrinterId}
            onChange={(e) => setLocal((p) => ({ ...p, defaultPrinterId: e.target.value }))}
          >
            <option value="" className="bg-zinc-900">
              None
            </option>
            {printers.map((printer) => (
              <option key={printer.id} value={printer.id} className="bg-zinc-900">
                {printer.manufacturer} {printer.model}
              </option>
            ))}
          </select>
        </SettingsField>
      </SettingsSection>

      <SettingsFooter onSave={handleSave} isSaving={saving} isDirty />
    </div>
  );
}