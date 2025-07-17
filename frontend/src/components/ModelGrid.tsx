import React from 'react';
import ModelCard from './ModelCard';
import { Model } from '../services/api.client';

// Extended model interface to include missing properties from backend
interface ExtendedModel extends Model {
  thumbnailUrl?: string;
  downloads?: number;
}

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
    <div className="bg-white rounded-lg shadow-md overflow-hidden animate-pulse">
      <div className="aspect-video bg-gray-200" />
      <div className="p-4">
        <div className="h-4 bg-gray-200 rounded mb-2" />
        <div className="h-3 bg-gray-200 rounded w-2/3 mb-3" />
        <div className="flex justify-between">
          <div className="h-3 bg-gray-200 rounded w-12" />
          <div className="h-3 bg-gray-200 rounded w-12" />
          <div className="h-3 bg-gray-200 rounded w-12" />
        </div>
      </div>
    </div>
  );

  // Error state
  if (error) {
    return (
      <div className={`text-center py-12 ${className}`}>
        <div className="bg-red-50 border border-red-200 rounded-lg p-6 max-w-md mx-auto">
          <svg className="w-12 h-12 text-red-400 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <h3 className="text-lg font-medium text-red-800 mb-2">Error Loading Models</h3>
          <p className="text-red-600">{error}</p>
        </div>
      </div>
    );
  }

  // Loading state
  if (loading) {
    return (
      <div className={`grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-6 ${className}`}>
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
          <svg className="w-16 h-16 text-gray-400 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
          </svg>
          <h3 className="text-lg font-medium text-gray-900 mb-2">No Models Found</h3>
          <p className="text-gray-600 mb-6">There are no models to display at the moment.</p>
          <button 
            onClick={() => window.location.reload()}
            className="px-4 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700 transition-colors"
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
      <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-6">
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
            className="px-6 py-3 bg-indigo-600 text-white rounded-md hover:bg-indigo-700 transition-colors font-medium"
          >
            Load More Models
          </button>
        </div>
      )}

      {/* Loading more indicator */}
      {loadingMore && (
        <div className="text-center mt-8">
          <div className="inline-flex items-center">
            <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-indigo-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
              <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
              <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
            </svg>
            <span className="text-gray-700">Loading more models...</span>
          </div>
        </div>
      )}
    </div>
  );
};

export default ModelGrid; 