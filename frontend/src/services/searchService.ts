import { ApiClientFactory } from '../api/clientFactory';
import { SearchResponse, SearchResultItem, SearchType } from '../api/client';

export interface SearchParams {
  query: string;
  page?: number;
  pageSize?: number;
  type?: SearchType;
  category?: string;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface SearchResult {
  id: string;
  title: string;
  description?: string;
  thumbnailUrl?: string;
  avatar?: string;
  type: 'model' | 'user' | 'collection';
  author?: string;
  authorId?: string;
  createdAt: string;
  updatedAt: string;
  downloads?: number;
  likes?: number;
  modelCount?: number;
  username?: string;
  email?: string;
  relevanceScore: number;
}

export interface SearchResults {
  results: SearchResult[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  query: string;
  type: SearchType;
}

export class SearchService {
  static async search(params: SearchParams): Promise<SearchResults> {
    try {
      const response = await ApiClientFactory.getApiClient().search_Search(
        params.query,
        params.page || 1,
        params.pageSize || 20,
        params.type ?? SearchType.All,
        params.category ?? null,
        params.sortBy || 'relevance',
        params.sortDescending || false
      );

      return {
        results: (response.results || []).map(SearchService.mapSearchResultItem),
        totalCount: response.totalCount || 0,
        page: response.page || 1,
        pageSize: response.pageSize || 20,
        totalPages: response.totalPages || 0,
        query: response.query || params.query,
        type: response.type || SearchType.All
      };
    } catch (error) {
      console.error('Error searching:', error);
      throw new Error('Search failed. Please try again.');
    }
  }

  static async searchModels(params: { searchQuery: string; page?: number; pageSize?: number }): Promise<{ models: any[] }> {
    try {
      const results = await this.search({
        query: params.searchQuery,
        page: params.page || 1,
        pageSize: params.pageSize || 20,
        type: SearchType.Models
      });

      // Convert search results back to the format expected by the Dashboard
      const models = results.results.map(result => ({
        id: result.id,
        name: result.title,
        description: result.description || '',
        authorId: result.authorId || '',
        author: result.author ? { username: result.author } : undefined,
        createdAt: result.createdAt,
        updatedAt: result.updatedAt,
        thumbnailUrl: result.thumbnailUrl,
        downloads: result.downloads || 0,
        likes: result.likes || 0,
        downloadCount: result.downloads || 0,
        rating: 0,
        isLiked: false,
        isInCollection: false,
        // Add other required fields with defaults
        privacy: 0, // Public
        aiGenerated: false,
        wip: false,
        nsfw: false,
        isRemix: false,
        isPublic: true,
        isFeatured: false,
        userId: result.authorId || '',
        fileUrl: '',
        license: '',
        categories: [],
        tags: [],
        versions: [],
        comments: []
      }));

      return { models };
    } catch (error) {
      console.error('Error searching models:', error);
      throw new Error('Failed to search models');
    }
  }

  private static mapSearchResultItem(item: SearchResultItem): SearchResult {
    return {
      id: item.id || '',
      title: item.title || '',
      description: item.description,
      thumbnailUrl: item.thumbnailUrl,
      avatar: item.avatar,
      type: SearchService.mapSearchResultType(item.type),
      author: item.author,
      authorId: item.authorId,
      createdAt: item.createdAt ? item.createdAt.toISOString() : new Date().toISOString(),
      updatedAt: item.updatedAt ? item.updatedAt.toISOString() : new Date().toISOString(),
      downloads: item.downloads,
      likes: item.likes,
      modelCount: item.modelCount,
      username: item.username,
      email: item.email,
      relevanceScore: item.relevanceScore || 0
    };
  }

  private static mapSearchResultType(type?: any): 'model' | 'user' | 'collection' {
    switch (type) {
      case 0: // SearchResultType.Model
        return 'model';
      case 1: // SearchResultType.User
        return 'user';
      case 2: // SearchResultType.Collection
        return 'collection';
      default:
        return 'model';
    }
  }
}

export default SearchService;
