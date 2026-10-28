import { describe, it, expect } from "vitest";
import { hasAnyRole, normalizeRole } from "./roles";

describe("normalizeRole", () => {
  it("lowercases and trims", () => {
    expect(normalizeRole(" Admin ")).toBe("admin");
  });
});

describe("hasAnyRole", () => {
  it("matches roles case-insensitively", () => {
    expect(hasAnyRole(["admin"], ["Admin"])).toBe(true);
  });

  it("returns true when no roles required", () => {
    expect(hasAnyRole([], undefined)).toBe(true);
  });

  it("returns false when user lacks role", () => {
    expect(hasAnyRole(["user"], ["Admin"])).toBe(false);
  });
});
