import type { SiteAccessPolicy } from "@/lib/bff/fetchSiteAccessPolicy";

export type FeatureAccessLevel = "Full" | "ReadOnly" | "Hidden";

function normalizeAccess(value?: string | null): FeatureAccessLevel {
  if (value === "Hidden") {
    return "Hidden";
  }
  if (value === "ReadOnly") {
    return "ReadOnly";
  }
  return "Full";
}

export function getCommentsAccess(
  policy: Pick<SiteAccessPolicy, "modelCommentsAccess">
): FeatureAccessLevel {
  return normalizeAccess(policy.modelCommentsAccess);
}

export function getCollectionsAccess(
  policy: Pick<SiteAccessPolicy, "modelCollectionsAccess">
): FeatureAccessLevel {
  return normalizeAccess(policy.modelCollectionsAccess);
}

export function canShowCommentsSection(
  policy: Pick<SiteAccessPolicy, "modelCommentsAccess">
): boolean {
  return getCommentsAccess(policy) !== "Hidden";
}

export function canWriteComments(
  policy: Pick<SiteAccessPolicy, "modelCommentsAccess">,
  isAdminOrModerator: boolean
): boolean {
  const access = getCommentsAccess(policy);
  if (access === "Full") {
    return true;
  }
  if (access === "ReadOnly") {
    return isAdminOrModerator;
  }
  return false;
}

export function canShowCollectionsNav(
  policy: Pick<SiteAccessPolicy, "modelCollectionsAccess">
): boolean {
  return getCollectionsAccess(policy) !== "Hidden";
}

export function canMutateCollections(
  policy: Pick<SiteAccessPolicy, "modelCollectionsAccess">,
  isAdminOrModerator: boolean
): boolean {
  const access = getCollectionsAccess(policy);
  if (access === "Full") {
    return true;
  }
  if (access === "ReadOnly") {
    return isAdminOrModerator;
  }
  return false;
}