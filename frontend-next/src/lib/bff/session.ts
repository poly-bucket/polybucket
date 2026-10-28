import { EncryptJWT, jwtDecrypt } from "jose";
import { getSessionSecret } from "./env";
import { getEncryptionKey } from "./sessionKey";

export interface SealedSessionPayload {
  refreshToken: string;
  accessToken: string;
  accessExp: number;
  roles: string[];
}

const SESSION_ALG = "dir";
const SESSION_ENC = "A256GCM";

export async function sealSession(
  payload: SealedSessionPayload,
  maxAgeSeconds: number
): Promise<string> {
  return new EncryptJWT({ ...payload })
    .setProtectedHeader({ alg: SESSION_ALG, enc: SESSION_ENC })
    .setIssuedAt()
    .setExpirationTime(`${maxAgeSeconds}s`)
    .encrypt(await getEncryptionKey());
}

export async function unsealSession(token: string): Promise<SealedSessionPayload | null> {
  try {
    const { payload } = await jwtDecrypt(token, await getEncryptionKey(), {
      contentEncryptionAlgorithms: [SESSION_ENC],
      keyManagementAlgorithms: [SESSION_ALG],
    });
    const refreshToken = payload.refreshToken;
    const accessToken = payload.accessToken;
    const accessExp = payload.accessExp;
    const rolesRaw = payload.roles;
    if (
      typeof refreshToken !== "string" ||
      !refreshToken.trim() ||
      typeof accessToken !== "string" ||
      !accessToken.trim() ||
      typeof accessExp !== "number"
    ) {
      return null;
    }
    const roles = Array.isArray(rolesRaw)
      ? rolesRaw.filter((r): r is string => typeof r === "string")
      : [];
    return { refreshToken, accessToken, accessExp, roles };
  } catch {
    return null;
  }
}

export function accessTokenExpMs(accessToken: string): number {
  try {
    const parts = accessToken.split(".");
    if (parts.length !== 3) {
      return Date.now() + 15 * 60_000;
    }
    const payload = parts[1];
    const padded = payload + "=".repeat((4 - (payload.length % 4)) % 4);
    const binary = atob(padded.replace(/-/g, "+").replace(/_/g, "/"));
    const parsed = JSON.parse(binary) as { exp?: number };
    if (typeof parsed.exp === "number") {
      return parsed.exp * 1000;
    }
  } catch {
    /* fall through */
  }
  return Date.now() + 15 * 60_000;
}

export function withSessionRoles(
  payload: Omit<SealedSessionPayload, "roles">,
  roles: string[] | undefined
): SealedSessionPayload {
  return { ...payload, roles: roles ?? [] };
}
