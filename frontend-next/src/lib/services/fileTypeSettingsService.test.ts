import { describe, it, expect } from "vitest";
import {
  partitionUploadFilesByAllowance,
  type FileTypeSettingsData,
} from "./fileTypeSettingsService";

const stlType: FileTypeSettingsData = {
  fileExtension: ".stl",
  enabled: true,
  maxFileSizeBytes: 100 * 1024 * 1024,
  category: "3D",
};

function file(name: string, size = 100): File {
  return new File([new Uint8Array(size)], name);
}

describe("partitionUploadFilesByAllowance", () => {
  it("keeps allowed files and lists rejected names", () => {
    const { allowed, rejected } = partitionUploadFilesByAllowance(
      [stlType],
      [file("part.stl"), file("drawing.dxf"), file("readme.txt")]
    );

    expect(allowed.map((f) => f.name)).toEqual(["part.stl"]);
    expect(rejected).toEqual(["drawing.dxf", "readme.txt"]);
  });
});