import React, { useState, useEffect } from 'react';
import {
  Gavel as ModerationIcon,
  Search as SearchIcon,
  FilterList as FilterIcon,
  Block as BlockIcon,
  CheckCircle as ApproveIcon,
  Report as ReportIcon,
  Visibility as VisibilityIcon,
  VisibilityOff as HiddenIcon
} from '@mui/icons-material';
import { modelModerationService, ModelForModeration } from '../../../../services/modelModerationService';

// Use the ModelForModeration interface from the service

const ModelModerationActions: React.FC = () => {
  const [models, setModels] = useState<ModelForModeration[]>([]);
  const [loading, setLoading] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [selectedModels, setSelectedModels] = useState<string[]>([]);
  const [actionLoading, setActionLoading] = useState(false);
  const [message, setMessage] = useState<string | null>(null);

  // Load models awaiting moderation
  useEffect(() => {
    loadModels();
  }, []);

  const loadModels = async () => {
    try {
      setLoading(true);
      const response = await modelModerationService.getModelsAwaitingModeration();
      setModels(response.models);
    } catch (error) {
      console.error('Error loading models:', error);
      setMessage('Failed to load models');
    } finally {
      setLoading(false);
    }
  };

  const handleBulkAction = async (action: string) => {
    if (selectedModels.length === 0) {
      setMessage('Please select models to perform actions on');
      return;
    }

    setActionLoading(true);
    try {
      const result = await modelModerationService.performBulkAction(
        selectedModels, 
        action as 'approve' | 'reject' | 'hide' | 'show'
      );
      
      if (result.success) {
        setMessage(result.message);
        setSelectedModels([]);
        await loadModels(); // Refresh the list
      } else {
        setMessage(`Error: ${result.message}`);
      }
    } catch (error: any) {
      setMessage(`Error: ${error.message || 'Failed to perform action'}`);
    } finally {
      setActionLoading(false);
    }
  };

  const handleModelAction = async (modelId: string, action: string) => {
    setActionLoading(true);
    try {
      let result;
      
      switch (action) {
        case 'approve':
          result = await modelModerationService.approveModel(modelId);
          break;
        case 'reject':
          result = await modelModerationService.rejectModel(modelId);
          break;
        case 'toggle-visibility':
          const model = models.find(m => m.id === modelId);
          if (model) {
            result = await modelModerationService.toggleModelVisibility(modelId, !model.isPublic);
          }
          break;
        default:
          throw new Error('Unknown action');
      }
      
      if (result?.success) {
        setMessage(result.message);
        await loadModels(); // Refresh the list
      } else {
        setMessage(`Error: ${result?.message || 'Failed to perform action'}`);
      }
    } catch (error: any) {
      setMessage(`Error: ${error.message || 'Failed to perform action'}`);
    } finally {
      setActionLoading(false);
    }
  };

  const filteredModels = models.filter(model => {
    const matchesSearch = model.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         model.author.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus = statusFilter === 'all' || model.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const toggleModelSelection = (modelId: string) => {
    setSelectedModels(prev => 
      prev.includes(modelId) 
        ? prev.filter(id => id !== modelId)
        : [...prev, modelId]
    );
  };

  const selectAllModels = () => {
    setSelectedModels(filteredModels.map(model => model.id));
  };

  const clearSelection = () => {
    setSelectedModels([]);
  };

  return (
    <div className="space-y-6">
      <div className="lg-card p-6">
        <div className="flex justify-between items-center mb-6">
          <h3 className="text-lg font-medium text-white flex items-center">
            <ModerationIcon className="w-5 h-5 mr-2 text-blue-400" />
            Model Moderation Actions
          </h3>
        </div>

        {message && (
          <div className={`mb-4 p-3 rounded ${
            message.includes('Successfully') 
              ? 'bg-green-500/20 border border-green-500/50 text-green-300' 
              : 'bg-red-500/20 border border-red-500/50 text-red-300'
          }`}>
            {message}
          </div>
        )}

        {/* Search and Filter Controls */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
          <div className="relative">
            <SearchIcon className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-4 h-4" />
            <input
              type="text"
              placeholder="Search models..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="lg-input pl-10"
            />
          </div>
          
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            className="lg-input"
          >
            <option value="all">All Status</option>
            <option value="pending">Pending</option>
            <option value="approved">Approved</option>
            <option value="rejected">Rejected</option>
            <option value="flagged">Flagged</option>
          </select>

          <div className="flex gap-2">
            <button
              onClick={selectAllModels}
              className="lg-button lg-button-secondary text-sm"
            >
              Select All
            </button>
            <button
              onClick={clearSelection}
              className="lg-button lg-button-secondary text-sm"
            >
              Clear Selection
            </button>
          </div>
        </div>

        {/* Bulk Actions */}
        {selectedModels.length > 0 && (
          <div className="bg-yellow-900/20 border border-yellow-500/50 rounded-lg p-4 mb-6">
            <div className="flex items-center justify-between">
              <span className="text-yellow-300">
                {selectedModels.length} model(s) selected
              </span>
              <div className="flex gap-2">
                <button
                  onClick={() => handleBulkAction('approve')}
                  disabled={actionLoading}
                  className="lg-button lg-button-success text-sm"
                >
                  <ApproveIcon className="w-4 h-4 mr-1" />
                  Approve All
                </button>
                <button
                  onClick={() => handleBulkAction('reject')}
                  disabled={actionLoading}
                  className="lg-button lg-button-danger text-sm"
                >
                  <BlockIcon className="w-4 h-4 mr-1" />
                  Reject All
                </button>
                <button
                  onClick={() => handleBulkAction('hide')}
                  disabled={actionLoading}
                  className="lg-button lg-button-warning text-sm"
                >
                  <HiddenIcon className="w-4 h-4 mr-1" />
                  Hide All
                </button>
              </div>
            </div>
          </div>
        )}

        {/* Models Table */}
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-gray-700">
                <th className="text-left p-3 text-white/80">Select</th>
                <th className="text-left p-3 text-white/80">Model</th>
                <th className="text-left p-3 text-white/80">Author</th>
                <th className="text-left p-3 text-white/80">Status</th>
                <th className="text-left p-3 text-white/80">Reports</th>
                <th className="text-left p-3 text-white/80">Actions</th>
              </tr>
            </thead>
            <tbody>
              {filteredModels.map((model) => (
                <tr key={model.id} className="border-b border-gray-800 hover:bg-gray-800/50">
                  <td className="p-3">
                    <input
                      type="checkbox"
                      checked={selectedModels.includes(model.id)}
                      onChange={() => toggleModelSelection(model.id)}
                      className="rounded"
                    />
                  </td>
                  <td className="p-3">
                    <div>
                      <div className="font-medium text-white">{model.title}</div>
                      <div className="text-sm text-white/60">{model.category}</div>
                    </div>
                  </td>
                  <td className="p-3 text-white/80">{model.author}</td>
                  <td className="p-3">
                    <span className={`px-2 py-1 rounded-full text-xs ${
                      model.status === 'pending' ? 'bg-yellow-900 text-yellow-200' :
                      model.status === 'approved' ? 'bg-green-900 text-green-200' :
                      model.status === 'rejected' ? 'bg-red-900 text-red-200' :
                      'bg-orange-900 text-orange-200'
                    }`}>
                      {model.status}
                    </span>
                  </td>
                  <td className="p-3">
                    {model.reportCount > 0 ? (
                      <span className="text-red-400 font-medium">{model.reportCount}</span>
                    ) : (
                      <span className="text-white/60">0</span>
                    )}
                  </td>
                  <td className="p-3">
                    <div className="flex gap-1">
                      <button
                        onClick={() => handleModelAction(model.id, 'approve')}
                        disabled={actionLoading}
                        className="lg-button lg-button-success text-xs"
                        title="Approve"
                      >
                        <ApproveIcon className="w-3 h-3" />
                      </button>
                      <button
                        onClick={() => handleModelAction(model.id, 'reject')}
                        disabled={actionLoading}
                        className="lg-button lg-button-danger text-xs"
                        title="Reject"
                      >
                        <BlockIcon className="w-3 h-3" />
                      </button>
                      <button
                        onClick={() => handleModelAction(model.id, 'toggle-visibility')}
                        disabled={actionLoading}
                        className="lg-button lg-button-warning text-xs"
                        title={model.isPublic ? "Hide" : "Show"}
                      >
                        {model.isPublic ? <HiddenIcon className="w-3 h-3" /> : <VisibilityIcon className="w-3 h-3" />}
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {filteredModels.length === 0 && (
          <div className="text-center text-white/60 py-8">
            No models found matching your criteria
          </div>
        )}
      </div>
    </div>
  );
};

export default ModelModerationActions;
