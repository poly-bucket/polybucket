import React, { useState } from 'react';
import { ExtendedModel } from '../services/modelsService';
import { Model } from '../api/client';
import ModelEditForm from './ModelEditForm';
import ModelVersionManager from './ModelVersionManager';
import VersionEditor, { ExtendedModelVersion } from './VersionEditor';
import { useAppSelector } from '../store';

interface EditModelModalProps {
  model: ExtendedModel;
  isOpen: boolean;
  onClose: () => void;
  onModelUpdate?: (updatedModel: ExtendedModel) => void;
  onVersionCreate?: (model: ExtendedModel, versionData: any) => void;
  onVersionUpdate?: (model: ExtendedModel, versionId: string, updatedVersion: ExtendedModelVersion) => void;
}

type TabType = 'edit' | 'version' | 'editVersion';

interface VersionFormData {
  name: string;
  notes: string;
  files: any[];
}

const EditModelModal: React.FC<EditModelModalProps> = ({
  model,
  isOpen,
  onClose,
  onModelUpdate,
  onVersionCreate,
  onVersionUpdate
}) => {
  const { user } = useAppSelector(state => state.auth);
  const [activeTab, setActiveTab] = useState<TabType>('edit');
  const [loading, setLoading] = useState(false);
  const [modelVersions, setModelVersions] = useState<ExtendedModelVersion[]>([]);

  // Initialize versions when modal opens
  React.useEffect(() => {
    if (isOpen && model.versions) {
      const extendedVersions: ExtendedModelVersion[] = model.versions.map(version => ({
        ...version,
        billOfMaterials: [],
        printSettings: {
          id: version.id || '',
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
        notes: ''
      } as unknown as ExtendedModelVersion));
      setModelVersions(extendedVersions);
    }
  }, [isOpen, model.versions]);

  if (!isOpen) return null;

  const handleSaveModel = async (updatedModel: Partial<Model>) => {
    setLoading(true);
    try {
      if (!user?.accessToken) {
        throw new Error('No authentication token found');
      }

      const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:11666';
      
      const response = await fetch(`${baseUrl}/api/models/${model.id}`, {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${user.accessToken}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(updatedModel)
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || `Update failed: ${response.statusText}`);
      }

      const updatedModelData = await response.json();
      
      // Update the model with new data
      const updated = { ...model, ...updatedModelData } as ExtendedModel;
      
      if (onModelUpdate) {
        onModelUpdate(updated);
      }
      
      onClose();
    } catch (error) {
      console.error('Failed to update model:', error);
      // TODO: Add proper error handling with toast notification
      alert(`Failed to update model: ${error instanceof Error ? error.message : 'Unknown error'}`);
    } finally {
      setLoading(false);
    }
  };

  const handleCreateVersion = async (versionData: VersionFormData) => {
    setLoading(true);
    try {
      if (!user?.accessToken) {
        throw new Error('No authentication token found');
      }

      const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:11666';
      
      const formData = new FormData();
      formData.append('name', versionData.name);
      formData.append('notes', versionData.notes);
      
      // Add files
      versionData.files.forEach((file) => {
        formData.append('files', file.file);
        if (file.isThumbnail) {
          formData.append('thumbnailFileId', file.id);
        }
      });

      const response = await fetch(`${baseUrl}/api/models/${model.id}/versions`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${user.accessToken}`
        },
        body: formData
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || `Version creation failed: ${response.statusText}`);
      }

      const newVersion = await response.json();
      
      if (onVersionCreate) {
        onVersionCreate(model, { ...versionData, id: newVersion.id });
      }
      
      onClose();
    } catch (error) {
      console.error('Failed to create version:', error);
      alert(`Failed to create version: ${error instanceof Error ? error.message : 'Unknown error'}`);
    } finally {
      setLoading(false);
    }
  };

  const handleVersionUpdate = (versionId: string, updatedVersion: ExtendedModelVersion) => {
    // Update local state
    setModelVersions(prev => 
      prev.map(v => v.id === versionId ? updatedVersion : v)
    );
    
    // Notify parent component
    if (onVersionUpdate) {
      onVersionUpdate(model, versionId, updatedVersion);
    }
  };

  const handleCancel = () => {
    if (!loading) {
      onClose();
    }
  };

  return (
    <div className="lg-modal-overlay">
      {/* Modal */}
      <div className="lg-modal max-w-6xl">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-white/10">
          <div>
            <h2 className="text-2xl font-bold text-white">Manage Model</h2>
            <p className="text-white/60 mt-1">{model.name}</p>
          </div>
          <button
            onClick={handleCancel}
            disabled={loading}
            className="p-2 text-white/40 hover:text-white/80 disabled:opacity-50 transition-colors"
          >
            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        {/* Tab Navigation */}
        <div className="lg-tabs">
          <nav className="flex space-x-8 px-6">
            <button
              onClick={() => setActiveTab('edit')}
              disabled={loading}
              className={`lg-tab-button ${
                activeTab === 'edit' ? 'active' : ''
              } disabled:opacity-50`}
            >
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
              </svg>
              <span>Edit Details</span>
            </button>
            
            <button
              onClick={() => setActiveTab('version')}
              disabled={loading}
              className={`lg-tab-button ${
                activeTab === 'version' ? 'active' : ''
              } disabled:opacity-50`}
            >
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M9 19l3 3m0 0l3-3m-3 3V10" />
              </svg>
              <span>New Version</span>
            </button>

            <button
              onClick={() => setActiveTab('editVersion')}
              disabled={loading}
              className={`lg-tab-button ${
                activeTab === 'editVersion' ? 'active' : ''
              } disabled:opacity-50`}
            >
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
              <span>Edit Versions</span>
            </button>
          </nav>
        </div>

        {/* Tab Content */}
        <div className="lg-tab-panel max-h-[calc(100vh-200px)] overflow-y-auto">
          {activeTab === 'edit' && (
            <div className="p-6">
              <ModelEditForm
                model={model}
                onSave={handleSaveModel}
                onCancel={handleCancel}
                loading={loading}
              />
            </div>
          )}
          
          {activeTab === 'version' && (
            <div className="p-6">
              <ModelVersionManager
                model={model}
                onCreateVersion={handleCreateVersion}
                onCancel={handleCancel}
                loading={loading}
              />
            </div>
          )}

          {activeTab === 'editVersion' && (
            <div className="p-6">
              <VersionEditor
                model={model}
                versions={modelVersions}
                onVersionUpdate={handleVersionUpdate}
                onCancel={handleCancel}
                loading={loading}
              />
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default EditModelModal; 