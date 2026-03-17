"use client";

import { useEffect, useRef } from "react";
import { Button } from "@/components/primitives/button";
import { cn } from "@/lib/utils";

interface DeleteModelDialogProps {
  isOpen: boolean;
  modelName: string;
  isDeleting: boolean;
  onClose: () => void;
  onConfirm: () => void;
}

export function DeleteModelDialog({
  isOpen,
  modelName,
  isDeleting,
  onClose,
  onConfirm,
}: DeleteModelDialogProps) {
  const dialogRef = useRef<HTMLDivElement>(null);
  const previousActiveElement = useRef<HTMLElement | null>(null);

  useEffect(() => {
    if (!isOpen) return;
    previousActiveElement.current = document.activeElement as HTMLElement | null;
    const focusable = dialogRef.current?.querySelectorAll(
      'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
    );
    const first = focusable?.[0] as HTMLElement | undefined;
    first?.focus();
    return () => {
      previousActiveElement.current?.focus();
    };
  }, [isOpen]);

  useEffect(() => {
    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === "Escape" && isOpen && !isDeleting) onClose();
    };
    window.addEventListener("keydown", handleEscape);
    return () => window.removeEventListener("keydown", handleEscape);
  }, [isOpen, isDeleting, onClose]);

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div
        className="absolute inset-0 bg-black/40 backdrop-blur-sm"
        onClick={() => !isDeleting && onClose()}
        aria-hidden
      />
      <div
        ref={dialogRef}
        role="dialog"
        aria-modal="true"
        aria-labelledby="delete-dialog-title"
        aria-describedby="delete-dialog-description"
        className={cn(
          "relative z-50 w-full max-w-md rounded-xl border border-white/20 bg-white/10 p-6 shadow-xl backdrop-blur-md",
          "mx-4"
        )}
      >
        <div className="mb-4 flex items-center gap-4">
          <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-full bg-red-500/20">
            <svg
              className="h-6 w-6 text-red-400"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z"
              />
            </svg>
          </div>
          <div>
            <h3
              id="delete-dialog-title"
              className="text-lg font-semibold text-white"
            >
              Delete Model
            </h3>
            <p className="text-sm text-white/60">This action cannot be undone</p>
          </div>
        </div>
        <p
          id="delete-dialog-description"
          className="mb-6 text-white/80"
        >
          Are you sure you want to delete <strong>{modelName}</strong>? This
          will permanently remove the model and all its files from the platform.
        </p>
        <div className="flex gap-3">
          <Button
            variant="outline"
            className="flex-1"
            onClick={onClose}
            disabled={isDeleting}
          >
            Cancel
          </Button>
          <Button
            variant="destructive"
            className="flex-1"
            onClick={onConfirm}
            disabled={isDeleting}
          >
            {isDeleting ? "Deleting..." : "Delete Model"}
          </Button>
        </div>
      </div>
    </div>
  );
}
