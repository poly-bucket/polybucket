'use client';

import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { 
  Download, 
  ExternalLink, 
  CheckCircle, 
  AlertCircle, 
  Loader2,
  Github,
  GitBranch,
  Package
} from 'lucide-react';
import { usePluginInstallation } from '@/hooks/usePluginInstallation';
import { PluginSummary } from '@/types/plugin';
import { toast } from 'sonner';

interface InstallButtonProps {
  plugin: PluginSummary;
  variant?: 'default' | 'outline' | 'secondary' | 'ghost' | 'link' | 'destructive';
  size?: 'default' | 'sm' | 'lg' | 'icon';
  className?: string;
  showRepositoryInfo?: boolean;
}

export function InstallButton({ 
  plugin, 
  variant = 'default', 
  size = 'default',
  className = '',
  showRepositoryInfo = true 
}: InstallButtonProps) {
  const [showDetails, setShowDetails] = useState(false);
  
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
        action: {
          label: 'Download',
          onClick: () => downloadDirectly(downloadUrl),
        },
      });
    },
    onError: (error) => {
      toast.error('Installation failed', {
        description: error,
      });
    },
  });

  const handleInstall = async () => {
    await installPlugin(plugin.id);
  };

  const handleDirectDownload = async () => {
    if (state.downloadUrl) {
      const success = await downloadDirectly(state.downloadUrl);
      if (success) {
        toast.success('Download started');
      } else {
        toast.error('Download failed');
      }
    }
  };

  const getButtonContent = () => {
    if (isInstalling) {
      return (
        <>
          <Loader2 className="h-4 w-4 animate-spin" />
          Installing... {state.progress}%
        </>
      );
    }

    if (isSuccess) {
      return (
        <>
          <CheckCircle className="h-4 w-4" />
          Ready to Download
        </>
      );
    }

    if (isError) {
      return (
        <>
          <AlertCircle className="h-4 w-4" />
          Installation Failed
        </>
      );
    }

    return (
      <>
        <Download className="h-4 w-4" />
        Install Plugin
      </>
    );
  };

  const getButtonVariant = () => {
    if (isSuccess) return 'default';
    if (isError) return 'destructive';
    return variant;
  };

  return (
    <div className="space-y-2">
      <div className="flex items-center gap-2">
        <Button
          onClick={handleInstall}
          variant={getButtonVariant()}
          size={size}
          className={className}
          disabled={isInstalling}
        >
          {getButtonContent()}
        </Button>

        {isSuccess && (
          <Button
            onClick={handleDirectDownload}
            variant="outline"
            size={size}
          >
            <Download className="h-4 w-4 mr-2" />
            Download Now
          </Button>
        )}

        {isError && (
          <Button
            onClick={reset}
            variant="outline"
            size={size}
          >
            Try Again
          </Button>
        )}

        {showRepositoryInfo && (
          <Button
            onClick={() => setShowDetails(!showDetails)}
            variant="ghost"
            size={size}
          >
            <ExternalLink className="h-4 w-4" />
          </Button>
        )}
      </div>

      {showDetails && (
        <div className="p-3 border rounded-lg bg-muted/50 space-y-2">
          <div className="flex items-center gap-2 text-sm">
            <Package className="h-4 w-4" />
            <span className="font-medium">Installation Info</span>
          </div>
          
          <div className="grid grid-cols-2 gap-2 text-xs">
            <div>
              <span className="text-muted-foreground">Version:</span>
              <Badge variant="secondary" className="ml-1">
                v{plugin.version}
              </Badge>
            </div>
            <div>
              <span className="text-muted-foreground">Downloads:</span>
              <span className="ml-1 font-medium">{plugin.downloads.toLocaleString()}</span>
            </div>
            <div>
              <span className="text-muted-foreground">Rating:</span>
              <span className="ml-1 font-medium">{plugin.averageRating.toFixed(1)}</span>
            </div>
            <div>
              <span className="text-muted-foreground">Reviews:</span>
              <span className="ml-1 font-medium">{plugin.reviewCount}</span>
            </div>
          </div>

          <div className="pt-2 border-t">
            <div className="flex items-center gap-2 text-xs text-muted-foreground">
              <Github className="h-3 w-3" />
              <span>This plugin is hosted on GitHub</span>
            </div>
            <div className="flex items-center gap-2 text-xs text-muted-foreground mt-1">
              <GitBranch className="h-3 w-3" />
              <span>Downloaded directly from repository</span>
            </div>
          </div>

          <div className="pt-2">
            <Button
              onClick={() => openRepository(`https://github.com/${plugin.author}/${plugin.name}`)}
              variant="outline"
              size="sm"
              className="w-full"
            >
              <Github className="h-3 w-3 mr-2" />
              View Repository
            </Button>
          </div>
        </div>
      )}

      {isInstalling && (
        <div className="w-full bg-secondary rounded-full h-2">
          <div 
            className="bg-primary h-2 rounded-full transition-all duration-300"
            style={{ width: `${state.progress}%` }}
          />
        </div>
      )}
    </div>
  );
}
