import type { SiteAccessPolicy } from "@/lib/bff/fetchSiteAccessPolicy";

export function areModelVersionsEnabled(
  policy: Pick<SiteAccessPolicy, "modelVersioningEnabled">
): boolean {
  return policy.modelVersioningEnabled !== false;
}

export function canMutateModelVersions(
  policy: Pick<SiteAccessPolicy, "modelVersioningEnabled">
): boolean {
  return areModelVersionsEnabled(policy);
}

export function canViewModelVersionHistory(
  policy: Pick<SiteAccessPolicy, "modelVersioningEnabled">,
  context: { isOwner: boolean; isAdminOrModerator: boolean }
): boolean {
  if (areModelVersionsEnabled(policy)) {
    return true;
  }

  return context.isOwner || context.isAdminOrModerator;
}