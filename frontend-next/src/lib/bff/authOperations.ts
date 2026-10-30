import type { NextRequest } from "next/server";
import { NextResponse } from "next/server";
import {
  apiGetJson,
  apiPostJson,
  type ApiLoginResponse,
  type ApiRefreshResponse,
  type ApiRegisterResponse,
} from "./apiClient";
import { appendSessionCookie, readSessionCookieValue, refreshMaxAgeFromIso } from "./cookies";
import {
  accessTokenExpMs,
  unsealSession,
  type SealedSessionPayload,
} from "./session";
import { rolesFromRoleString } from "./userProfile";
import {
  profileFromAccessToken,
  profileFromAuthentication,
  profileFromLoginResponse,
  toClientJson,
  type BffUserProfile,
} from "./userProfile";

export async function sealTokensInResponse(
  response: NextResponse,
  accessToken: string,
  refreshToken: string,
  refreshExpiresAt?: string,
  roles: string[] = []
): Promise<SealedSessionPayload> {
  const payload: SealedSessionPayload = {
    accessToken,
    refreshToken,
    accessExp: accessTokenExpMs(accessToken),
    roles,
  };
  const maxAge = refreshMaxAgeFromIso(refreshExpiresAt);
  await appendSessionCookie(response, payload, maxAge);
  return payload;
}

export async function readSealedSession(request: NextRequest): Promise<SealedSessionPayload | null> {
  const raw = readSessionCookieValue(request);
  if (!raw) {
    return null;
  }
  return unsealSession(raw);
}

export async function refreshSessionPayload(
  refreshToken: string
): Promise<
  | { ok: true; payload: SealedSessionPayload; profile: BffUserProfile; refreshExpiresAt?: string }
  | { ok: false; status: number }
> {
  const result = await apiPostJson<ApiRefreshResponse>("/api/auth/refresh-token", {
    refreshToken,
  });
  if (!result.ok || !result.data?.authentication) {
    return { ok: false, status: result.status || 401 };
  }
  const auth = result.data.authentication;
  const accessToken = auth.accessToken ?? "";
  const newRefresh = auth.refreshToken ?? "";
  if (!accessToken.trim() || !newRefresh.trim()) {
    return { ok: false, status: 401 };
  }
  const profile = profileFromAuthentication(auth);
  if (!profile) {
    return { ok: false, status: 401 };
  }
  const hydrated = await hydrateProfileFromMe(accessToken);
  const roles = hydrated?.roles ?? profile.roles ?? [];
  const payload: SealedSessionPayload = {
    accessToken,
    refreshToken: newRefresh,
    accessExp: accessTokenExpMs(accessToken),
    roles,
  };
  return {
    ok: true,
    payload,
    profile: hydrated ?? profile,
    refreshExpiresAt: auth.refreshTokenExpiresAt,
  };
}

export async function handleLoginBody(
  body: Record<string, unknown>
): Promise<
  | {
      kind: "twoFactor";
      response: NextResponse;
    }
  | {
      kind: "success";
      response: NextResponse;
    }
  | {
      kind: "error";
      response: NextResponse;
    }
> {
  const result = await apiPostJson<ApiLoginResponse>("/api/auth/login", body);
  if (!result.ok || !result.data) {
    return {
      kind: "error",
      response: NextResponse.json(
        result.data ?? { message: result.text || "Login failed" },
        { status: result.status || 401 }
      ),
    };
  }
  const login = result.data;
  if (login.requiresTwoFactor) {
    return {
      kind: "twoFactor",
      response: NextResponse.json({
        requiresTwoFactor: true,
        twoFactorToken: login.twoFactorToken ?? null,
      }),
    };
  }
  const accessToken = login.token ?? "";
  const refreshToken = login.refreshToken ?? "";
  if (!accessToken.trim() || !refreshToken.trim()) {
    return {
      kind: "error",
      response: NextResponse.json({ message: "Invalid login response" }, { status: 502 }),
    };
  }
  const profile = profileFromLoginResponse(login, accessToken);
  if (!profile) {
    return {
      kind: "error",
      response: NextResponse.json({ message: "Invalid token" }, { status: 502 }),
    };
  }
  const hydrated = await hydrateProfileFromMe(accessToken);
  const effectiveProfile = hydrated ?? profile;
  const json = NextResponse.json({
    user: toClientJson(effectiveProfile).user,
    requiresFirstTimeSetup: login.requiresFirstTimeSetup ?? false,
    setupStep: login.setupStep ?? null,
    isAdmin: effectiveProfile.roles?.some((r) => r.toLowerCase() === "admin") ?? false,
  });
  await sealTokensInResponse(
    json,
    accessToken,
    refreshToken,
    login.refreshTokenExpiresAt,
    effectiveProfile.roles ?? []
  );
  return { kind: "success", response: json };
}

