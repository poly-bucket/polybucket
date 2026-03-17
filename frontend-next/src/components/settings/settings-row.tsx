"use client";

import * as React from "react";
import { cn } from "@/lib/utils";

interface SettingsRowProps {
  label: string;
  description?: string;
  children: React.ReactNode;
  className?: string;
}

export function SettingsRow({
  label,
  description,
  children,
  className,
}: SettingsRowProps) {
  return (
    <div
      className={cn(
        "flex flex-col gap-1 sm:flex-row sm:items-center sm:justify-between py-3 border-b border-white/10 last:border-b-0",
        className
      )}
    >
      <div className="flex-1 min-w-0">
        <p className="text-sm font-medium text-white">{label}</p>
        {description && (
          <p className="text-xs text-white/60 mt-0.5">{description}</p>
        )}
      </div>
      <div className="flex-shrink-0 sm:pl-4">{children}</div>
    </div>
  );
}
