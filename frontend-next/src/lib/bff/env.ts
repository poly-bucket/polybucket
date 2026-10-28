function readEnv(name: string): string | undefined {
  const value = process.env[name];
  if (value === undefined || value.trim() === "") {
    return undefined;
  }
  return value.trim();
}

export function getPolybucketApiUrl(): string {
  const internal = readEnv("POLYBUCKET_API_URL");
  if (internal) {
    return internal.replace(/\/$/, "");
  }
  const publicUrl = readEnv("NEXT_PUBLIC_API_URL");
  if (publicUrl) {
    return publicUrl.replace(/\/$/, "");
  }
  return "http://localhost:11666";
}

export function getSessionSecret(): string {
  const secret = readEnv("SESSION_SECRET");
  if (secret && secret.length >= 32) {
    return secret;
  }
  if (process.env.NODE_ENV === "production") {
    throw new Error("SESSION_SECRET must be set to at least 32 characters in production");
  }
  return "development-session-secret-min-32-chars!!";
}

export function isProduction(): boolean {
  return process.env.NODE_ENV === "production";
}
