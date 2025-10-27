import React, { useState, useRef, useCallback } from 'react';
import { Canvas, useThree, useFrame } from '@react-three/fiber';
import { OrbitControls, Stage } from '@react-three/drei';
import { Suspense } from 'react';
import * as THREE from 'three';
import { STLLoader } from 'three/examples/jsm/loaders/STLLoader.js';
import { GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader.js';
import { OBJLoader } from 'three/examples/jsm/loaders/OBJLoader.js';

interface ThumbnailGeneratorProps {
  modelFile: File;
  onThumbnailGenerated: (thumbnailBlob: Blob, fileName: string) => void;
  onCancel: () => void;
}

interface RenderSettings {
  backgroundColor: string;
  modelColor: string;
  metalness: number;
  roughness: number;
  autoRotate: boolean;
  cameraDistance: number;
  lightIntensity: number;
  lightColor: string;
  viewMode: 'solid' | 'wireframe' | 'points';
}

// Model component for thumbnail generation
const ThumbnailModel = ({ 
  file, 
  settings,
  onGeometryLoaded 
}: { 
  file: File;
  settings: RenderSettings;
  onGeometryLoaded?: (geometry: THREE.BufferGeometry | null) => void;
}) => {
  const [geometry, setGeometry] = useState<THREE.BufferGeometry | null>(null);
  const [centeredGeometry, setCenteredGeometry] = useState<THREE.BufferGeometry | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  React.useEffect(() => {
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
        let loadedGeometry: THREE.BufferGeometry | null = null;

        if (fileExtension === '.stl') {
          const loader = new STLLoader();
          const arrayBuffer = await file.arrayBuffer();
          loadedGeometry = loader.parse(arrayBuffer);
        } else if (fileExtension === '.gltf' || fileExtension === '.glb') {
          const loader = new GLTFLoader();
          const arrayBuffer = await file.arrayBuffer();
          const gltf = await new Promise<{ scene: THREE.Group }>((resolve, reject) => {
            loader.parse(arrayBuffer, '', resolve, reject);
          });
          
          // Convert GLTF scene to geometry (simplified approach)
          const box = new THREE.Box3().setFromObject(gltf.scene);
          const size = box.getSize(new THREE.Vector3());
          const geometry = new THREE.BoxGeometry(size.x, size.y, size.z);
          loadedGeometry = geometry;
        } else if (fileExtension === '.obj') {
          const loader = new OBJLoader();
          const arrayBuffer = await file.arrayBuffer();
          const object = loader.parse(new TextDecoder().decode(arrayBuffer));
          
          // Convert OBJ object to geometry (simplified approach)
          const box = new THREE.Box3().setFromObject(object);
          const size = box.getSize(new THREE.Vector3());
          const geometry = new THREE.BoxGeometry(size.x, size.y, size.z);
          loadedGeometry = geometry;
        } else if (fileExtension === '.step' || fileExtension === '.stp') {
          // For STEP files, create a placeholder geometry since we don't have a STEP loader
          // In a real implementation, you'd want to use a STEP parser library
          const geometry = new THREE.BoxGeometry(1, 1, 1);
          loadedGeometry = geometry;
        }

        if (loadedGeometry) {
          setGeometry(loadedGeometry);
          
          // Center and scale the geometry
          loadedGeometry.computeBoundingBox();
          const boundingBox = loadedGeometry.boundingBox!;
          const center = boundingBox.getCenter(new THREE.Vector3());
          const size = boundingBox.getSize(new THREE.Vector3());
          const maxDim = Math.max(size.x, size.y, size.z);
          const scale = 1 / maxDim;
          
          const centered = loadedGeometry.clone();
          centered.translate(-center.x, -center.y, -center.z);
          centered.scale(scale, scale, scale);
          
          setCenteredGeometry(centered);
          
          if (onGeometryLoaded) {
            onGeometryLoaded(centered);
          }
        }
      } catch (error) {
        console.error('Error loading model:', error);
      } finally {
        setIsLoading(false);
      }
    };

    loadModel();
  }, [file, onGeometryLoaded]);

  if (isLoading) {
    return null;
  }

  if (!centeredGeometry) return null;

  if (settings.viewMode === 'points') {
    return (
      <group>
        <points geometry={centeredGeometry}>
          <pointsMaterial 
            size={0.5}
            color={settings.modelColor}
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
          color={settings.modelColor}
          metalness={settings.metalness}
          roughness={settings.roughness}
          wireframe={settings.viewMode === 'wireframe'}
          side={THREE.DoubleSide}
        />
      </mesh>
    </group>
  );
};

