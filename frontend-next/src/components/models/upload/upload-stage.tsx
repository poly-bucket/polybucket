"use client";

import type { ReactNode } from "react";
import { Camera, Check, ChevronLeft, ChevronRight, Loader2, Upload } from "lucide-react";
import ModelViewer from "@/components/model-viewer/model-viewer";
import PDFViewer from "@/components/viewers/pdf-viewer";
import MarkdownViewer from "@/components/viewers/markdown-viewer";
import { Button } from "@/components/primitives/button";
import { useUserSettings } from "@/contexts/UserSettingsContext";
import { useFileDataUrl } from "@/hooks/use-file-data-url";
import { cn } from "@/lib/utils";
import type { UploadedFile } from "../file-queue";
import { formatFileSize, UPLOAD_FILE_TYPE_TAGS, type UploadFileType } from "../upload-shared";

const STAGE_HEIGHT = 400;

interface UploadStageProps {
  files: UploadedFile[];
  activeFileId: string | null;
  getFileType: (name: string) => UploadFileType;
  onActiveFileChange: (fileId: string) => void;
  onBrowse: () => void;
  onShowFormats: () => void;
  isDragActive: boolean;
  isExtractingZip: boolean;
  canAddMore: boolean;
  maxFiles: number;
  maxFileSizeLabel: string;
  coverFileId: string | null;
  onSetCover: (fileId: string) => void;
  onOpenThumbnailGenerator?: () => void;
  suspendBackground3d?: boolean;
}

function ImagePreview({ file }: { file: File }) {
  const url = useFileDataUrl(file);
  if (!url) return null;
  return (
    <div className="flex h-full w-full items-center justify-center p-6">
      <img src={url} alt="" className="max-h-full max-w-full rounded-md object-contain shadow-2xl" />
    </div>
  );
}

function Readout({ children, className }: { children: ReactNode; className?: string }) {
  return (
    <span
      className={cn(
        "rounded-md border border-white/10 bg-black/45 px-2 py-1 font-mono text-[11px] tracking-wide text-white/70 backdrop-blur-sm",
        className
      )}
    >
      {children}
    </span>
  );
}

