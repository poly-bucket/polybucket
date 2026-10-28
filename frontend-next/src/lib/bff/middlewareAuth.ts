import { hasAnyRole } from "@/lib/security/roles";

const ADMIN_REQUIRED = ["Admin"];
const MODERATION_REQUIRED = ["Admin", "Moderator"];

export function requiredRolesForPath(pathname: string): string[] | null {
  if (pathname === "/admin" || pathname.startsWith("/admin/")) {
    return ADMIN_REQUIRED;
  }
  if (pathname === "/moderation" || pathname.startsWith("/moderation/")) {
    return MODERATION_REQUIRED;
  }
  return null;
}

export function pathAllowedForRoles(pathname: string, userRoles: string[] | undefined): boolean {
  const required = requiredRolesForPath(pathname);
  if (!required) {
    return true;
  }
  return hasAnyRole(userRoles, required);
}
