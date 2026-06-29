export const PRIVATE_SITE_MARKER_HEADER = "x-site-access";
export const PRIVATE_SITE_MARKER_VALUE = "login-required";

const AUTH_PATHS = ["/login", "/setup"];

type HeaderLookup =
  | { get(name: string): string | null | undefined }
  | Record<string, unknown>
  | null
  | undefined;

function readHeader(headers: HeaderLookup, name: string): string | null {
  if (!headers) {
    return null;
  }
  const maybeGetter = (headers as { get?: unknown }).get;
  if (typeof maybeGetter === "function") {
    const value =
      (headers as { get(n: string): string | null | undefined }).get(name) ??
      (headers as { get(n: string): string | null | undefined }).get(name.toLowerCase());
    return typeof value === "string" ? value : null;
  }
  const record = headers as Record<string, unknown>;
  const lower = name.toLowerCase();
  for (const key of Object.keys(record)) {
    if (key.toLowerCase() === lower) {
      const value = record[key];
      return typeof value === "string" ? value : null;
    }
  }
  return null;
}

function isAuthPath(pathname: string): boolean {
  return AUTH_PATHS.some((path) => pathname === path || pathname.startsWith(`${path}/`));
}

/**
 * Returns the /login redirect target when a response is a private-site block
 * (401 carrying the X-Site-Access: login-required marker) and the current path
 * is not already an auth path. Returns null otherwise, so ordinary 401s and
 * loop-prone auth pages are left untouched.
 */
export function shouldRedirectToLogin(
  status: number | undefined,
  headers: HeaderLookup,
  currentPath: string
): string | null {
  if (status !== 401) {
    return null;
  }
  const marker = readHeader(headers, PRIVATE_SITE_MARKER_HEADER);
  if (marker?.toLowerCase() !== PRIVATE_SITE_MARKER_VALUE) {
    return null;
  }
  if (isAuthPath(currentPath)) {
    return null;
  }
  const safePath = currentPath.startsWith("/") ? currentPath : "/";
  return `/login?redirect=${encodeURIComponent(safePath)}`;
}
