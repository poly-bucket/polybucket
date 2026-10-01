import { Skeleton } from "@/components/ui/glass/skeleton";
import { cn } from "@/lib/utils";

export interface ListRowSkeletonProps {
  count?: number;
  showIcon?: boolean;
  className?: string;
}

export function ListRowSkeleton({
  count = 4,
  showIcon = true,
  className,
}: ListRowSkeletonProps) {
  return (
    <div className={cn("space-y-2 p-2", className)}>
      {Array.from({ length: count }).map((_, i) => (
        <div key={i} className="flex items-center gap-3 rounded-md px-2 py-2">
          {showIcon && <Skeleton className="h-8 w-8 shrink-0 rounded-md" />}
          <div className="flex min-w-0 flex-1 flex-col gap-1.5">
            <Skeleton className="h-4 w-3/5 max-w-[200px]" />
            <Skeleton className="h-3 w-4/5 max-w-[280px]" />
          </div>
        </div>
      ))}
    </div>
  );
}
