import { test, expect } from "@playwright/test";
import {
  applyAuthenticatedProfileSession,
  E2E_PROFILE_USER,
  stubProfileSettingsApi,
} from "./helpers/authenticatedProfile";

test.describe("Settings profile avatar", () => {
  test.beforeEach(async ({ context }) => {
    await applyAuthenticatedProfileSession(context);
  });

  test("saving a generated pattern posts minidenticon svg with salt", async ({
    page,
  }) => {
    // Arrange
    let regenerateBody: { salt?: string; avatar?: string } | null = null;

    await stubProfileSettingsApi(page);

    await page.route("**/api/polybucket/api/users/avatar/regenerate", async (route) => {
      regenerateBody = route.request().postDataJSON() as {
        salt?: string;
        avatar?: string;
      };
      await route.fulfill({
        status: 200,
        contentType: "application/json",
        body: JSON.stringify({
          avatar: regenerateBody.avatar,
          userId: E2E_PROFILE_USER.id,
          salt: regenerateBody.salt,
        }),
      });
    });

    await page.goto("/settings/profile");
    await expect(page.getByText("Final avatar preview")).toBeVisible();

    const seed = "E2ESeed77";
    await page.getByLabel(/tweak the pattern/i).fill(seed);

    // Act
    await page.getByRole("button", { name: /save avatar/i }).click();

    // Assert
    await expect(
      page.getByText(/avatar updated/i)
    ).toBeVisible({ timeout: 15_000 });

    expect(regenerateBody).not.toBeNull();
    expect(regenerateBody!.salt).toBe(seed);
    expect(regenerateBody!.avatar).toBeTruthy();
    expect(regenerateBody!.avatar!.trimStart().toLowerCase().startsWith("<svg")).toBe(
      true
    );
    expect(regenerateBody!.avatar).toContain('viewBox="-1.5 -1.5 8 8"');
    expect(regenerateBody!.avatar).not.toContain('viewBox="0 0 5 5"');
  });
});