"use client";

import * as React from "react";
import { Button } from "@/components/primitives/button";
import { cn } from "@/lib/utils";

interface SettingsFooterProps {
  onSave?: () => void;
  onCancel?: () => void;
  onReset?: () => void;
  isSaving?: boolean;
  isDirty?: boolean;
  submit?: boolean;
  className?: string;
}

export function SettingsFooter({
  onSave,
  onCancel,
  onReset,
  isSaving = false,
  isDirty = false,
  submit = false,
  className,
}: SettingsFooterProps) {
  return (
    <div
      className={cn(
        "flex flex-wrap items-center gap-3 pt-4 border-t border-white/10 mt-6",
        className
      )}
    >
      {onSave && (
        <Button
          type={submit ? "submit" : "button"}
          onClick={submit ? undefined : onSave}
          disabled={isSaving || !isDirty}
          size="sm"
        >
          {isSaving ? "Saving..." : "Save changes"}
        </Button>
      )}
      {onCancel && isDirty && (
        <Button variant="outline" onClick={onCancel} size="sm" disabled={isSaving}>
          Cancel
        </Button>
      )}
      {onReset && (
        <Button variant="ghost" onClick={onReset} size="sm" disabled={isSaving}>
          Reset
        </Button>
      )}
    </div>
  );
}
