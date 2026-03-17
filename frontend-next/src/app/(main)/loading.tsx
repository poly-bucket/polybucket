export default function Loading() {
  return (
    <div className="flex min-h-[50vh] items-center justify-center">
      <div className="flex flex-col items-center gap-4">
        <div className="size-8 animate-spin rounded-full border-2 border-white/30 border-t-white" />
        <p className="text-white/70">Loading...</p>
      </div>
    </div>
  );
}
