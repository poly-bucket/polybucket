"use client";

import { useState, useCallback, useEffect } from "react";
import { SettingsSection } from "@/components/settings/settings-section";
import { SettingsToggle } from "@/components/settings/settings-toggle";
import { SettingsField } from "@/components/settings/settings-field";
import { SettingsFooter } from "@/components/settings/settings-footer";
import { Input } from "@/components/primitives/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  getSiteSettings,
  updateSiteSettingsMerged,
  mergeSiteSettingsPatch,
} from "@/lib/services/adminService";
import type { SiteSettingsData } from "@/lib/services/siteSettingsTypes";
import { toast } from "sonner";
import { ProfileSettingsFormSkeleton } from "@/components/ui/skeletons";

const LANGUAGES = [
  { value: "en", label: "English" },
  { value: "es", label: "Spanish" },
  { value: "fr", label: "French" },
  { value: "de", label: "German" },
];

export function SiteSettingsSection() {
  const [base, setBase] = useState<SiteSettingsData | null>(null);
  const [form, setForm] = useState<SiteSettingsData | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const settings = await getSiteSettings();
      setBase(settings ?? {});
      setForm(settings ?? {});
    } catch {
      setError("Failed to load site settings");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    load();
  }, [load]);

  const patch = (updates: Partial<SiteSettingsData>) => {
    setForm((prev) => (prev ? mergeSiteSettingsPatch(prev, updates) : prev));
  };

  const isDirty =
    base && form ? JSON.stringify(base) !== JSON.stringify(form) : false;

  const save = async () => {
    if (!form) return;
    setSaving(true);
    try {
      const result = await updateSiteSettingsMerged(form);
      if (result?.success) {
        setBase(form);
        toast.success("Site settings saved");
      } else {
        toast.error(result?.message ?? "Failed to save site settings");
      }
    } catch {
      toast.error("Failed to save site settings");
    } finally {
      setSaving(false);
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
      title="Site identity & access"
      description="Public site information and who can browse or register"
    >
      {error && (
        <div className="mb-4 rounded-lg border border-red-500/50 bg-red-500/10 px-4 py-3 text-red-400">
          {error}
        </div>
      )}
      <SettingsField label="Site name" description="Shown in titles and emails">
        <Input
          variant="glass"
          value={form.siteName ?? ""}
          onChange={(e) => patch({ siteName: e.target.value })}
          className="text-white"
        />
      </SettingsField>
      <SettingsField label="Site description" description="Short tagline for your instance">
        <Input
          variant="glass"
          value={form.siteDescription ?? ""}
          onChange={(e) => patch({ siteDescription: e.target.value })}
          className="text-white"
        />
      </SettingsField>
      <SettingsField label="Contact email" description="Public contact for this instance">
        <Input
          variant="glass"
          type="email"
          value={form.contactEmail ?? ""}
          onChange={(e) => patch({ contactEmail: e.target.value })}
          className="text-white"
        />
      </SettingsField>
      <SettingsToggle
        label="Allow public browsing"
        description="When off, visitors must sign in to view the site"
        checked={form.allowPublicBrowsing ?? true}
        onCheckedChange={(v) => patch({ allowPublicBrowsing: v })}
      />
      <SettingsToggle
        label="Allow user registration"
        description="Let new users create accounts"
        checked={form.allowUserRegistration ?? false}
        onCheckedChange={(v) => patch({ allowUserRegistration: v })}
      />
      <SettingsToggle
        label="Disable email subsystem"
        description="Turn off outbound email for this site (overrides SMTP configuration)"
        checked={form.disableEmailSettings ?? false}
        onCheckedChange={(v) => patch({ disableEmailSettings: v })}
      />
      <SettingsField label="Default language" description="Default locale for new sessions">
        <Select
          value={form.defaultLanguage ?? "en"}
          onValueChange={(v) => patch({ defaultLanguage: v })}
        >
          <SelectTrigger variant="glass">
            <SelectValue />
          </SelectTrigger>
          <SelectContent variant="glass">
            {LANGUAGES.map((l) => (
              <SelectItem key={l.value} value={l.value}>
                {l.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </SettingsField>
      <SettingsToggle
        label="Show advanced options in admin"
        description="Hints admins that extended model settings are available"
        checked={form.showAdvancedOptions ?? false}
        onCheckedChange={(v) => patch({ showAdvancedOptions: v })}
      />
      <SettingsFooter onSave={save} isSaving={saving} isDirty={isDirty} />
    </SettingsSection>
  );
}
