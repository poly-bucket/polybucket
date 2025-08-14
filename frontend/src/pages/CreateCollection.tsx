import React, { useState, useEffect, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowLeftIcon } from '@heroicons/react/24/outline';
import NavigationBar from '../components/common/NavigationBar';
import IconSelector from '../components/common/IconSelector';
import collectionsService from '../services/collectionsService';
import { minidenticon } from 'minidenticons';

const CreateCollection: React.FC = () => {
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [visibility, setVisibility] = useState<'Public' | 'Private' | 'Unlisted'>('Private');
  const [password, setPassword] = useState('');
  const [selectedIcon, setSelectedIcon] = useState<string>('');
  const [iconType, setIconType] = useState<'fontawesome' | 'generated' | 'model-thumbnail'>('generated');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  
  // Generate a random salt for the collection avatar
  const [collectionSalt, setCollectionSalt] = useState('');
  const [generatedAvatar, setGeneratedAvatar] = useState<string>('');
  
  const navigate = useNavigate();

  // Generate random salt on component mount
  useEffect(() => {
    const generateRandomSalt = () => {
      const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
      let result = '';
      for (let i = 0; i < 8; i++) {
        result += chars.charAt(Math.floor(Math.random() * chars.length));
      }
      return result;
    };
    
    const salt = generateRandomSalt();
    setCollectionSalt(salt);
  }, []);

  // Generate avatar preview when salt or name changes
  useEffect(() => {
    if (collectionSalt && name.trim()) {
      const seed = `${name.trim()}-${collectionSalt}`;
      const avatar = minidenticon(seed, 50, 50);
      setGeneratedAvatar(avatar);
      
      // Auto-select the generated avatar if none is selected or if we're currently using generated type
      if (!selectedIcon || iconType === 'generated') {
        setSelectedIcon(avatar);
        setIconType('generated');
      }
    }
  }, [collectionSalt, name, iconType]);

  // Update selectedIcon when generatedAvatar changes to keep them in sync
  useEffect(() => {
    if (generatedAvatar && iconType === 'generated' && selectedIcon !== generatedAvatar) {
      setSelectedIcon(generatedAvatar);
    }
  }, [generatedAvatar, iconType, selectedIcon]);

  const handleIconSelect = (icon: string, type: 'fontawesome' | 'generated' | 'model-thumbnail') => {
    setSelectedIcon(icon);
    setIconType(type);
    
    // If user manually selects generated type, ensure we use the current generated avatar
    if (type === 'generated' && generatedAvatar) {
      setSelectedIcon(generatedAvatar);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!name.trim()) {
      setError('Collection name is required');
      return;
    }

    try {
      setLoading(true);
      setError('');
      
      // Prepare avatar data
      let avatarData: string | undefined;
      if (iconType === 'generated' && generatedAvatar) {
        avatarData = generatedAvatar;
      } else if (iconType === 'fontawesome' && selectedIcon) {
        avatarData = selectedIcon;
      }
      // Note: model-thumbnail type doesn't set avatar on creation
      
      await collectionsService.createCollection({
        name: name.trim(),
        description: description.trim() || undefined,
        visibility,
        password: password.trim() || undefined,
        avatar: avatarData
      });
      
      navigate('/my-collections');
    } catch (err) {
      console.error('Error creating collection:', err);
      setError('Failed to create collection. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const regenerateSalt = () => {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    let result = '';
    for (let i = 0; i < 8; i++) {
      result += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    setCollectionSalt(result);
    
    // Reset the selected icon to trigger regeneration
    if (iconType === 'generated') {
      setSelectedIcon('');
    }
  };

  return (
    <div className="lg-container min-h-screen text-white">
      <NavigationBar
        title="Create Collection"
        showSearch={false}
        showUploadButton={false}
        showHomeLink={true}
      />

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="flex gap-8">
          {/* Left Panel - Collection Form */}
          <div className="flex-1">
            <div className="flex items-center mb-6">
              <button
                onClick={() => navigate('/my-collections')}
                className="mr-4 p-2 text-gray-400 hover:text-gray-300 rounded-lg hover:bg-gray-700 transition-colors"
              >
                <ArrowLeftIcon className="w-5 h-5" />
              </button>
              <h1 className="text-3xl font-bold text-green-400">Create New Collection</h1>
            </div>

            {/* Form */}
            <div className="lg-card p-6">
              <form onSubmit={handleSubmit} className="space-y-6">
                {error && (
                  <div className="bg-red-900 border border-red-600 rounded-md p-4">
                    <p className="text-red-200">{error}</p>
                  </div>
                )}

                {/* Name */}
                <div>
                  <label htmlFor="name" className="block text-sm font-medium text-white mb-2">
                    Collection Name *
                  </label>
                  <input
                    type="text"
                    id="name"
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                    className="lg-input"
                    placeholder="Enter collection name"
                    maxLength={100}
                    required
                  />
                  <p className="mt-1 text-sm text-gray-400">
                    {name.length}/100 characters
                  </p>
                </div>

                {/* Description */}
                <div>
                  <label htmlFor="description" className="block text-sm font-medium text-white mb-2">
                    Description
                  </label>
                  <textarea
                    id="description"
                    value={description}
                    onChange={(e) => setDescription(e.target.value)}
                    rows={4}
                    className="lg-input resize-none"
                    placeholder="Describe your collection (optional)"
                    maxLength={500}
                  />
                  <p className="mt-1 text-sm text-gray-400">
                    {description.length}/500 characters
                  </p>
                </div>

                {/* Icon Selection */}
                <div>
                  <label className="block text-sm font-medium text-white mb-2">
                    Collection Icon
                  </label>
                  
                  {/* Auto-generated Avatar Preview */}
                  {generatedAvatar && (
                    <div className="mb-4 p-4 bg-gray-800 rounded-lg border border-gray-600">
                      <div className="flex items-center justify-between mb-3">
                        <h4 className="text-sm font-medium text-white">Auto-generated Avatar</h4>
                        <button
                          type="button"
                          onClick={regenerateSalt}
                          className="text-green-400 hover:text-green-300 text-sm underline"
                        >
                          Regenerate
                        </button>
                      </div>
                      <div className="flex items-center space-x-4">
                        <div 
                          className="w-16 h-16 rounded-lg overflow-hidden bg-gray-700 flex items-center justify-center"
                          dangerouslySetInnerHTML={{ __html: generatedAvatar }}
                        />
                        <div className="flex-1">
                          <p className="text-sm text-gray-300 mb-1">
                            Generated from: <span className="text-green-400">{name.trim() || 'Collection Name'}-{collectionSalt}</span>
                          </p>
                          <p className="text-xs text-gray-400">
                            This avatar is automatically generated and will be unique to your collection.
                          </p>
                        </div>
                      </div>
                    </div>
                  )}

                  <IconSelector
                    selectedIcon={selectedIcon}
                    onIconSelect={handleIconSelect}
                    collectionName={name}
                    collectionId={`${name.trim() || 'collection'}-${collectionSalt}`}
                    className="mb-6"
                  />
                </div>

                {/* Visibility */}
                <div>
                  <label className="block text-sm font-medium text-white mb-2">
                    Visibility
                  </label>
                  <div className="space-y-3">
                    <label className="flex items-center">
                      <input
                        type="radio"
                        name="visibility"
                        value="Public"
                        checked={visibility === 'Public'}
                        onChange={(e) => setVisibility(e.target.value as 'Public' | 'Private' | 'Unlisted')}
                        className="h-4 w-4 text-green-600 focus:ring-green-500"
                      />
                      <div className="ml-3">
                        <div className="text-sm font-medium text-white">Public</div>
                        <div className="text-sm text-gray-400">Anyone can see and browse this collection</div>
                      </div>
                    </label>
                    
                    <label className="flex items-center">
                      <input
                        type="radio"
                        name="visibility"
                        value="Unlisted"
                        checked={visibility === 'Unlisted'}
                        onChange={(e) => setVisibility(e.target.value as 'Public' | 'Private' | 'Unlisted')}
                        className="h-4 w-4 text-green-600 focus:ring-green-500"
                      />
                      <div className="ml-3">
                        <div className="text-sm font-medium text-white">Unlisted</div>
                        <div className="text-sm text-gray-400">Only people with the link can see this collection</div>
                      </div>
                    </label>
                    
                    <label className="flex items-center">
                      <input
                        type="radio"
                        name="visibility"
                        value="Private"
                        checked={visibility === 'Private'}
                        onChange={(e) => setVisibility(e.target.value as 'Public' | 'Private' | 'Unlisted')}
                        className="h-4 w-4 text-green-600 focus:ring-green-500"
                      />
                      <div className="ml-3">
                        <div className="text-sm font-medium text-white">Private</div>
                        <div className="text-sm text-gray-400">Only you can see this collection</div>
                      </div>
                    </label>
                  </div>
                </div>

                {/* Password Protection (only for Unlisted) */}
                {visibility === 'Unlisted' && (
                  <div>
                    <label htmlFor="password" className="block text-sm font-medium text-white mb-2">
                      Password Protection (Optional)
                    </label>
                    <input
                      type="password"
                      id="password"
                      value={password}
                      onChange={(e) => setPassword(e.target.value)}
                      className="lg-input"
                      placeholder="Enter password to protect this collection (optional)"
                      maxLength={100}
                    />
                    <p className="mt-1 text-sm text-gray-400">
                      If set, people will need this password to access your collection
                    </p>
                  </div>
                )}

                {/* Actions */}
                <div className="flex justify-end space-x-3 pt-6 border-t border-gray-600">
                  <button
                    type="button"
                    onClick={() => navigate('/my-collections')}
                    className="lg-button"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    disabled={loading || !name.trim()}
                    className="lg-button lg-button-primary disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    {loading ? (
                      <div className="flex items-center">
                        <div className="lg-spinner w-4 h-4 mr-2"></div>
                        Creating...
                      </div>
                    ) : (
                      'Create Collection'
                    )}
                  </button>
                </div>
              </form>
            </div>
          </div>

          {/* Right Panel - Collection Preview */}
          <div className="w-80">
            <div className="lg-card p-4">
              <h3 className="text-lg font-medium text-green-400 mb-4">Collection Preview</h3>
              
              {name.trim() ? (
                <div className="space-y-4">
                  {/* Avatar Preview */}
                  <div className="text-center">
                    <div className="w-24 h-24 mx-auto mb-3 rounded-lg overflow-hidden bg-gray-700 flex items-center justify-center">
                      {selectedIcon ? (
                        iconType === 'generated' || iconType === 'fontawesome' ? (
                          <div 
                            className="w-full h-full"
                            dangerouslySetInnerHTML={{ __html: selectedIcon }}
                          />
                        ) : (
                          <div className="w-full h-full bg-blue-500 flex items-center justify-center">
                            <span className="text-white text-xs">Model Thumbnail</span>
                          </div>
                        )
                      ) : (
                        <span className="text-gray-400 text-xs">No Icon</span>
                      )}
                    </div>
                    <p className="text-sm text-gray-300">{name.trim()}</p>
                  </div>

                  {/* Collection Details */}
                  <div className="space-y-3 text-sm">
                    <div className="flex justify-between">
                      <span className="text-gray-400">Visibility:</span>
                      <span className="text-white capitalize">{visibility.toLowerCase()}</span>
                    </div>
                    {description.trim() && (
                      <div>
                        <span className="text-gray-400 block mb-1">Description:</span>
                        <p className="text-white text-xs">{description.trim()}</p>
                      </div>
                    )}
                    {visibility === 'Unlisted' && password.trim() && (
                      <div className="flex justify-between">
                        <span className="text-gray-400">Password Protected:</span>
                        <span className="text-green-400">Yes</span>
                      </div>
                    )}
                  </div>

                  {/* Tips */}
                  <div className="p-3 bg-blue-900 border border-blue-600 rounded text-blue-200 text-xs">
                    <strong>💡 Tip:</strong> Your collection will automatically get a unique generated avatar based on its name and a random salt. You can also choose from FontAwesome icons or let it use the thumbnail of the last added model.
                  </div>
                </div>
              ) : (
                <div className="text-center py-8 text-gray-500">
                  <p>Enter a collection name to see a preview</p>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default CreateCollection; 