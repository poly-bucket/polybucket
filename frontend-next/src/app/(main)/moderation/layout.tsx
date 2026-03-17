"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/contexts/AuthContext";
import { useModerationNavItems } from "@/lib/plugins";
import { PanelLayout } from "@/components/layout/panel-layout";
import { Skeleton } from "@/components/ui/skeleton";

export default function ModerationLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const router = useRouter();
  const { user, isLoading } = useAuth();
  const navItems = useModerationNavItems();

  const isModeratorOrAdmin = !!user?.roles?.some(
    (r) =>
      r?.toLowerCase() === "admin" || r?.toLowerCase() === "moderator"
  );

  useEffect(() => {
    if (isLoading) return;
    if (!user) {
      router.replace("/login?redirect=/moderation");
      return;
    }
    if (!isModeratorOrAdmin) {
      router.replace("/dashboard");
      return;
    }
  }, [user, isLoading, isModeratorOrAdmin, router]);

  if (isLoading) {
    return (
      <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
        <Skeleton className="mb-8 h-8 w-48" />
        <div className="flex gap-8">
          <div className="w-56 space-y-4">
            <Skeleton className="h-8" />
            <Skeleton className="h-8" />
            <Skeleton className="h-8" />
            <Skeleton className="h-8" />
          </div>
          <div className="flex-1 space-y-4">
            <Skeleton className="h-32" />
            <Skeleton className="h-32" />
          </div>
        </div>
      </div>
    );
  }

  if (!user || !isModeratorOrAdmin) {
    return null;
  }

  if (navItems.length === 0) {
    return (
      <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
        <h1 className="mb-4 text-2xl font-semibold text-white">
          Moderation Control Panel
        </h1>
        <p className="text-white/70">
          You do not have permission to access any moderation sections.
        </p>
      </div>
    );
  }

  return (
    <PanelLayout
      title="Moderation Control Panel"
      description="Manage reports, banned users, and audit logs"
      navItems={navItems}
    >
      {children}
    </PanelLayout>
  );
}
