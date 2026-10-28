import type { NextRequest } from "next/server";
import { NextResponse } from "next/server";
import { AUTH_ACTION_TOKEN_COOKIE } from "@/lib/auth/authConstants";
import { isProduction } from "./env";

export const ACTION_TOKEN_MAX_AGE_SECONDS = 300;

const ACTION_TOKEN_PATHS = ["/verify-email", "/reset-password"];

export function isActionTokenPath(pathname: string): boolean {
  return ACTION_TOKEN_PATHS.includes(pathname);
}

export function redirectWithoutQueryToken(request: NextRequest): NextResponse | null {
  const token = request.nextUrl.searchParams.get("token");
  if (!token?.trim()) {
    return null;
  }
  const clean = new URL(request.url);
  clean.searchParams.delete("token");
  const response = NextResponse.redirect(clean);
  response.cookies.set(AUTH_ACTION_TOKEN_COOKIE, token, {
    httpOnly: true,
    secure: isProduction(),
    sameSite: "lax",
    path: "/",
    maxAge: ACTION_TOKEN_MAX_AGE_SECONDS,
  });
  return response;
}

export function clearActionTokenCookie(response: NextResponse): void {
  response.cookies.set(AUTH_ACTION_TOKEN_COOKIE, "", {
    httpOnly: true,
    secure: isProduction(),
    sameSite: "lax",
    path: "/",
    maxAge: 0,
  });
}
