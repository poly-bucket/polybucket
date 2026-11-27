import { test, expect } from "@playwright/test";
import path from "node:path";
import { hasE2ECredentials, loginAsTestUser } from "./loginTestUser";

const FIXTURE_STL = path.join(__dirname, "fixtures", "cube.stl");

test.describe("Model upload thumbnail", () => {
  test("sends CSP that allows blob and data image previews", async ({ page }) => {
    const response = await page.goto("/models/upload");
    expect(response).not.toBeNull();
    const csp = response!.headers()["content-security-policy"] ?? "";

    expect(csp).toContain("blob:");
    expect(csp).toMatch(/img-src[^;]*\bdata:/);
    expect(csp).toMatch(/img-src[^;]*\bblob:/);
  });

  test("allows data and blob URLs in img under deployed CSP", async ({ page }) => {
    await page.goto("/");
    const results = await page.evaluate(async () => {
      const pngBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/88BAwAI/Al+S0Y9YQAAAABJRU5ErkJggg==";
      const bytes = Uint8Array.from(atob(pngBase64), (character) => character.charCodeAt(0));
      const blob = new Blob([bytes], { type: "image/png" });
      const blobUrl = URL.createObjectURL(blob);

      const loadImage = (src: string) =>
        new Promise<"load" | "error">((resolve) => {
          const image = new Image();
          image.onload = () => resolve("load");
          image.onerror = () => resolve("error");
          image.src = src;
        });

      const dataResult = await loadImage(`data:image/png;base64,${pngBase64}`);
      const blobResult = await loadImage(blobUrl);
      URL.revokeObjectURL(blobUrl);
      return { dataResult, blobResult };
    });

    expect(results.dataResult).toBe("load");
    expect(results.blobResult).toBe("load");
  });

  test("opens thumbnail generator and keeps upload route after STL upload", async ({
    page,
  }) => {
    test.skip(!hasE2ECredentials(), "Set E2E_LOGIN_EMAIL and E2E_LOGIN_PASSWORD");

    const loggedIn = await loginAsTestUser(page);
    expect(loggedIn).toBe(true);

    await page.goto("/models/upload");
    await expect(page).toHaveURL(/\/models\/upload/);

    const fileInput = page.locator('input[type="file"]').first();
    await fileInput.setInputFiles(FIXTURE_STL);

    await expect(page.getByText(/cube\.stl/i)).toBeVisible({ timeout: 30_000 });

    await expect(
      page.getByRole("button", { name: /generate thumbnail from 3d preview/i })
    ).toBeVisible({ timeout: 30_000 });

    await page.getByRole("button", { name: /generate thumbnail from 3d preview/i }).click();

    await expect(
      page.getByRole("heading", { name: /generate custom thumbnail/i })
    ).toBeVisible();

    await expect(page).toHaveURL(/\/models\/upload/);
    await expect(page.getByRole("dialog")).toBeVisible();
  });
});