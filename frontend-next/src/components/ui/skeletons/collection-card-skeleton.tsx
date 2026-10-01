import { Card } from "@/components/primitives/card";
import { Skeleton } from "@/components/ui/glass/skeleton";

export function CollectionCardSkeleton() {
  return (
    <Card
      variant="glass"
      className="flex h-80 flex-col overflow-hidden border-white/20 sm:h-96"
    >
      <Skeleton className="h-40 flex-shrink-0 sm:h-48" />
      <div className="flex flex-1 flex-col gap-2 p-4">
        <Skeleton className="h-5 w-3/4" />
        <Skeleton className="h-3 w-1/2" />
        <Skeleton className="mt-auto h-3 w-24" />
      </div>
    </Card>
  );
}

export function CollectionCardSkeletonGrid({
  count = 8,
}: {
  count?: number;
}) {
  return (
    <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
      {Array.from({ length: count }).map((_, i) => (
        <CollectionCardSkeleton key={i} />
      ))}
    </div>
  );
}
