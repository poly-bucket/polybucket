// JWT utility functions for decoding and validating tokens

export interface JWTPayload {
  sub: string; // User ID
  email: string;
  name: string; // Username
  role: string;
  email_verified: string;
  exp: number; // Expiration time
  iat: number; // Issued at time
  iss: string; // Issuer
  aud: string; // Audience
}

export interface DecodedUser {
  id: string;
  email: string;
  username: string;
  role: string;
  isEmailVerified: boolean;
}

/**
 * Decodes a JWT token and returns the payload
 * @param token - The JWT token to decode
 * @returns The decoded payload or null if invalid
 */
export const decodeJWT = (token: string): JWTPayload | null => {
  try {
    // JWT tokens have 3 parts separated by dots
    const parts = token.split('.');
    if (parts.length !== 3) {
      console.warn('Invalid JWT format: token does not have 3 parts');
      return null;
    }

    // Decode the payload (second part)
    const payload = parts[1];
    // Add padding if needed for base64 decoding
    const paddedPayload = payload + '='.repeat((4 - payload.length % 4) % 4);
    const decodedPayload = atob(paddedPayload.replace(/-/g, '+').replace(/_/g, '/'));
    
    return JSON.parse(decodedPayload);
  } catch (error) {
    console.error('Failed to decode JWT:', error);
    return null;
  }
};

/**
 * Extracts user information from a JWT token
 * @param token - The JWT token to decode
 * @returns User information or null if invalid
 */
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
    isEmailVerified: payload.email_verified === 'true'
  };
};

/**
 * Checks if a JWT token is expired
 * @param token - The JWT token to check
 * @returns True if the token is expired, false otherwise
 */
export const isTokenExpired = (token: string): boolean => {
  const payload = decodeJWT(token);
  if (!payload) {
    return true; // Consider invalid tokens as expired
  }

  const currentTime = Math.floor(Date.now() / 1000);
  return payload.exp < currentTime;
};

/**
 * Gets the expiration time of a JWT token
 * @param token - The JWT token to check
 * @returns The expiration time as a Date object, or null if invalid
 */
export const getTokenExpiration = (token: string): Date | null => {
  const payload = decodeJWT(token);
  if (!payload) {
    return null;
  }

  return new Date(payload.exp * 1000);
};

/**
 * Checks if a JWT token will expire within a specified time
 * @param token - The JWT token to check
 * @param minutes - Number of minutes before expiration to consider as "expiring soon"
 * @returns True if the token expires within the specified time
 */
export const isTokenExpiringSoon = (token: string, minutes: number = 5): boolean => {
  const payload = decodeJWT(token);
  if (!payload) {
    return true;
  }

  const currentTime = Math.floor(Date.now() / 1000);
  const expirationThreshold = payload.exp - (minutes * 60);
  
  return currentTime >= expirationThreshold;
}; 