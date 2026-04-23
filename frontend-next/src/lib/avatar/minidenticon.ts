import { minidenticon } from "minidenticons";

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
  return `data:image/svg+xml,${encodeURIComponent(svg)}`;
}

function looksLikeSvg(s: string): boolean {
  return s.trimStart().toLowerCase().startsWith("<svg");
}

/**
 * Resolves a stored user/collection avatar field to a value suitable for <img src>.
 * Handles http(s) URLs, data: URLs, raw SVG markup, and some legacy encodings.
 */
export function resolvedImageSrcFromAvatarField(
  value: string | undefined | null
): string | null {
  if (value == null) return null;
  const t = value.trim();
  if (t === "") return null;

  if (t.startsWith("http://") || t.startsWith("https://")) {
    return t;
  }
  if (t.startsWith("data:")) {
    return t;
  }
  if (looksLikeSvg(t)) {
    return svgToDataUrl(t);
  }

  if (!t.includes("<") && t.length >= 8 && typeof atob === "function") {
    const normalized = t.replace(/\s/g, "");
    try {
      const bin = atob(normalized);
      if (bin.trimStart().toLowerCase().startsWith("<svg")) {
        return `data:image/svg+xml;base64,${normalized}`;
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
    return { profilePictureUrl: t };
  }
  return { storedAvatar: t };
}
