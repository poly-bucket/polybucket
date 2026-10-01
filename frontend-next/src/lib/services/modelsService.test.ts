import { describe, expect, it, vi, beforeEach } from "vitest";
import { linkModelCategories } from "./modelsService";
import { ApiClientFactory } from "@/lib/api/clientFactory";

vi.mock("@/lib/api/clientFactory", () => ({
  ApiClientFactory: {
    getApiClient: vi.fn(),
  },
}));

describe("linkModelCategories", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("maps category names to ids and links them", async () => {
    const addCategory = vi.fn().mockResolvedValue(undefined);
    const getCategories = vi.fn().mockResolvedValue({
      categories: [
        { id: "cat-1", name: "Tools" },
        { id: "cat-2", name: "Art" },
      ],
    });

    vi.mocked(ApiClientFactory.getApiClient).mockReturnValue({
      getCategories_GetCategories: getCategories,
      addCategoryToModel_AddCategoryToModel: addCategory,
    } as unknown as ReturnType<typeof ApiClientFactory.getApiClient>);

    const result = await linkModelCategories("model-1", ["Tools", "Unknown"]);

    expect(getCategories).toHaveBeenCalledWith(1, 100, null);
    expect(addCategory).toHaveBeenCalledTimes(1);
    expect(addCategory).toHaveBeenCalledWith("model-1", "cat-1");
    expect(result.skipped).toBe(1);
    expect(result.failed).toBe(0);
  });
});
