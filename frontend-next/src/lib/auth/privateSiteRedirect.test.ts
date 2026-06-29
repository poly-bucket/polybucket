import { describe, it, expect } from "vitest";
import {
  PRIVATE_SITE_MARKER_HEADER,
  PRIVATE_SITE_MARKER_VALUE,
  shouldRedirectToLogin,
} from "./privateSiteRedirect";

describe("shouldRedirectToLogin", () => {
  it("returns the encoded /login target for a marked 401 on a normal path", () => {
    // Arrange
    const headers = { [PRIVATE_SITE_MARKER_HEADER]: PRIVATE_SITE_MARKER_VALUE };

    // Act
    const result = shouldRedirectToLogin(401, headers, "/models/123");

    // Assert
    expect(result).toBe("/login?redirect=%2Fmodels%2F123");
  });

  it("reads the marker case-insensitively and via a Headers-like getter", () => {
    // Arrange
    const headers = new Headers({ "X-Site-Access": PRIVATE_SITE_MARKER_VALUE });

    // Act
    const result = shouldRedirectToLogin(401, headers, "/dashboard");

    // Assert
    expect(result).toBe("/login?redirect=%2Fdashboard");
  });

  it("returns null when already on the /login path (no loop)", () => {
    // Arrange
    const headers = { [PRIVATE_SITE_MARKER_HEADER]: PRIVATE_SITE_MARKER_VALUE };

    // Act
    const result = shouldRedirectToLogin(401, headers, "/login");

    // Assert
    expect(result).toBeNull();
  });

  it("returns null when on a /setup path (no loop)", () => {
    // Arrange
    const headers = { [PRIVATE_SITE_MARKER_HEADER]: PRIVATE_SITE_MARKER_VALUE };

    // Act
    const result = shouldRedirectToLogin(401, headers, "/setup/admin");

    // Assert
    expect(result).toBeNull();
  });

  it("returns null for a 401 without the marker (ordinary 401)", () => {
    // Arrange
    const headers = { "content-type": "application/json" };

    // Act
    const result = shouldRedirectToLogin(401, headers, "/models/123");

    // Assert
    expect(result).toBeNull();
  });

  it("returns null for a non-401 response carrying the marker", () => {
    // Arrange
    const headers = { [PRIVATE_SITE_MARKER_HEADER]: PRIVATE_SITE_MARKER_VALUE };

    // Act
    const result = shouldRedirectToLogin(403, headers, "/models/123");

    // Assert
    expect(result).toBeNull();
  });
});
