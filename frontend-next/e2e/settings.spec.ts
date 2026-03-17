import { test, expect } from "@playwright/test";

test.describe("Settings", () => {
  test("unauthenticated user cannot access settings", async ({ page }) => {
    await page.goto("/settings");

    await expect(page).toHaveURL(/\/(login|auth)/);
    expect(page.url()).toContain("redirect=");
  });

  test("unauthenticated user redirected from settings profile", async ({ page }) => {
    await page.goto("/settings/profile");

    await expect(page).toHaveURL(/\/(login|auth)/);
    expect(page.url()).toContain("redirect=");
  });
});
