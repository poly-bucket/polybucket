import React, { useState, useEffect } from 'react';
import { Settings as SettingsIcon } from '@mui/icons-material';
import type { ModelConfigurationSettings } from '../../services/modelConfigurationSettingsService';
import modelConfigurationSettingsService from '../../services/modelConfigurationSettingsService';
import UploadSettings from './UploadSettings';
import ContentSettings from './ContentSettings';
import ModerationSettings from '../../components/admin/tabs/components/ModerationSettings';
import FeatureSettings from './FeatureSettings';

const ModelConfigurationSettingsComponent: React.FC = () => {
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState<string | null>(null);
  const [settings, setSettings] = useState<ModelConfigurationSettings>({
    allowAnonUploads: false,
    requireUploadModeration: true,
    defaultPrivacySetting: "Public",
    allowAnonDownloads: true,
    enableModelVersioning: true,
    limitTotalModels: 1000,
    allowNSFWContent: false,
    allowAIGeneratedContent: true,
    requireModelDescription: true,
    requireModelTags: false,
    minDescriptionLength: 10,
    maxDescriptionLength: 2000,
    maxTagsPerModel: 10,
    autoApproveVerifiedUsers: false,
    requireThumbnail: false,
    allowModelRemixing: true,
    requireRemixAttribution: true,
    maxModelsPerUser: 100,
    enableModelComments: true,
    enableModelLikes: true,
    enableModelDownloads: true,
    requireLicenseSelection: true,
    allowCustomLicenses: false,
    enableModelCollections: true,
    requireCategorySelection: false,
    maxCategoriesPerModel: 3,
    enableModelSharing: true,
    enableModelEmbedding: true,
    requireModelPreview: false,
    autoGenerateModelPreviews: true,
    enableModelAnalytics: true,
    requireUserAgreement: false,
    userAgreementText: "",
    enableModelExport: true,
    enableModelImport: false,
    requireModelValidation: false,
    enableModelBackup: true,
    modelBackupRetentionDays: 90,
    enableModelArchiving: true,
    modelArchiveThresholdDays: 365,
    requireModeratorApproval: true,
    enableAutoModeration: false,
    requireContentRating: false,
    enableModelFlagging: true,
    requireFlagReason: true,
    enableModelReporting: true,
    requireReportDetails: true,
    enableModelBlocking: true,
    requireBlockReason: true,
    enableModelWhitelisting: false,
    enableModelBlacklisting: false,
    requireModelApproval: true,
    enableModelRejection: true,
    requireRejectionReason: true,
    enableModelAppeals: true,
    requireAppealDetails: true,
    enableModelLocking: true,
    requireLockReason: true,
    enableModelUnlocking: true,
    requireUnlockApproval: true,
    enableModelDeletion: true,
    requireDeletionApproval: true,
    requireDeletionReason: true,
    enableModelRestoration: true,
    requireRestorationApproval: true
  });

  const fetchSettings = async () => {
    try {
      setLoading(true);
      setMessage(null);
      const response = await modelConfigurationSettingsService.getSettings();
      if (response.success && response.settings) {
        setSettings(response.settings);
      } else {
        setMessage(`Failed to load settings: ${response.message}`);
      }
    } catch (err) {
      console.error('Error loading model configuration settings:', err);
      setMessage('Error loading settings. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const saveSettings = async () => {
    try {
      setSaving(true);
      setMessage(null);
      const response = await modelConfigurationSettingsService.updateSettings(settings);
      if (response.success) {
        setMessage('Settings saved successfully!');
        setTimeout(() => setMessage(null), 3000);
      } else {
        setMessage(`Failed to save settings: ${response.message}`);
      }
    } catch (err) {
      console.error('Error saving model configuration settings:', err);
      setMessage('Error saving settings. Please try again.');
    } finally {
      setSaving(false);
    }
  };

  const updateSettings = (updates: Partial<ModelConfigurationSettings>) => {
    setSettings(prev => ({
      ...prev,
      ...updates
    }));
  };

  useEffect(() => {
    fetchSettings();
  }, []);

  return (
    <div className="lg-card p-6">
      <div className="flex justify-between items-center mb-6">
        <h3 className="text-lg font-medium text-white flex items-center">
          <SettingsIcon className="w-5 h-5 mr-2 text-blue-400" />
          Model Configuration Settings
        </h3>
        <div className="flex gap-2">
          <button
            onClick={fetchSettings}
            disabled={loading}
            className="lg-button lg-button-secondary"
          >
            {loading ? 'Loading...' : 'Refresh'}
          </button>
          <button
            onClick={saveSettings}
            disabled={saving}
            className="lg-button lg-button-primary"
          >
            {saving ? 'Saving...' : 'Save Settings'}
          </button>
        </div>
      </div>

      {message && (
        <div className={`mb-4 p-3 rounded ${
          message.includes('successfully') 
            ? 'bg-green-500/20 border border-green-500/50 text-green-300' 
            : 'bg-red-500/20 border border-red-500/50 text-red-300'
        }`}>
          {message}
        </div>
      )}

      {loading ? (
        <div className="text-center text-white/60 py-8">Loading settings...</div>
      ) : (
        <div className="space-y-6">
          <UploadSettings 
            settings={settings} 
            setSettings={updateSettings}
          />
          <ContentSettings 
            settings={settings} 
            setSettings={updateSettings}
          />
          <ModerationSettings 
            settings={settings} 
            setSettings={updateSettings}
          />
          <FeatureSettings 
            settings={settings} 
            setSettings={updateSettings}
          />
        </div>
      )}
    </div>
  );
};

export default ModelConfigurationSettingsComponent;
