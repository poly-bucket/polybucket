import { useState, useEffect } from 'react';
import { ModelCard } from '../components/model-card/model-card';
import { UploadModal } from '../components/upload-modal/upload-modal';
import { Model, ModelUploadData } from '../types/models';
import { ModelsService } from '../services/models.service';

export const Dashboard = () => {
  const [isUploadModalOpen, setIsUploadModalOpen] = useState(false);
  const [models, setModels] = useState<Model[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  useEffect(() => {
    const fetchModels = async () => {
      try {
        setIsLoading(true);
        setError(null);
        const response = await ModelsService.getModels(page);
        setModels(response.models);
        setTotalPages(response.totalPages);
      } catch (err) {
        setError('Failed to load models. Please try again later.');
        console.error('Error loading models:', err);
      } finally {
        setIsLoading(false);
      }
    };

    fetchModels();
  }, [page]);

  const handleUpload = (modelData: ModelUploadData) => {
    // Handle the upload logic here
    console.log('Uploading model:', modelData);
    // You would typically make an API call here
  };

  const handlePageChange = (newPage: number) => {
    setPage(newPage);
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="w-16 h-16 border-4 border-green-400 rounded-full border-t-transparent animate-spin"></div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex flex-col items-center justify-center min-h-screen">
        <p className="mb-4 text-red-500">{error}</p>
        <button 
          onClick={() => setPage(page)} 
          className="px-4 py-2 text-green-400 border border-green-500 rounded hover:bg-green-500 hover:text-black"
        >
          Try Again
        </button>
      </div>
    );
  }

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

      {models.length === 0 ? (
        <div className="flex flex-col items-center justify-center p-8 text-center">
          <p className="mb-4 text-lg text-green-300/80">No models found</p>
          <button
            onClick={() => setIsUploadModalOpen(true)}
            className="px-4 py-2 text-green-400 border border-green-500 rounded hover:bg-green-500 hover:text-black"
          >
            Upload Your First Model
          </button>
        </div>
      ) : (
        <>
          <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
            {models.map((model) => (
              <ModelCard key={model.id} model={model} />
            ))}
          </div>

          {totalPages > 1 && (
            <div className="flex justify-center gap-2 mt-8">
              <button
                onClick={() => handlePageChange(page - 1)}
                disabled={page === 1}
                className="px-4 py-2 text-green-400 border border-green-500 rounded disabled:opacity-50 disabled:cursor-not-allowed hover:bg-green-500 hover:text-black"
              >
                Previous
              </button>
              <span className="flex items-center px-4 text-green-400">
                Page {page} of {totalPages}
              </span>
              <button
                onClick={() => handlePageChange(page + 1)}
                disabled={page === totalPages}
                className="px-4 py-2 text-green-400 border border-green-500 rounded disabled:opacity-50 disabled:cursor-not-allowed hover:bg-green-500 hover:text-black"
              >
                Next
              </button>
            </div>
          )}
        </>
      )}

      <UploadModal
        isOpen={isUploadModalOpen}
        onClose={() => setIsUploadModalOpen(false)}
        onUpload={handleUpload}
      />
    </div>
  );
};
