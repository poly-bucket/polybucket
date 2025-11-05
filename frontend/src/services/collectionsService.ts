import { API_CONFIG } from '../api/config';
import { AxiosHttpClient } from '../api/axiosAdapter';
import {
  GetUserCollectionsClient,
  GetCollectionByIdClient,
  CreateCollectionClient,
  UpdateCollectionClient,
  DeleteCollectionClient,
  AccessCollectionClient,
  GetFavoriteCollectionsClient,
  FavoriteCollectionClient,
  AddModelToCollectionClient,
  RemoveModelFromCollectionClient,
  CreateCollectionCommand,
  UpdateCollectionCommand,
  AccessCollectionCommand,
  FavoriteCollectionCommand,
  FileResponse
} from './api.client';

const sharedHttpClient = new AxiosHttpClient(API_CONFIG.baseUrl);

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

const collectionsService = {
  async getUserCollections(): Promise<Collection[]> {
    const client = new GetUserCollectionsClient(API_CONFIG.baseUrl, sharedHttpClient);
    const response = await client.getCurrentUserCollections(1, 100, null);
    const data = await parseFileResponseAsJson<{ collections?: Collection[]; items?: Collection[] }>(response);
    return data.collections || data.items || [];
  },

  async getCollectionById(id: string): Promise<Collection> {
    const client = new GetCollectionByIdClient(API_CONFIG.baseUrl, sharedHttpClient);
    const response = await client.getCollectionById(id);
    return await parseFileResponseAsJson<Collection>(response);
  },

  async accessCollection(id: string, password?: string): Promise<Collection> {
    const client = new AccessCollectionClient(API_CONFIG.baseUrl, sharedHttpClient);
    const command = new AccessCollectionCommand({
      collectionId: id,
      password: password
    });
    const response = await client.accessCollection(id, command);
    return await parseFileResponseAsJson<Collection>(response);
  },

  async createCollection(collection: CreateCollectionRequest): Promise<Collection> {
    const client = new CreateCollectionClient(API_CONFIG.baseUrl, sharedHttpClient);
    const command = new CreateCollectionCommand({
      name: collection.name,
      description: collection.description,
      visibility: collection.visibility as any,
      password: collection.password,
      avatar: collection.avatar
    });
    const response = await client.createCollection(command);
    return await parseFileResponseAsJson<Collection>(response);
  },

  async updateCollection(collection: UpdateCollectionRequest): Promise<Collection> {
    const client = new UpdateCollectionClient(API_CONFIG.baseUrl, sharedHttpClient);
    const command = new UpdateCollectionCommand({
      id: collection.id,
      name: collection.name,
      description: collection.description,
      visibility: collection.visibility as any,
      password: collection.password
    });
    const response = await client.updateCollection(collection.id, command);
    return await parseFileResponseAsJson<Collection>(response);
  },

  async deleteCollection(id: string): Promise<void> {
    const client = new DeleteCollectionClient(API_CONFIG.baseUrl, sharedHttpClient);
    await client.deleteCollection(id);
  },

  async addModelToCollection(collectionId: string, modelId: string): Promise<void> {
    const client = new AddModelToCollectionClient(API_CONFIG.baseUrl, sharedHttpClient);
    await client.addModelToCollection2(collectionId, modelId);
  },

  async removeModelFromCollection(collectionId: string, modelId: string): Promise<void> {
    const client = new RemoveModelFromCollectionClient(API_CONFIG.baseUrl, sharedHttpClient);
    await client.removeModelFromCollection2(collectionId, modelId);
  },

  async getFavoriteCollections(): Promise<Collection[]> {
    const client = new GetFavoriteCollectionsClient(API_CONFIG.baseUrl, sharedHttpClient);
    const response = await client.getFavoriteCollections();
    const data = await parseFileResponseAsJson<{ collections?: Collection[]; items?: Collection[] }>(response);
    return data.collections || data.items || [];
  },

  async toggleFavorite(collectionId: string, isFavorite: boolean): Promise<{ success: boolean; message: string; isFavorite: boolean }> {
    const client = new FavoriteCollectionClient(API_CONFIG.baseUrl, sharedHttpClient);
    const command = new FavoriteCollectionCommand({
      collectionId: collectionId,
      isFavorite: isFavorite
    });
    const response = await client.toggleFavorite(collectionId, command);
    return await parseFileResponseAsJson<{ success: boolean; message: string; isFavorite: boolean }>(response);
  }
};

export default collectionsService;
