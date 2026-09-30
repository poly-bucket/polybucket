import { describe, it, expect } from "vitest";
import {
  applyDownloadCountToModel,
  parseDownloadCountFromHeaders,
} from "./modelDownloadsService";

describe("modelDownloadsService", () => {
  it("parses download count headers case-insensitively", () => {
    const result = parseDownloadCountFromHeaders({
      "X-Model-Downloads": "12",
      "x-model-download-counted": "true",
    });

    expect(result).toEqual({ downloads: 12, counted: true });
  });

  it("applies counted downloads to the model", () => {
    const updated = applyDownloadCountToModel(
      { downloads: 1, name: "m" },
      { "x-model-downloads": "5", "x-model-download-counted": "true" }
    );

    expect(updated.downloads).toBe(5);
  });

  it("does not change the model when counted is false", () => {
    const model = { downloads: 1 };
    const updated = applyDownloadCountToModel(model, {
      "x-model-downloads": "5",
      "x-model-download-counted": "false",
    });

    expect(updated).toEqual(model);
  });
});
