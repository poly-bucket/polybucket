"use client";

import { useEffect, useState } from "react";
import { toast } from "sonner";
import { Button } from "@/components/primitives/button";
import { Input } from "@/components/primitives/input";
import { SettingsSection } from "@/components/admin/settings-section";
import {
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
        setForm({
          provider: s.provider || "minio",
          endpoint: s.endpoint || "",
          port: s.port || 9000,
          useSsl: s.useSsl,
          bucketName: s.bucketName || "",
          accessKey: s.accessKey || "",
          secretKey: "",
          clearSecretKey: false,
          externalEndpoint: s.externalEndpoint ?? undefined,
          externalPort: s.externalPort ?? undefined,
          externalUseSsl: s.externalUseSsl ?? undefined,
          region: s.region || "us-east-1",
        });
      })
      .catch(() => toast.error("Could not load storage settings"))
      .finally(() => setLoading(false));
  }, []);

  if (loading || !form) {
    return <p className="text-white/60 text-sm">Loading storage settings…</p>;
  }

  const inputClass =
    "border-white/20 bg-white/5 text-white placeholder:text-white/50";

  const handleSave = async () => {
    setSaving(true);
    try {
      const updated = await updateStorageSettings(form);
      setSettings(updated);
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
      const result = await testStorageConfiguration(form);
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
      description="MinIO or S3-compatible storage for model files and images. Settings are stored in the database."
    >
      {settings?.managedByEnvironment.length ? (
        <p className="text-amber-200/90 text-sm mb-4">
          Some fields are locked by environment variables:{" "}
          {settings.managedByEnvironment.join(", ")}
        </p>
      ) : null}

      <div className="grid gap-4 sm:grid-cols-2">
        <div className="sm:col-span-2">
          <label className="text-sm text-white/80">Endpoint</label>
          <Input
            className={inputClass}
            value={form.endpoint}
            disabled={settings?.managedByEnvironment.includes("Endpoint")}
            onChange={(e) => setForm({ ...form, endpoint: e.target.value })}
          />
        </div>
        <div>
          <label className="text-sm text-white/80">Port</label>
          <Input
            type="number"
            className={inputClass}
            value={form.port}
            onChange={(e) =>
              setForm({ ...form, port: Number(e.target.value) || 9000 })
            }
          />
        </div>
        <div>
          <label className="text-sm text-white/80">Bucket</label>
          <Input
            className={inputClass}
            value={form.bucketName}
            onChange={(e) => setForm({ ...form, bucketName: e.target.value })}
          />
        </div>
        <div>
          <label className="text-sm text-white/80">Access key</label>
          <Input
            className={inputClass}
            value={form.accessKey}
            onChange={(e) => setForm({ ...form, accessKey: e.target.value })}
          />
        </div>
        <div>
          <label className="text-sm text-white/80">
            Secret key {settings?.hasSecretKey ? "(leave blank to keep)" : ""}
          </label>
          <Input
            type="password"
            className={inputClass}
            value={form.secretKey ?? ""}
            onChange={(e) => setForm({ ...form, secretKey: e.target.value })}
          />
        </div>
      </div>

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
