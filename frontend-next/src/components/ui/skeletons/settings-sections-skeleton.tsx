import { Skeleton } from "@/components/ui/glass/skeleton";
import { Card } from "@/components/primitives/card";
import { cn } from "@/lib/utils";

export interface SettingsSectionsSkeletonProps {
  sections?: number;
  togglesPerSection?: number;
  showPageTitle?: boolean;
  pageTitleWidth?: string;
  className?: string;
}

export function SettingsSectionsSkeleton({
  sections = 2,
  togglesPerSection = 4,
  showPageTitle = true,
  pageTitleWidth = "w-48",
  className,
}: SettingsSectionsSkeletonProps) {
  return (
    <div className={cn("space-y-6", className)}>
      {showPageTitle && (
        <Skeleton className={cn("h-8", pageTitleWidth)} />
      )}
      {Array.from({ length: sections }).map((_, s) => (
        <Card key={s} variant="glass" className="border-white/20 p-6">
          <Skeleton className="mb-2 h-6 w-40" />
          <Skeleton className="mb-6 h-4 w-64 max-w-full" />
          <div className="space-y-4">
            {Array.from({ length: togglesPerSection }).map((_, t) => (
              <div
                key={t}
                className="flex items-center justify-between gap-4 border-b border-white/5 pb-4 last:border-0 last:pb-0"
              >
                <div className="flex-1 space-y-1.5">
                  <Skeleton className="h-4 w-36" />
                  <Skeleton className="h-3 w-56 max-w-full" />
                </div>
                <Skeleton className="h-6 w-11 rounded-full" />
              </div>
            ))}
          </div>
        </Card>
      ))}
    </div>
  );
}
