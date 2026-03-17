import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

const PROTECTED_PATHS = [
  "/setup",
  "/models/upload",
  "/settings",
  "/my-models",
  "/ext",
  "/admin",
  "/moderation",
];
const AUTH_SESSION_COOKIE = "polybucket-session";

function isProtectedPath(pathname: string): boolean {
  return PROTECTED_PATHS.some(
    (path) => pathname === path || pathname.startsWith(`${path}/`)
  );
}

export function middleware(request: NextRequest) {
  if (!isProtectedPath(request.nextUrl.pathname)) {
    return NextResponse.next();
  }

  const sessionCookie = request.cookies.get(AUTH_SESSION_COOKIE);

  if (!sessionCookie?.value) {
    const loginUrl = new URL("/login", request.url);
    loginUrl.searchParams.set("redirect", request.nextUrl.pathname);
    return NextResponse.redirect(loginUrl);
  }

  return NextResponse.next();
}

export const config = {
  matcher: [
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
