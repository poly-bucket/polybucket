import React, { useState, useEffect } from 'react';
import { Settings as SettingsIcon } from '@mui/icons-material';
import type { SiteModelSettings } from '../../../../services/siteModelSettingsService';
import siteModelSettingsService from '../../../../services/siteModelSettingsService';
import FileUploadSettings from '../../../../acp/model-settings/FileUploadSettings';
import ModelBehaviorSettings from '../../../../acp/model-settings/ModelBehaviorSettings';

const SiteModelSettingsComponent: React.FC = () => {
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [settings, setSettings] = useState<SiteModelSettings>({
    maxFileSizeBytes: 100 * 1024 * 1024, // 100MB default
    allowedFileTypes: ".stl,.obj,.fbx,.3ds,.glb,.gltf,.ply,.step,.stp,.iges,.igs,.brep,.png,.jpg,.jpeg,.gif",
    maxFilesPerUpload: 5,
    enableFileCompression: true,
    autoGeneratePreviews: true,
    defaultModelPrivacy: "Public",
    autoApproveModels: false,
    requireModeration: true,
    requireLoginForUpload: true,
    allowPublicBrowsing: true
  } as SiteModelSettings);

  const fetchSettings = async () => {
    try {
      setLoading(true);
      const response = await siteModelSettingsService.getSettings();
      if (response.success && response.settings) {
        setSettings(response.settings);
      }
    } catch (err) {
      console.error('Error loading settings:', err);
    } finally {
      setLoading(false);
    }
  };

  const saveSettings = async () => {
    try {
      setSaving(true);
      const response = await siteModelSettingsService.updateSettings(settings);
      if (response.success) {
        // Settings saved successfully
        console.log('Settings saved successfully');
      } else {
        console.error('Failed to update settings:', response.message);
      }
    } catch (err) {
      console.error('Error saving settings:', err);
    } finally {
      setSaving(false);
    }
  };

  const updateSettings = (updates: Partial<SiteModelSettings>) => {
    setSettings(prev => {
      const newSettings = { ...prev, ...updates };
      return newSettings;
    });
  };

  useEffect(() => {
    fetchSettings();
  }, []);

  return (
    <div className="lg-card p-6">
      <div className="flex justify-between items-center mb-6">
        <h3 className="text-lg font-medium text-white flex items-center">
          <SettingsIcon className="w-5 h-5 mr-2 text-blue-400" />
          Site Model Settings
        </h3>
        <button
          onClick={saveSettings}
          disabled={saving}
          className="lg-button lg-button-primary"
        >
          {saving ? 'Saving...' : 'Save Settings'}
        </button>
      </div>

      {loading ? (
        <div className="text-center text-white/60 py-8">Loading settings...</div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <FileUploadSettings 
            settings={settings} 
            setSettings={updateSettings}
          />
          <ModelBehaviorSettings 
            settings={settings} 
            setSettings={updateSettings}
          />
        </div>
      )}
    </div>
  );
};

export default SiteModelSettingsComponent;
