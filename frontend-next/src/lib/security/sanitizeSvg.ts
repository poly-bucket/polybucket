import { maxCollectionAvatarSvgLength } from "@/lib/avatar/minidenticon";

const EVENT_HANDLER = /\s(on[a-z]+)\s*=\s*("[^"]*"|'[^']*'|[^\s>]+)/gi;
const SCRIPT_BLOCK = /<script[\s\S]*?<\/script>/gi;
const FOREIGN_OBJECT = /<foreignObject[\s\S]*?<\/foreignObject>/gi;

export function sanitizeSvg(raw: string, maxLength: number = maxCollectionAvatarSvgLength): string | null {
  if (raw.length > maxLength) return null;
  const trimmed = raw.trim();
  if (!trimmed.toLowerCase().startsWith("<svg")) return null;

  let out = trimmed.replace(SCRIPT_BLOCK, "").replace(FOREIGN_OBJECT, "");
  out = out.replace(EVENT_HANDLER, "");
  out = out.replace(/\s(xlink:href|href)\s*=\s*("https?:[^"]*"|'https?:[^']*')/gi, "");
  out = out.replace(/\s(xlink:href|href)\s*=\s*("[^"]*"|'[^']*')/gi, "");

  if (!out.trim().toLowerCase().startsWith("<svg")) return null;
  return out;
}
