const CONTROL_OR_BAD = /[\u0000-\u001F\u007F\\]/;

export function safeExternalHref(raw: string | undefined | null): string | null {
  if (raw == null) return null;
  const t = raw.trim();
  if (t === "" || CONTROL_OR_BAD.test(t)) return null;
  let parsed: URL;
  try {
    parsed = new URL(t);
  } catch {
    return null;
  }
  if (parsed.protocol !== "http:" && parsed.protocol !== "https:") {
    return null;
  }
  return parsed.href;
}

export function safeInternalPath(raw: string | undefined | null): string | null {
  if (raw == null) return null;
  const t = raw.trim();
  if (t === "" || !t.startsWith("/") || t.startsWith("//")) return null;
  if (CONTROL_OR_BAD.test(t)) return null;
  if (/^[a-zA-Z][a-zA-Z0-9+.-]*:/.test(t)) return null;
  return t;
}

export function safeNotificationHref(raw: string | undefined | null): string | null {
  return safeInternalPath(raw);
}

export function safeExternalHrefForImage(
  raw: string | undefined | null,
  allowHttpInDev: boolean
): string | null {
  const href = safeExternalHref(raw);
  if (!href) return null;
  if (href.startsWith("https:")) return href;
  if (allowHttpInDev && href.startsWith("http:")) return href;
  return null;
}