// Camera controller for thumbnail generation
const ThumbnailCameraController = ({ 
  settings,
  onCameraReady,
  onRendererReady
}: { 
  settings: RenderSettings;
  onCameraReady: () => void;
  onRendererReady: (renderer: THREE.WebGLRenderer) => void;
}) => {
  const { camera, gl } = useThree();
  
  React.useEffect(() => {
    camera.position.set(0, 0, settings.cameraDistance);
    camera.lookAt(0, 0, 0);
    camera.updateMatrixWorld();
    onCameraReady();
    onRendererReady(gl);
  }, [camera, settings.cameraDistance, onCameraReady, gl, onRendererReady]);

  return null;
};

// Custom hook for taking screenshots
const useScreenshot = () => {
  const { gl, scene, camera } = useThree();
  
  const takeScreenshot = useCallback(async (): Promise<Blob> => {
    return new Promise((resolve, reject) => {
      try {
        // Force a render
        gl.render(scene, camera);
        
        // Take the screenshot
        gl.domElement.toBlob((blob) => {
          if (blob && blob.size > 0) {
            resolve(blob);
          } else {
            reject(new Error('Failed to capture screenshot - empty blob'));
          }
        }, 'image/png', 0.9);
      } catch (error) {
        reject(error);
      }
    });
  }, [gl, scene, camera]);
  
  return { takeScreenshot };
};

// Screenshot component that provides the screenshot functionality
const ScreenshotProvider = ({ 
  onScreenshotReady 
}: { 
  onScreenshotReady: (takeScreenshot: () => Promise<Blob>) => void;
}) => {
  const { takeScreenshot } = useScreenshot();
  
  React.useEffect(() => {
    onScreenshotReady(takeScreenshot);
  }, [takeScreenshot, onScreenshotReady]);
  
  return null;
};

// WebGL context monitor component
const WebGLMonitor = ({ onContextLost }: { onContextLost: () => void }) => {
  const { gl } = useThree();
  
  React.useEffect(() => {
    const handleContextLost = () => {
      console.warn('WebGL context lost in ThumbnailGenerator');
      onContextLost();
    };

    const handleContextRestored = () => {
      console.log('WebGL context restored in ThumbnailGenerator');
    };

    gl.domElement.addEventListener('webglcontextlost', handleContextLost);
    gl.domElement.addEventListener('webglcontextrestored', handleContextRestored);

    return () => {
      gl.domElement.removeEventListener('webglcontextlost', handleContextLost);
      gl.domElement.removeEventListener('webglcontextrestored', handleContextRestored);
    };
  }, [gl, onContextLost]);

  return null;
};

