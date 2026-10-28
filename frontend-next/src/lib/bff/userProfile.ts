import { extractUserFromJWT } from "@/lib/utils/jwtUtils";
import type { AuthUser } from "@/lib/auth/authSession";
import type { ApiAuthenticationResponse, ApiLoginResponse } from "./apiClient";
import { accessTokenExpMs } from "./session";

export type BffUserProfile = AuthUser;

export function rolesFromRoleString(role?: string | null): string[] {
  if (!role?.trim()) {
    return [];
  }
  return [role.trim()];
}

export function profileFromAccessToken(
  accessToken: string,
  extras?: Partial<AuthUser>
): BffUserProfile | null {
  const decoded = extractUserFromJWT(accessToken);
  if (!decoded) {
    return null;
  }
  return {
    id: decoded.id,
    email: decoded.email,
    username: decoded.username,
    roles: decoded.role ? [decoded.role] : [],
    isEmailVerified: decoded.isEmailVerified,
    accessExpiresAt: accessTokenExpMs(accessToken),
    ...extras,
  };
}

export function profileFromLoginResponse(
  login: ApiLoginResponse,
  accessToken: string
): BffUserProfile | null {
  return profileFromAccessToken(accessToken, {
    requiresFirstTimeSetup: login.requiresFirstTimeSetup,
    setupStep: login.setupStep ?? undefined,
  });
}

export function profileFromAuthentication(
  auth: ApiAuthenticationResponse
): BffUserProfile | null {
  const at = auth.accessToken;
  if (!at?.trim()) {
    return null;
  }
  const fromJwt = profileFromAccessToken(at);
  if (!fromJwt) {
    const u = auth.user;
    if (u?.id && u.email && u.username) {
      return {
        id: u.id,
        email: u.email,
        username: u.username,
        roles: u.role ? [u.role] : [],
        isEmailVerified: u.isEmailVerified,
        avatar: u.avatar,
        accessExpiresAt: accessTokenExpMs(at),
      };
    }
    return null;
  }
  if (auth.user?.avatar) {
    fromJwt.avatar = auth.user.avatar;
  }
  return fromJwt;
}

export function toClientJson(user: BffUserProfile) {
  const { accessExpiresAt, ...rest } = user;
  return { user: { ...rest, accessExpiresAt } };
}
