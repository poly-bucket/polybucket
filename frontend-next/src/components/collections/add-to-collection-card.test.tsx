import { describe, it, expect, vi, beforeEach } from "vitest";
import userEvent from "@testing-library/user-event";
import { render, screen, waitFor } from "@/test/test-utils";
import { AddToCollectionCard } from "./add-to-collection-card";

vi.mock("@/lib/services/collectionsService", () => ({
  collectionsService: {
    getUserCollections: vi.fn(),
    addModelToCollection: vi.fn(),
  },
}));

vi.mock("sonner", () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
}));

import { collectionsService } from "@/lib/services/collectionsService";
import { toast } from "sonner";

const mockModel = {
  id: "mod-1",
  name: "Test Model",
  description: "",
  author: { id: "u1", username: "alice" },
  likes: 0,
  downloads: 0,
  files: [],
  versions: [],
  tags: [],
  categories: [],
  comments: [],
} as never;

describe("AddToCollectionCard", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(collectionsService.getUserCollections).mockResolvedValue({
      collections: [
        { id: "c1", name: "Collection A" },
        { id: "c2", name: "Collection B" },
      ],
      totalCount: 2,
      page: 1,
      totalPages: 1,
    } as never);
  });

  it("renders Add to Collection button", () => {
    render(<AddToCollectionCard model={mockModel} isOwner={false} />);
    expect(screen.getByRole("button", { name: /add to collection/i })).toBeInTheDocument();
  });

  it("returns null when model has no id", () => {
    const { container } = render(
      <AddToCollectionCard model={{ ...(mockModel as Record<string, unknown>), id: undefined } as never} isOwner={false} />
    );
    expect(container.firstChild).toBeNull();
  });

  it("loads and lists collections when dropdown is opened", async () => {
    const user = userEvent.setup();
    render(<AddToCollectionCard model={mockModel} isOwner={false} />);

    await user.click(screen.getByRole("button", { name: /add to collection/i }));

    await waitFor(() => {
      expect(collectionsService.getUserCollections).toHaveBeenCalled();
    });
    expect(screen.getByRole("menuitem", { name: "Collection A" })).toBeInTheDocument();
    expect(screen.getByRole("menuitem", { name: "Collection B" })).toBeInTheDocument();
  });

  it("calls addModelToCollection and shows toast on selection", async () => {
    const user = userEvent.setup();
    vi.mocked(collectionsService.addModelToCollection).mockResolvedValue(undefined);

    render(<AddToCollectionCard model={mockModel} isOwner={false} />);
    await user.click(screen.getByRole("button", { name: /add to collection/i }));

    await waitFor(() => {
      expect(screen.getByRole("menuitem", { name: "Collection A" })).toBeInTheDocument();
    });
    await user.click(screen.getByRole("menuitem", { name: "Collection A" }));

    await waitFor(() => {
      expect(collectionsService.addModelToCollection).toHaveBeenCalledWith("c1", "mod-1");
      expect(toast.success).toHaveBeenCalledWith("Model added to collection");
    });
  });

  it("shows error toast when add fails", async () => {
    const user = userEvent.setup();
    vi.mocked(collectionsService.addModelToCollection).mockRejectedValue(new Error("Fail"));

    render(<AddToCollectionCard model={mockModel} isOwner={false} />);
    await user.click(screen.getByRole("button", { name: /add to collection/i }));

    await waitFor(() => {
      expect(screen.getByRole("menuitem", { name: "Collection A" })).toBeInTheDocument();
    });
    await user.click(screen.getByRole("menuitem", { name: "Collection A" }));

    await waitFor(() => {
      expect(toast.error).toHaveBeenCalledWith("Failed to add model to collection");
    });
  });

  it("shows no collections message when list is empty", async () => {
    vi.mocked(collectionsService.getUserCollections).mockResolvedValue({
      collections: [],
      totalCount: 0,
      page: 1,
      totalPages: 1,
    } as never);

    const user = userEvent.setup();
    render(<AddToCollectionCard model={mockModel} isOwner={false} />);
    await user.click(screen.getByRole("button", { name: /add to collection/i }));

    await waitFor(() => {
      expect(screen.getByText(/no collections/i)).toBeInTheDocument();
    });
  });
});
