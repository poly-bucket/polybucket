import type { SiteAccessPolicy } from "@/lib/bff/fetchSiteAccessPolicy";

type DownloadPolicy = Pick<
  SiteAccessPolicy,
  "allowAnonymousModelDownload" | "modelDownloadsEnabled" | "allowPublicBrowsing"
>;

export function areModelDownloadsEnabled(
  policy: Pick<SiteAccessPolicy, "modelDownloadsEnabled">
): boolean {
  return policy.modelDownloadsEnabled !== false;
}

function isAnonDownloadablePrivacy(privacy?: string | null): boolean {
  const p = String(privacy ?? "Public").toLowerCase();
  return p === "public" || p === "unlisted";
}

export function canDownloadWithoutSignIn(
  policy: Pick<SiteAccessPolicy, "allowAnonymousModelDownload" | "modelDownloadsEnabled">,
  privacy?: string | null
): boolean {
  if (!areModelDownloadsEnabled(policy)) {
    return false;
  }

  if (!policy.allowAnonymousModelDownload) {
    return false;
  }

  return isAnonDownloadablePrivacy(privacy);
}

export function mayDownloadModel(
  isAuthenticated: boolean,
  policy: DownloadPolicy,
  privacy?: string | null
): boolean {
  if (!areModelDownloadsEnabled(policy)) {
    return false;
  }

  if (isAuthenticated) {
    return true;
  }

  if (!policy.allowPublicBrowsing) {
    return false;
  }

  return canDownloadWithoutSignIn(policy, privacy);
}