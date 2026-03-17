import Link from "next/link";

export default function NotFound() {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-gradient-to-br from-slate-900 via-slate-800 to-slate-900 px-4">
      <div className="text-center">
        <h1 className="text-6xl font-bold text-white">404</h1>
        <p className="mt-4 text-lg text-white/70">Page not found</p>
        <p className="mt-2 text-sm text-white/50">
          The page you are looking for does not exist or has been moved.
        </p>
        <Link
          href="/dashboard"
          className="mt-8 inline-flex items-center rounded-md border border-white/20 bg-white/10 px-4 py-2 text-sm font-medium text-white transition-colors hover:border-white/30 hover:bg-white/15"
        >
          Return to Dashboard
        </Link>
      </div>
    </div>
  );
}
