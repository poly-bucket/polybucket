import React, { useState, useEffect } from 'react';
import { Settings as SettingsIcon } from '@mui/icons-material';
import fileTypeSettingsService, { FileTypeSettingsData } from '../../../../services/fileTypeSettingsService';

const FileTypeSettings: React.FC = () => {
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [fileTypes, setFileTypes] = useState<FileTypeSettingsData[]>([]);
  const [editingFileType, setEditingFileType] = useState<FileTypeSettingsData | null>(null);
  const [message, setMessage] = useState<string | null>(null);

  const fetchFileTypes = async () => {
    try {
      setLoading(true);
      const response = await fileTypeSettingsService.getFileSettings();
      if (response.success && response.fileTypes) {
        setFileTypes(response.fileTypes);
      }
    } catch (err) {
      console.error('Error loading file type settings:', err);
      setMessage('Error loading file type settings');
    } finally {
      setLoading(false);
    }
  };

  const saveFileType = async (fileType: FileTypeSettingsData) => {
    try {
      setSaving(true);
      const response = await fileTypeSettingsService.updateFileSettings(fileType);
      if (response.success) {
        setMessage('File type settings updated successfully');
        setEditingFileType(null);
        await fetchFileTypes(); // Refresh the list
      } else {
        setMessage(`Failed to update: ${response.message}`);
      }
    } catch (err) {
      console.error('Error saving file type settings:', err);
      setMessage('Error saving file type settings');
    } finally {
      setSaving(false);
    }
  };

  const handleEdit = (fileType: FileTypeSettingsData) => {
    setEditingFileType({ ...fileType });
  };

  const handleCancel = () => {
    setEditingFileType(null);
  };

  const handleSave = () => {
    if (editingFileType) {
      saveFileType(editingFileType);
    }
  };

  const handleInputChange = (field: keyof FileTypeSettingsData, value: any) => {
    if (editingFileType) {
      setEditingFileType(prev => prev ? { ...prev, [field]: value } : null);
    }
  };

  const formatFileSize = (bytes: number) => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  useEffect(() => {
    fetchFileTypes();
  }, []);

  useEffect(() => {
    if (message) {
      const timer = setTimeout(() => setMessage(null), 5000);
      return () => clearTimeout(timer);
    }
  }, [message]);

  if (loading) {
    return <div className="text-center text-white/60 py-8">Loading file type settings...</div>;
  }

  return (
    <div className="lg-card p-6">
      <div className="flex justify-between items-center mb-6">
        <h3 className="text-lg font-medium text-white flex items-center">
          <SettingsIcon className="w-5 h-5 mr-2 text-blue-400" />
          File Type Settings
        </h3>
      </div>

      {message && (
        <div className={`mb-4 p-3 rounded text-sm ${
          message.includes('Error') || message.includes('Failed') 
            ? 'bg-red-900 border border-red-600 text-red-200' 
            : 'bg-green-900 border border-green-600 text-green-200'
        }`}>
          {message}
        </div>
      )}

      <div className="space-y-4">
        {fileTypes.map(fileType => (
          <div key={fileType.id} className="bg-gray-700 rounded-lg p-4">
            {editingFileType?.id === fileType.id ? (
              // Edit mode
              <div className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-white mb-2">File Extension</label>
                    <input
                      type="text"
                      value={editingFileType.fileExtension}
                      onChange={(e) => handleInputChange('fileExtension', e.target.value)}
                      className="lg-input"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-white mb-2">Display Name</label>
                    <input
                      type="text"
                      value={editingFileType.displayName}
                      onChange={(e) => handleInputChange('displayName', e.target.value)}
                      className="lg-input"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-white mb-2">Max File Size (bytes)</label>
                    <input
                      type="number"
                      value={editingFileType.maxFileSizeBytes}
                      onChange={(e) => handleInputChange('maxFileSizeBytes', parseInt(e.target.value))}
                      className="lg-input"
                    />
                    <p className="text-xs text-gray-400 mt-1">
                      Current: {formatFileSize(editingFileType.maxFileSizeBytes)}
                    </p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-white mb-2">Max Per Upload</label>
                    <input
                      type="number"
                      value={editingFileType.maxPerUpload}
                      onChange={(e) => handleInputChange('maxPerUpload', parseInt(e.target.value))}
                      className="lg-input"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-white mb-2">Category</label>
                    <select
                      value={editingFileType.category}
                      onChange={(e) => handleInputChange('category', e.target.value)}
                      className="lg-input"
                    >
                      <option value="3D">3D</option>
                      <option value="Image">Image</option>
                      <option value="Document">Document</option>
                      <option value="Archive">Archive</option>
                    </select>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-white mb-2">Priority</label>
                    <input
                      type="number"
                      value={editingFileType.priority}
                      onChange={(e) => handleInputChange('priority', parseInt(e.target.value))}
                      className="lg-input"
                    />
                  </div>
                </div>
                
                <div className="space-y-3">
                  <label className="block text-sm font-medium text-white">Options</label>
                  <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                    <label className="flex items-center">
                      <input
                        type="checkbox"
                        checked={editingFileType.enabled}
                        onChange={(e) => handleInputChange('enabled', e.target.checked)}
                        className="mr-2 text-green-600 bg-gray-800 border-gray-600 rounded focus:ring-green-500"
                      />
                      <span className="text-sm text-gray-300">Enabled</span>
                    </label>
                    <label className="flex items-center">
                      <input
                        type="checkbox"
                        checked={editingFileType.requiresPreview}
                        onChange={(e) => handleInputChange('requiresPreview', e.target.checked)}
                        className="mr-2 text-green-600 bg-gray-800 border-gray-600 rounded focus:ring-green-500"
                      />
                      <span className="text-sm text-gray-300">Requires Preview</span>
                    </label>
                    <label className="flex items-center">
                      <input
                        type="checkbox"
                        checked={editingFileType.isCompressible}
                        onChange={(e) => handleInputChange('isCompressible', e.target.checked)}
                        className="mr-2 text-green-600 bg-gray-800 border-gray-600 rounded focus:ring-green-500"
                      />
                      <span className="text-sm text-gray-300">Compressible</span>
                    </label>
                    <label className="flex items-center">
                      <input
                        type="checkbox"
                        checked={editingFileType.isDefault}
                        onChange={(e) => handleInputChange('isDefault', e.target.checked)}
                        className="mr-2 text-green-600 bg-gray-800 border-gray-600 rounded focus:ring-green-500"
                      />
                      <span className="text-sm text-gray-300">Default</span>
                    </label>
                  </div>
                </div>

                <div>
                  <label className="block text-sm font-medium text-white mb-2">Description</label>
                  <textarea
                    value={editingFileType.description}
                    onChange={(e) => handleInputChange('description', e.target.value)}
                    rows={2}
                    className="lg-input resize-none"
                  />
                </div>

                <div className="flex justify-end space-x-3">
                  <button
                    onClick={handleCancel}
                    className="lg-button"
                  >
                    Cancel
                  </button>
                  <button
                    onClick={handleSave}
                    disabled={saving}
                    className="lg-button lg-button-primary disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    {saving ? 'Saving...' : 'Save Changes'}
                  </button>
                </div>
              </div>
            ) : (
              // View mode
              <div className="flex justify-between items-start">
                <div className="flex-1">
                  <div className="flex items-center gap-3 mb-2">
                    <span className="text-lg font-medium text-white">{fileType.displayName}</span>
                    <span className="text-sm text-gray-400">({fileType.fileExtension})</span>
                    <span className={`px-2 py-1 rounded-full text-xs ${
                      fileType.enabled 
                        ? 'bg-green-900 text-green-200' 
                        : 'bg-red-900 text-red-200'
                    }`}>
                      {fileType.enabled ? 'Enabled' : 'Disabled'}
                    </span>
                    <span className="px-2 py-1 rounded-full text-xs bg-blue-900 text-blue-200">
                      {fileType.category}
                    </span>
                  </div>
                  <p className="text-sm text-gray-300 mb-2">{fileType.description}</p>
                  <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm text-gray-400">
                    <div>Max Size: {formatFileSize(fileType.maxFileSizeBytes)}</div>
                    <div>Max Per Upload: {fileType.maxPerUpload}</div>
                    <div>Priority: {fileType.priority}</div>
                    <div>MIME: {fileType.mimeType}</div>
                  </div>
                  <div className="flex gap-2 mt-2">
                    {fileType.requiresPreview && (
                      <span className="px-2 py-1 rounded-full text-xs bg-purple-900 text-purple-200">
                        Preview Required
                      </span>
                    )}
                    {fileType.isCompressible && (
                      <span className="px-2 py-1 rounded-full text-xs bg-yellow-900 text-yellow-200">
                        Compressible
                      </span>
                    )}
                    {fileType.isDefault && (
                      <span className="px-2 py-1 rounded-full text-xs bg-indigo-900 text-indigo-200">
                        Default
                      </span>
                    )}
                  </div>
                </div>
                <button
                  onClick={() => handleEdit(fileType)}
                  className="lg-button lg-button-primary text-sm"
                >
                  Edit
                </button>
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
};

export default FileTypeSettings;
