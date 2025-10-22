import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeftIcon, PencilIcon, TrashIcon, PlusIcon } from '@heroicons/react/24/outline';
import NavigationBar from '../components/common/NavigationBar';
import collectionsService, { Collection } from '../services/collectionsService';
import { useAppSelector } from '../utils/hooks';
import PasswordPrompt from '../components/collections/PasswordPrompt';
import ModelGrid from '../components/ModelGrid';
import LayoutControls from '../components/common/LayoutControls';
import CollectionsBar from '../components/collections/CollectionsBar';
import { ExtendedModel } from '../services/modelsService';

const CollectionDetails: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user } = useAppSelector((state) => state.auth);
  
  const [collection, setCollection] = useState<Collection | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [showPasswordPrompt, setShowPasswordPrompt] = useState(false);
  const [passwordError, setPasswordError] = useState('');
  
  // Collections sidebar state
  const [isCollectionsSidebarCollapsed, setIsCollectionsSidebarCollapsed] = useState(
    localStorage.getItem('collectionsSidebarCollapsed') === 'true'
  );

  useEffect(() => {
    if (id) {
      loadCollection();
    }
  }, [id]);

  const loadCollection = async () => {
    if (!id) return;
    
    try {
      setLoading(true);
      setError('');
      const collectionData = await collectionsService.getCollectionById(id);
      setCollection(collectionData);
    } catch (err: any) {
      console.error('Error loading collection:', err);
      
      // Check if it's a password required error
      if (err.response?.status === 401 && err.response?.data?.requiresPassword) {
        setShowPasswordPrompt(true);
        setError('');
      } else {
        setError('Failed to load collection');
      }
    } finally {
      setLoading(false);
    }
  };

  const handlePasswordSubmit = async (password: string) => {
    if (!id) return;
    
    try {
      setPasswordError('');
      const collectionData = await collectionsService.accessCollection(id, password);
      setCollection(collectionData);
      setShowPasswordPrompt(false);
    } catch (err: any) {
      console.error('Error accessing collection:', err);
      if (err.response?.status === 401) {
        setPasswordError('Incorrect password');
      } else {
        setPasswordError('Failed to access collection');
      }
    }
  };

  const handlePasswordCancel = () => {
    setShowPasswordPrompt(false);
    navigate('/my-collections');
  };

  const handleEdit = () => {
    if (collection) {
      navigate(`/my-collections/${collection.id}/edit`);
    }
  };

  const handleDelete = () => {
    setShowDeleteModal(true);
  };

  const confirmDelete = async () => {
    if (!collection) return;
    
    try {
      await collectionsService.deleteCollection(collection.id);
      navigate('/my-collections');
    } catch (err) {
      console.error('Error deleting collection:', err);
      setError('Failed to delete collection');
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

  const isOwner = collection?.ownerId === user?.id;

  // Convert collection models to ExtendedModel format for ModelGrid
  const getCollectionModels = (): ExtendedModel[] => {
    if (!collection?.collectionModels) return [];
    
    return collection.collectionModels
      .filter(collectionModel => collectionModel.model)
      .map((collectionModel) => ({
        id: collectionModel.model!.id,
        name: collectionModel.model!.name || 'Untitled Model',
        description: collectionModel.model!.description || '',
        thumbnailUrl: collectionModel.model!.thumbnailUrl || undefined,
        author: collection.owner ? {
          id: collection.owner.id,
          username: collection.owner.username
        } : undefined,
        likes: [],
        comments: [],
        wip: false,
        aiGenerated: false,
        nsfw: false,
        privacy: 'Public' as any,
        license: '',
        categories: [],
        createdAt: collectionModel.addedAt,
        updatedAt: collectionModel.addedAt,
        userId: collection.owner?.id || '',
        isRemix: false,
        isPublic: true,
        isFeatured: false,
        authorId: collection.owner?.id || '',
        downloads: 0,
        fileUrl: '',
        versions: [],
        tags: [],
        categoryCollection: []
      }));
  };

  if (loading) {
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
            title="Collection Details"
            showSearch={true}
            showUploadButton={true}
            showHomeLink={true}
          />

          {/* Main Content Area */}
          <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
            <div className="flex items-center justify-center py-12">
              <div className="lg-spinner h-12 w-12"></div>
            </div>
          </main>
        </div>
      </div>
    );
  }

  if (error || !collection) {
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
            title="Collection Details"
            showSearch={true}
            showUploadButton={true}
            showHomeLink={true}
          />

          {/* Main Content Area */}
          <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
            <div className="text-center py-12">
              <div className="bg-red-900/20 border border-red-500/30 rounded-md p-4 max-w-md mx-auto">
                <p className="text-red-200">{error || 'Collection not found'}</p>
                <button 
                  onClick={() => navigate('/my-collections')}
                  className="mt-2 text-sm text-red-400 hover:text-red-300 underline"
                >
                  Back to Collections
                </button>
              </div>
            </div>
          </main>
        </div>
      </div>
    );
  }

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
          title={collection.name}
          showSearch={true}
          showUploadButton={true}
          showHomeLink={true}
        />

        {/* Main Content Area */}
        <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          {/* Collection Header */}
          <div className="mb-6">
            <div className="flex items-center mb-4">
              <button
                onClick={() => navigate('/my-collections')}
                className="mr-4 p-2 text-gray-400 hover:text-gray-300 rounded-lg hover:bg-white/10 transition-colors"
              >
                <ArrowLeftIcon className="w-5 h-5" />
              </button>
              <div className="flex-1">
                <h1 className="text-3xl font-bold text-white">{collection.name}</h1>
                {collection.description && (
                  <p className="mt-2 text-gray-300">{collection.description}</p>
                )}
                <div className="mt-4 flex items-center space-x-4 text-sm text-gray-400">
                  <span className={`px-3 py-1 rounded-full ${
                    collection.visibility === 'Public' ? 'bg-green-500/20 text-green-400 border border-green-500/30' :
                    collection.visibility === 'Unlisted' ? 'bg-yellow-500/20 text-yellow-400 border border-yellow-500/30' :
                    'bg-gray-500/20 text-gray-400 border border-gray-500/30'
                  }`}>
                    {collection.visibility}
                  </span>
                  <span>{collection.collectionModels?.length || 0} models</span>
                  {collection.owner && (
                    <span>by {collection.owner.username}</span>
                  )}
                </div>
              </div>
              
              {isOwner && (
                <div className="flex space-x-2">
                  <button
                    onClick={handleEdit}
                    className="lg-button"
                  >
                    <PencilIcon className="w-4 h-4 mr-2" />
                    Edit
                  </button>
                  <button
                    onClick={handleDelete}
                    className="lg-button bg-red-600 hover:bg-red-700 border-red-600"
                  >
                    <TrashIcon className="w-4 h-4 mr-2" />
                    Delete
                  </button>
                </div>
              )}
            </div>
          </div>

          {/* Layout Controls */}
          <div className="flex justify-end mb-4">
            <LayoutControls compact={true} />
          </div>

          {/* Models Grid */}
          <ModelGrid 
            models={getCollectionModels()} 
            loading={loading}
            error={error}
            onModelClick={handleModelClick}
          />
        </main>
      </div>

      {/* Delete Confirmation Modal */}
      {showDeleteModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 overflow-y-auto h-full w-full z-50">
          <div className="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-gray-800 border-gray-600">
            <div className="mt-3 text-center">
              <div className="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-red-500/20">
                <TrashIcon className="h-6 w-6 text-red-400" />
              </div>
              <h3 className="text-lg font-medium text-white mt-2">Delete Collection</h3>
              <div className="mt-2 px-7 py-3">
                <p className="text-sm text-gray-300">
                  Are you sure you want to delete "{collection.name}"? This action cannot be undone.
                </p>
              </div>
              <div className="flex gap-4 justify-center mt-4">
                <button
                  onClick={() => setShowDeleteModal(false)}
                  className="lg-button"
                >
                  Cancel
                </button>
                <button
                  onClick={confirmDelete}
                  className="lg-button bg-red-600 hover:bg-red-700 border-red-600"
                >
                  Delete
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Password Prompt Modal */}
      {showPasswordPrompt && (
        <PasswordPrompt
          collectionName={id || ''}
          onSubmit={handlePasswordSubmit}
          onCancel={handlePasswordCancel}
          error={passwordError}
        />
      )}
    </div>
  );
};

export default CollectionDetails; 