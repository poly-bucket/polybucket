"use client";

import React from "react";
import { Button } from "@/components/primitives/button";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/primitives/card";

interface ThumbnailGeneratorProps {
  modelFile: File;
  onThumbnailGenerated: (thumbnailBlob: Blob, fileName: string) => void;
  onCancel: () => void;
}

export default function ThumbnailGenerator({
  onCancel,
}: ThumbnailGeneratorProps) {
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
      <Card variant="glass" className="w-full max-w-md">
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle>Custom Thumbnail</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <p className="text-muted-foreground text-sm">
            Custom thumbnail generation coming soon. You can select an existing
            image from your upload queue as the thumbnail.
          </p>
          <div className="flex justify-end gap-2">
            <Button variant="outline" onClick={onCancel}>
              Cancel
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
