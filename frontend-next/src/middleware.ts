import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";
import {
  resolvePrivateBrowseRedirect,
  resolveRoleProtectedRedirect,
} from "@/lib/auth/browseAccess";
import {
  E2E_ALLOW_PUBLIC_BROWSING_COOKIE,
  readE2EPublicBrowsingOverride,
} from "@/lib/auth/e2eSiteAccessOverride";
import { fetchSiteAccessPolicy } from "@/lib/bff/fetchSiteAccessPolicy";
import { isActionTokenPath, redirectWithoutQueryToken } from "@/lib/bff/actionToken";
import {
  hasValidSessionCookieShape,
  readSessionCookieValue,
} from "@/lib/bff/cookies";
import { unsealSession } from "@/lib/bff/session";

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
  const publicBrowsingOverride = readE2EPublicBrowsingOverride(
    process.env.E2E_SITE_ACCESS_OVERRIDE,
    request.cookies.get(E2E_ALLOW_PUBLIC_BROWSING_COOKIE)?.value
  );
  const allowPublicBrowse = publicBrowsingOverride ?? allowPublicBrowsing;

  const sealedCookie = readSessionCookieValue(request);
  const hasBrowseSession = hasValidSessionCookieShape(request);
  let session = null;
  if (sealedCookie) {
    session = await unsealSession(sealedCookie);
  }
  const hasValidSession = session !== null;

  const privateRedirect = resolvePrivateBrowseRedirect(
    pathname,
    origin,
    allowPublicBrowse,
    hasBrowseSession || hasValidSession
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
