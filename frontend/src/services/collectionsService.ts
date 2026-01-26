import { ApiClientFactory } from '../api/clientFactory';
import {
  CreateCollectionCommand,
  UpdateCollectionCommand,
  AccessCollectionCommand,
  FavoriteCollectionCommand,
  FileResponse
} from '../api/client';

async function parseFileResponseAsJson<T>(fileResponse: FileResponse): Promise<T> {
  if (fileResponse.data instanceof Blob) {
    const text = await fileResponse.data.text();
    return JSON.parse(text);
  }
  throw new Error('Invalid response format');
}

export interface Collection {
  id: string;
  name: string;
  description?: string;
  visibility: 'Public' | 'Private' | 'Unlisted';
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
  passwordHash?: string;
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
  visibility: 'Public' | 'Private' | 'Unlisted';
  password?: string;
  avatar?: string;
}

export interface UpdateCollectionRequest {
  id: string;
  name?: string;
  description?: string;
  visibility?: 'Public' | 'Private' | 'Unlisted';
  password?: string;
}

export interface AccessCollectionRequest {
  password?: string;
}

const api = () => ApiClientFactory.getApiClient();

const collectionsService = {
  async getUserCollections(): Promise<Collection[]> {
    const response = await api().getUserCollections_GetCurrentUserCollections(1, 100, null);
    const data = await parseFileResponseAsJson<{ collections?: Collection[]; items?: Collection[] }>(response);
    return data.collections || data.items || [];
  },

  async getCollectionById(id: string): Promise<Collection> {
    const response = await api().getCollectionById_GetCollectionById(id);
    return await parseFileResponseAsJson<Collection>(response);
  },

  async accessCollection(id: string, password?: string): Promise<Collection> {
    const command = new AccessCollectionCommand({ collectionId: id, password });
    const response = await api().accessCollection_AccessCollection(id, command);
    return await parseFileResponseAsJson<Collection>(response);
  },

  async createCollection(collection: CreateCollectionRequest): Promise<Collection> {
    const command = new CreateCollectionCommand({
      name: collection.name,
      description: collection.description,
      visibility: collection.visibility as any,
      password: collection.password,
      avatar: collection.avatar
    });
    const response = await api().createCollection_CreateCollection(command);
    return await parseFileResponseAsJson<Collection>(response);
  },

  async updateCollection(collection: UpdateCollectionRequest): Promise<Collection> {
    const command = new UpdateCollectionCommand({
      id: collection.id,
      name: collection.name,
      description: collection.description,
      visibility: collection.visibility as any,
      password: collection.password
    });
    const response = await api().updateCollection_UpdateCollection(collection.id, command);
    return await parseFileResponseAsJson<Collection>(response);
  },

  async deleteCollection(id: string): Promise<void> {
    await api().deleteCollection_DeleteCollection(id);
  },

  async addModelToCollection(collectionId: string, modelId: string): Promise<void> {
    await api().addModelToCollection_AddModelToCollection2(collectionId, modelId);
  },

  async removeModelFromCollection(collectionId: string, modelId: string): Promise<void> {
    await api().removeModelFromCollection_RemoveModelFromCollection2(collectionId, modelId);
  },

  async getFavoriteCollections(): Promise<Collection[]> {
    const response = await api().getFavoriteCollections_GetFavoriteCollections();
    const data = await parseFileResponseAsJson<{ collections?: Collection[]; items?: Collection[] }>(response);
    return data.collections || data.items || [];
  },

  async toggleFavorite(collectionId: string, isFavorite: boolean): Promise<{ success: boolean; message: string; isFavorite: boolean }> {
    const command = new FavoriteCollectionCommand({ collectionId, isFavorite });
    const response = await api().favoriteCollection_ToggleFavorite(collectionId, command);
    return await parseFileResponseAsJson<{ success: boolean; message: string; isFavorite: boolean }>(response);
  }
};

export default collectionsService;
