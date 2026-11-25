import { describe, expect, it } from "vitest";
import {
  canMutateCollections,
  canShowCommentsSection,
  canWriteComments,
  getCommentsAccess,
} from "./featureAccess";

describe("featureAccess", () => {
  it("treats missing access as Full", () => {
    expect(getCommentsAccess({ modelCommentsAccess: "Full" })).toBe("Full");
  });

  it("hides comments when access is Hidden", () => {
    expect(canShowCommentsSection({ modelCommentsAccess: "Hidden" })).toBe(false);
  });

  it("blocks user writes in ReadOnly unless admin or moderator", () => {
    expect(canWriteComments({ modelCommentsAccess: "ReadOnly" }, false)).toBe(false);
    expect(canWriteComments({ modelCommentsAccess: "ReadOnly" }, true)).toBe(true);
  });

  it("blocks collection mutations when Hidden", () => {
    expect(canMutateCollections({ modelCollectionsAccess: "Hidden" }, true)).toBe(false);
  });
});