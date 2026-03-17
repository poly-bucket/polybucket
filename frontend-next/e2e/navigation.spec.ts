import { test, expect } from "@playwright/test";

const LOGIN_PATH = "/login";

test.describe("Navigation", () => {
  test("home redirects to dashboard or auth", async ({ page }) => {
    await page.goto("/");

    await expect(page).toHaveURL(/\/(dashboard|login|auth)/);
  });

  test("dashboard page loads", async ({ page }) => {
    await page.goto("/dashboard");

    await expect(page.locator("main, [role='main'], body")).toBeVisible();
  });

  test("login page is accessible", async ({ page }) => {
    await page.goto(LOGIN_PATH);

    await expect(page.getByRole("heading", { name: /welcome back/i })).toBeVisible();
  });

  test("unauthenticated access to my-models redirects to login", async ({ page }) => {
    await page.goto("/my-models");

    await expect(page).toHaveURL(/\/(login|auth)/);
    expect(page.url()).toContain("redirect=");
  });

  test("unauthenticated access to settings redirects to login", async ({ page }) => {
    await page.goto("/settings");

    await expect(page).toHaveURL(/\/(login|auth)/);
    expect(page.url()).toContain("redirect=");
  });

  test("unauthenticated access to models upload redirects to login", async ({
    page,
  }) => {
    await page.goto("/models/upload");

    await expect(page).toHaveURL(/\/(login|auth)/);
    expect(page.url()).toContain("redirect=");
  });
});
