import type { FileTypeSettingsData } from "@/lib/api/client";
import { UpdateFileSettingsCommand } from "@/lib/api/client";

const BYTES_PER_MB = 1024 * 1024;

export function formatBytes(bytes?: number | null): string {
  if (bytes == null || bytes === 0) return "—";
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < BYTES_PER_MB) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / BYTES_PER_MB).toFixed(1)} MB`;
}

export function bytesToDisplayMb(bytes?: number | null): string {
  if (bytes == null || bytes <= 0) return "";
  const mb = bytes / BYTES_PER_MB;
  return Number.isInteger(mb) ? String(mb) : mb.toFixed(2);
}

export function displayMbToBytes(mb: number): number {
  return Math.round(mb * BYTES_PER_MB);
}

export function buildUpdateFileSettingsCommand(
  ft: FileTypeSettingsData,
  overrides: Partial<FileTypeSettingsData> = {}
): UpdateFileSettingsCommand {
  const merged = { ...ft, ...overrides };
  return new UpdateFileSettingsCommand({
    id: merged.id!,
    fileExtension: merged.fileExtension!,
    enabled: merged.enabled ?? true,
    maxFileSizeBytes: merged.maxFileSizeBytes,
    maxPerUpload: merged.maxPerUpload,
    displayName: merged.displayName,
    description: merged.description,
    mimeType: merged.mimeType,
    requiresPreview: merged.requiresPreview ?? false,
    isCompressible: merged.isCompressible ?? true,
    category: merged.category,
    priority: merged.priority ?? 0,
    isDefault: merged.isDefault ?? false,
  });
}

export type FileTypeFormValidation = {
  valid: boolean;
  displayName?: string;
  maxSizeMb?: string;
  maxPerUpload?: string;
  mimeType?: string;
  description?: string;
};

export function validateFileTypeForm(input: {
  displayName: string;
  maxSizeMb: string;
  maxPerUpload: string;
  mimeType: string;
  description: string;
  storageMaxBytes?: number;
}): FileTypeFormValidation {
  const errors: FileTypeFormValidation = { valid: true };

  const name = input.displayName.trim();
  if (!name) {
    errors.valid = false;
    errors.displayName = "Display name is required.";
  } else if (name.length > 100) {
    errors.valid = false;
    errors.displayName = "Display name must be at most 100 characters.";
  }

  const mb = parseFloat(input.maxSizeMb);
  if (!input.maxSizeMb.trim() || Number.isNaN(mb) || mb <= 0) {
    errors.valid = false;
    errors.maxSizeMb = "Enter a max size greater than 0 MB.";
  } else if (
    input.storageMaxBytes != null &&
    input.storageMaxBytes > 0 &&
    displayMbToBytes(mb) > input.storageMaxBytes
  ) {
    errors.valid = false;
    errors.maxSizeMb = `Max size cannot exceed storage limit (${formatBytes(input.storageMaxBytes)}).`;
  }

  const maxPer = parseInt(input.maxPerUpload, 10);
  if (Number.isNaN(maxPer) || maxPer < 1 || maxPer > 100) {
    errors.valid = false;
    errors.maxPerUpload = "Max per upload must be between 1 and 100.";
  }

  if ((input.mimeType ?? "").length > 50) {
    errors.valid = false;
    errors.mimeType = "MIME type must be at most 50 characters.";
  }

  if ((input.description ?? "").length > 500) {
    errors.valid = false;
    errors.description = "Description must be at most 500 characters.";
  }

  return errors;
}

export const FILE_TYPE_CATEGORY_OPTIONS = [
  "3D",
  "Image",
  "Document",
  "Archive",
] as const;

export function getFileTypeDisableConfirmReason(
  ft: FileTypeSettingsData,
  nextEnabled: boolean,
  fileTypes: FileTypeSettingsData[]
): string | null {
  if (nextEnabled || ft.enabled === false) return null;
  if (ft.isDefault) {
    return "This is a default file type. Disabling it may affect expected upload formats.";
  }
  const sameCategoryEnabled = fileTypes.filter(
    (row) =>
      row.category === ft.category && row.enabled && row.id !== ft.id
  );
  if (sameCategoryEnabled.length === 0) {
    return `This is the only enabled type in category "${ft.category ?? "Unknown"}". Disabling it may block uploads for that category.`;
  }
  return null;
}
