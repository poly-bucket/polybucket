import { Link } from 'react-router-dom';
import { ModelViewer } from '../model-viewer/model-viewer';

interface ModelCardProps {
  model: {
    id: number;
    title: string;
    description: string;
    thumbnailUrl: string;
    modelUrl: string;
    likes: number;
    downloads: number;
    creator: string;
  };
}

export const ModelCard: React.FC<ModelCardProps> = ({ model }) => {
  // Function to format numbers (e.g., 1000 -> 1k)
  const formatNumber = (num: number): string => {
    if (num >= 1000000) {
      return `${(num / 1000000).toFixed(1)}M`;
    }
    if (num >= 1000) {
      return `${(num / 1000).toFixed(1)}k`;
    }
    return num.toString();
  };

  return (
    <div className="bg-gray-900 rounded-lg border border-green-500/30 overflow-hidden transition-transform duration-200 hover:scale-[1.02]">
      <Link to={`/model/${model.id}`}>
        <div className="relative">
          <div className="p-4">
            <h3 className="mb-2 text-lg font-semibold text-green-400">{model.title}</h3>
            <p className="mb-3 text-sm text-green-300/80 line-clamp-2">{model.description}</p>
          </div>
          <ModelViewer 
            modelUrl={model.modelUrl}
            color="#808080"
            metalness={0.5}
            roughness={0.5}
          />
        </div>
        <div className="flex flex-col gap-2 p-4">
          <div className="text-sm text-green-300/80">
            by <span className="font-medium text-green-400 hover:underline">{model.creator}</span>
          </div>
          
          <div className="flex items-center justify-between text-sm">
            <div className="flex items-center gap-4">
              <div className="flex items-center gap-1 text-green-400">
                <svg className="w-4 h-4 text-green-500" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M3.172 5.172a4 4 0 015.656 0L10 6.343l1.172-1.171a4 4 0 115.656 5.656L10 17.657l-6.828-6.829a4 4 0 010-5.656z" clipRule="evenodd" />
                </svg>
                <span>{formatNumber(model.likes)}</span>
              </div>
              
              <div className="flex items-center gap-1 text-green-400">
                <svg className="w-4 h-4 text-green-500" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M3 17a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm3.293-7.707a1 1 0 011.414 0L9 10.586V3a1 1 0 112 0v7.586l1.293-1.293a1 1 0 111.414 1.414l-3 3a1 1 0 01-1.414 0l-3-3a1 1 0 010-1.414z" clipRule="evenodd" />
                </svg>
                <span>{formatNumber(model.downloads)}</span>
              </div>
            </div>
          </div>
        </div>
      </Link>
    </div>
  );
}; 