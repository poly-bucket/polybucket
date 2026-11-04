import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";
import { AUTH_SESSION_COOKIE } from "@/lib/auth/authConstants";
import {
  resolvePrivateBrowseRedirect,
  resolveRoleProtectedRedirect,
} from "@/lib/auth/browseAccess";
import { fetchSiteAccessPolicy } from "@/lib/bff/fetchSiteAccessPolicy";
import { isActionTokenPath, redirectWithoutQueryToken } from "@/lib/bff/actionToken";
import { unsealSession } from "@/lib/bff/session";

function hasSessionCookieValue(value: string | undefined): boolean {
  return Boolean(value && value !== "1" && value.length > 20);
}

export async function middleware(request: NextRequest) {
  const pathname = request.nextUrl.pathname;

  if (pathname.startsWith("/api/")) {
    return NextResponse.next();
  }

  if (isActionTokenPath(pathname)) {
    const stripped = redirectWithoutQueryToken(request);
    if (stripped) {
      return stripped;
    }
    return NextResponse.next();
  }

  const origin = request.nextUrl.origin;
  const { allowPublicBrowsing } = await fetchSiteAccessPolicy(origin);

  const cookieValue = request.cookies.get(AUTH_SESSION_COOKIE)?.value;
  let hasValidSession = false;
  let session = null;
  if (hasSessionCookieValue(cookieValue)) {
    session = await unsealSession(cookieValue!);
    hasValidSession = session !== null;
  }

  const privateRedirect = resolvePrivateBrowseRedirect(
    pathname,
    origin,
    allowPublicBrowsing,
    hasValidSession
  );
  if (privateRedirect) {
    return NextResponse.redirect(new URL(privateRedirect, request.url));
  }

  const roleRedirect = resolveRoleProtectedRedirect(
    pathname,
    origin,
    hasValidSession,
    session?.roles
  );
  if (roleRedirect) {
    return NextResponse.redirect(new URL(roleRedirect, request.url));
  }

  return NextResponse.next();
}

export const config = {
  matcher: [
    "/((?!_next/static|_next/image|favicon.ico|.*\\.(?:svg|png|jpg|jpeg|gif|webp|ico)$).*)",
  ],
};
