export function normalizeRole(role: string | undefined | null): string {
  return (role ?? "").trim().toLowerCase();
}

export function hasAnyRole(
  userRoles: string[] | undefined,
  requiredRoles: string[] | undefined
): boolean {
  if (!requiredRoles?.length) return true;
  const normalized = new Set((userRoles ?? []).map(normalizeRole));
  return requiredRoles.some((r) => normalized.has(normalizeRole(r)));
}
