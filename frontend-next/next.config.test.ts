import { describe, it, expect } from "vitest";
import { securityResponseHeaders } from "@/lib/security/securityHeaders";

describe("securityResponseHeaders", () => {
  it("includes referrer policy and CSP", () => {
    const headers = securityResponseHeaders();
    const keys = headers.map((h) => h.key);
    expect(keys).toContain("Referrer-Policy");
    expect(keys).toContain("Content-Security-Policy");
    const csp = headers.find((h) => h.key === "Content-Security-Policy");
    expect(csp?.value).toContain("default-src 'self'");
  });
});
