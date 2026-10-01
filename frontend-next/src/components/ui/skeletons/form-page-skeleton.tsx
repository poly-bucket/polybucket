import { Card } from "@/components/primitives/card";
import { Skeleton } from "@/components/ui/glass/skeleton";
import { SettingsSectionsSkeleton } from "@/components/ui/skeletons/settings-sections-skeleton";
import { cn } from "@/lib/utils";

export function ProfileSettingsFormSkeleton() {
  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Skeleton className="h-20 w-20 rounded-full" />
        <div className="space-y-2">
          <Skeleton className="h-4 w-32" />
          <Skeleton className="h-9 w-28 rounded-md" />
        </div>
      </div>
      {Array.from({ length: 5 }).map((_, i) => (
        <div key={i} className="space-y-2">
          <Skeleton className="h-4 w-24" />
          <Skeleton className="h-10 w-full max-w-md rounded-md" />
        </div>
      ))}
    </div>
  );
}

export function LayoutPreferencesSkeleton() {
  return (
    <div className="space-y-8">
      <SettingsSectionsSkeleton
        sections={2}
        togglesPerSection={3}
        showPageTitle={false}
      />
      <div className="grid grid-cols-3 gap-3 sm:grid-cols-3">
        {Array.from({ length: 3 }).map((_, i) => (
          <Skeleton key={i} className="h-24 rounded-lg" />
        ))}
      </div>
    </div>
  );
}

export function AuthCardSkeleton({ className }: { className?: string }) {
  return (
    <div className={cn("mx-auto w-full max-w-md space-y-4 rounded-xl border border-white/20 p-6", className)}>
      <Skeleton className="mx-auto h-8 w-40" />
      <Skeleton className="h-4 w-full" />
      <Skeleton className="h-10 w-full rounded-md" />
      <Skeleton className="h-10 w-full rounded-md" />
      <Skeleton className="h-10 w-full rounded-md" />
    </div>
  );
}

export function RouteContentSkeleton() {
  return (
    <div className="mx-auto max-w-6xl px-4 py-8 sm:px-6 lg:px-8">
      <Skeleton className="mb-8 h-8 w-48" />
      <div className="space-y-4">
        <Skeleton className="h-24 rounded-lg" />
        <Skeleton className="h-32 rounded-lg" />
      </div>
    </div>
  );
}

export function UploadWizardSkeleton() {
  return (
    <div className="mx-auto max-w-4xl space-y-8 px-4 py-8">
      <Skeleton className="h-8 w-48" />
      <div className="flex gap-2">
        {Array.from({ length: 4 }).map((_, i) => (
          <Skeleton key={i} className="h-2 flex-1 rounded-full" />
        ))}
      </div>
      <Skeleton className="h-64 w-full rounded-xl" />
    </div>
  );
}

export function CommentSkeleton({ count = 3 }: { count?: number }) {
  return (
    <div className="space-y-4">
      {Array.from({ length: count }).map((_, i) => (
        <div key={i} className="flex gap-3">
          <Skeleton className="h-9 w-9 shrink-0 rounded-full" />
          <div className="flex-1 space-y-2 rounded-lg border border-white/10 p-3">
            <Skeleton className="h-4 w-28" />
            <Skeleton className="h-3 w-full" />
            <Skeleton className="h-3 w-4/5" />
          </div>
        </div>
      ))}
    </div>
  );
}

export function CheckboxListSkeleton({ rows = 5 }: { rows?: number }) {
  return (
    <div className="space-y-2">
      {Array.from({ length: rows }).map((_, i) => (
        <div key={i} className="flex items-center gap-3 py-1">
          <Skeleton className="h-4 w-4 rounded" />
          <Skeleton className="h-4 flex-1 max-w-[200px]" />
        </div>
      ))}
    </div>
  );
}

export function ModelQueueCardSkeleton({ count = 3 }: { count?: number }) {
  return (
    <div className="space-y-4">
      {Array.from({ length: count }).map((_, i) => (
        <div
          key={i}
          className="flex gap-4 rounded-lg border border-white/10 p-4"
        >
          <Skeleton className="h-20 w-20 shrink-0 rounded-lg" />
          <div className="flex flex-1 flex-col gap-2">
            <Skeleton className="h-5 w-48" />
            <Skeleton className="h-4 w-32" />
            <div className="mt-auto flex gap-2">
              <Skeleton className="h-9 w-24 rounded-md" />
              <Skeleton className="h-9 w-24 rounded-md" />
            </div>
          </div>
        </div>
      ))}
    </div>
  );
}

export function ThemeTabSkeleton() {
  return (
    <div className="space-y-6">
      <Skeleton className="h-8 w-32" />
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        {Array.from({ length: 4 }).map((_, i) => (
          <Skeleton key={i} className="h-28 rounded-lg" />
        ))}
      </div>
      <Card variant="glass" className="border-white/20 p-6">
        <Skeleton className="mb-4 h-6 w-36" />
        <div className="flex flex-wrap gap-3">
          {Array.from({ length: 8 }).map((_, i) => (
            <Skeleton key={i} className="h-10 w-10 rounded-full" />
          ))}
        </div>
      </Card>
    </div>
  );
}

export function ChartBlockSkeleton() {
  return (
    <Card variant="glass" className="border-white/20 p-6">
      <Skeleton className="mb-4 h-6 w-40" />
      <Skeleton className="h-48 w-full rounded-lg" />
    </Card>
  );
}
