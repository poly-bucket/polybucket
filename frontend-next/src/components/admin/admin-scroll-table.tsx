"use client";

import * as React from "react";
import { cn } from "@/lib/utils";

interface AdminScrollTableProps {
  children: React.ReactNode;
  className?: string;
  tableClassName?: string;
  footer?: React.ReactNode;
}

export function AdminScrollTable({
  children,
  className,
  tableClassName,
  footer,
}: AdminScrollTableProps) {
  return (
    <div
      className={cn(
        "w-full min-w-0 rounded-lg border border-white/10 overflow-hidden",
        className
      )}
    >
      <div className="max-w-full overflow-x-auto">
        <table className={cn("w-full", tableClassName)}>{children}</table>
      </div>
      {footer}
    </div>
  );
}
