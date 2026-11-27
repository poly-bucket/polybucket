"use client";

import { ArrowRight, CircleAlert, CircleCheck, Loader2 } from "lucide-react";
import { Button } from "@/components/primitives/button";
import type { UploadWizardStep } from "./use-upload-wizard";

interface UploadWizardFooterProps {
  currentStep: UploadWizardStep;
  canAdvance: boolean;
  isUploading: boolean;
  blockers?: string[];
  summary?: string;
  onBack: () => void;
  onNext: () => void;
  onUpload: () => void;
  onCancel: () => void;
}

export function UploadWizardFooter({
  currentStep,
  canAdvance,
  isUploading,
  blockers = [],
  summary,
  onBack,
  onNext,
  onUpload,
  onCancel,
}: UploadWizardFooterProps) {
  const showBack = currentStep !== "compose";
  const firstBlocker = blockers[0];

  return (
    <div className="sticky bottom-0 z-10 -mx-4 mt-8 border-t border-white/10 bg-background/80 px-4 py-3 backdrop-blur-md sm:mx-0 sm:mb-4 sm:rounded-xl sm:border sm:px-5">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex min-w-0 items-center gap-3">
          <Button type="button" variant="ghost" onClick={onCancel} disabled={isUploading}>
            Cancel
          </Button>
          <p className="flex min-w-0 items-center gap-2 text-sm" aria-live="polite">
            {firstBlocker ? (
              <>
                <CircleAlert className="size-4 shrink-0 text-amber-400" aria-hidden />
                <span className="truncate text-foreground/85">{firstBlocker}</span>
                {blockers.length > 1 && (
                  <span className="shrink-0 font-mono text-xs text-muted-foreground">
                    +{blockers.length - 1} more
                  </span>
                )}
              </>
            ) : (
              summary && (
                <>
                  <CircleCheck className="size-4 shrink-0 text-primary" aria-hidden />
                  <span className="truncate font-mono text-xs tabular-nums text-muted-foreground">
                    {summary}
                  </span>
                </>
              )
            )}
          </p>
        </div>
        <div className="ml-auto flex flex-wrap gap-2">
          {showBack && (
            <Button type="button" variant="outline" onClick={onBack} disabled={isUploading}>
              Back to edit
            </Button>
          )}
          {currentStep === "review" ? (
            <Button
              type="button"
              variant="default"
              onClick={onUpload}
              disabled={!canAdvance || isUploading}
            >
              {isUploading && <Loader2 className="motion-safe:animate-spin" aria-hidden />}
              {isUploading ? "Uploading…" : "Upload model"}
            </Button>
          ) : (
            <Button
              type="button"
              variant="default"
              onClick={onNext}
              disabled={!canAdvance || isUploading}
            >
              Review upload
              <ArrowRight aria-hidden />
            </Button>
          )}
        </div>
      </div>
    </div>
  );
}
