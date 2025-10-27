import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import NavigationBar from '../components/common/NavigationBar';
import ModelGrid from '../models/ModelGrid';
import LayoutControls from '../components/common/LayoutControls';
import { DeleteModelService } from '../services/deleteModelService';
import { ApiClientFactory } from '../api/clientFactory';
import { ExtendedModel } from '../services/modelsService';
import { 
  Delete as DeleteIcon,
  SelectAll as SelectAllIcon,
  Clear as ClearIcon,
  Warning as WarningIcon
} from '@mui/icons-material';

const MyModels: React.FC = () => {
  const navigate = useNavigate();
  const { user } = useAuth();
  const [models, setModels] = useState<ExtendedModel[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string>('');
  const [selectedModels, setSelectedModels] = useState<Set<string>>(new Set());
  const [isDeleting, setIsDeleting] = useState(false);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [deleteCount, setDeleteCount] = useState(0);

  // Fetch user's models
  const fetchMyModels = async () => {
    if (!user?.accessToken) return;

    try {
      setLoading(true);
      setError('');
      
      const client = ApiClientFactory.getModelsByUserClient();
      const response = await client.getModelsByUserId(user.id, 1, 50, false, true);
      
      if (response && response.models) {
        const extendedModels: ExtendedModel[] = response.models.map((model: any) => ({
          ...model,
          downloadCount: model.downloads || 0,
          rating: 0,
          isLiked: false,
          isInCollection: false
        }));
        setModels(extendedModels);
      } else {
        setModels([]);
      }
    } catch (err) {
      console.error('Error fetching my models:', err);
      setError('Failed to load your models. Please try again later.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchMyModels();
  }, [user]);

  const handleModelClick = (model: ExtendedModel) => {
    navigate(`/models/${model.id}`);
  };

  const handleSelectModel = (modelId: string) => {
    setSelectedModels(prev => {
      const newSet = new Set(prev);
      if (newSet.has(modelId)) {
        newSet.delete(modelId);
      } else {
        newSet.add(modelId);
      }
      return newSet;
    });
  };

  const handleSelectAll = () => {
    if (selectedModels.size === models.length) {
      setSelectedModels(new Set());
    } else {
      setSelectedModels(new Set(models.map(m => m.id!)));
    }
  };

  const handleBulkDelete = async () => {
    if (!user?.accessToken || selectedModels.size === 0) return;

    setIsDeleting(true);
    try {
      const modelIds = Array.from(selectedModels);
      const result = await DeleteModelService.deleteModels(modelIds, user.accessToken);
      
      if (result.success) {
        alert(result.message || `${modelIds.length} models deleted successfully`);
        setSelectedModels(new Set());
        fetchMyModels(); // Refresh the list
      } else {
        alert(`Failed to delete models: ${result.message}`);
      }
    } catch (error) {
      console.error('Bulk delete error:', error);
      alert(`Failed to delete models: ${error instanceof Error ? error.message : 'Unknown error'}`);
    } finally {
      setIsDeleting(false);
      setShowDeleteConfirm(false);
    }
  };

  const handleDeleteClick = () => {
    setDeleteCount(selectedModels.size);
    setShowDeleteConfirm(true);
  };

  if (loading) {
    return (
      <div className="lg-container min-h-screen flex items-center justify-center">
        <div className="lg-spinner"></div>
      </div>
    );
  }

  return (
    <div className="lg-container min-h-screen">
      <NavigationBar
        title="My Models"
        showSearch={false}
        showUploadButton={true}
        showHomeLink={true}
      />

      <div className="max-w-7xl mx-auto px-3 sm:px-4 md:px-6 lg:px-8 py-4 sm:py-6 lg:py-8">
        {/* Header with bulk actions */}
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
          <div>
            <h1 className="text-2xl font-bold text-white mb-2">My Models</h1>
            <p className="text-gray-400">
              {models.length} model{models.length !== 1 ? 's' : ''} • {selectedModels.size} selected
            </p>
          </div>
          
          {models.length > 0 && (
            <div className="flex items-center gap-3">
              <button
                onClick={handleSelectAll}
                className="lg-button lg-button-secondary flex items-center"
              >
                <SelectAllIcon className="w-4 h-4 mr-2" />
                {selectedModels.size === models.length ? 'Deselect All' : 'Select All'}
              </button>
              
              {selectedModels.size > 0 && (
                <>
                  <button
                    onClick={() => setSelectedModels(new Set())}
                    className="lg-button lg-button-secondary flex items-center"
                  >
                    <ClearIcon className="w-4 h-4 mr-2" />
                    Clear Selection
                  </button>
                  
                  <button
                    onClick={handleDeleteClick}
                    disabled={isDeleting}
                    className="lg-button bg-red-600 hover:bg-red-700 text-white flex items-center disabled:opacity-50"
                  >
                    <DeleteIcon className="w-4 h-4 mr-2" />
                    Delete Selected ({selectedModels.size})
                  </button>
                </>
              )}
            </div>
          )}
        </div>

        {/* Error message */}
        {error && (
          <div className="lg-card p-4 mb-6 bg-red-900/20 border border-red-500/30">
            <p className="text-red-300">{error}</p>
          </div>
        )}

        {/* Models Grid */}
        {models.length > 0 ? (
          <>
            <div className="flex justify-end mb-4">
              <LayoutControls compact={true} />
            </div>
            
            <ModelGrid
              models={models}
              loading={false}
              error=""
              onModelClick={handleModelClick}
              selectedModels={selectedModels}
              onModelSelect={handleSelectModel}
              showSelection={true}
            />
          </>
        ) : (
          <div className="lg-card p-12 text-center">
            <div className="w-16 h-16 bg-gray-700 rounded-full flex items-center justify-center mx-auto mb-4">
              <svg className="w-8 h-8 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
              </svg>
            </div>
            <h3 className="text-xl font-semibold text-white mb-2">No Models Yet</h3>
            <p className="text-gray-400 mb-6">You haven't uploaded any models yet. Start by uploading your first 3D model!</p>
            <button
              onClick={() => navigate('/upload-model')}
              className="lg-button lg-button-primary"
            >
              Upload Your First Model
            </button>
          </div>
        )}

        {/* Delete Confirmation Modal */}
        {showDeleteConfirm && (
          <div className="lg-modal-overlay">
            <div className="lg-modal max-w-md">
              <div className="p-6">
                <div className="flex items-center mb-4">
                  <div className="w-12 h-12 bg-red-100 rounded-full flex items-center justify-center mr-4">
                    <WarningIcon className="w-6 h-6 text-red-600" />
                  </div>
                  <div>
                    <h3 className="text-lg font-semibold text-white">Delete Models</h3>
                    <p className="text-gray-400">This action cannot be undone</p>
                  </div>
                </div>
                
                <p className="text-gray-300 mb-6">
                  Are you sure you want to delete <strong>{deleteCount}</strong> model{deleteCount !== 1 ? 's' : ''}? 
                  This will permanently remove the model{deleteCount !== 1 ? 's' : ''} and all associated files from the platform.
                </p>
                
                <div className="flex space-x-3">
                  <button
                    onClick={() => setShowDeleteConfirm(false)}
                    disabled={isDeleting}
                    className="flex-1 lg-button lg-button-secondary"
                  >
                    Cancel
                  </button>
                  <button
                    onClick={handleBulkDelete}
                    disabled={isDeleting}
                    className="flex-1 bg-red-600 hover:bg-red-700 text-white lg-button disabled:opacity-50"
                  >
                    {isDeleting ? 'Deleting...' : `Delete ${deleteCount} Model${deleteCount !== 1 ? 's' : ''}`}
                  </button>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default MyModels;
