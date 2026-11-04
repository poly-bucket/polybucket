import { isAuthPath } from "@/lib/auth/authPaths";
import { isSafeInternalRedirect } from "@/lib/bff/safeRedirect";
import { pathAllowedForRoles } from "@/lib/bff/middlewareAuth";

export const ROLE_PROTECTED_PATHS = [
  "/setup",
  "/models/upload",
  "/settings",
  "/my-models",
  "/ext",
  "/admin",
  "/moderation",
];

export function isRoleProtectedPath(pathname: string): boolean {
  return ROLE_PROTECTED_PATHS.some(
    (path) => pathname === path || pathname.startsWith(`${path}/`)
  );
}

export function buildLoginRedirectUrl(pathname: string, origin: string): string {
  const loginUrl = new URL("/login", origin);
  if (isSafeInternalRedirect(pathname, origin)) {
    loginUrl.searchParams.set("redirect", pathname);
  }
  return `${loginUrl.pathname}${loginUrl.search}`;
}

export function resolvePrivateBrowseRedirect(
  pathname: string,
  origin: string,
  allowPublicBrowsing: boolean,
  hasValidSession: boolean
): string | null {
  if (isAuthPath(pathname)) {
    return null;
  }
  if (allowPublicBrowsing || hasValidSession) {
    return null;
  }
  return buildLoginRedirectUrl(pathname, origin);
}

export function resolveRoleProtectedRedirect(
  pathname: string,
  origin: string,
  hasValidSession: boolean,
  roles: string[] | undefined
): string | null {
  if (!isRoleProtectedPath(pathname)) {
    return null;
  }
  if (!hasValidSession) {
    return buildLoginRedirectUrl(pathname, origin);
  }
  if (!pathAllowedForRoles(pathname, roles)) {
    return "/dashboard";
  }
  return null;
}
