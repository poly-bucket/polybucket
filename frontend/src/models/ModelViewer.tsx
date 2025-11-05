import React, { useRef, useState, useEffect, useMemo, useCallback, startTransition } from 'react';
import { Canvas, useFrame, useThree } from '@react-three/fiber';
import { OrbitControls, useGLTF, useProgress, Html } from '@react-three/drei';
import * as THREE from 'three';
import { STLLoader } from 'three/examples/jsm/loaders/STLLoader.js';
import { GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader.js';
import { ThreeMFLoader } from 'three/examples/jsm/loaders/3MFLoader.js';
import { exportTo3MF } from 'three-3mf-exporter';
import { API_CONFIG } from '../api/config';

// Types
export type ViewMode = 'solid' | 'wireframe' | 'points' | 'normals';

interface ModelViewerProps {
  modelUrl?: string;
  width?: number | string;
  height?: number | string;
  autoRotate?: boolean;
  className?: string;
  accessToken?: string;
  fileId?: string;
  modelId?: string;
  fileName?: string;
  fileData?: ArrayBuffer | File;
  fileType?: string;
  showControls?: boolean;
  onBoundingBoxCalculated?: (boundingBox: THREE.Box3) => void;
  isUploadMode?: boolean;
  modelFile?: File;
  onShowThumbnailGenerator?: () => void;
  showFPS?: boolean;
  showBuildPlate?: boolean;
}

// Floating Control Panel Components
const ViewModeControls = ({ 
  renderSettings, 
  setRenderSettings, 
  showViewControls, 
  setShowViewControls,
  isUploadMode,
  onShowThumbnailGenerator,
  onExport3MF
}: {
  renderSettings: {
    view: ViewMode;
  };
  setRenderSettings: (settings: (prev: any) => any) => void;
  showViewControls: boolean;
  setShowViewControls: (show: boolean) => void;
  isUploadMode?: boolean;
  onShowThumbnailGenerator?: () => void;
  onExport3MF?: () => void;
}) => {
  return (
    <div className="absolute top-4 left-4 z-10 pointer-events-none">
      {/* Toggle Button */}
      <button
        onClick={() => setShowViewControls(!showViewControls)}
        className={`${
          showViewControls 
            ? 'lg-card text-blue-400' 
            : 'lg-card hover:bg-gray-700 text-white'
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
        <div className="lg-card p-4 shadow-xl border border-gray-700 min-w-48 pointer-events-auto">
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
                className="lg-input text-xs"
              >
                <option value="solid">Solid</option>
                <option value="wireframe">Wireframe</option>
                <option value="points">Points</option>
                <option value="normals">Normals</option>
              </select>
            </div>

            {onExport3MF && (
              <div>
                <button
                  onClick={onExport3MF}
                  className="w-full px-3 py-2 text-white text-xs rounded transition-colors bg-green-600 hover:bg-green-700"
                  title="Export to 3MF format"
                >
                  Export to 3MF
                </button>
              </div>
            )}

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
            ? 'lg-card text-purple-400' 
            : 'lg-card hover:bg-gray-700 text-white'
        } p-2 rounded-lg shadow-lg transition-all duration-200 mb-2 pointer-events-auto`}
        title="Material Controls"
      >
        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 21a4 4 0 01-4-4V5a2 2 0 012-2h4a2 2 0 012 2v12a4 4 0 01-4 4zM21 5a2 2 0 00-2-2h-4a2 2 0 00-2 2v12a4 4 0 004 4h4a2 2 0 002-2V5z" />
        </svg>
      </button>

      {/* Material Panel */}
      {showMaterialControls && (
        <div className="lg-card p-4 shadow-xl border border-gray-700 min-w-48 pointer-events-auto">
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
            ? 'lg-card text-yellow-400' 
            : 'lg-card hover:bg-gray-700 text-white'
        } p-2 rounded-lg shadow-lg transition-all duration-200 mb-2 pointer-events-auto`}
        title="Lighting Controls"
      >
        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
        </svg>
      </button>

      {/* Lighting Panel */}
      {showLightingControls && (
        <div className="lg-card p-4 shadow-xl border border-gray-700 min-w-48 pointer-events-auto">
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

const GenerateThumbnailControl = ({ 
  isUploadMode, 
  onShowThumbnailGenerator 
}: {
  isUploadMode?: boolean;
  onShowThumbnailGenerator?: () => void;
}) => {
  if (!isUploadMode || !onShowThumbnailGenerator) return null;
  
  return (
    <div className="absolute top-4 right-20 z-10 pointer-events-none">
      <button
        onClick={onShowThumbnailGenerator}
        className="lg-card hover:bg-gray-700 text-white p-2 rounded-lg shadow-lg transition-all duration-200 mb-2 pointer-events-auto"
        title="Generate Custom Image"
      >
        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
        </svg>
      </button>
    </div>
  );
};

const AnimationControls = ({ 
  autoRotate, 
  setAutoRotate, 
  showAnimationControls, 
  setShowAnimationControls,
  onResetCamera
}: {
  autoRotate: boolean;
  setAutoRotate: (rotate: boolean) => void;
  showAnimationControls: boolean;
  setShowAnimationControls: (show: boolean) => void;
  onResetCamera?: () => void;
}) => {
  return (
    <div className="absolute top-4 right-4 z-10 pointer-events-none">
      {/* Toggle Button */}
      <button
        onClick={() => setShowAnimationControls(!showAnimationControls)}
        className={`${
          showAnimationControls 
            ? 'lg-card text-green-400' 
            : 'lg-card hover:bg-gray-700 text-white'
        } p-2 rounded-lg shadow-lg transition-all duration-200 mb-2 pointer-events-auto`}
        title="Animation Controls"
      >
        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
        </svg>
      </button>

      {/* Animation Panel */}
      {showAnimationControls && (
        <div className="lg-card p-4 shadow-xl border border-gray-700 min-w-48 pointer-events-auto">
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
            {onResetCamera && (
              <div className="flex items-center justify-between">
                <span className="text-xs text-gray-300">Camera</span>
                <button
                  onClick={onResetCamera}
                  className="px-3 py-1 text-white text-xs rounded transition-colors bg-blue-600 hover:bg-blue-700"
                >
                  Reset View
                </button>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
};

// STL Loader component
const STLModel = React.memo(({ url, accessToken, fileData, renderSettings, autoRotate, onBoundingBoxCalculated }: { 
  url?: string; 
  accessToken?: string; 
  fileData?: ArrayBuffer | Blob; 
  renderSettings: { 
    color: string; 
    view: ViewMode; 
    metalness: number; 
    roughness: number; 
  };
  autoRotate: boolean;
  onBoundingBoxCalculated?: (boundingBox: THREE.Box3) => void;
}) => {
  const meshRef = useRef<THREE.Mesh>(null);
  const [geometry, setGeometry] = useState<THREE.BufferGeometry | null>(null);

  useEffect(() => {
    const loadSTL = async () => {
      try {
        const loader = new STLLoader();
        let arrayBuffer: ArrayBuffer;
        
        if (fileData) {
          // Use direct file data if provided
          if (fileData instanceof Blob) {
            arrayBuffer = await fileData.arrayBuffer();
          } else {
            arrayBuffer = fileData;
          }
        } else if (url) {
          // Fallback to fetching from URL
          const headers: HeadersInit = {};
          if (accessToken) {
            headers['Authorization'] = `Bearer ${accessToken}`;
          }
          
          const response = await fetch(url, { headers });
          if (!response.ok) {
            throw new Error(`Failed to fetch STL: ${response.status}`);
          }
          
          arrayBuffer = await response.arrayBuffer();
        } else {
          throw new Error('No file data or URL provided');
        }
        
        const geometry = loader.parse(arrayBuffer);
        
        // Use startTransition for expensive operations to prevent UI blocking
        startTransition(() => {
          setGeometry(geometry);
          
          // Calculate bounding box and report it
          if (onBoundingBoxCalculated) {
            geometry.computeBoundingBox();
            const boundingBox = geometry.boundingBox!.clone();
            onBoundingBoxCalculated(boundingBox);
          }
        });
      } catch (error) {
        // Error loading STL
      }
    };
    
    loadSTL();
  }, [url, accessToken, fileData, onBoundingBoxCalculated]);

  // Cleanup geometry on unmount
  useEffect(() => {
    return () => {
      if (geometry) {
        geometry.dispose();
      }
    };
  }, [geometry]);

  // Memoize material to prevent recreation on every render
  // IMPORTANT: This must be called before any conditional returns to follow rules of hooks
  const material = useMemo(() => {
    const materialProps = {
      color: renderSettings.color,
      metalness: renderSettings.metalness,
      roughness: renderSettings.roughness,
      wireframe: renderSettings.view === 'wireframe',
      transparent: renderSettings.view === 'points',
      opacity: renderSettings.view === 'points' ? 0.8 : 1
    };
    
    if (renderSettings.view === 'points') {
      return new THREE.PointsMaterial({ 
        ...materialProps, 
        size: 0.1,
        sizeAttenuation: true 
      });
    } else {
      return new THREE.MeshStandardMaterial(materialProps);
    }
  }, [renderSettings.color, renderSettings.metalness, renderSettings.roughness, renderSettings.view]);

  const isPointsMode = renderSettings.view === 'points';

  useFrame((state: any) => {
    if (meshRef.current && autoRotate) {
      meshRef.current.rotation.y += 0.005;
    }
  });

  // Dispose of material on unmount
  useEffect(() => {
    return () => {
      if (material) {
        material.dispose();
      }
    };
  }, [material]);

  if (!geometry) {
    return null;
  }

  return isPointsMode ? (
    <points ref={meshRef} geometry={geometry} material={material} />
  ) : (
    <mesh ref={meshRef} geometry={geometry} material={material} />
  );
});

// GLTF/GLB Model component
const GLTFModel = React.memo(({ url, accessToken, fileData, renderSettings, autoRotate, onBoundingBoxCalculated }: { 
  url?: string; 
  accessToken?: string; 
  fileData?: ArrayBuffer | Blob; 
  renderSettings: { 
    color: string; 
    view: ViewMode; 
    metalness: number; 
    roughness: number; 
  };
  autoRotate: boolean;
  onBoundingBoxCalculated?: (boundingBox: THREE.Box3) => void;
}) => {
  // For GLTF files, we need to use a custom loader with authentication
  const [scene, setScene] = useState<THREE.Group | null>(null);
  const meshRef = useRef<THREE.Group>(null);

  useEffect(() => {
    const loadGLTF = async () => {
      try {
        const loader = new GLTFLoader();
        
        let arrayBuffer: ArrayBuffer;
        
        if (fileData) {
          // Use direct file data if provided
          if (fileData instanceof Blob) {
            arrayBuffer = await fileData.arrayBuffer();
          } else {
            arrayBuffer = fileData;
          }
        } else if (url) {
          // Fallback to fetching from URL
          const headers: HeadersInit = {};
          if (accessToken) {
            headers['Authorization'] = `Bearer ${accessToken}`;
          }
          
          const response = await fetch(url, { headers });
          if (!response.ok) {
            throw new Error(`Failed to fetch GLTF: ${response.status}`);
          }
          
          arrayBuffer = await response.arrayBuffer();
        } else {
          throw new Error('No file data or URL provided');
        }
        
        const gltf = await new Promise<{ scene: THREE.Group }>((resolve, reject) => {
          loader.parse(arrayBuffer, '', resolve, reject);
        });
        
        // Use startTransition for expensive operations to prevent UI blocking
        startTransition(() => {
          setScene(gltf.scene);
          
          // Calculate bounding box and report it
          if (onBoundingBoxCalculated) {
            const boundingBox = new THREE.Box3().setFromObject(gltf.scene);
            onBoundingBoxCalculated(boundingBox);
          }
        });
      } catch (error) {
        // Error loading GLTF
      }
    };
    
    loadGLTF();
  }, [url, accessToken, fileData, onBoundingBoxCalculated]);

  useFrame((state: any) => {
    if (meshRef.current && autoRotate) {
      meshRef.current.rotation.y += 0.005;
    }
  });

  if (!scene) {
    return null;
  }

  return <primitive ref={meshRef} object={scene} />;
});

// 3MF Model component
const ThreeMFModel = React.memo(({ url, accessToken, fileData, renderSettings, autoRotate, onBoundingBoxCalculated }: { 
  url?: string; 
  accessToken?: string; 
  fileData?: ArrayBuffer | Blob; 
  renderSettings: { 
    color: string; 
    view: ViewMode; 
    metalness: number; 
    roughness: number; 
  };
  autoRotate: boolean;
  onBoundingBoxCalculated?: (boundingBox: THREE.Box3) => void;
}) => {
  const [scene, setScene] = useState<THREE.Group | null>(null);
  const meshRef = useRef<THREE.Group>(null);

  useEffect(() => {
    const load3MF = async () => {
      let blobUrl: string | null = null;
      
      try {
        const loader = new ThreeMFLoader();
        
        let arrayBuffer: ArrayBuffer;
        
        if (fileData) {
          // Use direct file data
          arrayBuffer = fileData instanceof Blob ? await fileData.arrayBuffer() : fileData;
        } else if (url) {
          // Fetch from URL
          const headers: HeadersInit = {};
          if (accessToken) {
            headers['Authorization'] = `Bearer ${accessToken}`;
          }
          const response = await fetch(url, { headers });
          if (!response.ok) {
            throw new Error(`Failed to fetch 3MF: ${response.status}`);
          }
          arrayBuffer = await response.arrayBuffer();
        } else {
          throw new Error('No file data or URL provided');
        }
        
        // Use parse method which directly accepts ArrayBuffer
        // This is the most efficient method for the official Three.js loader
        let group: THREE.Group;
        
        try {
          group = loader.parse(arrayBuffer);
        } catch (parseError) {
          // If parse fails, try using loadAsync with a Blob URL as fallback
          const blob = new Blob([arrayBuffer], { type: 'model/3mf' });
          blobUrl = URL.createObjectURL(blob);
          group = await loader.loadAsync(blobUrl);
        }
        
        if (!group || !(group instanceof THREE.Group)) {
          throw new Error('Failed to load 3MF file: Invalid group returned');
        }
        
        // Traverse the group to ensure all meshes are properly set up
        group.traverse((child) => {
          if (child instanceof THREE.Mesh) {
            // Ensure geometry is valid
            if (!child.geometry) {
              return;
            }
            
            // Update material based on render settings
            if (child.material) {
              if (Array.isArray(child.material)) {
                child.material.forEach((mat) => {
                  if (mat instanceof THREE.MeshStandardMaterial || mat instanceof THREE.MeshPhongMaterial) {
                    mat.wireframe = renderSettings.view === 'wireframe';
                  }
                });
              } else if (child.material instanceof THREE.MeshStandardMaterial || child.material instanceof THREE.MeshPhongMaterial) {
                child.material.wireframe = renderSettings.view === 'wireframe';
              }
            }
          }
        });
        
        // Use startTransition for expensive operations to prevent UI blocking
        startTransition(() => {
          setScene(group);
          
          // Calculate bounding box and report it
          if (onBoundingBoxCalculated) {
            const boundingBox = new THREE.Box3().setFromObject(group);
            onBoundingBoxCalculated(boundingBox);
          }
        });
      } catch (error) {
        console.error('Error loading 3MF:', error);
        if (error instanceof Error) {
          console.error('Error details:', error.message, error.stack);
        }
      } finally {
        // Clean up blob URL if we created one
        if (blobUrl) {
          URL.revokeObjectURL(blobUrl);
        }
      }
    };
    
    load3MF();
  }, [url, accessToken, fileData, onBoundingBoxCalculated, renderSettings.view]);

  useFrame((state: any) => {
    if (meshRef.current && autoRotate) {
      meshRef.current.rotation.y += 0.005;
    }
  });

  if (!scene) {
    return null;
  }

  return <primitive ref={meshRef} object={scene} />;
});

// Camera Controller component
function CameraController({ 
  cameraSettings,
  resetCamera
}: { 
  cameraSettings: {
    position: [number, number, number];
    minDistance: number;
    maxDistance: number;
    target: [number, number, number];
  };
  resetCamera?: boolean;
}) {
  const { camera } = useThree();
  const [hasSetInitialPosition, setHasSetInitialPosition] = useState(false);
  
  useEffect(() => {
    // Only set camera position on initial load or when explicitly reset
    if (!hasSetInitialPosition || resetCamera) {
      camera.position.set(...cameraSettings.position);
      camera.lookAt(...cameraSettings.target);
      camera.updateMatrixWorld();
      setHasSetInitialPosition(true);
    }
  }, [camera, cameraSettings.position, cameraSettings.target, resetCamera, hasSetInitialPosition]);

  return null;
}



// OrbitControls component
const OrbitControlsComponent = React.memo((props: any) => {
  return <OrbitControls {...props} />;
});

// Build Plate component - Creates a 3D printer build plate with grid pattern
const BuildPlate = React.memo(({ boundingBox }: { boundingBox: THREE.Box3 | null }) => {
  const gridTexture = useMemo(() => {
    const canvas = document.createElement('canvas');
    canvas.width = 512;
    canvas.height = 512;
    const ctx = canvas.getContext('2d')!;
    
    // White background for build plate
    ctx.fillStyle = '#FFFFFF';
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    
    // Dark gray grid lines for visibility on white background
    ctx.strokeStyle = '#cccccc';
    ctx.lineWidth = 1;
    
    const gridSize = 32;
    
    // Draw vertical lines
    for (let x = 0; x <= canvas.width; x += gridSize) {
      ctx.beginPath();
      ctx.moveTo(x, 0);
      ctx.lineTo(x, canvas.height);
      ctx.stroke();
    }
    
    // Draw horizontal lines
    for (let y = 0; y <= canvas.height; y += gridSize) {
      ctx.beginPath();
      ctx.moveTo(0, y);
      ctx.lineTo(canvas.width, y);
      ctx.stroke();
    }
    
    const texture = new THREE.CanvasTexture(canvas);
    texture.wrapS = THREE.RepeatWrapping;
    texture.wrapT = THREE.RepeatWrapping;
    texture.repeat.set(10, 10);
    
    return texture;
  }, []);

  // Cleanup texture on unmount
  useEffect(() => {
    return () => {
      gridTexture.dispose();
    };
  }, [gridTexture]);

  if (!boundingBox) {
    return null;
  }

  const size = boundingBox.getSize(new THREE.Vector3());
  const center = boundingBox.getCenter(new THREE.Vector3());
  
  // Calculate plate size: 1.5x the model's largest dimension
  const maxDimension = Math.max(size.x, size.z);
  const plateSize = Math.max(maxDimension * 1.5, 50);
  
  // Position plate slightly below the bottom of the model
  const plateY = boundingBox.min.y - 0.1;
  
  return (
    <mesh 
      position={[center.x, plateY, center.z]} 
      rotation={[-Math.PI / 2, 0, 0]}
      receiveShadow
    >
      <planeGeometry args={[plateSize, plateSize]} />
      <meshStandardMaterial 
        map={gridTexture}
        color="#FFFFFF"
        metalness={0.1}
        roughness={0.8}
        side={THREE.DoubleSide}
      />
    </mesh>
  );
});

// FPS Counter component - Optimized to reduce render calls
const FPSCounter = React.memo(() => {
  const [fps, setFps] = useState(0);
  const frameCount = useRef(0);
  const lastTime = useRef(performance.now());
  const lastUpdateTime = useRef(performance.now());
  
  useFrame(() => {
    frameCount.current++;
    const currentTime = performance.now();
    
    // Update FPS every 500ms instead of 1000ms for more responsive updates
    // but with throttling to prevent excessive re-renders
    if (currentTime - lastUpdateTime.current >= 500) {
      const newFps = Math.round(frameCount.current * 1000 / (currentTime - lastTime.current));
      if (Math.abs(newFps - fps) > 1) { // Only update if FPS changed significantly
        setFps(newFps);
      }
      frameCount.current = 0;
      lastTime.current = currentTime;
      lastUpdateTime.current = currentTime;
    }
  });

  return (
    <Html position={[0, 0, 0]} style={{ pointerEvents: 'none' }}>
      <div 
        className="absolute top-2 left-2 bg-black bg-opacity-50 text-white text-xs px-2 py-1 rounded font-mono"
        style={{ userSelect: 'none' }}
      >
        {fps} FPS
      </div>
    </Html>
  );
});

// Loading component with progress bar
function Loader() {
  const { progress, active } = useProgress();
  const roundedProgress = Math.round(progress);
  
  if (!active) return null;
  
  return (
    <Html center>
      <div className="bg-gray-900/95 backdrop-blur-sm rounded-lg p-6 min-w-[320px] shadow-xl border border-gray-700">
        <div className="mb-4">
          <div className="flex items-center justify-between mb-2">
            <span className="text-sm font-medium text-white">Loading 3D Model</span>
            <span className="text-sm font-semibold text-blue-400">{roundedProgress}%</span>
          </div>
          <div className="w-full bg-gray-700 rounded-full h-2.5 overflow-hidden">
            <div 
              className="bg-blue-600 h-2.5 rounded-full transition-all duration-300 ease-out shadow-sm"
              style={{ width: `${roundedProgress}%` }}
            ></div>
          </div>
        </div>
        <p className="text-xs text-gray-400 text-center">
          {roundedProgress < 30 ? 'Initializing viewer...' : 
           roundedProgress < 60 ? 'Loading geometry...' : 
           roundedProgress < 90 ? 'Processing mesh...' : 
           'Finalizing...'}
        </p>
      </div>
    </Html>
  );
}

/**
 * ModelViewer Component
 * 
 * A 3D model viewer that supports STL, GLTF/GLB, and 3MF files with automatic camera positioning.
 * 
 * Features:
 * - On-demand rendering (frameloop="demand") - only renders when necessary
 * - React 18 concurrency - expensive operations use startTransition to prevent UI blocking
 * - Material memoization - prevents GPU overhead from material recreation
 * - Automatic camera positioning based on model bounding box
 * - Dynamic zoom limits (min/max distance) based on model size
 * - Multiple view modes (solid, wireframe, points, normals)
 * - Material controls (color, metalness, roughness)
 * - Lighting controls (light angle, height)
 * - Animation controls (auto-rotate, reset camera)
 * - 3MF export functionality for 3D printing workflows
 * - Keyboard shortcuts for all controls
 * 
 * Supported Formats:
 * - STL: Standard triangle format with material support
 * - GLTF/GLB: Modern 3D format with animations and materials
 * - 3MF: 3D Manufacturing Format with colors, materials, and print settings
 * 
 * Keyboard Shortcuts (when showControls=true):
 * - Ctrl/Cmd + V: Toggle view mode controls
 * - Ctrl/Cmd + M: Toggle material controls
 * - Ctrl/Cmd + L: Toggle lighting controls
 * - Ctrl/Cmd + A: Toggle animation controls
 * - Ctrl/Cmd + R: Reset camera to optimal position
 * - Ctrl/Cmd + E: Export current model to 3MF format
 */
const ModelViewer: React.FC<ModelViewerProps> = ({
  modelUrl,
  width = 300,
  height = 200,
  autoRotate = false,
  className = '',
  accessToken,
  fileId,
  modelId,
  fileName,
  fileData,
  fileType,
  showControls = false,
  onBoundingBoxCalculated,
  isUploadMode = false,
  modelFile,
  onShowThumbnailGenerator,
  showFPS = false,
  showBuildPlate = true
}) => {
  const containerStyle = useMemo(() => {
    return {
      width: typeof width === 'string' ? width : `${width}px`,
      height: typeof height === 'string' ? height : `${height}px`
    };
  }, [width, height]);


  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [streamUrl, setStreamUrl] = useState<string | null>(null);
  
  // Camera control states
  const [currentAutoRotate, setCurrentAutoRotate] = useState(autoRotate);
  const [renderSettings, setRenderSettings] = useState({
    color: '#888888',
    view: 'solid' as ViewMode,
    metalness: 0.5,
    roughness: 0.5,
    lightAngle: 0,
    lightHeight: 0
  });

  // Memoize directional light position to avoid recalculation on every render
  const directionalLightPosition = useMemo(() => [
    Math.cos(renderSettings.lightAngle * Math.PI / 180) * 10,
    renderSettings.lightHeight / 10,
    Math.sin(renderSettings.lightAngle * Math.PI / 180) * 10
  ] as [number, number, number], [renderSettings.lightAngle, renderSettings.lightHeight]);
  const [showViewControls, setShowViewControls] = useState(false);
  const [showMaterialControls, setShowMaterialControls] = useState(false);
  const [showLightingControls, setShowLightingControls] = useState(false);
  const [showAnimationControls, setShowAnimationControls] = useState(false);
  
  // Camera positioning state
  const [cameraSettings, setCameraSettings] = useState({
    position: [0, 0, 5] as [number, number, number],
    minDistance: 2,
    maxDistance: 2000,
    target: [0, 0, 0] as [number, number, number]
  });

  // Store the last calculated bounding box for reset functionality
  const [lastBoundingBox, setLastBoundingBox] = useState<THREE.Box3 | null>(null);
  const [shouldResetCamera, setShouldResetCamera] = useState(false);

  // Calculate optimal camera position based on bounding box
  // Memoize to prevent recreation on every render
  const calculateCameraPosition = useCallback((boundingBox: THREE.Box3) => {
    const size = boundingBox.getSize(new THREE.Vector3());
    const center = boundingBox.getCenter(new THREE.Vector3());
    
    // Calculate the maximum dimension to ensure the model fits
    const maxDimension = Math.max(size.x, size.y, size.z);
    
    // Ensure we have a reasonable minimum size
    const effectiveMaxDimension = Math.max(maxDimension, 1);
    
    // Calculate optimal camera distance based on model size
    // Use a factor to ensure the model fits comfortably in view
    const distanceFactor = 2.5;
    const optimalDistance = effectiveMaxDimension * distanceFactor;
    
    // Ensure the camera distance is reasonable (not too close or too far)
    const clampedDistance = Math.max(optimalDistance, 1);
    
    // Set minimum and maximum distances based on model size
    const minDistance = Math.max(effectiveMaxDimension * 0.3, 0.5); // Allow getting closer for detail
    const maxDistance = Math.max(clampedDistance * 6, 100); // Allow zooming out significantly
    
    // Position camera at optimal distance from center
    const cameraPosition: [number, number, number] = [
      center.x,
      center.y,
      center.z + clampedDistance
    ];
    
    setCameraSettings(prev => {
      // Only update if the values actually changed to prevent unnecessary re-renders
      const newTarget = [center.x, center.y, center.z] as [number, number, number];
      if (
        prev.position[0] === cameraPosition[0] &&
        prev.position[1] === cameraPosition[1] &&
        prev.position[2] === cameraPosition[2] &&
        prev.minDistance === minDistance &&
        prev.maxDistance === maxDistance &&
        prev.target[0] === newTarget[0] &&
        prev.target[1] === newTarget[1] &&
        prev.target[2] === newTarget[2]
      ) {
        return prev;
      }
      return {
        position: cameraPosition,
        minDistance,
        maxDistance,
        target: newTarget
      };
    });
  }, []);

  // Handle bounding box calculation
  // Memoize to prevent recreation on every render, which causes infinite loops in child components
  const handleBoundingBoxCalculated = useCallback((boundingBox: THREE.Box3) => {
    setLastBoundingBox(prev => {
      // Only update if bounding box actually changed
      if (prev && 
          prev.min.equals(boundingBox.min) && 
          prev.max.equals(boundingBox.max)) {
        return prev;
      }
      return boundingBox.clone();
    });
    
    // Use startTransition for expensive camera calculations
    startTransition(() => {
      calculateCameraPosition(boundingBox);
    });
  }, [calculateCameraPosition]);

  // Reset camera to optimal position
  // Memoize to prevent recreation on every render
  const handleResetCamera = useCallback(() => {
    if (lastBoundingBox) {
      // Use startTransition for smooth camera reset
      startTransition(() => {
        calculateCameraPosition(lastBoundingBox);
        setShouldResetCamera(true);
        // Reset the flag after a short delay to allow the effect to run
        setTimeout(() => setShouldResetCamera(false), 100);
      });
    }
  }, [lastBoundingBox, calculateCameraPosition]);

  useEffect(() => {
    const generateStreamUrl = () => {
      // Priority: fileData > fileId > modelId+fileName > modelUrl
      if (fileData) {
        return null; // No URL needed when using direct file data
      } else if (fileId) {
        return `${API_CONFIG.baseUrl}/api/files/stream/${fileId}`;
      } else if (modelId && fileName) {
        return `${API_CONFIG.baseUrl}/api/files/stream/model/${modelId}/${encodeURIComponent(fileName)}`;
      } else if (modelUrl) {
        return modelUrl;
      }
      return null;
    };

    const url = generateStreamUrl();
    
    if (fileData) {
      // If we have direct file data, no need to check URL accessibility
      setIsLoading(false);
      setError(null);
      setStreamUrl('direct-file-data');
    } else if (!url) {
      setError('No model URL or file data provided');
      setIsLoading(false);
      return;
    } else {
      setStreamUrl(url);
      setIsLoading(true);
      setError(null);

      // Check if the URL is accessible with authentication
      const headers: HeadersInit = {};
      if (accessToken) {
        headers['Authorization'] = `Bearer ${accessToken}`;
      }

      fetch(url, { 
        method: 'HEAD',
        headers
      })
        .then(response => {
          if (!response.ok) {
            throw new Error(`Model not found (${response.status})`);
          }
          setIsLoading(false);
        })
        .catch(err => {
          setError('Failed to load model');
          setIsLoading(false);
        });
    }
  }, [modelUrl, fileId, modelId, fileName, fileData, accessToken]);

  const getFileType = () => {
    if (fileType) {
      // Extract file extension from fileType (which might be a filename)
      const lastDotIndex = fileType.lastIndexOf('.');
      if (lastDotIndex !== -1) {
        const extension = fileType.substring(lastDotIndex + 1).toLowerCase();
        return extension;
      }
      // If no extension found, treat the whole string as the type
      return fileType.toLowerCase();
    }
    if (fileName) {
      // Extract file extension from fileName
      const lastDotIndex = fileName.lastIndexOf('.');
      if (lastDotIndex !== -1) {
        const extension = fileName.substring(lastDotIndex + 1).toLowerCase();
        return extension;
      }
    }
    if (streamUrl && streamUrl !== 'direct-file-data') {
      const url = streamUrl.toLowerCase();
      if (url.includes('.stl')) return 'stl';
      if (url.includes('.obj')) return 'obj';
      if (url.includes('.fbx')) return 'fbx';
      if (url.includes('.gltf') || url.includes('.glb')) return 'gltf';
      if (url.includes('.3mf')) return '3mf';
      if (url.includes('.step') || url.includes('.stp')) return 'step';
    }
    return 'stl'; // Default to STL
  };

  const fileTypeLower = getFileType();
  const isSTL = fileTypeLower === 'stl';
  const isGLTF = fileTypeLower === 'gltf' || fileTypeLower === 'glb';
  const is3MF = fileTypeLower === '3mf';
  const isSTEP = fileTypeLower === 'step';

  // Export to 3MF functionality
  const handleExport3MF = useCallback(async () => {
    try {
      // Get the current scene from the canvas
      const canvas = document.querySelector('canvas');
      if (!canvas) {
        return;
      }

      // Create a temporary scene with the current model
      const tempScene = new THREE.Scene();
      
      // Clone the current model based on file type
      if (isSTL && fileData) {
        // For STL, we need to recreate the geometry and material
        const loader = new STLLoader();
        let arrayBuffer: ArrayBuffer;
        
        if (fileData instanceof Blob) {
          arrayBuffer = await fileData.arrayBuffer();
        } else {
          arrayBuffer = fileData;
        }
        
        const geometry = loader.parse(arrayBuffer);
        const material = new THREE.MeshStandardMaterial({
          color: renderSettings.color,
          metalness: renderSettings.metalness,
          roughness: renderSettings.roughness
        });
        
        const mesh = new THREE.Mesh(geometry, material);
        tempScene.add(mesh);
      } else if (isGLTF && fileData) {
        // For GLTF, we need to load and add the model
        const loader = new GLTFLoader();
        let arrayBuffer: ArrayBuffer;
        
        if (fileData instanceof Blob) {
          arrayBuffer = await fileData.arrayBuffer();
        } else {
          arrayBuffer = fileData;
        }
        
        const gltf = await new Promise<{ scene: THREE.Group }>((resolve, reject) => {
          loader.parse(arrayBuffer, '', resolve, reject);
        });
        
        tempScene.add(gltf.scene);
      } else if (is3MF && fileData) {
        // For 3MF, we need to load and add the model
        const loader = new ThreeMFLoader();
        let arrayBuffer: ArrayBuffer;
        
        if (fileData instanceof Blob) {
          arrayBuffer = await fileData.arrayBuffer();
        } else {
          arrayBuffer = fileData;
        }
        
        const group = loader.parse(arrayBuffer);
        tempScene.add(group);
      } else {
        return;
      }

      // Export to 3MF
      const blob = await exportTo3MF(tempScene, {
        printer_name: 'Generic 3D Printer',
        filament: 'Generic PLA',
        printableWidth: 256,
        printableDepth: 256,
        printableHeight: 256,
        printerSettingsId: 'Generic 0.4 nozzle',
        printSettingsId: '0.20mm Standard'
      });

      // Download the file
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `${fileName || 'model'}.3mf`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(url);


    } catch (error) {
      // Error exporting to 3MF
    }
  }, [isSTL, isGLTF, is3MF, fileData, fileName, renderSettings]);

  // Keyboard shortcuts for controls
  useEffect(() => {
    if (!showControls) return;

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
      if ((event.ctrlKey || event.metaKey) && event.key === 'r') {
        event.preventDefault();
        handleResetCamera();
      }
      if ((event.ctrlKey || event.metaKey) && event.key === 'e') {
        event.preventDefault();
        handleExport3MF();
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [showControls, handleResetCamera, handleExport3MF]);

  if (!isSTL && !isGLTF && !is3MF && !isSTEP) {
    return (
      <div 
        className={`bg-gray-800 rounded-lg flex items-center justify-center ${className}`}
        style={{ width, height }}
      >
        <div className="text-gray-400 text-sm">
          Unsupported format: {fileTypeLower}
        </div>
      </div>
    );
  }

  return (
    <div 
      className={`rounded-lg overflow-hidden relative ${className}`}
      style={containerStyle}
    >
      <Canvas
        camera={{ 
          position: cameraSettings.position, 
          fov: 50 
        }}
        style={containerStyle}
        frameloop="demand"
        gl={{ 
          antialias: false,
          alpha: true,
          powerPreference: "high-performance",
          stencil: false,
          depth: true,
          premultipliedAlpha: false,
          preserveDrawingBuffer: false
        }}
        dpr={Math.min(window.devicePixelRatio, 2)}
      >

        <CameraController 
          cameraSettings={cameraSettings}
          resetCamera={shouldResetCamera}
        />
        <ambientLight intensity={0.6} />
        <directionalLight 
          position={directionalLightPosition}
          intensity={1.2} 
        />
        
        {isLoading && <Loader />}
        {showFPS && <FPSCounter />}
        
        {showBuildPlate && <BuildPlate boundingBox={lastBoundingBox} />}
        
        {isSTL && (
          <STLModel 
            url={streamUrl || undefined} 
            accessToken={accessToken} 
            fileData={fileData}
            renderSettings={renderSettings}
            autoRotate={currentAutoRotate}
            onBoundingBoxCalculated={handleBoundingBoxCalculated}
          />
        )}
        {isGLTF && (
          <GLTFModel 
            url={streamUrl || undefined} 
            accessToken={accessToken} 
            fileData={fileData}
            renderSettings={renderSettings}
            autoRotate={currentAutoRotate}
            onBoundingBoxCalculated={handleBoundingBoxCalculated}
          />
        )}
        {is3MF && (
          <ThreeMFModel 
            url={streamUrl || undefined} 
            accessToken={accessToken} 
            fileData={fileData}
            renderSettings={renderSettings}
            autoRotate={currentAutoRotate}
            onBoundingBoxCalculated={handleBoundingBoxCalculated}
          />
        )}
        
        <OrbitControlsComponent 
          enablePan={true}
          enableZoom={true}
          enableRotate={true}
          autoRotate={false}
          maxDistance={cameraSettings.maxDistance}
          minDistance={cameraSettings.minDistance}
          target={cameraSettings.target}
          zoomSpeed={0.8}
          panSpeed={0.8}
          rotateSpeed={0.8}
        />
      </Canvas>

      {/* Camera Controls */}
      {showControls && (
        <>
          <ViewModeControls
            renderSettings={renderSettings}
            setRenderSettings={setRenderSettings}
            showViewControls={showViewControls}
            setShowViewControls={setShowViewControls}
            isUploadMode={false}
            onShowThumbnailGenerator={undefined}
            onExport3MF={handleExport3MF}
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
          <GenerateThumbnailControl
            isUploadMode={isUploadMode}
            onShowThumbnailGenerator={onShowThumbnailGenerator}
          />
          <AnimationControls
            autoRotate={currentAutoRotate}
            setAutoRotate={setCurrentAutoRotate}
            showAnimationControls={showAnimationControls}
            setShowAnimationControls={setShowAnimationControls}
            onResetCamera={handleResetCamera}
          />
        </>
      )}
    </div>
  );
};

export default ModelViewer; 