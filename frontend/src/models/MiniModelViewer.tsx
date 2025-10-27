import React, { useRef, useState, useEffect } from 'react';
import { Canvas, useFrame, useThree } from '@react-three/fiber';
import { OrbitControls, useProgress, Html } from '@react-three/drei';
import * as THREE from 'three';
import { STLLoader } from 'three/examples/jsm/loaders/STLLoader.js';
import { API_CONFIG } from '../api/config';

interface MiniModelViewerProps {
  modelUrl?: string;
  width?: number;
  height?: number;
  autoRotate?: boolean;
  className?: string;
  accessToken?: string;
  fileId?: string;
  modelId?: string;
  fileName?: string;
  fileData?: ArrayBuffer;
  fileType?: string;
  onBoundingBoxCalculated?: (boundingBox: THREE.Box3) => void;
}

// STL Loader component for mini viewer
function STLModel({ url, accessToken, fileData, autoRotate, onBoundingBoxCalculated }: { 
  url?: string; 
  accessToken?: string; 
  fileData?: ArrayBuffer | Blob; 
  autoRotate: boolean;
  onBoundingBoxCalculated?: (boundingBox: THREE.Box3) => void;
}) {
  const meshRef = useRef<THREE.Mesh>(null);
  const [geometry, setGeometry] = useState<THREE.BufferGeometry | null>(null);

  useEffect(() => {
    const loadSTL = async () => {
      try {
        const loader = new STLLoader();
        let arrayBuffer: ArrayBuffer;
        
        if (fileData) {
          if (fileData instanceof Blob) {
            arrayBuffer = await fileData.arrayBuffer();
          } else {
            arrayBuffer = fileData;
          }
        } else if (url) {
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
        setGeometry(geometry);
        
        if (onBoundingBoxCalculated) {
          geometry.computeBoundingBox();
          const boundingBox = geometry.boundingBox!.clone();
          onBoundingBoxCalculated(boundingBox);
        }
      } catch (error) {
        console.error('Error loading STL:', error);
        // The parent component will handle the error state
      }
    };
    
    loadSTL();
  }, [url, accessToken, fileData, onBoundingBoxCalculated]);

  useFrame((state: any) => {
    if (meshRef.current && autoRotate) {
      meshRef.current.rotation.y += 0.01;
    }
  });

  if (!geometry) {
    return null;
  }

  return (
    <mesh ref={meshRef} geometry={geometry}>
      <meshStandardMaterial 
        color="#888888"
        metalness={0.5}
        roughness={0.5}
      />
    </mesh>
  );
}

// GLTF/GLB Model component for mini viewer
function GLTFModel({ url, accessToken, fileData, autoRotate, onBoundingBoxCalculated }: { 
  url?: string; 
  accessToken?: string; 
  fileData?: ArrayBuffer | Blob; 
  autoRotate: boolean;
  onBoundingBoxCalculated?: (boundingBox: THREE.Box3) => void;
}) {
  const [scene, setScene] = useState<THREE.Group | null>(null);
  const meshRef = useRef<THREE.Group>(null);

  useEffect(() => {
    const loadGLTF = async () => {
      try {
        const { GLTFLoader } = await import('three/examples/jsm/loaders/GLTFLoader.js');
        const loader = new GLTFLoader();
        
        let arrayBuffer: ArrayBuffer;
        
        if (fileData) {
          if (fileData instanceof Blob) {
            arrayBuffer = await fileData.arrayBuffer();
          } else {
            arrayBuffer = fileData;
          }
        } else if (url) {
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
        
        setScene(gltf.scene);
        
        if (onBoundingBoxCalculated) {
          const boundingBox = new THREE.Box3().setFromObject(gltf.scene);
          onBoundingBoxCalculated(boundingBox);
        }
      } catch (error) {
        console.error('Error loading GLTF:', error);
        // The parent component will handle the error state
      }
    };
    
    loadGLTF();
  }, [url, accessToken, fileData, onBoundingBoxCalculated]);

  useFrame((state: any) => {
    if (meshRef.current && autoRotate) {
      meshRef.current.rotation.y += 0.01;
    }
  });

  if (!scene) {
    return null;
  }

  return <primitive ref={meshRef} object={scene} />;
}

// Camera Controller for mini viewer
function CameraController({ 
  cameraSettings
}: { 
  cameraSettings: {
    position: [number, number, number];
    minDistance: number;
    maxDistance: number;
    target: [number, number, number];
  };
}) {
  const { camera } = useThree();
  
  useEffect(() => {
    camera.position.set(...cameraSettings.position);
    camera.lookAt(...cameraSettings.target);
    camera.updateMatrixWorld();
  }, [camera, cameraSettings]);

  return null;
}

// Loading component for mini viewer
function Loader() {
  const { progress } = useProgress();
  return (
    <Html center>
      <div className="text-white text-xs">
        {Math.round(progress)}%
      </div>
    </Html>
  );
}

/**
 * MiniModelViewer Component
 * 
 * A lightweight 3D model viewer optimized for small previews in grid layouts.
 * Features:
 * - Automatic camera positioning based on model bounding box
 * - Auto-rotate functionality controlled by user settings
 * - Simplified rendering for performance
 * - Support for STL and GLTF/GLB files
 */
const MiniModelViewer: React.FC<MiniModelViewerProps> = ({
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
  onBoundingBoxCalculated
}) => {
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [streamUrl, setStreamUrl] = useState<string | null>(null);
  const [hasError, setHasError] = useState(false);
  
  // Camera positioning state
  const [cameraSettings, setCameraSettings] = useState({
    position: [0, 0, 5] as [number, number, number],
    minDistance: 1,
    maxDistance: 2000,
    target: [0, 0, 0] as [number, number, number]
  });

  // Calculate optimal camera position based on bounding box
  const calculateCameraPosition = (boundingBox: THREE.Box3) => {
    const size = boundingBox.getSize(new THREE.Vector3());
    const center = boundingBox.getCenter(new THREE.Vector3());
    
    const maxDimension = Math.max(size.x, size.y, size.z);
    const effectiveMaxDimension = Math.max(maxDimension, 1);
    
    const distanceFactor = 2.0; // Slightly closer for mini viewer
    const optimalDistance = effectiveMaxDimension * distanceFactor;
    const clampedDistance = Math.max(optimalDistance, 1);
    
    const minDistance = Math.max(effectiveMaxDimension * 0.5, 0.5);
    const maxDistance = Math.max(clampedDistance * 4, 100);
    
    const cameraPosition: [number, number, number] = [
      center.x,
      center.y,
      center.z + clampedDistance
    ];
    
    setCameraSettings({
      position: cameraPosition,
      minDistance,
      maxDistance,
      target: [center.x, center.y, center.z]
    });
  };

  // Handle bounding box calculation
  const handleBoundingBoxCalculated = (boundingBox: THREE.Box3) => {
    calculateCameraPosition(boundingBox);
    if (onBoundingBoxCalculated) {
      onBoundingBoxCalculated(boundingBox);
    }
  };

  useEffect(() => {
    const generateStreamUrl = () => {
      if (fileData) {
        return null;
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
          console.error('Model loading error:', err);
          setError('Failed to load model');
          setHasError(true);
          setIsLoading(false);
        });
    }
  }, [modelUrl, fileId, modelId, fileName, fileData, accessToken]);

  const getFileType = () => {
    if (fileType) {
      const lastDotIndex = fileType.lastIndexOf('.');
      if (lastDotIndex !== -1) {
        const extension = fileType.substring(lastDotIndex + 1).toLowerCase();
        return extension;
      }
      return fileType.toLowerCase();
    }
    if (fileName) {
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
    return 'stl';
  };

  const fileTypeLower = getFileType();
  const isSTL = fileTypeLower === 'stl';
  const isGLTF = fileTypeLower === 'gltf' || fileTypeLower === 'glb';
  const is3MF = fileTypeLower === '3mf';
  const isSTEP = fileTypeLower === 'step';

  if (!isSTL && !isGLTF && !is3MF && !isSTEP) {
    return (
      <div 
        className={`bg-gray-800 rounded-lg flex items-center justify-center ${className}`}
        style={{ width, height }}
      >
        <div className="text-gray-400 text-xs">
          Unsupported format
        </div>
      </div>
    );
  }

  if (hasError) {
    return (
      <div 
        className={`bg-gray-800 rounded-lg flex items-center justify-center ${className}`}
        style={{ width, height }}
      >
        <div className="text-gray-400 text-xs">
          Failed to load model
          <br />
          <small>Debug: {modelId} / {fileName}</small>
        </div>
      </div>
    );
  }

  return (
    <div 
      className={`bg-gray-900 rounded-lg overflow-hidden relative ${className}`}
      style={{ width, height }}
    >
      <Canvas
        camera={{ 
          position: cameraSettings.position, 
          fov: 50 
        }}
        style={{ width, height }}
      >
        <CameraController 
          cameraSettings={cameraSettings}
        />
        <ambientLight intensity={0.6} />
        <directionalLight 
          position={[10, 10, 5]} 
          intensity={0.8} 
        />
        
        {isLoading && <Loader />}
        
        {isSTL && (
          <STLModel 
            url={streamUrl || undefined} 
            accessToken={accessToken} 
            fileData={fileData}
            autoRotate={autoRotate}
            onBoundingBoxCalculated={handleBoundingBoxCalculated}
          />
        )}
        {isGLTF && (
          <GLTFModel 
            url={streamUrl || undefined} 
            accessToken={accessToken} 
            fileData={fileData}
            autoRotate={autoRotate}
            onBoundingBoxCalculated={handleBoundingBoxCalculated}
          />
        )}
        
        <OrbitControls 
          enablePan={false}
          enableZoom={false}
          enableRotate={true}
          autoRotate={false}
          maxDistance={cameraSettings.maxDistance}
          minDistance={cameraSettings.minDistance}
          target={cameraSettings.target}
          rotateSpeed={0.5}
        />
      </Canvas>
    </div>
  );
};

export default MiniModelViewer; 