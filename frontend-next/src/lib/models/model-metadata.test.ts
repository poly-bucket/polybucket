import { describe, expect, it } from "vitest";
import { LicenseTypes, PrivacySettings } from "@/lib/api/client";
import { modelMetadataSchema } from "./model-metadata";

describe("modelMetadataSchema", () => {
  it("requires a title", () => {
    const result = modelMetadataSchema.safeParse({
      name: "",
      description: "",
      privacy: PrivacySettings.Public,
      license: LicenseTypes.MIT,
      aiGenerated: false,
      wip: false,
      nsfw: false,
      isRemix: false,
      remixUrl: "",
    });
    expect(result.success).toBe(false);
  });

  it("requires remix URL when isRemix is true", () => {
    const result = modelMetadataSchema.safeParse({
      name: "Test Model",
      description: "",
      privacy: PrivacySettings.Public,
      license: LicenseTypes.MIT,
      aiGenerated: false,
      wip: false,
      nsfw: false,
      isRemix: true,
      remixUrl: "",
    });
    expect(result.success).toBe(false);
  });

  it("accepts valid metadata", () => {
    const result = modelMetadataSchema.safeParse({
      name: "Test Model",
      description: "Hello",
      privacy: PrivacySettings.Public,
      license: LicenseTypes.MIT,
      aiGenerated: false,
      wip: false,
      nsfw: false,
      isRemix: true,
      remixUrl: "https://example.com/model/1",
    });
    expect(result.success).toBe(true);
  });
});
