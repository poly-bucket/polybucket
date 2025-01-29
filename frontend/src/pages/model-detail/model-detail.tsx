// import { useParams } from 'react-router-dom';
import { ModelViewer } from '../../components/model-viewer/model-viewer';
import { useState } from 'react';
import { ViewerControls } from '../../components/model-viewer/types';
import { CommentSection } from '../../components/comments/comment-section';

export const ModelDetail: React.FC = () => {
  // const { id } = useParams();
  
  // Add state for model properties
  const [modelColor, setModelColor] = useState('#808080');
  const [metalness, setMetalness] = useState(0.5);
  const [roughness, setRoughness] = useState(0.5);

  // This would normally come from an API
  const model = {
    id: 1,
    title: "Cool 3D Model",
    description: "An awesome 3D printable model with detailed features perfect for 3D printing. This model has been tested and prints well with standard settings.",
    modelUrl: "../src/sample/model1.stl",
    likes: 156,
    dislikes: 12,
    downloads: 1234,
    creator: "John Doe",
    createdAt: "2024-02-20",
    license: "Creative Commons - Attribution",
    categories: ["Toys", "Games", "Accessories"]
  };

  const handleControlsChange = (controls: ViewerControls) => {
    setModelColor(controls.color);
    setMetalness(controls.metalness);
    setRoughness(controls.roughness);
  };

  return (
    <div className="container px-4 py-8 mx-auto bg-black">
      <div className="grid grid-cols-1 gap-8 lg:grid-cols-2">
        <div>
          <div className="p-4 bg-gray-900 border rounded-lg border-green-500/30">
            <ModelViewer 
              modelUrl={model.modelUrl}
              color={modelColor}
              metalness={metalness}
              roughness={roughness}
              onControlsChange={handleControlsChange}
            />
          </div>
        </div>
        
        <div className="p-6 bg-gray-900 border rounded-lg border-green-500/30">
          <h1 className="mb-4 text-3xl font-bold text-green-400">{model.title}</h1>
          <div className="flex gap-4 mb-6">
            <button className="px-6 py-2 text-black bg-green-500 rounded-md hover:bg-green-400">
              Download
            </button>
            <div className="flex gap-2">
              <button className="flex items-center gap-2 px-6 py-2 text-green-400 border rounded-md border-green-500/30 hover:bg-gray-800">
                <span>❤️</span>
                <span>{model.likes}</span>
              </button>
              <button className="flex items-center gap-2 px-6 py-2 text-green-400 border rounded-md border-green-500/30 hover:bg-gray-800">
                <span>👎</span>
                <span>{model.dislikes}</span>
              </button>
            </div>
          </div>
          
          <div className="mb-6">
            <h2 className="mb-2 text-xl font-semibold text-green-400">Description</h2>
            <p className="text-green-300/80">{model.description}</p>
          </div>
          
          <div className="grid grid-cols-2 gap-4 mb-6">
            <div>
              <h3 className="font-semibold text-green-400">Created by</h3>
              <p className="text-green-300/80">{model.creator}</p>
            </div>
            <div>
              <h3 className="font-semibold text-green-400">Published on</h3>
              <p className="text-green-300/80">{model.createdAt}</p>
            </div>
            <div>
              <h3 className="font-semibold text-green-400">License</h3>
              <p className="text-green-300/80">{model.license}</p>
            </div>
            <div>
              <h3 className="font-semibold text-green-400">Categories</h3>
              <div className="flex flex-wrap gap-2">
                {model.categories.map(category => (
                  <span key={category} className="px-2 py-1 text-sm text-green-400 bg-gray-800 border rounded-md border-green-500/30">
                    {category}
                  </span>
                ))}
              </div>
            </div>
          </div>
        </div>
      </div>
      
      <div className="mt-12">
        <CommentSection modelId={model.id} />
      </div>
    </div>
  );
}; 