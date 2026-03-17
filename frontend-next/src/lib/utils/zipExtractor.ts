import JSZip from "jszip";

export interface ExtractedFile {
  name: string;
  content: Blob;
  size: number;
  type: string;
}

export interface ZipExtractionResult {
  success: boolean;
  files: ExtractedFile[];
  error?: string;
}

const MAX_TOTAL_SIZE = 100 * 1024 * 1024;
const MAX_FILE_COUNT = 100;
const MAX_INDIVIDUAL_FILE_SIZE = 50 * 1024 * 1024;
const MAX_COMPRESSION_RATIO = 100;

const MIME_TYPE_MAP: Record<string, string> = {
  ".stl": "application/octet-stream",
  ".obj": "text/plain",
  ".fbx": "application/octet-stream",
  ".gltf": "model/gltf+json",
  ".glb": "model/gltf-binary",
  ".3mf": "model/3mf",
  ".step": "application/octet-stream",
  ".stp": "application/octet-stream",
  ".jpg": "image/jpeg",
  ".jpeg": "image/jpeg",
  ".png": "image/png",
  ".gif": "image/gif",
  ".webp": "image/webp",
  ".bmp": "image/bmp",
  ".md": "text/markdown",
  ".markdown": "text/markdown",
  ".txt": "text/plain",
  ".pdf": "application/pdf",
};

export async function extractZipFile(
  zipFile: File
): Promise<ZipExtractionResult> {
  try {
    const zip = new JSZip();
    const zipContent = await zip.loadAsync(zipFile);

    const fileEntries = Object.values(zipContent.files).filter(
      (entry) => !entry.dir
    );
    if (fileEntries.length > MAX_FILE_COUNT) {
      return {
        success: false,
        files: [],
        error: `Too many files in zip archive. Maximum allowed: ${MAX_FILE_COUNT}, found: ${fileEntries.length}`,
      };
    }

    const totalCompressedSize = zipFile.size;
    let totalUncompressedSize = 0;
    for (const entry of fileEntries) {
      totalUncompressedSize += (entry as { _data?: { uncompressedSize?: number } })._data?.uncompressedSize ?? 0;
    }

    if (totalUncompressedSize > 0 && totalCompressedSize > 0) {
      const compressionRatio = totalUncompressedSize / totalCompressedSize;
      if (compressionRatio > MAX_COMPRESSION_RATIO) {
        return {
          success: false,
          files: [],
          error:
            "Suspicious compression ratio detected. This might be a zip bomb attack.",
        };
      }
    }

    const extractedFiles: ExtractedFile[] = [];
    let currentTotalSize = 0;

    for (const [fileName, zipEntry] of Object.entries(zipContent.files)) {
      if (zipEntry.dir) continue;
      if (
        fileName.includes("..") ||
        fileName.startsWith("/") ||
        fileName.includes("\\")
      )
        continue;

      const uncompressedSize =
        (zipEntry as { _data?: { uncompressedSize?: number } })._data
          ?.uncompressedSize ?? 0;
      if (uncompressedSize > MAX_INDIVIDUAL_FILE_SIZE) continue;

      try {
        const fileContent = await zipEntry.async("blob");
        if (currentTotalSize + fileContent.size > MAX_TOTAL_SIZE) break;

        const fileExtension = fileName
          .toLowerCase()
          .substring(fileName.lastIndexOf("."));
        const mimeType =
          MIME_TYPE_MAP[fileExtension] ?? "application/octet-stream";

        extractedFiles.push({
          name: fileName,
          content: fileContent,
          size: fileContent.size,
          type: mimeType,
        });
        currentTotalSize += fileContent.size;
      } catch {
        // skip failed file
      }
    }

    if (extractedFiles.length === 0) {
      return {
        success: false,
        files: [],
        error: "No valid files found in zip archive",
      };
    }

    return { success: true, files: extractedFiles };
  } catch (error) {
    return {
      success: false,
      files: [],
      error:
        error instanceof Error ? error.message : "Unknown error during zip extraction",
    };
  }
}

export async function isValidZipFile(file: File): Promise<boolean> {
  try {
    if (file.size < 22) return false;
    if (file.size > MAX_TOTAL_SIZE) return false;

    const arrayBuffer = await file.slice(0, 4).arrayBuffer();
    const uint8Array = new Uint8Array(arrayBuffer);
    return (
      uint8Array[0] === 0x50 &&
      uint8Array[1] === 0x4b &&
      uint8Array[2] === 0x03 &&
      uint8Array[3] === 0x04
    );
  } catch {
    return false;
  }
}

export function convertToFiles(extractedFiles: ExtractedFile[]): File[] {
  return extractedFiles.map(
    (ef) => new File([ef.content], ef.name, { type: ef.type })
  );
}
