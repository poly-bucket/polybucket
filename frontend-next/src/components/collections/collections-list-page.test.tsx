import { describe, it, expect, vi, beforeEach } from "vitest";
import userEvent from "@testing-library/user-event";
import { render, screen, waitFor } from "@/test/test-utils";
import { CollectionsListPage } from "./collections-list-page";

const mockPush = vi.fn();

vi.mock("next/navigation", () => ({
  useRouter: () => ({ push: mockPush }),
}));

vi.mock("@/lib/services/collectionsService", () => ({
  collectionsService: {
    getUserCollections: vi.fn(),
    deleteCollection: vi.fn(),
    toggleFavorite: vi.fn(),
  },
}));

import { collectionsService } from "@/lib/services/collectionsService";

const mockUser = { id: "user-1", email: "alice@test.com", username: "alice", accessToken: "token" };

describe("CollectionsListPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(collectionsService.getUserCollections).mockResolvedValue({
      collections: [],
      totalCount: 0,
      page: 1,
      totalPages: 1,
    } as never);
  });

  it("shows sign in message when not authenticated", () => {
    render(<CollectionsListPage />, { mockAuth: { user: null } });
    expect(screen.getByText(/sign in to view your collections/i)).toBeInTheDocument();
  });

  it("shows empty state when no collections", async () => {
    render(<CollectionsListPage />, { mockAuth: { user: mockUser } });

    await waitFor(() => {
      expect(screen.getByText(/no collections yet/i)).toBeInTheDocument();
    });
    expect(screen.getByRole("link", { name: /create your first collection/i })).toBeInTheDocument();
  });

  it("renders grid of collections when data is returned", async () => {
    vi.mocked(collectionsService.getUserCollections).mockResolvedValue({
      collections: [
        {
          id: "c1",
          name: "Collection 1",
          visibility: "Public",
          favorite: false,
          collectionModels: [],
        },
      ],
      totalCount: 1,
      page: 1,
      totalPages: 1,
    } as never);

    render(<CollectionsListPage />, { mockAuth: { user: mockUser } });

    await waitFor(() => {
      expect(screen.getByText("Collection 1")).toBeInTheDocument();
    });
  });

  it("has search and create buttons", async () => {
    render(<CollectionsListPage />, { mockAuth: { user: mockUser } });

    await waitFor(() => {
      expect(screen.getByPlaceholderText(/search collections/i)).toBeInTheDocument();
    });
    const createLinks = screen.getAllByRole("link", { name: /create/i });
    expect(createLinks.some((el) => el.getAttribute("href") === "/collections/create")).toBe(true);
  });

  it("opens delete dialog and removes collection on confirm", async () => {
    const collections = [
      { id: "c1", name: "To Delete", visibility: "Public", favorite: false, collectionModels: [] },
    ];
    vi.mocked(collectionsService.getUserCollections).mockResolvedValue({
      collections,
      totalCount: 1,
      page: 1,
      totalPages: 1,
    } as never);
    vi.mocked(collectionsService.deleteCollection).mockResolvedValue(undefined);

    const user = userEvent.setup();
    render(<CollectionsListPage />, { mockAuth: { user: mockUser } });

    await waitFor(() => {
      expect(screen.getByText("To Delete")).toBeInTheDocument();
    });

    const menuButton = screen.getByRole("button");
    await user.click(menuButton);
    const deleteItem = await screen.findByRole("menuitem", { name: /delete/i });
    await user.click(deleteItem);

    await waitFor(() => {
      expect(screen.getByRole("dialog")).toBeInTheDocument();
    });
    await user.click(screen.getByRole("button", { name: /^delete$/i }));

    await waitFor(() => {
      expect(collectionsService.deleteCollection).toHaveBeenCalledWith("c1");
    });
  });
});
