import api from '../utils/axiosConfig';

const API_URL = '/collections';

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
  // Get all collections for the current user
  async getUserCollections(): Promise<Collection[]> {
    const response = await api.get(`${API_URL}/mine`);
    return response.data;
  },

  // Get a specific collection by ID
  async getCollectionById(id: string): Promise<Collection> {
    const response = await api.get(`${API_URL}/${id}`);
    return response.data;
  },

  // Access a collection (with password if required)
  async accessCollection(id: string, password?: string): Promise<Collection> {
    const response = await api.post(`${API_URL}/${id}/access`, { collectionId: id, password });
    return response.data;
  },

  // Create a new collection
  async createCollection(collection: CreateCollectionRequest): Promise<Collection> {
    const response = await api.post(API_URL, collection);
    return response.data;
  },

  // Update an existing collection
  async updateCollection(collection: UpdateCollectionRequest): Promise<Collection> {
    const response = await api.put(`${API_URL}/${collection.id}`, collection);
    return response.data;
  },

  // Delete a collection
  async deleteCollection(id: string): Promise<void> {
    await api.delete(`${API_URL}/${id}`);
  },

  // Add a model to a collection
  async addModelToCollection(collectionId: string, modelId: string): Promise<void> {
    await api.post(`${API_URL}/${collectionId}/models/${modelId}`);
  },

  // Remove a model from a collection
  async removeModelFromCollection(collectionId: string, modelId: string): Promise<void> {
    await api.delete(`${API_URL}/${collectionId}/models/${modelId}`);
  }
};

export default collectionsService; 