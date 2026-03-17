"use client";

import {
  FileIcon,
  HardDrive,
  TrendingUp,
  Eye,
  EyeOff,
  List,
  CloudUpload,
} from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/primitives/card";
import { StatCard } from "@/components/admin/stat-card";
import { Skeleton } from "@/components/ui/skeleton";
import { Button } from "@/components/primitives/button";
import { useAdminQuery } from "@/lib/hooks/use-admin-query";
import { formatDate } from "@/lib/utils/format";
import { getAdminModelStatistics } from "@/lib/services/adminService";
import type { GetAdminModelStatisticsResponse } from "@/lib/api/client";

function DashboardSkeleton() {
  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold text-white">Analytics Dashboard</h2>
      <Card variant="glass" className="border-white/20">
        <CardHeader>
          <CardTitle className="text-white">Model Statistics Overview</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            {[1, 2, 3, 4].map((i) => (
              <div
                key={i}
                className="rounded-lg border border-white/10 p-4 flex flex-col gap-2"
              >
                <div className="flex items-center space-x-2">
                  <Skeleton className="h-5 w-5 rounded" />
                  <Skeleton className="h-8 w-16" />
                </div>
                <Skeleton className="h-4 w-24" />
              </div>
            ))}
          </div>
        </CardContent>
      </Card>
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <Card variant="glass" className="border-white/20">
          <CardHeader>
            <Skeleton className="h-6 w-40" />
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {[1, 2, 3, 4].map((i) => (
                <Skeleton key={i} className="h-12 w-full rounded-lg" />
              ))}
            </div>
          </CardContent>
        </Card>
        <Card variant="glass" className="border-white/20">
          <CardHeader>
            <Skeleton className="h-6 w-32" />
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {[1, 2, 3].map((i) => (
                <Skeleton key={i} className="h-12 w-full rounded-lg" />
              ))}
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

export function DashboardTab() {
  const { data: statistics, isLoading, error, refetch } = useAdminQuery(
    () => getAdminModelStatistics(),
    []
  );

  if (isLoading) {
    return <DashboardSkeleton />;
  }

  if (error) {
    return (
      <div className="space-y-6">
        <h2 className="text-2xl font-bold text-white">Analytics Dashboard</h2>
        <Card variant="glass" className="border-white/20">
          <CardContent className="py-6">
            <div className="text-center text-red-400">
              <p className="text-lg font-medium">{error}</p>
              <Button
                variant="outline"
                onClick={() => refetch()}
                className="mt-2"
              >
                Retry
              </Button>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold text-white">Analytics Dashboard</h2>

      <Card variant="glass" className="border-white/20">
        <CardHeader>
          <CardTitle className="text-white">Model Statistics Overview</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            <StatCard
              icon={FileIcon}
              value={statistics?.totalModels?.toLocaleString() ?? 0}
              label="Total Models"
              colorClass="text-blue-400"
            />
            <StatCard
              icon={HardDrive}
              value={statistics?.totalFileSizeFormatted ?? "0 B"}
              label="Total Storage Used"
              colorClass="text-green-400"
            />
            <StatCard
              icon={FileIcon}
              value={statistics?.totalFiles?.toLocaleString() ?? 0}
              label="Total Files"
              colorClass="text-purple-400"
            />
            <StatCard
              icon={TrendingUp}
              value={statistics?.averageFilesPerModel?.toFixed(1) ?? "0"}
              label="Avg Files/Model"
              colorClass="text-orange-400"
            />
          </div>
        </CardContent>
      </Card>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <Card variant="glass" className="border-white/20">
          <CardHeader>
            <CardTitle className="text-white">Model Distribution</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              <div className="flex justify-between items-center p-3 rounded-lg bg-white/5">
                <div className="flex items-center space-x-2">
                  <Eye className="h-4 w-4 text-green-400" />
                  <span className="text-white">Public Models</span>
                </div>
                <span className="text-lg font-semibold text-green-400">
                  {statistics?.publicModels ?? 0}
                </span>
              </div>
              <div className="flex justify-between items-center p-3 rounded-lg bg-white/5">
                <div className="flex items-center space-x-2">
                  <EyeOff className="h-4 w-4 text-blue-400" />
                  <span className="text-white">Private Models</span>
                </div>
                <span className="text-lg font-semibold text-blue-400">
                  {statistics?.privateModels ?? 0}
                </span>
              </div>
              <div className="flex justify-between items-center p-3 rounded-lg bg-white/5">
                <div className="flex items-center space-x-2">
                  <List className="h-4 w-4 text-yellow-400" />
                  <span className="text-white">Unlisted Models</span>
                </div>
                <span className="text-lg font-semibold text-yellow-400">
                  {statistics?.unlistedModels ?? 0}
                </span>
              </div>
              <div className="flex justify-between items-center p-3 rounded-lg bg-white/5">
                <div className="flex items-center space-x-2">
                  <CloudUpload className="h-4 w-4 text-red-400" />
                  <span className="text-white">AI Generated</span>
                </div>
                <span className="text-lg font-semibold text-red-400">
                  {statistics?.aiGeneratedModels ?? 0}
                </span>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card variant="glass" className="border-white/20">
          <CardHeader>
            <CardTitle className="text-white">Recent Activity</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              <div className="flex items-center justify-between p-3 rounded-lg bg-white/5">
                <div>
                  <div className="text-white font-medium">Last Model Uploaded</div>
                  <div className="text-sm text-white/60">
                    {formatDate(statistics?.lastModelUploaded)}
                  </div>
                </div>
                <span className="text-sm text-white/40">Upload</span>
              </div>
              <div className="flex items-center justify-between p-3 rounded-lg bg-white/5">
                <div>
                  <div className="text-white font-medium">Last Model Updated</div>
                  <div className="text-sm text-white/60">
                    {formatDate(statistics?.lastModelUpdated)}
                  </div>
                </div>
                <span className="text-sm text-white/40">Update</span>
              </div>
              <div className="flex items-center justify-between p-3 rounded-lg bg-white/5">
                <div>
                  <div className="text-white font-medium">Average File Size</div>
                  <div className="text-sm text-white/60">
                    {statistics?.averageFileSizeMB?.toFixed(2) ?? "0"} MB
                  </div>
                </div>
                <span className="text-sm text-white/40">Size</span>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {statistics?.topUploaders && statistics.topUploaders.length > 0 && (
        <Card variant="glass" className="border-white/20">
          <CardHeader>
            <CardTitle className="text-white">Top Uploaders</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {statistics.topUploaders.slice(0, 5).map((uploader, index) => (
                <div
                  key={uploader.userId}
                  className="flex items-center justify-between p-3 rounded-lg bg-white/5"
                >
                  <div className="flex items-center space-x-3">
                    <div className="flex h-6 w-6 items-center justify-center rounded-full bg-indigo-500 text-sm font-bold text-white">
                      {index + 1}
                    </div>
                    <div>
                      <div className="font-medium text-white">
                        {uploader.username}
                      </div>
                      <div className="text-sm text-white/60">
                        {uploader.modelCount} models
                      </div>
                    </div>
                  </div>
                  <span className="text-sm text-white/40">
                    {uploader.totalFileSizeFormatted}
                  </span>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {statistics?.fileTypeDistribution &&
        statistics.fileTypeDistribution.length > 0 && (
          <Card variant="glass" className="border-white/20">
            <CardHeader>
              <CardTitle className="text-white">
                File Type Distribution
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                {statistics.fileTypeDistribution.slice(0, 8).map((fileType, index) => (
                  <div
                    key={`filetype-${index}-${fileType.fileExtension ?? "unknown"}`}
                    className="flex items-center justify-between p-3 rounded-lg bg-white/5"
                  >
                    <div className="flex items-center space-x-3">
                      <div className="flex h-6 w-6 items-center justify-center rounded-full bg-purple-500 text-sm font-bold text-white">
                        {index + 1}
                      </div>
                      <div>
                        <div className="font-medium text-white">
                          {(fileType.fileExtension ?? "Unknown").toUpperCase()}
                        </div>
                        <div className="text-sm text-white/60">
                          {fileType.count} files
                        </div>
                      </div>
                    </div>
                    <div className="text-right">
                      <div className="text-sm text-white/40">
                        {fileType.totalSizeFormatted}
                      </div>
                      <div className="text-xs text-white/30">
                        {fileType.percentage?.toFixed(1)}%
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        )}
    </div>
  );
}
