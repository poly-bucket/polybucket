import { describe, it, expect } from "vitest";
import {
  svgToDataUrl,
  resolvedImageSrcFromAvatarField,
  splitAvatarForDisplay,
} from "./minidenticon";

describe("svgToDataUrl", () => {
  it("encodes svg for use in img src", () => {
    const s = '<svg xmlns="http://www.w3.org/2000/svg"><rect/></svg>';
    const u = svgToDataUrl(s);
    expect(u.startsWith("data:image/svg+xml,")).toBe(true);
    expect(decodeURIComponent(u.replace(/^data:image\/svg\+xml,/, ""))).toBe(s);
  });
});

describe("resolvedImageSrcFromAvatarField", () => {
  it("returns null for empty", () => {
    expect(resolvedImageSrcFromAvatarField(null)).toBeNull();
    expect(resolvedImageSrcFromAvatarField("")).toBeNull();
  });

  it("passes through http(s) URLs", () => {
    expect(resolvedImageSrcFromAvatarField("https://x.test/a.png")).toBe(
      "https://x.test/a.png"
    );
  });

  it("passes through data URLs", () => {
    const d = "data:image/svg+xml;base64,PHN2Zy8+";
    expect(resolvedImageSrcFromAvatarField(d)).toBe(d);
  });

  it("wraps raw svg markup", () => {
    const s = "<svg xmlns=\"http://www.w3.org/2000/svg\"></svg>";
    const r = resolvedImageSrcFromAvatarField(s);
    expect(r?.startsWith("data:image/svg+xml,")).toBe(true);
  });
});

describe("splitAvatarForDisplay", () => {
  it("splits http as profile picture", () => {
    const u = "https://cdn.test/photo.png";
    expect(splitAvatarForDisplay(u)).toEqual({ profilePictureUrl: u });
  });

  it("splits other as stored avatar", () => {
    const s = "<svg></svg>";
    expect(splitAvatarForDisplay(s)).toEqual({ storedAvatar: s });
  });
});
