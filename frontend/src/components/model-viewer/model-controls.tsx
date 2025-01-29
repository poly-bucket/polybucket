import { ViewerControls } from './types';

interface ModelControlsProps {
  controls: ViewerControls;
  onChange: (controls: ViewerControls) => void;
}

export const ModelControls: React.FC<ModelControlsProps> = ({ controls, onChange }) => {
  return (
    <div className="absolute bottom-4 left-4 p-4 bg-gray-900/90 border rounded-lg border-green-500/30 backdrop-blur-sm">
      <div className="space-y-4">
        <div>
          <label className="block mb-2 text-sm font-medium text-green-400">
            Color
          </label>
          <input
            type="color"
            value={controls.color}
            onChange={(e) => onChange({ ...controls, color: e.target.value })}
            className="w-full h-8 bg-gray-800 border rounded cursor-pointer border-green-500/30"
            aria-label="Color control"
          />
        </div>
        
        <div>
          <label className="block mb-2 text-sm font-medium text-green-400">
            Metalness: {controls.metalness.toFixed(2)}
          </label>
          <input
            type="range"
            min="0"
            max="1"
            step="0.1"
            value={controls.metalness}
            onChange={(e) => onChange({ ...controls, metalness: Number(e.target.value) })}
            className="w-full accent-green-500"
            aria-label="Metalness control"
          />
        </div>
        
        <div>
          <label className="block mb-2 text-sm font-medium text-green-400">
            Roughness: {controls.roughness.toFixed(2)}
          </label>
          <input
            type="range"
            min="0"
            max="1"
            step="0.1"
            value={controls.roughness}
            onChange={(e) => onChange({ ...controls, roughness: Number(e.target.value) })}
            className="w-full accent-green-500"
            aria-label="Roughness control"
          />
        </div>
      </div>
    </div>
  );
}; 