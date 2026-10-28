import { test, expect } from "@playwright/test";

test.describe("Security RBAC", () => {
  test("redirects unauthenticated users from admin to login", async ({ page }) => {
    await page.goto("/admin");
    await expect(page).toHaveURL(/\/login/);
  });

  test("does not allow forgeable session marker for admin", async ({ page, context }) => {
    await context.addCookies([
      {
        name: "polybucket-session",
        value: "1",
        domain: "localhost",
        path: "/",
      },
    ]);
    await page.goto("/admin");
    await expect(page).toHaveURL(/\/login/);
  });
});
