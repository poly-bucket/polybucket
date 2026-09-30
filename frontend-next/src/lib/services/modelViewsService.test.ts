import { describe, it, expect, vi, beforeEach } from "vitest";
import axiosInstance from "@/lib/api/axiosConfig";
import { modelViewSessionKey, recordModelView } from "./modelViewsService";

vi.mock("@/lib/api/axiosConfig", () => ({
  default: {
    post: vi.fn(),
  },
}));

describe("modelViewsService", () => {
  beforeEach(() => {
    vi.mocked(axiosInstance.post).mockReset();
  });

  it("builds a stable session storage key per model", () => {
    expect(modelViewSessionKey("abc")).toBe("model-view:abc");
  });

  it("posts to the view endpoint and returns the response", async () => {
    vi.mocked(axiosInstance.post).mockResolvedValue({
      data: { views: 3, counted: true },
    });

    const result = await recordModelView("model-1");

    expect(axiosInstance.post).toHaveBeenCalledWith("/api/models/model-1/view");
    expect(result).toEqual({ views: 3, counted: true });
  });
});
