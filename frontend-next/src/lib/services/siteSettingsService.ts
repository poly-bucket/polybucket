import axiosInstance from "@/lib/api/axiosConfig";
import type {
  GetSiteSettingsResponse,
  SiteSettingsData,
} from "./siteSettingsTypes";
import { siteSettingsToCommand } from "./siteSettingsForm";
import type { UpdateSiteSettingsResponse } from "@/lib/api/client";
import { ApiClientFactory } from "@/lib/api/clientFactory";

function mapSettings(raw: Record<string, unknown> | undefined): SiteSettingsData | null {
  if (!raw) return null;
  return {
    siteName: raw.siteName as string | undefined,
    siteDescription: raw.siteDescription as string | undefined,
    contactEmail: raw.contactEmail as string | undefined,
    allowPublicBrowsing: raw.allowPublicBrowsing as boolean | undefined,
    requireLoginForUpload: raw.requireLoginForUpload as boolean | undefined,
    allowUserRegistration: raw.allowUserRegistration as boolean | undefined,
    requireEmailVerification: raw.requireEmailVerification as boolean | undefined,
    disableEmailSettings: raw.disableEmailSettings as boolean | undefined,
    maxFileSizeBytes: raw.maxFileSizeBytes as number | undefined,
    allowedFileTypes: raw.allowedFileTypes as string | undefined,
    maxFilesPerUpload: raw.maxFilesPerUpload as number | undefined,
    enableFileCompression: raw.enableFileCompression as boolean | undefined,
    autoGeneratePreviews: raw.autoGeneratePreviews as boolean | undefined,
    defaultModelPrivacy: raw.defaultModelPrivacy as string | undefined,
    autoApproveModels: raw.autoApproveModels as boolean | undefined,
    requireModeration: raw.requireModeration as boolean | undefined,
    defaultUserRole: raw.defaultUserRole as string | undefined,
    allowCustomRoles: raw.allowCustomRoles as boolean | undefined,
    maxFailedLoginAttempts: raw.maxFailedLoginAttempts as number | undefined,
    lockoutDurationMinutes: raw.lockoutDurationMinutes as number | undefined,
    requireStrongPasswords: raw.requireStrongPasswords as boolean | undefined,
    passwordMinLength: raw.passwordMinLength as number | undefined,
    defaultTheme: raw.defaultTheme as string | undefined,
    defaultLanguage: raw.defaultLanguage as string | undefined,
    showAdvancedOptions: raw.showAdvancedOptions as boolean | undefined,
    enableFederation: raw.enableFederation as boolean | undefined,
    instanceName: raw.instanceName as string | undefined,
    instanceDescription: raw.instanceDescription as string | undefined,
    adminContact: raw.adminContact as string | null | undefined,
  };
}

export async function getSiteSettings(): Promise<SiteSettingsData | null> {
  const { data } = await axiosInstance.get<GetSiteSettingsResponse>(
    "/api/SystemSetup/site-settings"
  );
  const settings = data.settings ?? (data as { Settings?: Record<string, unknown> }).Settings;
  return mapSettings(settings as Record<string, unknown> | undefined);
}

export async function updateSiteSettingsMerged(
  settings: SiteSettingsData
): Promise<UpdateSiteSettingsResponse> {
  const command = siteSettingsToCommand(settings);
  return ApiClientFactory.getApiClient().systemSetup_UpdateSiteSettings(command);
}
