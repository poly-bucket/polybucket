import React, { useState } from 'react';

interface PDFViewerProps {
  file?: File;
  url?: string;
  width?: number | string;
  height?: number | string;
  className?: string;
  onLoad?: () => void;
  onError?: (error: string) => void;
}

const PDFViewer: React.FC<PDFViewerProps> = ({
  file,
  url,
  width = '100%',
  height = '100%',
  className = '',
  onLoad,
  onError
}) => {
  const [pdfUrl, setPdfUrl] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  React.useEffect(() => {
    const setupPDF = async () => {
      try {
        setLoading(true);
        setError(null);

        if (file) {
          // Create a blob URL from the file
          const blobUrl = URL.createObjectURL(file);
          setPdfUrl(blobUrl);
        } else if (url) {
          setPdfUrl(url);
        } else {
          throw new Error('No file or URL provided');
        }

        onLoad?.();
      } catch (err) {
        const errorMessage = err instanceof Error ? err.message : 'Failed to load PDF';
        setError(errorMessage);
        onError?.(errorMessage);
        console.error('PDF loading error:', err);
      } finally {
        setLoading(false);
      }
    };

    setupPDF();

    // Cleanup function to revoke blob URL when component unmounts
    return () => {
      if (pdfUrl && pdfUrl.startsWith('blob:')) {
        URL.revokeObjectURL(pdfUrl);
      }
    };
  }, [file, url, onLoad, onError]);

  if (loading) {
    return (
      <div 
        className={`flex items-center justify-center bg-gray-800 ${className}`}
        style={{ width, height }}
      >
        <div className="text-center">
          <div className="lg-spinner w-8 h-8 mx-auto mb-2"></div>
          <p className="text-gray-400 text-sm">Loading PDF...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div 
        className={`flex items-center justify-center bg-gray-800 ${className}`}
        style={{ width, height }}
      >
        <div className="text-center">
          <svg className="mx-auto h-12 w-12 text-red-400 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z" />
          </svg>
          <p className="text-red-400 text-sm mb-2">Failed to load PDF</p>
          <p className="text-gray-500 text-xs">{error}</p>
        </div>
      </div>
    );
  }

  if (!pdfUrl) {
    return (
      <div 
        className={`flex items-center justify-center bg-gray-800 ${className}`}
        style={{ width, height }}
      >
        <div className="text-center">
          <p className="text-gray-400 text-sm">No PDF to display</p>
        </div>
      </div>
    );
  }

  return (
    <div 
      className={`bg-white ${className}`}
      style={{ width, height }}
    >
      {/* PDF Viewer using iframe - most reliable for viewing */}
      <iframe
        src={`${pdfUrl}#toolbar=1&navpanes=1&scrollbar=1`}
        width="100%"
        height="100%"
        style={{ border: 'none' }}
        title="PDF Viewer"
        onLoad={() => setLoading(false)}
        onError={() => {
          setError('Failed to load PDF in iframe');
          onError?.('Failed to load PDF in iframe');
        }}
      />
    </div>
  );
};

export default PDFViewer;