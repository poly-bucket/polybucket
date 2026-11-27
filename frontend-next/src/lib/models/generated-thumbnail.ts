const PNG_SIGNATURE = [0x89, 0x50, 0x4e, 0x47] as const;

export function isPngBlobHeader(header: Uint8Array): boolean {
  if (header.length < PNG_SIGNATURE.length) {
    return false;
  }
  return PNG_SIGNATURE.every((byte, index) => header[index] === byte);
}

export async function validateGeneratedThumbnailBlob(blob: Blob): Promise<string | null> {
  if (blob.size === 0) {
    return "empty";
  }
  const header = new Uint8Array(await blob.slice(0, 8).arrayBuffer());
  if (!isPngBlobHeader(header)) {
    return "invalid-png";
  }
  return null;
}