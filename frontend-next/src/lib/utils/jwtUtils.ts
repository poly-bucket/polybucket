export interface JWTPayload {
  sub: string;
  email: string;
  name: string;
  role: string;
  email_verified: string;
  exp: number;
  iat: number;
  iss: string;
  aud: string;
}

export interface DecodedUser {
  id: string;
  email: string;
  username: string;
  role: string;
  isEmailVerified: boolean;
}

export const decodeJWT = (token: string): JWTPayload | null => {
  try {
    const parts = token.split(".");
    if (parts.length !== 3) {
      return null;
    }

    const payload = parts[1];
    const paddedPayload = payload + "=".repeat((4 - (payload.length % 4)) % 4);
    const decodedPayload = atob(paddedPayload.replace(/-/g, "+").replace(/_/g, "/"));

    return JSON.parse(decodedPayload) as JWTPayload;
  } catch {
    return null;
  }
};

export const extractUserFromJWT = (token: string): DecodedUser | null => {
  const payload = decodeJWT(token);
  if (!payload) {
    return null;
  }

  return {
    id: payload.sub,
    email: payload.email,
    username: payload.name,
    role: payload.role,
    isEmailVerified: payload.email_verified === "true",
  };
};

export const isTokenExpired = (token: string): boolean => {
  const payload = decodeJWT(token);
  if (!payload) {
    return true;
  }

  const currentTime = Math.floor(Date.now() / 1000);
  return payload.exp < currentTime;
};
