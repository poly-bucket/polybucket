"use client";

import UploadPreviewCarousel from "../upload-preview-carousel";
import { Card } from "@/components/primitives/card";
import { UploadThumbnailPanel } from "./upload-thumbnail-panel";
import { UploadMetadataForm } from "./upload-metadata-form";
import type { UploadedFile } from "../file-queue";
import type { UploadFileType } from "../upload-shared";
import type { ModelMetadataFormValues } from "@/lib/models/model-metadata";
import type { UseFormReturn } from "react-hook-form";

interface UploadDetailsStepProps {
  uploadedFiles: UploadedFile[];
  previewFileId: string | null;
  getFileType: (name: string) => UploadFileType;
  onSelectPreviewFile: (id: string) => void;
  imageFiles: UploadedFile[];
  selectedThumbnailFileId: string | null;
  onSelectThumbnail: (fileId: string) => void;
  canGenerateFrom3d: boolean;
  onOpenThumbnailGenerator: () => void;
  metadataForm: UseFormReturn<ModelMetadataFormValues>;
  categories: string[];
  onCategoriesChange: (categories: string[]) => void;
  onMetadataValidityChange: (valid: boolean) => void;
  onMetadataDirtyChange: (dirty: boolean) => void;
}

export function UploadDetailsStep({
  uploadedFiles,
  previewFileId,
  getFileType,
  onSelectPreviewFile,
  imageFiles,
  selectedThumbnailFileId,
  onSelectThumbnail,
  canGenerateFrom3d,
  onOpenThumbnailGenerator,
  metadataForm,
  categories,
  onCategoriesChange,
  onMetadataValidityChange,
  onMetadataDirtyChange,
}: UploadDetailsStepProps) {
  return (
    <div className="grid grid-cols-1 gap-6 lg:grid-cols-2 lg:items-start">
      <div className="space-y-4 lg:sticky lg:top-4">
        <UploadPreviewCarousel
          files={uploadedFiles}
          activeFileId={previewFileId}
          getFileType={getFileType}
          onActiveFileChange={onSelectPreviewFile}
        />
      </div>
      <div className="space-y-6">
        <UploadThumbnailPanel
          imageFiles={imageFiles}
          selectedThumbnailFileId={selectedThumbnailFileId}
          onSelectThumbnail={onSelectThumbnail}
          canGenerateFrom3d={canGenerateFrom3d}
          onOpenThumbnailGenerator={onOpenThumbnailGenerator}
        />
        <Card variant="glass" className="p-4 sm:p-6">
          <h3 className="text-lg font-medium mb-4">Model details</h3>
          <UploadMetadataForm
            form={metadataForm}
            categories={categories}
            onCategoriesChange={onCategoriesChange}
            onValidityChange={onMetadataValidityChange}
            onDirtyChange={onMetadataDirtyChange}
          />
        </Card>
      </div>
    </div>
  );
}
