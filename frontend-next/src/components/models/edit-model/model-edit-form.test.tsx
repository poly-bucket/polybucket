import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { toast } from "sonner";
import { LicenseTypes, PrivacySettings, type Model } from "@/lib/api/client";
import { ModelEditForm } from "./model-edit-form";

const {
  updateModel,
  addTag,
  removeTag,
  addCategory,
  removeCategory,
  getCategories,
  getModelById,
} = vi.hoisted(() => ({
  updateModel: vi.fn(),
  addTag: vi.fn(),
  removeTag: vi.fn(),
  addCategory: vi.fn(),
  removeCategory: vi.fn(),
  getCategories: vi.fn(),
  getModelById: vi.fn(),
}));

vi.mock("sonner", () => ({
  toast: {
    error: vi.fn(),
    success: vi.fn(),
  },
}));

vi.mock("@/lib/api/clientFactory", () => ({
  ApiClientFactory: {
    getApiClient: () => ({
      updateModel_UpdateModel: updateModel,
      addTagToModel_AddTagToModel: addTag,
      removeTagFromModel_RemoveTagFromModel: removeTag,
      addCategoryToModel_AddCategoryToModel: addCategory,
      removeCategoryFromModel_RemoveCategoryFromModel: removeCategory,
      getCategories_GetCategories: getCategories,
      getModelById_GetModel: getModelById,
    }),
  },
}));

const model = {
  id: "model-1",
  name: "Widget",
  description: "A widget",
  privacy: PrivacySettings.Public,
  license: LicenseTypes.MIT,
  aiGenerated: false,
  wip: false,
  nsfw: false,
  isRemix: false,
  remixUrl: "",
  categories: [{ id: "cat-art", name: "Art" }],
  tags: [{ id: "tag-old", name: "old" }],
} as Model;

describe("ModelEditForm", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    updateModel.mockResolvedValue({ model });
    addTag.mockResolvedValue(null);
    removeTag.mockResolvedValue(null);
    removeCategory.mockResolvedValue(null);
    addCategory.mockRejectedValue(new Error("category failed"));
    getCategories.mockResolvedValue({
      categories: [
        { id: "cat-art", name: "Art" },
        { id: "cat-toys", name: "Toys" },
      ],
    });
    getModelById.mockResolvedValue({ model });
  });

  it("Arrange: tag and category changes; Act: submit; Assert: client calls and category failure toast", async () => {
    const user = userEvent.setup();
    const onSave = vi.fn();

    render(
      <ModelEditForm model={model} onSave={onSave} onCancel={vi.fn()} />
    );

    await user.click(screen.getByRole("button", { name: "Toys" }));
    await user.click(screen.getByRole("button", { name: "Art" }));
    await user.type(screen.getByPlaceholderText("Add a tag..."), "newtag");
    await user.click(screen.getByRole("button", { name: "Add" }));
    await user.click(screen.getByRole("button", { name: "Remove tag old" }));
    await user.click(screen.getByRole("button", { name: "Save Changes" }));

    await waitFor(() => {
      expect(addTag).toHaveBeenCalledWith("model-1", "newtag");
    });

    expect(removeTag).toHaveBeenCalledWith("model-1", "tag-old");
    expect(addCategory).toHaveBeenCalledWith("model-1", "cat-toys");
    expect(removeCategory).toHaveBeenCalledWith("model-1", "cat-art");
    expect(toast.error).toHaveBeenCalledWith("Some category updates failed (1)");
    expect(getModelById).toHaveBeenCalledWith("model-1");
    expect(onSave).toHaveBeenCalled();
  });
});
