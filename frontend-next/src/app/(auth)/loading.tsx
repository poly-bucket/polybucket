import { AuthCardSkeleton } from "@/components/ui/skeletons";

export default function Loading() {
  return (
    <div className="flex min-h-[50vh] items-center justify-center px-4">
      <AuthCardSkeleton />
    </div>
  );
}
