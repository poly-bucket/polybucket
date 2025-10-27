import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { ChevronLeftIcon, ChevronRightIcon, PlusIcon, FolderIcon, CheckIcon, StarIcon } from '@heroicons/react/24/outline';
import { StarIcon as StarIconSolid } from '@heroicons/react/24/solid';
import UserAvatar from '../ucp/UserAvatar';
import CollectionAvatar from './CollectionAvatar';
import collectionsService, { Collection } from '../services/collectionsService';

interface CollectionsBarProps {
  isCollapsed: boolean;
  onToggle: () => void;
}

interface DragData {
  type: string;
  modelId: string;
  modelName: string;
  thumbnailUrl?: string;
}

const CollectionsBar: React.FC<CollectionsBarProps> = ({ isCollapsed, onToggle }) => {
  const [collections, setCollections] = useState<Collection[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [isMounted, setIsMounted] = useState(false);
  const [dragOverCollection, setDragOverCollection] = useState<string | null>(null);
  const [dropSuccess, setDropSuccess] = useState<string | null>(null);
  const [dropError, setDropError] = useState<string | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    loadCollections();
    // Add a small delay for the entrance animation
    const timer = setTimeout(() => setIsMounted(true), 100);
    return () => clearTimeout(timer);
  }, []);

  const loadCollections = async () => {
    try {
      setLoading(true);
      const favoriteCollections = await collectionsService.getFavoriteCollections();
      setCollections(favoriteCollections);
    } catch (err) {
      setError('Failed to load favorite collections');
      console.error('Error loading favorite collections:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleDragOver = (e: React.DragEvent, collectionId: string) => {
    e.preventDefault();
    e.stopPropagation();
    
    // Check if this is a model being dragged
    try {
      const data = JSON.parse(e.dataTransfer.getData('application/json'));
      if (data.type === 'model') {
        setDragOverCollection(collectionId);
      }
    } catch {
      // Invalid data, ignore
    }
  };

  const handleDragLeave = (e: React.DragEvent, collectionId: string) => {
    e.preventDefault();
    e.stopPropagation();
    
    // Only remove drag over state if we're actually leaving the collection element
    if (!e.currentTarget.contains(e.relatedTarget as Node)) {
      setDragOverCollection(null);
    }
  };

  const handleDrop = async (e: React.DragEvent, collectionId: string) => {
    e.preventDefault();
    e.stopPropagation();
    
    setDragOverCollection(null);
    
    try {
      const data: DragData = JSON.parse(e.dataTransfer.getData('application/json'));
      
      if (data.type === 'model') {
        // Add model to collection
        await collectionsService.addModelToCollection(collectionId, data.modelId);
        
        // Show success feedback
        setDropSuccess(collectionId);
        
        // Reload collections to show updated model count
        await loadCollections();
        
        // Hide success feedback after animation
        setTimeout(() => setDropSuccess(null), 2000);
      }
    } catch (err) {
      console.error('Error adding model to collection:', err);
      
      // Show error feedback
      setDropError(collectionId);
      
      // Hide error feedback after animation
      setTimeout(() => setDropError(null), 3000);
    }
  };

  const getCollectionIcon = (collection: Collection) => {
    
    // First, try to use the collection's own avatar if it exists
    if (collection.avatar) {
      // Check if the avatar is SVG content (starts with <svg)
      if (collection.avatar.trim().startsWith('<svg')) {
        return (
          <div 
            className="w-8 h-8 rounded-md overflow-hidden"
            dangerouslySetInnerHTML={{ __html: collection.avatar }}
          />
        );
      }
      
      // If it's a URL, use it as an image source
      if (collection.avatar.startsWith('http') || collection.avatar.startsWith('/')) {
        return (
          <img 
            src={collection.avatar} 
            alt={collection.name}
            className="w-8 h-8 rounded-md object-cover"
          />
        );
      }
      
      // If it's neither SVG nor URL, it might be malformed data
      console.warn('Invalid avatar format for collection:', collection.id, collection.avatar);
    }
    
    // Use the collection's first model as thumbnail if available
    // if (collection.collectionModels && collection.collectionModels.length > 0) {
    //   const firstModel = collection.collectionModels[0];
    //   if (firstModel.model && firstModel.model.thumbnailUrl) {
    //     return (
    //       <img 
    //         src={firstModel.model.thumbnailUrl} 
    //         alt={collection.name}
    //         className="w-8 h-8 rounded-md object-cover"
    //       />
    //     );
    //   }
    // }
    
    // Fallback to CollectionAvatar with collection ID - generate a unique avatar
    return (
      <div className={`w-8 h-8 flex items-center justify-center text-white text-xs font-semibold ${
        // Generate different colors based on collection ID for visual variety
        collection.id.charCodeAt(0) % 4 === 0 ? 'bg-gradient-to-br from-indigo-500 to-purple-600' :
        collection.id.charCodeAt(0) % 4 === 1 ? 'bg-gradient-to-br from-emerald-500 to-teal-600' :
        collection.id.charCodeAt(0) % 4 === 2 ? 'bg-gradient-to-br from-orange-500 to-red-600' :
        'bg-gradient-to-br from-pink-500 to-rose-600'
      }`}>
        {collection.name.charAt(0).toUpperCase()}
      </div>
    );
  };

  const handleCreateCollection = () => {
    navigate('/my-collections/create');
  };

  const handleViewAllCollections = () => {
    navigate('/my-collections');
  };

  const handleToggleFavorite = async (collectionId: string, currentFavorite: boolean) => {
    try {
      await collectionsService.toggleFavorite(collectionId, !currentFavorite);
      // Reload collections to reflect the change
      await loadCollections();
    } catch (err) {
      console.error('Error toggling favorite:', err);
      // You could add a toast notification here
    }
  };

  const baseClasses = "lg-sidebar absolute left-0 top-0 h-full z-40 flex flex-col transition-all duration-300 ease-out";
  const mountAnimation = isMounted ? "translate-x-0" : "-translate-x-full";
  const slideAnimation = isCollapsed ? "-translate-x-2" : "translate-x-0";

  if (isCollapsed) {
    return (
      <div className={`${baseClasses} w-12 ${mountAnimation} ${slideAnimation}`}>
        {/* Toggle Button */}
        <button
          onClick={onToggle}
          className="p-3 hover:bg-white/10 border-b border-white/10 transition-colors duration-200 group"
          title="Expand Collections"
        >
          <ChevronRightIcon className="w-5 h-5 text-white/60 group-hover:text-white/80 transition-colors duration-200" />
        </button>
        
        {/* Collapsed Collections Icons */}
        <div className="flex-1 overflow-y-auto py-2 space-y-1">
          {collections.slice(0, 8).map((collection) => (
            <div
              key={collection.id}
              className={`relative mx-1 collection-drop-zone ${
                dragOverCollection === collection.id 
                  ? 'drag-over' 
                  : ''
              } ${
                dropSuccess === collection.id 
                  ? 'drop-success' 
                  : ''
              } ${
                dropError === collection.id 
                  ? 'drop-error' 
                  : ''
              }`}
              onDragOver={(e) => handleDragOver(e, collection.id)}
              onDragLeave={(e) => handleDragLeave(e, collection.id)}
              onDrop={(e) => handleDrop(e, collection.id)}
            >
              <Link
                to={`/my-collections/${collection.id}`}
                className="block p-2 group"
                title={collection.name}
              >
                <div className="flex items-center justify-center group-hover:scale-110 transition-transform duration-200">
                  {getCollectionIcon(collection)}
                </div>
              </Link>
              
              {/* Favorite Star Indicator */}
              {collection.favorite && (
                <div className="absolute top-1 right-1">
                  <StarIconSolid className="w-3 h-3 text-yellow-400" />
                </div>
              )}
              
              {/* Drop Success Indicator */}
              {dropSuccess === collection.id && (
                <div className="absolute inset-0 flex items-center justify-center rounded-lg animate-pulse">
                  <CheckIcon className="w-4 h-4 text-green-400" />
                </div>
              )}
              
              {/* Drop Error Indicator */}
              {dropError === collection.id && (
                <div className="absolute inset-0 flex items-center justify-center rounded-lg animate-pulse">
                  <svg className="w-4 h-4 text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </div>
              )}
            </div>
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
          className="p-3 hover:bg-white/10 border-t border-white/10 transition-colors duration-200 group"
          title="Create Collection"
        >
          <PlusIcon className="w-5 h-5 text-white/60 group-hover:text-white/80 transition-colors duration-200" />
        </button>
      </div>
    );
  }

  return (
    <div className={`${baseClasses} w-64 ${mountAnimation} ${slideAnimation}`}>
      {/* Header */}
      <div className="flex items-center justify-between p-4 border-b border-white/10">
        <div className="flex-1">
          <h2 className="text-lg font-semibold text-white">Favorite Collections</h2>
          <p className="text-xs text-white/60 mt-1">
            Your pinned collections - drag models here to add them
          </p>
        </div>
        <button
          onClick={onToggle}
          className="p-2 hover:bg-white/10 rounded-lg transition-all duration-200 group ml-4"
          title="Collapse Collections"
        >
          <ChevronLeftIcon className="w-5 h-5 text-white/60 group-hover:text-white/80 transition-colors duration-200" />
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
              className="mt-2 text-sm text-indigo-400 hover:text-indigo-300 transition-colors duration-200"
            >
              Try again
            </button>
          </div>
        ) : collections.length === 0 ? (
          <div className="text-center py-8 text-white/60">
            <FolderIcon className="w-12 h-12 mx-auto mb-4 text-white/30" />
            <p className="text-sm">No favorite collections yet</p>
            <p className="text-xs text-white/40 mt-1 mb-3">
              Pin collections to see them here
            </p>
            <button
              onClick={handleViewAllCollections}
              className="mt-2 text-sm text-indigo-400 hover:text-indigo-300 transition-colors duration-200"
            >
              View all collections
            </button>
          </div>
        ) : (
          <div className="space-y-2">
            {collections.map((collection) => (
              <div
                key={collection.id}
                className={`relative collection-drop-zone ${
                  dragOverCollection === collection.id 
                    ? 'drag-over' 
                    : ''
                } ${
                  dropSuccess === collection.id 
                    ? 'drop-success' 
                    : ''
                } ${
                  dropError === collection.id 
                    ? 'drop-error' 
                    : ''
                }`}
                onDragOver={(e) => handleDragOver(e, collection.id)}
                onDragLeave={(e) => handleDragLeave(e, collection.id)}
                onDrop={(e) => handleDrop(e, collection.id)}
              >
                <Link
                  to={`/my-collections/${collection.id}`}
                  className="flex items-center p-3 transition-all duration-200 group"
                >
                  <div className="mr-3">
                    <div className="group-hover:scale-110 transition-transform duration-200">
                      {getCollectionIcon(collection)}
                    </div>
                  </div>
                  <div className="flex-1 min-w-0">
                    <h3 className="text-sm font-medium text-white truncate group-hover:text-white/90 transition-colors duration-200">
                      {collection.name}
                    </h3>
                    <p className="text-xs text-white/60 group-hover:text-white/70 transition-colors duration-200">
                      {collection.collectionModels?.length || 0} models
                    </p>
                  </div>
                  <div className="flex-shrink-0 flex items-center space-x-2">
                    <span className={`inline-block w-2 h-2 rounded-full transition-all duration-200 ${
                      collection.visibility === 'Public' ? 'bg-green-400' :
                      collection.visibility === 'Unlisted' ? 'bg-yellow-400' :
                      'bg-white/40'
                    }`} title={collection.visibility} />
                    
                    <button
                      onClick={(e) => {
                        e.preventDefault();
                        e.stopPropagation();
                        handleToggleFavorite(collection.id, collection.favorite || false);
                      }}
                      className="p-1 hover:bg-white/10 rounded transition-colors duration-200 group"
                      title={collection.favorite ? "Remove from favorites" : "Add to favorites"}
                    >
                      {collection.favorite ? (
                        <StarIconSolid className="w-4 h-4 text-yellow-400 group-hover:text-yellow-300" />
                      ) : (
                        <StarIcon className="w-4 h-4 text-white/40 group-hover:text-white/60" />
                      )}
                    </button>
                  </div>
                </Link>
                
                {/* Drop Success Indicator */}
                {dropSuccess === collection.id && (
                  <div className="absolute inset-0 flex items-center justify-center rounded-lg animate-pulse">
                    <div className="flex items-center space-x-2 text-white px-3 py-1 rounded-full text-sm font-medium">
                      <CheckIcon className="w-4 h-4" />
                      <span>Added!</span>
                    </div>
                  </div>
                )}
                
                {/* Drop Error Indicator */}
                {dropError === collection.id && (
                  <div className="absolute inset-0 flex items-center justify-center rounded-lg animate-pulse">
                    <div className="flex items-center space-x-2 text-white px-3 py-1 rounded-full text-sm font-medium">
                      <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                      </svg>
                      <span>Failed</span>
                    </div>
                  </div>
                )}
                
                {/* Drag Over Indicator */}
                {dragOverCollection === collection.id && (
                  <div className="absolute inset-0 flex items-center justify-center rounded-lg">
                    <div className="text-white text-sm font-medium">
                      Drop to add
                    </div>
                  </div>
                )}
              </div>
            ))}
          </div>
        )}
      </div>
      
      {/* Footer Actions */}
      <div className="border-t border-white/10 p-4 space-y-2">
        <button
          onClick={handleCreateCollection}
          className="lg-button lg-button-primary w-full flex items-center justify-center transition-all duration-200 hover:scale-105"
        >
          <PlusIcon className="w-4 h-4 mr-2" />
          New Collection
        </button>
        
        <button
          onClick={handleViewAllCollections}
          className="lg-button w-full flex items-center justify-center transition-all duration-200 hover:scale-105"
        >
          View All
        </button>
      </div>
    </div>
  );
};

export default CollectionsBar; 