"use client";

import { useState, useCallback, useEffect } from "react";
import { SettingsSection } from "@/components/settings/settings-section";
import { SettingsToggle } from "@/components/settings/settings-toggle";
import { SettingsField } from "@/components/settings/settings-field";
import { SettingsFooter } from "@/components/settings/settings-footer";
import { Button } from "@/components/primitives/button";
import { Input } from "@/components/primitives/input";
import {
  getFontAwesomeSettings,
  updateFontAwesomeSettings,
  testFontAwesomeLicense,
  type AdminFontAwesomeSettings,
} from "@/lib/services/adminService";
import { toast } from "sonner";
import { ProfileSettingsFormSkeleton } from "@/components/ui/skeletons";

export function FontAwesomeSettingsSection() {
  const [base, setBase] = useState<AdminFontAwesomeSettings | null>(null);
  const [form, setForm] = useState<AdminFontAwesomeSettings | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [testing, setTesting] = useState(false);

  const load = useCallback(async () => {
    try {
      setLoading(true);
      const settings = await getFontAwesomeSettings();
      setBase(settings);
      setForm(settings);
    } catch {
      toast.error("Failed to load Font Awesome settings");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    load();
  }, [load]);

  const patch = (updates: Partial<AdminFontAwesomeSettings>) => {
    setForm((prev) => (prev ? { ...prev, ...updates } : prev));
  };

  const isDirty =
    base && form ? JSON.stringify(base) !== JSON.stringify(form) : false;

  const save = async () => {
    if (!form) return;
    setSaving(true);
    try {
      const updated = await updateFontAwesomeSettings(form);
      setBase(updated);
      setForm(updated);
      toast.success("Font Awesome settings saved");
    } catch {
      toast.error("Failed to save Font Awesome settings");
    } finally {
      setSaving(false);
    }
  };

  const testLicense = async () => {
    const key = form?.proLicenseKey?.trim();
    if (!key) {
      toast.error("Enter a license key to test");
      return;
    }
    setTesting(true);
    try {
      const result = await testFontAwesomeLicense({ licenseKey: key });
      if (result.isValid) {
        toast.success(result.message ?? "License is valid");
      } else {
        toast.error(result.message ?? "Invalid license");
      }
    } catch {
      toast.error("License test failed");
    } finally {
      setTesting(false);
    }
  };

  if (loading) {
    return <ProfileSettingsFormSkeleton />;
  }

  if (!form) {
    return null;
  }

  return (
    <SettingsSection
      title="Font Awesome Pro"
      description="Optional Pro kit for richer icons across the UI"
    >
      <SettingsToggle
        label="Enable Font Awesome Pro"
        checked={form.isProEnabled ?? false}
        onCheckedChange={(v) => patch({ isProEnabled: v })}
      />
      <SettingsToggle
        label="Use Pro icons"
        checked={form.useProIcons ?? true}
        onCheckedChange={(v) => patch({ useProIcons: v })}
      />
      <SettingsToggle
        label="Fallback to free icons"
        checked={form.fallbackToFree ?? true}
        onCheckedChange={(v) => patch({ fallbackToFree: v })}
      />
      <SettingsField label="Pro license key">
        <Input
          variant="glass"
          type="password"
          value={form.proLicenseKey ?? ""}
          onChange={(e) => patch({ proLicenseKey: e.target.value })}
          className="text-white"
          placeholder={
            form.hasProLicenseKey && !form.proLicenseKey
              ? "Key stored — enter new value to replace"
              : "••••••••"
          }
        />
      </SettingsField>
      <SettingsField label="Pro kit URL">
        <Input
          variant="glass"
          value={form.proKitUrl ?? ""}
          onChange={(e) => patch({ proKitUrl: e.target.value })}
          className="text-white"
        />
      </SettingsField>
      <div className="flex gap-2">
        <Button variant="outline" onClick={testLicense} disabled={testing}>
          {testing ? "Testing…" : "Test license"}
        </Button>
      </div>
      {form.isLicenseValid === false && form.licenseErrorMessage && (
        <p className="text-sm text-red-400">{form.licenseErrorMessage}</p>
      )}
      <SettingsFooter onSave={save} isSaving={saving} isDirty={isDirty} />
    </SettingsSection>
  );
}
