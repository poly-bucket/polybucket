import {
  areModelVersionsEnabled,
  canMutateModelVersions,
  canViewModelVersionHistory,
} from "./versioningAccess";

describe("versioningAccess", () => {
  it("treats undefined modelVersioningEnabled as enabled", () => {
    expect(areModelVersionsEnabled({ modelVersioningEnabled: true })).toBe(true);
  });

  it("canMutateModelVersions follows the site flag", () => {
    expect(canMutateModelVersions({ modelVersioningEnabled: false })).toBe(false);
    expect(canMutateModelVersions({ modelVersioningEnabled: true })).toBe(true);
  });

  it("canViewModelVersionHistory allows owner when versioning is disabled", () => {
    const policy = { modelVersioningEnabled: false };
    expect(
      canViewModelVersionHistory(policy, { isOwner: true, isAdminOrModerator: false })
    ).toBe(true);
    expect(
      canViewModelVersionHistory(policy, { isOwner: false, isAdminOrModerator: false })
    ).toBe(false);
  });

  it("canViewModelVersionHistory allows admin when versioning is disabled", () => {
    expect(
      canViewModelVersionHistory(
        { modelVersioningEnabled: false },
        { isOwner: false, isAdminOrModerator: true }
      )
    ).toBe(true);
  });
});