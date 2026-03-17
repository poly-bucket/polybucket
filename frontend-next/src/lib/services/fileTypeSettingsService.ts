import { ApiClientFactory } from "@/lib/api/clientFactory";
import type {
  FileTypeSettingsData,
  GetFileSettingsResponse,
} from "@/lib/api/client";

export type { FileTypeSettingsData };

export interface FileTypeSettingsServiceResult {
  fileTypes: FileTypeSettingsData[];
  getExtensionsByCategory: (category: string) => string[];
  isFileAllowed: (file: File) => boolean;
}

const client = () => ApiClientFactory.getApiClient();

export async function getFileSettings(): Promise<GetFileSettingsResponse> {
  return client().getFileSettings_GetFileSettings();
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
  file: File
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
  const maxBytes = ft.maxFileSizeBytes ?? 100 * 1024 * 1024;
  return file.size <= maxBytes;
}
