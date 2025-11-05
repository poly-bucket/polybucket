// API types for plugin browsing
export interface PluginSummary {
  id: string;
  name: string;
  description: string;
  category: string;
  version: string;
  author: string;
  authorId?: string;
  downloads: number;
  averageRating: number;
  reviewCount: number;
  isVerified: boolean;
  isFeatured: boolean;
  createdAt: string;
  updatedAt: string;
  tags: string[];
}

export interface PluginBrowseRequest {
  search?: string;
  category?: string;
  tags?: string[];
  sortBy?: string;
  sortOrder?: string;
  page?: number;
  pageSize?: number;
  isVerified?: boolean;
  isFeatured?: boolean;
  minRating?: number;
}

export interface PluginBrowseResponse {
  plugins: PluginSummary[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface PluginCategory {
  id: string;
  label: string;
  count: number;
}

// Sort options
export const SORT_OPTIONS = {
  DOWNLOADS: 'downloads',
  RATING: 'rating',
  CREATED: 'created',
  UPDATED: 'updated',
  NAME: 'name'
} as const;

export const SORT_ORDER = {
  ASC: 'asc',
  DESC: 'desc'
} as const;

export type SortOption = typeof SORT_OPTIONS[keyof typeof SORT_OPTIONS];
export type SortOrder = typeof SORT_ORDER[keyof typeof SORT_ORDER];

// Installation-related types
export interface PluginDownloadInfo {
  downloadUrl: string;
  version: string;
  size: number;
  checksum: string;
  repositoryUrl: string;
  repositoryType: 'github' | 'gitlab' | 'bitbucket' | 'other';
  branch?: string;
  commitHash?: string;
}

export interface MarketplacePluginDetails {
  id: string;
  name: string;
  description: string;
  version: string;
  author: string;
  repositoryUrl: string;
  repositoryType: 'github' | 'gitlab' | 'bitbucket' | 'other';
  downloadUrl: string;
  size: number;
  checksum: string;
  category: string;
  tags: string[];
  isVerified: boolean;
  isFeatured: boolean;
  downloads: number;
  averageRating: number;
  reviewCount: number;
  createdAt: string;
  updatedAt: string;
  lastCommitAt?: string;
  license?: string;
  readmeUrl?: string;
  documentationUrl?: string;
  issuesUrl?: string;
  releasesUrl?: string;
}

export interface PluginInstallationRequest {
  pluginId: string;
  version?: string;
  userId?: string;
  userAgent?: string;
  installationMethod: 'marketplace' | 'direct' | 'cli';
}

export interface PluginInstallationResponse {
  success: boolean;
  installationId: string;
  downloadUrl: string;
  version: string;
  message?: string;
}

// Installation status types
export type InstallationStatus = 'idle' | 'downloading' | 'installing' | 'success' | 'error';

export interface InstallationState {
  status: InstallationStatus;
  progress: number;
  error?: string;
  downloadUrl?: string;
  version?: string;
}
