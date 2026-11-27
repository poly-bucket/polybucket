"use client";

import { Plus, Star, X } from "lucide-react";
import { Button } from "@/components/primitives/button";
import { useFileDataUrl } from "@/hooks/use-file-data-url";
import { cn } from "@/lib/utils";
import type { UploadedFile } from "../file-queue";
import { formatFileSize, UPLOAD_FILE_TYPE_TAGS, type UploadFileType } from "../upload-shared";

interface UploadFileListProps {
  files: UploadedFile[];
  selectedFileId: string | null;
  coverFileId: string | null;
  getFileType: (name: string) => UploadFileType;
  maxFiles: number;
  canAddMore: boolean;
  onSelectFile: (id: string) => void;
  onRemoveFile: (id: string) => void;
  onSetCover: (id: string) => void;
  onBrowse: () => void;
  onClearAll: () => void;
}

function ImageThumb({ file }: { file: File }) {
  const url = useFileDataUrl(file);
  return (
    <span className="block size-8 shrink-0 overflow-hidden rounded border border-white/15 bg-white/5">
      {url && <img src={url} alt="" className="h-full w-full object-cover" />}
    </span>
  );
}

export function UploadFileList({
  files,
  selectedFileId,
  coverFileId,
  getFileType,
  maxFiles,
  canAddMore,
  onSelectFile,
  onRemoveFile,
  onSetCover,
  onBrowse,
  onClearAll,
}: UploadFileListProps) {
  const totalBytes = files.reduce((sum, file) => sum + file.size, 0);
  const hasCoverCandidate = files.some((file) => {
    const type = getFileType(file.name);
    return type === "image" || type === "3d";
  });

  return (
    <div className="space-y-3">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <div className="flex items-baseline gap-3">
          <h2 className="text-sm font-semibold text-foreground">Files</h2>
          <span className="font-mono text-xs tabular-nums text-muted-foreground">
            {files.length} / {maxFiles}
            {files.length > 0 && ` · ${formatFileSize(totalBytes)}`}
          </span>
        </div>
        <div className="flex gap-1">
          <Button type="button" variant="ghost" size="sm" onClick={onClearAll} className="text-muted-foreground hover:text-destructive">
            Clear all
          </Button>
          <Button type="button" size="sm" onClick={onBrowse} disabled={!canAddMore}>
            <Plus aria-hidden />
            {canAddMore ? "Add files" : "Limit reached"}
          </Button>
        </div>
      </div>

      <ul className="max-h-80 divide-y divide-white/[0.06] overflow-y-auto rounded-xl border border-white/10 bg-white/[0.02]">
        {files.map((file) => {
          const type = getFileType(file.name);
          const tag = UPLOAD_FILE_TYPE_TAGS[type];
          const isSelected = file.id === selectedFileId;
          const isCover = file.id === coverFileId;

          return (
            <li
              key={file.id}
              className={cn(
                "group relative flex items-center gap-3 px-3 py-2 motion-safe:transition-colors",
                isSelected ? "bg-primary/[0.12]" : "hover:bg-white/[0.04]"
              )}
            >
              <span
                aria-hidden
                className={cn(
                  "absolute inset-y-1 left-0 w-0.5 rounded-full",
                  isSelected ? "bg-primary" : "bg-transparent"
                )}
              />
              <button
                type="button"
                onClick={() => onSelectFile(file.id)}
                aria-current={isSelected ? "true" : undefined}
                className="flex min-w-0 flex-1 items-center gap-3 rounded text-left outline-none focus-visible:ring-2 focus-visible:ring-primary/60"
              >
                {type === "image" ? (
                  <ImageThumb file={file.file} />
                ) : (
                  <span
                    className={cn(
                      "flex h-8 w-8 shrink-0 items-center justify-center rounded border font-mono text-[10px] font-semibold tracking-wider",
                      tag.className
                    )}
                  >
                    {tag.label}
                  </span>
                )}
                <span className="min-w-0 flex-1">
                  <span className="block truncate text-sm text-foreground">{file.name}</span>
                  <span className="block font-mono text-[11px] tabular-nums text-muted-foreground">
                    {formatFileSize(file.size)}
                    {isCover && <span className="text-primary"> · cover</span>}
                  </span>
                </span>
              </button>

              {type === "image" && (
                <Button
                  type="button"
                  variant="ghost"
                  size="icon-sm"
                  onClick={() => onSetCover(file.id)}
                  aria-pressed={isCover}
                  aria-label={isCover ? `${file.name} is the cover image` : `Use ${file.name} as cover image`}
                  title={isCover ? "Cover image" : "Use as cover image"}
                  className={cn(isCover ? "text-primary" : "text-muted-foreground")}
                >
                  <Star className={cn(isCover && "fill-current")} aria-hidden />
                </Button>
              )}
              <Button
                type="button"
                variant="ghost"
                size="icon-sm"
                onClick={() => onRemoveFile(file.id)}
                aria-label={`Remove ${file.name}`}
                className="text-muted-foreground hover:text-destructive"
              >
                <X aria-hidden />
              </Button>
            </li>
          );
        })}
      </ul>

      {!coverFileId && hasCoverCandidate && (
        <p className="text-xs text-muted-foreground">
          No cover image yet. Star an image above, or use Capture cover on a 3D file.
        </p>
      )}
    </div>
  );
}
