"use client";

import type { ReactNode } from "react";
import { UploadStepper } from "./upload-stepper";
import { UploadWizardFooter } from "./upload-wizard-footer";
import type { UploadWizardStep } from "./use-upload-wizard";

interface UploadWizardShellProps {
  currentStep: UploadWizardStep;
  stepCompletion: Record<UploadWizardStep, boolean>;
  isStepReachable: (step: UploadWizardStep) => boolean;
  onStepClick: (step: UploadWizardStep) => void;
  canAdvance: boolean;
  isUploading: boolean;
  blockers?: string[];
  summary?: string;
  onBack: () => void;
  onNext: () => void;
  onUpload: () => void;
  onCancel: () => void;
  children: ReactNode;
}

const STEP_INTROS: Record<UploadWizardStep, string> = {
  compose: "Add your files, then name and describe the model.",
  review: "Check how the model will appear, then upload it.",
};

export function UploadWizardShell({
  currentStep,
  stepCompletion,
  isStepReachable,
  onStepClick,
  canAdvance,
  isUploading,
  blockers,
  summary,
  onBack,
  onNext,
  onUpload,
  onCancel,
  children,
}: UploadWizardShellProps) {
  return (
    <div className="mx-auto w-full max-w-6xl px-4 py-6 sm:py-8">
      <header className="mb-6 flex flex-wrap items-end justify-between gap-4 sm:mb-8">
        <div className="space-y-1">
          <h1 className="text-2xl font-semibold tracking-tight text-white sm:text-[1.75rem]">
            Upload model
          </h1>
          <p className="text-sm text-muted-foreground">{STEP_INTROS[currentStep]}</p>
        </div>
        <UploadStepper
          currentStep={currentStep}
          stepCompletion={stepCompletion}
          isStepReachable={isStepReachable}
          onStepClick={onStepClick}
        />
      </header>
      <div className="pb-2">{children}</div>
      <UploadWizardFooter
        currentStep={currentStep}
        canAdvance={canAdvance}
        isUploading={isUploading}
        blockers={blockers}
        summary={summary}
        onBack={onBack}
        onNext={onNext}
        onUpload={onUpload}
        onCancel={onCancel}
      />
    </div>
  );
}
