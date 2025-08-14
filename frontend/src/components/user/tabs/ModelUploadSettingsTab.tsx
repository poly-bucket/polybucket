import React, { useState, useEffect } from 'react';
import { Upload, Settings, Category, Tag, Description, Lock, Public, Group } from '@mui/icons-material';

const ModelUploadSettingsTab: React.FC = () => {
  const [uploadDefaults, setUploadDefaults] = useState({
    defaultPrivacy: 'private',
    defaultAllowComments: true,
    defaultAllowDownloads: true,
    defaultRequireApproval: false,
    defaultCategory: '',
    defaultTags: [] as string[],
    defaultDescription: '',
    defaultLicense: 'all-rights-reserved',
    autoGenerateThumbnails: true,
    autoOptimizeModels: true,
    maxFileSize: 100,
    allowedFileTypes: ['stl', 'obj', '3mf', 'blend', 'fbx'],
    watermarkSettings: {
      enabled: false,
      text: '',
      position: 'bottom-right',
      opacity: 0.7
    },
    qualitySettings: {
      preserveOriginal: true,
      generateLowPoly: false,
      generateMediumPoly: true,
      generateHighPoly: false
    }
  });

  const [isSaving, setIsSaving] = useState(false);
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' as 'success' | 'error' });

  useEffect(() => {
    // Load user's upload defaults from API
    loadUploadDefaults();
  }, []);

  const loadUploadDefaults = async () => {
    try {
      // Mock API call - replace with actual endpoint
      console.log('Loading upload defaults...');
    } catch (error) {
      console.error('Failed to load upload defaults:', error);
    }
  };

  const handleDefaultChange = (field: string, value: any) => {
    setUploadDefaults(prev => ({ ...prev, [field]: value }));
  };

  const handleWatermarkChange = (field: string, value: any) => {
    setUploadDefaults(prev => ({
      ...prev,
      watermarkSettings: {
        ...prev.watermarkSettings,
        [field]: value
      }
    }));
  };

  const handleQualityChange = (field: string, value: any) => {
    setUploadDefaults(prev => ({
      ...prev,
      qualitySettings: {
        ...prev.qualitySettings,
        [field]: value
      }
    }));
  };

  const handleTagChange = (index: number, value: string) => {
    const newTags = [...uploadDefaults.defaultTags];
    newTags[index] = value;
    setUploadDefaults(prev => ({ ...prev, defaultTags: newTags }));
  };

  const addTag = () => {
    if (uploadDefaults.defaultTags.length < 15) {
      setUploadDefaults(prev => ({
        ...prev,
        defaultTags: [...prev.defaultTags, '']
      }));
    }
  };

  const removeTag = (index: number) => {
    const newTags = uploadDefaults.defaultTags.filter((_, i) => i !== index);
    setUploadDefaults(prev => ({ ...prev, defaultTags: newTags }));
  };

  const saveDefaults = async () => {
    setIsSaving(true);
    try {
      // Mock API call - replace with actual endpoint
      await new Promise(resolve => setTimeout(resolve, 1000));
      setSnackbar({ open: true, message: 'Upload defaults saved successfully', severity: 'success' });
    } catch (error) {
      setSnackbar({ open: true, message: 'Failed to save upload defaults', severity: 'error' });
    } finally {
      setIsSaving(false);
    }
  };

  const resetToSystemDefaults = () => {
    setUploadDefaults({
      defaultPrivacy: 'private',
      defaultAllowComments: true,
      defaultAllowDownloads: true,
      defaultRequireApproval: false,
      defaultCategory: '',
      defaultTags: [],
      defaultDescription: '',
      defaultLicense: 'all-rights-reserved',
      autoGenerateThumbnails: true,
      autoOptimizeModels: true,
      maxFileSize: 100,
      allowedFileTypes: ['stl', 'obj', '3mf', 'blend', 'fbx'],
      watermarkSettings: {
        enabled: false,
        text: '',
        position: 'bottom-right',
        opacity: 0.7
      },
      qualitySettings: {
        preserveOriginal: true,
        generateLowPoly: false,
        generateMediumPoly: true,
        generateHighPoly: false
      }
    });
    setSnackbar({ open: true, message: 'Reset to system defaults', severity: 'success' });
  };

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold text-white">Model Upload Settings</h2>
      
      {/* Default Privacy and Permissions */}
      <div className="lg-card p-6">
        <h3 className="text-lg font-medium text-white mb-4">Default Privacy & Permissions</h3>
        
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-white/80 mb-2">Default Model Privacy</label>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
              <label className="flex items-center p-3 border border-white/20 rounded-lg cursor-pointer hover:bg-white/5">
                <input
                  type="radio"
                  name="defaultPrivacy"
                  value="private"
                  checked={uploadDefaults.defaultPrivacy === 'private'}
                  onChange={(e) => handleDefaultChange('defaultPrivacy', e.target.value)}
                  className="mr-3"
                />
                <div className="flex items-center space-x-2">
                  <Lock className="w-5 h-5 text-red-400" />
                  <div>
                    <div className="text-white font-medium">Private</div>
                    <div className="text-sm text-white/60">Only you can see</div>
                  </div>
                </div>
              </label>
              
              <label className="flex items-center p-3 border border-white/20 rounded-lg cursor-pointer hover:bg-white/5">
                <input
                  type="radio"
                  name="defaultPrivacy"
                  value="shared"
                  checked={uploadDefaults.defaultPrivacy === 'shared'}
                  onChange={(e) => handleDefaultChange('defaultPrivacy', e.target.value)}
                  className="mr-3"
                />
                <div className="flex items-center space-x-2">
                  <Group className="w-5 h-5 text-yellow-400" />
                  <div>
                    <div className="text-white font-medium">Shared</div>
                    <div className="text-sm text-white/60">Invited users only</div>
                  </div>
                </div>
              </label>
              
              <label className="flex items-center p-3 border border-white/20 rounded-lg cursor-pointer hover:bg-white/5">
                <input
                  type="radio"
                  name="defaultPrivacy"
                  value="public"
                  checked={uploadDefaults.defaultPrivacy === 'public'}
                  onChange={(e) => handleDefaultChange('defaultPrivacy', e.target.value)}
                  className="mr-3"
                />
                <div className="flex items-center space-x-2">
                  <Public className="w-5 h-5 text-green-400" />
                  <div>
                    <div className="text-white font-medium">Public</div>
                    <div className="text-sm text-white/60">Everyone can see</div>
                  </div>
                </div>
              </label>
            </div>
          </div>
          
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="flex items-center justify-between">
              <div>
                <div className="text-white font-medium">Allow Comments</div>
                <div className="text-sm text-white/60">Users can comment on your models</div>
              </div>
              <input
                type="checkbox"
                checked={uploadDefaults.defaultAllowComments}
                onChange={(e) => handleDefaultChange('defaultAllowComments', e.target.checked)}
                className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
              />
            </div>
            
            <div className="flex items-center justify-between">
              <div>
                <div className="text-white font-medium">Allow Downloads</div>
                <div className="text-sm text-white/60">Users can download your models</div>
              </div>
              <input
                type="checkbox"
                checked={uploadDefaults.defaultAllowDownloads}
                onChange={(e) => handleDefaultChange('defaultAllowDownloads', e.target.checked)}
                className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
              />
            </div>
            
            <div className="flex items-center justify-between">
              <div>
                <div className="text-white font-medium">Require Approval</div>
                <div className="text-sm text-white/60">Models need approval before publishing</div>
              </div>
              <input
                type="checkbox"
                checked={uploadDefaults.defaultRequireApproval}
                onChange={(e) => handleDefaultChange('defaultRequireApproval', e.target.checked)}
                className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
              />
            </div>
            
            <div className="flex items-center justify-between">
              <div>
                <div className="text-white font-medium">Auto-generate Thumbnails</div>
                <div className="text-sm text-white/60">Automatically create model previews</div>
              </div>
              <input
                type="checkbox"
                checked={uploadDefaults.autoGenerateThumbnails}
                onChange={(e) => handleDefaultChange('autoGenerateThumbnails', e.target.checked)}
                className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
              />
            </div>
          </div>
        </div>
      </div>

      {/* Default Content Settings */}
      <div className="lg-card p-6">
        <h3 className="text-lg font-medium text-white mb-4">Default Content Settings</h3>
        
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-white/80 mb-2">Default Category</label>
            <select
              value={uploadDefaults.defaultCategory}
              onChange={(e) => handleDefaultChange('defaultCategory', e.target.value)}
              className="lg-input"
            >
              <option value="">Select a category</option>
              <option value="artistic">Artistic</option>
              <option value="functional">Functional</option>
              <option value="mechanical">Mechanical</option>
              <option value="architectural">Architectural</option>
              <option value="characters">Characters</option>
              <option value="vehicles">Vehicles</option>
              <option value="electronics">Electronics</option>
              <option value="jewelry">Jewelry</option>
              <option value="other">Other</option>
            </select>
          </div>
          
          <div>
            <label className="block text-sm font-medium text-white/80 mb-2">Default Description Template</label>
            <textarea
              value={uploadDefaults.defaultDescription}
              onChange={(e) => handleDefaultChange('defaultDescription', e.target.value)}
              rows={3}
              className="lg-input"
              placeholder="Enter a default description template for new models..."
            />
            <p className="text-sm text-white/60 mt-1">
              Use placeholders like {[`{name}`, `{category}`, `{date}`, `{creator}`].join(', ')} for dynamic content
            </p>
          </div>
          
          <div>
            <label className="block text-sm font-medium text-white/80 mb-2">Default Tags</label>
            <div className="space-y-2">
              {uploadDefaults.defaultTags.map((tag, index) => (
                <div key={index} className="flex items-center space-x-2">
                  <input
                    type="text"
                    value={tag}
                    onChange={(e) => handleTagChange(index, e.target.value)}
                    className="lg-input flex-1"
                    placeholder="Enter tag name"
                  />
                  <button
                    onClick={() => removeTag(index)}
                    className="lg-button lg-button-secondary w-10 h-10 p-0"
                  >
                    ×
                  </button>
                </div>
              ))}
              {uploadDefaults.defaultTags.length < 15 && (
                <button
                  onClick={addTag}
                  className="lg-button lg-button-secondary text-sm"
                >
                  + Add Tag
                </button>
              )}
            </div>
            <p className="text-sm text-white/60 mt-1">Maximum 15 default tags</p>
          </div>
          
          <div>
            <label className="block text-sm font-medium text-white/80 mb-2">Default License</label>
            <select
              value={uploadDefaults.defaultLicense}
              onChange={(e) => handleDefaultChange('defaultLicense', e.target.value)}
              className="lg-input"
            >
              <option value="all-rights-reserved">All Rights Reserved</option>
              <option value="creative-commons">Creative Commons</option>
              <option value="public-domain">Public Domain</option>
              <option value="mit">MIT License</option>
              <option value="gpl">GPL License</option>
              <option value="custom">Custom License</option>
            </select>
          </div>
        </div>
      </div>

      {/* File Processing Settings */}
      <div className="lg-card p-6">
        <h3 className="text-lg font-medium text-white mb-4">File Processing Settings</h3>
        
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-white/80 mb-2">Maximum File Size (MB)</label>
            <input
              type="number"
              value={uploadDefaults.maxFileSize}
              onChange={(e) => handleDefaultChange('maxFileSize', parseInt(e.target.value))}
              className="lg-input"
              min="1"
              max="1000"
            />
            <p className="text-sm text-white/60 mt-1">Maximum file size for uploads</p>
          </div>
          
          <div>
            <label className="block text-sm font-medium text-white/80 mb-2">Auto-optimize Models</label>
            <div className="flex items-center justify-between">
              <div className="text-sm text-white/60">Automatically optimize models for web viewing</div>
              <input
                type="checkbox"
                checked={uploadDefaults.autoOptimizeModels}
                onChange={(e) => handleDefaultChange('autoOptimizeModels', e.target.checked)}
                className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
              />
            </div>
          </div>
        </div>
        
        <div className="mt-4">
          <label className="block text-sm font-medium text-white/80 mb-2">Quality Settings</label>
          <div className="space-y-3">
            <div className="flex items-center justify-between">
              <div>
                <div className="text-white font-medium">Preserve Original</div>
                <div className="text-sm text-white/60">Keep the original high-quality file</div>
              </div>
              <input
                type="checkbox"
                checked={uploadDefaults.qualitySettings.preserveOriginal}
                onChange={(e) => handleQualityChange('preserveOriginal', e.target.checked)}
                className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
              />
            </div>
            
            <div className="flex items-center justify-between">
              <div>
                <div className="text-white font-medium">Generate Low Poly</div>
                <div className="text-sm text-white/60">Create low-poly version for fast loading</div>
              </div>
              <input
                type="checkbox"
                checked={uploadDefaults.qualitySettings.generateLowPoly}
                onChange={(e) => handleQualityChange('generateLowPoly', e.target.checked)}
                className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
              />
            </div>
            
            <div className="flex items-center justify-between">
              <div>
                <div className="text-white font-medium">Generate Medium Poly</div>
                <div className="text-sm text-white/60">Create medium-poly version for balance</div>
              </div>
              <input
                type="checkbox"
                checked={uploadDefaults.qualitySettings.generateMediumPoly}
                onChange={(e) => handleQualityChange('generateMediumPoly', e.target.checked)}
                className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
              />
            </div>
            
            <div className="flex items-center justify-between">
              <div>
                <div className="text-white font-medium">Generate High Poly</div>
                <div className="text-sm text-white/60">Create high-poly version for quality</div>
              </div>
              <input
                type="checkbox"
                checked={uploadDefaults.qualitySettings.generateHighPoly}
                onChange={(e) => handleQualityChange('generateHighPoly', e.target.checked)}
                className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
              />
            </div>
          </div>
        </div>
      </div>

      {/* Watermark Settings */}
      <div className="lg-card p-6">
        <h3 className="text-lg font-medium text-white mb-4">Watermark Settings</h3>
        
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <div>
              <div className="text-white font-medium">Enable Watermarks</div>
              <div className="text-sm text-white/60">Add watermarks to your uploaded models</div>
            </div>
            <input
              type="checkbox"
              checked={uploadDefaults.watermarkSettings.enabled}
              onChange={(e) => handleWatermarkChange('enabled', e.target.checked)}
              className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
            />
          </div>
          
          {uploadDefaults.watermarkSettings.enabled && (
            <>
              <div>
                <label className="block text-sm font-medium text-white/80 mb-2">Watermark Text</label>
                <input
                  type="text"
                  value={uploadDefaults.watermarkSettings.text}
                  onChange={(e) => handleWatermarkChange('text', e.target.value)}
                  className="lg-input"
                  placeholder="Enter watermark text (e.g., your username)"
                />
              </div>
              
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-white/80 mb-2">Position</label>
                  <select
                    value={uploadDefaults.watermarkSettings.position}
                    onChange={(e) => handleWatermarkChange('position', e.target.value)}
                    className="lg-input"
                  >
                    <option value="top-left">Top Left</option>
                    <option value="top-right">Top Right</option>
                    <option value="bottom-left">Bottom Left</option>
                    <option value="bottom-right">Bottom Right</option>
                    <option value="center">Center</option>
                  </select>
                </div>
                
                <div>
                  <label className="block text-sm font-medium text-white/80 mb-2">Opacity</label>
                  <input
                    type="range"
                    min="0.1"
                    max="1"
                    step="0.1"
                    value={uploadDefaults.watermarkSettings.opacity}
                    onChange={(e) => handleWatermarkChange('opacity', parseFloat(e.target.value))}
                    className="w-full"
                  />
                  <div className="text-sm text-white/60 mt-1">
                    {Math.round(uploadDefaults.watermarkSettings.opacity * 100)}%
                  </div>
                </div>
              </div>
            </>
          )}
        </div>
      </div>

      {/* Actions */}
      <div className="flex justify-between items-center">
        <button
          onClick={resetToSystemDefaults}
          className="lg-button lg-button-secondary"
        >
          Reset to System Defaults
        </button>
        
        <button
          onClick={saveDefaults}
          disabled={isSaving}
          className="lg-button lg-button-primary"
        >
          {isSaving ? 'Saving...' : 'Save Defaults'}
        </button>
      </div>

      {/* Information Panel */}
      <div className="lg-card p-6 bg-blue-900/20 border-blue-500/30">
        <div className="flex items-start space-x-3">
          <Settings className="w-6 h-6 text-blue-400 mt-1" />
          <div>
            <h4 className="text-lg font-medium text-blue-300 mb-2">How Upload Defaults Work</h4>
            <div className="text-blue-200/80 space-y-2 text-sm">
              <p>
                These settings will be applied as defaults when you upload new models. You can still override 
                individual settings during the upload process.
              </p>
              <p>
                <strong>Privacy:</strong> Determines who can see your models by default. Private models are only 
                visible to you, shared models are visible to invited users, and public models are visible to everyone.
              </p>
              <p>
                <strong>Quality Settings:</strong> Control which quality versions of your models are generated. 
                Lower quality versions load faster but may have reduced detail.
              </p>
              <p>
                <strong>Watermarks:</strong> Add your username or branding to models to protect your work and 
                build recognition.
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* Snackbar */}
      {snackbar.open && (
        <div className={`fixed bottom-4 right-4 p-4 rounded-lg shadow-lg z-50 ${
          snackbar.severity === 'success' ? 'bg-green-600' : 'bg-red-600'
        } text-white`}>
          <div className="flex items-center space-x-2">
            <span>{snackbar.message}</span>
            <button
              onClick={() => setSnackbar(prev => ({ ...prev, open: false }))}
              className="ml-2 text-white/80 hover:text-white"
            >
              ×
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

export default ModelUploadSettingsTab;
