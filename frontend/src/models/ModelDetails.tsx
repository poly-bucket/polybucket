import React, { useState, useEffect, useMemo, useCallback, Suspense, lazy } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { ApiClientFactory } from '../api/clientFactory';
import { ExtendedModel } from '../services/modelsService';
import { PrivacySettings } from '../services/api.client';
import { API_CONFIG } from '../api/config';
import NavigationBar from '../components/common/NavigationBar';
import UserAvatar from '../ucp/UserAvatar';
import EditModelModal from './EditModelModal';
import PDFViewer from '../components/common/PDFViewer';
import MarkdownViewer from '../components/common/MarkdownViewer';
import { DeleteModelService } from '../services/deleteModelService';

const ModelViewer = lazy(() => import('./ModelViewer'));

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
  const [activeCarouselIndex, setActiveCarouselIndex] = useState(0);
  const [isLiked, setIsLiked] = useState(false);
  const [likeCount, setLikeCount] = useState(0);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [downloadingFiles, setDownloadingFiles] = useState<Set<string>>(new Set());
  const [isDeleting, setIsDeleting] = useState(false);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [carouselItems, setCarouselItems] = useState<Array<{
    id: string;
    type: 'image' | '3d' | 'pdf' | 'markdown';
    url?: string;
    fileData?: ArrayBuffer;
    fileName?: string;
    fileType?: string;
    file?: File;
  }>>([]);

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

  const fetchModelFile = useCallback(async (modelId: string, fileName: string): Promise<ArrayBuffer> => {
    try {
      console.log('Fetching model file:', { modelId, fileName });
      const streamUrl = `${API_CONFIG.baseUrl}/api/files/stream/model/${modelId}/${encodeURIComponent(fileName)}`;
      console.log('Stream URL:', streamUrl);
      
      const response = await fetch(streamUrl, {
        headers: {
          'Authorization': `Bearer ${user?.accessToken}`
        }
      });
      
      console.log('Response status:', response.status);
      console.log('Response headers:', Object.fromEntries(response.headers.entries()));
      
      if (response.ok) {
        const arrayBuffer = await response.arrayBuffer();
        console.log('File data received, size:', arrayBuffer.byteLength);
        return arrayBuffer;
      } else {
        console.error('Failed to fetch file:', response.status, response.statusText);
        const errorText = await response.text();
        console.error('Error response:', errorText);
        throw new Error(`Failed to fetch file: ${response.status} ${response.statusText}`);
      }
    } catch (error) {
      console.error('Failed to fetch model file:', error);
      throw error;
    }
  }, [user?.accessToken]);

  const modelFiles: ModelFile[] = useMemo(() => {
    return model?.files?.map((file, index) => ({
      id: file.id || index.toString(),
      name: file.name || `${model?.name || 'Model'}.stl`,
      size: file.size ? `${(file.size / 1024 / 1024).toFixed(1)} MB` : 'Unknown',
      type: file.mimeType || 'STL',
      downloadUrl: ''
    })) || [];
  }, [model?.files, model?.name]);

  useEffect(() => {
    const fetchModel = async () => {
      if (!id) {
        setError('Model ID not provided');
        setLoading(false);
        return;
      }

      try {
        const client = ApiClientFactory.getModelByIdClient();
        const response = await client.getModel(id);
        
        if (response && response.model && response.model.id && response.model.name) {
          const extendedModel: ExtendedModel = {
            ...response.model,
            id: response.model.id,
            name: response.model.name,
            description: response.model.description || '',
            userId: response.model.authorId || response.model.authorId || '',
            authorId: response.model.authorId || response.model.authorId || '',
            license: response.model.license?.toString(),
            privacy: response.model.privacy || PrivacySettings.Public,
            categories: response.model.categories?.map(cat => cat.toString()) || [],
            aiGenerated: response.model.aiGenerated || false,
            wip: response.model.wip || false,
            nsfw: response.model.nsfw || false,
            isRemix: response.model.isRemix || false,
            isPublic: response.model.isPublic || false,
            isFeatured: response.model.isFeatured || false,
            createdAt: response.model.createdAt?.toISOString() || new Date().toISOString(),
            updatedAt: response.model.updatedAt?.toISOString(),
            downloadCount: 0,
            rating: 0,
            isLiked: false,
            isInCollection: false,
            likes: Array.isArray(response.model.likes) ? response.model.likes : [],
            isFederated: response.model.isFederated || false,
            remoteInstanceId: response.model.remoteInstanceId,
            remoteModelId: response.model.remoteModelId,
            remoteAuthorId: response.model.remoteAuthorId,
            lastFederationSync: response.model.lastFederationSync?.toISOString()
          };
          setModel(extendedModel);
          setLikeCount(Array.isArray(response.model.likes) ? response.model.likes.length : (response.model.likes || 0));
          
          // Prepare carousel items - images first, then 3D models, then documents
          const items: Array<{
            id: string;
            type: 'image' | '3d' | 'pdf' | 'markdown';
            url?: string;
            fileData?: ArrayBuffer;
            fileName?: string;
            fileType?: string;
            file?: File;
          }> = [];

          // Add thumbnail image first if available (using ExtendedModel property)
          if ((response.model as any).thumbnailUrl) {
            items.push({
              id: 'thumbnail',
              type: 'image',
              url: (response.model as any).thumbnailUrl
            });
          }

          // Add other image files
          if (response.model?.files) {
            const imageFiles = response.model.files.filter(file => 
              file.mimeType?.startsWith('image/') || 
              file.name?.toLowerCase().match(/\.(jpg|jpeg|png|gif|webp|bmp)$/)
            );
            
            imageFiles.forEach(file => {
              if (file.name && file.id && response.model?.id) {
                items.push({
                  id: file.id,
                  type: 'image',
                  url: `${API_CONFIG.baseUrl}/api/files/stream/model/${response.model.id}/${encodeURIComponent(file.name)}`
                });
              }
            });
          }

          // Add 3D model files
          if (response.model?.files) {
            const modelFiles = response.model.files.filter(file => 
              file.mimeType?.startsWith('model/') || 
              file.name?.toLowerCase().match(/\.(stl|obj|fbx|gltf|glb|3mf|step|stp)$/)
            );
            
            for (const file of modelFiles) {
              if (file.name && file.id && response.model?.id) {
                try {
                  const fileData = await fetchModelFile(response.model.id, file.name);
                  items.push({
                    id: file.id,
                    type: '3d',
                    fileData,
                    fileName: file.name,
                    fileType: file.name.split('.').pop()?.toLowerCase()
                  });
                } catch (error) {
                  console.error(`Failed to fetch 3D model file ${file.name}:`, error);
                }
              }
            }
          }

          // Add document files (PDF, Markdown, etc.)
          if (response.model?.files) {
            const documentFiles = response.model.files.filter(file => 
              file.name?.toLowerCase().match(/\.(pdf|md|markdown|txt)$/) ||
              file.mimeType?.startsWith('text/') ||
              file.mimeType === 'application/pdf'
            );
            
            for (const file of documentFiles) {
              if (file.name && file.id && response.model?.id) {
                try {
                  const fileData = await fetchModelFile(response.model.id, file.name);
                  const fileType = file.name.split('.').pop()?.toLowerCase();
                  const isMarkdown = isMarkdownFile(file.name);
                  
                  items.push({
                    id: file.id,
                    type: isMarkdown ? 'markdown' : 'pdf',
                    fileData,
                    fileName: file.name,
                    fileType: fileType,
                    file: new File([fileData], file.name, { type: file.mimeType || 'application/octet-stream' })
                  });
                } catch (error) {
                  console.error(`Failed to fetch document file ${file.name}:`, error);
                }
              }
            }
          }

          setCarouselItems(items);
        } else {
          setError('Model not found or invalid response structure');
        }
      } catch (err) {
        if (err instanceof Error) {
          setError(`Failed to load model: ${err.message}`);
        } else {
          setError('Failed to load model - unknown error');
        }
      } finally {
        setLoading(false);
      }
    };

    fetchModel();
  }, [id, fetchModelFile]);

  const handleDownload = async () => {
    if (model) {
      try {
        console.log('Starting download for model:', model.id);
        const client = ApiClientFactory.getDownloadModelClient();
        const response = await client.downloadModel(model.id!);
        
        if (response && response.data) {
          // Validate that we received a blob
          if (!(response.data instanceof Blob)) {
            console.error('Download failed: Response data is not a blob', response.data);
            alert('Download failed: Invalid file format received');
            return;
          }
          
          // Validate blob size
          if (response.data.size === 0) {
            console.error('Download failed: Blob is empty');
            alert('Download failed: File is empty');
            return;
          }
          
          // Validate blob type (should be application/zip)
          if (response.data.type !== 'application/zip') {
            console.warn('Download warning: Expected ZIP file but received:', response.data.type);
          }
          
          console.log('Download successful:', {
            fileName: response.fileName,
            blobSize: response.data.size,
            blobType: response.data.type
          });
          
          // Test ZIP file integrity by reading the first few bytes
          try {
            const arrayBuffer = await response.data.slice(0, 4).arrayBuffer();
            const uint8Array = new Uint8Array(arrayBuffer);
            const header = Array.from(uint8Array).map(b => b.toString(16).padStart(2, '0')).join(' ');
            console.log('ZIP file header bytes:', header);
            
            // Check for ZIP file signature (PK\x03\x04)
            if (uint8Array[0] === 0x50 && uint8Array[1] === 0x4B && uint8Array[2] === 0x03 && uint8Array[3] === 0x04) {
              console.log('ZIP file signature verified successfully');
            } else {
              console.error('ZIP file signature verification failed. Expected PK\\x03\\x04, got:', header);
              alert('Download failed: Invalid ZIP file format');
              return;
            }
          } catch (error) {
            console.error('Failed to verify ZIP file integrity:', error);
            alert('Download failed: Could not verify file integrity');
            return;
          }
          
          // Test if we can open the ZIP file programmatically
          try {
            const testArrayBuffer = await response.data.slice(0, 100).arrayBuffer();
            console.log('Successfully read first 100 bytes of ZIP file');
          } catch (error) {
            console.error('Failed to read ZIP file content:', error);
            alert('Download failed: File content cannot be read');
            return;
          }
          
          // Create a download link and trigger download
          const url = window.URL.createObjectURL(response.data);
          const link = document.createElement('a');
          link.href = url;
          link.download = response.fileName || `${model.name}.zip`;
          
          // Add event listener to detect if download actually started
          link.addEventListener('click', () => {
            console.log('Download link clicked, file should start downloading');
          });
          
          document.body.appendChild(link);
          link.click();
          document.body.removeChild(link);
          
          // Keep the blob URL alive for a bit longer to ensure download completes
          setTimeout(() => {
            window.URL.revokeObjectURL(url);
            console.log('Blob URL revoked after download');
          }, 5000);
          
          // Increment download count (demo)
          setModel(prev => prev ? { ...prev, downloadCount: (prev.downloadCount || 0) + 1 } as ExtendedModel : null);
        } else {
          console.error('Download failed: No response data');
          alert('Download failed: No file data received');
        }
      } catch (error) {
        console.error('Download failed:', error);
        alert('Download failed: ' + (error instanceof Error ? error.message : 'Unknown error'));
      }
    }
  };

  const handleFileDownload = async (file: any) => {
    try {
      setDownloadingFiles(prev => new Set(prev).add(file.id));
      
      // Use the streaming endpoint for individual files
      const streamUrl = `${API_CONFIG.baseUrl}/api/files/stream/model/${model!.id}/${encodeURIComponent(file.name)}`;
      
      const response = await fetch(streamUrl, {
        headers: {
          'Authorization': `Bearer ${user?.accessToken}`
        }
      });
      
      if (response.ok) {
        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = file.name || 'download';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);
      } else {
        console.error('File download failed:', response.status, response.statusText);
        // You might want to show an error message to the user
      }
    } catch (error) {
      console.error('File download failed:', error);
      // You might want to show an error message to the user
    } finally {
      setDownloadingFiles(prev => {
        const newSet = new Set(prev);
        newSet.delete(file.id);
        return newSet;
      });
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

  const handleDeleteModel = async () => {
    if (!model || !user?.accessToken) {
      alert('Unable to delete model: Missing authentication');
      return;
    }

    setIsDeleting(true);
    try {
      const result = await DeleteModelService.deleteModel(model.id!, user.accessToken);
      
      if (result.success) {
        alert('Model deleted successfully');
        navigate('/dashboard');
      } else {
        alert(`Failed to delete model: ${result.message}`);
      }
    } catch (error) {
      console.error('Delete model error:', error);
      alert(`Failed to delete model: ${error instanceof Error ? error.message : 'Unknown error'}`);
    } finally {
      setIsDeleting(false);
      setShowDeleteConfirm(false);
    }
  };

  const formatNumber = (num: number): string => {
    if (num >= 1000000) return (num / 1000000).toFixed(1) + 'M';
    if (num >= 1000) return (num / 1000).toFixed(1) + 'K';
    return num.toString();
  };

  const getFileType = (fileName: string) => {
    const fileExtension = fileName.toLowerCase().substring(fileName.lastIndexOf('.'));
    if (['.stl', '.obj', '.fbx', '.gltf', '.glb', '.3mf', '.step', '.stp'].includes(fileExtension)) return '3d';
    if (['.jpg', '.jpeg', '.png', '.gif', '.webp', '.bmp'].includes(fileExtension)) return 'image';
    if (fileName.toLowerCase().endsWith('.pdf')) return 'pdf';
    if (['.md', '.markdown'].includes(fileExtension)) return 'markdown';
    return 'unknown';
  };

  const isMarkdownFile = (fileName: string) => {
    const fileExtension = fileName.toLowerCase().substring(fileName.lastIndexOf('.'));
    return fileExtension === '.md' || fileExtension === '.markdown';
  };

  const ModelViewerFallback: React.FC = () => {
    return (
      <div className="w-full h-full flex flex-col items-center justify-center bg-gray-900">
        <div className="w-full max-w-md px-4">
          <div className="mb-4">
            <div className="flex items-center justify-between mb-2">
              <span className="text-sm font-medium text-gray-300">Loading 3D Viewer</span>
            </div>
            <div className="w-full bg-gray-700 rounded-full h-2.5 overflow-hidden">
              <div 
                className="bg-blue-600 h-2.5 rounded-full transition-all duration-300 ease-out animate-pulse"
                style={{ width: '60%' }}
              ></div>
            </div>
          </div>
          <div className="flex items-center justify-center mb-2">
            <div className="lg-spinner"></div>
          </div>
          <p className="text-xs text-gray-400 text-center">Initializing 3D viewer component...</p>
        </div>
      </div>
    );
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
      <div className="lg-container min-h-screen flex items-center justify-center">
        <div className="lg-spinner"></div>
      </div>
    );
  }

  if (error || !model) {
    return (
      <div className="lg-container min-h-screen flex items-center justify-center">
        <div className="text-center">
          <h1 className="text-2xl font-bold text-white mb-4">Model Not Found</h1>
          <p className="text-gray-300 mb-6">{error}</p>
          <button
            onClick={() => navigate('/dashboard')}
            className="lg-button lg-button-primary"
          >
            Back to Dashboard
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="lg-container min-h-screen flex flex-col">
      {/* Navigation Bar */}
      <NavigationBar
        title={model.name}
        showSearch={false}
        showUploadButton={false}
        showHomeLink={true}
      />

      {/* Main Content - Padding for fixed navbar */}
      <div className="flex-1 pt-20">
      <div className="max-w-[95rem] mx-auto px-3 sm:px-4 md:px-6 lg:px-8 py-4 sm:py-6 lg:py-8">
        {/* Breadcrumb */}
        <nav className="flex mb-6" aria-label="Breadcrumb">
          <ol className="flex items-center space-x-2">
            <li>
              <Link to="/dashboard" className="text-gray-400 hover:text-white">
                3D Models
              </Link>
            </li>
            <li>
              <svg className="w-5 h-5 text-gray-500" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M7.293 14.707a1 1 0 010-1.414L10.586 10 7.293 6.707a1 1 0 011.414-1.414l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0z" clipRule="evenodd" />
              </svg>
            </li>
            <li>
              <span className="text-white font-medium">{model.name}</span>
            </li>
          </ol>
        </nav>

        <div className="grid lg:grid-cols-6 xl:grid-cols-7 2xl:grid-cols-8 gap-6 lg:gap-8 xl:gap-12">
          {/* Left Column - Images and Gallery */}
          <div className="lg:col-span-4 xl:col-span-5 2xl:col-span-6">
            {/* Main Carousel Viewer */}
            <div className="lg-card overflow-hidden mb-4">
              <div className="relative w-full bg-gray-800 rounded-lg" style={{ 
                aspectRatio: '16/9', 
                minHeight: '300px',
                maxHeight: '70vh'
              }}>
                {carouselItems.length > 0 ? (
                  carouselItems[activeCarouselIndex].type === 'image' ? (
                    <img
                      src={carouselItems[activeCarouselIndex].url}
                      alt={`${model.name} - View ${activeCarouselIndex + 1}`}
                      className="w-full h-full object-contain"
                    />
                  ) : carouselItems[activeCarouselIndex].type === '3d' && carouselItems[activeCarouselIndex].fileData ? (
                    <div className="w-full h-full">
                      <Suspense fallback={<ModelViewerFallback />}>
                        <ModelViewer
                          fileData={carouselItems[activeCarouselIndex].fileData}
                          fileType={carouselItems[activeCarouselIndex].fileType}
                          width="100%"
                          height="100%"
                          autoRotate={false}
                          className="w-full h-full"
                          accessToken={user?.accessToken}
                          showControls={true}
                        />
                      </Suspense>
                    </div>
                  ) : carouselItems[activeCarouselIndex].type === 'pdf' && carouselItems[activeCarouselIndex].file ? (
                    <div className="w-full h-full">
                      <PDFViewer
                        file={carouselItems[activeCarouselIndex].file}
                        width="100%"
                        height="100%"
                        className="w-full h-full"
                      />
                    </div>
                  ) : carouselItems[activeCarouselIndex].type === 'markdown' && carouselItems[activeCarouselIndex].file ? (
                    <div className="w-full h-full">
                      <MarkdownViewer
                        file={carouselItems[activeCarouselIndex].file}
                        width="100%"
                        height="100%"
                        className="w-full h-full"
                      />
                    </div>
                  ) : (
                    <div className="w-full h-full flex items-center justify-center text-gray-400">
                      Loading...
                    </div>
                  )
                ) : (
                  <div className="w-full h-full flex items-center justify-center text-gray-400">
                    No preview available
                  </div>
                )}
              </div>
            </div>

            {/* Carousel Navigation */}
            {carouselItems.length > 1 && (
              <div className="flex flex-wrap gap-2 sm:gap-3 mb-6 sm:mb-8">
                {carouselItems.map((item, index) => (
                  <button
                    key={item.id}
                    onClick={() => setActiveCarouselIndex(index)}
                    className={`w-16 h-16 sm:w-20 sm:h-20 rounded-lg overflow-hidden border-2 transition-all duration-200 hover:scale-105 ${
                      activeCarouselIndex === index ? 'border-blue-500 ring-2 ring-blue-500/50' : 'border-gray-600 hover:border-gray-500'
                    }`}
                  >
                    {item.type === 'image' ? (
                      <img
                        src={item.url}
                        alt={`View ${index + 1}`}
                        className="w-full h-full object-cover"
                      />
                    ) : item.type === '3d' ? (
                      <div className="w-full h-full flex items-center justify-center bg-gray-700 text-gray-300 text-xs">
                        3D
                      </div>
                    ) : item.type === 'pdf' ? (
                      <div className="w-full h-full flex items-center justify-center bg-red-700 text-red-200 text-xs">
                        PDF
                      </div>
                    ) : item.type === 'markdown' ? (
                      <div className="w-full h-full flex items-center justify-center bg-yellow-700 text-yellow-200 text-xs">
                        MD
                      </div>
                    ) : (
                      <div className="w-full h-full flex items-center justify-center bg-gray-700 text-gray-300 text-xs">
                        File
                      </div>
                    )}
                  </button>
                ))}
              </div>
            )}

            {/* Description */}
            <div className="lg-card p-6 mb-6">
              <h2 className="text-xl font-bold text-white mb-4">Description</h2>
              <div className="prose prose-invert max-w-none">
                <p className="text-gray-300 leading-relaxed mb-4">
                  {model.description}
                </p>
              </div>
            </div>

            {/* Print Settings */}
            {/* <div className="lg-card p-6 mb-6">
              <h2 className="text-xl font-bold text-white mb-4">Recommended Print Settings</h2>
              <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
                <div className="lg-card p-4">
                  <div className="text-sm text-gray-400">Layer Height</div>
                  <div className="font-semibold text-white">{printSettings.layerHeight}</div>
                </div>
                <div className="lg-card p-4">
                  <div className="text-sm text-gray-400">Infill</div>
                  <div className="font-semibold text-white">{printSettings.infill}</div>
                </div>
                <div className="lg-card p-4">
                  <div className="text-sm text-gray-400">Supports</div>
                  <div className="font-semibold text-white">
                    {printSettings.supports ? 'Required' : 'Not Required'}
                  </div>
                </div>
                <div className="lg-card p-4">
                  <div className="text-sm text-gray-400">Print Speed</div>
                  <div className="font-semibold text-white">{printSettings.printSpeed}</div>
                </div>
                <div className="lg-card p-4">
                  <div className="text-sm text-gray-400">Nozzle Temp</div>
                  <div className="font-semibold text-white">{printSettings.nozzleTemp}</div>
                </div>
                <div className="lg-card p-4">
                  <div className="text-sm text-gray-400">Bed Temp</div>
                  <div className="font-semibold text-white">{printSettings.bedTemp}</div>
                </div>
              </div>
            </div> */}

            {/* Files */}
            <div className="lg-card p-6 mb-6">
              <h2 className="text-xl font-bold text-white mb-4">Files</h2>
              <div className="space-y-3">
                {modelFiles.map((file) => {
                  const fileType = getFileType(file.name);
                  const getFileIcon = (type: string) => {
                    switch (type) {
                      case '3d':
                        return (
                          <div className="w-10 h-10 bg-blue-600 rounded-lg flex items-center justify-center mr-3">
                            <span className="text-xs font-semibold text-white">3D</span>
                          </div>
                        );
                      case 'image':
                        return (
                          <div className="w-10 h-10 bg-purple-600 rounded-lg flex items-center justify-center mr-3">
                            <span className="text-xs font-semibold text-white">IMG</span>
                          </div>
                        );
                      case 'pdf':
                        return (
                          <div className="w-10 h-10 bg-red-600 rounded-lg flex items-center justify-center mr-3">
                            <span className="text-xs font-semibold text-white">PDF</span>
                          </div>
                        );
                      case 'markdown':
                        return (
                          <div className="w-10 h-10 bg-yellow-600 rounded-lg flex items-center justify-center mr-3">
                            <span className="text-xs font-semibold text-white">MD</span>
                          </div>
                        );
                      default:
                        return (
                          <div className="w-10 h-10 bg-gray-600 rounded-lg flex items-center justify-center mr-3">
                            <span className="text-xs font-semibold text-white">{file.type}</span>
                          </div>
                        );
                    }
                  };

                  return (
                    <div key={file.id} className="flex items-center justify-between p-3 border border-gray-600 rounded-lg hover:bg-gray-700/50">
                      <div className="flex items-center">
                        {getFileIcon(fileType)}
                        <div>
                          <h3 className="font-medium text-white">{file.name}</h3>
                          <p className="text-sm text-gray-400">{file.size}</p>
                        </div>
                      </div>
                      <button
                        onClick={() => handleFileDownload(file)}
                        disabled={downloadingFiles.has(file.id)}
                        className="lg-button lg-button-primary text-sm"
                      >
                        {downloadingFiles.has(file.id) ? 'Downloading...' : 'Download'}
                      </button>
                    </div>
                  );
                })}
              </div>
            </div>

            {/* Bill of Materials */}
            {/* <div className="lg-card p-6">
              <h2 className="text-xl font-bold text-white mb-4">Bill of Materials</h2>
              <div className="space-y-4">
                {billOfMaterials.map((item) => (
                  <div key={item.id} className="flex items-center justify-between p-4 border border-gray-600 rounded-lg">
                    <div className="flex-1">
                      <div className="flex items-center">
                        <h3 className="font-medium text-white">{item.name}</h3>
                        {item.required && (
                          <span className="ml-2 lg-badge lg-badge-error text-xs">
                            Required
                          </span>
                        )}
                      </div>
                      <p className="text-sm text-gray-400">Quantity: {item.quantity}</p>
                    </div>
                    <div className="flex items-center space-x-4">
                      {item.price && (
                        <span className="font-semibold text-white">${item.price}</span>
                      )}
                      {item.url && (
                        <a
                          href={item.url}
                          className="lg-button lg-button-primary text-sm"
                        >
                          Buy
                        </a>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            </div> */}
          </div>

          {/* Right Column - Model Info and Actions */}
          <div className="lg:col-span-2 xl:col-span-2 2xl:col-span-2">
            <div className="sticky top-6">
              {/* Model Header */}
              <div className="lg-card p-6 mb-6">
                <h1 className="text-2xl font-bold text-white mb-2">{model.name}</h1>
                
                {/* Author Info */}
                <div className="flex items-center mb-4">
                  <UserAvatar 
                    userId={model.authorId || model.userId || 'unknown'}
                    username={model.author?.username || 'Unknown'} 
                    size="md"
                  />
                  <div className="ml-3">
                    <p className="font-medium text-white">{model.author?.username}</p>
                    <p className="text-sm text-gray-400">Creator</p>
                  </div>
                </div>

                {/* Federation Info */}
                {model.isFederated && (
                  <div className="mb-4 p-3 bg-purple-500/10 border border-purple-500/30 rounded-lg">
                    <div className="flex items-center gap-2 mb-2">
                      <svg className="w-5 h-5 text-purple-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3.055 11H5a2 2 0 012 2v1a2 2 0 002 2 2 2 0 012 2v2.945M8 3.935V5.5A2.5 2.5 0 0010.5 8h.5a2 2 0 012 2 2 2 0 104 0 2 2 0 012-2h1.064M15 20.488V18a2 2 0 012-2h3.064M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                      </svg>
                      <span className="text-sm font-semibold text-purple-300">Federated Model</span>
                    </div>
                    <div className="text-xs text-gray-400 space-y-1">
                      {model.remoteInstanceId && (
                        <p>Origin Instance: <span className="text-gray-300">{model.remoteInstanceId}</span></p>
                      )}
                      {model.lastFederationSync && (
                        <p>Last Synced: <span className="text-gray-300">{new Date(model.lastFederationSync).toLocaleString()}</span></p>
                      )}
                      <p className="text-purple-300 mt-2">
                        This model is a local copy from a federated PolyBucket instance
                      </p>
                    </div>
                  </div>
                )}

                {/* Status Badges */}
                <div className="flex flex-wrap gap-2 mb-4">
                  {model.wip && (
                    <span className="lg-badge lg-badge-warning">
                      Work in Progress
                    </span>
                  )}
                  {model.aiGenerated && (
                    <span className="lg-badge lg-badge-info">
                      AI Generated
                    </span>
                  )}
                  {model.nsfw && (
                    <span className="lg-badge lg-badge-error">
                      NSFW
                    </span>
                  )}
                  {model.isFeatured && (
                    <span className="lg-badge lg-badge-success">
                      Featured
                    </span>
                  )}
                </div>

                {/* Action Buttons */}
                <div className="space-y-3">
                  <button
                    onClick={handleDownload}
                    className="w-full lg-button lg-button-primary flex items-center justify-center"
                  >
                    <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 10v6m0 0l-4-4m4 4l4-4m5 8H3a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v12a2 2 0 01-2 2z" />
                    </svg>
                    Download ({formatNumber(model.downloadCount || 0)})
                  </button>

                  {/* Edit Button - Only shown to model owner */}
                  {isOwner && (
                    <button
                      onClick={() => setIsEditModalOpen(true)}
                      className="w-full lg-button lg-button-secondary flex items-center justify-center"
                    >
                      <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                      </svg>
                      Edit Model
                    </button>
                  )}

                  {/* Delete Button - Only shown to model owner */}
                  {isOwner && (
                    <button
                      onClick={() => setShowDeleteConfirm(true)}
                      disabled={isDeleting}
                      className="w-full lg-button bg-red-600 hover:bg-red-700 text-white flex items-center justify-center disabled:opacity-50"
                    >
                      <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                      </svg>
                      {isDeleting ? 'Deleting...' : 'Delete Model'}
                    </button>
                  )}

                  <div className="grid grid-cols-2 gap-3">
                    {/* <button
                      onClick={handleLike}
                      className={`lg-button ${
                        isLiked 
                          ? 'lg-badge lg-badge-error' 
                          : 'hover:bg-gray-700'
                      } flex items-center justify-center`}
                    >
                      <svg className={`w-4 h-4 mr-2 ${isLiked ? 'fill-current' : ''}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
                      </svg>
                      {formatNumber(likeCount)}
                    </button> */}

                    <button className="lg-button hover:bg-gray-700 flex items-center justify-center">
                      <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8.684 13.342C8.886 12.938 9 12.482 9 12c0-.482-.114-.938-.316-1.342m0 2.684a3 3 0 110-2.684m0 2.684l6.632 3.316m-6.632-6l6.632-3.316m0 0a3 3 0 105.367-2.684 3 3 0 00-5.367 2.684zm0 9.316a3 3 0 105.367 2.684 3 3 0 00-5.367-2.684z" />
                      </svg>
                      Share
                    </button>
                  </div>
                </div>
              </div>

              {/* Model Stats */}
              <div className="lg-card p-6 mb-6">
                <h2 className="text-lg font-semibold text-white mb-4">Statistics</h2>
                <div className="space-y-3">
                  <div className="flex justify-between">
                    <span className="text-gray-400">Views</span>
                    <span className="font-semibold text-white">{formatNumber((model.downloadCount || 0) * 3)}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-400">Downloads</span>
                    <span className="font-semibold text-white">{formatNumber(model.downloadCount || 0)}</span>
                  </div>
                  {/* <div className="flex justify-between">
                    <span className="text-gray-400">Likes</span>
                    <span className="font-semibold text-white">{formatNumber(likeCount)}</span>
                  </div> */}
                  {/* <div className="flex justify-between">
                    <span className="text-gray-400">Comments</span>
                    <span className="font-semibold text-white">{formatNumber(model.comments?.length || 0)}</span>
                  </div> */}
                  <div className="flex justify-between">
                    <span className="text-gray-400">Published</span>
                    <span className="font-semibold text-white">
                      {new Date(model.createdAt || '').toLocaleDateString()}
                    </span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-400">Last Updated</span>
                    <span className="font-semibold text-white">
                      {new Date(model.updatedAt || '').toLocaleDateString()}
                    </span>
                  </div>
                </div>
              </div>

              {/* License Info */}
              <div className="lg-card p-6">
                <h2 className="text-lg font-semibold text-white mb-4">License</h2>
                <div className="flex items-center">
                  <div className="w-8 h-8 bg-blue-500 rounded flex items-center justify-center text-white text-sm font-bold mr-3">
                    MIT
                  </div>
                  <div>
                    <p className="font-medium text-white">MIT License</p>
                    <p className="text-sm text-gray-400">Free for personal and commercial use</p>
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

      {/* Delete Confirmation Modal */}
      {showDeleteConfirm && (
        <div className="lg-modal-overlay">
          <div className="lg-modal max-w-md">
            <div className="p-6">
              <div className="flex items-center mb-4">
                <div className="w-12 h-12 bg-red-100 rounded-full flex items-center justify-center mr-4">
                  <svg className="w-6 h-6 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z" />
                  </svg>
                </div>
                <div>
                  <h3 className="text-lg font-semibold text-white">Delete Model</h3>
                  <p className="text-gray-400">This action cannot be undone</p>
                </div>
              </div>
              
              <p className="text-gray-300 mb-6">
                Are you sure you want to delete <strong>{model?.name}</strong>? This will permanently remove the model and all its files from the platform.
              </p>
              
              <div className="flex space-x-3">
                <button
                  onClick={() => setShowDeleteConfirm(false)}
                  disabled={isDeleting}
                  className="flex-1 lg-button lg-button-secondary"
                >
                  Cancel
                </button>
                <button
                  onClick={handleDeleteModel}
                  disabled={isDeleting}
                  className="flex-1 bg-red-600 hover:bg-red-700 text-white lg-button disabled:opacity-50"
                >
                  {isDeleting ? 'Deleting...' : 'Delete Model'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
      </div>
    </div>
  );
};

export default ModelDetails; 