"use client";

import * as React from "react";
import { Input } from "@/components/primitives/input";
import { cn } from "@/lib/utils";

interface SettingsFieldProps {
  label: string;
  description?: string;
  error?: string;
  children?: React.ReactNode;
  className?: string;
}

export function SettingsField({
  label,
  description,
  error,
  children,
  className,
}: SettingsFieldProps) {
  return (
    <div
      className={cn(
        "flex flex-col gap-2 py-3 border-b border-white/10 last:border-b-0",
        className
      )}
    >
      <div>
        <label className="text-sm font-medium text-white">{label}</label>
        {description && (
          <p className="text-xs text-white/60 mt-0.5">{description}</p>
        )}
      </div>
      <div className="mt-1">{children}</div>
      {error && <p className="text-xs text-red-400">{error}</p>}
    </div>
  );
}
