export function formatDate(date?: Date | string | null): string {
  if (!date) return "Never";
  return new Date(date).toLocaleDateString();
}
