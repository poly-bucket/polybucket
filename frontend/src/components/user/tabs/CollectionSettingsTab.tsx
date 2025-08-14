import React, { useState, useEffect } from 'react';
import { Folder, Lock, Public, Group, Settings } from '@mui/icons-material';

const CollectionSettingsTab: React.FC = () => {
  const [collectionDefaults, setCollectionDefaults] = useState({
    defaultPrivacy: 'private',
    defaultAllowComments: true,
    defaultAllowDownloads: true,
    defaultRequireApproval: false,
    defaultTags: [] as string[],
    defaultDescription: '',
    autoArchiveInactive: true,
    archiveAfterDays: 90,
    maxCollections: 50,
    allowPublicCollections: true,
    allowPrivateCollections: true,
    allowSharedCollections: true
  });

  const [isSaving, setIsSaving] = useState(false);
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' as 'success' | 'error' });

  useEffect(() => {
    // Load user's collection defaults from API
    loadCollectionDefaults();
  }, []);

  const loadCollectionDefaults = async () => {
    try {
      // Mock API call - replace with actual endpoint
      console.log('Loading collection defaults...');
    } catch (error) {
      console.error('Failed to load collection defaults:', error);
    }
  };

  const handleDefaultChange = (field: string, value: any) => {
    setCollectionDefaults(prev => ({ ...prev, [field]: value }));
  };

  const handleTagChange = (index: number, value: string) => {
    const newTags = [...collectionDefaults.defaultTags];
    newTags[index] = value;
    setCollectionDefaults(prev => ({ ...prev, defaultTags: newTags }));
  };

  const addTag = () => {
    if (collectionDefaults.defaultTags.length < 10) {
      setCollectionDefaults(prev => ({
        ...prev,
        defaultTags: [...prev.defaultTags, '']
      }));
    }
  };

  const removeTag = (index: number) => {
    const newTags = collectionDefaults.defaultTags.filter((_, i) => i !== index);
    setCollectionDefaults(prev => ({ ...prev, defaultTags: newTags }));
  };

  const saveDefaults = async () => {
    setIsSaving(true);
    try {
      // Mock API call - replace with actual endpoint
      await new Promise(resolve => setTimeout(resolve, 1000));
      setSnackbar({ open: true, message: 'Collection defaults saved successfully', severity: 'success' });
    } catch (error) {
      setSnackbar({ open: true, message: 'Failed to save collection defaults', severity: 'error' });
    } finally {
      setIsSaving(false);
    }
  };

  const resetToSystemDefaults = () => {
    setCollectionDefaults({
      defaultPrivacy: 'private',
      defaultAllowComments: true,
      defaultAllowDownloads: true,
      defaultRequireApproval: false,
      defaultTags: [],
      defaultDescription: '',
      autoArchiveInactive: true,
      archiveAfterDays: 90,
      maxCollections: 50,
      allowPublicCollections: true,
      allowPrivateCollections: true,
      allowSharedCollections: true
    });
    setSnackbar({ open: true, message: 'Reset to system defaults', severity: 'success' });
  };

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold text-white">Collection Settings</h2>
      
      {/* Default Privacy Settings */}
      <div className="lg-card p-6">
        <h3 className="text-lg font-medium text-white mb-4">Default Privacy Settings</h3>
        
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-white/80 mb-2">Default Collection Privacy</label>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
              <label className="flex items-center p-3 border border-white/20 rounded-lg cursor-pointer hover:bg-white/5">
                <input
                  type="radio"
                  name="defaultPrivacy"
                  value="private"
                  checked={collectionDefaults.defaultPrivacy === 'private'}
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
                  checked={collectionDefaults.defaultPrivacy === 'shared'}
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
                  checked={collectionDefaults.defaultPrivacy === 'public'}
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
                <div className="text-sm text-white/60">Users can comment on models in collections</div>
              </div>
              <input
                type="checkbox"
                checked={collectionDefaults.defaultAllowComments}
                onChange={(e) => handleDefaultChange('defaultAllowComments', e.target.checked)}
                className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
              />
            </div>
            
            <div className="flex items-center justify-between">
              <div>
                <div className="text-white font-medium">Allow Downloads</div>
                <div className="text-sm text-white/60">Users can download models from collections</div>
              </div>
              <input
                type="checkbox"
                checked={collectionDefaults.defaultAllowDownloads}
                onChange={(e) => handleDefaultChange('defaultAllowDownloads', e.target.checked)}
                className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
              />
            </div>
            
            <div className="flex items-center justify-between">
              <div>
                <div className="text-white font-medium">Require Approval</div>
                <div className="text-sm text-white/60">New models need approval before publishing</div>
              </div>
              <input
                type="checkbox"
                checked={collectionDefaults.defaultRequireApproval}
                onChange={(e) => handleDefaultChange('defaultRequireApproval', e.target.checked)}
                className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
              />
            </div>
            
            <div className="flex items-center justify-between">
              <div>
                <div className="text-white font-medium">Auto-archive Inactive</div>
                <div className="text-sm text-white/60">Automatically archive inactive collections</div>
              </div>
              <input
                type="checkbox"
                checked={collectionDefaults.autoArchiveInactive}
                onChange={(e) => handleDefaultChange('autoArchiveInactive', e.target.checked)}
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
            <label className="block text-sm font-medium text-white/80 mb-2">Default Description Template</label>
            <textarea
              value={collectionDefaults.defaultDescription}
              onChange={(e) => handleDefaultChange('defaultDescription', e.target.value)}
              rows={3}
              className="lg-input"
              placeholder="Enter a default description template for new collections..."
            />
            <p className="text-sm text-white/60 mt-1">
              Use placeholders like {[`{name}`, `{category}`, `{date}`].join(', ')} for dynamic content
            </p>
          </div>
          
          <div>
            <label className="block text-sm font-medium text-white/80 mb-2">Default Tags</label>
            <div className="space-y-2">
              {collectionDefaults.defaultTags.map((tag, index) => (
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
              {collectionDefaults.defaultTags.length < 10 && (
                <button
                  onClick={addTag}
                  className="lg-button lg-button-secondary text-sm"
                >
                  + Add Tag
                </button>
              )}
            </div>
            <p className="text-sm text-white/60 mt-1">Maximum 10 default tags</p>
          </div>
        </div>
      </div>

      {/* Collection Limits and Permissions */}
      <div className="lg-card p-6">
        <h3 className="text-lg font-medium text-white mb-4">Collection Limits & Permissions</h3>
        
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-white/80 mb-2">Maximum Collections</label>
            <input
              type="number"
              value={collectionDefaults.maxCollections}
              onChange={(e) => handleDefaultChange('maxCollections', parseInt(e.target.value))}
              className="lg-input"
              min="1"
              max="1000"
            />
            <p className="text-sm text-white/60 mt-1">Maximum number of collections you can create</p>
          </div>
          
          <div>
            <label className="block text-sm font-medium text-white/80 mb-2">Archive After (Days)</label>
            <input
              type="number"
              value={collectionDefaults.archiveAfterDays}
              onChange={(e) => handleDefaultChange('archiveAfterDays', parseInt(e.target.value))}
              className="lg-input"
              min="30"
              max="365"
            />
            <p className="text-sm text-white/60 mt-1">Days of inactivity before auto-archiving</p>
          </div>
        </div>
        
        <div className="mt-4 space-y-3">
          <div className="flex items-center justify-between">
            <div>
              <div className="text-white font-medium">Allow Public Collections</div>
              <div className="text-sm text-white/60">Create collections visible to everyone</div>
            </div>
            <input
              type="checkbox"
              checked={collectionDefaults.allowPublicCollections}
              onChange={(e) => handleDefaultChange('allowPublicCollections', e.target.checked)}
              className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
            />
          </div>
          
          <div className="flex items-center justify-between">
            <div>
              <div className="text-white font-medium">Allow Private Collections</div>
              <div className="text-sm text-white/60">Create collections visible only to you</div>
            </div>
            <input
              type="checkbox"
              checked={collectionDefaults.allowPrivateCollections}
              onChange={(e) => handleDefaultChange('allowPrivateCollections', e.target.checked)}
              className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
            />
          </div>
          
          <div className="flex items-center justify-between">
            <div>
              <div className="text-white font-medium">Allow Shared Collections</div>
              <div className="text-sm text-white/60">Create collections shared with specific users</div>
            </div>
            <input
              type="checkbox"
              checked={collectionDefaults.allowSharedCollections}
              onChange={(e) => handleDefaultChange('allowSharedCollections', e.target.checked)}
              className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
            />
          </div>
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
            <h4 className="text-lg font-medium text-blue-300 mb-2">How Collection Defaults Work</h4>
            <div className="text-blue-200/80 space-y-2 text-sm">
              <p>
                These settings will be applied as defaults when you create new collections. You can still override 
                individual settings when creating specific collections.
              </p>
              <p>
                <strong>Privacy:</strong> Determines who can see your collections by default. Private collections 
                are only visible to you, shared collections are visible to invited users, and public collections 
                are visible to everyone.
              </p>
              <p>
                <strong>Auto-archiving:</strong> Collections that haven't been updated in the specified number 
                of days will be automatically archived to keep your profile organized.
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

export default CollectionSettingsTab;
