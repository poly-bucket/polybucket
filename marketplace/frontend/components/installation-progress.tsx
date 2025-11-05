import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Progress } from '@/components/ui/progress';
import { Badge } from '@/components/ui/badge';
import { 
  CheckCircle, 
  AlertCircle, 
  Loader2, 
  Download,
  Package,
  ExternalLink
} from 'lucide-react';
import { InstallationState } from '@/types/plugin';

interface InstallationProgressProps {
  state: InstallationState;
  pluginName: string;
  className?: string;
}

export function InstallationProgress({ state, pluginName, className = '' }: InstallationProgressProps) {
  const getStatusIcon = () => {
    switch (state.status) {
      case 'success':
        return <CheckCircle className="h-5 w-5 text-green-500" />;
      case 'error':
        return <AlertCircle className="h-5 w-5 text-red-500" />;
      case 'downloading':
      case 'installing':
        return <Loader2 className="h-5 w-5 animate-spin text-blue-500" />;
      default:
        return <Package className="h-5 w-5 text-muted-foreground" />;
    }
  };

  const getStatusText = () => {
    switch (state.status) {
      case 'success':
        return 'Installation Complete';
      case 'error':
        return 'Installation Failed';
      case 'downloading':
        return 'Downloading Plugin';
      case 'installing':
        return 'Installing Plugin';
      default:
        return 'Ready to Install';
    }
  };

  const getStatusColor = () => {
    switch (state.status) {
      case 'success':
        return 'bg-green-500';
      case 'error':
        return 'bg-red-500';
      case 'downloading':
      case 'installing':
        return 'bg-blue-500';
      default:
        return 'bg-gray-500';
    }
  };

  const handleDownload = () => {
    if (state.downloadUrl) {
      const link = document.createElement('a');
      link.href = state.downloadUrl;
      link.download = '';
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    }
  };

  return (
    <Card className={className}>
      <CardHeader className="pb-3">
        <CardTitle className="flex items-center gap-2 text-base">
          {getStatusIcon()}
          {getStatusText()}
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="space-y-2">
          <div className="flex items-center justify-between text-sm">
            <span className="font-medium">{pluginName}</span>
            {state.version && (
              <Badge variant="outline">v{state.version}</Badge>
            )}
          </div>
          
          {state.status === 'downloading' || state.status === 'installing' ? (
            <div className="space-y-2">
              <Progress value={state.progress} className="w-full" />
              <div className="text-xs text-muted-foreground text-center">
                {state.progress}% complete
              </div>
            </div>
          ) : null}

          {state.status === 'success' && (
            <div className="space-y-3">
              <div className="text-sm text-green-600 bg-green-50 p-3 rounded-lg">
                Plugin is ready for download from GitHub repository
              </div>
              <div className="flex gap-2">
                <button
                  onClick={handleDownload}
                  className="flex items-center gap-2 px-3 py-2 bg-primary text-primary-foreground rounded-md text-sm hover:bg-primary/90 transition-colors"
                >
                  <Download className="h-4 w-4" />
                  Download Now
                </button>
                {state.downloadUrl && (
                  <button
                    onClick={() => window.open(state.downloadUrl, '_blank')}
                    className="flex items-center gap-2 px-3 py-2 border border-input bg-background hover:bg-accent hover:text-accent-foreground rounded-md text-sm transition-colors"
                  >
                    <ExternalLink className="h-4 w-4" />
                    Open Repository
                  </button>
                )}
              </div>
            </div>
          )}

          {state.status === 'error' && (
            <div className="text-sm text-red-600 bg-red-50 p-3 rounded-lg">
              <div className="font-medium mb-1">Installation Error:</div>
              <div>{state.error}</div>
            </div>
          )}
        </div>

        {state.status === 'idle' && (
          <div className="text-sm text-muted-foreground">
            Click install to begin downloading from the GitHub repository
          </div>
        )}
      </CardContent>
    </Card>
  );
}
