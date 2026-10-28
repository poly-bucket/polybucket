import { minidenticon } from "minidenticons";
import { sanitizeSvg } from "@/lib/security/sanitizeSvg";
import { safeExternalHrefForImage } from "@/lib/security/safeUrl";

const DEFAULT_SAT = 50;
const DEFAULT_LIGHT = 50;

export function minidenticonSvg(
  seed: string,
  saturation: number = DEFAULT_SAT,
  lightness: number = DEFAULT_LIGHT
): string {
  return minidenticon(seed, saturation, lightness);
}

export function svgToDataUrl(svg: string): string {
  const safe = sanitizeSvg(svg);
  if (!safe) {
    return "";
  }
  return `data:image/svg+xml,${encodeURIComponent(safe)}`;
}

function looksLikeSvg(s: string): boolean {
  return s.trimStart().toLowerCase().startsWith("<svg");
}

const allowHttpImages = process.env.NODE_ENV !== "production";

/**
 * Resolves a stored user/collection avatar field to a value suitable for <img src>.
 * Handles http(s) URLs, sanitized raw SVG markup, and some legacy encodings.
 */
export function resolvedImageSrcFromAvatarField(
  value: string | undefined | null
): string | null {
  if (value == null) return null;
  const t = value.trim();
  if (t === "") return null;

  if (t.startsWith("http://") || t.startsWith("https://")) {
    return safeExternalHrefForImage(t, allowHttpImages);
  }
  if (t.startsWith("data:")) {
    return null;
  }
  if (looksLikeSvg(t)) {
    const safe = sanitizeSvg(t);
    return safe ? svgToDataUrl(safe) : null;
  }

  if (!t.includes("<") && t.length >= 8 && typeof atob === "function") {
    const normalized = t.replace(/\s/g, "");
    try {
      const bin = atob(normalized);
      if (bin.trimStart().toLowerCase().startsWith("<svg")) {
        const safe = sanitizeSvg(bin);
        return safe ? `data:image/svg+xml;base64,${btoa(safe)}` : null;
      }
    } catch {
      // not base64
    }
  }

  return null;
}

/**
 * Minidenticon preview for a user, matching old avatarService.previewAvatar / AvatarRegeneration.
 */
export function previewUserAvatarSvg(userId: string, salt?: string): string {
  const seed = salt ? `${userId}-${salt}` : userId;
  return minidenticonSvg(seed, DEFAULT_SAT, DEFAULT_LIGHT);
}

export const maxCollectionAvatarSvgLength = 256_000;

/**
 * Map a single `avatar` string from search APIs onto UserAvatar props:
 * treat absolute URLs as profile photos; everything else as stored SVG/identicon.
 */
export function splitAvatarForDisplay(avatar?: string | null): {
  profilePictureUrl?: string;
  storedAvatar?: string;
} {
  if (avatar == null || !String(avatar).trim()) return {};
  const t = String(avatar).trim();
  if (t.startsWith("http://") || t.startsWith("https://")) {
    const url = safeExternalHrefForImage(t, allowHttpImages);
    return url ? { profilePictureUrl: url } : {};
  }
  return { storedAvatar: t };
}
