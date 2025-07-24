import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeftIcon, PencilIcon, TrashIcon, PlusIcon } from '@heroicons/react/24/outline';
import NavigationBar from '../components/common/NavigationBar';
import collectionsService, { Collection } from '../services/collectionsService';
import { useAppSelector } from '../utils/hooks';
import PasswordPrompt from '../components/collections/PasswordPrompt';

const CollectionDetails: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user } = useAppSelector((state) => state.auth);
  
  const [collection, setCollection] = useState<Collection | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [showPasswordPrompt, setShowPasswordPrompt] = useState(false);
  const [passwordError, setPasswordError] = useState('');

  useEffect(() => {
    if (id) {
      loadCollection();
    }
  }, [id]);

  const loadCollection = async () => {
    if (!id) return;
    
    try {
      setLoading(true);
      setError('');
      const collectionData = await collectionsService.getCollectionById(id);
      setCollection(collectionData);
    } catch (err: any) {
      console.error('Error loading collection:', err);
      
      // Check if it's a password required error
      if (err.response?.status === 401 && err.response?.data?.requiresPassword) {
        setShowPasswordPrompt(true);
        setError('');
      } else {
        setError('Failed to load collection');
      }
    } finally {
      setLoading(false);
    }
  };

  const handlePasswordSubmit = async (password: string) => {
    if (!id) return;
    
    try {
      setPasswordError('');
      const collectionData = await collectionsService.accessCollection(id, password);
      setCollection(collectionData);
      setShowPasswordPrompt(false);
    } catch (err: any) {
      console.error('Error accessing collection:', err);
      if (err.response?.status === 401) {
        setPasswordError('Incorrect password');
      } else {
        setPasswordError('Failed to access collection');
      }
    }
  };

  const handlePasswordCancel = () => {
    setShowPasswordPrompt(false);
    navigate('/my-collections');
  };

  const handleEdit = () => {
    if (collection) {
      navigate(`/collections/${collection.id}/edit`);
    }
  };

  const handleDelete = () => {
    setShowDeleteModal(true);
  };

  const confirmDelete = async () => {
    if (!collection) return;
    
    try {
      await collectionsService.deleteCollection(collection.id);
      navigate('/my-collections');
    } catch (err) {
      console.error('Error deleting collection:', err);
      setError('Failed to delete collection');
    }
  };

  const isOwner = collection?.ownerId === user?.id;

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50">
        <NavigationBar
          title="Collection Details"
          showSearch={false}
          showUploadButton={false}
          showHomeLink={true}
        />
        <div className="flex items-center justify-center py-12">
          <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-indigo-500"></div>
        </div>
      </div>
    );
  }

  if (error || !collection) {
    return (
      <div className="min-h-screen bg-gray-50">
        <NavigationBar
          title="Collection Details"
          showSearch={false}
          showUploadButton={false}
          showHomeLink={true}
        />
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
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
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <NavigationBar
        title={collection.name}
        showSearch={false}
        showUploadButton={false}
        showHomeLink={true}
      />

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Header */}
        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 mb-6">
          <div className="flex items-start justify-between">
            <div className="flex items-center">
                             <button
                 onClick={() => navigate('/my-collections')}
                 className="mr-4 p-2 text-gray-400 hover:text-gray-600 rounded-lg hover:bg-gray-100"
               >
                <ArrowLeftIcon className="w-5 h-5" />
              </button>
              <div>
                <h1 className="text-3xl font-bold text-gray-900">{collection.name}</h1>
                {collection.description && (
                  <p className="mt-2 text-gray-600">{collection.description}</p>
                )}
                <div className="mt-4 flex items-center space-x-4 text-sm text-gray-500">
                  <span className={`px-3 py-1 rounded-full ${
                    collection.visibility === 'Public' ? 'bg-green-100 text-green-800' :
                    collection.visibility === 'Unlisted' ? 'bg-yellow-100 text-yellow-800' :
                    'bg-gray-100 text-gray-800'
                  }`}>
                    {collection.visibility}
                  </span>
                  <span>{collection.collectionModels?.length || 0} models</span>
                  {collection.owner && (
                    <span>by {collection.owner.username}</span>
                  )}
                </div>
              </div>
            </div>
            
            {isOwner && (
              <div className="flex space-x-2">
                <button
                  onClick={handleEdit}
                  className="inline-flex items-center px-3 py-2 border border-gray-300 shadow-sm text-sm leading-4 font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                >
                  <PencilIcon className="w-4 h-4 mr-2" />
                  Edit
                </button>
                <button
                  onClick={handleDelete}
                  className="inline-flex items-center px-3 py-2 border border-transparent text-sm leading-4 font-medium rounded-md text-white bg-red-600 hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500"
                >
                  <TrashIcon className="w-4 h-4 mr-2" />
                  Delete
                </button>
              </div>
            )}
          </div>
        </div>

        {/* Models Section */}
        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
          <div className="flex items-center justify-between mb-6">
            <h2 className="text-xl font-semibold text-gray-900">Models in Collection</h2>
            {isOwner && (
              <button
                onClick={() => navigate('/upload-model')}
                className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
              >
                <PlusIcon className="w-4 h-4 mr-2" />
                Add Model
              </button>
            )}
          </div>

                     {collection.collectionModels && collection.collectionModels.length > 0 ? (
             <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
               {collection.collectionModels
                 .filter(collectionModel => collectionModel.model)
                 .map((collectionModel) => (
                   <div
                     key={collectionModel.model!.id}
                     className="bg-gray-50 rounded-lg p-4 hover:bg-gray-100 cursor-pointer transition-colors"
                     onClick={() => navigate(`/models/${collectionModel.model!.id}`)}
                   >
                     <div className="aspect-w-1 aspect-h-1 mb-3">
                       {collectionModel.model!.thumbnailUrl ? (
                         <img
                           src={collectionModel.model!.thumbnailUrl}
                           alt={collectionModel.model!.name}
                           className="w-full h-32 object-cover rounded-md"
                         />
                       ) : (
                         <div className="w-full h-32 bg-gray-200 rounded-md flex items-center justify-center">
                           <span className="text-gray-400">No thumbnail</span>
                         </div>
                       )}
                     </div>
                     <h3 className="font-medium text-gray-900 truncate">
                       {collectionModel.model!.name}
                     </h3>
                     <p className="text-sm text-gray-500 truncate">
                       {collectionModel.model!.description || 'No description'}
                     </p>
                   </div>
                 ))}
             </div>
          ) : (
            <div className="text-center py-12">
              <div className="text-gray-400 mb-4">
                <svg className="mx-auto h-12 w-12" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
                </svg>
              </div>
              <h3 className="text-lg font-medium text-gray-900 mb-2">No models yet</h3>
              <p className="text-gray-500 mb-4">
                This collection doesn't have any models yet.
              </p>
              {isOwner && (
                <button
                  onClick={() => navigate('/upload-model')}
                  className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                >
                  <PlusIcon className="w-4 h-4 mr-2" />
                  Upload Model
                </button>
              )}
            </div>
          )}
        </div>
      </div>

      {/* Delete Confirmation Modal */}
      {showDeleteModal && (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
          <div className="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white">
            <div className="mt-3 text-center">
              <div className="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-red-100">
                <TrashIcon className="h-6 w-6 text-red-600" />
              </div>
              <h3 className="text-lg font-medium text-gray-900 mt-2">Delete Collection</h3>
              <div className="mt-2 px-7 py-3">
                <p className="text-sm text-gray-500">
                  Are you sure you want to delete "{collection.name}"? This action cannot be undone.
                </p>
              </div>
              <div className="flex gap-4 justify-center mt-4">
                <button
                  onClick={() => setShowDeleteModal(false)}
                  className="px-4 py-2 bg-gray-300 text-gray-700 text-base font-medium rounded-md shadow-sm hover:bg-gray-400 focus:outline-none focus:ring-2 focus:ring-gray-300"
                >
                  Cancel
                </button>
                <button
                  onClick={confirmDelete}
                  className="px-4 py-2 bg-red-600 text-white text-base font-medium rounded-md shadow-sm hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-red-500"
                >
                  Delete
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Password Prompt Modal */}
      {showPasswordPrompt && (
        <PasswordPrompt
          collectionName={id || ''}
          onSubmit={handlePasswordSubmit}
          onCancel={handlePasswordCancel}
          error={passwordError}
        />
      )}
    </div>
  );
};

export default CollectionDetails; 