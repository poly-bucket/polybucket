import { describe, expect, it } from "vitest";
import {
  canAdvanceFromStep,
  isStepReachable,
  suggestTitleFromFiles,
} from "./use-upload-wizard";

describe("useUploadWizard helpers", () => {
  const baseCtx = {
    fileCount: 0,
    settingsLoaded: true,
    metadataValid: false,
    isFormDirty: false,
  };

  it("locks details and review until files are present", () => {
    expect(canAdvanceFromStep("files", baseCtx)).toBe(false);
    expect(isStepReachable("details", baseCtx)).toBe(false);
    expect(isStepReachable("review", baseCtx)).toBe(false);

    const withFiles = { ...baseCtx, fileCount: 2 };
    expect(canAdvanceFromStep("files", withFiles)).toBe(true);
    expect(isStepReachable("details", withFiles)).toBe(true);
    expect(isStepReachable("review", withFiles)).toBe(false);
  });

  it("allows review when metadata is valid and files exist", () => {
    const ctx = { ...baseCtx, fileCount: 1, metadataValid: true };
    expect(isStepReachable("review", ctx)).toBe(true);
    expect(canAdvanceFromStep("review", ctx)).toBe(true);
  });

  it("suggests title from first 3d file name", () => {
    const getFileType = (name: string) => (name.endsWith(".stl") ? "3d" : "unknown");
    expect(suggestTitleFromFiles(["readme.txt", "cool-part.stl"], getFileType)).toBe(
      "cool part"
    );
  });
});
