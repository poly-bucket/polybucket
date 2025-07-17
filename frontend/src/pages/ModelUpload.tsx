import React, { useState, useRef, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Canvas, useThree } from '@react-three/fiber';
import { OrbitControls, Stage } from '@react-three/drei';
import { Suspense } from 'react';
import * as THREE from 'three';
import { STLLoader } from 'three/examples/jsm/loaders/STLLoader.js';
import { OBJLoader } from 'three/examples/jsm/loaders/OBJLoader.js';
import { GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader.js';
import { FBXLoader } from 'three/examples/jsm/loaders/FBXLoader.js';

interface UploadedFile {
  id: string;
  name: string;
  size: number;
  type: string;
  file: File;
  progress: number;
  isThumbnail: boolean;
}

interface ModelData {
  title: string;
  description: string;
  privacy: 'public' | 'private';
  license: string;
  categories: string[];
  aiGenerated: boolean;
  workInProgress: boolean;
  nsfw: boolean;
  remix: boolean;
}

type ViewMode = 'solid' | 'wireframe' | 'points' | 'normals';

// Model component for React Three Fiber
const Model = ({ 
  file, 
  color, 
  metalness, 
  roughness, 
  viewMode,
  onGeometryLoaded
}: { 
  file: File | null;
  color: string;
  metalness: number;
  roughness: number;
  viewMode: ViewMode;
  onGeometryLoaded?: (geometry: THREE.BufferGeometry | null) => void;
}) => {
  const [geometry, setGeometry] = useState<THREE.BufferGeometry | null>(null);
  const [centeredGeometry, setCenteredGeometry] = useState<THREE.BufferGeometry | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    if (!file) {
      setGeometry(null);
      setCenteredGeometry(null);
      if (onGeometryLoaded) {
        onGeometryLoaded(null);
      }
      return;
    }

    const loadModel = async () => {
      setIsLoading(true);
      setGeometry(null);
      setCenteredGeometry(null);
      
      try {
        const fileExtension = file.name.toLowerCase().substring(file.name.lastIndexOf('.'));
        let loader: any;

        switch (fileExtension) {
        case '.stl':
          loader = new STLLoader();
          break;
        case '.obj':
          loader = new OBJLoader();
          break;
        case '.gltf':
        case '.glb':
          loader = new GLTFLoader();
          break;
        case '.fbx':
          loader = new FBXLoader();
          break;
        default:
          console.warn('Unsupported 3D format:', fileExtension);
          setIsLoading(false);
          return;
      }

      const arrayBuffer = await file.arrayBuffer();
      console.log('File loaded, size:', arrayBuffer.byteLength);

      let geom: THREE.BufferGeometry;

      if (fileExtension === '.stl') {
        console.log('Parsing STL file...');
        geom = loader.parse(arrayBuffer);
        console.log('STL geometry:', geom);
      } else if (fileExtension === '.obj') {
        const text = new TextDecoder().decode(arrayBuffer);
        const obj = loader.parse(text);
        // For OBJ, we need to extract geometry from the loaded object
        if (obj.children.length > 0 && obj.children[0] instanceof THREE.Mesh) {
          geom = obj.children[0].geometry;
        } else {
          setIsLoading(false);
          return;
        }
      } else if (fileExtension === '.gltf' || fileExtension === '.glb') {
        const result = await loader.parseAsync(arrayBuffer);
        if (result.scene.children.length > 0 && result.scene.children[0] instanceof THREE.Mesh) {
          geom = result.scene.children[0].geometry;
        } else {
          setIsLoading(false);
          return;
        }
      } else {
        const result = await loader.parseAsync(arrayBuffer);
        if (result.children.length > 0 && result.children[0] instanceof THREE.Mesh) {
          geom = result.children[0].geometry;
        } else {
          setIsLoading(false);
          return;
        }
      }

              setGeometry(geom);
        setCenteredGeometry(geom);
        
        // Notify parent component
        if (onGeometryLoaded) {
          onGeometryLoaded(geom);
        }
    } catch (error) {
      console.error('Error loading model:', error);
    } finally {
      setIsLoading(false);
    }
    };

    loadModel();
  }, [file]);



  if (isLoading) {
    return (
      <mesh>
        <boxGeometry args={[1, 1, 1]} />
        <meshStandardMaterial color="#666666" />
      </mesh>
    );
  }

  if (!centeredGeometry) {
    return (
      <mesh>
        <boxGeometry args={[1, 1, 1]} />
        <meshStandardMaterial color="#888888" />
      </mesh>
    );
  }

  // Ensure geometry is valid before rendering
  if (!centeredGeometry.attributes.position || centeredGeometry.attributes.position.count === 0) {
    return (
      <mesh>
        <boxGeometry args={[1, 1, 1]} />
        <meshStandardMaterial color="#888888" />
      </mesh>
    );
  }

  if (viewMode === 'points') {
    return (
      <group>
        <points geometry={centeredGeometry}>
          <pointsMaterial 
            size={0.5}
            color={color}
            sizeAttenuation={true}
          />
        </points>
      </group>
    );
  }

  return (
    <group>
      <mesh geometry={centeredGeometry}>
        <meshStandardMaterial 
          color={color}
          metalness={metalness}
          roughness={roughness}
          wireframe={viewMode === 'wireframe'}
          side={THREE.DoubleSide}
        />
      </mesh>
    </group>
  );
};



