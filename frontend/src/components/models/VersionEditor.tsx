import React, { useState } from 'react';
import { ExtendedModel } from '../../services/modelsService';
import { ModelVersion } from '../../services/api.client';
import BillOfMaterialsManager, { BillOfMaterial } from './BillOfMaterialsManager';
import PrintSettingsEditor, { PrintSettings } from './PrintSettingsEditor';

export interface ExtendedModelVersion extends ModelVersion {
  billOfMaterials?: BillOfMaterial[];
  printSettings?: PrintSettings;
  files?: any[];
  notes?: string;
}

interface VersionEditorProps {
  model: ExtendedModel;
  versions: ExtendedModelVersion[];
  onVersionUpdate: (versionId: string, updatedVersion: ExtendedModelVersion) => void;
  onCancel: () => void;
  loading?: boolean;
}

const VersionEditor: React.FC<VersionEditorProps> = ({
  model,
  versions,
  onVersionUpdate,
  onCancel,
  loading = false
}) => {
  const [selectedVersionId, setSelectedVersionId] = useState<string>(
    versions.length > 0 ? versions[0].id || '' : ''
  );
  const [activeTab, setActiveTab] = useState<'details' | 'bom' | 'print'>('details');

  const selectedVersion = versions.find(v => v.id === selectedVersionId);

  if (!selectedVersion) {
    return (
      <div className="bg-white p-6 rounded-lg shadow-lg max-w-4xl mx-auto">
        <div className="text-center py-8">
          <svg className="mx-auto h-12 w-12 text-gray-400 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
          </svg>
          <h3 className="text-lg font-medium text-gray-900 mb-2">No Versions Available</h3>
          <p className="text-gray-600">This model doesn't have any versions to edit.</p>
          <button
            onClick={onCancel}
            className="mt-4 px-4 py-2 bg-gray-600 text-white rounded-md hover:bg-gray-700"
          >
            Close
          </button>
        </div>
      </div>
    );
  }

  const handleVersionDetailsUpdate = (field: keyof ExtendedModelVersion, value: any) => {
    const updated = { ...selectedVersion, [field]: value } as ExtendedModelVersion;
    onVersionUpdate(selectedVersionId, updated);
  };

  const handleBOMUpdate = (bom: BillOfMaterial[]) => {
    const updated = { ...selectedVersion, billOfMaterials: bom } as ExtendedModelVersion;
    onVersionUpdate(selectedVersionId, updated);
  };

  const handlePrintSettingsUpdate = (settings: PrintSettings) => {
    const updated = { ...selectedVersion, printSettings: settings } as ExtendedModelVersion;
    onVersionUpdate(selectedVersionId, updated);
  };

  // Default print settings if none exist
  const defaultPrintSettings: PrintSettings = {
    id: selectedVersionId,
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
  };

  return (
    <div className="bg-white p-6 rounded-lg shadow-lg max-w-6xl mx-auto">
      <div className="mb-6">
        <h2 className="text-2xl font-bold text-gray-800 mb-2">Edit Model Version</h2>
        <p className="text-gray-600">Model: {model.name}</p>
      </div>

      {/* Version Selector */}
      <div className="mb-6">
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Select Version to Edit
        </label>
        <select
          value={selectedVersionId}
          onChange={(e) => setSelectedVersionId(e.target.value)}
          disabled={loading}
          className="w-full max-w-md px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent disabled:opacity-50"
        >
          {versions.map((version) => (
            <option key={version.id} value={version.id}>
              {version.name} (v{version.version})
            </option>
          ))}
        </select>
      </div>

      {/* Tab Navigation */}
      <div className="border-b border-gray-200 mb-6">
        <nav className="flex space-x-8">
          {[
            { id: 'details', label: 'Version Details', icon: '📝' },
            { id: 'bom', label: 'Bill of Materials', icon: '📋' },
            { id: 'print', label: 'Print Settings', icon: '⚙️' }
          ].map((tab) => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id as any)}
              disabled={loading}
              className={`py-4 px-1 border-b-2 font-medium text-sm transition-colors ${
                activeTab === tab.id
                  ? 'border-green-500 text-green-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              } disabled:opacity-50`}
            >
              <div className="flex items-center space-x-2">
                <span>{tab.icon}</span>
                <span>{tab.label}</span>
              </div>
            </button>
          ))}
        </nav>
      </div>

      {/* Tab Content */}
      <div className="space-y-6">
        {activeTab === 'details' && (
          <div className="space-y-6">
            <h3 className="text-lg font-semibold text-gray-800">Version Information</h3>
            
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Version Name
                </label>
                <input
                  type="text"
                  value={selectedVersion.name || ''}
                  onChange={(e) => handleVersionDetailsUpdate('name', e.target.value)}
                  disabled={loading}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent disabled:opacity-50"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Version Number
                </label>
                <input
                  type="number"
                  min="1"
                  value={selectedVersion.version || 1}
                  onChange={(e) => handleVersionDetailsUpdate('version', parseInt(e.target.value) || 1)}
                  disabled={loading}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent disabled:opacity-50"
                />
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Version Notes
              </label>
              <textarea
                value={selectedVersion.notes || ''}
                onChange={(e) => handleVersionDetailsUpdate('notes', e.target.value)}
                disabled={loading}
                rows={4}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent resize-none disabled:opacity-50"
                placeholder="Describe what's new or changed in this version..."
              />
            </div>

            {/* Files Summary (Read-only for now) */}
            <div>
              <h4 className="text-md font-medium text-gray-700 mb-2">Files in this Version</h4>
              <div className="bg-gray-50 rounded-lg p-4">
                {selectedVersion.files && selectedVersion.files.length > 0 ? (
                  <div className="space-y-2">
                    {selectedVersion.files.map((file: any, index: number) => (
                      <div key={index} className="flex items-center justify-between">
                        <span className="text-sm text-gray-600">{file.name || `File ${index + 1}`}</span>
                        <span className="text-xs text-gray-500">{file.size || 'Unknown size'}</span>
                      </div>
                    ))}
                  </div>
                ) : (
                  <p className="text-sm text-gray-500">No files associated with this version</p>
                )}
              </div>
            </div>
          </div>
        )}

        {activeTab === 'bom' && (
          <BillOfMaterialsManager
            billOfMaterials={selectedVersion.billOfMaterials || []}
            onUpdate={handleBOMUpdate}
            readonly={loading}
          />
        )}

        {activeTab === 'print' && (
          <PrintSettingsEditor
            printSettings={selectedVersion.printSettings || defaultPrintSettings}
            onUpdate={handlePrintSettingsUpdate}
            readonly={loading}
          />
        )}
      </div>

      {/* Action Buttons */}
      <div className="flex justify-end space-x-4 pt-6 mt-6 border-t">
        <button
          onClick={onCancel}
          disabled={loading}
          className="px-6 py-2 text-gray-600 bg-gray-100 rounded-md hover:bg-gray-200 focus:outline-none focus:ring-2 focus:ring-gray-500 disabled:opacity-50"
        >
          Close
        </button>
        <button
          disabled={loading}
          className="px-6 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-green-500 disabled:opacity-50"
        >
          {loading ? 'Saving...' : 'Changes Saved Automatically'}
        </button>
      </div>
    </div>
  );
};

export default VersionEditor; 