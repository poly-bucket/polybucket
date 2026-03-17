import { test, expect } from "@playwright/test";

const LOGIN_PATH = "/login";

test.describe("Login", () => {
  test("renders login form", async ({ page }) => {
    await page.goto(LOGIN_PATH);

    await expect(page.getByRole("heading", { name: /welcome back/i })).toBeVisible();
    await expect(page.getByPlaceholder(/email or username/i)).toBeVisible();
    await expect(page.getByPlaceholder(/password/i)).toBeVisible();
    await expect(page.getByRole("button", { name: /sign in/i })).toBeVisible();
  });

  test("shows error on invalid login", async ({ page }) => {
    await page.goto(LOGIN_PATH);

    await page.getByPlaceholder(/email or username/i).fill("invalid@test.com");
    await page.getByPlaceholder(/password/i).fill("wrongpassword");
    await page.getByRole("button", { name: /sign in/i }).click();

    await expect(
      page.getByText(/login failed|invalid|error|unauthorized|network/i)
    ).toBeVisible({ timeout: 15000 });
  });

  test("redirects to login when accessing protected route unauthenticated", async ({
    page,
  }) => {
    await page.goto("/my-models");

    await expect(page).toHaveURL(/\/(login|auth)(\?|$)/);
    expect(page.url()).toContain("redirect=");
  });
});
