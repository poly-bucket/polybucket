import React from 'react';
import type { ModelConfigurationSettings } from '../../../../services/modelConfigurationSettingsService';

interface UploadSettingsProps {
  settings: ModelConfigurationSettings;
  setSettings: (updates: Partial<ModelConfigurationSettings>) => void;
}

const UploadSettings: React.FC<UploadSettingsProps> = ({ settings, setSettings }) => {
  return (
    <div className="lg-card p-4">
      <h4 className="text-md font-medium text-white mb-4">Upload Settings</h4>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        
        {/* Anonymous Uploads */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Allow Anonymous Uploads</label>
          <input
            type="checkbox"
            checked={settings.allowAnonUploads}
            onChange={(e) => setSettings({ allowAnonUploads: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Require Upload Moderation */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Require Upload Moderation</label>
          <input
            type="checkbox"
            checked={settings.requireUploadModeration}
            onChange={(e) => setSettings({ requireUploadModeration: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Default Privacy Setting */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Default Privacy Setting</label>
          <select
            value={settings.defaultPrivacySetting}
            onChange={(e) => setSettings({ defaultPrivacySetting: e.target.value })}
            className="lg-select"
          >
            <option value="Public">Public</option>
            <option value="Private">Private</option>
            <option value="Unlisted">Unlisted</option>
          </select>
        </div>

        {/* Allow Anonymous Downloads */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Allow Anonymous Downloads</label>
          <input
            type="checkbox"
            checked={settings.allowAnonDownloads}
            onChange={(e) => setSettings({ allowAnonDownloads: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Enable Model Versioning */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Model Versioning</label>
          <input
            type="checkbox"
            checked={settings.enableModelVersioning}
            onChange={(e) => setSettings({ enableModelVersioning: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Limit Total Models */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Limit Total Models</label>
          <input
            type="number"
            value={settings.limitTotalModels}
            onChange={(e) => setSettings({ limitTotalModels: parseInt(e.target.value) || 0 })}
            className="lg-input w-24"
            min="1"
            max="100000"
          />
        </div>

        {/* Max Models Per User */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Max Models Per User</label>
          <input
            type="number"
            value={settings.maxModelsPerUser}
            onChange={(e) => setSettings({ maxModelsPerUser: parseInt(e.target.value) || 0 })}
            className="lg-input w-24"
            min="1"
            max="10000"
          />
        </div>

        {/* Auto Approve Verified Users */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Auto Approve Verified Users</label>
          <input
            type="checkbox"
            checked={settings.autoApproveVerifiedUsers}
            onChange={(e) => setSettings({ autoApproveVerifiedUsers: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Require Thumbnail */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Require Thumbnail</label>
          <input
            type="checkbox"
            checked={settings.requireThumbnail}
            onChange={(e) => setSettings({ requireThumbnail: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Require Model Description */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Require Model Description</label>
          <input
            type="checkbox"
            checked={settings.requireModelDescription}
            onChange={(e) => setSettings({ requireModelDescription: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Require Model Tags */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Require Model Tags</label>
          <input
            type="checkbox"
            checked={settings.requireModelTags}
            onChange={(e) => setSettings({ requireModelTags: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Require License Selection */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Require License Selection</label>
          <input
            type="checkbox"
            checked={settings.requireLicenseSelection}
            onChange={(e) => setSettings({ requireLicenseSelection: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Allow Custom Licenses */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Allow Custom Licenses</label>
          <input
            type="checkbox"
            checked={settings.allowCustomLicenses}
            onChange={(e) => setSettings({ allowCustomLicenses: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Require Category Selection */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Require Category Selection</label>
          <input
            type="checkbox"
            checked={settings.requireCategorySelection}
            onChange={(e) => setSettings({ requireCategorySelection: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Min Description Length */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Min Description Length</label>
          <input
            type="number"
            value={settings.minDescriptionLength}
            onChange={(e) => setSettings({ minDescriptionLength: parseInt(e.target.value) || 0 })}
            className="lg-input w-24"
            min="0"
            max="1000"
          />
        </div>

        {/* Max Description Length */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Max Description Length</label>
          <input
            type="number"
            value={settings.maxDescriptionLength}
            onChange={(e) => setSettings({ maxDescriptionLength: parseInt(e.target.value) || 0 })}
            className="lg-input w-24"
            min="10"
            max="10000"
          />
        </div>

        {/* Max Tags Per Model */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Max Tags Per Model</label>
          <input
            type="number"
            value={settings.maxTagsPerModel}
            onChange={(e) => setSettings({ maxTagsPerModel: parseInt(e.target.value) || 0 })}
            className="lg-input w-24"
            min="0"
            max="100"
          />
        </div>

        {/* Max Categories Per Model */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Max Categories Per Model</label>
          <input
            type="number"
            value={settings.maxCategoriesPerModel}
            onChange={(e) => setSettings({ maxCategoriesPerModel: parseInt(e.target.value) || 0 })}
            className="lg-input w-24"
            min="0"
            max="20"
          />
        </div>

      </div>
    </div>
  );
};

export default UploadSettings;
