import {
  defaultModelMetadataRules,
  parseModelMetadataRules,
  type ModelMetadataRules,
} from "@/lib/models/model-metadata";
import { getPolybucketApiUrl } from "./env";

export type SiteAccessPolicy = {
  allowPublicBrowsing: boolean;
  requireLoginForUpload: boolean;
  allowAnonymousModelUpload: boolean;
  defaultModelPrivacy: string;
  modelMetadataRules: ModelMetadataRules;
};

async function readPolicyFromUrl(url: string): Promise<SiteAccessPolicy | null> {
  try {
    const response = await fetch(url, { cache: "no-store" });
    if (!response.ok) {
      return null;
    }
    const data = (await response.json()) as {
      allowPublicBrowsing?: boolean;
      requireLoginForUpload?: boolean;
      allowAnonymousModelUpload?: boolean;
      defaultModelPrivacy?: string;
      modelMetadataRules?: Partial<ModelMetadataRules>;
    };
    return {
      allowPublicBrowsing: data.allowPublicBrowsing === true,
      requireLoginForUpload: data.requireLoginForUpload !== false,
      allowAnonymousModelUpload: data.allowAnonymousModelUpload === true,
      defaultModelPrivacy: data.defaultModelPrivacy ?? "Public",
      modelMetadataRules: parseModelMetadataRules(data.modelMetadataRules),
    };
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

  return {
    allowPublicBrowsing: false,
    requireLoginForUpload: true,
    allowAnonymousModelUpload: false,
    defaultModelPrivacy: "Public",
    modelMetadataRules: defaultModelMetadataRules,
  };
}