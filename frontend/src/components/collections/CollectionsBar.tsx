import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { ChevronLeftIcon, ChevronRightIcon, PlusIcon, FolderIcon } from '@heroicons/react/24/outline';
import UserAvatar from '../UserAvatar';
import CollectionAvatar from '../CollectionAvatar';
import collectionsService, { Collection } from '../../services/collectionsService';

interface CollectionsBarProps {
  isCollapsed: boolean;
  onToggle: () => void;
}

const CollectionsBar: React.FC<CollectionsBarProps> = ({ isCollapsed, onToggle }) => {
  const [collections, setCollections] = useState<Collection[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const navigate = useNavigate();

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

  const getCollectionIcon = (collection: Collection) => {
    // Use the collection's first model as thumbnail if available
    if (collection.collectionModels && collection.collectionModels.length > 0) {
      const firstModel = collection.collectionModels[0].model;
      if (firstModel?.thumbnailUrl) {
        return (
          <img 
            src={firstModel.thumbnailUrl} 
            alt={collection.name}
            className="w-8 h-8 rounded-md object-cover"
          />
        );
      }
    }
    
    // Fallback to CollectionAvatar with collection ID
    return (
      <CollectionAvatar 
        collectionId={collection.id}
        collectionName={collection.name}
        avatar={collection.avatar}
        size="sm"
        className="w-8 h-8"
      />
    );
  };

  const handleCreateCollection = () => {
    navigate('/my-collections/create');
  };

  const handleViewAllCollections = () => {
    navigate('/my-collections');
  };

  if (isCollapsed) {
    return (
      <div className="lg-sidebar fixed left-0 top-0 h-full w-12 z-40 flex flex-col">
        {/* Toggle Button */}
        <button
          onClick={onToggle}
          className="p-3 hover:bg-white/10 border-b border-white/10"
          title="Expand Collections"
        >
          <ChevronRightIcon className="w-6 h-6 text-white/60" />
        </button>
        
        {/* Collapsed Collections Icons */}
        <div className="flex-1 overflow-y-auto py-2">
          {collections.slice(0, 8).map((collection) => (
                          <Link
                key={collection.id}
                to={`/my-collections/${collection.id}`}
                className="block p-2 hover:bg-white/10"
                title={collection.name}
              >
              {getCollectionIcon(collection)}
            </Link>
          ))}
          
          {collections.length > 8 && (
            <div className="p-2 text-center text-xs text-white/40">
              +{collections.length - 8}
            </div>
          )}
        </div>
        
        {/* Add Collection Button */}
        <button
          onClick={handleCreateCollection}
          className="p-3 hover:bg-white/10 border-t border-white/10"
          title="Create Collection"
        >
          <PlusIcon className="w-6 h-6 text-white/60" />
        </button>
      </div>
    );
  }

  return (
    <div className="lg-sidebar fixed left-0 top-0 h-full w-64 z-40 flex flex-col">
      {/* Header */}
      <div className="flex items-center justify-between p-4 border-b border-white/10">
        <h2 className="text-lg font-semibold text-white">Collections</h2>
        <button
          onClick={onToggle}
          className="p-1 hover:bg-white/10 rounded"
          title="Collapse Collections"
        >
          <ChevronLeftIcon className="w-5 h-5 text-white/60" />
        </button>
      </div>
      
      {/* Collections List */}
      <div className="flex-1 overflow-y-auto p-4">
        {loading ? (
          <div className="flex items-center justify-center py-8">
            <div className="lg-spinner h-8 w-8"></div>
          </div>
        ) : error ? (
          <div className="text-center py-8 text-red-400">
            <p>{error}</p>
            <button 
              onClick={loadCollections}
              className="mt-2 text-sm text-indigo-400 hover:text-indigo-300"
            >
              Try again
            </button>
          </div>
        ) : collections.length === 0 ? (
          <div className="text-center py-8 text-white/60">
            <FolderIcon className="w-12 h-12 mx-auto mb-4 text-white/30" />
            <p className="text-sm">No collections yet</p>
            <button
              onClick={handleCreateCollection}
              className="mt-2 text-sm text-indigo-400 hover:text-indigo-300"
            >
              Create your first collection
            </button>
          </div>
        ) : (
          <div className="space-y-2">
            {collections.map((collection) => (
              <Link
                key={collection.id}
                to={`/my-collections/${collection.id}`}
                className="flex items-center p-3 hover:bg-white/10 rounded-lg transition-colors"
              >
                <div className="mr-3">
                  {getCollectionIcon(collection)}
                </div>
                <div className="flex-1 min-w-0">
                  <h3 className="text-sm font-medium text-white truncate">
                    {collection.name}
                  </h3>
                  <p className="text-xs text-white/60">
                    {collection.collectionModels?.length || 0} models
                  </p>
                </div>
                <div className="flex-shrink-0">
                  <span className={`inline-block w-2 h-2 rounded-full ${
                    collection.visibility === 'Public' ? 'bg-green-400' :
                    collection.visibility === 'Unlisted' ? 'bg-yellow-400' :
                    'bg-white/40'
                  }`} title={collection.visibility} />
                </div>
              </Link>
            ))}
          </div>
        )}
      </div>
      
      {/* Footer Actions */}
      <div className="border-t border-white/10 p-4 space-y-2">
        <button
          onClick={handleCreateCollection}
          className="lg-button lg-button-primary w-full flex items-center justify-center"
        >
          <PlusIcon className="w-4 h-4 mr-2" />
          New Collection
        </button>
        
        <button
          onClick={handleViewAllCollections}
          className="lg-button w-full flex items-center justify-center"
        >
          View All
        </button>
      </div>
    </div>
  );
};

export default CollectionsBar; 