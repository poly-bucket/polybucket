import { useState, useEffect } from 'react';
import { useParams, Navigate } from 'react-router-dom';
import { ModelViewer } from '../../components/model-viewer/model-viewer';
import { CommentSection } from '../../components/comments/comment-section';
import { ReportDialog } from '../../components/report-dialog/report-dialog';
import { ReportType } from '../../services/reports.service';
import { ModelsService } from '../../services/models.service';
import { Model } from '../../types/models';
import { routes } from '../../navigation/routes';

type Tab = 'details' | 'comments' | 'makes' | 'remixes' | 'apps';

export const ModelDetails = () => {
  const { id } = useParams<{ id: string }>();
  const [activeTab, setActiveTab] = useState<Tab>('details');
  const [model, setModel] = useState<Model | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isReportDialogOpen, setIsReportDialogOpen] = useState(false);

  useEffect(() => {
    if (id) {
      loadModel();
    }
  }, [id]);

  const loadModel = async () => {
    if (!id) return;
    
    try {
      setIsLoading(true);
      const fetchedModel = await ModelsService.getModelById(id);
      setModel(fetchedModel);
      setError(null);
    } catch (err) {
      setError('Failed to load model');
      console.error('Error loading model:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const renderTabContent = () => {
    if (!model) return null;

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
                <p className="text-green-300/80">{model.author.username}</p>
              </div>
              <div>
                <h3 className="mb-2 text-lg font-semibold text-green-400">Published on</h3>
                <p className="text-green-300/80">{new Date(model.createdAt).toLocaleDateString()}</p>
              </div>
              <div>
                <h3 className="mb-2 text-lg font-semibold text-green-400">License</h3>
                <p className="text-green-300/80">{model.license || 'All rights reserved'}</p>
              </div>
              <div>
                <h3 className="mb-2 text-lg font-semibold text-green-400">Categories</h3>
                <div className="flex flex-wrap gap-2">
                  {model.categories?.map((category: string) => (
                    <span key={category} className="badge">{category}</span>
                  ))}
                </div>
              </div>
            </div>
          </div>
        );
      case 'comments':
        return <CommentSection modelId={id!} />;
      case 'makes':
        return <div className="text-green-300/80">Community makes will appear here.</div>;
      case 'remixes':
        return <div className="text-green-300/80">Remixed versions will appear here.</div>;
      case 'apps':
        return <div className="text-green-300/80">Compatible apps will appear here.</div>;
    }
  };

  if (!id) {
    return <Navigate to={routes.dashboard} replace />;
  }

  if (isLoading) {
    return <div className="text-green-400">Loading model...</div>;
  }

  if (error || !model) {
    return <div className="text-red-500">{error || 'Model not found'}</div>;
  }

  const modelUrl = model?.files[0]?.path || '';

  return (
    <>
      <div className="container px-4 py-8 mx-auto max-w-7xl">
        <div className="mb-8">
          <div className="flex justify-between items-start">
            <div>
              <h1 className="mb-4 text-3xl font-bold text-green-400">{model.name}</h1>
              <div className="flex items-center gap-4 text-green-300/80">
                <span>by {model.author.username}</span>
                <span>•</span>
                <span>{model.likes} likes</span>
                <span>•</span>
                <span>{model.downloads} downloads</span>
              </div>
            </div>
            <button
              onClick={() => setIsReportDialogOpen(true)}
              className="text-green-400/60 hover:text-red-500"
              title="Report model"
            >
              <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 21v-4m0 0V5a2 2 0 012-2h6.5l1 1H21l-3 6 3 6h-8.5l-1-1H5a2 2 0 00-2 2zm9-13.5V9" />
              </svg>
            </button>
          </div>
        </div>

        {/* Model Viewer */}
        <div className="mb-8">
          <ModelViewer 
            modelUrl={modelUrl}
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

      <ReportDialog
        isOpen={isReportDialogOpen}
        onClose={() => setIsReportDialogOpen(false)}
        type={ReportType.Model}
        targetId={model.id}
        targetName={model.name}
      />
    </>
  );
}; 