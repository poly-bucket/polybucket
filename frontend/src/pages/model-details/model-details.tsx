import { useState } from 'react';
import { ModelViewer } from '../../components/model-viewer/model-viewer';
import { CommentSection } from '../../components/comments/comment-section';

type Tab = 'details' | 'comments' | 'makes' | 'remixes' | 'apps';

export const ModelDetails = () => {
  const [activeTab, setActiveTab] = useState<Tab>('details');
  
  // This would come from your API/route params
  const model = {
    id: 1,
    title: "Cool 3D Model",
    description: "An awesome 3D printable model with detailed features perfect for 3D printing. This model has been tested and prints well with standard settings.",
    creator: "John Doe",
    createdAt: "2024-02-20",
    license: "Creative Commons - Attribution",
    categories: ["Toys", "Games", "Accessories"],
    modelUrl: "../../src/sample/model1.stl",
    likes: 156,
    downloads: 12
  };

  const renderTabContent = () => {
    switch (activeTab) {
      case 'details':
        return (
          <div className="space-y-6">
            <div className="prose prose-invert max-w-none">
              <h2 className="text-xl font-semibold text-green-400">Description</h2>
              <p className="text-green-300/80">{model.description}</p>
            </div>
            
            <div className="grid grid-cols-2 gap-6">
              <div>
                <h3 className="mb-2 text-lg font-semibold text-green-400">Created by</h3>
                <p className="text-green-300/80">{model.creator}</p>
              </div>
              <div>
                <h3 className="mb-2 text-lg font-semibold text-green-400">Published on</h3>
                <p className="text-green-300/80">{model.createdAt}</p>
              </div>
              <div>
                <h3 className="mb-2 text-lg font-semibold text-green-400">License</h3>
                <p className="text-green-300/80">{model.license}</p>
              </div>
              <div>
                <h3 className="mb-2 text-lg font-semibold text-green-400">Categories</h3>
                <div className="flex flex-wrap gap-2">
                  {model.categories.map(category => (
                    <span key={category} className="badge">{category}</span>
                  ))}
                </div>
              </div>
            </div>
          </div>
        );
      case 'comments':
        return <CommentSection modelId={model.id} />;
      case 'makes':
        return <div className="text-green-300/80">Community makes will appear here.</div>;
      case 'remixes':
        return <div className="text-green-300/80">Remixed versions will appear here.</div>;
      case 'apps':
        return <div className="text-green-300/80">Compatible apps will appear here.</div>;
    }
  };

  return (
    <div className="container px-4 py-8 mx-auto max-w-7xl">
      <div className="mb-8">
        <h1 className="mb-4 text-3xl font-bold text-green-400">{model.title}</h1>
        <div className="flex items-center gap-4 text-green-300/80">
          <span>by {model.creator}</span>
          <span>•</span>
          <span>{model.likes} likes</span>
          <span>•</span>
          <span>{model.downloads} downloads</span>
        </div>
      </div>

      {/* Model Viewer */}
      <div className="mb-8">
        <ModelViewer 
          modelUrl={model.modelUrl}
          color="#808080"
        />
      </div>

      {/* Action Buttons */}
      <div className="flex gap-4 mb-8">
        <button className="btn-primary">
          Download
        </button>
        <button className="btn-secondary">
          Like
        </button>
        <button className="btn-secondary">
          Share
        </button>
      </div>

      {/* Tabs */}
      <div className="border-b border-green-500/30">
        <nav className="flex gap-8">
          {[
            { id: 'details', label: 'Thing Details' },
            { id: 'comments', label: 'Comments' },
            { id: 'makes', label: 'Makes' },
            { id: 'remixes', label: 'Remixes' },
            { id: 'apps', label: 'Apps' }
          ].map(tab => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id as Tab)}
              className={`px-4 py-2 font-medium border-b-2 transition-colors ${
                activeTab === tab.id
                  ? 'border-green-500 text-green-400'
                  : 'border-transparent text-green-300/60 hover:text-green-300'
              }`}
            >
              {tab.label}
            </button>
          ))}
        </nav>
      </div>

      {/* Tab Content */}
      <div className="py-8">
        {renderTabContent()}
      </div>
    </div>
  );
}; 