import React from 'react';
import { SiteModelSettings } from '../../services/siteModelSettingsService';

interface FileUploadSettingsProps {
  settings: SiteModelSettings;
  setSettings: (updates: Partial<SiteModelSettings>) => void;
}

const FileUploadSettings: React.FC<FileUploadSettingsProps> = ({ settings, setSettings }) => {
  const formatFileSize = (bytes: number | undefined) => {
    if (!bytes) return '0 MB';
    const mb = bytes / (1024 * 1024);
    return `${mb} MB`;
  };

  return (
    <div className="space-y-4">
      <h4 className="text-md font-medium text-white mb-3">File Upload Settings</h4>
      
      <div>
        <label className="block text-sm font-medium text-white/80 mb-2">
          Max File Size (MB)
        </label>
        <input
          type="number"
          value={Math.round((settings.maxFileSizeBytes || 0) / (1024 * 1024))}
          onChange={(e) => setSettings({
            maxFileSizeBytes: parseInt(e.target.value) * 1024 * 1024
          })}
          min="1"
          max="1024"
          className="lg-input w-full"
        />
        <p className="text-xs text-white/60 mt-1">
          Current: {formatFileSize(settings.maxFileSizeBytes)}
        </p>
      </div>

      <div>
        <label className="block text-sm font-medium text-white/80 mb-2">
          Max Files per Upload
        </label>
        <input
          type="number"
          value={settings.maxFilesPerUpload || 5}
          onChange={(e) => setSettings({
            maxFilesPerUpload: parseInt(e.target.value)
          })}
          min="1"
          max="20"
          className="lg-input w-full"
        />
      </div>

      <div>
        <label className="block text-sm font-medium text-white/80 mb-2">
          Allowed File Types
        </label>
        <input
          type="text"
          value={settings.allowedFileTypes || ''}
          onChange={(e) => setSettings({
            allowedFileTypes: e.target.value
          })}
          className="lg-input w-full"
          placeholder=".stl,.obj,.fbx,.png,.jpg"
        />
        <p className="text-xs text-white/60 mt-1">
          Comma-separated list of file extensions
        </p>
      </div>

      <div className="flex items-center space-x-3">
        <input
          type="checkbox"
          id="enableFileCompression"
          checked={settings.enableFileCompression || false}
          onChange={(e) => setSettings({
            enableFileCompression: e.target.checked
          })}
          className="w-4 h-4 text-blue-600 bg-white/10 border-white/20 rounded focus:ring-blue-500"
        />
        <label htmlFor="enableFileCompression" className="text-sm text-white/80">
          Enable File Compression
        </label>
      </div>

      <div className="flex items-center space-x-3">
        <input
          type="checkbox"
          id="autoGeneratePreviews"
          checked={settings.autoGeneratePreviews || false}
          onChange={(e) => setSettings({
            autoGeneratePreviews: e.target.checked
          })}
          className="w-4 h-4 text-blue-600 bg-white/10 border-white/20 rounded focus:ring-blue-500"
        />
        <label htmlFor="autoGeneratePreviews" className="text-sm text-white/80">
          Auto-generate Previews
        </label>
      </div>
    </div>
  );
};

export default FileUploadSettings;
