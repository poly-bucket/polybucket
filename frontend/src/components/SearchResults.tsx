import React from 'react';
import { useNavigate } from 'react-router-dom';
import { SearchResult } from '../services/searchService';
import ModelGrid from './ModelGrid';
import { ExtendedModel } from '../services/modelsService';

interface SearchResultsProps {
  results: SearchResult[];
  loading: boolean;
  error: string;
  onResultClick: (result: SearchResult) => void;
}

const SearchResults: React.FC<SearchResultsProps> = ({ 
  results, 
  loading, 
  error, 
  onResultClick 
}) => {
  const navigate = useNavigate();

  const handleResultClick = (result: SearchResult) => {
    onResultClick(result);
    
    // Navigate based on result type
    switch (result.type) {
      case 'model':
        navigate(`/models/${result.id}`);
        break;
      case 'user':
        navigate(`/users/${result.id}`);
        break;
      case 'collection':
        navigate(`/collections/${result.id}`);
        break;
    }
  };

  const getResultIcon = (type: string) => {
    switch (type) {
      case 'model':
        return '📦';
      case 'user':
        return '👤';
      case 'collection':
        return '📁';
      default:
        return '📄';
    }
  };

  const getResultTypeLabel = (type: string) => {
    switch (type) {
      case 'model':
        return 'Model';
      case 'user':
        return 'User';
      case 'collection':
        return 'Collection';
      default:
        return 'Item';
    }
  };

  // Convert search results to ExtendedModel format for ModelGrid
  const convertToExtendedModels = (searchResults: SearchResult[]): ExtendedModel[] => {
    return searchResults
      .filter(result => result.type === 'model')
      .map(result => ({
        id: result.id,
        name: result.title,
        description: result.description || '',
        thumbnailUrl: result.thumbnailUrl,
        author: result.author ? { username: result.author } : undefined,
        likes: result.likes ? [{ id: '1', userId: '1' }] : [],
        comments: [],
        downloads: result.downloads || 0,
        createdAt: result.createdAt,
        updatedAt: result.createdAt,
        versions: [],
        categories: [],
        tags: [],
        collections: [],
        isPublic: true,
        isFeatured: false,
        isModerated: false,
        moderationStatus: 'approved' as const,
        moderationNotes: '',
        fileSize: 0,
        printTime: 0,
        filamentLength: 0,
        layerHeight: 0,
        infillPercentage: 0,
        supports: false,
        rafts: false,
        brims: false,
        skirts: false,
        printSpeed: 0,
        nozzleTemperature: 0,
        bedTemperature: 0,
        material: '',
        nozzleSize: 0,
        retractionDistance: 0,
        retractionSpeed: 0,
        buildPlateAdhesion: '',
        printSettings: '',
        notes: '',
        billOfMaterials: [],
        printSettings: {
          id: result.id,
          layerHeight: '0.2mm',
          infill: '15%',
          supports: false,
          printSpeed: '50mm/s',
          nozzleTemp: '210°C',
          bedTemp: '60°C',
          material: 'PLA',
          nozzleSize: '0.4mm',
          retraction: '6mm',
          buildPlateAdhesion: 'skirt',
          notes: ''
        },
        files: [],
        version: '1.0',
        versionNotes: '',
        isLatestVersion: true,
        versionCreatedAt: result.createdAt,
        versionUpdatedAt: result.createdAt
      } as ExtendedModel));
  };

  // Separate models from other result types
  const modelResults = convertToExtendedModels(results);
  const otherResults = results.filter(result => result.type !== 'model');

  if (loading) {
    return (
      <div className="flex items-center justify-center py-12">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-500"></div>
        <span className="ml-3 text-white/60">Searching...</span>
      </div>
    );
  }

  if (error) {
    return (
      <div className="text-center py-12">
        <div className="text-red-400 mb-2">⚠️</div>
        <p className="text-red-400">{error}</p>
      </div>
    );
  }

  if (results.length === 0) {
    return (
      <div className="text-center py-12">
        <div className="text-white/40 mb-2">🔍</div>
        <p className="text-white/60">No results found</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="text-sm text-white/60">
        Found {results.length} result{results.length !== 1 ? 's' : ''}
      </div>
      
      {/* Model Results using ModelGrid for consistent view preferences */}
      {modelResults.length > 0 && (
        <div>
          <h3 className="text-lg font-medium text-white mb-4">Models ({modelResults.length})</h3>
          <ModelGrid
            models={modelResults}
            onModelClick={(model) => {
              const searchResult = results.find(r => r.id === model.id && r.type === 'model');
              if (searchResult) {
                handleResultClick(searchResult);
              }
            }}
          />
        </div>
      )}

      {/* Other Results (Users, Collections, etc.) */}
      {otherResults.length > 0 && (
        <div>
          <h3 className="text-lg font-medium text-white mb-4">Other Results ({otherResults.length})</h3>
          <div className="grid gap-4">
            {otherResults.map((result) => (
              <div
                key={`${result.type}-${result.id}`}
                onClick={() => handleResultClick(result)}
                className="bg-white/5 backdrop-blur-sm border border-white/10 rounded-lg p-4 hover:bg-white/10 transition-all duration-200 cursor-pointer group"
              >
                <div className="flex items-start space-x-4">
                  {/* Thumbnail/Avatar */}
                  <div className="flex-shrink-0">
                    {result.thumbnailUrl || result.avatar ? (
                      <img
                        src={result.thumbnailUrl || result.avatar}
                        alt={result.title}
                        className="w-12 h-12 rounded-lg object-cover"
                      />
                    ) : (
                      <div className="w-12 h-12 rounded-lg bg-white/10 flex items-center justify-center text-2xl">
                        {getResultIcon(result.type)}
                      </div>
                    )}
                  </div>
                  
                  {/* Content */}
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center space-x-2 mb-1">
                      <h3 className="text-white font-medium truncate group-hover:text-indigo-300 transition-colors">
                        {result.title}
                      </h3>
                      <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-indigo-500/20 text-indigo-300">
                        {getResultTypeLabel(result.type)}
                      </span>
                    </div>
                    
                    {result.description && (
                      <p className="text-white/60 text-sm line-clamp-2 mb-2">
                        {result.description}
                      </p>
                    )}
                    
                    <div className="flex items-center space-x-4 text-xs text-white/40">
                      {result.author && (
                        <span>by {result.author}</span>
                      )}
                      {result.downloads !== undefined && (
                        <span>{result.downloads} downloads</span>
                      )}
                      {result.likes !== undefined && (
                        <span>{result.likes} likes</span>
                      )}
                      {result.modelCount !== undefined && (
                        <span>{result.modelCount} models</span>
                      )}
                      <span>
                        {new Date(result.createdAt).toLocaleDateString()}
                      </span>
                    </div>
                  </div>
                  
                  {/* Relevance Score (for debugging) */}
                  {result.relevanceScore > 0 && (
                    <div className="flex-shrink-0 text-xs text-white/40">
                      {Math.round(result.relevanceScore)}%
                    </div>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
};

export default SearchResults;
