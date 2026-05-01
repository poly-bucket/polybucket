"use client";

import type { UploadedFile } from "./file-queue";

export const MAX_FILES_PER_UPLOAD = 20;
export const SUPPORTED_3D_FORMATS = [
  ".stl",
  ".obj",
  ".fbx",
  ".gltf",
  ".glb",
  ".3mf",
  ".step",
  ".stp",
];
export const SUPPORTED_IMAGE_FORMATS = [
  ".jpg",
  ".jpeg",
  ".png",
  ".gif",
  ".webp",
  ".bmp",
];
export const SUPPORTED_DOCUMENT_FORMATS = [".pdf", ".md", ".markdown", ".txt"];

export type UploadFileType = "3d" | "image" | "pdf" | "markdown" | "unknown";

export function getFileExtension(fileName: string): string {
  const lastDot = fileName.lastIndexOf(".");
  return lastDot >= 0 ? fileName.substring(lastDot).toLowerCase() : "";
}

export function getUploadFileType(
  fileName: string,
  supported3d: string[],
  supportedImages: string[]
): UploadFileType {
  const ext = getFileExtension(fileName);
  if (supported3d.includes(ext)) return "3d";
  if (supportedImages.includes(ext)) return "image";
  if (ext === ".pdf") return "pdf";
  if (ext === ".md" || ext === ".markdown") return "markdown";
  return "unknown";
}

export function ensureUniqueFileName(fileName: string, takenNames: Set<string>): string {
  if (!takenNames.has(fileName)) return fileName;

  const dotIndex = fileName.lastIndexOf(".");
  const baseName = dotIndex >= 0 ? fileName.slice(0, dotIndex) : fileName;
  const extension = dotIndex >= 0 ? fileName.slice(dotIndex) : "";
  let counter = 1;
  let candidate = `${baseName} (${counter})${extension}`;
  while (takenNames.has(candidate)) {
    counter += 1;
    candidate = `${baseName} (${counter})${extension}`;
  }
  return candidate;
}

export function createUploadedFile(file: File, takenNames?: Set<string>): UploadedFile {
  const fileName = takenNames ? ensureUniqueFileName(file.name, takenNames) : file.name;
  const normalizedFile = fileName === file.name ? file : new File([file], fileName, { type: file.type });

  return {
    id: Math.random().toString(36).slice(2, 11),
    name: normalizedFile.name,
    size: normalizedFile.size,
    type: normalizedFile.type,
    file: normalizedFile,
    progress: 0,
    isThumbnail: false,
  };
}

export function setThumbnailSelection(
  files: UploadedFile[],
  selectedId: string | null
): UploadedFile[] {
  return files.map((file) => ({ ...file, isThumbnail: selectedId !== null && file.id === selectedId }));
}
