export const E2E_SITE_ACCESS_OVERRIDE_ENV = "E2E_SITE_ACCESS_OVERRIDE";
export const E2E_ALLOW_PUBLIC_BROWSING_COOKIE = "e2e-allow-public-browsing";

export function readE2EPublicBrowsingOverride(
  envFlag: string | undefined,
  cookieValue: string | undefined
): boolean | null {
  if (envFlag !== "1") {
    return null;
  }
  if (cookieValue === "1") {
    return true;
  }
  if (cookieValue === "0") {
    return false;
  }
  return null;
}
