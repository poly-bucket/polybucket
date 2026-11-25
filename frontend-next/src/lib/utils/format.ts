export interface FormatDateOptions {
  timeZone?: string;
  includeTime?: boolean;
}

export function formatDate(
  date?: Date | string | null,
  options?: FormatDateOptions
): string {
  if (!date) return "Never";
  const value = new Date(date);
  if (Number.isNaN(value.getTime())) return "Never";

  const timeZone = options?.timeZone ?? "UTC";
  try {
    return new Intl.DateTimeFormat(undefined, {
      timeZone,
      year: "numeric",
      month: "short",
      day: "numeric",
      ...(options?.includeTime
        ? { hour: "numeric", minute: "2-digit" }
        : {}),
    }).format(value);
  } catch {
    return value.toLocaleDateString();
  }
}