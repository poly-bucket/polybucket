import { ApiClientFactory } from "@/lib/api/clientFactory";
import type {
  SearchResponse,
  SearchResultItem,
} from "@/lib/api/client";
import { SearchType, SearchResultType } from "@/lib/api/client";

export interface SearchParams {
  query: string;
  page?: number;
  pageSize?: number;
  type?: SearchType;
  category?: string;
  sortBy?: string;
  sortDescending?: boolean;
}

export type SearchResultKind = "model" | "user" | "collection";

export interface SearchResult {
  id: string;
  title: string;
  description?: string;
  thumbnailUrl?: string;
  avatar?: string;
  kind: SearchResultKind;
  author?: string;
  authorId?: string;
  username?: string;
  createdAt?: Date;
  updatedAt?: Date;
  downloads?: number;
  likes?: number;
  modelCount?: number;
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

function mapResultType(type?: SearchResultType): SearchResultKind {
  switch (type) {
    case SearchResultType.User:
      return "user";
    case SearchResultType.Collection:
      return "collection";
    default:
      return "model";
  }
}

function mapItem(item: SearchResultItem): SearchResult {
  return {
    id: item.id ?? "",
    title: item.title ?? "",
    description: item.description ?? undefined,
    thumbnailUrl: item.thumbnailUrl ?? undefined,
    avatar: item.avatar ?? undefined,
    kind: mapResultType(item.type),
    author: item.author ?? undefined,
    authorId: item.authorId ?? undefined,
    username: item.username ?? undefined,
    createdAt: item.createdAt,
    updatedAt: item.updatedAt,
    downloads: item.downloads ?? undefined,
    likes: item.likes ?? undefined,
    modelCount: item.modelCount ?? undefined,
    relevanceScore: item.relevanceScore ?? 0,
  };
}

function mapResponse(response: SearchResponse, params: SearchParams): SearchResults {
  return {
    results: (response.results ?? []).map(mapItem),
    totalCount: response.totalCount ?? 0,
    page: response.page ?? params.page ?? 1,
    pageSize: response.pageSize ?? params.pageSize ?? 20,
    totalPages: response.totalPages ?? 0,
    query: response.query ?? params.query,
    type: response.type ?? params.type ?? SearchType.All,
  };
}

export async function search(params: SearchParams): Promise<SearchResults> {
  const response = await ApiClientFactory.getApiClient().search_Search(
    params.query,
    params.page ?? 1,
    params.pageSize ?? 20,
    params.type ?? SearchType.All,
    params.category ?? undefined,
    params.sortBy ?? "relevance",
    params.sortDescending ?? false,
  );
  return mapResponse(response, params);
}

export async function quickSearch(query: string): Promise<SearchResults> {
  return search({ query, page: 1, pageSize: 8, type: SearchType.All });
}

export { SearchType, SearchResultType };
