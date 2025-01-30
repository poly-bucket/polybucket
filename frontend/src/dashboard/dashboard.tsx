import { useState } from 'react';
import { ModelCard } from '../components/model-card/model-card';
import { UploadModal } from '../components/upload-modal/upload-modal';
import { Model, ModelUploadData } from '../types/models';

export const Dashboard = () => {
  const [isUploadModalOpen, setIsUploadModalOpen] = useState(false);

  // This would normally come from an API
  const models: Model[] = [
    {
      id: 1,
      title: "Cool 3D Model",
      description: "An awesome 3D printable model",
      thumbnailUrl: "/sample/thumbnail1.jpg",
      modelUrl: "src/sample/model1.stl",
      likes: 156,
      downloads: 1234,
      creator: "John Doe"
    },
    // Add more sample models...
  ];

  const handleUpload = (modelData: ModelUploadData) => {
    // Handle the upload logic here
    console.log('Uploading model:', modelData);
    // You would typically make an API call here
  };

  return (
    <div className="container px-4 py-8 mx-auto">
      <div className="flex items-center justify-between mb-8">
        <h1 className="text-3xl font-bold text-green-400">Dashboard</h1>
        <button 
          onClick={() => setIsUploadModalOpen(true)}
          className="btn-primary"
        >
          Upload Model
        </button>
      </div>

      <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
        {models.map((model) => (
          <ModelCard key={model.id} model={model} />
        ))}
      </div>

      <UploadModal
        isOpen={isUploadModalOpen}
        onClose={() => setIsUploadModalOpen(false)}
        onUpload={handleUpload}
      />
    </div>
  );
};
