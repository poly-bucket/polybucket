'use client';

import { useState, useEffect, useCallback } from 'react';
import { PluginSummary, PluginBrowseRequest, PluginBrowseResponse, SORT_OPTIONS, SORT_ORDER } from '@/types/plugin';
import { apiClient } from '@/lib/api';

interface UsePluginBrowseState {
  plugins: PluginSummary[];
  loading: boolean;
  error: string | null;
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

interface UsePluginBrowseActions {
  setSearch: (search: string) => void;
  setCategory: (category: string | null) => void;
  setSortBy: (sortBy: string) => void;
  setSortOrder: (sortOrder: string) => void;
  setPage: (page: number) => void;
  setPageSize: (pageSize: number) => void;
  setMinRating: (minRating: number | null) => void;
  setVerified: (verified: boolean | null) => void;
  setFeatured: (featured: boolean | null) => void;
  refresh: () => void;
  resetFilters: () => void;
}

interface UsePluginBrowseReturn extends UsePluginBrowseState, UsePluginBrowseActions {}

const DEFAULT_REQUEST: PluginBrowseRequest = {
  search: '',
  category: undefined,
  tags: [],
  sortBy: SORT_OPTIONS.DOWNLOADS,
  sortOrder: SORT_ORDER.DESC,
  page: 1,
  pageSize: 20,
  isVerified: undefined,
  isFeatured: undefined,
  minRating: undefined,
};

export function usePluginBrowse(): UsePluginBrowseReturn {
  const [request, setRequest] = useState<PluginBrowseRequest>(DEFAULT_REQUEST);
  const [state, setState] = useState<UsePluginBrowseState>({
    plugins: [],
    loading: false,
    error: null,
    totalCount: 0,
    page: 1,
    pageSize: 20,
    totalPages: 0,
    hasNextPage: false,
    hasPreviousPage: false,
  });

  const fetchPlugins = useCallback(async () => {
    setState(prev => ({ ...prev, loading: true, error: null }));
    
    try {
      const response: PluginBrowseResponse = await apiClient.browsePlugins(request);
      
      setState(prev => ({
        ...prev,
        plugins: response.plugins,
        totalCount: response.totalCount,
        page: response.page,
        pageSize: response.pageSize,
        totalPages: response.totalPages,
        hasNextPage: response.hasNextPage,
        hasPreviousPage: response.hasPreviousPage,
        loading: false,
      }));
    } catch (error) {
      setState(prev => ({
        ...prev,
        loading: false,
        error: error instanceof Error ? error.message : 'Failed to fetch plugins',
      }));
    }
  }, [request]);

  // Fetch plugins when request changes
  useEffect(() => {
    fetchPlugins();
  }, [fetchPlugins]);

  const updateRequest = useCallback((updates: Partial<PluginBrowseRequest>) => {
    setRequest(prev => ({ ...prev, ...updates }));
  }, []);

  const setSearch = useCallback((search: string) => {
    updateRequest({ search: search || undefined, page: 1 });
  }, [updateRequest]);

  const setCategory = useCallback((category: string | null) => {
    updateRequest({ category: category || undefined, page: 1 });
  }, [updateRequest]);

  const setSortBy = useCallback((sortBy: string) => {
    updateRequest({ sortBy, page: 1 });
  }, [updateRequest]);

  const setSortOrder = useCallback((sortOrder: string) => {
    updateRequest({ sortOrder, page: 1 });
  }, [updateRequest]);

  const setPage = useCallback((page: number) => {
    updateRequest({ page });
  }, [updateRequest]);

  const setPageSize = useCallback((pageSize: number) => {
    updateRequest({ pageSize, page: 1 });
  }, [updateRequest]);

  const setMinRating = useCallback((minRating: number | null) => {
    updateRequest({ minRating: minRating || undefined, page: 1 });
  }, [updateRequest]);

  const setVerified = useCallback((verified: boolean | null) => {
    updateRequest({ isVerified: verified || undefined, page: 1 });
  }, [updateRequest]);

  const setFeatured = useCallback((featured: boolean | null) => {
    updateRequest({ isFeatured: featured || undefined, page: 1 });
  }, [updateRequest]);

  const refresh = useCallback(() => {
    fetchPlugins();
  }, [fetchPlugins]);

  const resetFilters = useCallback(() => {
    setRequest(DEFAULT_REQUEST);
  }, []);

  return {
    ...state,
    setSearch,
    setCategory,
    setSortBy,
    setSortOrder,
    setPage,
    setPageSize,
    setMinRating,
    setVerified,
    setFeatured,
    refresh,
    resetFilters,
  };
}
