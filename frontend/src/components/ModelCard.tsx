import React from 'react';
import { ExtendedModel } from '../services/modelsService';
import { useAuth } from '../context/AuthContext';
import { useUserSettings } from '../context/UserSettingsContext';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faTools, faExclamationTriangle, faMagicWandSparkles } from '@fortawesome/free-solid-svg-icons';
import ClickableUsername from './common/ClickableUsername';

interface ModelCardProps {
  model: ExtendedModel;
  onClick?: (model: ExtendedModel) => void;
  className?: string;
}

const ModelCard: React.FC<ModelCardProps> = ({ model, onClick, className = '' }) => {
  const { user } = useAuth();
  const { settings } = useUserSettings();
  
  // Get auto-rotate setting from user settings, default to true
  const autoRotate = settings?.autoRotateModels ?? true;
  
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

  const handleDragStart = (e: React.DragEvent) => {
    e.stopPropagation();
    
    // Set drag data
    e.dataTransfer.setData('application/json', JSON.stringify({
      type: 'model',
      modelId: model.id,
      modelName: model.name,
      thumbnailUrl: model.thumbnailUrl
    }));
    
    // Set drag image (optional - shows a preview while dragging)
    if (e.currentTarget instanceof HTMLElement) {
      const dragImage = e.currentTarget.cloneNode(true) as HTMLElement;
      dragImage.style.opacity = '0.7';
      dragImage.style.transform = 'scale(0.8) rotate(5deg)';
      dragImage.style.pointerEvents = 'none';
      dragImage.style.position = 'absolute';
      dragImage.style.top = '-1000px';
      dragImage.style.zIndex = '9999';
      dragImage.style.width = '200px';
      dragImage.style.height = '200px';
      dragImage.style.overflow = 'hidden';
      
      document.body.appendChild(dragImage);
      
      e.dataTransfer.setDragImage(dragImage, 0, 0);
      
      // Clean up the drag image after a short delay
      setTimeout(() => {
        if (document.body.contains(dragImage)) {
          document.body.removeChild(dragImage);
        }
      }, 100);
    }
    
    // Add visual feedback using CSS classes
    e.currentTarget.classList.add('drag-start');
  };

  const handleDragEnd = (e: React.DragEvent) => {
    // Remove visual feedback and add end animation
    e.currentTarget.classList.remove('drag-start');
    e.currentTarget.classList.add('drag-end');
    
    // Remove the end animation class after animation completes
    setTimeout(() => {
      e.currentTarget.classList.remove('drag-end');
    }, 200);
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

  // Get thumbnail URL
  const thumbnailUrl = model.thumbnailUrl;

  // Calculate like percentage for the like bar
  const totalLikes = (model.likes?.length || 0);
  const likePercentage = totalLikes > 0 ? 100 : 0;

  // Get author name
  const authorName = model.author?.username || 'Unknown';

  return (
    <div 
      className={`lg-card cursor-pointer overflow-hidden group w-full h-72 sm:h-80 flex flex-col transition-all duration-200 ${className}`}
      onClick={handleClick}
      draggable={true}
      onDragStart={handleDragStart}
      onDragEnd={handleDragEnd}
      data-testid="model-card"
      title={`Drag to add to collection • ${model.name}`}
    >
      {/* Thumbnail Container - Fixed height */}
      <div className="relative h-40 sm:h-48 bg-white/10 overflow-hidden flex-shrink-0">
        {thumbnailUrl ? (
          <img
            src={thumbnailUrl}
            alt={model.name || 'Model thumbnail'}
            className="w-full h-full object-cover transition-transform duration-300 group-hover:scale-105"
            onError={(e) => {
              (e.target as HTMLImageElement).style.display = 'none';
              // Show placeholder when image fails to load
              const container = e.currentTarget.parentElement;
              if (container) {
                container.innerHTML = `
                  <div class="w-full h-full flex items-center justify-center bg-gray-800">
                    <div class="text-center">
                      <svg class="w-12 h-12 text-gray-400 mx-auto mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
                      </svg>
                      <div class="text-gray-400 text-sm">No preview</div>
                    </div>
                  </div>
                `;
              }
            }}
          />
        ) : (
          <div className="w-full h-full flex items-center justify-center bg-gray-800">
            <div className="text-center">
              <svg className="w-12 h-12 text-gray-400 mx-auto mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
              </svg>
              <div className="text-gray-400 text-sm">No preview</div>
            </div>
          </div>
        )}
        
        {/* Status Badges */}
        <div className="absolute top-2 left-2 flex flex-wrap gap-1">
          {model.wip && (
            <span className="lg-status-badge lg-badge-wip">
              <FontAwesomeIcon icon={faTools} />
            </span>
          )}
          {model.aiGenerated && (
            <span className="lg-badge-ai">
              <span className="lg-badge-ai-content">
                <FontAwesomeIcon icon={faMagicWandSparkles} />
              </span>
            </span>
          )}
          {model.nsfw && (
            <span className="lg-status-badge lg-badge-nsfw">
              <FontAwesomeIcon icon={faExclamationTriangle} />
            </span>
          )}
        </div>

        {/* Drag Handle - Subtle indicator */}
        <div className="absolute bottom-2 right-2 opacity-0 group-hover:opacity-100 transition-opacity duration-300">
          <div className="bg-white/20 backdrop-blur-sm p-1.5 rounded-full">
            <svg className="w-3 h-3 text-white/60" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 8h16M4 16h16" />
            </svg>
          </div>
        </div>

        {/* Like Button Overlay */}
        <div className="absolute top-2 right-2 opacity-0 group-hover:opacity-100 transition-opacity duration-300">
          <button
            onClick={handleLike}
            className="bg-white/90 hover:bg-white text-red-500 p-2 rounded-full shadow-md transition-all duration-200 hover:scale-110"
            title="Like this model"
          >
            <svg className="w-4 h-4 fill-current" viewBox="0 0 24 24">
              <path d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z"/>
            </svg>
          </button>
        </div>
      </div>

      {/* Content - Fixed height with flex layout */}
      <div className="p-3 sm:p-4 flex flex-col flex-1 min-h-0">
        {/* Model Title - Fixed height with ellipsis */}
        <h3 className="font-semibold text-white text-sm mb-1 leading-tight line-clamp-2 min-h-[2.5rem]">
          {model.name || 'Untitled Model'}
        </h3>

        {/* Author - Fixed height */}
        <div className="text-xs text-white/60 mb-2 sm:mb-3 flex-shrink-0">
          by{' '}
          <ClickableUsername
            userId={model.author?.id || ''}
            username={authorName}
            avatar={model.author?.avatar}
            profilePictureUrl={model.author?.profilePictureUrl}
            className="text-white/80 hover:text-blue-400"
          />
        </div>

        {/* Stats Row - Fixed height */}
        <div className="flex items-center justify-between text-xs text-white/60 flex-shrink-0">
          {/* Likes */}
          <div className="flex items-center space-x-1">
            <svg className="w-3 h-3 fill-current text-red-400" viewBox="0 0 24 24">
              <path d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z"/>
            </svg>
            <span>{formatNumber(totalLikes)}</span>
          </div>

          {/* Downloads */}
          <div className="flex items-center space-x-1">
            <svg className="w-3 h-3 fill-current text-green-400" viewBox="0 0 24 24">
              <path d="M19 9h-4V3H9v6H5l7 7 7-7zM5 18v2h14v-2H5z"/>
            </svg>
            <span>{formatNumber((model as any).downloads || 0)}</span>
          </div>

          {/* Views or Comments count */}
          <div className="flex items-center space-x-1">
            <svg className="w-3 h-3 fill-current text-blue-400" viewBox="0 0 24 24">
              <path d="M12 4.5C7 4.5 2.73 7.61 1 12c1.73 4.39 6 7.5 11 7.5s9.27-3.11 11-7.5c-1.73-4.39-6-7.5-11-7.5zM12 17c-2.76 0-5-2.24-5-5s2.24-5 5-5 5 2.24 5 5-2.24 5-5 5zm0-8c-1.66 0-3 1.34-3 3s1.34 3 3 3 3-1.34 3-3-1.34-3-3-3z"/>
            </svg>
            <span>{formatNumber(model.comments?.length || 0)}</span>
          </div>
        </div>

        {/* Like Bar - Fixed height at bottom */}
        <div className="mt-auto pt-2 sm:pt-3">
          {totalLikes > 0 && (
            <div className="bg-white/20 rounded-full h-1">
              <div 
                className="bg-red-400 h-1 rounded-full transition-all duration-300"
                style={{ width: `${likePercentage}%` }}
              />
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default ModelCard; 