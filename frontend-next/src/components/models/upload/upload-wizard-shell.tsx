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
  onBack: () => void;
  onNext: () => void;
  onUpload: () => void;
  onCancel: () => void;
  children: ReactNode;
}

export function UploadWizardShell({
  currentStep,
  stepCompletion,
  isStepReachable,
  onStepClick,
  canAdvance,
  isUploading,
  onBack,
  onNext,
  onUpload,
  onCancel,
  children,
}: UploadWizardShellProps) {
  return (
    <div className="mx-auto w-full max-w-4xl space-y-6 px-4 py-6 sm:space-y-8 sm:py-8">
      <div className="space-y-4">
        <h1 className="text-xl font-semibold tracking-tight text-white sm:text-2xl">
          Upload New Model
        </h1>
        <UploadStepper
          currentStep={currentStep}
          stepCompletion={stepCompletion}
          isStepReachable={isStepReachable}
          onStepClick={onStepClick}
        />
      </div>
      <div className="pb-2">{children}</div>
      <UploadWizardFooter
        currentStep={currentStep}
        canAdvance={canAdvance}
        isUploading={isUploading}
        onBack={onBack}
        onNext={onNext}
        onUpload={onUpload}
        onCancel={onCancel}
      />
    </div>
  );
}
