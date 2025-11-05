import React from 'react';
import ModelCard from './ModelCard';
import { ExtendedModel } from '../services/modelsService';
import { useUserSettings } from '../context/UserSettingsContext';

interface ModelGridProps {
  models: ExtendedModel[];
  loading?: boolean;
  error?: string;
  onModelClick?: (model: ExtendedModel) => void;
  onLoadMore?: () => void;
  hasMore?: boolean;
  loadingMore?: boolean;
  className?: string;
  selectedModels?: Set<string>;
  onModelSelect?: (modelId: string) => void;
  showSelection?: boolean;
}

const ModelGrid: React.FC<ModelGridProps> = ({
  models,
  loading = false,
  error,
  onModelClick,
  onLoadMore,
  hasMore = false,
  loadingMore = false,
  className = '',
  selectedModels = new Set(),
  onModelSelect,
  showSelection = false
}) => {
  const { settings } = useUserSettings();
  
  // Get layout settings from user settings
  const viewType = settings?.dashboardViewType || 'grid';
  const gridColumns = settings?.gridColumns || 4;
  const cardSpacing = settings?.cardSpacing || 'normal';
  
  // Calculate grid classes and styles based on settings
  const getGridClasses = () => {
    const spacingClass = cardSpacing === 'compact' ? 'gap-2 sm:gap-3' : 
                        cardSpacing === 'spacious' ? 'gap-6 sm:gap-8' : 
                        'gap-4 sm:gap-6';
    
    if (viewType === 'list') {
      return { className: `grid grid-cols-1 ${spacingClass}` };
    }
    
    // Grid view with dynamic columns
    let columnClass = 'grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4';
    
    // Add responsive column classes based on user preference
    if (gridColumns >= 5) {
      columnClass += ' xl:grid-cols-5';
    }
    if (gridColumns >= 6) {
      columnClass += ' 2xl:grid-cols-6';
    }
    

    
    // Use inline styles as fallback for custom column counts
    const gridStyle: React.CSSProperties = {};
    if (gridColumns > 4) {
      // Apply custom grid columns for larger screens
      gridStyle['--grid-columns-lg'] = Math.min(gridColumns, 4).toString();
      gridStyle['--grid-columns-xl'] = Math.min(gridColumns, 5).toString();
      gridStyle['--grid-columns-2xl'] = gridColumns.toString();
    }
    
    return { 
      className: `grid ${columnClass} ${spacingClass}`,
      style: gridStyle
    };
  };
  
  // Loading skeleton component
  const LoadingSkeleton = () => (
    <div className="lg-card overflow-hidden animate-pulse w-full h-72 sm:h-80 flex flex-col">
      <div className="h-40 sm:h-48 bg-white/10 flex-shrink-0" />
      <div className="p-3 sm:p-4 flex flex-col flex-1 min-h-0">
        <div className="h-4 bg-white/10 rounded mb-2 min-h-[2.5rem]" />
        <div className="h-3 bg-white/10 rounded w-2/3 mb-2 sm:mb-3 flex-shrink-0" />
        <div className="flex justify-between flex-shrink-0">
          <div className="h-3 bg-white/10 rounded w-12" />
          <div className="h-3 bg-white/10 rounded w-12" />
          <div className="h-3 bg-white/10 rounded w-12" />
        </div>
        <div className="mt-auto pt-2 sm:pt-3">
          <div className="h-1 bg-white/10 rounded-full" />
        </div>
      </div>
    </div>
  );

  // List view skeleton component
  const ListLoadingSkeleton = () => (
    <div className="lg-card overflow-hidden animate-pulse w-full h-24 flex items-center">
      <div className="h-16 w-16 bg-white/10 rounded mr-4 flex-shrink-0" />
      <div className="flex-1">
        <div className="h-4 bg-white/10 rounded mb-2 w-1/3" />
        <div className="h-3 bg-white/10 rounded w-1/4 mb-2" />
        <div className="flex space-x-4">
          <div className="h-3 bg-white/10 rounded w-12" />
          <div className="h-3 bg-white/10 rounded w-12" />
          <div className="h-3 bg-white/10 rounded w-12" />
        </div>
      </div>
    </div>
  );

  // List view model card component
  const ModelListItem: React.FC<{ model: ExtendedModel }> = ({ model }) => {
    const handleClick = () => {
      if (onModelClick) {
        onModelClick(model);
      }
    };

    const handleSelect = (e: React.MouseEvent) => {
      e.stopPropagation();
      if (onModelSelect) {
        onModelSelect(model.id!);
      }
    };

    const isSelected = selectedModels.has(model.id!);

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

    const totalLikes = (model.likes?.length || 0);
    const authorName = model.author?.username || 'Unknown';

    return (
      <div 
        className={`lg-card cursor-pointer overflow-hidden group w-full h-24 flex items-center transition-all duration-200 hover:bg-white/5 ${
          isSelected ? 'ring-2 ring-blue-500 bg-blue-500/10' : ''
        }`}
        onClick={handleClick}
        data-testid="model-list-item"
      >
        {/* Selection Checkbox */}
        {showSelection && (
          <div className="mr-3 flex-shrink-0" onClick={handleSelect}>
            <input
              type="checkbox"
              checked={isSelected}
              onChange={() => {}} // Controlled by parent
              className="w-4 h-4 text-blue-600 bg-gray-700 border-gray-600 rounded focus:ring-blue-500 focus:ring-2"
            />
          </div>
        )}

        {/* Thumbnail */}
        <div className="h-16 w-16 bg-white/10 rounded mr-4 flex-shrink-0 overflow-hidden">
          {model.thumbnailUrl ? (
            <img
              src={model.thumbnailUrl}
              alt={model.name || 'Model thumbnail'}
              className="w-full h-full object-cover"
            />
          ) : (
            <div className="w-full h-full flex items-center justify-center">
              <svg className="w-6 h-6 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
              </svg>
            </div>
          )}
        </div>

        {/* Content */}
        <div className="flex-1 min-w-0">
          <h3 className="font-semibold text-white text-sm mb-1 truncate">
            {model.name || 'Untitled Model'}
          </h3>
          <p className="text-xs text-white/60 mb-2">
            by {authorName}
          </p>
          <div className="flex items-center space-x-4 text-xs text-white/60">
            <span className="flex items-center space-x-1">
              <svg className="w-3 h-3 fill-current text-red-400" viewBox="0 0 24 24">
                <path d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z"/>
              </svg>
              <span>{formatNumber(totalLikes)}</span>
            </span>
            <span className="flex items-center space-x-1">
              <svg className="w-3 h-3 fill-current text-green-400" viewBox="0 0 24 24">
                <path d="M19 9h-4V3H9v6H5l7 7 7-7zM5 18v2h14v-2H5z"/>
              </svg>
              <span>{formatNumber((model as any).downloads || 0)}</span>
            </span>
            <span className="flex items-center space-x-1">
              <svg className="w-3 h-3 fill-current text-blue-400" viewBox="0 0 24 24">
                <path d="M12 4.5C7 4.5 2.73 7.61 1 12c1.73 4.39 6 7.5 11 7.5s9.27-3.11 11-7.5c-1.73-4.39-6-7.5-11-7.5zM12 17c-2.76 0-5-2.24-5-5s2.24-5 5-5 5 2.24 5 5-2.24 5-5 5zm0-8c-1.66 0-3 1.34-3 3s1.34 3 3 3 3-1.34 3-3-1.34-3-3-3z"/>
              </svg>
              <span>{formatNumber(model.comments?.length || 0)}</span>
            </span>
          </div>
        </div>
      </div>
    );
  };

  // Error state
  if (error) {
    return (
      <div className={`text-center py-12 ${className}`}>
        <div className="lg-card p-6 max-w-md mx-auto border-red-500/30">
          <svg className="w-12 h-12 text-red-400 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <h3 className="text-lg font-medium text-red-400 mb-2">Error Loading Models</h3>
          <p className="text-red-300">{error}</p>
        </div>
      </div>
    );
  }

  // Loading state
  // if (loading) {
  //   const gridProps = getGridClasses();
  //   return (
  //     <div className={`${gridProps.className} ${className}`} style={gridProps.style}>
  //       {Array.from({ length: viewType === 'list' ? 10 : 10 }).map((_, index) => (
  //         viewType === 'list' ? <ListLoadingSkeleton key={index} /> : <LoadingSkeleton key={index} />
  //       ))}
  //     </div>
  //   );
  // }

  // Empty state
  if (!loading && models.length === 0) {
    return (
      <div className={`text-center py-12 ${className}`}>
        <div className="max-w-md mx-auto">
          <svg className="w-16 h-16 text-white/40 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
          </svg>
          <h3 className="text-lg font-medium text-white mb-2">No Models Found</h3>
          <p className="text-white/60 mb-6">There are no models to display at the moment.</p>
          <button 
            onClick={() => window.location.reload()}
            className="lg-button lg-button-primary"
          >
            Refresh
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className={className}>
      {/* Model Grid/List */}
      {(() => {
        const gridProps = getGridClasses();
        return (
          <div className={gridProps.className} style={gridProps.style}>
            {models.map((model) => (
              viewType === 'list' ? (
                <ModelListItem key={model.id} model={model} />
              ) : (
                <ModelCard
                  key={model.id}
                  model={model}
                  onClick={onModelClick}
                  isSelected={selectedModels.has(model.id!)}
                  onSelect={onModelSelect}
                  showSelection={showSelection}
                />
              )
            ))}
            
            {/* Loading more skeletons */}
            {/* {loadingMore && Array.from({ length: viewType === 'list' ? 5 : 5 }).map((_, index) => (
              viewType === 'list' ? <ListLoadingSkeleton key={`loading-${index}`} /> : <LoadingSkeleton key={`loading-${index}`} />
            ))} */}
          </div>
        );
      })()}

      {/* Load More Button */}
      {hasMore && !loadingMore && onLoadMore && (
        <div className="text-center mt-8">
          <button
            onClick={onLoadMore}
            className="lg-button lg-button-primary"
          >
            Load More Models
          </button>
        </div>
      )}

      {/* Loading more indicator */}
      {/* {loadingMore && (
        <div className="text-center mt-8">
          <div className="inline-flex items-center">
            <div className="lg-spinner h-5 w-5 mr-3"></div>
            <span className="text-white/80">Loading more models...</span>
          </div>
        </div>
      )} */}
    </div>
  );
};

export default ModelGrid; 