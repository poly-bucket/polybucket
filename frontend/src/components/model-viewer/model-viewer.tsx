import { Canvas } from '@react-three/fiber';
import { OrbitControls, Stage } from '@react-three/drei';
import { Suspense, useState, useEffect } from 'react';
import { Model } from "../model-viewer/model"
import { ArrowsPointingOutIcon, Cog6ToothIcon, XMarkIcon } from '@heroicons/react/24/solid';
import { ModelControls } from './model-controls';
import { ViewerControls } from './types';

interface ModelViewerProps {
  modelUrl: string;
  color: string;
  metalness: number;
  roughness: number;
  onControlsChange?: (controls: ViewerControls) => void;
}

export const ModelViewer: React.FC<ModelViewerProps> = ({
  modelUrl,
  color,
  metalness,
  roughness,
  onControlsChange
}) => {
  const [isFullscreen, setIsFullscreen] = useState(false);
  const [showControls, setShowControls] = useState(false);

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

  const toggleControls = () => {
    setShowControls(!showControls);
  };

  const handleControlsChange = (newControls: ViewerControls) => {
    onControlsChange?.(newControls);
  };

  return (
    <div className="relative aspect-square">
      <div className="absolute flex gap-2 top-4 right-4">
        <button
          type="button"
          onClick={toggleControls}
          className="p-2 text-green-400 border rounded-lg bg-gray-900/90 border-green-500/30 backdrop-blur-sm hover:bg-gray-800"
          aria-label="Toggle controls"
        >
          <Cog6ToothIcon className="w-5 h-5" />
        </button>
        <button
          type="button"
          onClick={toggleFullscreen}
          className="p-2 text-green-400 border rounded-lg bg-gray-900/90 border-green-500/30 backdrop-blur-sm hover:bg-gray-800"
          aria-label="Toggle fullscreen"
        >
          <ArrowsPointingOutIcon className="w-5 h-5" />
        </button>
      </div>

      {showControls && (
        <ModelControls
          controls={{ color, metalness, roughness }}
          onChange={handleControlsChange}
        />
      )}

      <div 
        className={`relative ${
          isFullscreen 
            ? 'fixed inset-0 z-50 bg-gray-100' 
            : 'w-full h-[400px] bg-gray-100 rounded-lg'
        }`}
      >
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
                metalness={metalness}
                roughness={roughness}
              />
            </Stage>
          </Suspense>
          <OrbitControls 
            autoRotate={false}
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
      </div>
    </div>
  );
}; 