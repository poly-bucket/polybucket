import React from 'react';
import ModelCard from './ModelCard';
import { ExtendedModel } from '../services/modelsService';

interface ModelGridProps {
  models: ExtendedModel[];
  loading?: boolean;
  error?: string;
  onModelClick?: (model: ExtendedModel) => void;
  onLoadMore?: () => void;
  hasMore?: boolean;
  loadingMore?: boolean;
  className?: string;
}

const ModelGrid: React.FC<ModelGridProps> = ({
  models,
  loading = false,
  error,
  onModelClick,
  onLoadMore,
  hasMore = false,
  loadingMore = false,
  className = ''
}) => {
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
  if (loading) {
    return (
      <div className={`grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-4 sm:gap-6 ${className}`}>
        {Array.from({ length: 10 }).map((_, index) => (
          <LoadingSkeleton key={index} />
        ))}
      </div>
    );
  }

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
      {/* Model Grid */}
      <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-4 sm:gap-6">
        {models.map((model) => (
          <ModelCard
            key={model.id}
            model={model}
            onClick={onModelClick}
          />
        ))}
        
        {/* Loading more skeletons */}
        {loadingMore && Array.from({ length: 5 }).map((_, index) => (
          <LoadingSkeleton key={`loading-${index}`} />
        ))}
      </div>

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
      {loadingMore && (
        <div className="text-center mt-8">
          <div className="inline-flex items-center">
            <div className="lg-spinner h-5 w-5 mr-3"></div>
            <span className="text-white/80">Loading more models...</span>
          </div>
        </div>
      )}
    </div>
  );
};

export default ModelGrid; 