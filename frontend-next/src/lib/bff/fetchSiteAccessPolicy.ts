import {
  defaultModelMetadataRules,
  parseModelMetadataRules,
  type ModelMetadataRules,
} from "@/lib/models/model-metadata";
import { getPolybucketApiUrl } from "./env";
import type { FeatureAccessLevel } from "@/lib/auth/featureAccess";
import { DEFAULT_SITE_NAME } from "@/lib/site/defaultSiteName";

function parseAccessLevel(value?: string): FeatureAccessLevel {
  if (value === "Hidden" || value === "ReadOnly" || value === "Full") {
    return value;
  }
  return "Full";
}

export type SiteAccessPolicy = {
  siteName: string;
  allowPublicBrowsing: boolean;
  requireLoginForUpload: boolean;
  allowAnonymousModelUpload: boolean;
  allowAnonymousModelDownload: boolean;
  modelDownloadsEnabled: boolean;
  modelVersioningEnabled: boolean;
  modelSharingEnabled: boolean;
  modelReportingEnabled: boolean;
  modelCommentsAccess: FeatureAccessLevel;
  modelCollectionsAccess: FeatureAccessLevel;
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
      siteName?: string;
      allowPublicBrowsing?: boolean;
      requireLoginForUpload?: boolean;
      allowAnonymousModelUpload?: boolean;
      allowAnonymousModelDownload?: boolean;
      modelDownloadsEnabled?: boolean;
      modelVersioningEnabled?: boolean;
      modelSharingEnabled?: boolean;
      modelReportingEnabled?: boolean;
      modelCommentsAccess?: string;
      modelCollectionsAccess?: string;
      defaultModelPrivacy?: string;
      modelMetadataRules?: Partial<ModelMetadataRules>;
    };
    const siteName = data.siteName?.trim() || DEFAULT_SITE_NAME;
    return {
      siteName,
      allowPublicBrowsing: data.allowPublicBrowsing === true,
      requireLoginForUpload: data.requireLoginForUpload !== false,
      allowAnonymousModelUpload: data.allowAnonymousModelUpload === true,
      allowAnonymousModelDownload: data.allowAnonymousModelDownload === true,
      modelDownloadsEnabled: data.modelDownloadsEnabled !== false,
      modelVersioningEnabled: data.modelVersioningEnabled !== false,
      modelSharingEnabled: data.modelSharingEnabled !== false,
      modelReportingEnabled: data.modelReportingEnabled !== false,
      modelCommentsAccess: parseAccessLevel(data.modelCommentsAccess),
      modelCollectionsAccess: parseAccessLevel(data.modelCollectionsAccess),
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
    siteName: DEFAULT_SITE_NAME,
    allowPublicBrowsing: false,
    requireLoginForUpload: true,
    allowAnonymousModelUpload: false,
    allowAnonymousModelDownload: false,
    modelDownloadsEnabled: true,
    modelVersioningEnabled: true,
    modelSharingEnabled: true,
    modelReportingEnabled: true,
    modelCommentsAccess: "Full",
    modelCollectionsAccess: "Full",
    defaultModelPrivacy: "Public",
    modelMetadataRules: defaultModelMetadataRules,
  };
}