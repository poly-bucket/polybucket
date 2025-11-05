import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeftIcon } from '@heroicons/react/24/outline';
import NavigationBar from '../components/common/NavigationBar';
import collectionsService, { Collection } from '../services/collectionsService';
import { useAppSelector } from '../utils/hooks';

const EditCollection: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user } = useAppSelector((state) => state.auth);
  
  const [collection, setCollection] = useState<Collection | null>(null);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [visibility, setVisibility] = useState<'Public' | 'Private' | 'Unlisted'>('Private');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (id) {
      loadCollection();
    }
  }, [id]);

  const loadCollection = async () => {
    if (!id) return;
    
    try {
      setLoading(true);
      const collectionData = await collectionsService.getCollectionById(id);
      setCollection(collectionData);
      setName(collectionData.name);
      setDescription(collectionData.description || '');
      setVisibility(collectionData.visibility);
    } catch (err) {
      console.error('Error loading collection:', err);
      setError('Failed to load collection');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!collection || !name.trim()) {
      setError('Collection name is required');
      return;
    }

    try {
      setSaving(true);
      setError('');
      
      await collectionsService.updateCollection({
        id: collection.id,
        name: name.trim(),
        description: description.trim() || undefined,
        visibility,
        password: password.trim() || undefined
      });
      
      navigate(`/my-collections/${collection.id}`);
    } catch (err) {
      console.error('Error updating collection:', err);
      setError('Failed to update collection. Please try again.');
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex flex-col">
        <NavigationBar
          title="Edit Collection"
          showSearch={false}
          showUploadButton={false}
          showHomeLink={true}
        />
        <div className="flex-1 pt-20">
        <div className="flex items-center justify-center py-12">
          <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-indigo-500"></div>
        </div>
        </div>
      </div>
    );
  }

  if (error || !collection) {
    return (
      <div className="min-h-screen bg-gray-50 flex flex-col">
        <NavigationBar
          title="Edit Collection"
          showSearch={false}
          showUploadButton={false}
          showHomeLink={true}
        />
        <div className="flex-1 pt-20">
        <div className="max-w-2xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <div className="text-center py-12">
            <div className="bg-red-50 border border-red-200 rounded-md p-4 max-w-md mx-auto">
              <p className="text-red-800">{error || 'Collection not found'}</p>
              <button 
                onClick={() => navigate('/my-collections')}
                className="mt-2 text-sm text-red-600 hover:text-red-800 underline"
              >
                Back to Collections
              </button>
            </div>
          </div>
        </div>
        </div>
      </div>
    );
  }

  // Check if user is the owner
  if (collection.ownerId !== user?.id) {
    return (
      <div className="min-h-screen bg-gray-50 flex flex-col">
        <NavigationBar
          title="Edit Collection"
          showSearch={false}
          showUploadButton={false}
          showHomeLink={true}
        />
        <div className="flex-1 pt-20">
        <div className="max-w-2xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <div className="text-center py-12">
            <div className="bg-red-50 border border-red-200 rounded-md p-4 max-w-md mx-auto">
              <p className="text-red-800">You don't have permission to edit this collection.</p>
              <button 
                onClick={() => navigate('/my-collections')}
                className="mt-2 text-sm text-red-600 hover:text-red-800 underline"
              >
                Back to Collections
              </button>
            </div>
          </div>
        </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col">
      <NavigationBar
        title="Edit Collection"
        showSearch={false}
        showUploadButton={false}
        showHomeLink={true}
      />

      {/* Main Content - Padding for fixed navbar */}
      <div className="flex-1 pt-20">
      <div className="max-w-2xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
          {/* Header */}
          <div className="flex items-center mb-6">
            <button
              onClick={() => navigate(`/my-collections/${collection.id}`)}
              className="mr-4 p-2 text-gray-400 hover:text-gray-600 rounded-lg hover:bg-gray-100"
            >
              <ArrowLeftIcon className="w-5 h-5" />
            </button>
            <h1 className="text-2xl font-bold text-gray-900">Edit Collection</h1>
          </div>

          {/* Form */}
          <form onSubmit={handleSubmit} className="space-y-6">
            {error && (
              <div className="bg-red-50 border border-red-200 rounded-md p-4">
                <p className="text-red-800">{error}</p>
              </div>
            )}

            {/* Name */}
            <div>
              <label htmlFor="name" className="block text-sm font-medium text-gray-700 mb-2">
                Collection Name *
              </label>
              <input
                type="text"
                id="name"
                value={name}
                onChange={(e) => setName(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                placeholder="Enter collection name"
                maxLength={100}
                required
              />
              <p className="mt-1 text-sm text-gray-500">
                {name.length}/100 characters
              </p>
            </div>

            {/* Description */}
            <div>
              <label htmlFor="description" className="block text-sm font-medium text-gray-700 mb-2">
                Description
              </label>
              <textarea
                id="description"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                rows={4}
                className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                placeholder="Describe your collection (optional)"
                maxLength={500}
              />
              <p className="mt-1 text-sm text-gray-500">
                {description.length}/500 characters
              </p>
            </div>

            {/* Visibility */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
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
                    className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300"
                  />
                  <div className="ml-3">
                    <div className="text-sm font-medium text-gray-900">Public</div>
                    <div className="text-sm text-gray-500">Anyone can see and browse this collection</div>
                  </div>
                </label>
                
                <label className="flex items-center">
                  <input
                    type="radio"
                    name="visibility"
                    value="Unlisted"
                    checked={visibility === 'Unlisted'}
                    onChange={(e) => setVisibility(e.target.value as 'Public' | 'Private' | 'Unlisted')}
                    className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300"
                  />
                  <div className="ml-3">
                    <div className="text-sm font-medium text-gray-900">Unlisted</div>
                    <div className="text-sm text-gray-500">Only people with the link can see this collection</div>
                  </div>
                </label>
                
                <label className="flex items-center">
                  <input
                    type="radio"
                    name="visibility"
                    value="Private"
                    checked={visibility === 'Private'}
                    onChange={(e) => setVisibility(e.target.value as 'Public' | 'Private' | 'Unlisted')}
                    className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300"
                  />
                  <div className="ml-3">
                    <div className="text-sm font-medium text-gray-900">Private</div>
                    <div className="text-sm text-gray-500">Only you can see this collection</div>
                  </div>
                </label>
              </div>
            </div>

            {/* Password Protection (only for Unlisted) */}
            {visibility === 'Unlisted' && (
              <div>
                <label htmlFor="password" className="block text-sm font-medium text-gray-700 mb-2">
                  Password Protection (Optional)
                </label>
                <input
                  type="password"
                  id="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                  placeholder="Enter new password to protect this collection (leave empty to remove password)"
                  maxLength={100}
                />
                <p className="mt-1 text-sm text-gray-500">
                  If set, people will need this password to access your collection. Leave empty to remove password protection.
                </p>
              </div>
            )}

            {/* Actions */}
            <div className="flex justify-end space-x-3 pt-6 border-t border-gray-200">
              <button
                type="button"
                onClick={() => navigate(`/my-collections/${collection.id}`)}
                className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md shadow-sm hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
              >
                Cancel
              </button>
              <button
                type="submit"
                disabled={saving || !name.trim()}
                className="px-4 py-2 text-sm font-medium text-white bg-indigo-600 border border-transparent rounded-md shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {saving ? 'Saving...' : 'Save Changes'}
              </button>
            </div>
          </form>
        </div>
      </div>
      </div>
    </div>
  );
};

export default EditCollection; 