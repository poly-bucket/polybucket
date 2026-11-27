import { describe, expect, it } from "vitest";
import { isPngBlobHeader, validateGeneratedThumbnailBlob } from "./generated-thumbnail";

describe("generated-thumbnail", () => {
  it("isPngBlobHeader accepts a PNG signature", () => {
    const header = new Uint8Array([0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a]);

    expect(isPngBlobHeader(header)).toBe(true);
  });

  it("isPngBlobHeader rejects non-PNG bytes", () => {
    expect(isPngBlobHeader(new Uint8Array([0x00, 0x00]))).toBe(false);
    expect(isPngBlobHeader(new Uint8Array(4))).toBe(false);
  });

  it("validateGeneratedThumbnailBlob rejects empty blobs", async () => {
    expect(await validateGeneratedThumbnailBlob(new Blob([]))).toBe("empty");
  });

  it("validateGeneratedThumbnailBlob rejects invalid PNG content", async () => {
    expect(await validateGeneratedThumbnailBlob(new Blob(["not-a-png"]))).toBe("invalid-png");
  });

  it("validateGeneratedThumbnailBlob accepts a minimal PNG payload", async () => {
    const header = new Uint8Array([0x89, 0x50, 0x4e, 0x47, 0, 0, 0, 0]);
    const blob = new Blob([header]);

    expect(await validateGeneratedThumbnailBlob(blob)).toBeNull();
  });
});