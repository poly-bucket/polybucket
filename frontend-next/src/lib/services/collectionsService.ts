import { ApiClientFactory } from "@/lib/api/clientFactory";
import axiosInstance from "@/lib/api/axiosConfig";
import {
  CreateCollectionCommand,
  UpdateCollectionCommand,
  AccessCollectionCommand,
  FavoriteCollectionCommand,
  CollectionVisibility,
} from "@/lib/api/client";
import { parseFileResponseAsJson } from "@/lib/api/parseResponse";

export interface Collection {
  id: string;
  name: string;
  description?: string;
  visibility: "Public" | "Private" | "Unlisted";
  ownerId: string;
  owner?: {
    id: string;
    username: string;
    profilePictureUrl?: string;
    avatar?: string;
  };
  avatar?: string;
  favorite?: boolean;
  displayOrder?: number;
  collectionModels?: CollectionModel[];
  createdAt?: string;
  updatedAt?: string;
  isPasswordProtected?: boolean;
  modelCount?: number;
}

export interface CollectionModel {
  collectionId: string;
  modelId: string;
  model?: {
    id: string;
    name: string;
    description?: string;
    thumbnailUrl?: string;
  };
  addedAt: string;
}

export interface CreateCollectionRequest {
  name: string;
  description?: string;
  visibility: "Public" | "Private" | "Unlisted";
  password?: string;
  avatar?: string;
}

export interface UpdateCollectionRequest {
  id: string;
  name?: string;
  description?: string;
  visibility?: "Public" | "Private" | "Unlisted";
  password?: string;
  avatar?: string;
}

export interface GetUserCollectionsResponse {
  collections?: Collection[];
  totalCount?: number;
  page?: number;
  pageSize?: number;
  totalPages?: number;
}

const api = () => ApiClientFactory.getApiClient();

function mapCollectionDto(dto: {
  id?: string;
  name?: string;
  description?: string;
  visibility?: CollectionVisibility;
  ownerId?: string;
  owner?: unknown;
  avatar?: string;
  favorite?: boolean;
  displayOrder?: number;
  isPasswordProtected?: boolean;
  modelCount?: number;
  collectionModels?: unknown;
  createdAt?: Date | string;
  updatedAt?: Date | string;
}): Collection {
  const createdAt =
    dto.createdAt instanceof Date
      ? dto.createdAt.toISOString()
      : dto.createdAt;
  const updatedAt =
    dto.updatedAt instanceof Date
      ? dto.updatedAt.toISOString()
      : dto.updatedAt;
  return {
    id: dto.id!,
    name: dto.name!,
    description: dto.description,
    visibility: dto.visibility as Collection["visibility"],
    ownerId: dto.ownerId!,
    owner: dto.owner as Collection["owner"],
    avatar: dto.avatar,
    favorite: dto.favorite,
    displayOrder: dto.displayOrder,
    isPasswordProtected: dto.isPasswordProtected,
    modelCount: dto.modelCount,
    collectionModels: dto.collectionModels as CollectionModel[] | undefined,
    createdAt,
    updatedAt,
  };
}

export const collectionsService = {
  async getUserCollections(
    page: number = 1,
    pageSize: number = 12,
    searchQuery?: string | null
  ): Promise<GetUserCollectionsResponse> {
    const response =
      await api().getUserCollections_GetCurrentUserCollections(
        page,
        pageSize,
        searchQuery ?? null
      );
    return {
      collections: response.collections?.map((c) => mapCollectionDto(c)),
      totalCount: response.totalCount,
      page: response.page,
      pageSize: response.pageSize,
      totalPages: response.totalPages,
    };
  },

  async getCollectionById(id: string): Promise<Collection> {
    const response = await api().getCollectionById_GetCollectionById(id);
    return mapCollectionDto(response);
  },

  async accessCollection(id: string, password?: string): Promise<Collection> {
    const command = new AccessCollectionCommand({ collectionId: id, password });
    const response = await api().accessCollection_AccessCollection(id, command);
    return mapCollectionDto(response);
  },

  async createCollection(
    collection: CreateCollectionRequest
  ): Promise<Collection> {
    const command = new CreateCollectionCommand({
      name: collection.name,
      description: collection.description,
      visibility: collection.visibility as CollectionVisibility,
      password: collection.password,
      avatar: collection.avatar,
    });
    const response = await api().createCollection_CreateCollection(command);
    return mapCollectionDto(response);
  },

  async updateCollection(
    collection: UpdateCollectionRequest
  ): Promise<Collection> {
    const command = new UpdateCollectionCommand({
      id: collection.id,
      name: collection.name,
      description: collection.description,
      visibility: collection.visibility as CollectionVisibility | undefined,
      password: collection.password,
      avatar: collection.avatar,
    });
    const response = await api().updateCollection_UpdateCollection(
      collection.id,
      command
    );
    return mapCollectionDto(response);
  },

  async deleteCollection(id: string): Promise<void> {
    await api().deleteCollection_DeleteCollection(id);
  },

  async addModelToCollection(
    collectionId: string,
    modelId: string
  ): Promise<void> {
    await axiosInstance.post(`/api/collections/${collectionId}/models/${modelId}`);
  },

  async removeModelFromCollection(
    collectionId: string,
    modelId: string
  ): Promise<void> {
    await axiosInstance.delete(`/api/collections/${collectionId}/models/${modelId}`);
  },

  async getFavoriteCollections(): Promise<Collection[]> {
    const response = await api().getFavoriteCollections_GetFavoriteCollections();
    const data = await parseFileResponseAsJson<{
      collections?: Collection[];
      items?: Collection[];
    }>(response);
    return data.collections ?? data.items ?? [];
  },

  async toggleFavorite(
    collectionId: string,
    isFavorite: boolean
  ): Promise<{ success: boolean; message: string; isFavorite: boolean }> {
    const command = new FavoriteCollectionCommand({
      collectionId,
      isFavorite,
    });
    const response = await api().favoriteCollection_ToggleFavorite(
      collectionId,
      command
    );
    return parseFileResponseAsJson<{
      success: boolean;
      message: string;
      isFavorite: boolean;
    }>(response);
  },
};
