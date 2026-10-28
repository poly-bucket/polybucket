"use client";

import React, { createContext, useCallback, useContext, useEffect, useState } from "react";
import {
  clearSession,
  fetchSessionFromBff,
  getOrCreateRefreshPromise,
  getStoredUser,
  isSessionRestorable,
  purgeLegacyTokenStorage,
  type AuthUser,
  persistUser,
  subscribeAuthSession,
} from "@/lib/auth/authSession";
import { ApiClientFactory } from "@/lib/api/clientFactory";
import { isAxiosError } from "axios";

export type { AuthUser } from "@/lib/auth/authSession";

export interface LoginOptions {
  twoFactorToken?: string;
  backupCode?: string;
}

function getThrownLoginMessage(err: unknown): string {
  if (err && typeof err === "object") {
    const o = err as Record<string, unknown>;
    if (typeof o.message === "string" && o.message.trim()) {
      return o.message;
    }
    if (typeof o.detail === "string" && o.detail.trim()) {
      return o.detail;
    }
    if (typeof o.title === "string" && o.title.trim()) {
      return o.title;
    }
  }
  if (err instanceof Error && err.message) {
    return err.message;
  }
  return "An unexpected error occurred";
}

interface AuthContextValue {
  user: AuthUser | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  login: (
    emailOrUsername: string,
    password: string,
    options?: LoginOptions
  ) => Promise<LoginResult>;
  logout: () => Promise<void>;
  refreshUserFromMe: () => Promise<void>;
}

export interface LoginResult {
  success: boolean;
  requiresTwoFactor?: boolean;
  requiresFirstTimeSetup?: boolean;
  isAdmin?: boolean;
  setupStep?: string;
  error?: string;
}

export const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const refreshUserFromMe = useCallback(async () => {
    if (typeof window === "undefined") {
      return;
    }
    if (!getStoredUser()) {
      return;
    }
    try {
      const me = await ApiClientFactory.getApiClient().me_GetCurrentUser();
      const cur = getStoredUser();
      if (!cur) {
        return;
      }
      const next: AuthUser = {
        ...cur,
        avatar: me.avatar ?? cur.avatar,
        profilePictureUrl: me.profilePictureUrl ?? cur.profilePictureUrl,
        isEmailVerified: me.isEmailVerified ?? cur.isEmailVerified,
      };
      persistUser(next);
    } catch (err) {
      if (isAxiosError(err) && err.response?.status === 401) {
        setUser(getStoredUser());
      }
    }
  }, []);

  useEffect(() => {
    const unsubscribe = subscribeAuthSession(setUser);
    purgeLegacyTokenStorage();
    void (async () => {
      const sessionUser = await fetchSessionFromBff();
      setUser(sessionUser ?? getStoredUser());
      setIsLoading(false);
    })();
    return unsubscribe;
  }, []);

  useEffect(() => {
    if (isLoading || !user?.accessExpiresAt) {
      return;
    }
    const runAt = user.accessExpiresAt - 60_000;
    const delay = runAt - Date.now();
    if (delay < 0) {
      void getOrCreateRefreshPromise().then((ok) => {
        if (ok) {
          setUser(getStoredUser());
        }
      });
      return;
    }
    const t = setTimeout(() => {
      void getOrCreateRefreshPromise().then((ok) => {
        if (ok) {
          setUser(getStoredUser());
        }
      });
    }, delay);
    return () => clearTimeout(t);
  }, [isLoading, user?.accessExpiresAt]);

  useEffect(() => {
    if (typeof document === "undefined" || isLoading || !user) {
      return;
    }
    const onVisible = () => {
      if (document.visibilityState !== "visible") {
        return;
      }
      const exp = getStoredUser()?.accessExpiresAt;
      if (exp && exp - Date.now() < 60_000) {
        void getOrCreateRefreshPromise().then((ok) => {
          if (ok) {
            setUser(getStoredUser());
          }
        });
      }
    };
    document.addEventListener("visibilitychange", onVisible);
    return () => document.removeEventListener("visibilitychange", onVisible);
  }, [isLoading, user]);

  useEffect(() => {
    if (isLoading || !user) {
      return;
    }
    let active = true;
    void (async () => {
      try {
        const me = await ApiClientFactory.getApiClient().me_GetCurrentUser();
        if (!active) {
          return;
        }
        const cur = getStoredUser();
        if (!cur) {
          return;
        }
        const next: AuthUser = {
          ...cur,
          avatar: me.avatar ?? cur.avatar,
          profilePictureUrl: me.profilePictureUrl ?? cur.profilePictureUrl,
          isEmailVerified: me.isEmailVerified ?? cur.isEmailVerified,
        };
        persistUser(next);
      } catch (err) {
        if (isAxiosError(err) && err.response?.status === 401 && active) {
          setUser(getStoredUser());
        }
      }
    })();
    return () => {
      active = false;
    };
  }, [isLoading, user?.id]);

  const login = useCallback(
    async (
      emailOrUsername: string,
      password: string,
      options?: LoginOptions
    ): Promise<LoginResult> => {
      const trimmedBackup = options?.backupCode?.trim() ?? "";
      const trimmedTotp = options?.twoFactorToken?.trim() ?? "";
      const useBackup = trimmedBackup.length > 0;
      const body = {
        emailOrUsername,
        email: emailOrUsername,
        password,
        userAgent: typeof navigator !== "undefined" ? navigator.userAgent : "",
        twoFactorToken: useBackup ? undefined : trimmedTotp || undefined,
        backupCode: useBackup ? trimmedBackup : undefined,
      };

      try {
        const res = await fetch("/api/auth/login", {
          method: "POST",
          credentials: "include",
          headers: { "Content-Type": "application/json", Accept: "application/json" },
          body: JSON.stringify(body),
        });
        const data = (await res.json()) as {
          requiresTwoFactor?: boolean;
          user?: AuthUser;
          requiresFirstTimeSetup?: boolean;
          setupStep?: string;
          isAdmin?: boolean;
          message?: string;
        };

        if (!res.ok) {
          return { success: false, error: data.message ?? getThrownLoginMessage(data) };
        }

        if (data.requiresTwoFactor) {
          return { success: false, requiresTwoFactor: true };
        }

        if (data.user?.id) {
          persistUser(data.user);
          const isAdmin =
            data.isAdmin ??
            data.user.roles?.some((r) => r.toLowerCase() === "admin") ??
            false;
          return {
            success: true,
            requiresFirstTimeSetup: data.requiresFirstTimeSetup,
            isAdmin,
            setupStep: data.setupStep,
          };
        }

        return { success: false, error: "Invalid response format" };
      } catch (err: unknown) {
        return { success: false, error: getThrownLoginMessage(err) };
      }
    },
    []
  );

  const logout = useCallback(async () => {
    await clearSession();
  }, []);

  const value: AuthContextValue = {
    user,
    isLoading,
    isAuthenticated: isSessionRestorable(user),
    login,
    logout,
    refreshUserFromMe,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error("useAuth must be used within AuthProvider");
  }
  return ctx;
}
