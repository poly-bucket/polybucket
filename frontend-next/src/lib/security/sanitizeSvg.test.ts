import { describe, it, expect } from "vitest";
import { sanitizeSvg } from "./sanitizeSvg";

describe("sanitizeSvg", () => {
  it("keeps simple svg", () => {
    const svg = '<svg xmlns="http://www.w3.org/2000/svg"><rect/></svg>';
    expect(sanitizeSvg(svg)).toBe(svg);
  });

  it("strips script blocks", () => {
    const raw = '<svg><script>alert(1)</script><rect/></svg>';
    const out = sanitizeSvg(raw);
    expect(out).not.toContain("script");
    expect(out).toContain("<rect");
  });

  it("rejects non-svg input", () => {
    expect(sanitizeSvg("<div>hi</div>")).toBeNull();
  });
});
