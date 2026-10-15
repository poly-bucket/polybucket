"use client";

import { useState, useCallback, useEffect } from "react";
import { useAuth } from "@/contexts/AuthContext";
import { SettingsSection } from "@/components/settings/settings-section";
import { SettingsToggle } from "@/components/settings/settings-toggle";
import { SettingsFooter } from "@/components/settings/settings-footer";
import {
  getModerationSettings,
  updateModerationSettings,
  type ModerationSettings,
} from "@/lib/services/moderationSettingsService";
import { toast } from "sonner";
import { SettingsSectionsSkeleton } from "@/components/ui/skeletons";

export function ModerationSettingsSection() {
  const { user } = useAuth();
  const isAdmin = !!user?.roles?.some((r) => r?.toLowerCase() === "admin");

  const [base, setBase] = useState<ModerationSettings | null>(null);
  const [form, setForm] = useState<ModerationSettings | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const settings = await getModerationSettings();
      setBase(settings);
      setForm(settings);
    } catch {
      setError("Failed to load moderation settings");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    load();
  }, [load]);

  const patch = (updates: Partial<ModerationSettings>) => {
    setForm((prev) => (prev ? { ...prev, ...updates } : prev));
  };

  const isDirty =
    base && form ? JSON.stringify(base) !== JSON.stringify(form) : false;

  const save = async () => {
    if (!form || !isAdmin) return;
    setSaving(true);
    try {
      const updated = await updateModerationSettings(form);
      setBase(updated);
      setForm(updated);
      toast.success("Moderation settings saved");
    } catch {
      toast.error("Failed to save moderation settings");
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <SettingsSectionsSkeleton sections={1} togglesPerSection={5} pageTitleWidth="w-48" />
    );
  }

  if (!form) {
    return null;
  }

  return (
    <SettingsSection
      title="Model moderation workflow"
      description={
        isAdmin
          ? "Control when uploads require review and how they are approved"
          : "View site moderation policy (only admins can change these settings)"
      }
    >
      {error && (
        <div className="mb-4 rounded-lg border border-red-500/50 bg-red-500/10 px-4 py-3 text-red-400">
          {error}
        </div>
      )}
      <SettingsToggle
        label="Require moderation"
        description="New models must pass moderation before they are public"
        checked={form.requireModeration ?? true}
        onCheckedChange={(v) => patch({ requireModeration: v })}
        disabled={!isAdmin}
      />
      <SettingsToggle
        label="Auto-approve models"
        description="Skip the queue when moderation is enabled but auto-approve is on"
        checked={form.autoApproveModels ?? false}
        onCheckedChange={(v) => patch({ autoApproveModels: v })}
        disabled={!isAdmin}
      />
      <SettingsToggle
        label="Require upload moderation"
        description="Every upload enters the moderation pipeline"
        checked={form.requireUploadModeration ?? true}
        onCheckedChange={(v) => patch({ requireUploadModeration: v })}
        disabled={!isAdmin}
      />
      <SettingsToggle
        label="Require moderator approval"
        description="A moderator must explicitly approve each model"
        checked={form.requireModeratorApproval ?? true}
        onCheckedChange={(v) => patch({ requireModeratorApproval: v })}
        disabled={!isAdmin}
      />
      <SettingsToggle
        label="Auto-approve verified users"
        description="Trusted verified accounts can bypass the queue"
        checked={form.autoApproveVerifiedUsers ?? false}
        onCheckedChange={(v) => patch({ autoApproveVerifiedUsers: v })}
        disabled={!isAdmin}
      />
      {isAdmin && (
        <SettingsFooter onSave={save} isSaving={saving} isDirty={isDirty} />
      )}
    </SettingsSection>
  );
}
