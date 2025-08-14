import React, { useState } from 'react';
import { ApiClientFactory } from '../api/clientFactory';

const ZipDownloadTest: React.FC = () => {
  const [testResults, setTestResults] = useState<string[]>([]);
  const [isTesting, setIsTesting] = useState(false);

  const addResult = (message: string) => {
    setTestResults(prev => [...prev, `${new Date().toISOString()}: ${message}`]);
  };

  const testZipDownload = async () => {
    setIsTesting(true);
    setTestResults([]);
    
    try {
      addResult('Starting ZIP download test...');
      
      // Test with a known model ID (you can change this)
      const testModelId = '1'; // Assuming model ID 1 exists
      addResult(`Testing download for model ID: ${testModelId}`);
      
      const client = ApiClientFactory.getDownloadModelClient();
      addResult('Download client created successfully');
      
      addResult('Calling downloadModel...');
      const response = await client.downloadModel(testModelId);
      addResult(`Download response received: ${response ? 'SUCCESS' : 'FAILED'}`);
      
      if (response && response.data) {
        addResult(`Response data type: ${typeof response.data}`);
        addResult(`Response data constructor: ${response.data.constructor.name}`);
        
        if (response.data instanceof Blob) {
          addResult(`Blob size: ${response.data.size} bytes`);
          addResult(`Blob type: ${response.data.type}`);
          
          // Test ZIP file integrity
          try {
            const arrayBuffer = await response.data.slice(0, 4).arrayBuffer();
            const uint8Array = new Uint8Array(arrayBuffer);
            const header = Array.from(uint8Array).map(b => b.toString(16).padStart(2, '0')).join(' ');
            addResult(`ZIP header bytes: ${header}`);
            
            // Check for ZIP file signature (PK\x03\x04)
            if (uint8Array[0] === 0x50 && uint8Array[1] === 0x4B && uint8Array[2] === 0x03 && uint8Array[3] === 0x04) {
              addResult('ZIP file signature verified successfully');
            } else {
              addResult('ZIP file signature verification failed');
            }
            
            // Test if we can read more of the file
            try {
              const testArrayBuffer = await response.data.slice(0, 100).arrayBuffer();
              addResult(`Successfully read first 100 bytes of ZIP file`);
            } catch (error) {
              addResult(`Failed to read ZIP file content: ${error}`);
            }
            
          } catch (error) {
            addResult(`Failed to verify ZIP file integrity: ${error}`);
          }
          
          // Test download
          try {
            const url = window.URL.createObjectURL(response.data);
            const link = document.createElement('a');
            link.href = url;
            link.download = response.fileName || `test-model-${testModelId}.zip`;
            
            addResult('Creating download link...');
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            
            addResult('Download link clicked successfully');
            
            // Clean up
            setTimeout(() => {
              window.URL.revokeObjectURL(url);
              addResult('Blob URL revoked');
            }, 5000);
            
          } catch (error) {
            addResult(`Failed to create download: ${error}`);
          }
          
        } else {
          addResult(`Response data is not a Blob: ${response.data}`);
        }
      } else {
        addResult('No response data received');
      }
      
    } catch (error) {
      addResult(`Test failed with error: ${error}`);
      console.error('ZIP download test error:', error);
    } finally {
      setIsTesting(false);
      addResult('Test completed');
    }
  };

  const clearResults = () => {
    setTestResults([]);
  };

  return (
    <div className="p-4 border rounded-lg bg-gray-50">
      <h3 className="text-lg font-semibold mb-4">ZIP Download Test</h3>
      
      <div className="mb-4">
        <button
          onClick={testZipDownload}
          disabled={isTesting}
          className="px-4 py-2 bg-blue-500 text-white rounded disabled:bg-gray-400"
        >
          {isTesting ? 'Testing...' : 'Test ZIP Download'}
        </button>
        
        <button
          onClick={clearResults}
          className="ml-2 px-4 py-2 bg-gray-500 text-white rounded"
        >
          Clear Results
        </button>
      </div>
      
      <div className="bg-white border rounded p-4 max-h-96 overflow-y-auto">
        <h4 className="font-medium mb-2">Test Results:</h4>
        {testResults.length === 0 ? (
          <p className="text-gray-500">No test results yet. Click "Test ZIP Download" to start.</p>
        ) : (
          <div className="space-y-1">
            {testResults.map((result, index) => (
              <div key={index} className="text-sm font-mono bg-gray-100 p-2 rounded">
                {result}
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default ZipDownloadTest;
