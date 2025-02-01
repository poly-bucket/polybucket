import { Link } from 'react-router-dom';
import { ModelViewer } from '../model-viewer/model-viewer';
import { Model } from '../../types/models';

interface ModelCardProps {
  model: Model;
}

export const ModelCard: React.FC<ModelCardProps> = ({ model }) => {
  // Get the first STL file for the model viewer
  const stlFile = model.files.find(f => f.name.toLowerCase().endsWith('.stl'));
  // Construct the full URL for the model file
  const modelUrl = stlFile ? `https://localhost:44378/files/${stlFile.name}` : '';

  return (
    <div className="bg-gray-900 rounded-lg border border-green-500/30 overflow-hidden transition-transform duration-200 hover:scale-[1.02]">
      <Link to={`/model/${model.id}`}>
        <div className="relative">
          <div className="p-4">
            <h3 className="mb-2 text-lg font-semibold text-green-400">{model.name}</h3>
            <p className="mb-3 text-sm text-green-300/80 line-clamp-2">{model.description}</p>
            <div className="flex flex-wrap gap-2 mb-3">
              {model.categories.map((category, index) => (
                <span 
                  key={index} 
                  className="px-2 py-1 text-xs text-green-400 bg-gray-800 rounded-full"
                >
                  {category}
                </span>
              ))}
            </div>
            <div className="flex items-center gap-2 text-sm text-green-300/60">
              <span>{new Date(model.createdAt).toLocaleDateString()}</span>
              {model.wip && <span className="px-1 text-xs bg-yellow-500/20 rounded">WIP</span>}
              {model.aiGenerated && <span className="px-1 text-xs bg-blue-500/20 rounded">AI</span>}
            </div>
          </div>
          {modelUrl ? (
            <div className="relative aspect-[16/9] w-full">
              <ModelViewer 
                modelUrl={modelUrl}
                color="#808080"
              />
            </div>
          ) : (
            <div className="flex items-center justify-center w-full aspect-[16/9] bg-gray-800">
              <div className="text-green-400/60 text-sm">
                Preview not available
              </div>
            </div>
          )}
        </div>
      </Link>
    </div>
  );
}; 