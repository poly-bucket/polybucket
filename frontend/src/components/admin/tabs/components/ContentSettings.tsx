import React from 'react';
import type { ModelConfigurationSettings } from '../../../../services/modelConfigurationSettingsService';

interface ContentSettingsProps {
  settings: ModelConfigurationSettings;
  setSettings: (updates: Partial<ModelConfigurationSettings>) => void;
}

const ContentSettings: React.FC<ContentSettingsProps> = ({ settings, setSettings }) => {
  return (
    <div className="lg-card p-4">
      <h4 className="text-md font-medium text-white mb-4">Content Settings</h4>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        
        {/* Allow NSFW Content */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Allow NSFW Content</label>
          <input
            type="checkbox"
            checked={settings.allowNSFWContent}
            onChange={(e) => setSettings({ allowNSFWContent: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Allow AI Generated Content */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Allow AI Generated Content</label>
          <input
            type="checkbox"
            checked={settings.allowAIGeneratedContent}
            onChange={(e) => setSettings({ allowAIGeneratedContent: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Allow Model Remixing */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Allow Model Remixing</label>
          <input
            type="checkbox"
            checked={settings.allowModelRemixing}
            onChange={(e) => setSettings({ allowModelRemixing: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Require Remix Attribution */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Require Remix Attribution</label>
          <input
            type="checkbox"
            checked={settings.requireRemixAttribution}
            onChange={(e) => setSettings({ requireRemixAttribution: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Enable Model Comments */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Model Comments</label>
          <input
            type="checkbox"
            checked={settings.enableModelComments}
            onChange={(e) => setSettings({ enableModelComments: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Enable Model Likes */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Model Likes</label>
          <input
            type="checkbox"
            checked={settings.enableModelLikes}
            onChange={(e) => setSettings({ enableModelLikes: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Enable Model Downloads */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Model Downloads</label>
          <input
            type="checkbox"
            checked={settings.enableModelDownloads}
            onChange={(e) => setSettings({ enableModelDownloads: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Enable Model Collections */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Model Collections</label>
          <input
            type="checkbox"
            checked={settings.enableModelCollections}
            onChange={(e) => setSettings({ enableModelCollections: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Enable Model Sharing */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Model Sharing</label>
          <input
            type="checkbox"
            checked={settings.enableModelSharing}
            onChange={(e) => setSettings({ enableModelSharing: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Enable Model Embedding */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Model Embedding</label>
          <input
            type="checkbox"
            checked={settings.enableModelEmbedding}
            onChange={(e) => setSettings({ enableModelEmbedding: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Require Model Preview */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Require Model Preview</label>
          <input
            type="checkbox"
            checked={settings.requireModelPreview}
            onChange={(e) => setSettings({ requireModelPreview: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Auto Generate Model Previews */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Auto Generate Model Previews</label>
          <input
            type="checkbox"
            checked={settings.autoGenerateModelPreviews}
            onChange={(e) => setSettings({ autoGenerateModelPreviews: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Enable Model Analytics */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Model Analytics</label>
          <input
            type="checkbox"
            checked={settings.enableModelAnalytics}
            onChange={(e) => setSettings({ enableModelAnalytics: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Require User Agreement */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Require User Agreement</label>
          <input
            type="checkbox"
            checked={settings.requireUserAgreement}
            onChange={(e) => setSettings({ requireUserAgreement: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Enable Model Export */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Model Export</label>
          <input
            type="checkbox"
            checked={settings.enableModelExport}
            onChange={(e) => setSettings({ enableModelExport: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Enable Model Import */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Model Import</label>
          <input
            type="checkbox"
            checked={settings.enableModelImport}
            onChange={(e) => setSettings({ enableModelImport: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Require Model Validation */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Require Model Validation</label>
          <input
            type="checkbox"
            checked={settings.requireModelValidation}
            onChange={(e) => setSettings({ requireModelValidation: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Enable Model Backup */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Model Backup</label>
          <input
            type="checkbox"
            checked={settings.enableModelBackup}
            onChange={(e) => setSettings({ enableModelBackup: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Enable Model Archiving */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Model Archiving</label>
          <input
            type="checkbox"
            checked={settings.enableModelArchiving}
            onChange={(e) => setSettings({ enableModelArchiving: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Model Backup Retention Days */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Model Backup Retention (Days)</label>
          <input
            type="number"
            value={settings.modelBackupRetentionDays}
            onChange={(e) => setSettings({ modelBackupRetentionDays: parseInt(e.target.value) || 0 })}
            className="lg-input w-24"
            min="1"
            max="3650"
          />
        </div>

        {/* Model Archive Threshold Days */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Model Archive Threshold (Days)</label>
          <input
            type="number"
            value={settings.modelArchiveThresholdDays}
            onChange={(e) => setSettings({ modelArchiveThresholdDays: parseInt(e.target.value) || 0 })}
            className="lg-input w-24"
            min="1"
            max="3650"
          />
        </div>

      </div>

      {/* User Agreement Text */}
      {settings.requireUserAgreement && (
        <div className="mt-4">
          <label className="text-sm text-white/80 block mb-2">User Agreement Text</label>
          <textarea
            value={settings.userAgreementText}
            onChange={(e) => setSettings({ userAgreementText: e.target.value })}
            className="lg-textarea w-full"
            rows={4}
            placeholder="Enter the user agreement text..."
            maxLength={10000}
          />
        </div>
      )}
    </div>
  );
};

export default ContentSettings;
