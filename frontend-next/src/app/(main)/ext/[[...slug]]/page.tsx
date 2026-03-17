"use client";

import { useParams } from "next/navigation";
import Link from "next/link";
import { usePluginRoutes } from "@/lib/plugins";

export default function ExtPage() {
  const params = useParams();
  const slug = params?.slug;
  const currentPath = Array.isArray(slug) ? slug.join("/") : String(slug ?? "");

  const routes = usePluginRoutes();
  const route = routes.find((r) => {
    const routePath = r.path.startsWith("/") ? r.path.slice(1) : r.path;
    return routePath === currentPath;
  });

  if (!route) {
    return (
      <div className="mx-auto max-w-2xl px-4 py-16 text-center">
        <h1 className="mb-4 text-2xl font-semibold text-white">Not Found</h1>
        <p className="mb-6 text-white/80">
          The page you are looking for does not exist.
        </p>
        <Link
          href="/dashboard"
          className="rounded-md border border-white/20 bg-white/10 px-4 py-2 text-sm text-white hover:bg-white/20"
        >
          Back to Dashboard
        </Link>
      </div>
    );
  }

  const Component = route.component;
  return <Component />;
}
