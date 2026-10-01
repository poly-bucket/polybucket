"use client";

import { useState } from "react";
import { Card } from "@/components/primitives/card";
import { UploadModelPreviewCard } from "./upload-model-preview-card";
import type { UploadedFile } from "../file-queue";
import type { UploadFileType } from "../upload-shared";
import type { ModelMetadataFormValues } from "@/lib/models/model-metadata";
import { cn } from "@/lib/utils";
import { Check, ChevronDown, ChevronRight, AlertTriangle } from "lucide-react";

function formatFileSize(bytes: number): string {
  if (bytes === 0) return "0 B";
  const k = 1024;
  const sizes = ["B", "KB", "MB", "GB"];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return `${parseFloat((bytes / Math.pow(k, i)).toFixed(2))} ${sizes[i]}`;
}

interface UploadReviewStepProps {
  metadata: ModelMetadataFormValues;
  categories: string[];
  files: UploadedFile[];
  getFileType: (name: string) => UploadFileType;
  thumbnailPreviewUrl: string | null;
  hasThumbnail: boolean;
}

function ChecklistRow({
  ok,
  label,
  detail,
  warn,
}: {
  ok: boolean;
  label: string;
  detail: string;
  warn?: boolean;
}) {
  return (
    <div className="flex gap-3 text-sm">
      <span className="mt-0.5 shrink-0">
        {warn ? (
          <AlertTriangle className="h-4 w-4 text-amber-400" aria-hidden />
        ) : ok ? (
          <Check className="h-4 w-4 text-primary" aria-hidden />
        ) : (
          <span className="inline-block h-4 w-4 rounded-full border border-white/30" />
        )}
      </span>
      <div>
        <p className="font-medium text-foreground">{label}</p>
        <p className={cn("text-muted-foreground", warn && "text-amber-200/80")}>{detail}</p>
      </div>
    </div>
  );
}

export function UploadReviewStep({
  metadata,
  categories,
  files,
  getFileType,
  thumbnailPreviewUrl,
  hasThumbnail,
}: UploadReviewStepProps) {
  const [filesExpanded, setFilesExpanded] = useState(false);

  const flags: string[] = [];
  if (metadata.aiGenerated) flags.push("AI Generated");
  if (metadata.wip) flags.push("Work in Progress");
  if (metadata.nsfw) flags.push("NSFW");
  if (metadata.isRemix) flags.push("Remix");

  return (
    <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
      <Card variant="glass" className="p-4 sm:p-6 space-y-5">
        <h2 className="text-lg font-semibold">Review checklist</h2>
        <div className="space-y-4">
          <ChecklistRow
            ok={metadata.name.trim().length > 0}
            label="Title"
            detail={metadata.name.trim() || "Required"}
          />
          <div>
            <ChecklistRow
              ok={files.length > 0}
              label="Files"
              detail={`${files.length} file(s)`}
            />
            {files.length > 0 && (
              <button
                type="button"
                className="mt-2 flex items-center gap-1 text-xs text-primary hover:underline"
                onClick={() => setFilesExpanded((v) => !v)}
              >
                {filesExpanded ? (
                  <ChevronDown className="h-3.5 w-3.5" />
                ) : (
                  <ChevronRight className="h-3.5 w-3.5" />
                )}
                {filesExpanded ? "Hide file list" : "Show file list"}
              </button>
            )}
            {filesExpanded && (
              <ul className="mt-2 space-y-1 text-xs text-muted-foreground border-l border-white/10 pl-3">
                {files.map((f) => (
                  <li key={f.id}>
                    {f.name} · {formatFileSize(f.size)} · {getFileType(f.name)}
                  </li>
                ))}
              </ul>
            )}
          </div>
          <ChecklistRow
            ok={hasThumbnail}
            warn={!hasThumbnail}
            label="Thumbnail"
            detail={hasThumbnail ? "Cover image selected" : "Optional — default placeholder will be used"}
          />
          <ChecklistRow
            ok={true}
            label="Privacy & license"
            detail={`${metadata.privacy} · ${metadata.license}`}
          />
          {flags.length > 0 && (
            <ChecklistRow ok={true} label="Flags" detail={flags.join(", ")} />
          )}
          {categories.length > 0 && (
            <ChecklistRow
              ok={true}
              label="Categories"
              detail={categories.join(", ")}
            />
          )}
        </div>
      </Card>

      <div className="space-y-3">
        <p className="text-sm font-medium text-muted-foreground">Listing preview</p>
        <UploadModelPreviewCard
          title={metadata.name.trim() || "Untitled model"}
          description={metadata.description ?? ""}
          thumbnailUrl={thumbnailPreviewUrl}
          categories={categories}
        />
      </div>
    </div>
  );
}
