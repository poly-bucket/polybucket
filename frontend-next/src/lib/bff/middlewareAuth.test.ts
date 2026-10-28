import { describe, it, expect } from "vitest";
import { pathAllowedForRoles, requiredRolesForPath } from "./middlewareAuth";

describe("middlewareAuth", () => {
  it("requires admin for admin paths", () => {
    expect(requiredRolesForPath("/admin/users")).toEqual(["Admin"]);
  });

  it("allows admin on admin paths", () => {
    expect(pathAllowedForRoles("/admin", ["Admin"])).toBe(true);
  });

  it("denies non-admin on admin paths", () => {
    expect(pathAllowedForRoles("/admin", ["User"])).toBe(false);
  });

  it("allows moderator on moderation paths", () => {
    expect(pathAllowedForRoles("/moderation/reports", ["Moderator"])).toBe(true);
  });
});
