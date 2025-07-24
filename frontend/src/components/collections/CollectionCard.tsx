import React from 'react';
import { Link } from 'react-router-dom';
import { EyeIcon, EyeSlashIcon, LinkIcon, LockClosedIcon } from '@heroicons/react/24/outline';
import UserAvatar from '../UserAvatar';
import CollectionAvatar from '../CollectionAvatar';
import { Collection } from '../../services/collectionsService';

interface CollectionCardProps {
  collection: Collection;
  onEdit?: (collection: Collection) => void;
  onDelete?: (collection: Collection) => void;
  showOwner?: boolean;
}

const CollectionCard: React.FC<CollectionCardProps> = ({ 
  collection, 
  onEdit, 
  onDelete, 
  showOwner = false 
}) => {
  const getCollectionThumbnail = () => {
    // Use the first model's thumbnail if available
    if (collection.collectionModels && collection.collectionModels.length > 0) {
      const firstModel = collection.collectionModels[0].model;
      if (firstModel?.thumbnailUrl) {
        return (
          <img 
            src={firstModel.thumbnailUrl} 
            alt={collection.name}
            className="w-full h-48 object-cover"
          />
        );
      }
    }
    
    // Fallback to a generated avatar-style background
    return (
      <div className="w-full h-48 bg-gradient-to-br from-indigo-500 to-purple-600 flex items-center justify-center">
        <CollectionAvatar 
          collectionId={collection.id}
          collectionName={collection.name}
          avatar={collection.avatar}
          size="lg"
          className="w-16 h-16"
        />
      </div>
    );
  };

  const getVisibilityIcon = () => {
    switch (collection.visibility) {
      case 'Public':
        return <EyeIcon className="w-4 h-4 text-green-600" title="Public" />;
      case 'Unlisted':
        return (
          <div className="flex items-center">
            <LinkIcon className="w-4 h-4 text-yellow-600" title="Unlisted" />
            {collection.passwordHash && (
              <LockClosedIcon className="w-3 h-3 text-yellow-600 ml-1" title="Password Protected" />
            )}
          </div>
        );
      case 'Private':
      default:
        return <EyeSlashIcon className="w-4 h-4 text-gray-600" title="Private" />;
    }
  };

  return (
    <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden hover:shadow-md transition-shadow">
      {/* Thumbnail */}
      <Link to={`/my-collections/${collection.id}`} className="block">
        {getCollectionThumbnail()}
      </Link>
      
      {/* Content */}
      <div className="p-4">
        {/* Header */}
        <div className="flex items-start justify-between mb-2">
          <Link 
            to={`/my-collections/${collection.id}`} 
            className="block flex-1 min-w-0"
          >
            <h3 className="text-lg font-semibold text-gray-900 truncate hover:text-indigo-600">
              {collection.name}
            </h3>
          </Link>
          
          <div className="flex items-center space-x-1 ml-2">
            {getVisibilityIcon()}
            
            {/* Action Menu */}
            {(onEdit || onDelete) && (
              <div className="relative group">
                <button className="p-1 text-gray-400 hover:text-gray-600">
                  <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                    <path d="M10 6a2 2 0 110-4 2 2 0 010 4zM10 12a2 2 0 110-4 2 2 0 010 4zM10 18a2 2 0 110-4 2 2 0 010 4z" />
                  </svg>
                </button>
                
                {/* Dropdown Menu */}
                <div className="absolute right-0 top-6 w-32 bg-white border border-gray-200 rounded-md shadow-lg z-10 opacity-0 group-hover:opacity-100 transition-opacity">
                  {onEdit && (
                    <button
                      onClick={() => onEdit(collection)}
                      className="block w-full text-left px-3 py-2 text-sm text-gray-700 hover:bg-gray-50"
                    >
                      Edit
                    </button>
                  )}
                  {onDelete && (
                    <button
                      onClick={() => onDelete(collection)}
                      className="block w-full text-left px-3 py-2 text-sm text-red-600 hover:bg-gray-50"
                    >
                      Delete
                    </button>
                  )}
                </div>
              </div>
            )}
          </div>
        </div>
        
        {/* Description */}
        {collection.description && (
          <p className="text-sm text-gray-600 mb-3 line-clamp-2">
            {collection.description}
          </p>
        )}
        
        {/* Meta Info */}
        <div className="flex items-center justify-between text-xs text-gray-500">
          <span>
            {collection.collectionModels?.length || 0} models
          </span>
          
          {showOwner && collection.owner && (
            <div className="flex items-center">
              <UserAvatar 
                userId={collection.owner.id}
                username={collection.owner.username}
                avatar={collection.owner.avatar}
                profilePictureUrl={collection.owner.profilePictureUrl}
                size="xs"
                className="w-4 h-4 mr-1"
              />
              <span>{collection.owner.username}</span>
            </div>
          )}
          
          <span className={`px-2 py-1 rounded-full text-xs ${
            collection.visibility === 'Public' ? 'bg-green-100 text-green-800' :
            collection.visibility === 'Unlisted' ? 'bg-yellow-100 text-yellow-800' :
            'bg-gray-100 text-gray-800'
          }`}>
            {collection.visibility}
          </span>
        </div>
      </div>
    </div>
  );
};

export default CollectionCard; 