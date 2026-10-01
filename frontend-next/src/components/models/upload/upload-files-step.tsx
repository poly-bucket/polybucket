"use client";

import { useRef, useState } from "react";
import FileDropZone from "../file-drop-zone";
import FileQueue, { type UploadedFile } from "../file-queue";
import { Card } from "@/components/primitives/card";
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
import type { UploadFileType } from "../upload-shared";
import { PreviewPanelSkeleton } from "@/components/ui/skeletons";

interface UploadFilesStepProps {
  settingsLoading: boolean;
  isExtractingZip: boolean;
  uploadedFiles: UploadedFile[];
  selectedFileId: string | null;
  acceptFormats: string[];
  formatGroups: { label: string; extensions: string[] }[];
  canAddMore: boolean;
  maxFiles: number;
  getFileType: (name: string) => UploadFileType;
  onFilesSelected: (files: File[]) => void;
  onSelectFile: (id: string) => void;
  onRemoveFile: (id: string) => void;
  onClearAll: () => void;
  onDownloadMarkdownTemplate: () => void;
}

export function UploadFilesStep({
  settingsLoading,
  isExtractingZip,
  uploadedFiles,
  selectedFileId,
  acceptFormats,
  formatGroups,
  canAddMore,
  maxFiles,
  getFileType,
  onFilesSelected,
  onSelectFile,
  onRemoveFile,
  onClearAll,
  onDownloadMarkdownTemplate,
}: UploadFilesStepProps) {
  const [formatsOpen, setFormatsOpen] = useState(false);
  const [clearConfirmOpen, setClearConfirmOpen] = useState(false);
  const markdownInputRef = useRef<HTMLInputElement>(null);

  const handleMarkdownImport = (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = e.target.files;
    if (files?.length) {
      onFilesSelected(Array.from(files));
    }
    e.target.value = "";
  };

  if (settingsLoading) {
    return (
      <Card variant="glass" className="overflow-hidden p-0">
        <PreviewPanelSkeleton minHeight="min-h-[320px]" />
      </Card>
    );
  }

  return (
    <>
      <Card variant="glass" className="overflow-hidden p-0">
        <div className="flex flex-wrap items-center justify-between gap-2 border-b border-white/10 px-4 py-3">
          <div>
            <p className="text-sm font-medium text-foreground">Model files</p>
            <p className="text-xs text-muted-foreground">
              Add meshes, images, docs, or zip archives (max {maxFiles} files).
            </p>
          </div>
          <div className="flex flex-wrap gap-2">
            <Button type="button" variant="outline" size="sm" onClick={() => setFormatsOpen(true)}>
              Supported formats
            </Button>
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={() => markdownInputRef.current?.click()}
            >
              Import metadata (.md)
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
        <div className="p-4 space-y-4">
          <FileDropZone
            onFilesSelected={onFilesSelected}
            acceptFormats={acceptFormats}
            canAddMore={canAddMore}
            maxFiles={maxFiles}
            variant={uploadedFiles.length > 0 ? "compact" : "large"}
            disabled={isExtractingZip}
            showFormatSummary={false}
          />
          <FileQueue
            files={uploadedFiles}
            selectedFileId={selectedFileId}
            onSelectFile={onSelectFile}
            onRemoveFile={onRemoveFile}
            getFileType={getFileType}
            maxFiles={maxFiles}
            onClearAll={() => setClearConfirmOpen(true)}
          />
        </div>
        <div className="border-t border-white/10 px-4 py-3">
          <button
            type="button"
            onClick={onDownloadMarkdownTemplate}
            className="text-sm text-primary hover:underline"
          >
            Download markdown template
          </button>
        </div>
      </Card>

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
