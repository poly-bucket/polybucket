"use client";

import React from "react";
import { Card } from "@/components/primitives/card";
import { Button } from "@/components/primitives/button";
import { cn } from "@/lib/utils";
import type { UploadedFile } from "../file-queue";

function ThumbnailImage({ file }: { file: File }) {
  const [url, setUrl] = React.useState<string | null>(null);
  React.useEffect(() => {
    const objectUrl = URL.createObjectURL(file);
    setUrl(objectUrl);
    return () => URL.revokeObjectURL(objectUrl);
  }, [file]);
  if (!url) return null;
  return (
    <img src={url} alt="" className="h-full w-full object-cover" />
  );
}

interface UploadThumbnailPanelProps {
  imageFiles: UploadedFile[];
  selectedThumbnailFileId: string | null;
  onSelectThumbnail: (fileId: string) => void;
  canGenerateFrom3d: boolean;
  onOpenThumbnailGenerator: () => void;
}

export function UploadThumbnailPanel({
  imageFiles,
  selectedThumbnailFileId,
  onSelectThumbnail,
  canGenerateFrom3d,
  onOpenThumbnailGenerator,
}: UploadThumbnailPanelProps) {
  const hasThumbnail = selectedThumbnailFileId !== null;

  return (
    <Card variant="glass" className="p-4 sm:p-6 space-y-4">
      <div>
        <h3 className="text-lg font-medium">Thumbnail</h3>
        <p className="text-sm text-muted-foreground mt-1">
          Choose a cover image for listings and search. You can upload images or generate one from a 3D
          preview.
        </p>
      </div>

      {!hasThumbnail && (
        <div
          className="rounded-md border border-amber-500/30 bg-amber-500/10 px-3 py-2 text-sm text-amber-100/90"
          role="status"
        >
          No thumbnail selected. Your model will use a default placeholder until you add one.
        </div>
      )}

      {canGenerateFrom3d && (
        <Button type="button" variant="outline" size="sm" onClick={onOpenThumbnailGenerator}>
          Generate thumbnail from 3D preview
        </Button>
      )}

      {imageFiles.length === 0 ? (
        <p className="text-sm text-muted-foreground">
          Upload a PNG or JPEG, or generate a thumbnail from your 3D file.
        </p>
      ) : (
        <div className="grid grid-cols-3 sm:grid-cols-4 gap-2">
          {imageFiles.map((file) => {
            const selected = file.id === selectedThumbnailFileId;
            return (
              <button
                key={file.id}
                type="button"
                onClick={() => onSelectThumbnail(file.id)}
                className={cn(
                  "aspect-square rounded-md border-2 overflow-hidden transition-colors",
                  selected
                    ? "border-primary ring-2 ring-primary/60"
                    : "border-white/20 hover:border-white/40"
                )}
                aria-pressed={selected}
                title={file.name}
              >
                <ThumbnailImage file={file.file} />
              </button>
            );
          })}
        </div>
      )}
    </Card>
  );
}
