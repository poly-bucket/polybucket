import React from 'react';
import { Model } from '../services/api.client';

// Extended model interface to include missing properties from backend
interface ExtendedModel extends Model {
  thumbnailUrl?: string;
  downloads?: number;
}

interface ModelCardProps {
  model: ExtendedModel;
  onClick?: (model: ExtendedModel) => void;
  className?: string;
}

const ModelCard: React.FC<ModelCardProps> = ({ model, onClick, className = '' }) => {
  const handleClick = () => {
    if (onClick) {
      onClick(model);
    }
  };

  const handleLike = (e: React.MouseEvent) => {
    e.stopPropagation();
    // TODO: Implement like functionality
    console.log('Like model:', model.id);
  };

  const formatNumber = (num: number | undefined): string => {
    if (!num) return '0';
    if (num >= 1000000) {
      return (num / 1000000).toFixed(1) + 'M';
    }
    if (num >= 1000) {
      return (num / 1000).toFixed(1) + 'K';
    }
    return num.toString();
  };

  // Get thumbnail URL - using Unsplash images for demo
  const thumbnailUrl = model.thumbnailUrl || 'https://images.unsplash.com/photo-1581833971358-2c8b550f87b3?w=400&h=225&fit=crop';

  // Calculate like percentage for the like bar
  const totalLikes = (model.likes?.length || 0);
  const likePercentage = totalLikes > 0 ? 100 : 0;

  // Get author name
  const authorName = model.author?.username || 'Unknown';

  return (
    <div 
      className={`bg-white rounded-lg shadow-md hover:shadow-lg transition-all duration-300 cursor-pointer overflow-hidden group ${className}`}
      onClick={handleClick}
    >
      {/* Thumbnail Container */}
      <div className="relative aspect-video bg-gray-100 overflow-hidden">
        <img
          src={thumbnailUrl}
          alt={model.name || 'Model thumbnail'}
          className="w-full h-full object-cover transition-transform duration-300 group-hover:scale-105"
          onError={(e) => {
            (e.target as HTMLImageElement).src = 'https://images.unsplash.com/photo-1581833971358-2c8b550f87b3?w=400&h=225&fit=crop';
          }}
        />
        
        {/* Status Badges */}
        <div className="absolute top-2 left-2 flex flex-wrap gap-1">
          {model.wip && (
            <span className="bg-yellow-500 text-white text-xs px-2 py-1 rounded-full font-medium">
              WIP
            </span>
          )}
          {model.aiGenerated && (
            <span className="bg-purple-500 text-white text-xs px-2 py-1 rounded-full font-medium">
              AI
            </span>
          )}
          {model.nsfw && (
            <span className="bg-red-500 text-white text-xs px-2 py-1 rounded-full font-medium">
              NSFW
            </span>
          )}
        </div>

        {/* Like Button Overlay */}
        <div className="absolute top-2 right-2 opacity-0 group-hover:opacity-100 transition-opacity duration-300">
          <button
            onClick={handleLike}
            className="bg-white bg-opacity-90 hover:bg-opacity-100 text-red-500 p-2 rounded-full shadow-md transition-all duration-200 hover:scale-110"
            title="Like this model"
          >
            <svg className="w-4 h-4 fill-current" viewBox="0 0 24 24">
              <path d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z"/>
            </svg>
          </button>
        </div>
      </div>

      {/* Content */}
      <div className="p-4">
        {/* Model Title */}
        <h3 className="font-semibold text-gray-900 text-sm mb-1 line-clamp-2 leading-tight">
          {model.name || 'Untitled Model'}
        </h3>

        {/* Author */}
        <p className="text-xs text-gray-600 mb-3">
          by {authorName}
        </p>

        {/* Stats Row */}
        <div className="flex items-center justify-between text-xs text-gray-500">
          {/* Likes */}
          <div className="flex items-center space-x-1">
            <svg className="w-3 h-3 fill-current text-red-500" viewBox="0 0 24 24">
              <path d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z"/>
            </svg>
            <span>{formatNumber(totalLikes)}</span>
          </div>

          {/* Downloads */}
          <div className="flex items-center space-x-1">
            <svg className="w-3 h-3 fill-current text-green-500" viewBox="0 0 24 24">
              <path d="M19 9h-4V3H9v6H5l7 7 7-7zM5 18v2h14v-2H5z"/>
            </svg>
            <span>{formatNumber(model.downloads || 0)}</span>
          </div>

          {/* Views or Comments count */}
          <div className="flex items-center space-x-1">
            <svg className="w-3 h-3 fill-current text-blue-500" viewBox="0 0 24 24">
              <path d="M12 4.5C7 4.5 2.73 7.61 1 12c1.73 4.39 6 7.5 11 7.5s9.27-3.11 11-7.5c-1.73-4.39-6-7.5-11-7.5zM12 17c-2.76 0-5-2.24-5-5s2.24-5 5-5 5 2.24 5 5-2.24 5-5 5zm0-8c-1.66 0-3 1.34-3 3s1.34 3 3 3 3-1.34 3-3-1.34-3-3-3z"/>
            </svg>
            <span>{formatNumber(model.comments?.length || 0)}</span>
          </div>
        </div>

        {/* Like Bar (similar to Thingiverse) */}
        {totalLikes > 0 && (
          <div className="mt-3">
            <div className="bg-gray-200 rounded-full h-1">
              <div 
                className="bg-red-500 h-1 rounded-full transition-all duration-300"
                style={{ width: `${likePercentage}%` }}
              />
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default ModelCard; 