import React, { useState, useEffect } from 'react';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';

interface MarkdownViewerProps {
  file?: File;
  content?: string;
  width?: number | string;
  height?: number | string;
  className?: string;
  onLoad?: () => void;
  onError?: (error: string) => void;
}

const MarkdownViewer: React.FC<MarkdownViewerProps> = ({
  file,
  content,
  width = '100%',
  height = '100%',
  className = '',
  onLoad,
  onError
}) => {
  const [markdownContent, setMarkdownContent] = useState<string>('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const loadMarkdown = async () => {
      try {
        setLoading(true);
        setError(null);

        let markdownText: string;
        
        if (file) {
          // Load from File object
          markdownText = await file.text();
        } else if (content) {
          // Use provided content
          markdownText = content;
        } else {
          throw new Error('No file or content provided');
        }

        setMarkdownContent(markdownText);
        onLoad?.();
      } catch (err) {
        const errorMessage = err instanceof Error ? err.message : 'Failed to load markdown';
        setError(errorMessage);
        onError?.(errorMessage);
        console.error('Markdown loading error:', err);
      } finally {
        setLoading(false);
      }
    };

    loadMarkdown();
  }, [file, content, onLoad, onError]);

  if (loading) {
    return (
      <div 
        className={`flex items-center justify-center bg-gray-800 ${className}`}
        style={{ width, height }}
      >
        <div className="text-center">
          <div className="lg-spinner w-8 h-8 mx-auto mb-2"></div>
          <p className="text-gray-400 text-sm">Loading markdown...</p>
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
          <p className="text-red-400 text-sm mb-2">Failed to load markdown</p>
          <p className="text-gray-500 text-xs">{error}</p>
        </div>
      </div>
    );
  }

  return (
    <div 
      className={`bg-gray-900 text-white overflow-auto ${className}`}
      style={{ width, height }}
    >
      <div className="p-6 prose prose-invert prose-lg max-w-none">
        <ReactMarkdown 
          remarkPlugins={[remarkGfm]}
          components={{
            // Custom styling for different elements
            h1: ({ children }) => (
              <h1 className="text-3xl font-bold text-green-400 mb-4 border-b border-gray-700 pb-2">
                {children}
              </h1>
            ),
            h2: ({ children }) => (
              <h2 className="text-2xl font-semibold text-blue-400 mb-3 mt-6">
                {children}
              </h2>
            ),
            h3: ({ children }) => (
              <h3 className="text-xl font-medium text-purple-400 mb-2 mt-4">
                {children}
              </h3>
            ),
            h4: ({ children }) => (
              <h4 className="text-lg font-medium text-yellow-400 mb-2 mt-3">
                {children}
              </h4>
            ),
            p: ({ children }) => (
              <p className="text-gray-300 leading-relaxed mb-4">
                {children}
              </p>
            ),
            ul: ({ children }) => (
              <ul className="list-disc list-inside text-gray-300 mb-4 space-y-1">
                {children}
              </ul>
            ),
            ol: ({ children }) => (
              <ol className="list-decimal list-inside text-gray-300 mb-4 space-y-1">
                {children}
              </ol>
            ),
            li: ({ children }) => (
              <li className="text-gray-300">
                {children}
              </li>
            ),
            blockquote: ({ children }) => (
              <blockquote className="border-l-4 border-green-500 pl-4 italic text-gray-400 bg-gray-800 py-2 mb-4">
                {children}
              </blockquote>
            ),
            code: ({ children, className }) => {
              const isInline = !className;
              if (isInline) {
                return (
                  <code className="bg-gray-800 text-green-400 px-1 py-0.5 rounded text-sm font-mono">
                    {children}
                  </code>
                );
              }
              return (
                <code className={className}>
                  {children}
                </code>
              );
            },
            pre: ({ children }) => (
              <pre className="bg-gray-800 border border-gray-700 rounded-lg p-4 overflow-x-auto mb-4">
                {children}
              </pre>
            ),
            a: ({ href, children }) => (
              <a 
                href={href} 
                className="text-blue-400 hover:text-blue-300 underline"
                target="_blank"
                rel="noopener noreferrer"
              >
                {children}
              </a>
            ),
            strong: ({ children }) => (
              <strong className="font-semibold text-white">
                {children}
              </strong>
            ),
            em: ({ children }) => (
              <em className="italic text-gray-200">
                {children}
              </em>
            ),
            hr: () => (
              <hr className="border-gray-700 my-6" />
            ),
            table: ({ children }) => (
              <div className="overflow-x-auto mb-4">
                <table className="min-w-full border border-gray-700">
                  {children}
                </table>
              </div>
            ),
            th: ({ children }) => (
              <th className="border border-gray-700 px-4 py-2 bg-gray-800 text-left text-sm font-medium text-gray-300">
                {children}
              </th>
            ),
            td: ({ children }) => (
              <td className="border border-gray-700 px-4 py-2 text-sm text-gray-300">
                {children}
              </td>
            ),
            img: ({ src, alt }) => (
              <img 
                src={src} 
                alt={alt} 
                className="max-w-full h-auto rounded-lg border border-gray-700 my-4"
              />
            )
          }}
        >
          {markdownContent}
        </ReactMarkdown>
      </div>
    </div>
  );
};

export default MarkdownViewer;
