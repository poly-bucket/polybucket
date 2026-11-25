import { test, expect } from "@playwright/test";
import { setPublicBrowsingOverride } from "./publicBrowsingCookie";

test.describe("Private site browsing", () => {
  test("redirects anonymous /dashboard to login when public browsing is disabled", async ({
    page,
  }) => {
    // Arrange
    await setPublicBrowsingOverride(page, false);

    // Act
    await page.goto("/dashboard");

    // Assert
    await expect(page).toHaveURL(/\/login\?redirect=%2Fdashboard/);
  });

  test("allows anonymous /dashboard when public browsing is enabled", async ({ page }) => {
    // Arrange
    await setPublicBrowsingOverride(page, true);

    // Act
    await page.goto("/dashboard");

    // Assert
    await expect(page).toHaveURL(/\/dashboard/);
    await expect(page.getByRole("heading", { name: "Dashboard" })).toBeVisible();
  });
});
