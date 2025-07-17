import React, { useEffect, useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../utils/hooks';
import { logout, getSetupStatus } from '../store/slices/authSlice';
import ModelGrid from './ModelGrid';
import UserAvatar from './UserAvatar';
import CollectionsBar from './collections/CollectionsBar';
import modelsService, { ExtendedModel } from '../services/modelsService';

const Dashboard: React.FC = () => {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { user, setupStatus } = useAppSelector((state) => state.auth);
  
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

  // Check setup status and redirect if needed
  useEffect(() => {
    // If we don't have setup status, fetch it
    if (!setupStatus) {
      dispatch(getSetupStatus());
    } else if (setupStatus && !setupStatus.isRoleConfigured) {
      // If roles are not configured, redirect to role setup
      navigate('/custom-role-setup');
    }
  }, [setupStatus, dispatch, navigate]);

  // Fetch models based on active tab and search query
  useEffect(() => {
    const fetchModels = async () => {
      setLoading(true);
      setError('');
      
      try {
        let fetchedModels: ExtendedModel[] = [];
        
        if (searchQuery.trim()) {
          // If searching, use search functionality
          const searchResult = await modelsService.searchModels({
            searchQuery: searchQuery.trim(),
            sortBy: activeTab === 'popular' ? 'popular' : activeTab === 'recent' ? 'newest' : undefined,
            pageSize: 50
          });
          fetchedModels = searchResult.models;
          setIsSearching(true);
        } else {
          // Otherwise use tab-based fetching
          switch (activeTab) {
            case 'featured':
              fetchedModels = await modelsService.getFeaturedModels();
              break;
            case 'popular':
              fetchedModels = await modelsService.getPopularModels();
              break;
            case 'recent':
              fetchedModels = await modelsService.getRecentModels();
              break;
          }
          setIsSearching(false);
        }
        
        setModels(fetchedModels);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load models');
      } finally {
        setLoading(false);
      }
    };

    // Debounce search to avoid too many API calls
    const timeoutId = setTimeout(fetchModels, searchQuery ? 500 : 0);
    
    return () => clearTimeout(timeoutId);
  }, [activeTab, searchQuery]);

  const handleLogout = () => {
    dispatch(logout());
    navigate('/login');
  };

  const handleModelClick = (model: ExtendedModel) => {
    navigate(`/models/${model.id}`);
  };

  const toggleCollectionsSidebar = () => {
    const newCollapsed = !isCollectionsSidebarCollapsed;
    setIsCollectionsSidebarCollapsed(newCollapsed);
    localStorage.setItem('collectionsSidebarCollapsed', newCollapsed.toString());
  };

  return (
    <div className="min-h-screen bg-gray-100">
      {/* Collections Sidebar */}
      <CollectionsBar 
        isCollapsed={isCollectionsSidebarCollapsed}
        onToggle={toggleCollectionsSidebar}
      />
      
      {/* Main Content with offset for sidebar */}
      <div className={`transition-all duration-200 ${
        isCollectionsSidebarCollapsed ? 'ml-12' : 'ml-64'
      }`}>
        <nav className="bg-white shadow-sm">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div className="flex justify-between h-16">
            <div className="flex">
              <div className="flex-shrink-0 flex items-center">
                <span className="text-xl font-bold text-indigo-600">PolyBucket</span>
              </div>
            </div>
            <div className="flex items-center">
              <div className="ml-3 relative">
                <div className="flex items-center">
                  <UserAvatar 
                    username={user?.username || ''} 
                    profilePictureUrl={user?.profilePictureUrl}
                    size="sm" 
                    className="mr-3"
                  />
                  <span className="mr-4 text-sm font-medium text-gray-700">
                    {user?.username}
                  </span>
                  <Link 
                    to="/admin-panel"
                    className="mr-4 px-3 py-2 rounded-md text-sm font-medium text-gray-700 hover:text-gray-900 hover:bg-gray-50"
                  >
                    Admin Panel
                  </Link>
                  {user?.roles?.includes('Admin') && (
                    <Link 
                      to="/admin"
                      className="mr-4 px-3 py-2 rounded-md text-sm font-medium text-gray-700 hover:text-gray-900 hover:bg-gray-50"
                    >
                      Legacy Admin
                    </Link>
                  )}
                  <Link 
                    to="/settings"
                    className="mr-4 px-3 py-2 rounded-md text-sm font-medium text-gray-700 hover:text-gray-900 hover:bg-gray-50"
                  >
                    Settings
                  </Link>
                  <Link
                    to="/upload-model"
                    className="mr-4 inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-green-600 hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-green-500"
                  >
                    <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                    </svg>
                    Upload
                  </Link>
                  <button
                    onClick={handleLogout}
                    className="px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                  >
                    Logout
                  </button>
                </div>
              </div>
            </div>
          </div>
          </div>
        </nav>

        <div className="py-6">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          {/* Header */}
          <div className="mb-8">
            <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between">
              <div>
                <h1 className="text-3xl font-bold leading-tight text-gray-900">
                  Welcome back, {user?.username}!
                </h1>
                <p className="mt-2 text-gray-600">
                  Discover amazing 3D models from our community
                </p>
              </div>
              
              {/* Search Bar */}
              <div className="mt-4 sm:mt-0 sm:ml-6">
                <div className="relative">
                  <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                    <svg className="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                    </svg>
                  </div>
                  <input
                    type="text"
                    placeholder="Search models..."
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    className="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-md leading-5 bg-white placeholder-gray-500 focus:outline-none focus:placeholder-gray-400 focus:ring-1 focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
                  />
                  {searchQuery && (
                    <button
                      onClick={() => setSearchQuery('')}
                      className="absolute inset-y-0 right-0 pr-3 flex items-center"
                    >
                      <svg className="h-5 w-5 text-gray-400 hover:text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                      </svg>
                    </button>
                  )}
                </div>
              </div>
            </div>
          </div>

          {/* Tab Navigation */}
          <div className="mb-6">
            <div className="flex items-center justify-between">
              <nav className="flex space-x-8" aria-label="Tabs">
                {[
                  { key: 'popular', label: 'Popular', icon: '🔥' },
                  { key: 'recent', label: 'Recent', icon: '⏰' },
                  { key: 'featured', label: 'Featured', icon: '⭐' },
                ].map((tab) => (
                  <button
                    key={tab.key}
                    onClick={() => setActiveTab(tab.key as any)}
                    disabled={isSearching}
                    className={`${
                      activeTab === tab.key && !isSearching
                        ? 'border-indigo-500 text-indigo-600'
                        : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                    } ${
                      isSearching ? 'opacity-50 cursor-not-allowed' : ''
                    } whitespace-nowrap py-2 px-1 border-b-2 font-medium text-sm flex items-center space-x-2`}
                  >
                    <span>{tab.icon}</span>
                    <span>{tab.label}</span>
                  </button>
                ))}
              </nav>
              
              {/* Search Results Info */}
              {isSearching && (
                <div className="text-sm text-gray-600">
                  {loading ? 'Searching...' : `${models.length} result${models.length !== 1 ? 's' : ''} for "${searchQuery}"`}
                </div>
              )}
            </div>
          </div>

          {/* Model Grid */}
          <ModelGrid
            models={models}
            loading={loading}
            error={error}
            onModelClick={handleModelClick}
            className="mb-8"
          />
        </div>
      </div>
      </div>
    </div>
  );
};

export default Dashboard; 