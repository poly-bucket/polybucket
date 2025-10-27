import React from 'react';
import type { ModelConfigurationSettings } from '../../services/modelConfigurationSettingsService';

interface FeatureSettingsProps {
  settings: ModelConfigurationSettings;
  setSettings: (updates: Partial<ModelConfigurationSettings>) => void;
}

const FeatureSettings: React.FC<FeatureSettingsProps> = ({ settings, setSettings }) => {
  return (
    <div className="lg-card p-4">
      <h4 className="text-md font-medium text-white mb-4">Feature Settings</h4>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        
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

        {/* Enable Model Flagging */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Model Flagging</label>
          <input
            type="checkbox"
            checked={settings.enableModelFlagging}
            onChange={(e) => setSettings({ enableModelFlagging: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Enable Model Reporting */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Model Reporting</label>
          <input
            type="checkbox"
            checked={settings.enableModelReporting}
            onChange={(e) => setSettings({ enableModelReporting: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Enable Model Blocking */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Model Blocking</label>
          <input
            type="checkbox"
            checked={settings.enableModelBlocking}
            onChange={(e) => setSettings({ enableModelBlocking: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Enable Model Whitelisting */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Model Whitelisting</label>
          <input
            type="checkbox"
            checked={settings.enableModelWhitelisting}
            onChange={(e) => setSettings({ enableModelWhitelisting: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Enable Model Blacklisting */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Model Blacklisting</label>
          <input
            type="checkbox"
            checked={settings.enableModelBlacklisting}
            onChange={(e) => setSettings({ enableModelBlacklisting: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Enable Model Rejection */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Model Rejection</label>
          <input
            type="checkbox"
            checked={settings.enableModelRejection}
            onChange={(e) => setSettings({ enableModelRejection: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Enable Model Appeals */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Model Appeals</label>
          <input
            type="checkbox"
            checked={settings.enableModelAppeals}
            onChange={(e) => setSettings({ enableModelAppeals: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Enable Model Locking */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Model Locking</label>
          <input
            type="checkbox"
            checked={settings.enableModelLocking}
            onChange={(e) => setSettings({ enableModelLocking: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Enable Model Unlocking */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Model Unlocking</label>
          <input
            type="checkbox"
            checked={settings.enableModelUnlocking}
            onChange={(e) => setSettings({ enableModelUnlocking: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Enable Model Deletion */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Model Deletion</label>
          <input
            type="checkbox"
            checked={settings.enableModelDeletion}
            onChange={(e) => setSettings({ enableModelDeletion: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

        {/* Enable Model Restoration */}
        <div className="flex items-center justify-between">
          <label className="text-sm text-white/80">Enable Model Restoration</label>
          <input
            type="checkbox"
            checked={settings.enableModelRestoration}
            onChange={(e) => setSettings({ enableModelRestoration: e.target.checked })}
            className="lg-checkbox"
          />
        </div>

      </div>
    </div>
  );
};

export default FeatureSettings;
