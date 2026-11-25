"use client";

import { useState, useCallback, useEffect } from "react";
import { SettingsSection } from "@/components/settings/settings-section";
import { SettingsToggle } from "@/components/settings/settings-toggle";
import { SettingsField } from "@/components/settings/settings-field";
import { SettingsFooter } from "@/components/settings/settings-footer";
import { Input } from "@/components/primitives/input";
import {
  getSiteSettings,
  updateSiteSettingsMerged,
  mergeSiteSettingsPatch,
} from "@/lib/services/adminService";
import type { SiteSettingsData } from "@/lib/services/siteSettingsTypes";
import { toast } from "sonner";
import { ProfileSettingsFormSkeleton } from "@/components/ui/skeletons";
import { getEmailSettings } from "@/lib/services/emailService";
import Link from "next/link";

type FederationInstanceSectionProps = {
  onFederationEnabledChange?: (enabled: boolean) => void;
};

export function FederationInstanceSection({
  onFederationEnabledChange,
}: FederationInstanceSectionProps) {
  const [base, setBase] = useState<SiteSettingsData | null>(null);
  const [form, setForm] = useState<SiteSettingsData | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [missingPublicUrl, setMissingPublicUrl] = useState(false);

  const load = useCallback(async () => {
    try {
      setLoading(true);
      const settings = await getSiteSettings();
      setBase(settings ?? {});
      setForm(settings ?? {});
    } catch {
      toast.error("Failed to load federation settings");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    load();
  }, [load]);

  useEffect(() => {
    onFederationEnabledChange?.(form?.enableFederation ?? false);
  }, [form?.enableFederation, onFederationEnabledChange]);

  useEffect(() => {
    if (!form?.enableFederation) {
      setMissingPublicUrl(false);
      return;
    }
    let cancelled = false;
    getEmailSettings()
      .then((email) => {
        if (!cancelled) {
          setMissingPublicUrl(!email.publicBaseUrl?.trim());
        }
      })
      .catch(() => {
        if (!cancelled) {
          setMissingPublicUrl(false);
        }
      });
    return () => {
      cancelled = true;
    };
  }, [form?.enableFederation]);

  const patch = (updates: Partial<SiteSettingsData>) => {
    setForm((prev) => (prev ? mergeSiteSettingsPatch(prev, updates) : prev));
  };

  const isDirty =
    base && form ? JSON.stringify(base) !== JSON.stringify(form) : false;

  const save = async () => {
    if (!form) return;
    if (form.enableFederation && !form.instanceName?.trim()) {
      toast.error("Instance name is required when federation is enabled.");
      return;
    }
    setSaving(true);
    try {
      const result = await updateSiteSettingsMerged(form);
      if (result?.success) {
        setBase(form);
        toast.success("Federation settings saved");
      } else {
        toast.error(result?.message ?? "Failed to save");
      }
    } catch {
      toast.error("Failed to save federation settings");
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
      title="This instance"
      description="How this PolyBucket node identifies itself in the federation network"
    >
      <SettingsToggle
        label="Enable federation"
        description="Allow peering with other PolyBucket instances"
        checked={form.enableFederation ?? false}
        onCheckedChange={(v) => patch({ enableFederation: v })}
      />
      {missingPublicUrl && (
        <p className="text-sm text-amber-400/90 mb-2">
          Peer instances need a public site URL. Set it in{" "}
          <Link href="/admin/system" className="underline hover:text-amber-300">
            Admin → System (Email)
          </Link>{" "}
          (or configure Instance:BaseUrl on the server). Until then, catalog and
          handshake peer endpoints return 503.
        </p>
      )}
      <SettingsField label="Instance name">
        <Input
          variant="glass"
          value={form.instanceName ?? ""}
          onChange={(e) => patch({ instanceName: e.target.value })}
          className="text-white"
        />
      </SettingsField>
      <SettingsField label="Instance description">
        <Input
          variant="glass"
          value={form.instanceDescription ?? ""}
          onChange={(e) => patch({ instanceDescription: e.target.value })}
          className="text-white"
        />
      </SettingsField>
      <SettingsField label="Admin contact email">
        <Input
          variant="glass"
          type="email"
          value={form.adminContact ?? ""}
          onChange={(e) => patch({ adminContact: e.target.value })}
          className="text-white"
        />
      </SettingsField>
      <SettingsFooter onSave={save} isSaving={saving} isDirty={isDirty} />
    </SettingsSection>
  );
}
