import React, { useEffect, useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import UserAvatar from '../components/UserAvatar';
import modelsService, { ExtendedModel } from '../services/modelsService';
import { useAuth } from '../context/AuthContext';
import EditModelModal from '../components/models/EditModelModal';

interface BillOfMaterial {
  id: string;
  name: string;
  quantity: number;
  price?: number;
  url?: string;
  required: boolean;
}

interface PrintSettings {
  layerHeight: string;
  infill: string;
  supports: boolean;
  printSpeed: string;
  nozzleTemp: string;
  bedTemp: string;
}

interface ModelFile {
  id: string;
  name: string;
  size: string;
  type: string;
  downloadUrl: string;
}

const ModelDetails: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user, isAuthenticated } = useAuth();
  const [model, setModel] = useState<ExtendedModel | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string>('');
  const [activeImageIndex, setActiveImageIndex] = useState(0);
  const [isLiked, setIsLiked] = useState(false);
  const [likeCount, setLikeCount] = useState(0);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);

  // Demo data for bill of materials and print settings
  const billOfMaterials: BillOfMaterial[] = [
    { id: '1', name: 'PLA Filament (1kg)', quantity: 1, price: 25.99, url: '#', required: true },
    { id: '2', name: 'Support Material', quantity: 1, price: 15.99, url: '#', required: false },
    { id: '3', name: 'M3 Screws (10pc)', quantity: 1, price: 5.99, url: '#', required: true },
  ];

  const printSettings: PrintSettings = {
    layerHeight: '0.2mm',
    infill: '15%',
    supports: false,
    printSpeed: '50mm/s',
    nozzleTemp: '210°C',
    bedTemp: '60°C'
  };

  const modelFiles: ModelFile[] = [
    { id: '1', name: `${model?.name || 'Model'}.stl`, size: '2.4 MB', type: 'STL', downloadUrl: '#' },
    { id: '2', name: `${model?.name || 'Model'}.obj`, size: '3.1 MB', type: 'OBJ', downloadUrl: '#' },
    { id: '3', name: `${model?.name || 'Model'}_support.stl`, size: '1.2 MB', type: 'STL', downloadUrl: '#' },
    { id: '4', name: 'Instructions.pdf', size: '487 KB', type: 'PDF', downloadUrl: '#' },
  ];

  // Demo images for the gallery
  const modelImages = [
    model?.thumbnailUrl || '',
    'https://images.unsplash.com/photo-1581833971358-2c8b550f87b3?w=800&h=600&fit=crop',
    'https://images.unsplash.com/photo-1578662996442-48f60103fc96?w=800&h=600&fit=crop',
    'https://images.unsplash.com/photo-1586023492125-27b2c045efd7?w=800&h=600&fit=crop',
  ].filter(Boolean);

  useEffect(() => {
    const fetchModel = async () => {
      if (!id) {
        setError('Model ID not provided');
        setLoading(false);
        return;
      }

      try {
        const modelData = await modelsService.getModelById(id);
        if (modelData) {
          setModel(modelData);
          setLikeCount(modelData.likes?.length || 0);
        } else {
          setError('Model not found');
        }
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load model');
      } finally {
        setLoading(false);
      }
    };

    fetchModel();
  }, [id]);

  const handleDownload = () => {
    if (model) {
      // Increment download count (demo)
      setModel(prev => prev ? { ...prev, downloads: (prev.downloads || 0) + 1 } as ExtendedModel : null);
      // In real app, trigger actual download
      console.log('Downloading model:', model.name);
    }
  };

  const handleLike = () => {
    setIsLiked(!isLiked);
    setLikeCount(prev => isLiked ? prev - 1 : prev + 1);
  };

  const handleModelUpdate = (updatedModel: ExtendedModel) => {
    setModel(updatedModel);
  };

  const handleVersionCreate = (model: ExtendedModel, versionData: any) => {
    // Update the model with new version info
    console.log('Version created for model:', model.name, 'Version data:', versionData);
    // In a real app, you'd refetch the model or update the versions array
  };

  const formatNumber = (num: number): string => {
    if (num >= 1000000) return (num / 1000000).toFixed(1) + 'M';
    if (num >= 1000) return (num / 1000).toFixed(1) + 'K';
    return num.toString();
  };

  // Check if current user is the model owner
  // For demo purposes, allow editing of all demo models (IDs 1-12) regardless of authentication
  const isDemoModel = model && ['1', '2', '3', '4', '5', '6', '7', '8', '9', '10', '11', '12'].includes(model.id || '');
  const isOwner = model && (
    isDemoModel || // Allow editing of all demo models regardless of auth status
    (isAuthenticated && user && (user.id === model.authorId || user.id === model.author?.id))
  );

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="animate-spin rounded-full h-32 w-32 border-t-2 border-b-2 border-indigo-500"></div>
      </div>
    );
  }

  if (error || !model) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <h1 className="text-2xl font-bold text-gray-900 mb-4">Model Not Found</h1>
          <p className="text-gray-600 mb-6">{error}</p>
          <button
            onClick={() => navigate('/dashboard')}
            className="px-6 py-3 bg-indigo-600 text-white rounded-md hover:bg-indigo-700"
          >
            Back to Dashboard
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Navigation */}
      <nav className="bg-white shadow-sm border-b">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center h-16">
            <button
              onClick={() => navigate('/dashboard')}
              className="flex items-center text-gray-600 hover:text-gray-900"
            >
              <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
              </svg>
              Back to Dashboard
            </button>
          </div>
        </div>
      </nav>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Breadcrumb */}
        <nav className="flex mb-6" aria-label="Breadcrumb">
          <ol className="flex items-center space-x-2">
            <li>
              <Link to="/dashboard" className="text-gray-500 hover:text-gray-700">
                3D Models
              </Link>
            </li>
            <li>
              <svg className="w-5 h-5 text-gray-400" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M7.293 14.707a1 1 0 010-1.414L10.586 10 7.293 6.707a1 1 0 011.414-1.414l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0z" clipRule="evenodd" />
              </svg>
            </li>
            <li>
              <span className="text-gray-900 font-medium">{model.name}</span>
            </li>
          </ol>
        </nav>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Left Column - Images and Gallery */}
          <div className="lg:col-span-2">
            {/* Main Image */}
            <div className="bg-white rounded-lg shadow-sm overflow-hidden mb-4">
              <div className="aspect-video bg-gray-100">
                <img
                  src={modelImages[activeImageIndex]}
                  alt={model.name}
                  className="w-full h-full object-cover"
                />
              </div>
            </div>

            {/* Image Thumbnails */}
            <div className="flex space-x-2 mb-8">
              {modelImages.map((image, index) => (
                <button
                  key={index}
                  onClick={() => setActiveImageIndex(index)}
                  className={`w-20 h-20 rounded-lg overflow-hidden border-2 ${
                    activeImageIndex === index ? 'border-indigo-500' : 'border-gray-200'
                  }`}
                >
                  <img
                    src={image}
                    alt={`View ${index + 1}`}
                    className="w-full h-full object-cover"
                  />
                </button>
              ))}
            </div>

            {/* Description */}
            <div className="bg-white rounded-lg shadow-sm p-6 mb-6">
              <h2 className="text-xl font-bold text-gray-900 mb-4">Description</h2>
              <div className="prose prose-gray max-w-none">
                <p className="text-gray-700 leading-relaxed mb-4">
                  {model.description}
                </p>
                <p className="text-gray-700 leading-relaxed mb-4">
                  This model has been carefully designed and tested for optimal printing results. 
                  No supports are required when printed in the recommended orientation.
                </p>
                <p className="text-gray-700 leading-relaxed">
                  Perfect for both beginners and experienced makers. The model includes detailed 
                  printing instructions and multiple file formats for maximum compatibility.
                </p>
              </div>
            </div>

            {/* Print Settings */}
            <div className="bg-white rounded-lg shadow-sm p-6 mb-6">
              <h2 className="text-xl font-bold text-gray-900 mb-4">Recommended Print Settings</h2>
              <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
                <div className="bg-gray-50 p-4 rounded-lg">
                  <div className="text-sm text-gray-600">Layer Height</div>
                  <div className="font-semibold text-gray-900">{printSettings.layerHeight}</div>
                </div>
                <div className="bg-gray-50 p-4 rounded-lg">
                  <div className="text-sm text-gray-600">Infill</div>
                  <div className="font-semibold text-gray-900">{printSettings.infill}</div>
                </div>
                <div className="bg-gray-50 p-4 rounded-lg">
                  <div className="text-sm text-gray-600">Supports</div>
                  <div className="font-semibold text-gray-900">
                    {printSettings.supports ? 'Required' : 'Not Required'}
                  </div>
                </div>
                <div className="bg-gray-50 p-4 rounded-lg">
                  <div className="text-sm text-gray-600">Print Speed</div>
                  <div className="font-semibold text-gray-900">{printSettings.printSpeed}</div>
                </div>
                <div className="bg-gray-50 p-4 rounded-lg">
                  <div className="text-sm text-gray-600">Nozzle Temp</div>
                  <div className="font-semibold text-gray-900">{printSettings.nozzleTemp}</div>
                </div>
                <div className="bg-gray-50 p-4 rounded-lg">
                  <div className="text-sm text-gray-600">Bed Temp</div>
                  <div className="font-semibold text-gray-900">{printSettings.bedTemp}</div>
                </div>
              </div>
            </div>

            {/* Files */}
            <div className="bg-white rounded-lg shadow-sm p-6 mb-6">
              <h2 className="text-xl font-bold text-gray-900 mb-4">Files</h2>
              <div className="space-y-3">
                {modelFiles.map((file) => (
                  <div key={file.id} className="flex items-center justify-between p-3 border border-gray-200 rounded-lg hover:bg-gray-50">
                    <div className="flex items-center">
                      <div className="w-10 h-10 bg-indigo-100 rounded-lg flex items-center justify-center mr-3">
                        <span className="text-xs font-semibold text-indigo-600">{file.type}</span>
                      </div>
                      <div>
                        <h3 className="font-medium text-gray-900">{file.name}</h3>
                        <p className="text-sm text-gray-600">{file.size}</p>
                      </div>
                    </div>
                    <a
                      href={file.downloadUrl}
                      className="px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors text-sm font-medium"
                    >
                      Download
                    </a>
                  </div>
                ))}
              </div>
            </div>

            {/* Bill of Materials */}
            <div className="bg-white rounded-lg shadow-sm p-6">
              <h2 className="text-xl font-bold text-gray-900 mb-4">Bill of Materials</h2>
              <div className="space-y-4">
                {billOfMaterials.map((item) => (
                  <div key={item.id} className="flex items-center justify-between p-4 border border-gray-200 rounded-lg">
                    <div className="flex-1">
                      <div className="flex items-center">
                        <h3 className="font-medium text-gray-900">{item.name}</h3>
                        {item.required && (
                          <span className="ml-2 px-2 py-1 text-xs bg-red-100 text-red-700 rounded-full">
                            Required
                          </span>
                        )}
                      </div>
                      <p className="text-sm text-gray-600">Quantity: {item.quantity}</p>
                    </div>
                    <div className="flex items-center space-x-4">
                      {item.price && (
                        <span className="font-semibold text-gray-900">${item.price}</span>
                      )}
                      {item.url && (
                        <a
                          href={item.url}
                          className="px-3 py-1 text-sm bg-indigo-600 text-white rounded hover:bg-indigo-700"
                        >
                          Buy
                        </a>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </div>

          {/* Right Column - Model Info and Actions */}
          <div className="lg:col-span-1">
            <div className="sticky top-6">
              {/* Model Header */}
              <div className="bg-white rounded-lg shadow-sm p-6 mb-6">
                <h1 className="text-2xl font-bold text-gray-900 mb-2">{model.name}</h1>
                
                {/* Author Info */}
                <div className="flex items-center mb-4">
                  <UserAvatar 
                    username={model.author?.username || 'Unknown'} 
                    size="md"
                  />
                  <div className="ml-3">
                    <p className="font-medium text-gray-900">{model.author?.username}</p>
                    <p className="text-sm text-gray-600">Creator</p>
                  </div>
                </div>

                {/* Status Badges */}
                <div className="flex flex-wrap gap-2 mb-4">
                  {model.wip && (
                    <span className="px-2 py-1 text-xs bg-yellow-100 text-yellow-800 rounded-full">
                      Work in Progress
                    </span>
                  )}
                  {model.aiGenerated && (
                    <span className="px-2 py-1 text-xs bg-purple-100 text-purple-800 rounded-full">
                      AI Generated
                    </span>
                  )}
                  {model.nsfw && (
                    <span className="px-2 py-1 text-xs bg-red-100 text-red-800 rounded-full">
                      NSFW
                    </span>
                  )}
                  {model.isFeatured && (
                    <span className="px-2 py-1 text-xs bg-green-100 text-green-800 rounded-full">
                      Featured
                    </span>
                  )}
                </div>

                {/* Action Buttons */}
                <div className="space-y-3">
                  <button
                    onClick={handleDownload}
                    className="w-full bg-indigo-600 text-white py-3 px-4 rounded-lg font-semibold hover:bg-indigo-700 transition-colors flex items-center justify-center"
                  >
                    <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 10v6m0 0l-4-4m4 4l4-4m5 8H3a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v12a2 2 0 01-2 2z" />
                    </svg>
                    Download ({formatNumber(model.downloads || 0)})
                  </button>

                  {/* Edit Button - Only shown to model owner */}
                  {isOwner && (
                    <button
                      onClick={() => setIsEditModalOpen(true)}
                      className="w-full bg-green-600 text-white py-3 px-4 rounded-lg font-semibold hover:bg-green-700 transition-colors flex items-center justify-center"
                    >
                      <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                      </svg>
                      Edit Model
                    </button>
                  )}

                  <div className="grid grid-cols-2 gap-3">
                    <button
                      onClick={handleLike}
                      className={`py-2 px-4 rounded-lg font-medium flex items-center justify-center transition-colors ${
                        isLiked 
                          ? 'bg-red-50 text-red-600 border border-red-200' 
                          : 'bg-gray-50 text-gray-700 border border-gray-200 hover:bg-gray-100'
                      }`}
                    >
                      <svg className={`w-4 h-4 mr-2 ${isLiked ? 'fill-current' : ''}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
                      </svg>
                      {formatNumber(likeCount)}
                    </button>

                    <button className="py-2 px-4 rounded-lg font-medium bg-gray-50 text-gray-700 border border-gray-200 hover:bg-gray-100 flex items-center justify-center">
                      <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8.684 13.342C8.886 12.938 9 12.482 9 12c0-.482-.114-.938-.316-1.342m0 2.684a3 3 0 110-2.684m0 2.684l6.632 3.316m-6.632-6l6.632-3.316m0 0a3 3 0 105.367-2.684 3 3 0 00-5.367 2.684zm0 9.316a3 3 0 105.367 2.684 3 3 0 00-5.367-2.684z" />
                      </svg>
                      Share
                    </button>
                  </div>
                </div>
              </div>

              {/* Model Stats */}
              <div className="bg-white rounded-lg shadow-sm p-6 mb-6">
                <h2 className="text-lg font-semibold text-gray-900 mb-4">Statistics</h2>
                <div className="space-y-3">
                  <div className="flex justify-between">
                    <span className="text-gray-600">Views</span>
                    <span className="font-semibold">{formatNumber((model.downloads || 0) * 3)}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-600">Downloads</span>
                    <span className="font-semibold">{formatNumber(model.downloads || 0)}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-600">Likes</span>
                    <span className="font-semibold">{formatNumber(likeCount)}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-600">Comments</span>
                    <span className="font-semibold">{formatNumber(model.comments?.length || 0)}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-600">Published</span>
                    <span className="font-semibold">
                      {new Date(model.createdAt || '').toLocaleDateString()}
                    </span>
                  </div>
                </div>
              </div>

              {/* License Info */}
              <div className="bg-white rounded-lg shadow-sm p-6">
                <h2 className="text-lg font-semibold text-gray-900 mb-4">License</h2>
                <div className="flex items-center">
                  <div className="w-8 h-8 bg-blue-500 rounded flex items-center justify-center text-white text-sm font-bold mr-3">
                    MIT
                  </div>
                  <div>
                    <p className="font-medium text-gray-900">MIT License</p>
                    <p className="text-sm text-gray-600">Free for personal and commercial use</p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Edit Model Modal */}
      {model && (
        <EditModelModal
          model={model}
          isOpen={isEditModalOpen}
          onClose={() => setIsEditModalOpen(false)}
          onModelUpdate={handleModelUpdate}
          onVersionCreate={handleVersionCreate}
        />
      )}
    </div>
  );
};

export default ModelDetails; 