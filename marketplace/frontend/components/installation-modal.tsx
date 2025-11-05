import { useState, useEffect } from 'react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Progress } from '@/components/ui/progress';
import { 
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog';
import { 
  Download, 
  ExternalLink, 
  CheckCircle, 
  AlertCircle, 
  Loader2,
  Github,
  GitBranch,
  Package,
  Shield,
  Star,
  Calendar,
  FileText,
  Code,
  Users
} from 'lucide-react';
import { usePluginInstallation } from '@/hooks/usePluginInstallation';
import { PluginSummary, MarketplacePluginDetails } from '@/types/plugin';
import { apiClient } from '@/lib/api';
import { toast } from 'sonner';

interface InstallationModalProps {
  plugin: PluginSummary;
  trigger?: React.ReactNode;
  children?: React.ReactNode;
}

export function InstallationModal({ plugin, trigger, children }: InstallationModalProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [pluginDetails, setPluginDetails] = useState<MarketplacePluginDetails | null>(null);
  const [isLoadingDetails, setIsLoadingDetails] = useState(false);
  const [selectedVersion, setSelectedVersion] = useState<string>(plugin.version);

  const {
    state,
    installPlugin,
    reset,
    downloadDirectly,
    openRepository,
    isInstalling,
    isSuccess,
    isError,
  } = usePluginInstallation({
    onSuccess: (downloadUrl, version) => {
      toast.success(`Plugin "${plugin.name}" ready for download!`, {
        description: `Version ${version} is ready to install`,
      });
    },
    onError: (error) => {
      toast.error('Installation failed', {
        description: error,
      });
    },
  });

  useEffect(() => {
    if (isOpen && !pluginDetails) {
      loadPluginDetails();
    }
  }, [isOpen, pluginDetails]);

  const loadPluginDetails = async () => {
    setIsLoadingDetails(true);
    try {
      const details = await apiClient.getPluginDetails(plugin.id);
      setPluginDetails(details);
    } catch (error) {
      console.error('Failed to load plugin details:', error);
      toast.error('Failed to load plugin details');
    } finally {
      setIsLoadingDetails(false);
    }
  };

  const handleInstall = async () => {
    await installPlugin(plugin.id, selectedVersion);
  };

  const handleDirectDownload = async () => {
    if (state.downloadUrl) {
      const success = await downloadDirectly(state.downloadUrl);
      if (success) {
        toast.success('Download started');
        setIsOpen(false);
      } else {
        toast.error('Download failed');
      }
    }
  };

  const handleClose = () => {
    setIsOpen(false);
    reset();
  };

  const formatFileSize = (bytes: number) => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  };

  return (
    <Dialog open={isOpen} onOpenChange={setIsOpen}>
      <DialogTrigger asChild>
        {trigger || (
          <Button variant="default">
            <Download className="h-4 w-4 mr-2" />
            Install Plugin
          </Button>
        )}
      </DialogTrigger>
      
      <DialogContent className="max-w-2xl max-h-[80vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Package className="h-5 w-5" />
            Install {plugin.name}
          </DialogTitle>
          <DialogDescription>
            Download and install this plugin directly from its GitHub repository
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-6">
          {/* Plugin Info */}
          <Card>
            <CardHeader className="pb-3">
              <div className="flex items-start justify-between">
                <div className="flex-1">
                  <CardTitle className="text-lg">{plugin.name}</CardTitle>
                  <p className="text-sm text-muted-foreground mt-1">{plugin.description}</p>
                </div>
                <div className="flex items-center gap-2">
                  {plugin.isVerified && (
                    <Badge variant="secondary" className="gap-1">
                      <Shield className="h-3 w-3" />
                      Verified
                    </Badge>
                  )}
                  {plugin.isFeatured && (
                    <Badge variant="default" className="gap-1">
                      <Star className="h-3 w-3" />
                      Featured
                    </Badge>
                  )}
                </div>
              </div>
            </CardHeader>
            <CardContent className="pt-0">
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
                <div className="flex items-center gap-2">
                  <Download className="h-4 w-4 text-muted-foreground" />
                  <span>{plugin.downloads.toLocaleString()}</span>
                </div>
                <div className="flex items-center gap-2">
                  <Star className="h-4 w-4 text-muted-foreground" />
                  <span>{plugin.averageRating.toFixed(1)}</span>
                </div>
                <div className="flex items-center gap-2">
                  <Users className="h-4 w-4 text-muted-foreground" />
                  <span>{plugin.reviewCount} reviews</span>
                </div>
                <div className="flex items-center gap-2">
                  <Calendar className="h-4 w-4 text-muted-foreground" />
                  <span>{formatDate(plugin.updatedAt)}</span>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Installation Details */}
          {isLoadingDetails ? (
            <Card>
              <CardContent className="flex items-center justify-center py-8">
                <Loader2 className="h-6 w-6 animate-spin" />
                <span className="ml-2">Loading installation details...</span>
              </CardContent>
            </Card>
          ) : pluginDetails ? (
            <Card>
              <CardHeader>
                <CardTitle className="text-base">Installation Details</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid grid-cols-2 gap-4 text-sm">
                  <div>
                    <span className="text-muted-foreground">Version:</span>
                    <Badge variant="outline" className="ml-2">
                      v{pluginDetails.version}
                    </Badge>
                  </div>
                  <div>
                    <span className="text-muted-foreground">Size:</span>
                    <span className="ml-2 font-medium">{formatFileSize(pluginDetails.size)}</span>
                  </div>
                  <div>
                    <span className="text-muted-foreground">Repository:</span>
                    <span className="ml-2 font-medium">{pluginDetails.repositoryType}</span>
                  </div>
                  <div>
                    <span className="text-muted-foreground">License:</span>
                    <span className="ml-2 font-medium">{pluginDetails.license || 'Not specified'}</span>
                  </div>
                </div>

                <div className="pt-4 border-t">
                  <div className="flex items-center gap-2 text-sm text-muted-foreground mb-2">
                    <Github className="h-4 w-4" />
                    <span>Repository Information</span>
                  </div>
                  <div className="space-y-2">
                    <div className="flex items-center justify-between">
                      <span className="text-sm">Repository URL:</span>
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => openRepository(pluginDetails.repositoryUrl)}
                        className="h-auto p-1"
                      >
                        <ExternalLink className="h-3 w-3" />
                      </Button>
                    </div>
                    {pluginDetails.readmeUrl && (
                      <div className="flex items-center justify-between">
                        <span className="text-sm">README:</span>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => window.open(pluginDetails.readmeUrl, '_blank')}
                          className="h-auto p-1"
                        >
                          <FileText className="h-3 w-3" />
                        </Button>
                      </div>
                    )}
                    {pluginDetails.documentationUrl && (
                      <div className="flex items-center justify-between">
                        <span className="text-sm">Documentation:</span>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => window.open(pluginDetails.documentationUrl, '_blank')}
                          className="h-auto p-1"
                        >
                          <Code className="h-3 w-3" />
                        </Button>
                      </div>
                    )}
                  </div>
                </div>
              </CardContent>
            </Card>
          ) : null}

          {/* Installation Progress */}
          {isInstalling && (
            <Card>
              <CardContent className="pt-6">
                <div className="space-y-3">
                  <div className="flex items-center gap-2">
                    <Loader2 className="h-4 w-4 animate-spin" />
                    <span className="font-medium">Installing plugin...</span>
                  </div>
                  <Progress value={state.progress} className="w-full" />
                  <div className="text-sm text-muted-foreground text-center">
                    {state.progress}% complete
                  </div>
                </div>
              </CardContent>
            </Card>
          )}

          {/* Success State */}
          {isSuccess && (
            <Card>
              <CardContent className="pt-6">
                <div className="text-center space-y-4">
                  <CheckCircle className="h-12 w-12 text-green-500 mx-auto" />
                  <div>
                    <h3 className="font-semibold">Installation Complete!</h3>
                    <p className="text-sm text-muted-foreground">
                      Plugin is ready for download
                    </p>
                  </div>
                  <div className="flex gap-2 justify-center">
                    <Button onClick={handleDirectDownload}>
                      <Download className="h-4 w-4 mr-2" />
                      Download Now
                    </Button>
                    <Button variant="outline" onClick={handleClose}>
                      Close
                    </Button>
                  </div>
                </div>
              </CardContent>
            </Card>
          )}

          {/* Error State */}
          {isError && (
            <Card>
              <CardContent className="pt-6">
                <div className="text-center space-y-4">
                  <AlertCircle className="h-12 w-12 text-red-500 mx-auto" />
                  <div>
                    <h3 className="font-semibold">Installation Failed</h3>
                    <p className="text-sm text-muted-foreground">
                      {state.error}
                    </p>
                  </div>
                  <div className="flex gap-2 justify-center">
                    <Button onClick={() => installPlugin(plugin.id, selectedVersion)}>
                      Try Again
                    </Button>
                    <Button variant="outline" onClick={handleClose}>
                      Close
                    </Button>
                  </div>
                </div>
              </CardContent>
            </Card>
          )}

          {/* Action Buttons */}
          {!isInstalling && !isSuccess && !isError && (
            <div className="flex justify-end gap-2">
              <Button variant="outline" onClick={handleClose}>
                Cancel
              </Button>
              <Button onClick={handleInstall}>
                <Download className="h-4 w-4 mr-2" />
                Install Plugin
              </Button>
            </div>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
}
