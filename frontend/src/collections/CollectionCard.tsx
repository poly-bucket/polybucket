import React, { useState, useRef, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { EyeIcon, EyeSlashIcon, LinkIcon, LockClosedIcon, BookmarkIcon } from '@heroicons/react/24/outline';
import { BookmarkIcon as BookmarkIconSolid } from '@heroicons/react/24/solid';
import UserAvatar from '../ucp/UserAvatar';
import CollectionAvatar from './CollectionAvatar';
import ClickableUsername from '../components/common/ClickableUsername';
import { Collection } from '../services/collectionsService';

interface CollectionCardProps {
  collection: Collection;
  onEdit?: (collection: Collection) => void;
  onDelete?: (collection: Collection) => void;
  onTogglePin?: (collection: Collection) => void;
  showOwner?: boolean;
}

const CollectionCard: React.FC<CollectionCardProps> = ({ 
  collection, 
  onEdit, 
  onDelete, 
  onTogglePin,
  showOwner = false 
}) => {
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);

  // Close menu when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        setIsMenuOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, []);
  const getCollectionThumbnail = () => {
    // Use the first model's thumbnail if available
    if (collection.collectionModels && collection.collectionModels.length > 0) {
      const firstModel = collection.collectionModels[0].model;
      if (firstModel?.thumbnailUrl) {
        return (
          <img 
            src={firstModel.thumbnailUrl} 
            alt={collection.name}
            className="w-full h-full object-cover transition-transform duration-300 group-hover:scale-105"
          />
        );
      }
    }
    
    // Fallback to a generated avatar-style background
    return (
      <div className="w-full h-full bg-gradient-to-br from-gray-700 to-gray-900 flex items-center justify-center">
        <div className="bg-white/10 backdrop-blur-sm rounded-full p-2 shadow-lg">
          <CollectionAvatar 
            collectionId={collection.id}
            collectionName={collection.name}
            avatar={collection.avatar}
            size="2xl"
            className="w-20 h-20"
          />
        </div>
      </div>
    );
  };

  const getVisibilityIcon = () => {
    switch (collection.visibility) {
      case 'Public':
        return <EyeIcon className="w-4 h-4 text-green-400" title="Public" />;
      case 'Unlisted':
        return (
          <div className="flex items-center">
            <LinkIcon className="w-4 h-4 text-yellow-400" title="Unlisted" />
            {collection.passwordHash && (
              <LockClosedIcon className="w-3 h-3 text-yellow-400 ml-1" title="Password Protected" />
            )}
          </div>
        );
      case 'Private':
      default:
        return <EyeSlashIcon className="w-4 h-4 text-gray-400" title="Private" />;
    }
  };

  return (
    <div className="lg-card cursor-pointer overflow-hidden group w-full h-72 sm:h-80 flex flex-col" data-testid="collection-card">
      {/* Thumbnail Container - Fixed height */}
      <div className="relative h-40 sm:h-48 bg-gradient-to-br from-gray-700 to-gray-900 overflow-hidden flex-shrink-0">
        <Link to={`/my-collections/${collection.id}`} className="block w-full h-full">
          {getCollectionThumbnail()}
        </Link>
        
        {/* Status Badges */}
        <div className="absolute top-2 left-2 flex flex-wrap gap-1">
          {collection.visibility === 'Private' && (
            <span className="lg-badge lg-badge-error text-xs">
              Private
            </span>
          )}
          {collection.visibility === 'Unlisted' && (
            <span className="lg-badge lg-badge-warning text-xs">
              Unlisted
            </span>
          )}
          {collection.visibility === 'Public' && (
            <span className="lg-badge lg-badge-success text-xs">
              Public
            </span>
          )}
        </div>

        {/* Action Menu Overlay */}
        {(onEdit || onDelete || onTogglePin) && (
          <div className="absolute top-2 right-2 opacity-0 group-hover:opacity-100 transition-opacity duration-300">
            <div className="relative" ref={menuRef}>
              <button 
                onClick={() => setIsMenuOpen(!isMenuOpen)}
                className="bg-white/90 hover:bg-white text-gray-700 p-2 rounded-full shadow-md transition-all duration-200 hover:scale-110"
              >
                <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                  <path d="M10 6a2 2 0 110-4 2 2 0 010 4zM10 12a2 2 0 110-4 2 2 0 010 4zM10 18a2 2 0 110-4 2 2 0 010 4z" />
                </svg>
              </button>
              
              {/* Dropdown Menu */}
              {isMenuOpen && (
                <div className="absolute right-0 top-8 w-40 bg-white border border-gray-200 rounded-md shadow-lg z-10">
                  {onTogglePin && (
                    <button
                      onClick={() => {
                        onTogglePin(collection);
                        setIsMenuOpen(false);
                      }}
                      className="flex items-center w-full text-left px-3 py-2 text-sm text-gray-700 hover:bg-gray-50"
                    >
                      {collection.favorite ? (
                        <>
                          <BookmarkIconSolid className="w-4 h-4 mr-2 text-yellow-500" />
                          Unpin from sidebar
                        </>
                      ) : (
                        <>
                          <BookmarkIcon className="w-4 h-4 mr-2" />
                          Pin to sidebar
                        </>
                      )}
                    </button>
                  )}
                  {onEdit && (
                    <button
                      onClick={() => {
                        onEdit(collection);
                        setIsMenuOpen(false);
                      }}
                      className="block w-full text-left px-3 py-2 text-sm text-gray-700 hover:bg-gray-50"
                    >
                      Edit
                    </button>
                  )}
                  {onDelete && (
                    <button
                      onClick={() => {
                        onDelete(collection);
                        setIsMenuOpen(false);
                      }}
                      className="block w-full text-left px-3 py-2 text-sm text-red-600 hover:bg-gray-50"
                    >
                      Delete
                    </button>
                  )}
                </div>
              )}
            </div>
          </div>
        )}
      </div>
      
      {/* Content - Fixed height with flex layout */}
      <div className="p-3 sm:p-4 flex flex-col flex-1 min-h-0">
        {/* Collection Title - Fixed height with ellipsis */}
        <Link 
          to={`/my-collections/${collection.id}`} 
          className="block flex-1 min-w-0"
        >
          <h3 className="font-semibold text-white text-sm mb-1 leading-tight line-clamp-2 min-h-[2.5rem]">
            {collection.name}
          </h3>
        </Link>
        
        {/* Description - Fixed height */}
        {collection.description && (
          <p className="text-xs text-white/60 mb-2 sm:mb-3 line-clamp-2 flex-shrink-0">
            {collection.description}
          </p>
        )}
        
        {/* Meta Info - Fixed height */}
        <div className="flex items-center justify-between text-xs text-white/60 flex-shrink-0">
          {/* Model Count */}
          <div className="flex items-center space-x-1">
            <svg className="w-3 h-3 fill-current text-blue-400" viewBox="0 0 24 24">
              <path d="M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zM9 17H7v-7h2v7zm4 0h-2V7h2v10zm4 0h-2v-4h2v4z"/>
            </svg>
            <span>{collection.collectionModels?.length || 0} models</span>
          </div>
          
          {/* Owner Info */}
          {showOwner && collection.owner && (
            <div className="flex items-center">
              <ClickableUsername
                userId={collection.owner.id}
                username={collection.owner.username}
                avatar={collection.owner.avatar}
                profilePictureUrl={collection.owner.profilePictureUrl}
                size="xs"
                showAvatar={true}
                className="text-white/60 hover:text-blue-400"
              />
            </div>
          )}
        </div>

        {/* Visibility Bar - Fixed height at bottom */}
        <div className="mt-auto pt-2 sm:pt-3">
          <div className="bg-white/20 rounded-full h-1">
            <div 
              className={`h-1 rounded-full transition-all duration-300 ${
                collection.visibility === 'Public' ? 'bg-green-400' :
                collection.visibility === 'Unlisted' ? 'bg-yellow-400' :
                'bg-gray-400'
              }`}
              style={{ width: '100%' }}
            />
          </div>
        </div>
      </div>
    </div>
  );
};

export default CollectionCard; 