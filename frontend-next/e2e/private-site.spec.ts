import { test, expect } from "@playwright/test";

async function mockSiteAccess(
  page: import("@playwright/test").Page,
  allowPublicBrowsing: boolean
) {
  await page.route("**/api/polybucket/auth/site-access", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({ allowPublicBrowsing }),
    });
  });
}

test.describe("Private site browsing", () => {
  test("redirects anonymous /dashboard to login when public browsing is disabled", async ({
    page,
  }) => {
    // Arrange
    await mockSiteAccess(page, false);

    // Act
    await page.goto("/dashboard");

    // Assert
    await expect(page).toHaveURL(/\/login\?redirect=%2Fdashboard/);
  });

  test("allows anonymous /dashboard when public browsing is enabled", async ({ page }) => {
    // Arrange
    await mockSiteAccess(page, true);

    // Act
    await page.goto("/dashboard");

    // Assert
    await expect(page).toHaveURL(/\/dashboard/);
    await expect(page.getByRole("heading", { name: "Dashboard" })).toBeVisible();
  });
});
