export interface Model {
  id: string;
  name: string;
  description: string;
  license: string;
  privacy: string;
  categories: string[];
  aiGenerated: boolean;
  wip: boolean;
  nsfw: boolean;
  isRemix: string;
  createdAt: string;
  updatedAt: string;
  files: ModelFile[];
  author: User;
  likes: number;
  downloads: number;
}

export interface ModelFile {
  name: string;
  path: string;
  size: number;
  mimeType: string;
  createdAt: string;
  updatedAt: string;
}

export interface PaginatedModelsResponse {
  models: Model[];
  totalCount: number;
  page: number;
  totalPages: number;
}

export interface User {
  id: number;
  username: string;
  avatarUrl: string;
  bio: string;
  joinDate: string;
  totalLikes: number;
  totalDownloads: number;
}

export interface ModelUploadData {
  title: string;
  description: string;
}

export interface Collection {
  id: number;
  title: string;
  description: string;
  visibility: 'public' | 'private' | 'unlisted';
  createdAt: string;
  updatedAt: string;
  userId: number;
  models: Model[];
}

export interface CollectionCreateData {
  title: string;
  description: string;
  visibility: Collection['visibility'];
} 