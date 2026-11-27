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

  it("locks review until files and metadata are ready", () => {
    expect(canAdvanceFromStep("compose", baseCtx)).toBe(false);
    expect(isStepReachable("review", baseCtx)).toBe(false);

    const withFilesOnly = { ...baseCtx, fileCount: 2 };
    expect(canAdvanceFromStep("compose", withFilesOnly)).toBe(false);
    expect(isStepReachable("review", withFilesOnly)).toBe(false);

    const ready = { ...withFilesOnly, metadataValid: true };
    expect(canAdvanceFromStep("compose", ready)).toBe(true);
    expect(isStepReachable("review", ready)).toBe(true);
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