export async function handleRegisterBody(
  body: Record<string, unknown>
): Promise<
  | { kind: "verification"; response: NextResponse }
  | { kind: "success"; response: NextResponse }
  | { kind: "error"; response: NextResponse }
> {
  const result = await apiPostJson<ApiRegisterResponse>("/api/auth/register", {
    email: body.email,
    username: body.username,
    password: body.password,
    confirmPassword: body.confirmPassword,
    userAgent: body.userAgent ?? "",
  });
  if (!result.ok || result.data === null) {
    let message = result.text || "Registration failed";
    if (result.data && typeof result.data === "object" && "message" in result.data) {
      const m = (result.data as { message?: string }).message;
      if (m) {
        message = m;
      }
    }
    return {
      kind: "error",
      response: NextResponse.json({ message }, { status: result.status || 400 }),
    };
  }
  const register = result.data;
  if (register.requiresEmailVerification) {
    return {
      kind: "verification",
      response: NextResponse.json({ requiresEmailVerification: true }),
    };
  }
  const auth = register.authentication;
  const accessToken = auth?.accessToken ?? "";
  const refreshToken = auth?.refreshToken ?? "";
  if (!accessToken.trim() || !refreshToken.trim()) {
    return {
      kind: "error",
      response: NextResponse.json({ message: "Invalid registration response" }, { status: 502 }),
    };
  }
  const profile = profileFromAuthentication(auth!);
  if (!profile) {
    return {
      kind: "error",
      response: NextResponse.json({ message: "Invalid token" }, { status: 502 }),
    };
  }
  const hydrated = await hydrateProfileFromMe(accessToken);
  const effectiveProfile = hydrated ?? profile;
  const json = NextResponse.json({
    user: toClientJson(effectiveProfile).user,
  });
  await sealTokensInResponse(
    json,
    accessToken,
    refreshToken,
    auth?.refreshTokenExpiresAt,
    effectiveProfile.roles ?? []
  );
  return { kind: "success", response: json };
}

export async function hydrateProfileFromMe(accessToken: string): Promise<BffUserProfile | null> {
  const me = await apiGetJson<{
    id?: string;
    email?: string;
    username?: string;
    role?: string;
    isEmailVerified?: boolean;
    avatar?: string;
    profilePictureUrl?: string;
  }>("/api/auth/me", accessToken);
  if (!me.ok || !me.data?.id) {
    return profileFromAccessTokenOnly(accessToken);
  }
  const base = profileFromAccessTokenOnly(accessToken);
  if (!base) {
    return null;
  }
  return {
    ...base,
    roles: me.data.role ? rolesFromRoleString(me.data.role) : base.roles,
    avatar: me.data.avatar ?? base.avatar,
    profilePictureUrl: me.data.profilePictureUrl ?? base.profilePictureUrl,
    isEmailVerified: me.data.isEmailVerified ?? base.isEmailVerified,
  };
}

function profileFromAccessTokenOnly(accessToken: string): BffUserProfile | null {
  return profileFromAccessToken(accessToken);
}

export async function ensureFreshAccessToken(
  session: SealedSessionPayload
): Promise<
  | { ok: true; session: SealedSessionPayload; rotated: boolean; refreshExpiresAt?: string }
  | { ok: false }
> {
  const skewMs = 60_000;
  if (session.accessExp - Date.now() > skewMs) {
    return { ok: true, session, rotated: false };
  }
  const refreshed = await refreshSessionPayload(session.refreshToken);
  if (!refreshed.ok) {
    return { ok: false };
  }
  return {
    ok: true,
    session: refreshed.payload,
    rotated: true,
    refreshExpiresAt: refreshed.refreshExpiresAt,
  };
}
