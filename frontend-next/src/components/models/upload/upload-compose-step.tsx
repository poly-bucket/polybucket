"use client";

import { useEffect, useRef, useState } from "react";
import { UploadFilesStep } from "./upload-files-step";
import { UploadMetadataForm } from "./upload-metadata-form";
import { Card } from "@/components/primitives/card";
import { Button } from "@/components/primitives/button";
import type { UploadedFile } from "../file-queue";
import type { UploadFileType } from "../upload-shared";
import type { ModelMetadataFormValues, ModelMetadataRules } from "@/lib/models/model-metadata";
import type { UseFormReturn } from "react-hook-form";

interface UploadComposeStepProps {
  settingsLoading: boolean;
  isExtractingZip: boolean;
  uploadedFiles: UploadedFile[];
  previewFileId: string | null;
  acceptFormats: string[];
  formatGroups: { label: string; extensions: string[] }[];
  canAddMore: boolean;
  maxFiles: number;
  maxFileSizeLabel: string;
  getFileType: (name: string) => UploadFileType;
  onFilesSelected: (files: File[]) => void;
  onSelectFile: (id: string) => void;
  onRemoveFile: (id: string) => void;
  onClearAll: () => void;
  onDownloadMarkdownTemplate: () => void;
  selectedThumbnailFileId: string | null;
  onSelectThumbnail: (fileId: string) => void;
  canGenerateFrom3d: boolean;
  onOpenThumbnailGenerator: () => void;
  suspendBackground3dPreview?: boolean;
  metadataForm: UseFormReturn<ModelMetadataFormValues>;
  metadataRules: ModelMetadataRules;
  categories: string[];
  onCategoriesChange: (categories: string[]) => void;
  onMetadataValidityChange: (valid: boolean) => void;
  onMetadataDirtyChange: (dirty: boolean) => void;
}

function hasFilePayload(e: DragEvent) {
  return Array.from(e.dataTransfer?.types ?? []).includes("Files");
}

function usePageFileDrop(onFiles: (files: File[]) => void, enabled: boolean) {
  const [isDragActive, setIsDragActive] = useState(false);
  const depthRef = useRef(0);
  const onFilesRef = useRef(onFiles);

  useEffect(() => {
    onFilesRef.current = onFiles;
  }, [onFiles]);

  useEffect(() => {
    if (!enabled) return;

    const handleEnter = (e: DragEvent) => {
      if (!hasFilePayload(e)) return;
      e.preventDefault();
      depthRef.current += 1;
      setIsDragActive(true);
    };
    const handleOver = (e: DragEvent) => {
      if (!hasFilePayload(e)) return;
      e.preventDefault();
      if (e.dataTransfer) e.dataTransfer.dropEffect = "copy";
    };
    const handleLeave = (e: DragEvent) => {
      if (!hasFilePayload(e)) return;
      depthRef.current = Math.max(0, depthRef.current - 1);
      if (depthRef.current === 0) setIsDragActive(false);
    };
    const handleDrop = (e: DragEvent) => {
      if (!hasFilePayload(e)) return;
      e.preventDefault();
      depthRef.current = 0;
      setIsDragActive(false);
      const files = Array.from(e.dataTransfer?.files ?? []);
      if (files.length > 0) onFilesRef.current(files);
    };

    window.addEventListener("dragenter", handleEnter);
    window.addEventListener("dragover", handleOver);
    window.addEventListener("dragleave", handleLeave);
    window.addEventListener("drop", handleDrop);
    return () => {
      window.removeEventListener("dragenter", handleEnter);
      window.removeEventListener("dragover", handleOver);
      window.removeEventListener("dragleave", handleLeave);
      window.removeEventListener("drop", handleDrop);
      depthRef.current = 0;
      setIsDragActive(false);
    };
  }, [enabled]);

  return enabled && isDragActive;
}

export function UploadComposeStep({
  settingsLoading,
  isExtractingZip,
  uploadedFiles,
  previewFileId,
  acceptFormats,
  formatGroups,
  canAddMore,
  maxFiles,
  maxFileSizeLabel,
  getFileType,
  onFilesSelected,
  onSelectFile,
  onRemoveFile,
  onClearAll,
  onDownloadMarkdownTemplate,
  selectedThumbnailFileId,
  onSelectThumbnail,
  canGenerateFrom3d,
  onOpenThumbnailGenerator,
  suspendBackground3dPreview = false,
  metadataForm,
  metadataRules,
  categories,
  onCategoriesChange,
  onMetadataValidityChange,
  onMetadataDirtyChange,
}: UploadComposeStepProps) {
  const markdownInputRef = useRef<HTMLInputElement>(null);
  const isDragActive = usePageFileDrop(
    onFilesSelected,
    !settingsLoading && !isExtractingZip && !suspendBackground3dPreview
  );

  const handleMarkdownImport = (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = e.target.files;
    if (files?.length) onFilesSelected(Array.from(files));
    e.target.value = "";
  };

  return (
    <div className="grid grid-cols-1 gap-6 lg:grid-cols-12 lg:items-start lg:gap-8">
      <div className="lg:sticky lg:top-20 lg:col-span-7">
        <UploadFilesStep
          settingsLoading={settingsLoading}
          isExtractingZip={isExtractingZip}
          isDragActive={isDragActive}
          uploadedFiles={uploadedFiles}
          selectedFileId={previewFileId}
          coverFileId={selectedThumbnailFileId}
          acceptFormats={acceptFormats}
          formatGroups={formatGroups}
          canAddMore={canAddMore}
          maxFiles={maxFiles}
          maxFileSizeLabel={maxFileSizeLabel}
          getFileType={getFileType}
          onFilesSelected={onFilesSelected}
          onSelectFile={onSelectFile}
          onRemoveFile={onRemoveFile}
          onClearAll={onClearAll}
          onSetCover={onSelectThumbnail}
          onOpenThumbnailGenerator={canGenerateFrom3d ? onOpenThumbnailGenerator : undefined}
          suspendBackground3dPreview={suspendBackground3dPreview}
        />
      </div>

      <Card variant="glass" className="gap-0 py-0 lg:col-span-5">
        <div className="flex items-center justify-between gap-3 border-b border-white/10 px-5 py-3.5">
          <h2 className="text-base font-semibold text-foreground">Details</h2>
          <div className="-mr-2 flex items-center gap-0.5">
            <Button
              type="button"
              variant="ghost"
              size="xs"
              className="text-muted-foreground"
              onClick={() => markdownInputRef.current?.click()}
            >
              Import .md
            </Button>
            <span className="text-white/15" aria-hidden>
              ·
            </span>
            <Button
              type="button"
              variant="ghost"
              size="xs"
              className="text-muted-foreground"
              onClick={onDownloadMarkdownTemplate}
            >
              Get template
            </Button>
            <input
              ref={markdownInputRef}
              type="file"
              accept=".md,.markdown"
              className="hidden"
              onChange={handleMarkdownImport}
            />
          </div>
        </div>
        <div className="px-5 py-5">
          <UploadMetadataForm
            form={metadataForm}
            categories={categories}
            allowModelRemixing={metadataRules.allowModelRemixing}
            descriptionRules={{
              required: metadataRules.requireModelDescription,
              min: metadataRules.minDescriptionLength,
              max: metadataRules.maxDescriptionLength,
            }}
            onCategoriesChange={onCategoriesChange}
            onValidityChange={onMetadataValidityChange}
            onDirtyChange={onMetadataDirtyChange}
          />
        </div>
      </Card>
    </div>
  );
}
