import { ApiClientFactory } from "@/lib/api/clientFactory";
import {
  ApiException,
  ProblemDetails,
  type GetUserSettingsResult,
  UpdateUserSettingsRequest,
} from "@/lib/api/client";
import { isAxiosError } from "axios";

function resolveSettingsFetchErrorStatus(error: unknown): number | undefined {
  if (ApiException.isApiException(error)) {
    return error.status;
  }
  if (isAxiosError(error) && typeof error.response?.status === "number") {
    return error.response.status;
  }
  if (error instanceof ProblemDetails && typeof error.status === "number") {
    return error.status;
  }
  if (typeof error === "object" && error !== null && "status" in error) {
    const maybeStatus = (error as { status?: unknown }).status;
    return typeof maybeStatus === "number" ? maybeStatus : undefined;
  }
  return undefined;
}

function isRecoverableSettingsFetchError(error: unknown): boolean {
  const status = resolveSettingsFetchErrorStatus(error);
  if (status !== undefined) {
    return status >= 400 && status < 500;
  }
  return error instanceof ProblemDetails;
}

const api = () => ApiClientFactory.getApiClient();

export interface UserSettingsData {
  language?: string;
  theme?: string;
  emailNotifications?: boolean;
  defaultPrinterId?: string;
  measurementSystem?: string;
  timeZone?: string;
  autoRotateModels?: boolean;
  notifyOnMentions?: boolean;
  notifyOnFollows?: boolean;
  notifyOnLikes?: boolean;
  notifyOnComments?: boolean;
  dashboardViewType?: string;
  cardSize?: string;
  cardSpacing?: string;
  gridColumns?: number;
}

export function getDefaultSettings(): UserSettingsData {
  return {
    language: "en",
    theme: "dark",
    emailNotifications: true,
    measurementSystem: "metric",
    timeZone: "UTC",
    autoRotateModels: true,
    notifyOnMentions: true,
    notifyOnFollows: true,
    notifyOnLikes: true,
    notifyOnComments: true,
    dashboardViewType: "grid",
    cardSize: "medium",
    cardSpacing: "normal",
    gridColumns: 4,
  };
}

export async function getUserSettings(): Promise<UserSettingsData | null> {
  try {
    const response: GetUserSettingsResult = await api().getUserSettings_GetUserSettings();
    const settings = response?.settings;
    if (!settings) return getDefaultSettings();
    return {
      language: settings.language ?? "en",
      theme: settings.theme ?? "dark",
      emailNotifications: settings.emailNotifications ?? true,
      defaultPrinterId: settings.defaultPrinterId,
      measurementSystem: settings.measurementSystem ?? "metric",
      timeZone: settings.timeZone ?? "UTC",
      autoRotateModels: settings.autoRotateModels ?? true,
      notifyOnMentions: settings.notifyOnMentions ?? true,
      notifyOnFollows: settings.notifyOnFollows ?? true,
      notifyOnLikes: settings.notifyOnLikes ?? true,
      notifyOnComments: settings.notifyOnComments ?? true,
      dashboardViewType: settings.dashboardViewType ?? "grid",
      cardSize: settings.cardSize ?? "medium",
      cardSpacing: settings.cardSpacing ?? "normal",
      gridColumns: settings.gridColumns ?? 4,
    };
  } catch (error) {
    if (isRecoverableSettingsFetchError(error)) {
      return getDefaultSettings();
    }

    console.error("Error fetching user settings:", error);
    return getDefaultSettings();
  }
}

export async function updateUserSettings(
  partial: Partial<UserSettingsData>
): Promise<boolean> {
  try {
    const request = new UpdateUserSettingsRequest({
      language: partial.language,
      theme: partial.theme,
      emailNotifications: partial.emailNotifications,
      defaultPrinterId: partial.defaultPrinterId,
      measurementSystem: partial.measurementSystem,
      timeZone: partial.timeZone,
      autoRotateModels: partial.autoRotateModels,
      notifyOnMentions: partial.notifyOnMentions,
      notifyOnFollows: partial.notifyOnFollows,
      notifyOnLikes: partial.notifyOnLikes,
      notifyOnComments: partial.notifyOnComments,
      dashboardViewType: partial.dashboardViewType,
      cardSize: partial.cardSize,
      cardSpacing: partial.cardSpacing,
      gridColumns: partial.gridColumns,
    });
    await api().updateUserSettings_UpdateUserSettings(request);
    return true;
  } catch (error) {
    console.error("Error updating user settings:", error);
    return false;
  }
}
