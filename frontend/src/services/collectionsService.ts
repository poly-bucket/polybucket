import axios from 'axios';

const API_URL = import.meta.env.VITE_API_URL ? `${import.meta.env.VITE_API_URL}/api/collections` : 'http://localhost:11666/api/collections';

// Helper to get auth headers
const getAuthHeaders = () => {
  const user = localStorage.getItem('user');
  if (user) {
    const userData = JSON.parse(user);
    return {
      'Authorization': `Bearer ${userData.accessToken}`,
      'Content-Type': 'application/json'
    };
  }
  return {
    'Content-Type': 'application/json'
  };
};

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
  };
  collectionModels?: CollectionModel[];
  createdAt?: string;
  updatedAt?: string;
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
}

export interface UpdateCollectionRequest {
  id: string;
  name?: string;
  description?: string;
  visibility?: 'Public' | 'Private' | 'Unlisted';
}

const collectionsService = {
  // Get all collections for the current user
  async getUserCollections(): Promise<Collection[]> {
    const response = await axios.get(`${API_URL}/mine`, { headers: getAuthHeaders() });
    return response.data;
  },

  // Get a specific collection by ID
  async getCollectionById(id: string): Promise<Collection> {
    const response = await axios.get(`${API_URL}/${id}`, { headers: getAuthHeaders() });
    return response.data;
  },

  // Create a new collection
  async createCollection(collection: CreateCollectionRequest): Promise<Collection> {
    const response = await axios.post(API_URL, collection, { headers: getAuthHeaders() });
    return response.data;
  },

  // Update an existing collection
  async updateCollection(collection: UpdateCollectionRequest): Promise<Collection> {
    const response = await axios.put(`${API_URL}/${collection.id}`, collection, { headers: getAuthHeaders() });
    return response.data;
  },

  // Delete a collection
  async deleteCollection(id: string): Promise<void> {
    await axios.delete(`${API_URL}/${id}`, { headers: getAuthHeaders() });
  },

  // Add a model to a collection
  async addModelToCollection(collectionId: string, modelId: string): Promise<void> {
    await axios.post(`${API_URL}/${collectionId}/models/${modelId}`, {}, { headers: getAuthHeaders() });
  },

  // Remove a model from a collection
  async removeModelFromCollection(collectionId: string, modelId: string): Promise<void> {
    await axios.delete(`${API_URL}/${collectionId}/models/${modelId}`, { headers: getAuthHeaders() });
  }
};

export default collectionsService; 