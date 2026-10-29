import { isAxiosError } from "axios";
import axiosInstance from "@/lib/api/axiosConfig";

export type StorageSettingSource = "Default" | "Database" | "Environment";

export interface StorageSettings {
  provider: string;
  endpoint: string;
  port: number;
  useSsl: boolean;
  bucketName: string;
  accessKey: string;
  hasSecretKey: boolean;
  externalEndpoint?: string | null;
  externalPort?: number | null;
  externalUseSsl?: boolean | null;
  region: string;
  isConfigured: boolean;
  lastSuccessfulTestAt?: string | null;
  validationErrors: string[];
  sources: Record<string, StorageSettingSource>;
  managedByEnvironment: string[];
}

export interface UpdateStorageSettingsRequest {
  provider: string;
  endpoint: string;
  port: number;
  useSsl: boolean;
  bucketName: string;
  accessKey: string;
  secretKey?: string;
  clearSecretKey: boolean;
  externalEndpoint?: string;
  externalPort?: number;
  externalUseSsl?: boolean;
  region: string;
}

export interface TestStorageConfigurationResult {
  success: boolean;
  message: string;
  testedAt?: string | null;
}

export interface ApplyStorageSetupResponse {
  success: boolean;
  message: string;
  restartScheduled?: boolean;
}

export async function getStorageSettings(): Promise<StorageSettings> {
  const { data } = await axiosInstance.get<StorageSettings>(
    "/api/system-settings/storage"
  );
  return data;
}

export async function updateStorageSettings(
  request: UpdateStorageSettingsRequest
): Promise<StorageSettings> {
  const { data } = await axiosInstance.put<StorageSettings>(
    "/api/system-settings/storage",
    request
  );
  return data;
}

export async function testStorageConfiguration(
  request?: UpdateStorageSettingsRequest
): Promise<TestStorageConfigurationResult> {
  const { data } = await axiosInstance.post<TestStorageConfigurationResult>(
    "/api/system-settings/storage/test",
    request ?? null
  );
  return data;
}

export async function applyStorageSetup(): Promise<ApplyStorageSetupResponse> {
  const { data } = await axiosInstance.post<ApplyStorageSetupResponse>(
    "/api/SystemSetup/apply-storage"
  );
  return data;
}

export async function skipEmailSetup(): Promise<{ success: boolean; message: string }> {
  const { data } = await axiosInstance.post<{ success: boolean; message: string }>(
    "/api/SystemSetup/skip-email"
  );
  return data;
}

export async function waitForApiHealth(
  apiBaseUrl: string,
  timeoutMs = 120_000,
  intervalMs = 2000
): Promise<boolean> {
  const deadline = Date.now() + timeoutMs;
  const healthUrl = `${apiBaseUrl.replace(/\/$/, "")}/health`;

  while (Date.now() < deadline) {
    try {
      const res = await fetch(healthUrl, { cache: "no-store" });
      if (res.ok) {
        return true;
      }
    } catch {
      // API still restarting
    }
    await new Promise((r) => setTimeout(r, intervalMs));
  }

  return false;
}

export function getStorageApiErrorMessage(error: unknown, fallback: string): string {
  if (!isAxiosError(error)) {
    return fallback;
  }

  const body = error.response?.data as
    | { detail?: string; title?: string; errors?: Record<string, string[]>; message?: string }
    | undefined;
  if (body?.errors) {
    const messages = Object.values(body.errors).flat().filter(Boolean);
    if (messages.length > 0) {
      return messages.join(" ");
    }
  }

  return body?.detail ?? body?.message ?? body?.title ?? fallback;
}
