import { isAxiosError } from "axios";
import axiosInstance from "@/lib/api/axiosConfig";

export interface AccountExportData {
  exportedAtUtc: string;
  userId: string;
  email: string;
  username: string;
  firstName?: string;
  lastName?: string;
  bio?: string;
  country?: string;
  websiteUrl?: string;
  twitterUrl?: string;
  instagramUrl?: string;
  youTubeUrl?: string;
  isProfilePublic: boolean;
  showEmail: boolean;
  showLastLogin: boolean;
  showStatistics: boolean;
  createdAt: string;
  lastLoginAt?: string;
  roleName?: string;
  modelCount?: number;
  collectionCount?: number;
  favoriteCollectionCount?: number;
  recentModels?: Array<{
    modelId: string;
    name: string;
    createdAt: string;
    privacy: string;
  }>;
  recentCollections?: Array<{
    collectionId: string;
    name: string;
    visibility: string;
    createdAt: string;
  }>;
  settings?: {
    language: string;
    theme: string;
    emailNotifications: boolean;
    measurementSystem: string;
    timeZone: string;
  };
}

function getErrorMessage(err: unknown): string {
  if (isAxiosError(err)) {
    const data = err.response?.data as { message?: string; title?: string; detail?: string } | undefined;
    if (data?.message) {
      return data.message;
    }
    if (data?.detail) {
      return data.detail;
    }
    if (data?.title) {
      return data.title;
    }
  }
  if (err instanceof Error && err.message) {
    return err.message;
  }
  return "Request failed";
}

export async function exportAccountData(): Promise<AccountExportData> {
  try {
    const { data } = await axiosInstance.get<AccountExportData>("/api/auth/account/export");
    return data;
  } catch (err: unknown) {
    throw new Error(getErrorMessage(err));
  }
}

export async function revokeAllSessions(): Promise<void> {
  try {
    await axiosInstance.post("/api/auth/account/sessions/revoke-all");
  } catch (err: unknown) {
    throw new Error(getErrorMessage(err));
  }
}

export interface ActiveSession {
  sessionId: string;
  createdAt: string;
  expiresAt: string;
  createdByIp: string;
}

export async function getActiveSessions(): Promise<ActiveSession[]> {
  try {
    const { data } = await axiosInstance.get<{ sessions?: ActiveSession[] }>("/api/auth/account/sessions");
    return data.sessions ?? [];
  } catch (err: unknown) {
    throw new Error(getErrorMessage(err));
  }
}

export async function revokeSession(sessionId: string): Promise<void> {
  try {
    await axiosInstance.post(`/api/auth/account/sessions/${sessionId}/revoke`);
  } catch (err: unknown) {
    throw new Error(getErrorMessage(err));
  }
}

export interface DeleteAccountPayload {
  password: string;
  twoFactorToken?: string;
  backupCode?: string;
}

export async function deleteAccount(payload: DeleteAccountPayload): Promise<void> {
  try {
    await axiosInstance.post("/api/auth/account/delete", {
      password: payload.password,
      twoFactorToken: payload.twoFactorToken,
      backupCode: payload.backupCode,
    });
  } catch (err: unknown) {
    throw new Error(getErrorMessage(err));
  }
}
