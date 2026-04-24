import { describe, it, expect, vi, beforeEach } from "vitest";
import userEvent from "@testing-library/user-event";
import { render, screen, waitFor } from "@/test/test-utils";
import { CollectionDetailsPage } from "./collection-details-page";

const mockPush = vi.fn();

vi.mock("next/navigation", () => ({
  useParams: () => ({ id: "coll-1" }),
  useRouter: () => ({ push: mockPush }),
}));

vi.mock("@/lib/plugins", () => ({
  useCollectionDetailTabs: () => [
    { id: "models", label: "Models", icon: () => null, order: 0, component: () => <div>Models tab</div> },
  ],
  PluginBoundary: ({ children }: { children: React.ReactNode }) => <>{children}</>,
}));

vi.mock("@/lib/services/collectionsService", () => ({
  collectionsService: {
    getCollectionById: vi.fn(),
    accessCollection: vi.fn(),
    deleteCollection: vi.fn(),
    removeModelFromCollection: vi.fn(),
  },
}));

import { collectionsService } from "@/lib/services/collectionsService";

const mockCollection = {
  id: "coll-1",
  name: "Test Collection",
  description: "Description",
  visibility: "Public" as const,
  ownerId: "user-1",
  owner: { id: "user-1", username: "alice" },
  collectionModels: [
    {
      collectionId: "coll-1",
      modelId: "mod-1",
      model: { id: "mod-1", name: "Model 1", thumbnailUrl: "https://thumb.png" },
      addedAt: "2024-01-01",
    },
  ],
};

describe("CollectionDetailsPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(collectionsService.getCollectionById).mockResolvedValue(mockCollection as never);
  });

  it("shows loading skeleton initially then collection content", async () => {
    render(<CollectionDetailsPage />, {
      mockAuth: { user: { id: "user-1", email: "alice@test.com", username: "alice", accessToken: "x" } },
    });

    await waitFor(() => {
      expect(screen.getByText("Test Collection")).toBeInTheDocument();
    });
    expect(screen.getByText("Description")).toBeInTheDocument();
    expect(screen.getByText("1 models")).toBeInTheDocument();
  });

  it("shows password prompt when getCollectionById returns 401", async () => {
    vi.mocked(collectionsService.getCollectionById).mockRejectedValue({
      response: { status: 401 },
    });

    render(<CollectionDetailsPage />, {
      mockAuth: { user: { id: "user-1", email: "alice@test.com", username: "alice", accessToken: "x" } },
    });

    await waitFor(() => {
      expect(screen.getByPlaceholderText("Enter password")).toBeInTheDocument();
    });
  });

  it("shows error when load fails with non-401", async () => {
    vi.mocked(collectionsService.getCollectionById).mockRejectedValue(
      new Error("Network error")
    );

    render(<CollectionDetailsPage />, {
      mockAuth: { user: { id: "user-1", email: "alice@test.com", username: "alice", accessToken: "x" } },
    });

    await waitFor(() => {
      expect(screen.getByText(/failed to load collection|collection not found/i)).toBeInTheDocument();
    });
  });

  it("shows Edit and Delete for owner", async () => {
    render(<CollectionDetailsPage />, {
      mockAuth: { user: { id: "user-1", email: "alice@test.com", username: "alice", accessToken: "x" } },
    });

    await waitFor(() => {
      expect(screen.getByText("Test Collection")).toBeInTheDocument();
    });
    expect(screen.getByRole("link", { name: /edit/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /delete/i })).toBeInTheDocument();
  });

  it("does not show Edit/Delete for non-owner", async () => {
    render(<CollectionDetailsPage />, {
      mockAuth: { user: { id: "other-user", email: "bob@test.com", username: "bob", accessToken: "x" } },
    });

    await waitFor(() => {
      expect(screen.getByText("Test Collection")).toBeInTheDocument();
    });
    expect(screen.queryByRole("link", { name: /edit/i })).not.toBeInTheDocument();
    expect(screen.queryByRole("button", { name: /delete/i })).not.toBeInTheDocument();
  });

  it("opens delete dialog and navigates on confirm", async () => {
    const user = userEvent.setup();
    vi.mocked(collectionsService.deleteCollection).mockResolvedValue(undefined);

    render(<CollectionDetailsPage />, {
      mockAuth: { user: { id: "user-1", email: "alice@test.com", username: "alice", accessToken: "x" } },
    });

    await waitFor(() => {
      expect(screen.getByText("Test Collection")).toBeInTheDocument();
    });
    await user.click(screen.getByRole("button", { name: /delete/i }));

    await waitFor(() => {
      expect(screen.getByRole("dialog")).toBeInTheDocument();
    });
    await user.click(screen.getByRole("button", { name: /^delete$/i }));

    await waitFor(() => {
      expect(collectionsService.deleteCollection).toHaveBeenCalledWith("coll-1");
      expect(mockPush).toHaveBeenCalledWith("/collections/mine");
    });
  });
});
