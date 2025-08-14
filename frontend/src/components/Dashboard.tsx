import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../utils/hooks';
import ModelGrid from './ModelGrid';
import NavigationBar from './common/NavigationBar';
import CollectionsBar from './collections/CollectionsBar';
import modelsService, { ExtendedModel } from '../services/modelsService';

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
  const [isSearching, setIsSearching] = useState(false);
  
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
  const handleSearch = async (query: string) => {
    if (!query.trim()) {
      // If search is empty, reload current tab's content
      setSearchQuery('');
      return;
    }

    try {
      setIsSearching(true);
      setError('');
      const searchResults = await modelsService.searchModels({ searchQuery: query });
      setModels(searchResults.models);
      setSearchQuery(query);
    } catch (err) {
      console.error('Error searching models:', err);
      setError('Search failed. Please try again.');
    } finally {
      setIsSearching(false);
    }
  };

  const handleModelClick = (model: ExtendedModel) => {
    navigate(`/models/${model.id}`);
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
          searchQuery={searchQuery}
          isSearching={isSearching}
        />

        {/* Main Content Area */}
        <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          {/* Tab Navigation */}
          <div className="mb-6">
            <div className="border-b border-white/10">
              <nav className="-mb-px flex space-x-8">
                {(['featured', 'popular', 'recent'] as const).map((tab) => (
                  <button
                    key={tab}
                    onClick={() => {
                      setActiveTab(tab);
                      setSearchQuery(''); // Clear search when switching tabs
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
          </div>

          {/* Models Grid */}
          <ModelGrid 
            models={models} 
            loading={loading}
            error={error}
            onModelClick={handleModelClick}
          />
        </main>
      </div>
    </div>
  );
};

export default Dashboard; 