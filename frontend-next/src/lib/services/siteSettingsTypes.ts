export interface SiteSettingsData {
  siteName?: string;
  siteDescription?: string;
  contactEmail?: string;
  allowPublicBrowsing?: boolean;
  requireLoginForUpload?: boolean;
  allowUserRegistration?: boolean;
  requireEmailVerification?: boolean;
  disableEmailSettings?: boolean;
  maxFileSizeBytes?: number;
  allowedFileTypes?: string;
  maxFilesPerUpload?: number;
  enableFileCompression?: boolean;
  autoGeneratePreviews?: boolean;
  defaultModelPrivacy?: string;
  autoApproveModels?: boolean;
  requireModeration?: boolean;
  defaultUserRole?: string;
  allowCustomRoles?: boolean;
  maxFailedLoginAttempts?: number;
  lockoutDurationMinutes?: number;
  requireStrongPasswords?: boolean;
  passwordMinLength?: number;
  defaultTheme?: string;
  defaultLanguage?: string;
  showAdvancedOptions?: boolean;
  enableFederation?: boolean;
  instanceName?: string;
  instanceDescription?: string;
  adminContact?: string | null;
}

export interface GetSiteSettingsResponse {
  success?: boolean;
  message?: string;
  settings?: SiteSettingsData | null;
}

export type SiteSettingsPatch = Partial<SiteSettingsData>;
