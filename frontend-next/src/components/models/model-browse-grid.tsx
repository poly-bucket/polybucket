"use client";

import { cn } from "@/lib/utils";
import {
  useModelBrowseLayout,
} from "@/lib/layout/use-model-browse-layout";
import type { ResolvedModelBrowseLayout } from "@/lib/layout/model-browse-layout";

interface ModelBrowseGridProps {
  children: React.ReactNode;
  layout?: ResolvedModelBrowseLayout;
  className?: string;
}

export function ModelBrowseGrid({ children, layout, className }: ModelBrowseGridProps) {
  const resolved = useModelBrowseLayout();
  const active = layout ?? resolved;

  return (
    <div
      className={cn(active.containerClassName, active.gapClassName, className)}
    >
      {children}
    </div>
  );
}