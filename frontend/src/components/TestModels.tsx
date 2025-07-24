import React, { useEffect, useState } from 'react';
import { ApiClientFactory } from '../api/clientFactory';

const TestModels: React.FC = () => {
  const [models, setModels] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string>('');

  useEffect(() => {
    const testApi = async () => {
      try {
        setLoading(true);
        setError('');
        
        console.log('Testing API call...');
        const client = ApiClientFactory.getModelsClient();
        const response = await client.getModels(1, 5);
        
        console.log('API Response:', response);
        setModels(response.models || []);
      } catch (err) {
        console.error('API Error:', err);
        setError(err instanceof Error ? err.message : 'Unknown error');
      } finally {
        setLoading(false);
      }
    };

    testApi();
  }, []);

  if (loading) {
    return <div>Loading...</div>;
  }

  if (error) {
    return <div>Error: {error}</div>;
  }

  return (
    <div>
      <h1>Test Models API</h1>
      <p>Found {models.length} models</p>
      {models.map((model) => (
        <div key={model.id} style={{ border: '1px solid #ccc', margin: '10px', padding: '10px' }}>
          <h3>{model.name}</h3>
          <p>ID: {model.id}</p>
          <p>Thumbnail URL: {model.thumbnailUrl || 'No thumbnail'}</p>
          {model.thumbnailUrl && (
            <img 
              src={model.thumbnailUrl} 
              alt={model.name}
              style={{ maxWidth: '200px', maxHeight: '200px' }}
              onError={(e) => {
                console.error('Image failed to load:', model.thumbnailUrl);
                (e.target as HTMLImageElement).style.display = 'none';
              }}
            />
          )}
        </div>
      ))}
    </div>
  );
};

export default TestModels; 