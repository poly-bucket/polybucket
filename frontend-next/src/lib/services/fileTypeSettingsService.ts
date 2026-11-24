import { ApiClientFactory } from "@/lib/api/clientFactory";
import type {
  FileTypeSettingsData,
  GetFileSettingsResponse,
} from "@/lib/api/client";
import { FileTypeSettingsData as FileTypeSettingsDataClass } from "@/lib/api/client";

export type { FileTypeSettingsData };

export interface FileTypeSettingsServiceResult {
  fileTypes: FileTypeSettingsData[];
  getExtensionsByCategory: (category: string) => string[];
  isFileAllowed: (file: File) => boolean;
}

const client = () => ApiClientFactory.getApiClient();

export const DEFAULT_MAX_FILES_PER_UPLOAD = 5;
export const DEFAULT_SITE_MAX_FILE_SIZE_BYTES = 100 * 1024 * 1024;

export interface ModelUploadConfig {
  fileTypes: FileTypeSettingsData[];
  maxFilesPerUpload: number;
  siteMaxFileSizeBytes: number;
}

export async function getFileSettings(): Promise<GetFileSettingsResponse> {
  return client().getFileSettings_GetFileSettings(1, 100, undefined, undefined);
}

function fileTypesFromFileConfig(
  config: Awaited<ReturnType<ReturnType<typeof client>["getFileConfig_GetFileConfig"]>>
): FileTypeSettingsData[] {
  const grouped = config.fileTypes ?? {};
  const result: FileTypeSettingsData[] = [];

  for (const list of Object.values(grouped)) {
    if (!Array.isArray(list)) continue;
    for (const item of list) {
      result.push(
        new FileTypeSettingsDataClass({
          fileExtension: item.extension,
          displayName: item.displayName,
          description: item.description,
          mimeType: item.mimeType,
          maxFileSizeBytes: item.maxFileSizeBytes,
          maxPerUpload: item.maxPerUpload,
          requiresPreview: item.requiresPreview,
          isCompressible: item.isCompressible,
          category: item.category,
          priority: item.priority,
          enabled: true,
        })
      );
    }
  }

  return result;
}

export async function getModelUploadConfig(): Promise<ModelUploadConfig> {
  const config = await client().getFileConfig_GetFileConfig();
  return {
    fileTypes: fileTypesFromFileConfig(config),
    maxFilesPerUpload:
      config.maxFilesPerUpload ?? DEFAULT_MAX_FILES_PER_UPLOAD,
    siteMaxFileSizeBytes:
      config.maxTotalFileSize ?? DEFAULT_SITE_MAX_FILE_SIZE_BYTES,
  };
}

export async function getFileTypesForUpload(): Promise<FileTypeSettingsData[]> {
  const uploadConfig = await getModelUploadConfig();
  return uploadConfig.fileTypes;
}

export function getEnabledFileTypesByCategory(
  fileTypes: FileTypeSettingsData[] | undefined
): Record<string, FileTypeSettingsData[]> {
  const enabledTypes = (fileTypes ?? []).filter((ft) => ft.enabled);
  const grouped = enabledTypes.reduce(
    (acc, fileType) => {
      const category = fileType.category ?? "Other";
      if (!acc[category]) acc[category] = [];
      acc[category].push(fileType);
      return acc;
    },
    {} as Record<string, FileTypeSettingsData[]>
  );
  Object.keys(grouped).forEach((category) => {
    grouped[category].sort(
      (a, b) => (a.priority ?? 0) - (b.priority ?? 0)
    );
  });
  return grouped;
}

export function getExtensionsByCategory(
  fileTypes: FileTypeSettingsData[] | undefined,
  category: string
): string[] {
  const grouped = getEnabledFileTypesByCategory(fileTypes);
  return (grouped[category] ?? []).map(
    (ft) => ft.fileExtension ?? ""
  ).filter(Boolean);
}

export function isFileTypeAllowed(
  fileTypes: FileTypeSettingsData[] | undefined,
  file: File,
  siteMaxFileSizeBytes: number = DEFAULT_SITE_MAX_FILE_SIZE_BYTES
): boolean {
  const ext = file.name
    .toLowerCase()
    .substring(file.name.lastIndexOf("."));
  const ft = (fileTypes ?? []).find(
    (f) =>
      f.enabled &&
      (f.fileExtension ?? "").toLowerCase() === ext
  );
  if (!ft) return false;
  const perTypeMax = ft.maxFileSizeBytes ?? siteMaxFileSizeBytes;
  const maxBytes = Math.min(siteMaxFileSizeBytes, perTypeMax);
  return file.size <= maxBytes;
}
