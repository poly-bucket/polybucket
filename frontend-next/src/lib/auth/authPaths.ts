export const AUTH_PATHS = [
  "/login",
  "/register",
  "/setup",
  "/verify-email",
  "/forgot-password",
  "/reset-password",
];

export function isAuthPath(pathname: string): boolean {
  return AUTH_PATHS.some((path) => pathname === path || pathname.startsWith(`${path}/`));
}
