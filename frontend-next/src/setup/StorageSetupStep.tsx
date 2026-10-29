"use client";

import { useState } from "react";
import { toast } from "sonner";
import { Button } from "@/components/primitives/button";
import { Input } from "@/components/primitives/input";
import {
  applyStorageSetup,
  getStorageApiErrorMessage,
  testStorageConfiguration,
  updateStorageSettings,
  waitForApiHealth,
  type UpdateStorageSettingsRequest,
} from "@/lib/services/storageService";

interface StorageSetupStepProps {
  onComplete: (data: Record<string, unknown>) => void;
  onBack: () => void;
}

const defaultForm: UpdateStorageSettingsRequest = {
  provider: "minio",
  endpoint: "minio",
  port: 9000,
  useSsl: false,
  bucketName: "polybucket-uploads",
  accessKey: "",
  secretKey: "",
  clearSecretKey: false,
  externalEndpoint: "localhost",
  externalPort: 9000,
  externalUseSsl: false,
  region: "us-east-1",
};

export default function StorageSetupStep({
  onComplete,
  onBack,
}: StorageSetupStepProps) {
  const [form, setForm] = useState(defaultForm);
  const [isTesting, setIsTesting] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [testPassed, setTestPassed] = useState(false);

  const buildRequest = (): UpdateStorageSettingsRequest => ({
    ...form,
    endpoint: form.endpoint.trim(),
    bucketName: form.bucketName.trim(),
    accessKey: form.accessKey.trim(),
    externalEndpoint: form.externalEndpoint?.trim() || undefined,
  });

  const handleTest = async () => {
    setIsTesting(true);
    setTestPassed(false);
    try {
      const result = await testStorageConfiguration(buildRequest());
      if (result.success) {
        setTestPassed(true);
        toast.success(result.message);
      } else {
        toast.error(result.message);
      }
    } catch (e) {
      toast.error(getStorageApiErrorMessage(e, "Storage test failed"));
    } finally {
      setIsTesting(false);
    }
  };

  const handleSaveAndApply = async () => {
    if (!testPassed) {
      toast.error("Run a successful connection test before continuing.");
      return;
    }

    setIsSaving(true);
    try {
      await updateStorageSettings(buildRequest());
      const apply = await applyStorageSetup();
      if (!apply.success) {
        toast.error(apply.message);
        return;
      }

      toast.info("Restarting API to connect to object storage…");
      const apiUrl =
        process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:11666";
      const healthy = await waitForApiHealth(apiUrl);
      if (!healthy) {
        toast.error(
          "The API did not come back online in time. Check container logs and refresh this page."
        );
        return;
      }

      toast.success("Object storage is connected.");
      onComplete({ storageConfigured: true });
    } catch (e) {
      toast.error(getStorageApiErrorMessage(e, "Could not save storage settings"));
    } finally {
      setIsSaving(false);
    }
  };

  const inputClass =
    "border-white/20 bg-white/5 text-white placeholder:text-white/50";

  return (
    <div className="space-y-6">
      <div>
        <h3 className="text-lg font-medium text-white mb-2">Object storage</h3>
        <p className="text-white/70 text-sm">
          PolyBucket stores uploads in MinIO or another S3-compatible service.
          Use the internal hostname from Docker (for example{" "}
          <code className="text-white/90">minio</code>) and a public hostname
          for browser downloads (for example{" "}
          <code className="text-white/90">localhost</code>).
        </p>
      </div>

      <div className="grid gap-4 sm:grid-cols-2">
        <div className="sm:col-span-2">
          <label className="text-sm text-white/80">Endpoint (internal)</label>
          <Input
            className={inputClass}
            value={form.endpoint}
            onChange={(e) => setForm((f) => ({ ...f, endpoint: e.target.value }))}
          />
        </div>
        <div>
          <label className="text-sm text-white/80">Port</label>
          <Input
            type="number"
            className={inputClass}
            value={form.port}
            onChange={(e) =>
              setForm((f) => ({ ...f, port: Number(e.target.value) || 9000 }))
            }
          />
        </div>
        <div>
          <label className="text-sm text-white/80">Bucket</label>
          <Input
            className={inputClass}
            value={form.bucketName}
            onChange={(e) =>
              setForm((f) => ({ ...f, bucketName: e.target.value }))
            }
          />
        </div>
        <div>
          <label className="text-sm text-white/80">Access key</label>
          <Input
            className={inputClass}
            value={form.accessKey}
            onChange={(e) =>
              setForm((f) => ({ ...f, accessKey: e.target.value }))
            }
          />
        </div>
        <div>
          <label className="text-sm text-white/80">Secret key</label>
          <Input
            type="password"
            className={inputClass}
            value={form.secretKey ?? ""}
            onChange={(e) =>
              setForm((f) => ({ ...f, secretKey: e.target.value }))
            }
          />
        </div>
        <div>
          <label className="text-sm text-white/80">External endpoint</label>
          <Input
            className={inputClass}
            value={form.externalEndpoint ?? ""}
            onChange={(e) =>
              setForm((f) => ({ ...f, externalEndpoint: e.target.value }))
            }
          />
        </div>
        <div>
          <label className="text-sm text-white/80">External port</label>
          <Input
            type="number"
            className={inputClass}
            value={form.externalPort ?? 9000}
            onChange={(e) =>
              setForm((f) => ({
                ...f,
                externalPort: Number(e.target.value) || 9000,
              }))
            }
          />
        </div>
      </div>

      <div className="flex flex-wrap gap-3">
        <Button
          type="button"
          variant="outline"
          onClick={onBack}
          className="border-white/20 text-white hover:bg-white/10"
        >
          Back
        </Button>
        <Button
          type="button"
          variant="secondary"
          onClick={handleTest}
          disabled={isTesting || isSaving}
        >
          {isTesting ? "Testing…" : "Test connection"}
        </Button>
        <Button
          type="button"
          onClick={handleSaveAndApply}
          disabled={isSaving || isTesting}
        >
          {isSaving ? "Saving & restarting API…" : "Save and apply"}
        </Button>
      </div>
    </div>
  );
}
