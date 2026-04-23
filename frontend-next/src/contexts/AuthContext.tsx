"use client";

import React, { createContext, useCallback, useContext, useEffect, useState } from "react";
import { ApiClientFactory } from "@/lib/api/clientFactory";
import { extractUserFromJWT } from "@/lib/utils/jwtUtils";
import { LoginCommand, type LoginCommandResponse } from "@/lib/api/client";

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

const AUTH_STORAGE_KEY = "polybucket-auth";
const AUTH_SESSION_COOKIE = "polybucket-session";

function setSessionCookie(present: boolean): void {
  if (typeof document === "undefined") return;
  if (present) {
    document.cookie = `${AUTH_SESSION_COOKIE}=1; path=/; max-age=${60 * 60 * 24 * 7}; SameSite=Lax`;
  } else {
    document.cookie = `${AUTH_SESSION_COOKIE}=; path=/; max-age=0`;
  }
}

function loadStoredUser(): AuthUser | null {
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

function saveUser(user: AuthUser | null): void {
  if (typeof window === "undefined") return;
  if (user) {
    localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(user));
    setSessionCookie(true);
  } else {
    localStorage.removeItem(AUTH_STORAGE_KEY);
    setSessionCookie(false);
  }
}

export const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const refreshUserFromMe = useCallback(async () => {
    if (typeof window === "undefined") return;
    try {
      const stored = localStorage.getItem(AUTH_STORAGE_KEY);
      if (!stored) return;
      const parsed = JSON.parse(stored) as AuthUser;
      if (!parsed.accessToken?.trim()) return;

      const me = await ApiClientFactory.getApiClient().me_GetCurrentUser();
      setUser((prev) => {
        if (!prev?.accessToken) return prev;
        const next: AuthUser = {
          ...prev,
          avatar: me.avatar ?? undefined,
          profilePictureUrl: me.profilePictureUrl ?? undefined,
        };
        saveUser(next);
        return next;
      });
    } catch {
      // 401: optional logout; keep existing session behavior
    }
  }, []);

  useEffect(() => {
    const stored = loadStoredUser();
    setUser(stored);
    if (stored) setSessionCookie(true);
    setIsLoading(false);
  }, []);

  useEffect(() => {
    if (isLoading || !user?.accessToken) return;
    let active = true;
    void (async () => {
      try {
        const me = await ApiClientFactory.getApiClient().me_GetCurrentUser();
        if (!active) return;
        setUser((prev) => {
          if (!prev?.accessToken) return prev;
          const next: AuthUser = {
            ...prev,
            avatar: me.avatar ?? undefined,
            profilePictureUrl: me.profilePictureUrl ?? undefined,
          };
          saveUser(next);
          return next;
        });
      } catch {
        /* ignore */
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

        setUser(authUser);
        saveUser(authUser);

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
    setUser(null);
    saveUser(null);
  }, []);

  const value: AuthContextValue = {
    user,
    isLoading,
    isAuthenticated: Boolean(user?.accessToken),
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
