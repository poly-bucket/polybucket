"use client";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/primitives/card";
import { Button } from "@/components/primitives/button";
import { getFileType, type ModelFileType } from "@/lib/utils/modelUtils";
import type { ModelFile } from "@/lib/api/client";
import { cn } from "@/lib/utils";

interface ModelDetailsFilesProps {
  files: ModelFile[];
  downloadingFileIds: Set<string>;
  isAuthenticated: boolean;
  onFileDownload: (file: { id: string; name: string }) => void;
}

function FileIcon({ type }: { type: ModelFileType }) {
  const baseClasses =
    "flex h-10 w-10 shrink-0 items-center justify-center rounded-lg text-xs font-semibold text-white";
  switch (type) {
    case "3d":
      return (
        <div className={cn(baseClasses, "bg-blue-600")}>3D</div>
      );
    case "image":
      return (
        <div className={cn(baseClasses, "bg-purple-600")}>IMG</div>
      );
    case "pdf":
      return (
        <div className={cn(baseClasses, "bg-red-600")}>PDF</div>
      );
    case "markdown":
      return (
        <div className={cn(baseClasses, "bg-yellow-600")}>MD</div>
      );
    default:
      return (
        <div className={cn(baseClasses, "bg-white/20")}>File</div>
      );
  }
}

function formatFileSize(size: number | undefined): string {
  if (!size) return "Unknown";
  if (size >= 1024 * 1024)
    return (size / 1024 / 1024).toFixed(1) + " MB";
  if (size >= 1024) return (size / 1024).toFixed(1) + " KB";
  return size + " B";
}

export function ModelDetailsFiles({
  files,
  downloadingFileIds,
  isAuthenticated,
  onFileDownload,
}: ModelDetailsFilesProps) {
  const normalizedFiles = files.map((file, index) => ({
    id: file.id ?? index.toString(),
    name: file.name ?? `file-${index}`,
    size: file.size,
    mimeType: file.mimeType,
  }));

  if (normalizedFiles.length === 0) {
    return (
      <Card variant="glass" className="border-white/20">
        <CardHeader>
          <CardTitle className="text-white">Files</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-white/60">No files attached to this model.</p>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card variant="glass" className="border-white/20">
      <CardHeader>
        <CardTitle className="text-white">Files</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="space-y-3">
          {normalizedFiles.map((file) => {
            const fileType = getFileType(file.name);
            return (
              <div
                key={file.id}
                className="flex items-center justify-between rounded-lg border border-white/20 p-3 transition-colors hover:bg-white/5"
              >
                <div className="flex items-center gap-3">
                  <FileIcon type={fileType} />
                  <div>
                    <h3 className="font-medium text-white">{file.name}</h3>
                    <p className="text-sm text-white/60">
                      {formatFileSize(file.size)}
                    </p>
                  </div>
                </div>
                <Button
                  variant="glass"
                  size="sm"
                  onClick={() => onFileDownload(file)}
                  disabled={
                    downloadingFileIds.has(file.id) || !isAuthenticated
                  }
                >
                  {downloadingFileIds.has(file.id)
                    ? "Downloading..."
                    : "Download"}
                </Button>
              </div>
            );
          })}
        </div>
      </CardContent>
    </Card>
  );
}
