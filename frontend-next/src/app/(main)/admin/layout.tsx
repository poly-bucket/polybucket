"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/contexts/AuthContext";
import { useAdminNavItems } from "@/lib/plugins";
import { PanelLayout } from "@/components/layout/panel-layout";
import { Skeleton } from "@/components/ui/skeleton";

export default function AdminLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const router = useRouter();
  const { user, isLoading } = useAuth();
  const navItems = useAdminNavItems();

  const isAdmin = !!user?.roles?.some(
    (r) => r?.toLowerCase() === "admin"
  );

  useEffect(() => {
    if (isLoading) return;
    if (!user) {
      router.replace("/login?redirect=/admin");
      return;
    }
    if (!isAdmin) {
      router.replace("/dashboard");
      return;
    }
  }, [user, isLoading, isAdmin, router]);

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

  if (!user || !isAdmin) {
    return null;
  }

  if (navItems.length === 0) {
    return (
      <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
        <h1 className="mb-4 text-2xl font-semibold text-white">
          Admin Control Panel
        </h1>
        <p className="text-white/70">
          You do not have permission to access any admin sections.
        </p>
      </div>
    );
  }

  return (
    <PanelLayout
      title="Admin Control Panel"
      description="Manage users, roles, models, and system settings"
      navItems={navItems}
    >
      {children}
    </PanelLayout>
  );
}
