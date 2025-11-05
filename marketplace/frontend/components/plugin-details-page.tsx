import { useState, useEffect } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Separator } from '@/components/ui/separator';
import { 
  Download, 
  Star, 
  Shield, 
  Sparkles, 
  Calendar, 
  User, 
  Tag,
  ExternalLink,
  Github,
  GitBranch,
  Package,
  FileText,
  Code,
  AlertTriangle,
  Loader2
} from 'lucide-react';
import Link from 'next/link';
import { PluginSummary, MarketplacePluginDetails } from '@/types/plugin';
import { apiClient } from '@/lib/api';
import { InstallButton } from '@/components/install-button';
import { InstallationModal } from '@/components/installation-modal';
import { RepositoryInfo } from '@/components/repository-info';
import { toast } from 'sonner';

interface PluginDetailsPageProps {
  pluginId: string;
}

export function PluginDetailsPage({ pluginId }: PluginDetailsPageProps) {
  const [plugin, setPlugin] = useState<PluginSummary | null>(null);
  const [pluginDetails, setPluginDetails] = useState<MarketplacePluginDetails | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadPluginData();
  }, [pluginId]);

  const loadPluginData = async () => {
    setIsLoading(true);
    setError(null);
    
    try {
      // Load basic plugin info
      const basicInfo = await apiClient.getPlugin(pluginId);
      setPlugin(basicInfo);

      // Load detailed plugin info
      const details = await apiClient.getPluginDetails(pluginId);
      setPluginDetails(details);
    } catch (err) {
      setError('Failed to load plugin information');
      console.error('Plugin loading error:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
  };

  const formatFileSize = (bytes: number) => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  if (isLoading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="flex items-center justify-center py-12">
          <Loader2 className="h-8 w-8 animate-spin" />
          <span className="ml-2">Loading plugin details...</span>
        </div>
      </div>
    );
  }

  if (error || !plugin) {
    return (
      <div className="container mx-auto px-4 py-8">
        <Card>
          <CardContent className="flex items-center justify-center py-12">
            <div className="text-center">
              <AlertTriangle className="h-12 w-12 text-red-500 mx-auto mb-4" />
              <h2 className="text-xl font-semibold mb-2">Plugin Not Found</h2>
              <p className="text-muted-foreground mb-4">{error}</p>
              <Link href="/">
                <Button>Back to Marketplace</Button>
              </Link>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      {/* Header */}
      <div className="mb-8">
        <div className="flex items-center gap-2 text-sm text-muted-foreground mb-4">
          <Link href="/" className="hover:text-foreground">Marketplace</Link>
          <span>/</span>
          <span>{plugin.category}</span>
          <span>/</span>
          <span className="text-foreground">{plugin.name}</span>
        </div>

        <div className="flex items-start justify-between gap-6">
          <div className="flex-1">
            <div className="flex items-center gap-3 mb-2">
              <h1 className="text-3xl font-bold">{plugin.name}</h1>
              {plugin.isVerified && (
                <Badge variant="secondary" className="gap-1">
                  <Shield className="h-3 w-3" />
                  Verified
                </Badge>
              )}
              {plugin.isFeatured && (
                <Badge variant="default" className="gap-1">
                  <Sparkles className="h-3 w-3" />
                  Featured
                </Badge>
              )}
            </div>
            <p className="text-lg text-muted-foreground mb-4">{plugin.description}</p>
            
            <div className="flex items-center gap-6 text-sm">
              <div className="flex items-center gap-2">
                <User className="h-4 w-4 text-muted-foreground" />
                <span>{plugin.author}</span>
              </div>
              <div className="flex items-center gap-2">
                <Package className="h-4 w-4 text-muted-foreground" />
                <span>v{plugin.version}</span>
              </div>
              <div className="flex items-center gap-2">
                <Calendar className="h-4 w-4 text-muted-foreground" />
                <span>Updated {formatDate(plugin.updatedAt)}</span>
              </div>
            </div>
          </div>

          <div className="flex-shrink-0">
            <InstallationModal plugin={plugin}>
              <Button size="lg" className="gap-2">
                <Download className="h-5 w-5" />
                Install Plugin
              </Button>
            </InstallationModal>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Main Content */}
        <div className="lg:col-span-2 space-y-6">
          {/* Stats */}
          <Card>
            <CardHeader>
              <CardTitle>Plugin Statistics</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                <div className="text-center">
                  <div className="text-2xl font-bold text-primary">{plugin.downloads.toLocaleString()}</div>
                  <div className="text-sm text-muted-foreground">Downloads</div>
                </div>
                <div className="text-center">
                  <div className="text-2xl font-bold text-primary">{plugin.averageRating.toFixed(1)}</div>
                  <div className="text-sm text-muted-foreground">Rating</div>
                </div>
                <div className="text-center">
                  <div className="text-2xl font-bold text-primary">{plugin.reviewCount}</div>
                  <div className="text-sm text-muted-foreground">Reviews</div>
                </div>
                <div className="text-center">
                  <div className="text-2xl font-bold text-primary">{plugin.tags.length}</div>
                  <div className="text-sm text-muted-foreground">Tags</div>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Tags */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Tag className="h-5 w-5" />
                Tags
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="flex flex-wrap gap-2">
                {plugin.tags.map((tag) => (
                  <Badge key={tag} variant="secondary">
                    {tag}
                  </Badge>
                ))}
              </div>
            </CardContent>
          </Card>

          {/* Repository Information */}
          {pluginDetails && (
            <RepositoryInfo 
              repositoryUrl={pluginDetails.repositoryUrl}
              repositoryType={pluginDetails.repositoryType}
            />
          )}
        </div>

        {/* Sidebar */}
        <div className="space-y-6">
          {/* Installation Card */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Download className="h-5 w-5" />
                Installation
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <InstallButton 
                plugin={plugin} 
                variant="default" 
                size="lg"
                className="w-full"
                showRepositoryInfo={true}
              />
              
              <Separator />
              
              <div className="space-y-3 text-sm">
                <div className="flex items-center gap-2 text-muted-foreground">
                  <Github className="h-4 w-4" />
                  <span>Hosted on GitHub</span>
                </div>
                <div className="flex items-center gap-2 text-muted-foreground">
                  <GitBranch className="h-4 w-4" />
                  <span>Direct repository access</span>
                </div>
                <div className="flex items-center gap-2 text-muted-foreground">
                  <Package className="h-4 w-4" />
                  <span>Version {plugin.version}</span>
                </div>
              </div>

              {pluginDetails && (
                <>
                  <Separator />
                  <div className="space-y-2 text-sm">
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Size:</span>
                      <span>{formatFileSize(pluginDetails.size)}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">License:</span>
                      <span>{pluginDetails.license || 'Not specified'}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Last Commit:</span>
                      <span>{pluginDetails.lastCommitAt ? formatDate(pluginDetails.lastCommitAt) : 'Unknown'}</span>
                    </div>
                  </div>
                </>
              )}
            </CardContent>
          </Card>

          {/* Quick Links */}
          {pluginDetails && (
            <Card>
              <CardHeader>
                <CardTitle>Quick Links</CardTitle>
              </CardHeader>
              <CardContent className="space-y-2">
                <Button 
                  variant="outline" 
                  className="w-full justify-start gap-2"
                  onClick={() => window.open(pluginDetails.repositoryUrl, '_blank')}
                >
                  <Github className="h-4 w-4" />
                  View Repository
                </Button>
                {pluginDetails.readmeUrl && (
                  <Button 
                    variant="outline" 
                    className="w-full justify-start gap-2"
                    onClick={() => window.open(pluginDetails.readmeUrl, '_blank')}
                  >
                    <FileText className="h-4 w-4" />
                    Read README
                  </Button>
                )}
                {pluginDetails.documentationUrl && (
                  <Button 
                    variant="outline" 
                    className="w-full justify-start gap-2"
                    onClick={() => window.open(pluginDetails.documentationUrl, '_blank')}
                  >
                    <Code className="h-4 w-4" />
                    Documentation
                  </Button>
                )}
                {pluginDetails.issuesUrl && (
                  <Button 
                    variant="outline" 
                    className="w-full justify-start gap-2"
                    onClick={() => window.open(pluginDetails.issuesUrl, '_blank')}
                  >
                    <AlertTriangle className="h-4 w-4" />
                    Report Issue
                  </Button>
                )}
              </CardContent>
            </Card>
          )}
        </div>
      </div>
    </div>
  );
}
