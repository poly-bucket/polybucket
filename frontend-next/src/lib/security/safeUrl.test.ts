import { describe, it, expect } from "vitest";
import {
  safeExternalHref,
  safeInternalPath,
  safeNotificationHref,
} from "./safeUrl";

describe("safeExternalHref", () => {
  it("allows https URLs", () => {
    expect(safeExternalHref("https://example.com/path")).toBe(
      "https://example.com/path"
    );
  });

  it("rejects javascript URLs", () => {
    expect(safeExternalHref("javascript:alert(1)")).toBeNull();
  });

  it("rejects data URLs", () => {
    expect(safeExternalHref("data:text/html,hi")).toBeNull();
  });
});

describe("safeInternalPath", () => {
  it("allows root-relative paths", () => {
    expect(safeInternalPath("/models/1")).toBe("/models/1");
  });

  it("rejects protocol-relative paths", () => {
    expect(safeInternalPath("//evil.example/phish")).toBeNull();
  });

  it("rejects backslash paths", () => {
    expect(safeInternalPath("/path\\evil")).toBeNull();
  });
});

describe("safeNotificationHref", () => {
  it("allows internal notification paths", () => {
    expect(safeNotificationHref("/models/abc")).toBe("/models/abc");
  });

  it("rejects external URLs", () => {
    expect(safeNotificationHref("https://evil.example/")).toBeNull();
  });
});
