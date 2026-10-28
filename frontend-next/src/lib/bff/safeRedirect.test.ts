import { describe, it, expect } from "vitest";
import { isSafeInternalRedirect } from "./safeRedirect";

describe("isSafeInternalRedirect", () => {
  const origin = "https://app.example.com";

  it("allows a normal relative path", () => {
    expect(isSafeInternalRedirect("/models/1", origin)).toBe(true);
  });

  it("rejects protocol-relative paths", () => {
    expect(isSafeInternalRedirect("//evil.example/phish", origin)).toBe(false);
  });

  it("rejects absolute external URLs", () => {
    expect(isSafeInternalRedirect("https://evil.example/", origin)).toBe(false);
  });
});
