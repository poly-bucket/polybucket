// @vitest-environment node
import { describe, it, expect, beforeEach, afterEach } from "vitest";
import { sealSession, unsealSession } from "./session";
import { clearEncryptionKeyCacheForTests } from "./sessionKey";

describe("sealed session", () => {
  const previousSecret = process.env.SESSION_SECRET;

  beforeEach(() => {
    process.env.SESSION_SECRET = "test-session-secret-at-least-32-chars";
    clearEncryptionKeyCacheForTests();
  });

  afterEach(() => {
    process.env.SESSION_SECRET = previousSecret;
    clearEncryptionKeyCacheForTests();
  });

  it("seals and unseals a payload with roles", async () => {
    const payload = {
      refreshToken: "refresh",
      accessToken: "access",
      accessExp: Date.now() + 60_000,
      roles: ["Admin"],
    };
    const sealed = await sealSession(payload, 3600);
    const opened = await unsealSession(sealed);
    expect(opened?.refreshToken).toBe("refresh");
    expect(opened?.accessToken).toBe("access");
    expect(opened?.accessExp).toBe(payload.accessExp);
    expect(opened?.roles).toEqual(["Admin"]);
  });
});
