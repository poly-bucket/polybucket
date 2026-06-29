import { describe, it, expect } from "vitest";
import { isImageUrl } from "./modelUtils";

describe("isImageUrl", () => {
  it("returns true for image extensions", () => {
    expect(isImageUrl("https://example.com/preview.png")).toBe(true);
    expect(isImageUrl("https://example.com/photo.JPG")).toBe(true);
    expect(isImageUrl("https://example.com/image.webp")).toBe(true);
  });

  it("returns true for image URLs with presigned query strings", () => {
    expect(
      isImageUrl(
        "http://localhost:9000/bucket/models/x/preview.png?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Signature=abc"
      )
    ).toBe(true);
  });

  it("returns false for 3D mesh files", () => {
    expect(isImageUrl("http://localhost:9000/bucket/models/x/part.stl")).toBe(
      false
    );
    expect(
      isImageUrl(
        "http://localhost:9000/bucket/models/x/part.stl?X-Amz-Signature=abc"
      )
    ).toBe(false);
  });

  it("returns false for empty or nullish values", () => {
    expect(isImageUrl(undefined)).toBe(false);
    expect(isImageUrl(null)).toBe(false);
    expect(isImageUrl("")).toBe(false);
  });
});
