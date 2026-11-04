import { describe, it, expect } from "vitest";
import {
  buildLoginRedirectUrl,
  resolvePrivateBrowseRedirect,
  resolveRoleProtectedRedirect,
} from "./browseAccess";

const ORIGIN = "http://localhost:3000";

describe("resolvePrivateBrowseRedirect", () => {
  it("redirects anonymous users from /dashboard when public browsing is disabled", () => {
    // Act
    const result = resolvePrivateBrowseRedirect("/dashboard", ORIGIN, false, false);

    // Assert
    expect(result).toBe("/login?redirect=%2Fdashboard");
  });

  it("allows /dashboard when public browsing is enabled", () => {
    // Act
    const result = resolvePrivateBrowseRedirect("/dashboard", ORIGIN, true, false);

    // Assert
    expect(result).toBeNull();
  });

  it("allows browse routes when the user has a valid session on a private site", () => {
    // Act
    const result = resolvePrivateBrowseRedirect("/dashboard", ORIGIN, false, true);

    // Assert
    expect(result).toBeNull();
  });

  it("does not redirect auth paths on a private site", () => {
    // Act
    const result = resolvePrivateBrowseRedirect("/login", ORIGIN, false, false);

    // Assert
    expect(result).toBeNull();
  });

  it("does not redirect /register on a private site", () => {
    // Act
    const result = resolvePrivateBrowseRedirect("/register", ORIGIN, false, false);

    // Assert
    expect(result).toBeNull();
  });
});

describe("resolveRoleProtectedRedirect", () => {
  it("redirects unauthenticated users from /settings", () => {
    // Act
    const result = resolveRoleProtectedRedirect("/settings", ORIGIN, false, undefined);

    // Assert
    expect(result).toBe("/login?redirect=%2Fsettings");
  });

  it("sends non-admin users away from /admin", () => {
    // Act
    const result = resolveRoleProtectedRedirect("/admin/system", ORIGIN, true, ["User"]);

    // Assert
    expect(result).toBe("/dashboard");
  });
});

describe("buildLoginRedirectUrl", () => {
  it("encodes the redirect query parameter", () => {
    // Act
    const result = buildLoginRedirectUrl("/models/abc", ORIGIN);

    // Assert
    expect(result).toBe("/login?redirect=%2Fmodels%2Fabc");
  });
});
