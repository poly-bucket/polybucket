"use client";

import { Check } from "lucide-react";
import { cn } from "@/lib/utils";
import type { UploadWizardStep } from "./use-upload-wizard";
import { UPLOAD_WIZARD_STEPS } from "./use-upload-wizard";

const STEP_LABELS: Record<UploadWizardStep, string> = {
  compose: "Files & details",
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
    <nav aria-label="Upload progress">
      <ol className="flex items-center gap-1 rounded-full border border-white/10 bg-white/[0.03] p-1">
        {UPLOAD_WIZARD_STEPS.map((step, index) => {
          const reachable = isStepReachable(step);
          const isCurrent = currentStep === step;
          const isComplete = stepCompletion[step] && !isCurrent;

          return (
            <li key={step} className="flex items-center gap-1">
              {index > 0 && <span className="h-px w-3 bg-white/15" aria-hidden />}
              <button
                type="button"
                disabled={!reachable && !isCurrent}
                onClick={() => reachable && onStepClick(step)}
                aria-current={isCurrent ? "step" : undefined}
                className={cn(
                  "flex items-center gap-2 rounded-full px-3 py-1 text-xs font-medium outline-none motion-safe:transition-colors focus-visible:ring-2 focus-visible:ring-primary/60 sm:text-sm",
                  isCurrent && "bg-primary text-primary-foreground",
                  !isCurrent && reachable && "text-foreground/80 hover:bg-white/[0.06]",
                  !reachable && !isCurrent && "cursor-not-allowed text-muted-foreground/50"
                )}
              >
                <span
                  className={cn(
                    "flex size-5 items-center justify-center rounded-full font-mono text-[11px]",
                    isCurrent ? "bg-primary-foreground/20" : "bg-white/10"
                  )}
                >
                  {isComplete ? <Check className="size-3" aria-hidden /> : index + 1}
                </span>
                {STEP_LABELS[step]}
              </button>
            </li>
          );
        })}
      </ol>
    </nav>
  );
}
