export default function Loading() {
  return (
    <div className="mx-auto max-w-6xl px-4 py-8 sm:px-6 lg:px-8">
      <div className="mb-8 h-8 w-48 animate-pulse rounded bg-white/10" />
      <div className="space-y-4">
        <div className="h-24 animate-pulse rounded-lg bg-white/10" />
        <div className="h-32 animate-pulse rounded-lg bg-white/10" />
      </div>
    </div>
  );
}
