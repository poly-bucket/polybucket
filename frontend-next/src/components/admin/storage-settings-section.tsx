"use client";

import { useEffect, useState } from "react";
import { toast } from "sonner";
import { Button } from "@/components/primitives/button";
import {
  StorageSettingsForm,
  buildStorageRequest,
} from "@/components/admin/storage-settings-form";
import { SettingsSection } from "@/components/settings/settings-section";
import {
  formFromStorageSettings,
  getStorageApiErrorMessage,
  getStorageSettings,
  testStorageConfiguration,
  updateStorageSettings,
  type StorageSettings,
  type UpdateStorageSettingsRequest,
} from "@/lib/services/storageService";

export function StorageSettingsSection() {
  const [settings, setSettings] = useState<StorageSettings | null>(null);
  const [form, setForm] = useState<UpdateStorageSettingsRequest | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [testing, setTesting] = useState(false);

  useEffect(() => {
    getStorageSettings()
      .then((s) => {
        setSettings(s);
        setForm(formFromStorageSettings(s));
      })
      .catch(() => toast.error("Could not load storage settings"))
      .finally(() => setLoading(false));
  }, []);

  if (loading || !form) {
    return <p className="text-white/60 text-sm">Loading storage settings…</p>;
  }

  const handleSave = async () => {
    setSaving(true);
    try {
      const updated = await updateStorageSettings(buildStorageRequest(form));
      setSettings(updated);
      setForm(formFromStorageSettings(updated));
      toast.success("Storage settings saved. Restart the API container to apply.");
    } catch (e) {
      toast.error(getStorageApiErrorMessage(e, "Could not save storage settings"));
    } finally {
      setSaving(false);
    }
  };

  const handleTest = async () => {
    setTesting(true);
    try {
      const result = await testStorageConfiguration(buildStorageRequest(form));
      if (result.success) {
        toast.success(result.message);
      } else {
        toast.error(result.message);
      }
    } catch (e) {
      toast.error(getStorageApiErrorMessage(e, "Storage test failed"));
    } finally {
      setTesting(false);
    }
  };

  return (
    <SettingsSection
      title="Object storage"
      description="MinIO, Silo, or SeaweedFS for model files and images. Settings are stored in the database."
    >
      <StorageSettingsForm
        form={form}
        onChange={setForm}
        hasStoredSecretKey={settings?.hasSecretKey}
        showProviderPrompt={!form.provider}
      />

      <div className="flex gap-3 mt-6">
        <Button type="button" variant="secondary" onClick={handleTest} disabled={testing}>
          {testing ? "Testing…" : "Test connection"}
        </Button>
        <Button type="button" onClick={handleSave} disabled={saving}>
          {saving ? "Saving…" : "Save"}
        </Button>
      </div>
    </SettingsSection>
  );
}
