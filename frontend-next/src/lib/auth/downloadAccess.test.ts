import { describe, it, expect } from "vitest";
import {
  mayDownloadModel,
  canDownloadWithoutSignIn,
  areModelDownloadsEnabled,
} from "./downloadAccess";
import type { SiteAccessPolicy } from "@/lib/bff/fetchSiteAccessPolicy";
import { defaultModelMetadataRules } from "@/lib/models/model-metadata";

function basePolicy(
  overrides: Partial<SiteAccessPolicy> = {}
): SiteAccessPolicy {
  return {
    allowPublicBrowsing: true,
    requireLoginForUpload: true,
    allowAnonymousModelUpload: false,
    allowAnonymousModelDownload: false,
    modelDownloadsEnabled: true,
    modelCommentsAccess: "Full",
    modelCollectionsAccess: "Full",
    defaultModelPrivacy: "Public",
    modelMetadataRules: defaultModelMetadataRules,
    ...overrides,
  };
}

describe("downloadAccess", () => {
  it("allows authenticated users when downloads are enabled", () => {
    expect(mayDownloadModel(true, basePolicy(), "Public")).toBe(true);
  });

  it("blocks everyone when downloads are disabled", () => {
    const policy = basePolicy({ modelDownloadsEnabled: false });
    expect(mayDownloadModel(true, policy, "Public")).toBe(false);
    expect(mayDownloadModel(false, policy, "Public")).toBe(false);
  });

  it("allows anonymous public download when policy permits", () => {
    const policy = basePolicy({ allowAnonymousModelDownload: true });
    expect(canDownloadWithoutSignIn(policy, "Public")).toBe(true);
    expect(mayDownloadModel(false, policy, "Unlisted")).toBe(true);
  });

  it("blocks anonymous download on private models", () => {
    const policy = basePolicy({ allowAnonymousModelDownload: true });
    expect(mayDownloadModel(false, policy, "Private")).toBe(false);
  });

  it("areModelDownloadsEnabled treats undefined as enabled", () => {
    expect(areModelDownloadsEnabled({ modelDownloadsEnabled: true })).toBe(true);
  });
});