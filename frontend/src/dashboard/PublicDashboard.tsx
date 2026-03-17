import React, { useEffect, useState, useCallback, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTokenValidation } from '../hooks/useTokenValidation';
import ModelGrid from '../models/ModelGrid';
import NavigationBar from '../components/common/NavigationBar';
import CollectionsBar from '../collections/CollectionsBar';
import LayoutControls from '../components/common/LayoutControls';
import SearchResults from './SearchResults';
import { ApiClientFactory } from '../api/clientFactory';
import { ExtendedModel } from '../services/modelsService';
import SearchService, { SearchResult } from '../services/searchService';
import { SearchType } from '../components/common/SearchBar';

const MAX_MODELS_IN_STATE = 150;
const MODELS_PER_PAGE = 50;

const PublicDashboard: React.FC = () => {
  const navigate = useNavigate();
  useTokenValidation();
  
  // Model state
  const [models, setModels] = useState<ExtendedModel[]>([]);
  const [loading, setLoading] = useState(true);
  const [loadingMore, setLoadingMore] = useState(false);
  const [error, setError] = useState<string>('');
  const [activeTab, setActiveTab] = useState<'featured' | 'popular' | 'recent'>('popular');
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [totalCount, setTotalCount] = useState(0);
  const [hasMore, setHasMore] = useState(true);
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [searchTags, setSearchTags] = useState<string[]>([]);
  const [selectedSearchTypes, setSelectedSearchTypes] = useState<SearchType[]>(['models']);
  const [isSearching, setIsSearching] = useState(false);
  const [searchResults, setSearchResults] = useState<SearchResult[]>([]);
  const [showSearchResults, setShowSearchResults] = useState(false);
  const [showFederatedOnly, setShowFederatedOnly] = useState(false);
  
  // Collections sidebar state
  const [isCollectionsSidebarCollapsed, setIsCollectionsSidebarCollapsed] = useState(
    localStorage.getItem('collectionsSidebarCollapsed') === 'true'
  );

  // Ref to track if we're currently loading to prevent duplicate requests
  const isLoadingRef = useRef(false);

  // Helper function to limit models to MAX_MODELS_IN_STATE
  const limitModelsState = useCallback((newModels: ExtendedModel[]): ExtendedModel[] => {
    if (newModels.length <= MAX_MODELS_IN_STATE) {
      return newModels;
    }
    // Keep the most recent MAX_MODELS_IN_STATE models
    return newModels.slice(-MAX_MODELS_IN_STATE);
  }, []);

  // Helper function to process and filter models based on active tab
  const processModels = useCallback((rawModels: any[]): ExtendedModel[] => {
    let processed = rawModels.map((model: any) => ({
      ...model,
      downloadCount: model.downloads || 0,
      rating: 0,
      isLiked: false,
      isInCollection: false
    }));

    // Filter based on active tab
    switch (activeTab) {
      case 'featured':
        processed = processed.filter(model => model.isFeatured);
        break;
      case 'popular':
        processed = processed.filter(model => (model.downloads || 0) > 0);
        break;
      case 'recent':
        // Sort by creation date (most recent first)
        processed = processed.sort((a, b) => 
          new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
        );
        break;
    }

    return processed;
  }, [activeTab]);

  // Load models function
  const loadModels = useCallback(async (page: number, isInitialLoad: boolean = false) => {
    if (isLoadingRef.current) return;
    
    try {
      if (isInitialLoad) {
        setLoading(true);
      } else {
        setLoadingMore(true);
      }
      setError('');
      isLoadingRef.current = true;
      
      const client = ApiClientFactory.getApiClient();
      const response = await client.getModels_GetModels(page || 1, MODELS_PER_PAGE || 50);
      
      let modelData: ExtendedModel[] = [];
      
      if (response.models && response.models.length > 0) {
        modelData = processModels(response.models);
      }
      
      setTotalPages(response.totalPages || 0);
      setTotalCount(response.totalCount || 0);
      setHasMore(page < (response.totalPages || 0));
      
      // Store raw models (before tab filtering) in allLoadedModels
      const rawModels = response.models?.map((model: any) => ({
        ...model,
        downloadCount: model.downloads || 0,
        rating: 0,
        isLiked: false,
        isInCollection: false
      })) || [];
      
      setAllLoadedModels(prevAll => {
        const existingIds = new Set(prevAll.map(m => m.id));
        const newUniqueModels = rawModels.filter((m: any) => !existingIds.has(m.id));
        const combined = [...prevAll, ...newUniqueModels];
        return limitModelsState(combined);
      });
      
      if (isInitialLoad) {
        setModels(limitModelsState(modelData));
      } else {
        setModels(prevModels => {
          // Combine existing models with new ones, avoiding duplicates
          const existingIds = new Set(prevModels.map(m => m.id));
          const newUniqueModels = modelData.filter(m => !existingIds.has(m.id));
          const combined = [...prevModels, ...newUniqueModels];
          return limitModelsState(combined);
        });
      }
      
      setCurrentPage(page);
    } catch (err) {
      console.error('Error loading models:', err);
      setError('Failed to load models. Please try again later.');
    } finally {
      setLoading(false);
      setLoadingMore(false);
      isLoadingRef.current = false;
    }
  }, [processModels, limitModelsState]);

  // Store all loaded models (before tab filtering) to avoid unnecessary reloads
  const [allLoadedModels, setAllLoadedModels] = useState<ExtendedModel[]>([]);

  // Load initial models when component mounts or tab changes
  useEffect(() => {
    // Reset totalCount when switching tabs to get fresh count for new tab
    setTotalCount(0);
    
    // If we already have models loaded, filter them first for immediate display
    if (allLoadedModels.length > 0) {
      const filteredModels = processModels(allLoadedModels);
      setModels(limitModelsState(filteredModels));
      setLoading(false);
      
      // Always load fresh data in the background for the new tab
      setCurrentPage(1);
      setHasMore(true);
      loadModels(1, false);
    } else {
      // No models loaded yet, start fresh
      setModels([]);
      setCurrentPage(1);
      setHasMore(true);
      loadModels(1, true);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [activeTab]);

  // Handle load more
  const handleLoadMore = useCallback(() => {
    if (!loadingMore && hasMore && !isLoadingRef.current) {
      loadModels(currentPage + 1, false);
    }
  }, [loadingMore, hasMore, currentPage, loadModels]);

  // Infinite scroll detection
  useEffect(() => {
    if (showSearchResults || loading || !hasMore) return;

    const handleScroll = () => {
      const scrollHeight = document.documentElement.scrollHeight;
      const scrollTop = document.documentElement.scrollTop || document.body.scrollTop;
      const clientHeight = document.documentElement.clientHeight;

      // Load more when user scrolls to within 200px of bottom
      if (scrollHeight - scrollTop - clientHeight < 200) {
        handleLoadMore();
      }
    };

    window.addEventListener('scroll', handleScroll, { passive: true });
    return () => window.removeEventListener('scroll', handleScroll);
  }, [showSearchResults, loading, hasMore, handleLoadMore]);

  // Filter models based on federated status
  const displayedModels = showFederatedOnly
    ? models.filter(model => model.isFederated)
    : models;

  // Handle search
  const handleSearch = async (query: string, searchTypes: SearchType[] = ['models']) => {
    if (!query.trim()) {
      return;
    }

    try {
      setIsSearching(true);
      setError('');
      
      // Add query as a search tag if it's not already in the list
      if (!searchTags.includes(query)) {
        setSearchTags(prev => [...prev, query]);
      }
      
      // Use search service with the selected types
      // If all types are selected or no specific types, search everything
      const searchType = searchTypes.length === 3 || searchTypes.includes('all') ? undefined : searchTypes[0] as any;
      
      const results = await SearchService.search({
        query: query,
        page: 1,
        pageSize: 50,
        type: searchType
      });
      
      setSearchResults(results.results);
      setShowSearchResults(true);
      setSearchQuery(query);
    } catch (err) {
      console.error('Error searching:', err);
      setError('Search failed. Please try again.');
    } finally {
      setIsSearching(false);
    }
  };

  // Handle clear search
  const handleClearSearch = () => {
    setSearchQuery('');
    setSearchTags([]);
    setShowSearchResults(false);
    setSearchResults([]);
  };

  // Handle search tag removal
  const handleSearchTagRemove = (tag: string) => {
    setSearchTags(prev => prev.filter(t => t !== tag));
    
    // If this was the last tag, clear search results
    if (searchTags.length === 1) {
      setShowSearchResults(false);
      setSearchResults([]);
    }
  };

  // Handle search tag addition
  const handleSearchTagAdd = (tag: string) => {
    if (!searchTags.includes(tag)) {
      setSearchTags(prev => [...prev, tag]);
      // Trigger search with the new tag
      handleSearch(tag, selectedSearchTypes);
    }
  };

  // Handle search type change
  const handleSearchTypeChange = (searchTypes: SearchType[]) => {
    setSelectedSearchTypes(searchTypes);
    // If there's an active search, re-run it with the new types
    if (searchQuery) {
      handleSearch(searchQuery, searchTypes);
    }
  };

  const handleModelClick = (model: ExtendedModel) => {
    navigate(`/models/${model.id}`);
  };

  const handleSearchResultClick = (result: SearchResult) => {
    // Clear search when clicking a result
    handleClearSearch();
  };

  const toggleCollectionsSidebar = () => {
    const newCollapsedState = !isCollectionsSidebarCollapsed;
    setIsCollectionsSidebarCollapsed(newCollapsedState);
    localStorage.setItem('collectionsSidebarCollapsed', newCollapsedState.toString());
  };

  return (
    <div className="min-h-screen flex flex-col">
      {/* Navigation Bar - Fixed at top */}
      <NavigationBar
        onSearch={handleSearch}
        onClearSearch={handleClearSearch}
        searchQuery={searchQuery}
        searchTags={searchTags}
        onSearchTagRemove={handleSearchTagRemove}
        onSearchTagAdd={handleSearchTagAdd}
        isSearching={isSearching}
        selectedSearchTypes={selectedSearchTypes}
        onSearchTypeChange={handleSearchTypeChange}
      />

      {/* Main Content Container */}
      <div className="flex flex-1 pt-20 overflow-hidden">
        {/* Collections Sidebar */}
        <CollectionsBar 
          isCollapsed={isCollectionsSidebarCollapsed}
          onToggle={toggleCollectionsSidebar}
        />
        
        {/* Main Content Area */}
        <div className="flex-1 transition-all duration-300 min-w-0">
          <main className="max-w-10xl mx-auto px-4 sm:px-6 lg:px-8 py-2">
          {/* Tab Navigation and Layout Controls */}
          <div className="mb-2">
            <div className="flex items-center justify-between mb-2">
              <div className="border-b border-white/10 flex-1">
                <nav className="-mb-px flex space-x-8 items-center">
                  {(['featured', 'popular', 'recent'] as const).map((tab) => (
                    <button
                      key={tab}
                      onClick={() => {
                        setActiveTab(tab);
                        handleClearSearch(); // Clear search when switching tabs
                      }}
                      className={`py-2 px-1 border-b-2 font-medium text-sm capitalize transition-colors duration-200 ${
                        activeTab === tab
                          ? 'border-indigo-500 text-indigo-400'
                          : 'border-transparent text-white/60 hover:text-white/80 hover:border-white/20'
                      }`}
                    >
                      {tab} Models
                    </button>
                  ))}
                  
                  {/* Federation Filter */}
                  <label className="flex items-center gap-2 ml-auto py-2 cursor-pointer">
                    <input
                      type="checkbox"
                      checked={showFederatedOnly}
                      onChange={(e) => setShowFederatedOnly(e.target.checked)}
                      className="w-4 h-4 text-purple-600 bg-gray-700 border-gray-600 rounded focus:ring-purple-500 focus:ring-2"
                    />
                    <span className="text-sm text-white/60 hover:text-white/80 flex items-center gap-1">
                      <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3.055 11H5a2 2 0 012 2v1a2 2 0 002 2 2 2 0 012 2v2.945M8 3.935V5.5A2.5 2.5 0 0010.5 8h.5a2 2 0 012 2 2 2 0 104 0 2 2 0 012-2h1.064M15 20.488V18a2 2 0 012-2h3.064M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                      </svg>
                      Federated only
                    </span>
                  </label>
                </nav>
              </div>
              
              {/* Layout Controls */}
              <div className="ml-6">
                <LayoutControls compact={true} />
              </div>
            </div>
          </div>

          {/* Search Results or Models Grid */}
          {showSearchResults ? (
            <SearchResults
              results={searchResults}
              loading={isSearching}
              error={error}
              onResultClick={handleSearchResultClick}
            />
          ) : (
            <ModelGrid 
              models={displayedModels} 
              loading={loading && (totalCount === 0 || models.length < totalCount)}
              error={error}
              onModelClick={handleModelClick}
              onLoadMore={handleLoadMore}
              hasMore={hasMore}
              loadingMore={loadingMore}
            />
          )}
          </main>
        </div>
      </div>
    </div>
  );
};

export default PublicDashboard; 