"use client";

import React, { createContext, useCallback, useContext, useEffect, useState } from "react";
import {
  getOrCreateRefreshPromise,
  getStoredUser,
  isSessionRestorable,
  type AuthUser,
  persistUser,
  subscribeAuthSession,
} from "@/lib/auth/authSession";
import { ApiClientFactory } from "@/lib/api/clientFactory";
import { isAxiosError } from "axios";
import { decodeJWT, extractUserFromJWT, isTokenExpired } from "@/lib/utils/jwtUtils";
import { LoginCommand, type LoginCommandResponse } from "@/lib/api/client";

export type { AuthUser } from "@/lib/auth/authSession";

interface AuthContextValue {
  user: AuthUser | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  login: (emailOrUsername: string, password: string) => Promise<LoginResult>;
  logout: () => void;
  /** Re-fetch /api/auth/me and merge avatar + profilePictureUrl into the session user. */
  refreshUserFromMe: () => Promise<void>;
}

export interface LoginResult {
  success: boolean;
  requiresTwoFactor?: boolean;
  requiresFirstTimeSetup?: boolean;
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
    const u = getStoredUser();
    if (!u?.accessToken?.trim()) {
      return;
    }
    try {
      const me = await ApiClientFactory.getApiClient().me_GetCurrentUser();
      const cur = getStoredUser();
      if (!cur?.accessToken) {
        return;
      }
      const next: AuthUser = {
        ...cur,
        avatar: me.avatar ?? cur.avatar,
        profilePictureUrl: me.profilePictureUrl ?? cur.profilePictureUrl,
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
    let s = getStoredUser();
    if (s && !isSessionRestorable(s)) {
      persistUser(null);
      s = getStoredUser();
    }
    setUser(s);
    setIsLoading(false);
    return unsubscribe;
  }, []);

  useEffect(() => {
    if (isLoading) {
      return;
    }
    if (!user) {
      return;
    }
    if (!user.accessToken?.trim()) {
      return;
    }
    if (isTokenExpired(user.accessToken) && user.refreshToken) {
      void getOrCreateRefreshPromise();
    }
  }, [isLoading, user]);

  useEffect(() => {
    if (isLoading) {
      return;
    }
    if (!user?.accessToken) {
      return;
    }
    if (!user.refreshToken) {
      return;
    }
    const payload = decodeJWT(user.accessToken);
    if (!payload?.exp) {
      return;
    }
    const expMs = payload.exp * 1000;
    const runAt = expMs - 60_000;
    const delay = runAt - Date.now();
    if (delay < 0) {
      return;
    }
    const t = setTimeout(() => {
      if (getStoredUser()?.accessToken) {
        void getOrCreateRefreshPromise().then(() => setUser(getStoredUser()));
      }
    }, delay);
    return () => clearTimeout(t);
  }, [isLoading, user?.accessToken, user?.refreshToken]);

  useEffect(() => {
    if (typeof document === "undefined" || isLoading) {
      return;
    }
    if (!user?.accessToken) {
      return;
    }
    if (!user.refreshToken) {
      return;
    }
    const onVisible = () => {
      if (document.visibilityState !== "visible") {
        return;
      }
      const s = getStoredUser();
      if (!s?.accessToken) {
        return;
      }
      if (isTokenExpired(s.accessToken)) {
        void getOrCreateRefreshPromise().then((ok) => {
          if (ok) {
            setUser(getStoredUser());
          }
        });
      }
    };
    document.addEventListener("visibilitychange", onVisible);
    return () => document.removeEventListener("visibilitychange", onVisible);
  }, [isLoading, user?.accessToken, user?.refreshToken]);

  useEffect(() => {
    if (isLoading || !user?.accessToken) {
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
        if (!cur?.accessToken) {
          return;
        }
        const next: AuthUser = {
          ...cur,
          avatar: me.avatar ?? cur.avatar,
          profilePictureUrl: me.profilePictureUrl ?? cur.profilePictureUrl,
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
  }, [isLoading, user?.accessToken]);

  const login = useCallback(async (emailOrUsername: string, password: string): Promise<LoginResult> => {
    const apiClient = ApiClientFactory.getApiClient();
    const command = new LoginCommand({
      emailOrUsername,
      email: emailOrUsername,
      password,
      userAgent: typeof navigator !== "undefined" ? navigator.userAgent : "",
    });

    try {
      const response: LoginCommandResponse = await apiClient.login_Login(command);

      if (response.requiresTwoFactor) {
        return {
          success: false,
          requiresTwoFactor: true,
          error: "Two-factor authentication is required. This flow is not yet implemented.",
        };
      }

      if (response.token) {
        const decoded = extractUserFromJWT(response.token);
        if (!decoded) {
          return { success: false, error: "Invalid authentication token received" };
        }

        const authUser: AuthUser = {
          id: decoded.id,
          email: decoded.email,
          username: decoded.username,
          accessToken: response.token,
          refreshToken: response.refreshToken,
          roles: decoded.role ? [decoded.role] : [],
          requiresFirstTimeSetup: response.requiresFirstTimeSetup,
          setupStep: response.setupStep,
        };

        persistUser(authUser);

        return {
          success: true,
          requiresFirstTimeSetup: response.requiresFirstTimeSetup,
          setupStep: response.setupStep,
        };
      }

      return { success: false, error: "Invalid response format" };
    } catch (err: unknown) {
      const error = err as { result?: { message?: string; detail?: string }; message?: string };
      const message =
        error.result?.message ?? error.result?.detail ?? error.message ?? "An unexpected error occurred";
      return { success: false, error: message };
    }
  }, []);

  const logout = useCallback(() => {
    persistUser(null);
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
