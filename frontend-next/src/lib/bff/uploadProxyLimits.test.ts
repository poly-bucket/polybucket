import { describe, it, expect } from "vitest";
import nextConfig from "../../../next.config";

const DEFAULT_NEXT_PROXY_BODY_LIMIT_BYTES = 10 * 1024 * 1024;

function parseConfiguredBodySizeLimit(limit: string | number | undefined): number {
  if (limit === undefined) {
    return 0;
  }
  if (typeof limit === "number") {
    return limit;
  }
  const normalized = limit.trim().toLowerCase();
  const match = /^(\d+(?:\.\d+)?)\s*(b|kb|mb|gb)$/.exec(normalized);
  if (!match) {
    return 0;
  }
  const value = Number.parseFloat(match[1]);
  const unit = match[2];
  const multiplier =
    unit === "gb"
      ? 1024 ** 3
      : unit === "mb"
        ? 1024 ** 2
        : unit === "kb"
          ? 1024
          : 1;
  return Math.floor(value * multiplier);
}

describe("upload proxy body limits", () => {
  it("raises proxyClientMaxBodySize above Next.js default 10MB for model uploads", () => {
    const configured = parseConfiguredBodySizeLimit(
      nextConfig.experimental?.proxyClientMaxBodySize
    );

    expect(configured).toBeGreaterThan(DEFAULT_NEXT_PROXY_BODY_LIMIT_BYTES);
  });
});