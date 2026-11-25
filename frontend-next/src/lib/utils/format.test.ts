import { describe, it, expect } from "vitest";
import { formatDate } from "./format";

describe("formatDate", () => {
  it("formats using the provided time zone", () => {
    const value = formatDate("2026-06-15T12:00:00.000Z", {
      timeZone: "America/New_York",
    });
    expect(value).toMatch(/Jun/);
    expect(value.length).toBeGreaterThan(4);
  });
});