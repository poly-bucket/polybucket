import type { SiteAccessPolicy } from "@/lib/bff/fetchSiteAccessPolicy";

export function mayAccessModelUpload(
  isAuthenticated: boolean,
  policy: Pick<SiteAccessPolicy, "allowAnonymousModelUpload" | "requireLoginForUpload">
): boolean {
  if (isAuthenticated) {
    return true;
  }

  return policy.allowAnonymousModelUpload && !policy.requireLoginForUpload;
}