import { describe, it, expect } from "vitest";
import {
  buildGeneratedUserAvatarSavePayload,
  previewUserAvatarSvg,
} from "./minidenticon";

describe("buildGeneratedUserAvatarSavePayload", () => {
  const userId = "11111111-1111-1111-1111-111111111111";

  it("Arrange: salt and user; Act: build; Assert: avatar matches preview svg", () => {
    const payload = buildGeneratedUserAvatarSavePayload(userId, "TestSeed1");
    expect(payload.salt).toBe("TestSeed1");
    expect(payload.avatar).toBe(previewUserAvatarSvg(userId, "TestSeed1"));
    expect(payload.avatar.trimStart().toLowerCase().startsWith("<svg")).toBe(true);
    expect(payload.avatar).toContain('viewBox="-1.5 -1.5 8 8"');
  });

  it("Arrange: blank salt; Act: build; Assert: omits salt but still sends preview svg", () => {
    const payload = buildGeneratedUserAvatarSavePayload(userId, "   ");
    expect(payload.salt).toBeUndefined();
    expect(payload.avatar).toBe(previewUserAvatarSvg(userId, undefined));
  });
});