"use client";

import { useRef, useState } from "react";
import type { UploadedFile } from "../file-queue";
import { Button } from "@/components/primitives/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { SupportedFormatsDialog } from "./supported-formats-dialog";
import { UploadStage } from "./upload-stage";
import { UploadFileList } from "./upload-file-list";
import type { UploadFileType } from "../upload-shared";
import { PreviewPanelSkeleton } from "@/components/ui/skeletons";

interface UploadFilesStepProps {
  settingsLoading: boolean;
  isExtractingZip: boolean;
  isDragActive: boolean;
  uploadedFiles: UploadedFile[];
  selectedFileId: string | null;
  coverFileId: string | null;
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
  onSetCover: (id: string) => void;
  onOpenThumbnailGenerator?: () => void;
  suspendBackground3dPreview?: boolean;
}

export function UploadFilesStep({
  settingsLoading,
  isExtractingZip,
  isDragActive,
  uploadedFiles,
  selectedFileId,
  coverFileId,
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
  onSetCover,
  onOpenThumbnailGenerator,
  suspendBackground3dPreview = false,
}: UploadFilesStepProps) {
  const [formatsOpen, setFormatsOpen] = useState(false);
  const [clearConfirmOpen, setClearConfirmOpen] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const browse = () => {
    if (canAddMore && !isExtractingZip) fileInputRef.current?.click();
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = e.target.files;
    if (files?.length) onFilesSelected(Array.from(files));
    e.target.value = "";
  };

  if (settingsLoading) {
    return (
      <div className="overflow-hidden rounded-xl border border-white/10">
        <PreviewPanelSkeleton minHeight="min-h-[400px]" />
      </div>
    );
  }

  return (
    <>
      <div className="space-y-5">
        <UploadStage
          files={uploadedFiles}
          activeFileId={selectedFileId}
          getFileType={getFileType}
          onActiveFileChange={onSelectFile}
          onBrowse={browse}
          onShowFormats={() => setFormatsOpen(true)}
          isDragActive={isDragActive}
          isExtractingZip={isExtractingZip}
          canAddMore={canAddMore}
          maxFiles={maxFiles}
          maxFileSizeLabel={maxFileSizeLabel}
          coverFileId={coverFileId}
          onSetCover={onSetCover}
          onOpenThumbnailGenerator={onOpenThumbnailGenerator}
          suspendBackground3d={suspendBackground3dPreview}
        />

        {uploadedFiles.length > 0 && (
          <UploadFileList
            files={uploadedFiles}
            selectedFileId={selectedFileId}
            coverFileId={coverFileId}
            getFileType={getFileType}
            maxFiles={maxFiles}
            canAddMore={canAddMore && !isExtractingZip}
            onSelectFile={onSelectFile}
            onRemoveFile={onRemoveFile}
            onSetCover={onSetCover}
            onBrowse={browse}
            onClearAll={() => setClearConfirmOpen(true)}
          />
        )}

        <input
          ref={fileInputRef}
          type="file"
          multiple
          accept={acceptFormats.map((f) => (f.startsWith(".") ? f : `.${f}`)).join(",")}
          onChange={handleFileChange}
          className="hidden"
          disabled={isExtractingZip || !canAddMore}
        />
      </div>

      <SupportedFormatsDialog
        open={formatsOpen}
        onOpenChange={setFormatsOpen}
        groups={formatGroups}
      />

      <Dialog open={clearConfirmOpen} onOpenChange={setClearConfirmOpen}>
        <DialogContent variant="glass" className="max-w-sm">
          <DialogHeader>
            <DialogTitle>Clear all files?</DialogTitle>
            <DialogDescription>
              This removes every file from the upload queue. You cannot undo this action.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter className="gap-2 sm:gap-0">
            <Button type="button" variant="outline" onClick={() => setClearConfirmOpen(false)}>
              Keep files
            </Button>
            <Button
              type="button"
              variant="destructive"
              onClick={() => {
                onClearAll();
                setClearConfirmOpen(false);
              }}
            >
              Clear all
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}
