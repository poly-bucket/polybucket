import { useState, useEffect } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { 
  Github, 
  GitBranch, 
  Star, 
  GitFork, 
  Calendar, 
  ExternalLink,
  FileText,
  Code,
  AlertTriangle,
  Loader2,
  Eye
} from 'lucide-react';
import { apiClient } from '@/lib/api';

interface RepositoryInfoProps {
  repositoryUrl: string;
  repositoryType: 'github' | 'gitlab' | 'bitbucket' | 'other';
  className?: string;
}

interface RepositoryData {
  name: string;
  description: string;
  stars: number;
  forks: number;
  lastCommit: string;
  license?: string;
  readme?: string;
  language?: string;
  openIssues?: number;
  watchers?: number;
}

export function RepositoryInfo({ repositoryUrl, repositoryType, className = '' }: RepositoryInfoProps) {
  const [repositoryData, setRepositoryData] = useState<RepositoryData | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadRepositoryData();
  }, [repositoryUrl]);

  const loadRepositoryData = async () => {
    setIsLoading(true);
    setError(null);
    
    try {
      const data = await apiClient.getRepositoryInfo(repositoryUrl);
      setRepositoryData(data);
    } catch (err) {
      setError('Failed to load repository information');
      console.error('Repository data loading error:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const getRepositoryIcon = () => {
    switch (repositoryType) {
      case 'github':
        return <Github className="h-4 w-4" />;
      case 'gitlab':
        return <GitBranch className="h-4 w-4" />;
      case 'bitbucket':
        return <GitBranch className="h-4 w-4" />;
      default:
        return <ExternalLink className="h-4 w-4" />;
    }
  };

  const getRepositoryName = () => {
    if (repositoryData?.name) {
      return repositoryData.name;
    }
    
    // Extract name from URL as fallback
    try {
      const url = new URL(repositoryUrl);
      const pathParts = url.pathname.split('/').filter(Boolean);
      return pathParts.length >= 2 ? pathParts[1] : 'Unknown Repository';
    } catch {
      return 'Unknown Repository';
    }
  };

  const formatDate = (dateString: string) => {
    try {
      return new Date(dateString).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
      });
    } catch {
      return 'Unknown';
    }
  };

  const openRepository = () => {
    window.open(repositoryUrl, '_blank', 'noopener,noreferrer');
  };

  return (
    <Card className={className}>
      <CardHeader className="pb-3">
        <CardTitle className="flex items-center gap-2 text-base">
          {getRepositoryIcon()}
          Repository Information
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        {isLoading ? (
          <div className="flex items-center justify-center py-4">
            <Loader2 className="h-5 w-5 animate-spin" />
            <span className="ml-2 text-sm">Loading repository data...</span>
          </div>
        ) : error ? (
          <div className="flex items-center gap-2 text-sm text-red-600 bg-red-50 p-3 rounded-lg">
            <AlertTriangle className="h-4 w-4" />
            <span>{error}</span>
          </div>
        ) : repositoryData ? (
          <div className="space-y-4">
            {/* Repository Header */}
            <div className="space-y-2">
              <div className="flex items-center justify-between">
                <h3 className="font-semibold">{getRepositoryName()}</h3>
                <Badge variant="outline" className="gap-1">
                  {getRepositoryIcon()}
                  {repositoryType}
                </Badge>
              </div>
              {repositoryData.description && (
                <p className="text-sm text-muted-foreground">{repositoryData.description}</p>
              )}
            </div>

            {/* Repository Stats */}
            <div className="grid grid-cols-2 md:grid-cols-4 gap-3 text-sm">
              <div className="flex items-center gap-2">
                <Star className="h-4 w-4 text-muted-foreground" />
                <span>{repositoryData.stars.toLocaleString()}</span>
              </div>
              <div className="flex items-center gap-2">
                <GitFork className="h-4 w-4 text-muted-foreground" />
                <span>{repositoryData.forks.toLocaleString()}</span>
              </div>
              {repositoryData.watchers && (
                <div className="flex items-center gap-2">
                  <Eye className="h-4 w-4 text-muted-foreground" />
                  <span>{repositoryData.watchers.toLocaleString()}</span>
                </div>
              )}
              {repositoryData.openIssues && (
                <div className="flex items-center gap-2">
                  <AlertTriangle className="h-4 w-4 text-muted-foreground" />
                  <span>{repositoryData.openIssues}</span>
                </div>
              )}
            </div>

            {/* Additional Info */}
            <div className="space-y-2 text-sm">
              <div className="flex items-center justify-between">
                <span className="text-muted-foreground">Last Updated:</span>
                <span>{formatDate(repositoryData.lastCommit)}</span>
              </div>
              {repositoryData.language && (
                <div className="flex items-center justify-between">
                  <span className="text-muted-foreground">Language:</span>
                  <Badge variant="secondary">{repositoryData.language}</Badge>
                </div>
              )}
              {repositoryData.license && (
                <div className="flex items-center justify-between">
                  <span className="text-muted-foreground">License:</span>
                  <span>{repositoryData.license}</span>
                </div>
              )}
            </div>

            {/* Action Buttons */}
            <div className="flex gap-2 pt-2">
              <Button onClick={openRepository} variant="outline" size="sm" className="flex-1">
                <ExternalLink className="h-4 w-4 mr-2" />
                View Repository
              </Button>
              {repositoryData.readme && (
                <Button 
                  onClick={() => window.open(repositoryData.readme, '_blank')} 
                  variant="outline" 
                  size="sm"
                >
                  <FileText className="h-4 w-4" />
                </Button>
              )}
            </div>
          </div>
        ) : (
          <div className="text-sm text-muted-foreground">
            Repository information not available
          </div>
        )}

        {/* Offline Notice */}
        <div className="pt-4 border-t">
          <div className="text-xs text-muted-foreground bg-muted/50 p-2 rounded">
            <div className="flex items-center gap-1 mb-1">
              <GitBranch className="h-3 w-3" />
              <span className="font-medium">Direct Repository Access</span>
            </div>
            <p>
              This plugin is hosted directly on {repositoryType}. You can access it even if the marketplace is unavailable.
            </p>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
