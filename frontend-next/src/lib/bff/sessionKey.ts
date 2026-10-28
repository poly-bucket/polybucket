import { getSessionSecret } from "./env";

let cachedKey: Uint8Array | null = null;

export async function getEncryptionKey(): Promise<Uint8Array> {
  if (cachedKey) {
    return cachedKey;
  }
  const data = new TextEncoder().encode(getSessionSecret());
  const hash = await crypto.subtle.digest("SHA-256", data);
  cachedKey = new Uint8Array(hash);
  return cachedKey;
}

export function clearEncryptionKeyCacheForTests(): void {
  cachedKey = null;
}
