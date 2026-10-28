import type { NextRequest, NextResponse } from "next/server";
import { AUTH_SESSION_COOKIE } from "@/lib/auth/authConstants";
import { isProduction } from "./env";
import { sealSession, type SealedSessionPayload } from "./session";

const DEFAULT_REFRESH_MAX_AGE = 60 * 60 * 24 * 7;

export function readSessionCookieValue(request: NextRequest): string | null {
  const value = request.cookies.get(AUTH_SESSION_COOKIE)?.value;
  if (!value || value === "1") {
    return null;
  }
  return value;
}

export function hasValidSessionCookieShape(request: NextRequest): boolean {
  const value = readSessionCookieValue(request);
  return Boolean(value && value.length > 20);
}

export async function appendSessionCookie(
  response: NextResponse,
  payload: SealedSessionPayload,
  refreshMaxAgeSeconds: number = DEFAULT_REFRESH_MAX_AGE
): Promise<void> {
  const sealed = await sealSession(payload, refreshMaxAgeSeconds);
  response.cookies.set(AUTH_SESSION_COOKIE, sealed, {
    httpOnly: true,
    secure: isProduction(),
    sameSite: "lax",
    path: "/",
    maxAge: refreshMaxAgeSeconds,
  });
}

export function clearSessionCookie(response: NextResponse): void {
  response.cookies.set(AUTH_SESSION_COOKIE, "", {
    httpOnly: true,
    secure: isProduction(),
    sameSite: "lax",
    path: "/",
    maxAge: 0,
  });
}

export function refreshMaxAgeFromIso(iso: string | undefined): number {
  if (!iso) {
    return DEFAULT_REFRESH_MAX_AGE;
  }
  const ms = Date.parse(iso);
  if (Number.isNaN(ms)) {
    return DEFAULT_REFRESH_MAX_AGE;
  }
  const seconds = Math.floor((ms - Date.now()) / 1000);
  return Math.max(60, Math.min(seconds, 60 * 60 * 24 * 30));
}
