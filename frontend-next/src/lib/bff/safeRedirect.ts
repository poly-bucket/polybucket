export function isSafeInternalRedirect(path: string | null | undefined, origin: string): boolean {
  if (!path || !path.startsWith("/") || path.startsWith("//") || path.includes("\\")) {
    return false;
  }
  try {
    const url = new URL(path, origin);
    if (url.origin !== origin) {
      return false;
    }
    if (!url.pathname.startsWith("/")) {
      return false;
    }
    return true;
  } catch {
    return false;
  }
}
