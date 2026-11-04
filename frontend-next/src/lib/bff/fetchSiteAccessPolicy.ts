export type SiteAccessPolicy = {
  allowPublicBrowsing: boolean;
};

export async function fetchSiteAccessPolicy(origin: string): Promise<SiteAccessPolicy> {
  try {
    const url = new URL("/api/polybucket/auth/site-access", origin);
    const response = await fetch(url, { cache: "no-store" });
    if (!response.ok) {
      return { allowPublicBrowsing: false };
    }
    const data = (await response.json()) as { allowPublicBrowsing?: boolean };
    return { allowPublicBrowsing: data.allowPublicBrowsing === true };
  } catch {
    return { allowPublicBrowsing: false };
  }
}
