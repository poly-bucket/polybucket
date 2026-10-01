import { Card } from "@/components/primitives/card";
import { ModelCardSkeleton } from "@/components/models/model-card";
import { Skeleton } from "@/components/ui/glass/skeleton";

export function ProfileHeaderSkeleton() {
  return (
    <Card variant="glass" className="border-white/20 p-4">
      <div className="flex flex-col gap-4 sm:flex-row sm:gap-4">
        <Skeleton className="h-24 w-24 shrink-0 rounded-full sm:h-28 sm:w-28" />
        <div className="flex flex-1 flex-col gap-3">
          <Skeleton className="h-8 w-48" />
          <Skeleton className="h-4 w-32" />
          <div className="flex flex-wrap gap-4">
            <Skeleton className="h-4 w-20" />
            <Skeleton className="h-4 w-24" />
            <Skeleton className="h-4 w-28" />
          </div>
        </div>
      </div>
    </Card>
  );
}

export function ProfilePageSkeleton() {
  return (
    <div className="mx-auto max-w-7xl space-y-6 px-4 py-8 sm:px-6 lg:px-8">
      <ProfileHeaderSkeleton />
      <div className="flex gap-2 border-b border-white/10 pb-2">
        <Skeleton className="h-9 w-28 rounded-md" />
        <Skeleton className="h-9 w-32 rounded-md" />
      </div>
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        {Array.from({ length: 8 }).map((_, i) => (
          <ModelCardSkeleton key={i} />
        ))}
      </div>
    </div>
  );
}
