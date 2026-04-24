import axios from "axios";
import type { AuthenticationResponse } from "@/lib/api/client";
import { getApiConfig } from "@/lib/api/config";
import { extractUserFromJWT, isTokenExpired } from "@/lib/utils/jwtUtils";
import { AUTH_SESSION_COOKIE, AUTH_STORAGE_KEY } from "./authConstants";

export { AUTH_SESSION_COOKIE, AUTH_STORAGE_KEY } from "./authConstants";

export interface AuthUser {
  id: string;
  email: string;
  username: string;
  accessToken: string;
  refreshToken?: string;
  roles?: string[];
  requiresFirstTimeSetup?: boolean;
  setupStep?: string;
  avatar?: string;
  profilePictureUrl?: string;
}

type AuthSessionListener = (user: AuthUser | null) => void;

const listeners = new Set<AuthSessionListener>();

export function subscribeAuthSession(listener: AuthSessionListener): () => void {
  listeners.add(listener);
  return () => {
    listeners.delete(listener);
  };
}

function emitAuthSession(user: AuthUser | null): void {
  for (const l of listeners) {
    l(user);
  }
}

function setSessionCookie(present: boolean): void {
  if (typeof document === "undefined") return;
  if (present) {
    document.cookie = `${AUTH_SESSION_COOKIE}=1; path=/; max-age=${60 * 60 * 24 * 7}; SameSite=Lax`;
  } else {
    document.cookie = `${AUTH_SESSION_COOKIE}=; path=/; max-age=0`;
  }
}

export function getStoredUser(): AuthUser | null {
  if (typeof window === "undefined") return null;
  try {
    const stored = localStorage.getItem(AUTH_STORAGE_KEY);
    if (!stored) return null;
    const parsed = JSON.parse(stored) as AuthUser;
    if (!parsed.accessToken) return null;
    return parsed;
  } catch {
    return null;
  }
}

export function persistUser(user: AuthUser | null): void {
  if (typeof window === "undefined") return;
  if (user) {
    localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(user));
    setSessionCookie(true);
  } else {
    localStorage.removeItem(AUTH_STORAGE_KEY);
    setSessionCookie(false);
  }
  emitAuthSession(user);
}

export function mapAuthenticationResponseToAuthUser(
  auth: AuthenticationResponse,
  previous?: AuthUser | null
): AuthUser | null {
  const at = auth.accessToken;
  if (!at?.trim()) {
    return null;
  }
  const u = auth.user;
  const profileFromApi = (u as { profilePictureUrl?: string } | undefined)?.profilePictureUrl;
  const decoded = extractUserFromJWT(at);
  if (decoded) {
    return {
      id: decoded.id,
      email: decoded.email,
      username: decoded.username,
      accessToken: at,
      refreshToken: auth.refreshToken ?? previous?.refreshToken,
      roles: decoded.role ? [decoded.role] : previous?.roles,
      requiresFirstTimeSetup: previous?.requiresFirstTimeSetup,
      setupStep: previous?.setupStep,
      avatar: u?.avatar ?? previous?.avatar,
      profilePictureUrl: profileFromApi ?? previous?.profilePictureUrl,
    };
  }
  if (u?.id && u.email && u.username) {
    return {
      id: u.id,
      email: u.email,
      username: u.username,
      accessToken: at,
      refreshToken: auth.refreshToken ?? previous?.refreshToken,
      roles: u.role ? [u.role] : previous?.roles,
      requiresFirstTimeSetup: previous?.requiresFirstTimeSetup,
      setupStep: previous?.setupStep,
      avatar: u.avatar ?? previous?.avatar,
      profilePictureUrl: profileFromApi ?? previous?.profilePictureUrl,
    };
  }
  return null;
}

export function isSessionRestorable(user: AuthUser | null | undefined): boolean {
  if (!user?.accessToken?.trim()) {
    return false;
  }
  if (!isTokenExpired(user.accessToken)) {
    return true;
  }
  return Boolean(user.refreshToken?.trim());
}

type RefreshResponseBody = { authentication?: AuthenticationResponse };

let refreshLock: Promise<AuthUser | null> | null = null;

function runRefreshWithPlainAxios(): Promise<AuthUser | null> {
  return (async () => {
    const previous = getStoredUser();
    if (!previous?.refreshToken?.trim()) {
      return null;
    }
    const config = getApiConfig();
    const base = config.baseUrl.replace(/\/$/, "");
    let data: RefreshResponseBody;
    try {
      const res = await axios.post<RefreshResponseBody>(`${base}/api/auth/refresh-token`, {
        refreshToken: previous.refreshToken,
      },
      {
        headers: { "Content-Type": "application/json", Accept: "application/json" },
        timeout: config.timeout,
        withCredentials: config.withCredentials,
      });
      data = res.data;
    } catch {
      return null;
    }
    const next = data?.authentication
      ? mapAuthenticationResponseToAuthUser(data.authentication, previous)
      : null;
    if (!next) {
      return null;
    }
    persistUser(next);
    return next;
  })();
}

export function getOrCreateRefreshPromise(): Promise<AuthUser | null> {
  if (refreshLock) {
    return refreshLock;
  }
  const p = runRefreshWithPlainAxios()
    .catch((): null => null)
    .finally(() => {
      refreshLock = null;
    });
  refreshLock = p;
  return p;
}

export function clearSession(): void {
  persistUser(null);
}
