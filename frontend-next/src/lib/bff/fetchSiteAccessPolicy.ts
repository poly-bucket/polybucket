import { getPolybucketApiUrl } from "./env";

export type SiteAccessPolicy = {
  allowPublicBrowsing: boolean;
};

async function readPolicyFromUrl(url: string): Promise<SiteAccessPolicy | null> {
  try {
    const response = await fetch(url, { cache: "no-store" });
    if (!response.ok) {
      return null;
    }
    const data = (await response.json()) as { allowPublicBrowsing?: boolean };
    return { allowPublicBrowsing: data.allowPublicBrowsing === true };
  } catch {
    return null;
  }
}

export async function fetchSiteAccessPolicy(origin: string): Promise<SiteAccessPolicy> {
  const bffUrl = new URL("/api/polybucket/auth/site-access", origin).toString();
  const fromBff = await readPolicyFromUrl(bffUrl);
  if (fromBff) {
    return fromBff;
  }

  const apiBase = getPolybucketApiUrl();
  const fromApi = await readPolicyFromUrl(`${apiBase}/api/auth/site-access`);
  if (fromApi) {
    return fromApi;
  }

  return { allowPublicBrowsing: false };
}
