import type { Page } from "@playwright/test";
import { E2E_ALLOW_PUBLIC_BROWSING_COOKIE } from "../src/lib/auth/e2eSiteAccessOverride";

export async function setPublicBrowsingOverride(
  page: Page,
  allowPublicBrowsing: boolean
) {
  await page.context().addCookies([
    {
      name: E2E_ALLOW_PUBLIC_BROWSING_COOKIE,
      value: allowPublicBrowsing ? "1" : "0",
      domain: "localhost",
      path: "/",
    },
  ]);
}
