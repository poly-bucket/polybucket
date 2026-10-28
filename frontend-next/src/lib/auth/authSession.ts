import { AUTH_PROFILE_STORAGE_KEY, AUTH_STORAGE_KEY } from "./authConstants";

export { AUTH_SESSION_COOKIE, AUTH_STORAGE_KEY, AUTH_PROFILE_STORAGE_KEY } from "./authConstants";

export interface AuthUser {
  id: string;
  email: string;
  username: string;
  roles?: string[];
  requiresFirstTimeSetup?: boolean;
  setupStep?: string;
  avatar?: string;
  profilePictureUrl?: string;
  isEmailVerified?: boolean;
  accessExpiresAt?: number;
}

type AuthSessionListener = (user: AuthUser | null) => void;

const listeners = new Set<AuthSessionListener>();

let memoryUser: AuthUser | null = null;

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

function readProfileFromSessionStorage(): AuthUser | null {
  if (typeof window === "undefined") return null;
  try {
    const stored = sessionStorage.getItem(AUTH_PROFILE_STORAGE_KEY);
    if (!stored) return null;
    const parsed = JSON.parse(stored) as AuthUser;
    if (!parsed.id || !parsed.email) return null;
    return parsed;
  } catch {
    return null;
  }
}

function writeProfileToSessionStorage(user: AuthUser | null): void {
  if (typeof window === "undefined") return;
  if (user) {
    sessionStorage.setItem(AUTH_PROFILE_STORAGE_KEY, JSON.stringify(user));
  } else {
    sessionStorage.removeItem(AUTH_PROFILE_STORAGE_KEY);
  }
}

export function purgeLegacyTokenStorage(): void {
  if (typeof window === "undefined") return;
  localStorage.removeItem(AUTH_STORAGE_KEY);
}

export function getStoredUser(): AuthUser | null {
  if (memoryUser) {
    return memoryUser;
  }
  const fromSession = readProfileFromSessionStorage();
  memoryUser = fromSession;
  return fromSession;
}

export function persistUser(user: AuthUser | null): void {
  if (typeof window === "undefined") return;
  memoryUser = user;
  writeProfileToSessionStorage(user);
  emitAuthSession(user);
}

export function isSessionRestorable(user: AuthUser | null | undefined): boolean {
  return Boolean(user?.id?.trim() && user?.email?.trim());
}

let refreshLock: Promise<AuthUser | null> | null = null;

async function runBffRefresh(): Promise<AuthUser | null> {
  try {
    const res = await fetch("/api/auth/refresh", {
      method: "POST",
      credentials: "include",
      headers: { Accept: "application/json" },
    });
    if (!res.ok) {
      return null;
    }
    const data = (await res.json()) as { user?: AuthUser };
    if (!data.user?.id) {
      return null;
    }
    persistUser(data.user);
    return data.user;
  } catch {
    return null;
  }
}

export function getOrCreateRefreshPromise(): Promise<AuthUser | null> {
  if (refreshLock) {
    return refreshLock;
  }
  const p = runBffRefresh()
    .catch((): null => null)
    .finally(() => {
      refreshLock = null;
    });
  refreshLock = p;
  return p;
}

export async function fetchSessionFromBff(): Promise<AuthUser | null> {
  try {
    const res = await fetch("/api/auth/session", {
      method: "GET",
      credentials: "include",
      headers: { Accept: "application/json" },
    });
    if (!res.ok) {
      return null;
    }
    const data = (await res.json()) as { user?: AuthUser };
    if (!data.user?.id) {
      return null;
    }
    persistUser(data.user);
    return data.user;
  } catch {
    return null;
  }
}

export async function clearSession(): Promise<void> {
  try {
    await fetch("/api/auth/logout", {
      method: "POST",
      credentials: "include",
    });
  } catch {
    /* ignore */
  }
  persistUser(null);
}
