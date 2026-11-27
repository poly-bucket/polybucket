import type { BrowserContext, Page } from "@playwright/test";
import { AUTH_PROFILE_STORAGE_KEY, AUTH_SESSION_COOKIE } from "../../src/lib/auth/authConstants";
import { sealSession } from "../../src/lib/bff/session";
import { clearEncryptionKeyCacheForTests } from "../../src/lib/bff/sessionKey";

const E2E_SESSION_SECRET = "development-session-secret-min-32-chars!!";

export const E2E_PROFILE_USER = {
  id: "11111111-1111-1111-1111-111111111111",
  email: "e2e-avatar@test.com",
  username: "e2eavatar",
  roles: ["User"],
} as const;

export async function applyAuthenticatedProfileSession(
  context: BrowserContext
): Promise<void> {
  process.env.SESSION_SECRET = E2E_SESSION_SECRET;
  clearEncryptionKeyCacheForTests();

  const sealed = await sealSession(
    {
      refreshToken: "e2e-refresh",
      accessToken: "e2e-access",
      accessExp: Date.now() + 60 * 60 * 1000,
      roles: ["User"],
    },
    3600
  );

  await context.addCookies([
    {
      name: AUTH_SESSION_COOKIE,
      value: sealed,
      domain: "localhost",
      path: "/",
    },
  ]);
}

export async function stubProfileSettingsApi(page: Page): Promise<void> {
  await page.addInitScript(
    ([key, user]) => {
      sessionStorage.setItem(key, JSON.stringify(user));
    },
    [AUTH_PROFILE_STORAGE_KEY, E2E_PROFILE_USER] as const
  );

  await page.route("**/api/auth/session", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({ user: E2E_PROFILE_USER }),
    });
  });

  await page.route("**/api/auth/refresh", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({ user: E2E_PROFILE_USER }),
    });
  });

  const profileBody = {
    id: E2E_PROFILE_USER.id,
    username: E2E_PROFILE_USER.username,
    avatar: "",
    bio: "",
    country: "",
    websiteUrl: "",
    twitterUrl: "",
    instagramUrl: "",
    youtubeUrl: "",
  };

  await page.route(
    `**/api/polybucket/api/users/profile/${E2E_PROFILE_USER.id}**`,
    async (route) => {
      await route.fulfill({
        status: 200,
        contentType: "application/json",
        body: JSON.stringify(profileBody),
      });
    }
  );

  await page.route("**/api/polybucket/api/auth/me**", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({
        ...E2E_PROFILE_USER,
        role: "User",
        avatar: "",
      }),
    });
  });

  await page.route("**/api/polybucket/api/users/settings**", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({}),
    });
  });

  await page.route("**/api/polybucket/api/notifications/**", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({ unreadCount: 0 }),
    });
  });

  await page.route("**/api/polybucket/api/plugins/**", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify([]),
    });
  });

  await page.route("**/api/polybucket/auth/site-access**", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({ allowPublicBrowsing: true }),
    });
  });
}