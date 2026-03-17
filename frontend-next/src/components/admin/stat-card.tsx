"use client";

import * as React from "react";
import type { LucideIcon } from "lucide-react";
import { cn } from "@/lib/utils";

interface StatCardProps {
  icon: LucideIcon;
  value: React.ReactNode;
  label: string;
  colorClass?: string;
  className?: string;
}

export function StatCard({
  icon: Icon,
  value,
  label,
  colorClass = "text-blue-400",
  className,
}: StatCardProps) {
  return (
    <div
      className={cn(
        "rounded-lg border border-white/10 glass-bg p-4",
        className
      )}
    >
      <div className="mb-2 flex items-center space-x-2">
        <Icon className={cn("h-5 w-5", colorClass)} />
        <div className={cn("text-2xl font-bold", colorClass)}>{value}</div>
      </div>
      <div className="text-sm text-white/60">{label}</div>
    </div>
  );
}
