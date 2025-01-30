export interface Model {
  id: number;
  title: string;
  description: string;
  thumbnailUrl: string;
  modelUrl: string;
  likes: number;
  downloads: number;
  creator: string;
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