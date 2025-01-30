import { Canvas } from '@react-three/fiber';
import { OrbitControls, Stage } from '@react-three/drei';
import { Suspense, useState, useEffect } from 'react';
import { Model } from "../model-viewer/model";
import { ArrowsPointingOutIcon, XMarkIcon } from '@heroicons/react/24/solid';

type ViewMode = 'solid' | 'wireframe' | 'points' | 'normals';

interface ModelViewerProps {
  modelUrl: string;
  color: string;
}

export const ModelViewer = ({
  modelUrl,
  color: initialColor,
}: ModelViewerProps) => {
  const [isFullscreen, setIsFullscreen] = useState(false);
  const [quality, setQuality] = useState<'Auto' | 'HD'>('Auto');
  const [speed, setSpeed] = useState(1);
  const [autoRotate, setAutoRotate] = useState(false);
  const [color, setColor] = useState(initialColor);
  const [roughnessValue, setRoughnessValue] = useState(0.5);
  const [metalnessValue, setMetalnessValue] = useState(0.5);
  const [viewMode, setViewMode] = useState<ViewMode>('solid');

  // Handle body class for overflow control
  useEffect(() => {
    if (isFullscreen) {
      document.body.classList.add('fullscreen-model');
    } else {
      document.body.classList.remove('fullscreen-model');
    }

    // Cleanup on unmount
    return () => {
      document.body.classList.remove('fullscreen-model');
    };
  }, [isFullscreen]);

  const toggleFullscreen = () => {
    setIsFullscreen(!isFullscreen);
  };

  return (
    <div className={`relative ${
      isFullscreen 
        ? 'model-viewer-fullscreen' 
        : 'model-viewer-container'
    }`}>
      <div className="w-full h-full bg-gray-100 rounded-lg">
        <Canvas 
          shadows 
          camera={{ 
            position: isFullscreen ? [0, 0, 200] : [0, 0, 150], 
            fov: isFullscreen ? 45 : 50 
          }}
          style={{ height: '100%' }}
        >
          <ambientLight intensity={0.5} />
          <pointLight position={[10, 10, 10]} />
          <Suspense fallback={null}>
            <Stage environment="city" intensity={0.6}>
              <Model 
                url={modelUrl} 
                color={color}
                metalness={metalnessValue}
                roughness={roughnessValue}
                viewMode={viewMode}
              />
            </Stage>
          </Suspense>
          <OrbitControls 
            autoRotate={autoRotate}
            autoRotateSpeed={speed}
            enableZoom={true}
            enablePan={true}
            enableRotate={true}
            maxDistance={500}
            minDistance={50}
          />
        </Canvas>

        {/* Close button for fullscreen mode */}
        {isFullscreen && (
          <button
            type="button"
            onClick={toggleFullscreen}
            className="absolute z-10 p-2 transition-all bg-white rounded-lg shadow-md top-4 left-4 bg-opacity-80 hover:bg-opacity-100"
            aria-label="Close fullscreen"
          >
            <XMarkIcon className="w-5 h-5" />
          </button>
        )}

        {/* Controls Overlay */}
        <div className="model-controls-overlay">
          <div className="flex items-center justify-between text-white">
            {/* Left Controls */}
            <div className="flex items-center gap-4">
              <button
                onClick={toggleFullscreen}
                className="flex items-center gap-2 hover:text-green-400"
              >
                <ArrowsPointingOutIcon className="w-5 h-5" />
                <span className="text-sm">Fullscreen</span>
              </button>

              <button
                onClick={() => setAutoRotate(!autoRotate)}
                className="flex items-center gap-2 hover:text-green-400"
              >
                <svg className="w-5 h-5" viewBox="0 0 24 24" fill="currentColor">
                  {autoRotate ? (
                    <path d="M10 9a3 3 0 100 6 3 3 0 000-6zm-5 3a5 5 0 110 10 5 5 0 010-10z" />
                  ) : (
                    <path d="M10 9a3 3 0 100 6 3 3 0 000-6z" />
                  )}
                </svg>
                <span className="text-sm">Auto-Rotate</span>
              </button>

              <div className="flex items-center gap-2">
                <span className="text-sm">Speed</span>
                <select 
                  value={speed}
                  onChange={(e) => setSpeed(Number(e.target.value))}
                  className="bg-transparent border border-white/20 rounded px-2 py-1 text-sm"
                >
                  <option value={0.5}>0.5x</option>
                  <option value={1}>1x</option>
                  <option value={1.5}>1.5x</option>
                  <option value={2}>2x</option>
                </select>
              </div>

              <div className="flex items-center gap-2">
                <span className="text-sm">Color</span>
                <input
                  type="color"
                  value={color}
                  onChange={(e) => setColor(e.target.value)}
                  className="w-8 h-8 bg-transparent border border-white/20 rounded cursor-pointer"
                />
              </div>

              <div className="flex items-center gap-2">
                <span className="text-sm">Roughness</span>
                <input
                  type="range"
                  min="0"
                  max="1"
                  step="0.1"
                  value={roughnessValue}
                  onChange={(e) => setRoughnessValue(Number(e.target.value))}
                  className="w-24 h-2 bg-transparent"
                />
              </div>

              <div className="flex items-center gap-2">
                <span className="text-sm">Metalness</span>
                <input
                  type="range"
                  min="0"
                  max="1"
                  step="0.1"
                  value={metalnessValue}
                  onChange={(e) => setMetalnessValue(Number(e.target.value))}
                  className="w-24 h-2 bg-transparent"
                />
              </div>

              <div className="flex items-center gap-2">
                <span className="text-sm">View Mode</span>
                <select 
                  value={viewMode}
                  onChange={(e) => setViewMode(e.target.value as ViewMode)}
                  className="bg-transparent border border-white/20 rounded px-2 py-1 text-sm"
                >
                  <option value="solid">Solid</option>
                  <option value="wireframe">Wireframe</option>
                  <option value="points">Points</option>
                  <option value="normals">Normals</option>
                </select>
              </div>
            </div>

            {/* Right Controls */}
            <div className="flex items-center gap-4">
              <button
                onClick={() => setQuality(quality === 'Auto' ? 'HD' : 'Auto')}
                className="flex items-center gap-2 hover:text-green-400"
              >
                <svg className="w-5 h-5" viewBox="0 0 24 24" fill="currentColor">
                  <path d="M12 15a3 3 0 100-6 3 3 0 000 6z" />
                  <path fillRule="evenodd" d="M1.323 11.447C2.811 6.976 7.028 3.75 12.001 3.75c4.97 0 9.185 3.223 10.675 7.69.12.362.12.752 0 1.113-1.487 4.471-5.705 7.697-10.677 7.697-4.97 0-9.186-3.223-10.675-7.69a1.762 1.762 0 010-1.113zM17.25 12a5.25 5.25 0 11-10.5 0 5.25 5.25 0 0110.5 0z" clipRule="evenodd" />
                </svg>
                <span className="text-sm">{quality}</span>
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}; 