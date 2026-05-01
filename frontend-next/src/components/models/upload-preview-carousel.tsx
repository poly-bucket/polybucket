"use client";

import React from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/primitives/card";
import { Button } from "@/components/primitives/button";
import ModelViewer from "@/components/model-viewer/model-viewer";
import PDFViewer from "@/components/viewers/pdf-viewer";
import MarkdownViewer from "@/components/viewers/markdown-viewer";
import { cn } from "@/lib/utils";
import type { UploadedFile } from "./file-queue";
import type { UploadFileType } from "./upload-shared";

interface UploadPreviewCarouselProps {
  files: UploadedFile[];
  activeFileId: string | null;
  getFileType: (fileName: string) => UploadFileType;
  onActiveFileChange: (fileId: string) => void;
  onOpenThumbnailGenerator: () => void;
}

function ImagePreview({ file }: { file: File }) {
  const [url, setUrl] = React.useState<string | null>(null);
  React.useEffect(() => {
    const objectUrl = URL.createObjectURL(file);
    setUrl(objectUrl);
    return () => URL.revokeObjectURL(objectUrl);
  }, [file]);
  if (!url) return null;
  return (
    <div className="h-full w-full flex items-center justify-center p-4">
      <img src={url} alt="Preview" className="max-w-full max-h-full object-contain" />
    </div>
  );
}

export default function UploadPreviewCarousel({
  files,
  activeFileId,
  getFileType,
  onActiveFileChange,
  onOpenThumbnailGenerator,
}: UploadPreviewCarouselProps) {
  const activeFile = files.find((file) => file.id === activeFileId) ?? null;
  const activeFileType = activeFile ? getFileType(activeFile.name) : "unknown";

  return (
    <Card variant="glass" className="overflow-hidden">
      <CardHeader className="space-y-3">
        <div className="flex items-center justify-between gap-4">
          <CardTitle>
            Preview {activeFile ? `: ${activeFile.name}` : ""}
          </CardTitle>
          {activeFileType === "3d" && (
            <Button variant="outline" size="sm" onClick={onOpenThumbnailGenerator}>
              Generate Thumbnail
            </Button>
          )}
        </div>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="h-96 w-full bg-muted/30 rounded-md overflow-hidden">
          {activeFile ? (
            <>
              {activeFileType === "3d" && (
                <ModelViewer file={activeFile.file} fileName={activeFile.name} autoRotate />
              )}
              {activeFileType === "image" && <ImagePreview file={activeFile.file} />}
              {activeFileType === "pdf" && (
                <PDFViewer
                  file={activeFile.file}
                  width="100%"
                  height={384}
                  className="h-96"
                />
              )}
              {activeFileType === "markdown" && (
                <MarkdownViewer
                  file={activeFile.file}
                  width="100%"
                  height={384}
                  className="h-96"
                />
              )}
              {activeFileType === "unknown" && (
                <div className="h-full w-full flex items-center justify-center text-muted-foreground">
                  Preview not available for this file type
                </div>
              )}
            </>
          ) : (
            <div className="h-full w-full flex items-center justify-center text-muted-foreground">
              Select a file from the queue to preview
            </div>
          )}
        </div>

        {files.length > 1 && (
          <div className="flex flex-wrap gap-2" role="tablist" aria-label="Upload preview carousel">
            {files.map((file) => {
              const type = getFileType(file.name);
              const selected = file.id === activeFileId;
              return (
                <button
                  type="button"
                  key={file.id}
                  onClick={() => onActiveFileChange(file.id)}
                  role="tab"
                  aria-selected={selected}
                  className={cn(
                    "h-16 w-16 sm:h-20 sm:w-20 rounded-md border-2 overflow-hidden transition-colors",
                    selected
                      ? "border-primary ring-2 ring-primary/60"
                      : "border-white/20 hover:border-white/40"
                  )}
                >
                  {type === "image" ? (
                    <ImagePreview file={file.file} />
                  ) : (
                    <div className="h-full w-full flex items-center justify-center text-xs text-foreground bg-white/5 uppercase">
                      {type === "unknown" ? "file" : type}
                    </div>
                  )}
                </button>
              );
            })}
          </div>
        )}
      </CardContent>
    </Card>
  );
}
