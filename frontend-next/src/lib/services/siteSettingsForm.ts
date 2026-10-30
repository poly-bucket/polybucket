import { UpdateSiteSettingsCommand } from "@/lib/api/client";
import type { SiteSettingsData, SiteSettingsPatch } from "./siteSettingsTypes";

export function siteSettingsToCommand(
  settings: SiteSettingsData
): UpdateSiteSettingsCommand {
  return new UpdateSiteSettingsCommand({
    siteName: settings.siteName ?? "",
    siteDescription: settings.siteDescription ?? "",
    contactEmail: settings.contactEmail ?? "",
    allowPublicBrowsing: settings.allowPublicBrowsing ?? true,
    requireLoginForUpload: settings.requireLoginForUpload ?? true,
    allowUserRegistration: settings.allowUserRegistration ?? false,
    requireEmailVerification: settings.requireEmailVerification ?? false,
    disableEmailSettings: settings.disableEmailSettings ?? false,
    maxFileSizeBytes: settings.maxFileSizeBytes,
    allowedFileTypes: settings.allowedFileTypes ?? "",
    maxFilesPerUpload: settings.maxFilesPerUpload,
    enableFileCompression: settings.enableFileCompression,
    autoGeneratePreviews: settings.autoGeneratePreviews,
    defaultModelPrivacy: settings.defaultModelPrivacy as UpdateSiteSettingsCommand["defaultModelPrivacy"],
    autoApproveModels: settings.autoApproveModels,
    requireModeration: settings.requireModeration,
    defaultUserRole: settings.defaultUserRole,
    allowCustomRoles: settings.allowCustomRoles,
    maxFailedLoginAttempts: settings.maxFailedLoginAttempts,
    lockoutDurationMinutes: settings.lockoutDurationMinutes,
    requireStrongPasswords: settings.requireStrongPasswords,
    passwordMinLength: settings.passwordMinLength,
    defaultTheme: settings.defaultTheme,
    defaultLanguage: settings.defaultLanguage,
    showAdvancedOptions: settings.showAdvancedOptions,
    enableFederation: settings.enableFederation,
    instanceName: settings.instanceName,
    instanceDescription: settings.instanceDescription,
    adminContact: settings.adminContact ?? undefined,
  });
}

export function mergeSiteSettingsPatch(
  base: SiteSettingsData,
  patch: SiteSettingsPatch
): SiteSettingsData {
  return { ...base, ...patch };
}
