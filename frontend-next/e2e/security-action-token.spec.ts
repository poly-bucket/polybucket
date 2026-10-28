import { test, expect } from "@playwright/test";

test.describe("Action token URL", () => {
  test("removes token from verify-email URL after redirect", async ({ page }) => {
    await page.goto("/verify-email?token=not-a-real-token");
    await expect(page).toHaveURL(/\/verify-email(\?|$)/);
    expect(page.url()).not.toContain("token=");
  });
});