export function UploadStage({
  files,
  activeFileId,
  getFileType,
  onActiveFileChange,
  onBrowse,
  onShowFormats,
  isDragActive,
  isExtractingZip,
  canAddMore,
  maxFiles,
  maxFileSizeLabel,
  coverFileId,
  onSetCover,
  onOpenThumbnailGenerator,
  suspendBackground3d = false,
}: UploadStageProps) {
  const { settings } = useUserSettings();
  const activeIndex = files.findIndex((file) => file.id === activeFileId);
  const activeFile = activeIndex >= 0 ? files[activeIndex] : null;
  const activeType = activeFile ? getFileType(activeFile.name) : "unknown";
  const tag = UPLOAD_FILE_TYPE_TAGS[activeType];
  const isEmpty = files.length === 0;

  const step = (delta: number) => {
    if (files.length < 2) return;
    const base = activeIndex >= 0 ? activeIndex : 0;
    const next = (base + delta + files.length) % files.length;
    onActiveFileChange(files[next].id);
  };

  return (
    <section
      aria-label="Model preview"
      data-drag-active={isDragActive}
      onClick={isEmpty && canAddMore ? onBrowse : undefined}
      onKeyDown={(e) => {
        if (e.target !== e.currentTarget) return;
        if (e.key === "ArrowRight") step(1);
        if (e.key === "ArrowLeft") step(-1);
      }}
      tabIndex={files.length > 1 ? 0 : undefined}
      className={cn(
        "upload-bed relative overflow-hidden rounded-xl border outline-none motion-safe:transition-[border-color,background-color] motion-safe:duration-200",
        "focus-visible:ring-2 focus-visible:ring-primary/60",
        isDragActive ? "border-primary/70" : "border-white/10",
        isEmpty && canAddMore && "cursor-pointer"
      )}
      style={{ height: STAGE_HEIGHT }}
    >
      <div className="upload-bed-grid" aria-hidden />

      {isEmpty ? (
        <div className="relative flex h-full flex-col items-center justify-center gap-5 px-6 text-center">
          <span
            className={cn(
              "flex size-14 items-center justify-center rounded-2xl border bg-white/[0.04] motion-safe:transition-transform motion-safe:duration-200",
              isDragActive ? "scale-110 border-primary/60 text-primary" : "border-white/15 text-white/70"
            )}
            aria-hidden
          >
            <Upload className="size-6" />
          </span>
          <div className="space-y-1.5">
            <p className="text-lg font-semibold tracking-tight text-foreground sm:text-xl">
              {isDragActive ? "Release to add files" : "Drop your model files here"}
            </p>
            <p className="mx-auto max-w-sm text-sm text-muted-foreground">
              Meshes, images, PDFs, or one .zip with everything. Include a .md file to fill in the details.
            </p>
          </div>
          <div className="flex flex-wrap items-center justify-center gap-3">
            <Button
              type="button"
              variant="default"
              onClick={(e) => {
                e.stopPropagation();
                onBrowse();
              }}
            >
              Choose files
            </Button>
            <Button
              type="button"
              variant="ghost"
              size="sm"
              className="text-muted-foreground"
              onClick={(e) => {
                e.stopPropagation();
                onShowFormats();
              }}
            >
              Supported formats
            </Button>
          </div>
        </div>
      ) : (
        <div className="relative h-full w-full">
          {activeFile ? (
            <>
              {activeType === "3d" && suspendBackground3d && (
                <div className="flex h-full w-full items-center justify-center p-4 text-center text-sm text-muted-foreground">
                  3D preview paused while the thumbnail editor is open
                </div>
              )}
              {activeType === "3d" && !suspendBackground3d && (
                <ModelViewer
                  file={activeFile.file}
                  fileName={activeFile.name}
                  autoRotate={settings?.autoRotateModels ?? false}
                />
              )}
              {activeType === "image" && <ImagePreview file={activeFile.file} />}
              {activeType === "pdf" && (
                <PDFViewer file={activeFile.file} width="100%" height={STAGE_HEIGHT} className="h-full" />
              )}
              {activeType === "markdown" && (
                <MarkdownViewer
                  file={activeFile.file}
                  width="100%"
                  height={STAGE_HEIGHT}
                  className="h-full"
                />
              )}
              {activeType === "unknown" && (
                <div className="flex h-full w-full items-center justify-center px-6 text-center text-sm text-muted-foreground">
                  No preview for this file type. It still uploads with the model.
                </div>
              )}
            </>
          ) : (
            <div className="flex h-full w-full items-center justify-center text-sm text-muted-foreground">
              Select a file below to preview it
            </div>
          )}

          {activeFile && (
            <>
              <div className="pointer-events-none absolute right-3 top-3 z-30">
                <div className="pointer-events-auto flex shrink-0 gap-2">
                  {activeType === "3d" && onOpenThumbnailGenerator && (
                    <Button type="button" size="sm" onClick={onOpenThumbnailGenerator}>
                      <Camera aria-hidden />
                      Capture cover
                    </Button>
                  )}
                  {activeType === "image" &&
                    (coverFileId === activeFile.id ? (
                      <span className="flex h-8 items-center gap-1.5 rounded-md border border-primary/50 bg-primary/20 px-3 text-xs font-medium text-primary-foreground">
                        <Check className="size-3.5" aria-hidden />
                        Cover image
                      </span>
                    ) : (
                      <Button type="button" size="sm" onClick={() => onSetCover(activeFile.id)}>
                        Use as cover
                      </Button>
                    ))}
                </div>
              </div>

              <div className="pointer-events-none absolute inset-x-3 bottom-3 z-30 flex items-end justify-between gap-3">
                <span className="flex min-w-0 items-center gap-2 rounded-md border border-white/10 bg-black/50 py-1 pl-1 pr-2.5 backdrop-blur-sm">
                  <span
                    className={cn(
                      "rounded border px-1.5 py-0.5 font-mono text-[10px] font-semibold tracking-wider",
                      tag.className
                    )}
                  >
                    {tag.label}
                  </span>
                  <span className="truncate text-xs font-medium text-white/90">{activeFile.name}</span>
                  <span className="shrink-0 font-mono text-[11px] tabular-nums text-white/55">
                    {formatFileSize(activeFile.size)}
                  </span>
                </span>
                {files.length > 1 && (
                  <div className="pointer-events-auto flex items-center gap-1 rounded-md border border-white/10 bg-black/45 p-0.5 backdrop-blur-sm">
                    <Button
                      type="button"
                      variant="ghost"
                      size="icon-xs"
                      onClick={() => step(-1)}
                      aria-label="Previous file"
                    >
                      <ChevronLeft aria-hidden />
                    </Button>
                    <span className="min-w-12 text-center font-mono text-[11px] tabular-nums text-white/70">
                      {activeIndex + 1} / {files.length}
                    </span>
                    <Button
                      type="button"
                      variant="ghost"
                      size="icon-xs"
                      onClick={() => step(1)}
                      aria-label="Next file"
                    >
                      <ChevronRight aria-hidden />
                    </Button>
                  </div>
                )}
              </div>
            </>
          )}
        </div>
      )}

      {isEmpty && (
        <div className="pointer-events-none absolute inset-x-3 bottom-3 flex justify-between gap-3">
          <Readout>0 / {maxFiles} files</Readout>
          <Readout>Max {maxFileSizeLabel} per file</Readout>
        </div>
      )}

      {!isEmpty && isDragActive && (
        <div className="absolute inset-0 z-40 flex items-center justify-center bg-black/40 backdrop-blur-[2px]">
          <span className="rounded-lg border border-primary/60 bg-black/60 px-4 py-2 text-sm font-medium text-foreground">
            {canAddMore ? "Release to add files" : `File limit reached (${maxFiles})`}
          </span>
        </div>
      )}

      {isExtractingZip && (
        <div
          className="absolute inset-0 z-40 flex items-center justify-center gap-2 bg-black/50 text-sm text-foreground backdrop-blur-[2px]"
          role="status"
        >
          <Loader2 className="size-4 motion-safe:animate-spin" aria-hidden />
          Unpacking archive…
        </div>
      )}
    </section>
  );
}