const ThumbnailGenerator: React.FC<ThumbnailGeneratorProps> = ({ 
  modelFile, 
  onThumbnailGenerated, 
  onCancel 
}) => {
  const [renderSettings, setRenderSettings] = useState<RenderSettings>({
    backgroundColor: '#1a1a1a',
    modelColor: '#888888',
    metalness: 0.5,
    roughness: 0.5,
    autoRotate: false,
    cameraDistance: 2.5,
    lightIntensity: 1.0,
    lightColor: '#ffffff',
    viewMode: 'solid'
  });

  const [isGenerating, setIsGenerating] = useState(false);
  const [cameraReady, setCameraReady] = useState(false);
  const [webglError, setWebglError] = useState<string | null>(null);
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const rendererRef = useRef<THREE.WebGLRenderer | null>(null);
  const takeScreenshotRef = useRef<(() => Promise<Blob>) | null>(null);

  // Check WebGL support on mount
  React.useEffect(() => {
    const canvas = document.createElement('canvas');
    const gl = canvas.getContext('webgl') || canvas.getContext('webgl2');
    
    if (!gl) {
      setWebglError('WebGL is not supported in this browser');
    } else {
      // Test basic WebGL functionality
      try {
        gl.createBuffer();
        gl.deleteBuffer(gl.createBuffer());
      } catch (error) {
        setWebglError('WebGL context creation failed');
      }
    }
  }, []);

  const handleSettingChange = (key: keyof RenderSettings, value: any) => {
    setRenderSettings(prev => ({ ...prev, [key]: value }));
  };

  const handleRefreshCanvas = () => {
    setWebglError(null);
    setCameraReady(false);
    // Force a re-render of the canvas
    if (canvasRef.current) {
      const canvas = canvasRef.current;
      const gl = canvas.getContext('webgl') || canvas.getContext('webgl2');
      if (gl && gl.isContextLost()) {
        // Try to restore the context
        gl.getExtension('WEBGL_lose_context')?.loseContext();
        setTimeout(() => {
          gl.getExtension('WEBGL_lose_context')?.restoreContext();
        }, 100);
      }
    }
  };

  const generateThumbnail = useCallback(async () => {
    if (!cameraReady || !takeScreenshotRef.current) {
      console.log('Cannot generate thumbnail: camera or screenshot function not ready');
      return;
    }

    setIsGenerating(true);

    try {
      // Wait for the render to complete
      await new Promise(resolve => setTimeout(resolve, 500));

      // Use the screenshot function
      const blob = await takeScreenshotRef.current();
      
      const fileName = `${modelFile.name.replace(/\.[^/.]+$/, '')}-thumbnail.png`;
      console.log(`Thumbnail generated successfully: ${fileName}, size: ${blob.size} bytes`);
      onThumbnailGenerated(blob, fileName);
      setWebglError(null); // Clear any previous errors
    } catch (error) {
      console.error('Error generating thumbnail:', error);
      setWebglError(error instanceof Error ? error.message : 'Unknown error occurred');
      // Show user-friendly error message
      alert('Failed to generate thumbnail. Please try again or adjust the settings.');
    } finally {
      setIsGenerating(false);
    }
  }, [cameraReady, onThumbnailGenerated, modelFile.name]);

  return (
    <div className="lg-modal-overlay">
      <div className="lg-modal max-w-4xl max-h-[90vh] overflow-y-auto">
        <div className="flex justify-between items-center mb-4">
          <h2 className="text-xl font-bold text-white">Generate Custom Image</h2>
          <button
            onClick={onCancel}
            className="text-white/40 hover:text-white/80 transition-colors"
          >
            ✕
          </button>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* 3D Preview */}
          <div className="lg:col-span-2">
            <div className="lg-card overflow-hidden h-96">
              <Canvas
                ref={canvasRef}
                camera={{ position: [0, 0, renderSettings.cameraDistance], fov: 45 }}
                gl={{ preserveDrawingBuffer: true }}
                style={{ 
                  background: renderSettings.backgroundColor,
                  width: '100%',
                  height: '100%'
                }}
              >
                <ThumbnailCameraController 
                  settings={renderSettings}
                  onCameraReady={() => setCameraReady(true)}
                  onRendererReady={(renderer) => {
                    rendererRef.current = renderer;
                  }}
                />
                
                <ScreenshotProvider 
                  onScreenshotReady={(takeScreenshot) => {
                    takeScreenshotRef.current = takeScreenshot;
                  }}
                />
                
                <WebGLMonitor onContextLost={() => setWebglError('WebGL context was lost')} />
                
                <ambientLight intensity={0.4} />
                <directionalLight 
                  position={[10, 10, 5]} 
                  intensity={renderSettings.lightIntensity}
                  color={renderSettings.lightColor}
                />
                
                <Suspense fallback={null}>
                  <Stage environment="city" intensity={0.6}>
                    <ThumbnailModel 
                      file={modelFile}
                      settings={renderSettings}
                    />
                  </Stage>
                </Suspense>
                
                <OrbitControls 
                  autoRotate={renderSettings.autoRotate}
                  autoRotateSpeed={1}
                  enableZoom={true}
                  enablePan={true}
                  enableRotate={true}
                  maxDistance={10}
                  minDistance={0.5}
                  target={[0, 0, 0]}
                />
              </Canvas>
            </div>
          </div>

          {/* Controls */}
          <div className="lg-card p-4">
            <div>
              <h3 className="text-lg font-semibold text-white mb-3">Render Settings</h3>
              
              {/* Background Color */}
              <div className="mb-3">
                <label className="block text-sm text-white/80 mb-1">Background Color</label>
                <input
                  type="color"
                  value={renderSettings.backgroundColor}
                  onChange={(e) => handleSettingChange('backgroundColor', e.target.value)}
                  className="w-full h-10 rounded border border-white/20 bg-white/10"
                />
              </div>

              {/* Model Color */}
              <div className="mb-3">
                <label className="block text-sm text-white/80 mb-1">Model Color</label>
                <input
                  type="color"
                  value={renderSettings.modelColor}
                  onChange={(e) => handleSettingChange('modelColor', e.target.value)}
                  className="w-full h-10 rounded border border-white/20 bg-white/10"
                />
              </div>

              {/* Metalness */}
              <div className="mb-3">
                <label className="block text-sm text-white/80 mb-1">Metalness: {renderSettings.metalness}</label>
                <input
                  type="range"
                  min="0"
                  max="1"
                  step="0.1"
                  value={renderSettings.metalness}
                  onChange={(e) => handleSettingChange('metalness', parseFloat(e.target.value))}
                  className="w-full slider"
                />
              </div>

              {/* Roughness */}
              <div className="mb-3">
                <label className="block text-sm text-white/80 mb-1">Roughness: {renderSettings.roughness}</label>
                <input
                  type="range"
                  min="0"
                  max="1"
                  step="0.1"
                  value={renderSettings.roughness}
                  onChange={(e) => handleSettingChange('roughness', parseFloat(e.target.value))}
                  className="w-full slider"
                />
              </div>

              {/* View Mode */}
              <div className="mb-3">
                <label className="block text-sm text-white/80 mb-1">View Mode</label>
                <select
                  value={renderSettings.viewMode}
                  onChange={(e) => handleSettingChange('viewMode', e.target.value as any)}
                  className="lg-input"
                >
                  <option value="solid">Solid</option>
                  <option value="wireframe">Wireframe</option>
                  <option value="points">Points</option>
                </select>
              </div>

              {/* Camera Distance */}
              <div className="mb-3">
                <label className="block text-sm text-white/80 mb-1">Camera Distance: {renderSettings.cameraDistance}</label>
                <input
                  type="range"
                  min="1"
                  max="5"
                  step="0.1"
                  value={renderSettings.cameraDistance}
                  onChange={(e) => handleSettingChange('cameraDistance', parseFloat(e.target.value))}
                  className="w-full slider"
                />
              </div>

              {/* Light Intensity */}
              <div className="mb-3">
                <label className="block text-sm text-white/80 mb-1">Light Intensity: {renderSettings.lightIntensity}</label>
                <input
                  type="range"
                  min="0"
                  max="2"
                  step="0.1"
                  value={renderSettings.lightIntensity}
                  onChange={(e) => handleSettingChange('lightIntensity', parseFloat(e.target.value))}
                  className="w-full slider"
                />
              </div>

              {/* Light Color */}
              <div className="mb-3">
                <label className="block text-sm text-white/80 mb-1">Light Color</label>
                <input
                  type="color"
                  value={renderSettings.lightColor}
                  onChange={(e) => handleSettingChange('lightColor', e.target.value)}
                  className="w-full h-10 rounded border border-white/20 bg-white/10"
                />
              </div>

              {/* Auto Rotate */}
              <div className="mb-3">
                <label className="flex items-center text-sm text-white/80">
                  <input
                    type="checkbox"
                    checked={renderSettings.autoRotate}
                    onChange={(e) => handleSettingChange('autoRotate', e.target.checked)}
                    className="mr-2 accent-color: var(--lg-primary)"
                  />
                  Auto Rotate
                </label>
              </div>
            </div>

            {/* Error Display */}
            {webglError && (
              <div className="mb-3 p-3 bg-red-500/20 border border-red-500/50 rounded text-red-200 text-sm">
                <strong>Error:</strong> {webglError}
                <br />
                <small>Try refreshing the canvas or adjusting the settings.</small>
                <button
                  onClick={handleRefreshCanvas}
                  className="mt-2 w-full lg-button lg-button-secondary text-sm"
                >
                  Refresh Canvas
                </button>
              </div>
            )}

            {/* Action Buttons */}
            <div className="space-y-2">
              <button
                onClick={generateThumbnail}
                disabled={isGenerating || !cameraReady}
                className="w-full lg-button lg-button-primary disabled:opacity-50"
              >
                {isGenerating ? 'Generating...' : 'Generate Image'}
              </button>
              
              <button
                onClick={onCancel}
                className="w-full lg-button lg-button-secondary"
              >
                Cancel
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ThumbnailGenerator; 