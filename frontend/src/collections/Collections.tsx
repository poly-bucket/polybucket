import React, { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { PlusIcon, FolderIcon } from '@heroicons/react/24/outline';
import CollectionCard from './CollectionCard';
import NavigationBar from '../components/common/NavigationBar';
import CollectionsBar from './CollectionsBar';
import collectionsService, { Collection } from '../services/collectionsService';
import { useAppSelector } from '../utils/hooks';

const Collections: React.FC = () => {
  const [collections, setCollections] = useState<Collection[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [selectedCollection, setSelectedCollection] = useState<Collection | null>(null);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  
  const [isCollectionsSidebarCollapsed, setIsCollectionsSidebarCollapsed] = useState(
    localStorage.getItem('collectionsSidebarCollapsed') === 'true'
  );
  
  const navigate = useNavigate();
  const { user } = useAppSelector((state) => state.auth);

  useEffect(() => {
    loadCollections();
  }, []);

  const loadCollections = async () => {
    try {
      setLoading(true);
      const userCollections = await collectionsService.getUserCollections();
      setCollections(userCollections);
    } catch (err) {
      setError('Failed to load collections');
      console.error('Error loading collections:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleCreateCollection = () => {
    navigate('/my-collections/create');
  };

  const handleEditCollection = (collection: Collection) => {
    navigate(`/my-collections/${collection.id}/edit`);
  };

  const handleDeleteCollection = (collection: Collection) => {
    setSelectedCollection(collection);
    setShowDeleteModal(true);
  };

  const handleTogglePin = async (collection: Collection) => {
    try {
      await collectionsService.toggleFavorite(collection.id, !collection.favorite);
      // Reload collections to reflect the change
      await loadCollections();
    } catch (err) {
      console.error('Error toggling pin:', err);
      // You could add a toast notification here
    }
  };

  const confirmDeleteCollection = async () => {
    if (!selectedCollection) return;
    
    try {
      await collectionsService.deleteCollection(selectedCollection.id);
      setCollections(collections.filter(c => c.id !== selectedCollection.id));
      setShowDeleteModal(false);
      setSelectedCollection(null);
    } catch (err) {
      console.error('Error deleting collection:', err);
      // You might want to show an error toast here
    }
  };

  const cancelDelete = () => {
    setShowDeleteModal(false);
    setSelectedCollection(null);
  };

  const toggleCollectionsSidebar = () => {
    const newCollapsedState = !isCollectionsSidebarCollapsed;
    setIsCollectionsSidebarCollapsed(newCollapsedState);
    localStorage.setItem('collectionsSidebarCollapsed', newCollapsedState.toString());
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
            title="My Collections"
            showSearch={true}
            showUploadButton={true}
            showHomeLink={true}
          />

          {/* Loading State - Padding for fixed navbar */}
          <div className="flex-1 pt-20">
            <div className="flex items-center justify-center py-12">
              <div className="lg-spinner h-12 w-12"></div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen flex flex-col">
      {/* Navigation Bar - Fixed at top */}
      <NavigationBar
        title="My Collections"
        showSearch={true}
        showUploadButton={true}
        showHomeLink={true}
      />

      {/* Main Content Container */}
      <div className="flex flex-1 pt-20">
        {/* Collections Sidebar */}
        <CollectionsBar 
          isCollapsed={isCollectionsSidebarCollapsed}
          onToggle={toggleCollectionsSidebar}
        />
        
        {/* Main Content Area */}
        <div className="flex-1 transition-all duration-300 min-w-0">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {error ? (
          <div className="text-center py-12">
            <div className="lg-card p-4 max-w-md mx-auto">
              <p className="text-red-400">{error}</p>
              <button 
                onClick={loadCollections}
                className="lg-button lg-button-primary mt-2"
              >
                Try again
              </button>
            </div>
          </div>
        ) : collections.length === 0 ? (
          <div className="text-center py-12">
            <FolderIcon className="w-16 h-16 mx-auto mb-4" style={{ color: 'var(--lg-text-tertiary)' }} />
            <h2 className="text-xl font-semibold mb-2" style={{ color: 'var(--lg-text-primary)' }}>No collections yet</h2>
            <p className="mb-6 max-w-md mx-auto" style={{ color: 'var(--lg-text-secondary)' }}>
              Collections help you organize your 3D models. Create your first collection to get started.
            </p>
            <button
              onClick={handleCreateCollection}
              className="lg-button lg-button-primary inline-flex items-center"
            >
              <PlusIcon className="w-5 h-5 mr-2" />
              Create Collection
            </button>
          </div>
        ) : (
          <>
            {/* Collections Grid */}
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
              {collections.map((collection) => (
                <CollectionCard
                  key={collection.id}
                  collection={collection}
                  onEdit={handleEditCollection}
                  onDelete={handleDeleteCollection}
                  onTogglePin={handleTogglePin}
                />
              ))}
            </div>
          </>
        )}
          </div>
        </div>
      </div>

      {/* Delete Confirmation Modal */}
      {showDeleteModal && selectedCollection && (
        <div className="lg-modal-overlay">
          <div className="lg-modal relative top-20 mx-auto p-5 w-96">
            <div className="mt-3 text-center">
              <div className="mx-auto flex items-center justify-center h-12 w-12 rounded-full" style={{ background: 'rgba(239, 68, 68, 0.2)' }}>
                <svg className="h-6 w-6" style={{ color: 'var(--lg-error)' }} fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z" />
                </svg>
              </div>
              <h3 className="text-lg font-medium mt-2" style={{ color: 'var(--lg-text-primary)' }}>Delete Collection</h3>
              <div className="mt-2 px-7 py-3">
                <p className="text-sm" style={{ color: 'var(--lg-text-secondary)' }}>
                  Are you sure you want to delete "{selectedCollection.name}"? This action cannot be undone.
                </p>
              </div>
              <div className="flex gap-4 justify-center mt-4">
                <button
                  onClick={cancelDelete}
                  className="lg-button"
                >
                  Cancel
                </button>
                <button
                  onClick={confirmDeleteCollection}
                  className="lg-button"
                  style={{ background: 'var(--lg-error)', borderColor: 'var(--lg-error)', color: 'white' }}
                >
                  Delete
                </button>
              </div>
          </div>
        </div>
      </div>
      )}
    </div>
  );
};

export default Collections; 