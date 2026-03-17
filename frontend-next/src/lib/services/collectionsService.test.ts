import { vi, describe, it, expect, beforeEach } from "vitest";
import { collectionsService } from "./collectionsService";

const mockGetUserCollections = vi.fn();
const mockGetCollectionById = vi.fn();
const mockAccessCollection = vi.fn();
const mockCreateCollection = vi.fn();
const mockUpdateCollection = vi.fn();
const mockDeleteCollection = vi.fn();
const mockAddModelToCollection = vi.fn();
const mockRemoveModelFromCollection = vi.fn();
const mockGetFavoriteCollections = vi.fn();
const mockToggleFavorite = vi.fn();

vi.mock("@/lib/api/clientFactory", () => ({
  ApiClientFactory: {
    getApiClient: () => ({
      getUserCollections_GetCurrentUserCollections: mockGetUserCollections,
      getCollectionById_GetCollectionById: mockGetCollectionById,
      accessCollection_AccessCollection: mockAccessCollection,
      createCollection_CreateCollection: mockCreateCollection,
      updateCollection_UpdateCollection: mockUpdateCollection,
      deleteCollection_DeleteCollection: mockDeleteCollection,
      addModelToCollection_AddModelToCollection2: mockAddModelToCollection,
      removeModelFromCollection_RemoveModelFromCollection2: mockRemoveModelFromCollection,
      getFavoriteCollections_GetFavoriteCollections: mockGetFavoriteCollections,
      favoriteCollection_ToggleFavorite: mockToggleFavorite,
    }),
  },
}));

function createBlobResponse<T>(data: T) {
  return { data: new Blob([JSON.stringify(data)], { type: "application/json" }) };
}

describe("collectionsService", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe("getUserCollections", () => {
    it("returns parsed collections from API response", async () => {
      const response = { collections: [{ id: "c1", name: "Test" }], totalCount: 1, page: 1, totalPages: 1 };
      mockGetUserCollections.mockResolvedValue(createBlobResponse(response));

      const result = await collectionsService.getUserCollections(1, 12, null);

      expect(result.collections).toHaveLength(1);
      expect(result.collections?.[0].id).toBe("c1");
      expect(result.collections?.[0].name).toBe("Test");
      expect(mockGetUserCollections).toHaveBeenCalledWith(1, 12, null);
    });

    it("handles API error", async () => {
      mockGetUserCollections.mockRejectedValue(new Error("Network error"));

      await expect(collectionsService.getUserCollections(1, 12)).rejects.toThrow();
    });
  });

  describe("getCollectionById", () => {
    it("returns parsed collection", async () => {
      const collection = { id: "c1", name: "Test", visibility: "Public" };
      mockGetCollectionById.mockResolvedValue(createBlobResponse(collection));

      const result = await collectionsService.getCollectionById("c1");

      expect(result.id).toBe("c1");
      expect(result.name).toBe("Test");
      expect(mockGetCollectionById).toHaveBeenCalledWith("c1");
    });
  });

  describe("accessCollection", () => {
    it("sends password and returns collection", async () => {
      const collection = { id: "c1", name: "Locked" };
      mockAccessCollection.mockResolvedValue(createBlobResponse(collection));

      const result = await collectionsService.accessCollection("c1", "secret");

      expect(result.name).toBe("Locked");
      expect(mockAccessCollection).toHaveBeenCalledWith("c1", expect.any(Object));
    });
  });

  describe("createCollection", () => {
    it("sends create command and returns collection", async () => {
      const created = { id: "c1", name: "New" };
      mockCreateCollection.mockResolvedValue(createBlobResponse(created));

      const result = await collectionsService.createCollection({
        name: "New",
        description: "Desc",
        visibility: "Public",
      });

      expect(result.id).toBe("c1");
      expect(mockCreateCollection).toHaveBeenCalledWith(expect.any(Object));
    });
  });

  describe("updateCollection", () => {
    it("sends update command and returns collection", async () => {
      const updated = { id: "c1", name: "Updated" };
      mockUpdateCollection.mockResolvedValue(createBlobResponse(updated));

      const result = await collectionsService.updateCollection({
        id: "c1",
        name: "Updated",
      });

      expect(result.name).toBe("Updated");
      expect(mockUpdateCollection).toHaveBeenCalledWith("c1", expect.any(Object));
    });
  });

  describe("deleteCollection", () => {
    it("calls delete endpoint", async () => {
      mockDeleteCollection.mockResolvedValue(undefined);

      await collectionsService.deleteCollection("c1");

      expect(mockDeleteCollection).toHaveBeenCalledWith("c1");
    });
  });

  describe("addModelToCollection", () => {
    it("calls add endpoint", async () => {
      mockAddModelToCollection.mockResolvedValue(undefined);

      await collectionsService.addModelToCollection("c1", "m1");

      expect(mockAddModelToCollection).toHaveBeenCalledWith("c1", "m1");
    });
  });

  describe("removeModelFromCollection", () => {
    it("calls remove endpoint", async () => {
      mockRemoveModelFromCollection.mockResolvedValue(undefined);

      await collectionsService.removeModelFromCollection("c1", "m1");

      expect(mockRemoveModelFromCollection).toHaveBeenCalledWith("c1", "m1");
    });
  });

  describe("getFavoriteCollections", () => {
    it("returns collections from collections field", async () => {
      const items = [{ id: "c1", name: "Fav" }];
      mockGetFavoriteCollections.mockResolvedValue(createBlobResponse({ collections: items }));

      const result = await collectionsService.getFavoriteCollections();

      expect(result).toHaveLength(1);
      expect(result[0].name).toBe("Fav");
    });

    it("falls back to items field", async () => {
      const items = [{ id: "c1", name: "Fav" }];
      mockGetFavoriteCollections.mockResolvedValue(createBlobResponse({ items }));

      const result = await collectionsService.getFavoriteCollections();

      expect(result).toHaveLength(1);
    });
  });

  describe("toggleFavorite", () => {
    it("sends command and returns result", async () => {
      mockToggleFavorite.mockResolvedValue(
        createBlobResponse({ success: true, message: "ok", isFavorite: true })
      );

      const result = await collectionsService.toggleFavorite("c1", true);

      expect(result.isFavorite).toBe(true);
      expect(mockToggleFavorite).toHaveBeenCalledWith("c1", expect.any(Object));
    });
  });
});
