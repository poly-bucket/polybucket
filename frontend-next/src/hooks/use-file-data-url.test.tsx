import { describe, expect, it } from "vitest";
import { renderHook, waitFor } from "@testing-library/react";
import { useFileDataUrl } from "./use-file-data-url";

describe("useFileDataUrl", () => {
  it("returns a data URL for a PNG file", async () => {
    const bytes = new Uint8Array([0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a]);
    const file = new File([bytes], "thumb.png", { type: "image/png" });

    const { result } = renderHook(() => useFileDataUrl(file));

    await waitFor(() => {
      expect(result.current).toMatch(/^data:image\/png;base64,/);
    });
  });

  it("returns null when file is absent", () => {
    const { result } = renderHook(() => useFileDataUrl(null));

    expect(result.current).toBeNull();
  });
});