// Floating Control Panel Components
const ViewModeControls = ({ 
  renderSettings, 
  setRenderSettings, 
  showViewControls, 
  setShowViewControls
}: {
  renderSettings: {
    view: ViewMode;
  };
  setRenderSettings: (settings: (prev: any) => any) => void;
  showViewControls: boolean;
  setShowViewControls: (show: boolean) => void;
}) => {
  return (
    <div className="absolute top-4 left-4 z-10 pointer-events-none">
      {/* Toggle Button */}
      <button
        onClick={() => setShowViewControls(!showViewControls)}
        className={`${
          showViewControls 
            ? 'bg-gray-700 text-blue-400' 
            : 'bg-gray-800 hover:bg-gray-700 text-white'
        } p-2 rounded-lg shadow-lg transition-all duration-200 mb-2 pointer-events-auto`}
        title="View Mode Controls"
      >
        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
        </svg>
      </button>

      {/* View Mode Panel */}
      {showViewControls && (
        <div className="bg-gray-800 bg-opacity-95 backdrop-blur-sm rounded-lg p-4 shadow-xl border border-gray-700 min-w-48 pointer-events-auto">
          <div className="flex justify-between items-center mb-4">
            <h3 className="text-sm font-medium text-white">View Mode</h3>
            <button
              onClick={() => setShowViewControls(false)}
              className="text-gray-400 hover:text-white"
            >
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>

          <div className="space-y-3">
            <div>
              <label className="block text-xs text-gray-300 mb-2">Render Mode</label>
              <select
                value={renderSettings.view}
                onChange={(e) => setRenderSettings(prev => ({ ...prev, view: e.target.value as ViewMode }))}
                className="w-full px-2 py-1 bg-gray-700 border border-gray-600 rounded text-white text-xs focus:outline-none focus:border-blue-500"
              >
                <option value="solid">Solid</option>
                <option value="wireframe">Wireframe</option>
                <option value="points">Points</option>
                <option value="normals">Normals</option>
              </select>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

const MaterialControls = ({ 
  renderSettings, 
  setRenderSettings, 
  showMaterialControls, 
  setShowMaterialControls
}: {
  renderSettings: {
    color: string;
    metalness: number;
    roughness: number;
  };
  setRenderSettings: (settings: (prev: any) => any) => void;
  showMaterialControls: boolean;
  setShowMaterialControls: (show: boolean) => void;
}) => {
  return (
    <div className="absolute top-4 left-20 z-10 pointer-events-none">
      {/* Toggle Button */}
      <button
        onClick={() => setShowMaterialControls(!showMaterialControls)}
        className={`${
          showMaterialControls 
            ? 'bg-gray-700 text-purple-400' 
            : 'bg-gray-800 hover:bg-gray-700 text-white'
        } p-2 rounded-lg shadow-lg transition-all duration-200 mb-2 pointer-events-auto`}
        title="Material Controls"
      >
        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 21a4 4 0 01-4-4V5a2 2 0 012-2h4a2 2 0 012 2v12a4 4 0 01-4 4zM21 5a2 2 0 00-2-2h-4a2 2 0 00-2 2v12a4 4 0 004 4h4a2 2 0 002-2V5z" />
        </svg>
      </button>

      {/* Material Panel */}
      {showMaterialControls && (
        <div className="bg-gray-800 bg-opacity-95 backdrop-blur-sm rounded-lg p-4 shadow-xl border border-gray-700 min-w-48 pointer-events-auto">
          <div className="flex justify-between items-center mb-4">
            <h3 className="text-sm font-medium text-white">Material</h3>
            <button
              onClick={() => setShowMaterialControls(false)}
              className="text-gray-400 hover:text-white"
            >
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>

          <div className="space-y-4">
            {/* Color Picker */}
            <div>
              <label className="block text-xs text-gray-300 mb-2">Color</label>
              <div className="flex items-center space-x-2">
                <div 
                  className="w-6 h-6 rounded border border-gray-600"
                  style={{ backgroundColor: renderSettings.color }}
                />
                <input
                  type="color"
                  value={renderSettings.color}
                  onChange={(e) => setRenderSettings(prev => ({ ...prev, color: e.target.value }))}
                  className="w-6 h-6 border border-gray-600 rounded cursor-pointer"
                />
              </div>
            </div>

            {/* Material Properties */}
            <div className="space-y-3">
              <div>
                <label className="block text-xs text-gray-300 mb-1">Metalness</label>
                <input
                  type="range"
                  min="0"
                  max="1"
                  step="0.1"
                  value={renderSettings.metalness}
                  onChange={(e) => setRenderSettings(prev => ({ ...prev, metalness: parseFloat(e.target.value) }))}
                  className="w-full h-1 bg-gray-700 rounded-lg appearance-none cursor-pointer slider"
                />
                <span className="text-xs text-gray-400">{renderSettings.metalness}</span>
              </div>
              <div>
                <label className="block text-xs text-gray-300 mb-1">Roughness</label>
                <input
                  type="range"
                  min="0"
                  max="1"
                  step="0.1"
                  value={renderSettings.roughness}
                  onChange={(e) => setRenderSettings(prev => ({ ...prev, roughness: parseFloat(e.target.value) }))}
                  className="w-full h-1 bg-gray-700 rounded-lg appearance-none cursor-pointer slider"
                />
                <span className="text-xs text-gray-400">{renderSettings.roughness}</span>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

const LightingControls = ({ 
  renderSettings, 
  setRenderSettings, 
  showLightingControls, 
  setShowLightingControls
}: {
  renderSettings: {
    lightAngle: number;
    lightHeight: number;
  };
  setRenderSettings: (settings: (prev: any) => any) => void;
  showLightingControls: boolean;
  setShowLightingControls: (show: boolean) => void;
}) => {
  return (
    <div className="absolute top-4 left-36 z-10 pointer-events-none">
      {/* Toggle Button */}
      <button
        onClick={() => setShowLightingControls(!showLightingControls)}
        className={`${
          showLightingControls 
            ? 'bg-gray-700 text-yellow-400' 
            : 'bg-gray-800 hover:bg-gray-700 text-white'
        } p-2 rounded-lg shadow-lg transition-all duration-200 mb-2 pointer-events-auto`}
        title="Lighting Controls"
      >
        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
        </svg>
      </button>

      {/* Lighting Panel */}
      {showLightingControls && (
        <div className="bg-gray-800 bg-opacity-95 backdrop-blur-sm rounded-lg p-4 shadow-xl border border-gray-700 min-w-48 pointer-events-auto">
          <div className="flex justify-between items-center mb-4">
            <h3 className="text-sm font-medium text-white">Lighting</h3>
            <button
              onClick={() => setShowLightingControls(false)}
              className="text-gray-400 hover:text-white"
            >
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>

          <div className="space-y-3">
            <div>
              <label className="block text-xs text-gray-300 mb-1">Light Angle</label>
              <input
                type="range"
                min="0"
                max="360"
                value={renderSettings.lightAngle}
                onChange={(e) => setRenderSettings(prev => ({ ...prev, lightAngle: parseInt(e.target.value) }))}
                className="w-full h-1 bg-gray-700 rounded-lg appearance-none cursor-pointer slider"
              />
              <span className="text-xs text-gray-400">{renderSettings.lightAngle}°</span>
            </div>
            <div>
              <label className="block text-xs text-gray-300 mb-1">Light Height</label>
              <input
                type="range"
                min="0"
                max="100"
                value={renderSettings.lightHeight}
                onChange={(e) => setRenderSettings(prev => ({ ...prev, lightHeight: parseInt(e.target.value) }))}
                className="w-full h-1 bg-gray-700 rounded-lg appearance-none cursor-pointer slider"
              />
              <span className="text-xs text-gray-400">{renderSettings.lightHeight}</span>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

const AnimationControls = ({ 
  autoRotate, 
  setAutoRotate, 
  showAnimationControls, 
  setShowAnimationControls
}: {
  autoRotate: boolean;
  setAutoRotate: (rotate: boolean) => void;
  showAnimationControls: boolean;
  setShowAnimationControls: (show: boolean) => void;
}) => {
  return (
    <div className="absolute top-4 right-4 z-10 pointer-events-none">
      {/* Toggle Button */}
      <button
        onClick={() => setShowAnimationControls(!showAnimationControls)}
        className={`${
          showAnimationControls 
            ? 'bg-gray-700 text-green-400' 
            : 'bg-gray-800 hover:bg-gray-700 text-white'
        } p-2 rounded-lg shadow-lg transition-all duration-200 mb-2 pointer-events-auto`}
        title="Animation Controls"
      >
        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
        </svg>
      </button>

      {/* Animation Panel */}
      {showAnimationControls && (
        <div className="bg-gray-800 bg-opacity-95 backdrop-blur-sm rounded-lg p-4 shadow-xl border border-gray-700 min-w-48 pointer-events-auto">
          <div className="flex justify-between items-center mb-4">
            <h3 className="text-sm font-medium text-white">Animation</h3>
            <button
              onClick={() => setShowAnimationControls(false)}
              className="text-gray-400 hover:text-white"
            >
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>

          <div className="space-y-3">
            <div className="flex items-center justify-between">
              <span className="text-xs text-gray-300">Auto Rotate</span>
              <button
                onClick={() => setAutoRotate(!autoRotate)}
                className={`px-3 py-1 text-white text-xs rounded transition-colors ${
                  autoRotate ? 'bg-red-600 hover:bg-red-700' : 'bg-purple-600 hover:bg-purple-700'
                }`}
              >
                {autoRotate ? 'Stop' : 'Start'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

const ModelUpload: React.FC = () => {
  const navigate = useNavigate();
  const [uploadedFiles, setUploadedFiles] = useState<UploadedFile[]>([]);
  const [modelData, setModelData] = useState<ModelData>({
    title: '',
    description: '',
    privacy: 'public',
    license: 'MIT',
    categories: [],
    aiGenerated: false,
    workInProgress: false,
    nsfw: false,
    remix: false
  });
  const [previewFile, setPreviewFile] = useState<UploadedFile | null>(null);
  const [isUploading, setIsUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [renderSettings, setRenderSettings] = useState({
    color: '#888888',
    view: 'solid' as ViewMode,
    metalness: 0.5,
    roughness: 0.5,
    lightAngle: 0,
    lightHeight: 0
  });
  const [autoRotate, setAutoRotate] = useState(false);
  const [currentGeometry, setCurrentGeometry] = useState<THREE.BufferGeometry | null>(null);
  const [showViewControls, setShowViewControls] = useState(false);
  const [showMaterialControls, setShowMaterialControls] = useState(false);
  const [showLightingControls, setShowLightingControls] = useState(false);
  const [showAnimationControls, setShowAnimationControls] = useState(false);
  
  const fileInputRef = useRef<HTMLInputElement>(null);

  // Keyboard shortcuts for controls
  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      if ((event.ctrlKey || event.metaKey) && event.key === 'v') {
        event.preventDefault();
        setShowViewControls(prev => !prev);
      }
      if ((event.ctrlKey || event.metaKey) && event.key === 'm') {
        event.preventDefault();
        setShowMaterialControls(prev => !prev);
      }
      if ((event.ctrlKey || event.metaKey) && event.key === 'l') {
        event.preventDefault();
        setShowLightingControls(prev => !prev);
      }
      if ((event.ctrlKey || event.metaKey) && event.key === 'a') {
        event.preventDefault();
        setShowAnimationControls(prev => !prev);
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, []);



  const supported3DFormats = ['.stl', '.obj', '.fbx', '.gltf', '.glb'];
  const supportedImageFormats = ['.jpg', '.jpeg', '.png', '.gif', '.webp', '.bmp'];
  const allSupportedFormats = [...supported3DFormats, ...supportedImageFormats];

  const categories = [
    'Art', 'Technology', 'Toys', 'Tools', 'Games', 
    'Household', 'Engineering', 'Fashion', 'Medical', 'Other'
  ];

  const licenses = [
    'MIT', 'GPL', 'Creative Commons', 'Commercial', 'Custom'
  ];

  const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
    const files = event.target.files;
    if (!files) return;

    Array.from(files).forEach(file => {
      const fileExtension = file.name.toLowerCase().substring(file.name.lastIndexOf('.'));
      
      if (allSupportedFormats.includes(fileExtension)) {
        const newFile: UploadedFile = {
          id: Math.random().toString(36).substr(2, 9),
          name: file.name,
          size: file.size,
          type: file.type,
          file: file,
          progress: 0,
          isThumbnail: false
        };

        setUploadedFiles(prev => [...prev, newFile]);

        // Auto-preview 3D models
        if (supported3DFormats.includes(fileExtension)) {
          // Reset geometry state when switching files
          setCurrentGeometry(null);
          setPreviewFile(newFile);
        } else if (supportedImageFormats.includes(fileExtension)) {
          setPreviewFile(newFile);
        }
      }
    });
  };

  const handleDragOver = (event: React.DragEvent) => {
    event.preventDefault();
  };

  const handleDrop = (event: React.DragEvent) => {
    event.preventDefault();
    const files = event.dataTransfer.files;
    
    if (files.length > 0) {
      const fileArray = Array.from(files);
      fileArray.forEach(file => {
        const fileExtension = file.name.toLowerCase().substring(file.name.lastIndexOf('.'));
        
        if (allSupportedFormats.includes(fileExtension)) {
          const newFile: UploadedFile = {
            id: Math.random().toString(36).substr(2, 9),
            name: file.name,
            size: file.size,
            type: file.type,
            file: file,
            progress: 0,
            isThumbnail: false
          };

          setUploadedFiles(prev => [...prev, newFile]);

          // Auto-preview 3D models
          if (supported3DFormats.includes(fileExtension)) {
            // Reset geometry state when switching files
            setCurrentGeometry(null);
            setPreviewFile(newFile);
          } else if (supportedImageFormats.includes(fileExtension)) {
            setPreviewFile(newFile);
          }
        }
      });
    }
  };

  const setAsThumbnail = (fileId: string) => {
    setUploadedFiles(prev => 
      prev.map(file => ({
        ...file,
        isThumbnail: file.id === fileId
      }))
    );
  };

  const removeFile = (fileId: string) => {
    setUploadedFiles(prev => {
      const filtered = prev.filter(file => file.id !== fileId);
      if (previewFile?.id === fileId) {
        setPreviewFile(filtered.length > 0 ? filtered[0] : null);
      }
      return filtered;
    });
  };

  const clearAllFiles = () => {
    setUploadedFiles([]);
    setPreviewFile(null);
  };

  const formatFileSize = (bytes: number) => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  const handleInputChange = (field: keyof ModelData, value: any) => {
    setModelData(prev => ({ ...prev, [field]: value }));
  };

  const handleCategoryToggle = (category: string) => {
    setModelData(prev => ({
      ...prev,
      categories: prev.categories.includes(category)
        ? prev.categories.filter(c => c !== category)
        : [...prev.categories, category]
    }));
  };

  const handleUpload = async () => {
    if (uploadedFiles.length === 0) return;

    setIsUploading(true);
    setUploadProgress(0);

    try {
      const formData = new FormData();
      
      // Add model data
      formData.append('name', modelData.title);
      formData.append('description', modelData.description);
      formData.append('privacy', modelData.privacy);
      formData.append('license', modelData.license);
      formData.append('categories', JSON.stringify(modelData.categories));
      formData.append('aiGenerated', modelData.aiGenerated.toString());
      formData.append('workInProgress', modelData.workInProgress.toString());
      formData.append('nsfw', modelData.nsfw.toString());
      formData.append('remix', modelData.remix.toString());

      // Add files
      uploadedFiles.forEach((file, index) => {
        formData.append(`files`, file.file);
        if (file.isThumbnail) {
          formData.append('thumbnailFileId', file.id);
        }
      });

      // Get auth token
      const token = localStorage.getItem('token');
      if (!token) {
        throw new Error('No authentication token found');
      }

      const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:11666';
      
      // Upload with progress tracking
      const response = await fetch(`${baseUrl}/api/models`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`
        },
        body: formData
      });

      if (!response.ok) {
        throw new Error(`Upload failed: ${response.statusText}`);
      }

      const result = await response.json();
      
      // Navigate back to dashboard after successful upload
      setTimeout(() => {
        navigate('/dashboard');
      }, 1000);

    } catch (error) {
      console.error('Upload failed:', error);
      alert('Upload failed. Please try again.');
    } finally {
      setIsUploading(false);
      setUploadProgress(0);
    }
  };

  return (
    <div className="min-h-screen bg-gray-900 text-white">
      <style>{`
        .slider::-webkit-slider-thumb {
          appearance: none;
          height: 16px;
          width: 16px;
          border-radius: 50%;
          background: #10b981;
          cursor: pointer;
          border: none;
        }
        .slider::-moz-range-thumb {
          height: 16px;
          width: 16px;
          border-radius: 50%;
          background: #10b981;
          cursor: pointer;
          border: none;
        }
      `}</style>
      <nav className="bg-gray-800 shadow-sm">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex items-center">
              <button
                onClick={() => navigate('/dashboard')}
                className="flex items-center text-green-400 hover:text-green-300"
              >
                <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                </svg>
                Back to Upload
              </button>
            </div>
          </div>
        </div>
      </nav>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="flex gap-8">
          {/* Left Panel - Upload Form */}
          <div className="flex-1">
            <h1 className="text-3xl font-bold text-green-400 mb-6">Upload New Model</h1>
            
            {previewFile && (
              <div className="mb-6">
                <p className="text-sm text-gray-400 mb-2">
                  Previewing: {previewFile.name}
                </p>
                <div className="bg-gray-800 rounded-lg overflow-hidden h-96 relative">
                  {supported3DFormats.includes(previewFile.name.toLowerCase().substring(previewFile.name.lastIndexOf('.'))) ? (
                    <>
                                          <Canvas 
                      shadows 
                      camera={{ position: [0, 0, 200], fov: 45 }}
                      style={{ height: '100%' }}
                    >
                        <ambientLight intensity={0.5} />
                        <pointLight position={[10, 10, 10]} />
                        <Suspense fallback={null}>
                          <Stage environment="city" intensity={0.6}>
                            <Model 
                              file={previewFile.file}
                              color={renderSettings.color}
                              metalness={renderSettings.metalness}
                              roughness={renderSettings.roughness}
                              viewMode={renderSettings.view}
                              onGeometryLoaded={setCurrentGeometry}
                            />
                          </Stage>
                        </Suspense>
  
                                              <OrbitControls 
                        autoRotate={autoRotate}
                        autoRotateSpeed={1}
                        enableZoom={true}
                        enablePan={true}
                        enableRotate={true}
                        maxDistance={2000}
                        minDistance={1}
                        target={[0, 0, 0]}
                        zoomSpeed={0.8}
                        panSpeed={0.8}
                        rotateSpeed={0.8}
                      />
                      </Canvas>
                                             <ViewModeControls
                          renderSettings={renderSettings}
                          setRenderSettings={setRenderSettings}
                          showViewControls={showViewControls}
                          setShowViewControls={setShowViewControls}
                        />
                        <MaterialControls
                          renderSettings={renderSettings}
                          setRenderSettings={setRenderSettings}
                          showMaterialControls={showMaterialControls}
                          setShowMaterialControls={setShowMaterialControls}
                        />
                        <LightingControls
                          renderSettings={renderSettings}
                          setRenderSettings={setRenderSettings}
                          showLightingControls={showLightingControls}
                          setShowLightingControls={setShowLightingControls}
                        />
                        <AnimationControls
                          autoRotate={autoRotate}
                          setAutoRotate={setAutoRotate}
                          showAnimationControls={showAnimationControls}
                          setShowAnimationControls={setShowAnimationControls}
                        />
                    </>
                  ) : (
                    <div className="w-full h-full flex items-center justify-center">
                      <img 
                        src={URL.createObjectURL(previewFile.file)} 
                        alt="Preview" 
                        className="max-w-full max-h-full object-contain"
                      />
                    </div>
                  )}
                </div>
                {supportedImageFormats.includes(previewFile.name.toLowerCase().substring(previewFile.name.lastIndexOf('.'))) && (
                  <button
                    onClick={() => setAsThumbnail(previewFile.id)}
                    className="mt-4 px-4 py-2 bg-green-600 hover:bg-green-700 text-white rounded-md"
                  >
                    Set as Thumbnail
                  </button>
                )}
                

              </div>
            )}

            {/* File Upload Area */}
            <div className="mb-6">
              <div
                className="border-2 border-dashed border-gray-600 rounded-lg p-8 text-center hover:border-green-500 transition-colors"
                onDragOver={handleDragOver}
                onDrop={handleDrop}
              >
                <svg className="mx-auto h-12 w-12 text-gray-400 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
                </svg>
                <p className="text-lg text-gray-300 mb-2">Drag files to upload</p>
                <p className="text-sm text-gray-500 mb-4">or</p>
                <button
                  onClick={() => fileInputRef.current?.click()}
                  className="px-6 py-3 bg-green-600 hover:bg-green-700 text-white rounded-md font-medium"
                >
                  Choose Files
                </button>
                <input
                  ref={fileInputRef}
                  type="file"
                  multiple
                  accept={allSupportedFormats.join(',')}
                  onChange={handleFileSelect}
                  className="hidden"
                />
                <div className="mt-4 text-sm text-gray-400">
                  <p>Supported 3D formats: {supported3DFormats.join(', ')}</p>
                  <p>Supported image formats: {supportedImageFormats.join(', ')}</p>
                </div>
              </div>
            </div>

            {/* Model Information */}
            <div className="space-y-6">
              <div>
                <label className="block text-sm font-medium text-gray-300 mb-2">Title</label>
                <input
                  type="text"
                  value={modelData.title}
                  onChange={(e) => handleInputChange('title', e.target.value)}
                  className="w-full px-3 py-2 bg-gray-800 border border-gray-600 rounded-md text-white focus:outline-none focus:border-green-500"
                  placeholder="Enter model title"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-300 mb-2">Description</label>
                <textarea
                  value={modelData.description}
                  onChange={(e) => handleInputChange('description', e.target.value)}
                  rows={4}
                  className="w-full px-3 py-2 bg-gray-800 border border-gray-600 rounded-md text-white focus:outline-none focus:border-green-500 resize-none"
                  placeholder="Enter model description"
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">Privacy</label>
                  <select
                    value={modelData.privacy}
                    onChange={(e) => handleInputChange('privacy', e.target.value)}
                    className="w-full px-3 py-2 bg-gray-800 border border-gray-600 rounded-md text-white focus:outline-none focus:border-green-500"
                  >
                    <option value="public">Public - Everyone can see this model</option>
                    <option value="private">Private - Only you can see this model</option>
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">License</label>
                  <select
                    value={modelData.license}
                    onChange={(e) => handleInputChange('license', e.target.value)}
                    className="w-full px-3 py-2 bg-gray-800 border border-gray-600 rounded-md text-white focus:outline-none focus:border-green-500"
                  >
                    {licenses.map(license => (
                      <option key={license} value={license}>{license}</option>
                    ))}
                  </select>
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-300 mb-2">Categories</label>
                <div className="flex flex-wrap gap-2">
                  {categories.map(category => (
                    <button
                      key={category}
                      onClick={() => handleCategoryToggle(category)}
                      className={`px-3 py-1 rounded-full text-sm border ${
                        modelData.categories.includes(category)
                          ? 'bg-green-600 border-green-500 text-white'
                          : 'bg-gray-800 border-gray-600 text-gray-300 hover:border-green-500'
                      }`}
                    >
                      {category}
                    </button>
                  ))}
                </div>
              </div>

              <div className="space-y-3">
                <label className="block text-sm font-medium text-gray-300">Options</label>
                <div className="space-y-2">
                  {[
                    { key: 'aiGenerated', label: 'AI Generated' },
                    { key: 'workInProgress', label: 'Work in Progress' },
                    { key: 'nsfw', label: 'NSFW' },
                    { key: 'remix', label: 'Remix of Another Model' }
                  ].map(option => (
                    <label key={option.key} className="flex items-center">
                      <input
                        type="checkbox"
                        checked={modelData[option.key as keyof ModelData] as boolean}
                        onChange={(e) => handleInputChange(option.key as keyof ModelData, e.target.checked)}
                        className="mr-2 text-green-600 bg-gray-800 border-gray-600 rounded focus:ring-green-500"
                      />
                      <span className="text-sm text-gray-300">{option.label}</span>
                    </label>
                  ))}
                </div>
              </div>

              <div className="flex justify-end space-x-4 pt-6">
                <button
                  onClick={() => navigate('/dashboard')}
                  className="px-6 py-2 border border-green-500 text-green-500 rounded-md hover:bg-green-500 hover:text-white transition-colors"
                >
                  Cancel
                </button>
                <button
                  onClick={handleUpload}
                  disabled={isUploading || uploadedFiles.length === 0}
                  className="px-6 py-2 bg-green-600 hover:bg-green-700 text-white rounded-md font-medium disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {isUploading ? 'Uploading...' : 'Upload Model'}
                </button>
              </div>
            </div>
          </div>

          {/* Right Panel - Upload Queue */}
          <div className="w-80">
            <div className="bg-gray-800 rounded-lg p-4">
              <div className="flex justify-between items-center mb-4">
                <h3 className="text-lg font-medium text-green-400">Upload Queue</h3>
                {uploadedFiles.length > 0 && (
                  <button
                    onClick={clearAllFiles}
                    className="text-red-400 hover:text-red-300 text-sm"
                  >
                    Clear All
                  </button>
                )}
              </div>

              {uploadedFiles.length === 0 ? (
                <p className="text-gray-500 text-center py-8">No files selected</p>
              ) : (
                <div className="space-y-3">
                  {uploadedFiles.map(file => (
                    <div key={file.id} className="bg-gray-700 rounded p-3">
                      <div className="flex justify-between items-start mb-2">
                        <div className="flex-1 min-w-0">
                          <p className="text-sm text-white truncate">
                            {file.name}
                            {file.isThumbnail && (
                              <span className="ml-2 text-xs text-green-400">Thumbnail</span>
                            )}
                          </p>
                          <p className="text-xs text-gray-400">{formatFileSize(file.size)}</p>
                        </div>
                        <button
                          onClick={() => removeFile(file.id)}
                          className="text-gray-400 hover:text-red-400 ml-2"
                        >
                          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                          </svg>
                        </button>
                      </div>
                      <div className="w-full bg-gray-600 rounded-full h-1">
                        <div
                          className="bg-green-500 h-1 rounded-full transition-all duration-300"
                          style={{ width: `${file.progress}%` }}
                        />
                      </div>
                      <p className="text-xs text-gray-400 mt-1">{file.progress}% done</p>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ModelUpload; 