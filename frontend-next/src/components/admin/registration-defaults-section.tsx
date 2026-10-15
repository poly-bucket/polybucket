"use client";

import { useState, useCallback, useEffect } from "react";
import { SettingsSection } from "@/components/settings/settings-section";
import { SettingsToggle } from "@/components/settings/settings-toggle";
import { SettingsField } from "@/components/settings/settings-field";
import { SettingsFooter } from "@/components/settings/settings-footer";
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
  getAllRolesUnpaginated,
} from "@/lib/services/adminService";
import type { SiteSettingsData } from "@/lib/services/siteSettingsTypes";
import type { RoleDto } from "@/lib/api/client";
import { toast } from "sonner";
import { ProfileSettingsFormSkeleton } from "@/components/ui/skeletons";

export function RegistrationDefaultsSection() {
  const [base, setBase] = useState<SiteSettingsData | null>(null);
  const [form, setForm] = useState<SiteSettingsData | null>(null);
  const [roles, setRoles] = useState<RoleDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  const load = useCallback(async () => {
    try {
      setLoading(true);
      const [settings, roleList] = await Promise.all([
        getSiteSettings(),
        getAllRolesUnpaginated(),
      ]);
      setBase(settings ?? {});
      setForm(settings ?? {});
      setRoles(roleList ?? []);
    } catch {
      toast.error("Failed to load registration defaults");
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
        toast.success("Registration defaults saved");
      } else {
        toast.error(result?.message ?? "Failed to save");
      }
    } catch {
      toast.error("Failed to save registration defaults");
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
      title="Registration defaults"
      description="Default role assignment for new users"
    >
      <SettingsField label="Default user role">
        <Select
          value={form.defaultUserRole ?? "User"}
          onValueChange={(v) => patch({ defaultUserRole: v })}
        >
          <SelectTrigger variant="glass">
            <SelectValue />
          </SelectTrigger>
          <SelectContent variant="glass">
            {roles.map((r) => (
              <SelectItem key={r.id ?? r.name} value={r.name ?? "User"}>
                {r.name}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </SettingsField>
      <SettingsToggle
        label="Allow custom roles"
        description="Let administrators define additional roles beyond system defaults"
        checked={form.allowCustomRoles ?? false}
        onCheckedChange={(v) => patch({ allowCustomRoles: v })}
      />
      <SettingsFooter onSave={save} isSaving={saving} isDirty={isDirty} />
    </SettingsSection>
  );
}
