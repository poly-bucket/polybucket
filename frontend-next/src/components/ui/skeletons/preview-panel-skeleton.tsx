import { Skeleton } from "@/components/ui/glass/skeleton";
import { cn } from "@/lib/utils";

export interface PreviewPanelSkeletonProps {
  className?: string;
  minHeight?: string;
}

export function PreviewPanelSkeleton({
  className,
  minHeight = "min-h-[300px]",
}: PreviewPanelSkeletonProps) {
  return (
    <div
      className={cn(
        "flex h-full w-full items-center justify-center bg-white/5",
        minHeight,
        className
      )}
    >
      <Skeleton className={cn("h-full w-full rounded-lg", minHeight)} />
    </div>
  );
}
