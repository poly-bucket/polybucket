"use client";

import { useEffect } from "react";

export default function Error({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  useEffect(() => {
    console.error(error);
  }, [error]);

  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-gradient-to-br from-slate-900 via-slate-800 to-slate-900 px-4">
      <div className="text-center">
        <h2 className="text-2xl font-semibold text-white">
          Something went wrong
        </h2>
        <p className="mt-4 text-white/70">
          An unexpected error occurred. Please try again.
        </p>
        <button
          type="button"
          onClick={() => reset()}
          className="mt-8 inline-flex items-center rounded-md border border-white/20 bg-white/10 px-4 py-2 text-sm font-medium text-white transition-colors hover:border-white/30 hover:bg-white/15"
        >
          Try again
        </button>
      </div>
    </div>
  );
}
