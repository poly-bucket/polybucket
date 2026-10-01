"use client";

import { cn } from "@/lib/utils";
import type { UploadWizardStep } from "./use-upload-wizard";
import { UPLOAD_WIZARD_STEPS } from "./use-upload-wizard";

const STEP_LABELS: Record<UploadWizardStep, string> = {
  files: "Files",
  details: "Details",
  review: "Review",
};

interface UploadStepperProps {
  currentStep: UploadWizardStep;
  stepCompletion: Record<UploadWizardStep, boolean>;
  isStepReachable: (step: UploadWizardStep) => boolean;
  onStepClick: (step: UploadWizardStep) => void;
}

export function UploadStepper({
  currentStep,
  stepCompletion,
  isStepReachable,
  onStepClick,
}: UploadStepperProps) {
  return (
    <nav aria-label="Upload progress" className="flex flex-wrap items-center gap-2 sm:gap-3">
      {UPLOAD_WIZARD_STEPS.map((step, index) => {
        const reachable = isStepReachable(step);
        const isCurrent = currentStep === step;
        const isComplete = stepCompletion[step] && !isCurrent;

        return (
          <div key={step} className="flex items-center gap-2 sm:gap-3">
            <button
              type="button"
              disabled={!reachable && !isCurrent}
              onClick={() => reachable && onStepClick(step)}
              aria-current={isCurrent ? "step" : undefined}
              className={cn(
                "flex items-center gap-2 rounded-full border px-3 py-1.5 text-xs sm:text-sm transition-colors",
                isCurrent && "border-primary bg-primary text-primary-foreground",
                !isCurrent &&
                  isComplete &&
                  "border-primary/50 text-foreground hover:border-primary/70",
                !isCurrent &&
                  !isComplete &&
                  reachable &&
                  "border-white/20 text-muted-foreground hover:border-white/40",
                !reachable && !isCurrent && "border-white/10 text-muted-foreground/50 cursor-not-allowed"
              )}
            >
              <span
                className={cn(
                  "flex h-5 w-5 items-center justify-center rounded-full text-[11px] font-medium",
                  isCurrent ? "bg-primary-foreground/20" : "bg-white/10"
                )}
              >
                {index + 1}
              </span>
              {STEP_LABELS[step]}
            </button>
            {index < UPLOAD_WIZARD_STEPS.length - 1 && (
              <span className="hidden sm:inline text-white/20" aria-hidden>
                /
              </span>
            )}
          </div>
        );
      })}
    </nav>
  );
}
