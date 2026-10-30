// @vitest-environment node
import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { handleRegisterBody } from "./authOperations";
import { clearEncryptionKeyCacheForTests } from "./sessionKey";

const mockPost = vi.fn();

vi.mock("./apiClient", () => ({
  apiPostJson: (...args: unknown[]) => mockPost(...args),
  apiGetJson: vi.fn().mockResolvedValue({ ok: false, status: 401, data: null }),
}));

vi.mock("./userProfile", () => ({
  profileFromAuthentication: () => ({
    id: "user-1",
    email: "new@test.com",
    username: "newuser",
    roles: ["User"],
  }),
  toClientJson: (profile: { id: string }) => ({ user: profile }),
  rolesFromRoleString: (role?: string | null) => (role ? [role] : []),
  profileFromAccessToken: () => null,
  profileFromLoginResponse: () => null,
}));

describe("handleRegisterBody", () => {
  const previousSecret = process.env.SESSION_SECRET;

  beforeEach(() => {
    process.env.SESSION_SECRET = "test-session-secret-at-least-32-chars";
    clearEncryptionKeyCacheForTests();
    mockPost.mockReset();
  });

  afterEach(() => {
    process.env.SESSION_SECRET = previousSecret;
    clearEncryptionKeyCacheForTests();
  });

  it("returns verification outcome without setting a session cookie when verification is required", async () => {
    // Arrange
    mockPost.mockResolvedValue({
      ok: true,
      status: 200,
      data: { requiresEmailVerification: true, authentication: null },
      text: "{}",
    });

    // Act
    const outcome = await handleRegisterBody({
      email: "new@test.com",
      username: "newuser",
      password: "Password123!",
      confirmPassword: "Password123!",
    });

    // Assert
    expect(outcome.kind).toBe("verification");
    expect(outcome.response.headers.get("set-cookie")).toBeNull();
    const json = await outcome.response.json();
    expect(json.requiresEmailVerification).toBe(true);
  });

  it("seals a session cookie and omits tokens from JSON when registration returns tokens", async () => {
    // Arrange
    mockPost.mockResolvedValue({
      ok: true,
      status: 200,
      data: {
        requiresEmailVerification: false,
        authentication: {
          accessToken: "access-token-value",
          refreshToken: "refresh-token-value",
          refreshTokenExpiresAt: new Date(Date.now() + 86400000).toISOString(),
        },
      },
      text: "{}",
    });

    // Act
    const outcome = await handleRegisterBody({
      email: "new@test.com",
      username: "newuser",
      password: "Password123!",
      confirmPassword: "Password123!",
    });

    // Assert
    expect(outcome.kind).toBe("success");
    expect(outcome.response.headers.get("set-cookie")).toContain("polybucket-session");
    const json = await outcome.response.json();
    expect(json.user?.id).toBe("user-1");
    expect(JSON.stringify(json)).not.toContain("access-token-value");
  });
});
