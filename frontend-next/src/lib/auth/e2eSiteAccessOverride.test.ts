import { describe, expect, it } from "vitest";
import { readE2EPublicBrowsingOverride } from "./e2eSiteAccessOverride";

describe("readE2EPublicBrowsingOverride", () => {
  it("returns null when the env flag is off", () => {
    // Arrange
    const envFlag = undefined;
    const cookieValue = "1";

    // Act
    const result = readE2EPublicBrowsingOverride(envFlag, cookieValue);

    // Assert
    expect(result).toBeNull();
  });

  it("returns null when the cookie is missing", () => {
    // Arrange
    const envFlag = "1";
    const cookieValue = undefined;

    // Act
    const result = readE2EPublicBrowsingOverride(envFlag, cookieValue);

    // Assert
    expect(result).toBeNull();
  });

  it("returns true when the flag is on and the cookie is 1", () => {
    // Arrange
    const envFlag = "1";
    const cookieValue = "1";

    // Act
    const result = readE2EPublicBrowsingOverride(envFlag, cookieValue);

    // Assert
    expect(result).toBe(true);
  });

  it("returns false when the flag is on and the cookie is 0", () => {
    // Arrange
    const envFlag = "1";
    const cookieValue = "0";

    // Act
    const result = readE2EPublicBrowsingOverride(envFlag, cookieValue);

    // Assert
    expect(result).toBe(false);
  });

  it("returns null when the cookie value is unknown", () => {
    // Arrange
    const envFlag = "1";
    const cookieValue = "yes";

    // Act
    const result = readE2EPublicBrowsingOverride(envFlag, cookieValue);

    // Assert
    expect(result).toBeNull();
  });
});
