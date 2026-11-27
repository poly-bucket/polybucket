import { describe, expect, it } from "vitest";
import { buildContentSecurityPolicy, securityResponseHeaders } from "./securityHeaders";

describe("buildContentSecurityPolicy", () => {
  it("allows blob URLs for default-src and image previews", () => {
    const csp = buildContentSecurityPolicy();

    expect(csp).toContain("default-src 'self' blob:");
    expect(csp).toMatch(/img-src[^;]*\bblob:/);
    expect(csp).toMatch(/img-src[^;]*\bdata:/);
  });

  it("keeps API origin in connect-src", () => {
    const csp = buildContentSecurityPolicy();
    expect(csp).toMatch(/connect-src 'self'/);
  });
});

describe("securityResponseHeaders", () => {
  it("includes CSP that supports upload thumbnail previews", () => {
    const headers = securityResponseHeaders();
    const csp = headers.find((header) => header.key === "Content-Security-Policy");

    expect(csp?.value).toContain("blob:");
    expect(csp?.value).toContain("data:");
  });
});