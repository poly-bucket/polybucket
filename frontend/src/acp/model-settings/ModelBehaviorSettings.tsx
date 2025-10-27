import React from 'react';
import { SiteModelSettings } from '../../services/siteModelSettingsService';

interface ModelBehaviorSettingsProps {
  settings: SiteModelSettings;
  setSettings: (updates: Partial<SiteModelSettings>) => void;
}

const ModelBehaviorSettings: React.FC<ModelBehaviorSettingsProps> = ({ settings, setSettings }) => {
  return (
    <div className="space-y-4">
      <h4 className="text-md font-medium text-white mb-3">Model Behavior Settings</h4>
      
      <div>
        <label className="block text-sm font-medium text-white/80 mb-2">
          Default Model Privacy
        </label>
        <select
          value={settings.defaultModelPrivacy || 'Public'}
          onChange={(e) => setSettings({
            defaultModelPrivacy: e.target.value
          })}
          className="lg-input w-full"
        >
          <option value="Public">Public</option>
          <option value="Private">Private</option>
          <option value="Unlisted">Unlisted</option>
        </select>
      </div>

      <div className="flex items-center space-x-3">
        <input
          type="checkbox"
          id="autoApproveModels"
          checked={settings.autoApproveModels || false}
          onChange={(e) => setSettings({
            autoApproveModels: e.target.checked
          })}
          className="w-4 h-4 text-blue-600 bg-white/10 border-white/20 rounded focus:ring-blue-500"
        />
        <label htmlFor="autoApproveModels" className="text-sm text-white/80">
          Auto-approve Models
        </label>
      </div>

      <div className="flex items-center space-x-3">
        <input
          type="checkbox"
          id="requireModeration"
          checked={settings.requireModeration || false}
          onChange={(e) => setSettings({
            requireModeration: e.target.checked
          })}
          className="w-4 h-4 text-blue-600 bg-white/10 border-white/20 rounded focus:ring-blue-500"
        />
        <label htmlFor="requireModeration" className="text-sm text-white/80">
          Require Moderation
        </label>
      </div>

      <div className="flex items-center space-x-3">
        <input
          type="checkbox"
          id="requireLoginForUpload"
          checked={settings.requireLoginForUpload || false}
          onChange={(e) => setSettings({
            requireLoginForUpload: e.target.checked
          })}
          className="w-4 h-4 text-blue-600 bg-white/10 border-white/20 rounded focus:ring-blue-500"
        />
        <label htmlFor="requireLoginForUpload" className="text-sm text-white/80">
          Require Login for Upload
        </label>
      </div>

      <div className="flex items-center space-x-3">
        <input
          type="checkbox"
          id="allowPublicBrowsing"
          checked={settings.allowPublicBrowsing || false}
          onChange={(e) => setSettings({
            allowPublicBrowsing: e.target.checked
          })}
          className="w-4 h-4 text-blue-600 bg-white/10 border-white/20 rounded focus:ring-blue-500"
        />
        <label htmlFor="allowPublicBrowsing" className="text-sm text-white/80">
          Allow Public Browsing
        </label>
      </div>
    </div>
  );
};

export default ModelBehaviorSettings;
