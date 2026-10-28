import { test, expect } from "@playwright/test";

test.describe("Security headers", () => {
  test("sends referrer policy on home page", async ({ page }) => {
    const response = await page.goto("/");
    expect(response).not.toBeNull();
    const headers = response!.headers();
    expect(headers["referrer-policy"]).toBe("strict-origin-when-cross-origin");
    expect(headers["content-security-policy"]).toContain("default-src 'self'");
  });
});
