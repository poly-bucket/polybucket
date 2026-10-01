"use client";

import { useCallback, useMemo, useState } from "react";
import type { ModelMetadataFormValues } from "@/lib/models/model-metadata";
import { titleFromFileName } from "@/lib/models/model-metadata";
import type { UploadFileType } from "../upload-shared";

export const UPLOAD_WIZARD_STEPS = ["files", "details", "review"] as const;
export type UploadWizardStep = (typeof UPLOAD_WIZARD_STEPS)[number];

export interface UploadWizardContext {
  fileCount: number;
  settingsLoaded: boolean;
  metadataValid: boolean;
  isFormDirty: boolean;
}

function stepIndex(step: UploadWizardStep): number {
  return UPLOAD_WIZARD_STEPS.indexOf(step);
}

export function canAdvanceFromStep(
  step: UploadWizardStep,
  ctx: UploadWizardContext
): boolean {
  switch (step) {
    case "files":
      return ctx.settingsLoaded && ctx.fileCount >= 1;
    case "details":
      return ctx.metadataValid;
    case "review":
      return ctx.metadataValid && ctx.fileCount >= 1;
    default:
      return false;
  }
}

export function isStepReachable(
  target: UploadWizardStep,
  ctx: UploadWizardContext
): boolean {
  const targetIdx = stepIndex(target);
  for (let i = 0; i < targetIdx; i++) {
    const step = UPLOAD_WIZARD_STEPS[i];
    if (!canAdvanceFromStep(step, ctx)) {
      return false;
    }
  }
  return true;
}

export function suggestTitleFromFiles(
  fileNames: string[],
  getFileType: (name: string) => UploadFileType
): string | null {
  const candidate = fileNames.find((name) => {
    const type = getFileType(name);
    return type === "3d" || type === "image" || type === "pdf";
  });
  if (!candidate) {
    const fallback = fileNames.find((name) => getFileType(name) !== "markdown");
    if (!fallback) return null;
    return titleFromFileName(fallback);
  }
  return titleFromFileName(candidate);
}

interface UseUploadWizardOptions {
  fileCount: number;
  settingsLoaded: boolean;
  metadataValid: boolean;
  isFormDirty: boolean;
  uploadedFileNames: string[];
  getFileType: (name: string) => UploadFileType;
  currentTitle: string;
  onAutoTitle: (title: string) => void;
}

export function useUploadWizard({
  fileCount,
  settingsLoaded,
  metadataValid,
  isFormDirty,
  uploadedFileNames,
  getFileType,
  currentTitle,
  onAutoTitle,
}: UseUploadWizardOptions) {
  const [currentStep, setCurrentStep] = useState<UploadWizardStep>("files");

  const ctx: UploadWizardContext = useMemo(
    () => ({
      fileCount,
      settingsLoaded,
      metadataValid,
      isFormDirty,
    }),
    [fileCount, settingsLoaded, metadataValid, isFormDirty]
  );

  const isDirty = fileCount > 0 || isFormDirty;

  const goToStep = useCallback(
    (step: UploadWizardStep) => {
      if (!isStepReachable(step, ctx)) return;
      setCurrentStep(step);
    },
    [ctx]
  );

  const goNext = useCallback(() => {
    const idx = stepIndex(currentStep);
    if (idx >= UPLOAD_WIZARD_STEPS.length - 1) return;
    if (!canAdvanceFromStep(currentStep, ctx)) return;
    setCurrentStep(UPLOAD_WIZARD_STEPS[idx + 1]);
  }, [currentStep, ctx]);

  const goBack = useCallback(() => {
    const idx = stepIndex(currentStep);
    if (idx <= 0) return;
    setCurrentStep(UPLOAD_WIZARD_STEPS[idx - 1]);
  }, [currentStep]);

  const applyAutoTitleIfNeeded = useCallback(
    (previousCount: number, nextFileNames: string[]) => {
      if (currentTitle.trim().length > 0) return;
      if (nextFileNames.length === 0) return;
      if (previousCount > 0 && nextFileNames.length <= previousCount) return;
      const suggested = suggestTitleFromFiles(nextFileNames, getFileType);
      if (suggested) onAutoTitle(suggested);
    },
    [currentTitle, getFileType, onAutoTitle]
  );

  const stepCompletion = useMemo(() => {
    return {
      files: canAdvanceFromStep("files", ctx),
      details: canAdvanceFromStep("details", ctx),
      review: canAdvanceFromStep("review", ctx),
    };
  }, [ctx]);

  return {
    currentStep,
    setCurrentStep,
    goToStep,
    goNext,
    goBack,
    isDirty,
    stepCompletion,
    canAdvanceFromCurrent: canAdvanceFromStep(currentStep, ctx),
    isStepReachable: (step: UploadWizardStep) => isStepReachable(step, ctx),
    applyAutoTitleIfNeeded,
  };
}
