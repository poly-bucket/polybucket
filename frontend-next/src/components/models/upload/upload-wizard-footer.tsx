"use client";

import { Button } from "@/components/primitives/button";
import type { UploadWizardStep } from "./use-upload-wizard";

interface UploadWizardFooterProps {
  currentStep: UploadWizardStep;
  canAdvance: boolean;
  isUploading: boolean;
  onBack: () => void;
  onNext: () => void;
  onUpload: () => void;
  onCancel: () => void;
}

export function UploadWizardFooter({
  currentStep,
  canAdvance,
  isUploading,
  onBack,
  onNext,
  onUpload,
  onCancel,
}: UploadWizardFooterProps) {
  const showBack = currentStep !== "files";

  return (
    <div
      className="sticky bottom-0 z-10 -mx-4 mt-8 border-t border-white/10 bg-background/80 px-4 py-4 backdrop-blur-md sm:-mx-0 sm:rounded-lg sm:border sm:px-6"
    >
      <div className="flex flex-wrap items-center justify-between gap-3">
        <Button type="button" variant="ghost" onClick={onCancel} disabled={isUploading}>
          Cancel
        </Button>
        <div className="flex flex-wrap gap-3">
          {showBack && (
            <Button type="button" variant="outline" onClick={onBack} disabled={isUploading}>
              Back
            </Button>
          )}
          {currentStep === "review" ? (
            <Button type="button" onClick={onUpload} disabled={!canAdvance || isUploading}>
              {isUploading ? "Uploading..." : "Upload Model"}
            </Button>
          ) : (
            <Button type="button" onClick={onNext} disabled={!canAdvance || isUploading}>
              {currentStep === "files" ? "Continue" : "Continue to Review"}
            </Button>
          )}
        </div>
      </div>
    </div>
  );
}
