import { Skeleton } from "@/components/ui/glass/skeleton";
import { cn } from "@/lib/utils";

export type TableRowsSkeletonVariant = "default" | "users" | "audit";

export interface TableRowsSkeletonProps {
  rows?: number;
  variant?: TableRowsSkeletonVariant;
  className?: string;
}

export function TableRowsSkeleton({
  rows = 6,
  variant = "default",
  className,
}: TableRowsSkeletonProps) {
  return (
    <div className={cn("divide-y divide-white/10", className)}>
      {Array.from({ length: rows }).map((_, i) => (
        <div
          key={i}
          className="flex items-center gap-4 px-4 py-3"
        >
          {variant === "users" && (
            <>
              <Skeleton className="h-10 w-10 shrink-0 rounded-full" />
              <div className="min-w-0 flex-1 space-y-1.5">
                <Skeleton className="h-4 w-32" />
                <Skeleton className="h-3 w-48" />
              </div>
              <Skeleton className="h-6 w-16 rounded-full" />
              <Skeleton className="h-6 w-14 rounded-full" />
              <Skeleton className="h-4 w-20" />
              <Skeleton className="h-4 w-20" />
              <div className="flex gap-2">
                <Skeleton className="h-8 w-16 rounded-md" />
                <Skeleton className="h-8 w-20 rounded-md" />
              </div>
            </>
          )}
          {variant === "audit" && (
            <>
              <Skeleton className="h-4 w-36" />
              <Skeleton className="h-4 w-24" />
              <Skeleton className="h-4 flex-1 max-w-md" />
            </>
          )}
          {variant === "default" && (
            <>
              <Skeleton className="h-4 w-24" />
              <Skeleton className="h-4 w-32" />
              <Skeleton className="h-4 w-20" />
              <Skeleton className="h-4 w-16" />
              <Skeleton className="h-4 flex-1" />
            </>
          )}
        </div>
      ))}
    </div>
  );
}
