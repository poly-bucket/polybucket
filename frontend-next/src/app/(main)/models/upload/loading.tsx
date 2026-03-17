export default function Loading() {
  return (
    <div className="mx-auto max-w-4xl px-4 py-8">
      <div className="space-y-8">
        <div className="h-8 w-48 animate-pulse rounded bg-white/10" />
        <div className="h-64 animate-pulse rounded-lg bg-white/10" />
      </div>
    </div>
  );
}
