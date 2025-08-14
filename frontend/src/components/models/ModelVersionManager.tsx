import React, { useState, useRef } from 'react';
import { ExtendedModel } from '../../services/modelsService';
import { ModelVersion } from '../../services/api.client';

interface UploadedFile {
  id: string;
  name: string;
  size: number;
  type: string;
  file: File;
  progress: number;
  isThumbnail: boolean;
}

interface VersionFormData {
  name: string;
  notes: string;
  files: UploadedFile[];
}

interface ModelVersionManagerProps {
  model: ExtendedModel;
  onCreateVersion: (versionData: VersionFormData) => Promise<void>;
  onCancel: () => void;
  loading?: boolean;
}

const ModelVersionManager: React.FC<ModelVersionManagerProps> = ({
  model,
  onCreateVersion,
  onCancel,
  loading = false
}) => {
  const [formData, setFormData] = useState<VersionFormData>({
    name: `${model.name} v${(model.versions?.length || 0) + 1}`,
    notes: '',
    files: []
  });

  const fileInputRef = useRef<HTMLInputElement>(null);

  // Supported file formats
  const supported3DFormats = ['.stl', '.obj', '.fbx', '.gltf', '.glb', '.3mf', '.step', '.stp'];
  const supportedImageFormats = ['.jpg', '.jpeg', '.png', '.gif', '.webp', '.bmp'];
  const supportedDocumentFormats = ['.pdf', '.md', '.markdown', '.txt'];
  const allSupportedFormats = [...supported3DFormats, ...supportedImageFormats, ...supportedDocumentFormats];

  const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
    const files = event.target.files;
    if (!files) return;

    Array.from(files).forEach(file => {
      const fileExtension = file.name.toLowerCase().substring(file.name.lastIndexOf('.'));
      
      if (allSupportedFormats.includes(fileExtension)) {
        const newFile: UploadedFile = {
          id: Math.random().toString(36).substr(2, 9),
          name: file.name,
          size: file.size,
          type: file.type,
          file: file,
          progress: 0,
          isThumbnail: false
        };

        setFormData(prev => ({
          ...prev,
          files: [...prev.files, newFile]
        }));
      }
    });

    // Clear the input
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  const handleDragOver = (event: React.DragEvent) => {
    event.preventDefault();
  };

  const handleDrop = (event: React.DragEvent) => {
    event.preventDefault();
    const files = event.dataTransfer.files;
    
    if (files.length > 0) {
      const fileArray = Array.from(files);
      fileArray.forEach(file => {
        const fileExtension = file.name.toLowerCase().substring(file.name.lastIndexOf('.'));
        
        if (allSupportedFormats.includes(fileExtension)) {
          const newFile: UploadedFile = {
            id: Math.random().toString(36).substr(2, 9),
            name: file.name,
            size: file.size,
            type: file.type,
            file: file,
            progress: 0,
            isThumbnail: false
          };

          setFormData(prev => ({
            ...prev,
            files: [...prev.files, newFile]
          }));
        }
      });
    }
  };

  const setAsThumbnail = (fileId: string) => {
    setFormData(prev => ({
      ...prev,
      files: prev.files.map(file => ({
        ...file,
        isThumbnail: file.id === fileId
      }))
    }));
  };

  const removeFile = (fileId: string) => {
    setFormData(prev => ({
      ...prev,
      files: prev.files.filter(file => file.id !== fileId)
    }));
  };

  const formatFileSize = (bytes: number) => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    await onCreateVersion(formData);
  };

  // Check if we have at least one 3D model file
  const has3DFile = formData.files.some(file => {
    const extension = file.name.toLowerCase().substring(file.name.lastIndexOf('.'));
    return supported3DFormats.includes(extension);
  });

  return (
    <div className="bg-white p-6 rounded-lg shadow-lg max-w-4xl mx-auto">
      <h2 className="text-2xl font-bold text-gray-800 mb-6">Create New Model Version</h2>
      
      <div className="mb-6 p-4 bg-blue-50 rounded-lg border border-blue-200">
        <h3 className="text-lg font-semibold text-blue-800 mb-2">Current Model: {model.name}</h3>
        <p className="text-blue-700">
          Current Version: v{model.versions?.length || 1} • 
          Creating: v{(model.versions?.length || 0) + 1}
        </p>
      </div>
      
      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Version Information */}
        <div className="grid grid-cols-1 gap-6">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Version Name *
            </label>
            <input
              type="text"
              value={formData.name}
              onChange={(e) => setFormData(prev => ({ ...prev, name: e.target.value }))}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent"
              placeholder="Enter version name"
              required
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Version Notes
            </label>
            <textarea
              value={formData.notes}
              onChange={(e) => setFormData(prev => ({ ...prev, notes: e.target.value }))}
              rows={4}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent resize-none"
              placeholder="Describe what's new in this version..."
            />
          </div>
        </div>

        {/* File Upload Area */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Files for New Version *
          </label>
          
          <div
            onDragOver={handleDragOver}
            onDrop={handleDrop}
            className="border-2 border-dashed border-gray-300 rounded-lg p-8 text-center hover:border-green-400 transition-colors"
          >
            <div className="space-y-4">
              <div>
                <svg className="mx-auto h-12 w-12 text-gray-400" stroke="currentColor" fill="none" viewBox="0 0 48 48">
                  <path d="M28 8H12a4 4 0 00-4 4v20m32-12v8m0 0v8a4 4 0 01-4 4H12a4 4 0 01-4-4v-4m32-4l-3.172-3.172a4 4 0 00-5.656 0L28 28M8 32l9.172-9.172a4 4 0 015.656 0L28 28m0 0l4 4m4-24h8m-4-4v8m-12 4h.02" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round" />
                </svg>
              </div>
              <div>
                <p className="text-lg text-gray-600">
                  Drop files here or{' '}
                  <button
                    type="button"
                    onClick={() => fileInputRef.current?.click()}
                    className="text-green-600 hover:text-green-500 font-medium"
                  >
                    browse
                  </button>
                </p>
                <p className="text-sm text-gray-500 mt-2">
                  Supported formats: {allSupportedFormats.join(', ')}
                </p>
                <p className="text-sm text-gray-500">
                  At least one 3D model file (.stl, .obj, .fbx, .gltf, .glb, .3mf, .step, .stp) is required
                </p>
              </div>
            </div>
          </div>

          <input
            ref={fileInputRef}
            type="file"
            multiple
            accept={allSupportedFormats.join(',')}
            onChange={handleFileSelect}
            className="hidden"
          />
        </div>

        {/* Uploaded Files List */}
        {formData.files.length > 0 && (
          <div>
            <h3 className="text-lg font-medium text-gray-800 mb-4">Uploaded Files ({formData.files.length})</h3>
            <div className="space-y-2">
              {formData.files.map((file) => {
                const is3D = supported3DFormats.includes(
                  file.name.toLowerCase().substring(file.name.lastIndexOf('.'))
                );
                const isImage = supportedImageFormats.includes(
                  file.name.toLowerCase().substring(file.name.lastIndexOf('.'))
                );

                return (
                  <div key={file.id} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg border">
                    <div className="flex items-center space-x-3">
                      <div className={`w-2 h-2 rounded-full ${is3D ? 'bg-blue-500' : isImage ? 'bg-green-500' : 'bg-gray-500'}`}></div>
                      <div>
                        <p className="text-sm font-medium text-gray-800">{file.name}</p>
                        <p className="text-xs text-gray-500">
                          {formatFileSize(file.size)} • {is3D ? '3D Model' : isImage ? 'Image' : 'Other'}
                        </p>
                      </div>
                    </div>
                    
                    <div className="flex items-center space-x-2">
                      {isImage && (
                        <button
                          type="button"
                          onClick={() => setAsThumbnail(file.id)}
                          className={`px-3 py-1 text-xs rounded-full border ${
                            file.isThumbnail
                              ? 'bg-green-100 border-green-500 text-green-700'
                              : 'bg-gray-100 border-gray-300 text-gray-600 hover:border-green-500'
                          }`}
                        >
                          {file.isThumbnail ? 'Thumbnail' : 'Set as Thumbnail'}
                        </button>
                      )}
                      <button
                        type="button"
                        onClick={() => removeFile(file.id)}
                        className="p-1 text-red-600 hover:text-red-800"
                      >
                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                        </svg>
                      </button>
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        )}

        {/* Validation Messages */}
        {formData.files.length > 0 && !has3DFile && (
          <div className="p-4 bg-yellow-50 border border-yellow-200 rounded-lg">
            <div className="flex">
              <svg className="w-5 h-5 text-yellow-400 mr-2" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
              </svg>
              <p className="text-yellow-800 text-sm">
                At least one 3D model file (.stl, .obj, .fbx, .gltf, .glb, .3mf, .step, .stp) is required for a new version.
              </p>
            </div>
          </div>
        )}

        {/* Action Buttons */}
        <div className="flex justify-end space-x-4 pt-4 border-t">
          <button
            type="button"
            onClick={onCancel}
            disabled={loading}
            className="px-6 py-2 text-gray-600 bg-gray-100 rounded-md hover:bg-gray-200 focus:outline-none focus:ring-2 focus:ring-gray-500 disabled:opacity-50"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={loading || !formData.name.trim() || !has3DFile}
            className="px-6 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-green-500 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {loading ? 'Creating Version...' : 'Create Version'}
          </button>
        </div>
      </form>
    </div>
  );
};

export default ModelVersionManager; 