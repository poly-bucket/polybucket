export function parseDownloadCountFromHeaders(
  headers?: Record<string, unknown>
): { downloads?: number; counted?: boolean } {
  if (!headers) {
    return {};
  }

  const normalized = Object.fromEntries(
    Object.entries(headers).map(([key, value]) => [key.toLowerCase(), value])
  );

  const downloadsRaw = normalized["x-model-downloads"];
  const countedRaw = normalized["x-model-download-counted"];

  const downloads =
    downloadsRaw != null && downloadsRaw !== ""
      ? Number.parseInt(String(downloadsRaw), 10)
      : undefined;

  const counted =
    countedRaw != null
      ? String(countedRaw).toLowerCase() === "true"
      : undefined;

  return {
    downloads: Number.isFinite(downloads) ? downloads : undefined,
    counted,
  };
}

export function applyDownloadCountToModel<T extends { downloads?: number }>(
  model: T,
  headers?: Record<string, unknown>
): T {
  const { downloads, counted } = parseDownloadCountFromHeaders(headers);
  if (!counted || downloads === undefined) {
    return model;
  }
  return { ...model, downloads };
}
