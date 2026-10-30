"use client";

import { useState } from "react";
import { toast } from "sonner";
import { Button } from "@/components/primitives/button";
import {
  StorageSettingsForm,
  buildStorageRequest,
} from "@/components/admin/storage-settings-form";
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

const emptyForm: UpdateStorageSettingsRequest = {
  provider: "",
  endpoint: "",
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
  forcePathStyle: false,
};

export default function StorageSetupStep({
  onComplete,
  onBack,
}: StorageSetupStepProps) {
  const [form, setForm] = useState(emptyForm);
  const [isTesting, setIsTesting] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [testPassed, setTestPassed] = useState(false);

  const handleTest = async () => {
    if (!form.provider) {
      toast.error("Select an object storage provider first.");
      return;
    }

    setIsTesting(true);
    setTestPassed(false);
    try {
      const result = await testStorageConfiguration(buildStorageRequest(form));
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
    if (!form.provider) {
      toast.error("Select an object storage provider first.");
      return;
    }

    if (!testPassed) {
      toast.error("Run a successful connection test before continuing.");
      return;
    }

    setIsSaving(true);
    try {
      await updateStorageSettings(buildStorageRequest(form));
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

  return (
    <div className="space-y-6">
      <div>
        <h3 className="text-lg font-medium text-white mb-2">Object storage</h3>
        <p className="text-white/70 text-sm">
          PolyBucket stores uploads in MinIO, Silo, or SeaweedFS. Use the internal
          hostname from Docker for the API and a public hostname for browser
          downloads (for example <code className="text-white/90">localhost</code>).
        </p>
      </div>

      <StorageSettingsForm form={form} onChange={setForm} showProviderPrompt />

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
