import { getPolybucketApiUrl } from "./env";

export type ApiLoginResponse = {
  token?: string;
  refreshToken?: string;
  tokenExpiresAt?: string;
  refreshTokenExpiresAt?: string;
  requiresPasswordChange?: boolean;
  requiresFirstTimeSetup?: boolean;
  setupStep?: string | null;
  requiresTwoFactor?: boolean;
  twoFactorToken?: string | null;
};

export type ApiAuthenticationResponse = {
  accessToken?: string;
  refreshToken?: string;
  accessTokenExpiresAt?: string;
  refreshTokenExpiresAt?: string;
  user?: {
    id?: string;
    email?: string;
    username?: string;
    role?: string;
    isEmailVerified?: boolean;
    avatar?: string;
  };
};

export type ApiRefreshResponse = {
  authentication?: ApiAuthenticationResponse;
};

export async function apiPostJson<T>(
  path: string,
  body: unknown,
  accessToken?: string
): Promise<{ ok: boolean; status: number; data: T | null; text: string }> {
  const base = getPolybucketApiUrl();
  const headers: Record<string, string> = {
    Accept: "application/json",
    "Content-Type": "application/json",
  };
  if (accessToken?.trim()) {
    headers.Authorization = `Bearer ${accessToken}`;
  }
  const res = await fetch(`${base}${path}`, {
    method: "POST",
    headers,
    body: JSON.stringify(body),
    cache: "no-store",
  });
  const text = await res.text();
  let data: T | null = null;
  if (text) {
    try {
      data = JSON.parse(text) as T;
    } catch {
      data = null;
    }
  }
  return { ok: res.ok, status: res.status, data, text };
}

export async function apiGetJson<T>(
  path: string,
  accessToken: string
): Promise<{ ok: boolean; status: number; data: T | null }> {
  const base = getPolybucketApiUrl();
  const res = await fetch(`${base}${path}`, {
    method: "GET",
    headers: {
      Accept: "application/json",
      Authorization: `Bearer ${accessToken}`,
    },
    cache: "no-store",
  });
  let data: T | null = null;
  if (res.ok) {
    try {
      data = (await res.json()) as T;
    } catch {
      data = null;
    }
  }
  return { ok: res.ok, status: res.status, data };
}
