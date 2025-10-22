import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../utils/hooks';
import ModelGrid from './ModelGrid';
import NavigationBar from './common/NavigationBar';
import CollectionsBar from './collections/CollectionsBar';
import LayoutControls from './common/LayoutControls';
import SearchResults from './SearchResults';
import modelsService, { ExtendedModel } from '../services/modelsService';
import SearchService, { SearchResult } from '../services/searchService';
import { SearchType } from './common/SearchBar';

const Dashboard: React.FC = () => {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { user } = useAppSelector((state) => state.auth);
  
  // Model state
  const [models, setModels] = useState<ExtendedModel[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string>('');
  const [activeTab, setActiveTab] = useState<'featured' | 'popular' | 'recent'>('popular');
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [searchTags, setSearchTags] = useState<string[]>([]);
  const [selectedSearchTypes, setSelectedSearchTypes] = useState<SearchType[]>(['models']);
  const [isSearching, setIsSearching] = useState(false);
  const [searchResults, setSearchResults] = useState<SearchResult[]>([]);
  const [showSearchResults, setShowSearchResults] = useState(false);
  
  // Collections sidebar state
  const [isCollectionsSidebarCollapsed, setIsCollectionsSidebarCollapsed] = useState(
    localStorage.getItem('collectionsSidebarCollapsed') === 'true'
  );

  // Load models when component mounts or tab changes
  useEffect(() => {
    const loadModels = async () => {
      try {
        setLoading(true);
        setError('');
        
        let modelData: ExtendedModel[] = [];
        
        switch (activeTab) {
          case 'featured':
            modelData = await modelsService.getFeaturedModels();
            break;
          case 'popular':
            modelData = await modelsService.getPopularModels();
            break;
          case 'recent':
            modelData = await modelsService.getRecentModels();
            break;
        }
        
        setModels(modelData);
      } catch (err) {
        console.error('Error loading models:', err);
        setError('Failed to load models. Please try again later.');
      } finally {
        setLoading(false);
      }
    };

    loadModels();
  }, [activeTab]);

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
    <div className="min-h-screen flex relative">
      {/* Collections Sidebar */}
      <CollectionsBar 
        isCollapsed={isCollectionsSidebarCollapsed}
        onToggle={toggleCollectionsSidebar}
      />
      
      {/* Main Content */}
      <div className="flex-1 transition-all duration-300">
        {/* Navigation Bar */}
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

        {/* Main Content Area */}
        <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          {/* Tab Navigation and Layout Controls */}
          <div className="mb-6">
            <div className="flex items-center justify-between mb-4">
              <div className="border-b border-white/10 flex-1">
                <nav className="-mb-px flex space-x-8">
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
              models={models} 
              loading={loading}
              error={error}
              onModelClick={handleModelClick}
            />
          )}
        </main>
      </div>
    </div>
  );
};

export default Dashboard; 