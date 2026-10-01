import { Skeleton } from "@/components/ui/glass/skeleton";
import { PreviewPanelSkeleton } from "@/components/ui/skeletons/preview-panel-skeleton";

export function ModelDetailsPageSkeleton() {
  return (
    <div className="mx-auto max-w-[95rem] px-3 py-4 sm:px-4 sm:py-6 md:px-6 lg:px-8 lg:py-8">
      <div className="mb-6 flex items-center gap-2">
        <Skeleton className="h-4 w-20" />
        <Skeleton className="h-4 w-4" />
        <Skeleton className="h-4 w-40" />
      </div>
      <div className="grid gap-6 lg:grid-cols-[1fr_320px] xl:grid-cols-[1fr_360px]">
        <div className="space-y-4">
          <PreviewPanelSkeleton minHeight="min-h-[320px] sm:min-h-[400px]" />
          <Skeleton className="h-10 w-full max-w-md" />
        </div>
        <div className="space-y-4">
          <Skeleton className="h-9 w-3/4" />
          <Skeleton className="h-5 w-1/2" />
          <div className="flex gap-3">
            <Skeleton className="h-9 w-24 rounded-md" />
            <Skeleton className="h-9 w-24 rounded-md" />
          </div>
          <div className="space-y-2 pt-2">
            <Skeleton className="h-4 w-full" />
            <Skeleton className="h-4 w-full" />
            <Skeleton className="h-4 w-4/5" />
          </div>
          <div className="space-y-2 border-t border-white/10 pt-4">
            <Skeleton className="h-5 w-24" />
            {Array.from({ length: 4 }).map((_, i) => (
              <Skeleton key={i} className="h-10 w-full rounded-md" />
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
