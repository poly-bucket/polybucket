"use client";

import React from "react";
import { cn } from "@/lib/utils";

export interface UploadedFile {
  id: string;
  name: string;
  size: number;
  type: string;
  file: File;
  progress: number;
  isThumbnail: boolean;
}

interface FileQueueProps {
  files: UploadedFile[];
  selectedFileId: string | null;
  onSelectFile: (id: string) => void;
  onRemoveFile: (id: string) => void;
  onThumbnailToggle?: (id: string, checked: boolean) => void;
  getFileType: (name: string) => "3d" | "image" | "pdf" | "markdown" | "unknown";
  supportedImageFormats: string[];
  maxFiles: number;
  onClearAll: () => void;
}

function formatFileSize(bytes: number): string {
  if (bytes === 0) return "0 B";
  const k = 1024;
  const sizes = ["B", "KB", "MB", "GB"];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return `${parseFloat((bytes / Math.pow(k, i)).toFixed(2))} ${sizes[i]}`;
}

export default function FileQueue({
  files,
  selectedFileId,
  onSelectFile,
  onRemoveFile,
  onThumbnailToggle,
  getFileType,
  supportedImageFormats,
  maxFiles,
  onClearAll,
}: FileQueueProps) {
  return (
    <div className="space-y-4">
      <div className="flex justify-between items-center">
        <div>
          <h3 className="text-lg font-medium text-primary">Upload Queue</h3>
          <p className="text-xs text-muted-foreground mt-1">
            {files.length} / {maxFiles} files
            {files.length >= maxFiles && (
              <span className="ml-2 text-yellow-500">(max reached)</span>
            )}
          </p>
        </div>
        {files.length > 0 && (
          <button
            type="button"
            onClick={onClearAll}
            className="text-sm text-destructive hover:text-destructive/80"
          >
            Clear All
          </button>
        )}
      </div>

      {files.some((f) => getFileType(f.name) === "image") && onThumbnailToggle && (
        <div className="p-3 rounded-lg bg-primary/10 border border-primary/20 text-sm text-foreground">
          <strong>Tip:</strong> Use the thumbnail selector in the preview step, or the image checkboxes here, to choose the model thumbnail.
        </div>
      )}

      {files.length === 0 ? (
        <p className="text-muted-foreground text-center py-8 text-sm">
          No files selected
        </p>
      ) : (
        <div className="space-y-3 max-h-64 overflow-y-auto">
          {files.map((file) => {
            const isSelected = selectedFileId === file.id;
            const fileType = getFileType(file.name);
            const ext = file.name.toLowerCase().substring(file.name.lastIndexOf("."));
            const isImage = supportedImageFormats.includes(ext);

            return (
              <div
                key={file.id}
                role="button"
                tabIndex={0}
                onClick={() => onSelectFile(file.id)}
                onKeyDown={(e) => {
                  if (e.key === "Enter" || e.key === " ") {
                    e.preventDefault();
                    onSelectFile(file.id);
                  }
                }}
                className={cn(
                  "rounded-lg p-3 cursor-pointer transition-all duration-200",
                  "bg-muted/50 hover:bg-muted",
                  isSelected && "ring-2 ring-primary bg-muted"
                )}
              >
                <div className="flex justify-between items-start gap-2">
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 flex-wrap">
                      {isImage && onThumbnailToggle && (
                        <input
                          type="checkbox"
                          checked={file.isThumbnail}
                          onChange={(e) => {
                            e.stopPropagation();
                            onThumbnailToggle(file.id, e.target.checked);
                          }}
                          onClick={(e) => e.stopPropagation()}
                          className="rounded border-input"
                          title="Set as thumbnail"
                        />
                      )}
                      <span className="text-sm text-foreground truncate block">
                        {file.name}
                        {file.isThumbnail && (
                          <span className="ml-2 text-xs text-primary">
                            Thumbnail
                          </span>
                        )}
                      </span>
                      {fileType === "3d" && (
                        <span className="text-xs text-blue-400">3D</span>
                      )}
                      {fileType === "image" && (
                        <span className="text-xs text-purple-400">Image</span>
                      )}
                      {fileType === "pdf" && (
                        <span className="text-xs text-red-400">PDF</span>
                      )}
                      {fileType === "markdown" && (
                        <span className="text-xs text-yellow-400">MD</span>
                      )}
                    </div>
                    <p className="text-xs text-muted-foreground mt-1">
                      {formatFileSize(file.size)}
                    </p>
                  </div>
                  <button
                    type="button"
                    onClick={(e) => {
                      e.stopPropagation();
                      onRemoveFile(file.id);
                    }}
                    className="text-muted-foreground hover:text-destructive shrink-0"
                    aria-label={`Remove ${file.name}`}
                  >
                    <svg
                      className="w-4 h-4"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M6 18L18 6M6 6l12 12"
                      />
                    </svg>
                  </button>
                </div>
                <div className="w-full bg-muted rounded-full h-1 mt-2">
                  <div
                    className="bg-primary h-1 rounded-full transition-all duration-300"
                    style={{ width: `${file.progress}%` }}
                  />
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
