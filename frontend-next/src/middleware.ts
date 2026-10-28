import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";
import { AUTH_SESSION_COOKIE } from "@/lib/auth/authConstants";
import { isSafeInternalRedirect } from "@/lib/bff/safeRedirect";
import { pathAllowedForRoles } from "@/lib/bff/middlewareAuth";
import { isActionTokenPath, redirectWithoutQueryToken } from "@/lib/bff/actionToken";
import { unsealSession } from "@/lib/bff/session";

const PROTECTED_PATHS = [
  "/setup",
  "/models/upload",
  "/settings",
  "/my-models",
  "/ext",
  "/admin",
  "/moderation",
];

function isProtectedPath(pathname: string): boolean {
  return PROTECTED_PATHS.some(
    (path) => pathname === path || pathname.startsWith(`${path}/`)
  );
}

export async function middleware(request: NextRequest) {
  const pathname = request.nextUrl.pathname;

  if (isActionTokenPath(pathname)) {
    const stripped = redirectWithoutQueryToken(request);
    if (stripped) {
      return stripped;
    }
    return NextResponse.next();
  }

  if (!isProtectedPath(pathname)) {
    return NextResponse.next();
  }

  const value = request.cookies.get(AUTH_SESSION_COOKIE)?.value;
  if (!value || value === "1" || value.length <= 20) {
    const loginUrl = new URL("/login", request.url);
    const redirectPath = pathname;
    if (isSafeInternalRedirect(redirectPath, request.nextUrl.origin)) {
      loginUrl.searchParams.set("redirect", redirectPath);
    }
    return NextResponse.redirect(loginUrl);
  }

  const session = await unsealSession(value);
  if (!session) {
    const loginUrl = new URL("/login", request.url);
    if (isSafeInternalRedirect(pathname, request.nextUrl.origin)) {
      loginUrl.searchParams.set("redirect", pathname);
    }
    return NextResponse.redirect(loginUrl);
  }

  if (!pathAllowedForRoles(pathname, session.roles)) {
    return NextResponse.redirect(new URL("/dashboard", request.url));
  }

  return NextResponse.next();
}

export const config = {
  matcher: [
    "/verify-email",
    "/reset-password",
    "/setup",
    "/setup/:path*",
    "/models/upload",
    "/models/upload/:path*",
    "/settings",
    "/settings/:path*",
    "/my-models",
    "/my-models/:path*",
    "/ext",
    "/ext/:path*",
    "/admin",
    "/admin/:path*",
    "/moderation",
    "/moderation/:path*",
  ],
};
