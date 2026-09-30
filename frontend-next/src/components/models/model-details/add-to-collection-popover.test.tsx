import { describe, it, expect, vi, beforeEach } from "vitest";
import userEvent from "@testing-library/user-event";
import { AxiosError, AxiosHeaders } from "axios";
import { render, screen, waitFor } from "@/test/test-utils";
import type { Collection } from "@/lib/services/collectionsService";
import { AddToCollectionPopover } from "./add-to-collection-popover";

const mockGetUserCollections = vi.fn();
const mockAdd = vi.fn();
const mockRemove = vi.fn();
const mockToastError = vi.fn();
const mockToastSuccess = vi.fn();

vi.mock("sonner", () => ({
  toast: {
    error: (message: string) => mockToastError(message),
    success: (message: string) => mockToastSuccess(message),
  },
}));

vi.mock("@/lib/services/collectionsService", () => ({
  collectionsService: {
    getUserCollections: (...args: unknown[]) => mockGetUserCollections(...args),
    addModelToCollection: (...args: unknown[]) => mockAdd(...args),
    removeModelFromCollection: (...args: unknown[]) => mockRemove(...args),
  },
}));

const signedInUser = { id: "user-1", email: "maker@example.com", username: "maker", accessToken: "token" };

function collection(id: string, name: string, modelIds: string[] = []): Collection {
  return {
    id,
    name,
    visibility: "Private",
    ownerId: "user-1",
    collectionModels: modelIds.map((modelId) => ({ collectionId: id, modelId, addedAt: "2026-09-01T00:00:00Z" })),
  };
}

describe("AddToCollectionPopover", () => {
  beforeEach(() => {
    mockGetUserCollections.mockReset().mockResolvedValue({
      collections: [collection("c1", "Workshop"), collection("c2", "Favorites", ["model-1"])],
    });
    mockAdd.mockReset().mockResolvedValue(undefined);
    mockRemove.mockReset().mockResolvedValue(undefined);
    mockToastError.mockReset();
    mockToastSuccess.mockReset();
  });

  it("does not load collections until opened", async () => {
    // Arrange
    render(<AddToCollectionPopover modelId="model-1" />, { mockAuth: { user: signedInUser } });

    // Act
    const trigger = screen.getByRole("button", { name: /add to collection/i });

    // Assert
    expect(trigger).toHaveAttribute("aria-expanded", "false");
    expect(mockGetUserCollections).not.toHaveBeenCalled();
  });

  it("lists the user's collections and marks ones that already contain the model", async () => {
    // Arrange
    const user = userEvent.setup();
    render(<AddToCollectionPopover modelId="model-1" />, { mockAuth: { user: signedInUser } });

    // Act
    await user.click(screen.getByRole("button", { name: /add to collection/i }));

    // Assert
    expect(await screen.findByRole("button", { name: "Workshop" })).toHaveAttribute("aria-pressed", "false");
    expect(screen.getByRole("button", { name: "Favorites" })).toHaveAttribute("aria-pressed", "true");
    expect(mockGetUserCollections).toHaveBeenCalledWith(1, 50);
  });

  it("adds the model to a collection it is not in", async () => {
    // Arrange
    const user = userEvent.setup();
    render(<AddToCollectionPopover modelId="model-1" />, { mockAuth: { user: signedInUser } });
    await user.click(screen.getByRole("button", { name: /add to collection/i }));

    // Act
    await user.click(await screen.findByRole("button", { name: "Workshop" }));

    // Assert
    expect(mockAdd).toHaveBeenCalledWith("c1", "model-1");
    await waitFor(() => expect(screen.getByRole("button", { name: "Workshop" })).toHaveAttribute("aria-pressed", "true"));
    expect(mockToastSuccess).toHaveBeenCalledWith("Added to Workshop");
  });

  it("removes the model from a collection it is already in", async () => {
    // Arrange
    const user = userEvent.setup();
    render(<AddToCollectionPopover modelId="model-1" />, { mockAuth: { user: signedInUser } });
    await user.click(screen.getByRole("button", { name: /add to collection/i }));

    // Act
    await user.click(await screen.findByRole("button", { name: "Favorites" }));

    // Assert
    expect(mockRemove).toHaveBeenCalledWith("c2", "model-1");
    await waitFor(() => expect(screen.getByRole("button", { name: "Favorites" })).toHaveAttribute("aria-pressed", "false"));
  });

  it("keeps the state unchanged and explains when email verification is required", async () => {
    // Arrange
    mockAdd.mockRejectedValue(
      new AxiosError("Forbidden", "ERR_BAD_REQUEST", undefined, undefined, {
        status: 403,
        statusText: "Forbidden",
        data: { code: "email_unverified" },
        headers: {},
        config: { headers: new AxiosHeaders() },
      })
    );
    const user = userEvent.setup();
    render(<AddToCollectionPopover modelId="model-1" />, { mockAuth: { user: signedInUser } });
    await user.click(screen.getByRole("button", { name: /add to collection/i }));

    // Act
    await user.click(await screen.findByRole("button", { name: "Workshop" }));

    // Assert
    await waitFor(() => expect(mockToastError).toHaveBeenCalledWith("Verify your email address to organize collections."));
    expect(screen.getByRole("button", { name: "Workshop" })).toHaveAttribute("aria-pressed", "false");
  });

  it("shows an empty state with a link to create a collection", async () => {
    // Arrange
    mockGetUserCollections.mockResolvedValue({ collections: [] });
    const user = userEvent.setup();
    render(<AddToCollectionPopover modelId="model-1" />, { mockAuth: { user: signedInUser } });

    // Act
    await user.click(screen.getByRole("button", { name: /add to collection/i }));

    // Assert
    expect(await screen.findByText(/don't have any collections yet/i)).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /new collection/i })).toHaveAttribute("href", "/collections/create");
  });

  it("closes when Escape is pressed", async () => {
    // Arrange
    const user = userEvent.setup();
    render(<AddToCollectionPopover modelId="model-1" />, { mockAuth: { user: signedInUser } });
    await user.click(screen.getByRole("button", { name: /add to collection/i }));
    await screen.findByRole("dialog", { name: /add to collection/i });

    // Act
    await user.keyboard("{Escape}");

    // Assert
    expect(screen.queryByRole("dialog", { name: /add to collection/i })).not.toBeInTheDocument();
  });
});
