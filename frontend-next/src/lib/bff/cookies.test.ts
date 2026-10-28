import { describe, it, expect, vi, beforeEach } from "vitest";
import { appendSessionCookie, clearSessionCookie } from "./cookies";
import { NextResponse } from "next/server";

describe("session cookies", () => {
  beforeEach(() => {
    vi.stubEnv("NODE_ENV", "production");
  });

  it("sets httpOnly and secure on session cookie", async () => {
    const response = NextResponse.json({});
    const setCookie = vi.spyOn(response.cookies, "set");
    await appendSessionCookie(
      response,
      {
        accessToken: "access-token-value",
        refreshToken: "refresh-token-value",
        accessExp: Date.now() + 60_000,
        roles: ["User"],
      },
      3600
    );
    expect(setCookie).toHaveBeenCalledWith(
      "polybucket-session",
      expect.any(String),
      expect.objectContaining({
        httpOnly: true,
        secure: true,
        sameSite: "lax",
      })
    );
  });

  it("clears session cookie with maxAge 0", () => {
    const response = NextResponse.json({});
    const setCookie = vi.spyOn(response.cookies, "set");
    clearSessionCookie(response);
    expect(setCookie).toHaveBeenCalledWith(
      "polybucket-session",
      "",
      expect.objectContaining({ maxAge: 0, httpOnly: true })
    );
  });
});
