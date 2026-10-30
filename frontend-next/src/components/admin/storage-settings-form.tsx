"use client";

import { useEffect, useState } from "react";
import { Input } from "@/components/primitives/input";
import { Switch } from "@/components/primitives/switch";
import {
  formFromProviderDefaults,
  getStorageProviders,
  type StorageProviderCatalogEntry,
  type UpdateStorageSettingsRequest,
} from "@/lib/services/storageService";

const inputClass =
  "border-white/20 bg-white/5 text-white placeholder:text-white/50";

export interface StorageSettingsFormProps {
  form: UpdateStorageSettingsRequest;
  onChange: (form: UpdateStorageSettingsRequest) => void;
  hasStoredSecretKey?: boolean;
  showProviderPrompt?: boolean;
}

export function StorageSettingsForm({
  form,
  onChange,
  hasStoredSecretKey = false,
  showProviderPrompt = true,
}: StorageSettingsFormProps) {
  const [providers, setProviders] = useState<StorageProviderCatalogEntry[]>([]);
  const [providersLoading, setProvidersLoading] = useState(true);

  useEffect(() => {
    getStorageProviders()
      .then((response) => setProviders(response.providers))
      .catch(() => setProviders([]))
      .finally(() => setProvidersLoading(false));
  }, []);

  const selectedProvider = providers.find((p) => p.id === form.provider);

  const handleProviderChange = (providerId: string) => {
    const entry = providers.find((p) => p.id === providerId);
    if (!entry) {
      onChange({ ...form, provider: providerId });
      return;
    }

    onChange(formFromProviderDefaults(providerId, entry.defaults));
  };

  return (
    <div className="space-y-4">
      <div>
        <label className="text-sm text-white/80">Object storage provider</label>
        <select
          className={`mt-1 flex h-10 w-full rounded-md border bg-transparent px-3 py-2 text-sm text-white ${inputClass}`}
          value={form.provider}
          disabled={providersLoading}
          onChange={(e) => handleProviderChange(e.target.value)}
        >
          {showProviderPrompt && !form.provider ? (
            <option value="">Select a provider…</option>
          ) : null}
          {providers.map((p) => (
            <option key={p.id} value={p.id} className="text-black">
              {p.displayName}
            </option>
          ))}
        </select>
        {selectedProvider ? (
          <p className="text-white/60 text-sm mt-2">{selectedProvider.description}</p>
        ) : showProviderPrompt ? (
          <p className="text-white/60 text-sm mt-2">
            Choose MinIO, Silo, or SeaweedFS, then enter connection details for your
            deployment.
          </p>
        ) : null}
      </div>

      <div className="grid gap-4 sm:grid-cols-2">
        <div className="sm:col-span-2">
          <label className="text-sm text-white/80">Endpoint (internal)</label>
          <Input
            className={inputClass}
            value={form.endpoint}
            onChange={(e) => onChange({ ...form, endpoint: e.target.value })}
          />
        </div>
        <div>
          <label className="text-sm text-white/80">Port</label>
          <Input
            type="number"
            className={inputClass}
            value={form.port}
            onChange={(e) =>
              onChange({ ...form, port: Number(e.target.value) || 9000 })
            }
          />
        </div>
        <div>
          <label className="text-sm text-white/80">Bucket</label>
          <Input
            className={inputClass}
            value={form.bucketName}
            onChange={(e) => onChange({ ...form, bucketName: e.target.value })}
          />
        </div>
        <div>
          <label className="text-sm text-white/80">Access key</label>
          <Input
            className={inputClass}
            value={form.accessKey}
            onChange={(e) => onChange({ ...form, accessKey: e.target.value })}
          />
        </div>
        <div>
          <label className="text-sm text-white/80">
            Secret key {hasStoredSecretKey ? "(leave blank to keep)" : ""}
          </label>
          <Input
            type="password"
            className={inputClass}
            value={form.secretKey ?? ""}
            onChange={(e) => onChange({ ...form, secretKey: e.target.value })}
          />
        </div>
        <div>
          <label className="text-sm text-white/80">External endpoint</label>
          <Input
            className={inputClass}
            value={form.externalEndpoint ?? ""}
            onChange={(e) =>
              onChange({ ...form, externalEndpoint: e.target.value })
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
              onChange({
                ...form,
                externalPort: Number(e.target.value) || 9000,
              })
            }
          />
        </div>
        <div>
          <label className="text-sm text-white/80">Region</label>
          <Input
            className={inputClass}
            value={form.region}
            onChange={(e) => onChange({ ...form, region: e.target.value })}
          />
        </div>
        <div className="flex items-center gap-3 sm:col-span-2">
          <Switch
            checked={form.useSsl}
            onCheckedChange={(checked) => onChange({ ...form, useSsl: checked })}
          />
          <span className="text-sm text-white/80">Use SSL (internal endpoint)</span>
        </div>
        <div className="flex items-center gap-3 sm:col-span-2">
          <Switch
            checked={form.externalUseSsl ?? false}
            onCheckedChange={(checked) =>
              onChange({ ...form, externalUseSsl: checked })
            }
          />
          <span className="text-sm text-white/80">Use SSL (external endpoint)</span>
        </div>
        <div className="flex items-center gap-3 sm:col-span-2">
          <Switch
            checked={form.forcePathStyle}
            onCheckedChange={(checked) =>
              onChange({ ...form, forcePathStyle: checked })
            }
          />
          <span className="text-sm text-white/80">
            Path-style URLs (recommended for SeaweedFS S3 gateway)
          </span>
        </div>
      </div>
    </div>
  );
}

export function buildStorageRequest(
  form: UpdateStorageSettingsRequest
): UpdateStorageSettingsRequest {
  return {
    ...form,
    endpoint: form.endpoint.trim(),
    bucketName: form.bucketName.trim(),
    accessKey: form.accessKey.trim(),
    externalEndpoint: form.externalEndpoint?.trim() || undefined,
  };
}
