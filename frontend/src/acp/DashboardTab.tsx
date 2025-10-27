import React, { useState, useEffect, useCallback } from 'react';
import { 
  People as PeopleIcon, 
  ViewInAr as ModelIcon, 
  Storage as StorageIcon, 
  FileCopy as FileIcon, 
  TrendingUp as TrendingIcon,
  Category as CategoryIcon,
  Visibility as VisibilityIcon,
  VisibilityOff as VisibilityOffIcon,
  CloudUpload as UploadIcon
} from '@mui/icons-material';
import { GetAdminModelStatisticsResponse } from '../api';
import { useAdminModelStatistics } from '../services/adminModelStatisticsService';

const DashboardTab: React.FC = () => {
  const [statistics, setStatistics] = useState<GetAdminModelStatisticsResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const { getAdminModelStatistics } = useAdminModelStatistics();

  const fetchStatistics = useCallback(async () => {
    try {
      setLoading(true);
      const stats = await getAdminModelStatistics();
      setStatistics(stats);
    } catch (err) {
      setError('Failed to load model statistics');
      console.error('Error loading statistics:', err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchStatistics();
  }, [fetchStatistics]);

  const formatDate = (date?: Date) => {
    if (!date) return 'Never';
    return new Date(date).toLocaleDateString();
  };

  if (loading) {
    return (
      <div className="space-y-6">
        <h2 className="text-2xl font-bold text-white">Analytics Dashboard</h2>
        <div className="text-center text-white/60 py-8">Loading analytics...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="space-y-6">
        <h2 className="text-2xl font-bold text-white">Analytics Dashboard</h2>
        <div className="lg-card p-6">
          <div className="text-center text-red-400">
            <p className="text-lg font-medium">{error}</p>
            <button 
              onClick={() => window.location.reload()} 
              className="mt-2 text-sm text-blue-400 hover:text-blue-300 underline"
            >
              Retry
            </button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold text-white">Analytics Dashboard</h2>
      
      {/* Model Statistics Overview */}
      <div className="lg-card p-6">
        <h3 className="text-lg font-medium text-white mb-4">Model Statistics Overview</h3>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          <div className="lg-card p-4">
            <div className="flex items-center space-x-2 mb-2">
              <FileIcon className="w-5 h-5 text-blue-400" />
              <div className="text-2xl font-bold text-blue-400">{statistics?.totalModels?.toLocaleString() || 0}</div>
            </div>
            <div className="text-sm text-white/60">Total Models</div>
          </div>
          
          <div className="lg-card p-4">
            <div className="flex items-center space-x-2 mb-2">
              <StorageIcon className="w-5 h-5 text-green-400" />
              <div className="text-2xl font-bold text-green-400">{statistics?.totalFileSizeFormatted || '0 B'}</div>
            </div>
            <div className="text-sm text-white/60">Total Storage Used</div>
          </div>
          
          <div className="lg-card p-4">
            <div className="flex items-center space-x-2 mb-2">
              <FileIcon className="w-5 h-5 text-purple-400" />
              <div className="text-2xl font-bold text-purple-400">{statistics?.totalFiles?.toLocaleString() || 0}</div>
            </div>
            <div className="text-sm text-white/60">Total Files</div>
          </div>
          
          <div className="lg-card p-4">
            <div className="flex items-center space-x-2 mb-2">
              <TrendingIcon className="w-5 h-5 text-orange-400" />
              <div className="text-2xl font-bold text-orange-400">{statistics?.averageFilesPerModel?.toFixed(1) || 0}</div>
            </div>
            <div className="text-sm text-white/60">Avg Files/Model</div>
          </div>
        </div>
      </div>

      {/* Model Distribution and Recent Activity */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="lg-card p-6">
          <h3 className="text-lg font-medium text-white mb-4">Model Distribution</h3>
          <div className="space-y-3">
            <div className="flex justify-between items-center p-3 bg-white/5 rounded-lg">
              <div className="flex items-center space-x-2">
                <VisibilityIcon className="w-4 h-4 text-green-400" />
                <span className="text-white">Public Models</span>
              </div>
              <span className="text-lg font-semibold text-green-400">{statistics?.publicModels || 0}</span>
            </div>
            
            <div className="flex justify-between items-center p-3 bg-white/5 rounded-lg">
              <div className="flex items-center space-x-2">
                <VisibilityOffIcon className="w-4 h-4 text-blue-400" />
                <span className="text-white">Private Models</span>
              </div>
              <span className="text-lg font-semibold text-blue-400">{statistics?.privateModels || 0}</span>
            </div>
            
            <div className="flex justify-between items-center p-3 bg-white/5 rounded-lg">
              <div className="flex items-center space-x-2">
                <CategoryIcon className="w-4 h-4 text-yellow-400" />
                <span className="text-white">Unlisted Models</span>
              </div>
              <span className="text-lg font-semibold text-yellow-400">{statistics?.unlistedModels || 0}</span>
            </div>
            
            <div className="flex justify-between items-center p-3 bg-white/5 rounded-lg">
              <div className="flex items-center space-x-2">
                <UploadIcon className="w-4 h-4 text-red-400" />
                <span className="text-white">AI Generated</span>
              </div>
              <span className="text-lg font-semibold text-red-400">{statistics?.aIGeneratedModels || 0}</span>
            </div>
          </div>
        </div>

        <div className="lg-card p-6">
          <h3 className="text-lg font-medium text-white mb-4">Recent Activity</h3>
          <div className="space-y-3">
            <div className="flex items-center justify-between p-3 bg-white/5 rounded-lg">
              <div>
                <div className="text-white font-medium">Last Model Uploaded</div>
                <div className="text-sm text-white/60">{formatDate(statistics?.lastModelUploaded)}</div>
              </div>
              <span className="text-sm text-white/40">Upload</span>
            </div>
            
            <div className="flex items-center justify-between p-3 bg-white/5 rounded-lg">
              <div>
                <div className="text-white font-medium">Last Model Updated</div>
                <div className="text-sm text-white/60">{formatDate(statistics?.lastModelUpdated)}</div>
              </div>
              <span className="text-sm text-white/40">Update</span>
            </div>
            
            <div className="flex items-center justify-between p-3 bg-white/5 rounded-lg">
              <div>
                <div className="text-white font-medium">Average File Size</div>
                <div className="text-sm text-white/60">{statistics?.averageFileSizeMB?.toFixed(2) || 0} MB</div>
              </div>
              <span className="text-sm text-white/40">Size</span>
            </div>
          </div>
        </div>
      </div>

      {/* Top Uploaders */}
      {statistics?.topUploaders && statistics.topUploaders.length > 0 && (
        <div className="lg-card p-6">
          <h3 className="text-lg font-medium text-white mb-4">Top Uploaders</h3>
          <div className="space-y-3">
            {statistics.topUploaders.slice(0, 5).map((uploader, index) => (
              <div key={uploader.userId} className="flex items-center justify-between p-3 bg-white/5 rounded-lg">
                <div className="flex items-center space-x-3">
                  <div className="w-6 h-6 bg-indigo-500 rounded-full flex items-center justify-center text-white text-sm font-bold">
                    {index + 1}
                  </div>
                  <div>
                    <div className="text-white font-medium">{uploader.username}</div>
                    <div className="text-sm text-white/60">{uploader.modelCount} models</div>
                  </div>
                </div>
                <span className="text-sm text-white/40">{uploader.totalFileSizeFormatted}</span>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* File Type Distribution */}
      {statistics?.fileTypeDistribution && statistics.fileTypeDistribution.length > 0 && (
        <div className="lg-card p-6">
          <h3 className="text-lg font-medium text-white mb-4">File Type Distribution</h3>
          <div className="space-y-3">
            {statistics.fileTypeDistribution.slice(0, 8).map((fileType, index) => (
              <div key={`filetype-${index}-${fileType.fileExtension || 'unknown'}`} className="flex items-center justify-between p-3 bg-white/5 rounded-lg">
                <div className="flex items-center space-x-3">
                  <div className="w-6 h-6 bg-purple-500 rounded-full flex items-center justify-center text-white text-sm font-bold">
                    {index + 1}
                  </div>
                  <div>
                    <div className="text-white font-medium">{fileType.fileExtension?.toUpperCase() || 'Unknown'}</div>
                    <div className="text-sm text-white/60">{fileType.count} files</div>
                  </div>
                </div>
                <div className="text-right">
                  <div className="text-sm text-white/40">{fileType.totalSizeFormatted}</div>
                  <div className="text-xs text-white/30">{fileType.percentage?.toFixed(1)}%</div>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Future Analytics Placeholder */}
      <div className="lg-card p-6">
        <h3 className="text-lg font-medium text-white mb-4">Future Analytics</h3>
        <div className="lg-badge-info p-4">
          <span className="text-lg">This analytics dashboard will be expanded to include:</span>
          <ul className="mt-2 text-sm space-y-1">
            <li>• User activity charts and trends</li>
            <li>• Model popularity and download analytics</li>
            <li>• System performance metrics</li>
            <li>• Storage usage trends over time</li>
            <li>• Geographic distribution of users</li>
            <li>• Peak usage time analysis</li>
          </ul>
        </div>
      </div>
    </div>
  );
};

export default DashboardTab;
