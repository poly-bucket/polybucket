"use client";

import type { ReactNode } from "react";
import { AlertTriangle, Check, Pencil } from "lucide-react";
import { Card } from "@/components/primitives/card";
import { Button } from "@/components/primitives/button";
import { UploadModelPreviewCard } from "./upload-model-preview-card";
import type { UploadedFile } from "../file-queue";
import { formatFileSize, UPLOAD_FILE_TYPE_TAGS, type UploadFileType } from "../upload-shared";
import type { ModelMetadataFormValues } from "@/lib/models/model-metadata";
import { cn } from "@/lib/utils";

interface UploadReviewStepProps {
  metadata: ModelMetadataFormValues;
  categories: string[];
  files: UploadedFile[];
  getFileType: (name: string) => UploadFileType;
  thumbnailPreviewUrl: string | null;
  hasThumbnail: boolean;
  onEdit?: () => void;
}

function ReviewSection({
  title,
  meta,
  onEdit,
  children,
}: {
  title: string;
  meta?: string;
  onEdit?: () => void;
  children: ReactNode;
}) {
  return (
    <section className="px-5 py-5">
      <div className="mb-3 flex items-center justify-between gap-3">
        <div className="flex items-baseline gap-3">
          <h2 className="text-sm font-semibold text-foreground">{title}</h2>
          {meta && (
            <span className="font-mono text-xs tabular-nums text-muted-foreground">{meta}</span>
          )}
        </div>
        {onEdit && (
          <Button type="button" variant="ghost" size="xs" onClick={onEdit} className="text-muted-foreground">
            <Pencil aria-hidden />
            Edit
          </Button>
        )}
      </div>
      {children}
    </section>
  );
}

function ReviewRow({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div className="grid grid-cols-[7rem_1fr] gap-3 py-1.5 text-sm">
      <dt className="text-muted-foreground">{label}</dt>
      <dd className="min-w-0 text-foreground">{children}</dd>
    </div>
  );
}

const Empty = () => <span className="text-muted-foreground">None</span>;

export function UploadReviewStep({
  metadata,
  categories,
  files,
  getFileType,
  thumbnailPreviewUrl,
  hasThumbnail,
  onEdit,
}: UploadReviewStepProps) {
  const flags: string[] = [];
  if (metadata.aiGenerated) flags.push("AI generated");
  if (metadata.wip) flags.push("Work in progress");
  if (metadata.nsfw) flags.push("NSFW");
  if (metadata.isRemix) flags.push("Remix");

  const totalBytes = files.reduce((sum, file) => sum + file.size, 0);
  const description = metadata.description?.trim() ?? "";

  return (
    <div className="grid grid-cols-1 gap-6 lg:grid-cols-12 lg:items-start lg:gap-8">
      <div className="space-y-3 lg:sticky lg:top-20 lg:col-span-5">
        <p className="text-xs font-medium text-muted-foreground">How it appears in listings</p>
        <UploadModelPreviewCard
          title={metadata.name.trim() || "Untitled model"}
          description={metadata.description ?? ""}
          thumbnailUrl={thumbnailPreviewUrl}
          categories={categories}
        />
        <div
          className={cn(
            "flex gap-2.5 rounded-lg border px-3 py-2.5 text-sm",
            hasThumbnail
              ? "border-white/10 bg-white/[0.02] text-muted-foreground"
              : "border-amber-500/30 bg-amber-500/10 text-amber-100/90"
          )}
          role="status"
        >
          {hasThumbnail ? (
            <Check className="mt-0.5 size-4 shrink-0 text-primary" aria-hidden />
          ) : (
            <AlertTriangle className="mt-0.5 size-4 shrink-0 text-amber-400" aria-hidden />
          )}
          <span>
            {hasThumbnail
              ? "Cover image selected."
              : "No cover image. The listing uses a placeholder until you add one."}
          </span>
        </div>
      </div>

      <Card variant="glass" className="gap-0 divide-y divide-white/10 py-0 lg:col-span-7">
        <ReviewSection title="Details" onEdit={onEdit}>
          <dl>
            <ReviewRow label="Title">{metadata.name.trim() || <Empty />}</ReviewRow>
            <ReviewRow label="Description">
              {description ? (
                <span className="line-clamp-4 whitespace-pre-line">{description}</span>
              ) : (
                <Empty />
              )}
            </ReviewRow>
            <ReviewRow label="Visibility">{metadata.privacy}</ReviewRow>
            <ReviewRow label="License">{metadata.license}</ReviewRow>
            <ReviewRow label="Categories">
              {categories.length > 0 ? (
                <span className="flex flex-wrap gap-1.5">
                  {categories.map((category) => (
                    <span
                      key={category}
                      className="rounded-full border border-white/15 px-2 py-0.5 text-xs text-foreground/85"
                    >
                      {category}
                    </span>
                  ))}
                </span>
              ) : (
                <Empty />
              )}
            </ReviewRow>
            <ReviewRow label="Labels">{flags.length > 0 ? flags.join(", ") : <Empty />}</ReviewRow>
          </dl>
        </ReviewSection>

        <ReviewSection
          title="Files"
          meta={`${files.length} · ${formatFileSize(totalBytes)}`}
          onEdit={onEdit}
        >
          <ul className="divide-y divide-white/[0.06] rounded-lg border border-white/10">
            {files.map((file) => {
              const tag = UPLOAD_FILE_TYPE_TAGS[getFileType(file.name)];
              return (
                <li key={file.id} className="flex items-center gap-3 px-3 py-2">
                  <span
                    className={cn(
                      "w-10 shrink-0 rounded border py-0.5 text-center font-mono text-[10px] font-semibold tracking-wider",
                      tag.className
                    )}
                  >
                    {tag.label}
                  </span>
                  <span className="min-w-0 flex-1 truncate text-sm text-foreground">{file.name}</span>
                  {file.isThumbnail && <span className="text-xs text-primary">Cover</span>}
                  <span className="shrink-0 font-mono text-[11px] tabular-nums text-muted-foreground">
                    {formatFileSize(file.size)}
                  </span>
                </li>
              );
            })}
          </ul>
        </ReviewSection>
      </Card>
    </div>
  );
}
