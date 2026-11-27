export function getModelFileLeafName(fileName: string): string {
  const normalized = fileName.replace(/\\/g, "/").replace(/\/+$/, "");
  const lastSlash = normalized.lastIndexOf("/");
  return lastSlash < 0 ? normalized : normalized.slice(lastSlash + 1);
}