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

export interface PluginDownloadInfo {
  pluginId: string;
  version: string;
  downloadUrl: string;
  fileName: string;
  fileSize: number;
  checksum: string;
  createdAt: string;
  releaseNotes: string;
}

export interface PluginVersionSummary {
  id: string;
  version: string;
  releaseNotes: string;
  createdAt: string;
}

export interface MarketplacePluginDetails {
  id: string;
  name: string;
  description: string;
  longDescription: string;
  category: string;
  version: string;
  repositoryUrl: string;
  repositoryBranch: string;
  repositorySubpath: string;
  targets: string[];
  license: string;
  isVerified: boolean;
  isFeatured: boolean;
  downloads: number;
  averageRating: number;
  reviewCount: number;
  author: string;
  createdAt: string;
  updatedAt: string;
  versions: PluginVersionSummary[];
  tags: string[];
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
  installationId?: string;
  message?: string;
  installedAt?: string;
}

export interface InstallationState {
  status: 'idle' | 'downloading' | 'installing' | 'success' | 'error';
  progress: number;
  error?: string;
  downloadUrl?: string;
  version?: string;
}
