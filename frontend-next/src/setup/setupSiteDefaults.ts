import { PrivacySettings } from "@/lib/api/client";

export const DEFAULT_ALLOWED_FILE_TYPES =
  ".stl,.obj,.fbx,.3ds,.glb,.gltf,.ply,.step,.stp,.iges,.igs,.brep,.png,.jpg,.jpeg,.gif";

export const DEFAULT_SITE_DESCRIPTION = "3D Model Repository";

export const defaultNonSecuritySiteSettings = {
  siteDescription: DEFAULT_SITE_DESCRIPTION,
  disableEmailSettings: false,
  maxFileSizeBytes: 100 * 1024 * 1024,
  allowedFileTypes: DEFAULT_ALLOWED_FILE_TYPES,
  maxFilesPerUpload: 5,
  enableFileCompression: true,
  autoGeneratePreviews: true,
  defaultModelPrivacy: PrivacySettings.Public,
  defaultUserRole: "User",
  allowCustomRoles: false,
  defaultTheme: "dark",
  defaultLanguage: "en",
} as const;

export const defaultSecuritySiteSettings = {
  requireLoginForUpload: false,
  requireEmailVerification: false,
  maxFailedLoginAttempts: 5,
  lockoutDurationMinutes: 15,
  requireStrongPasswords: true,
  passwordMinLength: 8,
  autoApproveModels: false,
  requireModeration: true,
} as const;

function readNum(
  v: unknown,
  fallback: number,
  min: number,
  max: number
): number {
  const n = typeof v === "number" ? v : Number(v);
  if (!Number.isFinite(n)) return fallback;
  return Math.min(max, Math.max(min, n));
}

function readBool(v: unknown, fallback: boolean): boolean {
  if (typeof v === "boolean") return v;
  return fallback;
}

export function readSiteSecurityFromData(
  data: Record<string, unknown>
) {
  const d = defaultSecuritySiteSettings;
  return {
    requireLoginForUpload: readBool(data.requireLoginForUpload, d.requireLoginForUpload),
    requireEmailVerification: readBool(
      data.requireEmailVerification,
      d.requireEmailVerification
    ),
    maxFailedLoginAttempts: readNum(
      data.maxFailedLoginAttempts,
      d.maxFailedLoginAttempts,
      1,
      10
    ),
    lockoutDurationMinutes: readNum(
      data.lockoutDurationMinutes,
      d.lockoutDurationMinutes,
      5,
      60
    ),
    requireStrongPasswords: readBool(
      data.requireStrongPasswords,
      d.requireStrongPasswords
    ),
    passwordMinLength: readNum(
      data.passwordMinLength,
      d.passwordMinLength,
      6,
      20
    ),
    autoApproveModels: readBool(data.autoApproveModels, d.autoApproveModels),
    requireModeration: readBool(data.requireModeration, d.requireModeration),
  };
}
