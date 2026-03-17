"use client";

import { useEffect, useState, useCallback } from "react";
import { useParams, useSearchParams, useRouter } from "next/navigation";
import { notFound } from "next/navigation";
import { Lock } from "lucide-react";
import { useAuth } from "@/contexts/AuthContext";
import {
  fetchUserProfile,
  type UserProfileData,
} from "@/lib/services/userProfileService";
import { useProfileTabs, PluginBoundary } from "@/lib/plugins";
import { ProfileHeader } from "./profile-header";

export function ProfilePageContent() {
  const params = useParams();
  const searchParams = useSearchParams();
  const router = useRouter();
  const { user: currentUser } = useAuth();
  const idOrUsername = params?.idOrUsername as string | undefined;

  const page = Math.max(1, parseInt(searchParams.get("page") ?? "1", 10) || 1);
  const q = searchParams.get("q") ?? "";

  const profileTabs = useProfileTabs();
  const tabFromUrl = searchParams.get("tab");
  const activeTabId =
    tabFromUrl && profileTabs.some((t) => t.id === tabFromUrl)
      ? tabFromUrl
      : profileTabs[0]?.id ?? null;
  const activeTab = profileTabs.find((t) => t.id === activeTabId);

  const [profile, setProfile] = useState<UserProfileData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const setUrlState = useCallback(
    (updates: { tab?: string; page?: number; q?: string }) => {
      const next = new URLSearchParams(searchParams.toString());
      if (updates.tab !== undefined) next.set("tab", updates.tab);
      if (updates.page !== undefined) next.set("page", String(updates.page));
      if (updates.q !== undefined) next.set("q", updates.q);
      router.replace(`?${next.toString()}`, { scroll: false });
    },
    [searchParams, router]
  );

  useEffect(() => {
    if (!idOrUsername) return;
    setLoading(true);
    setError(null);
    fetchUserProfile(idOrUsername)
      .then(setProfile)
      .catch((err: unknown) => {
        const status = (err as { status?: number })?.status;
        if (status === 404) {
          notFound();
        }
        const msg =
          (err as { result?: { message?: string }; message?: string })?.result
            ?.message ??
          (err as { message?: string })?.message ??
          "Failed to load user profile";
        setError(msg);
      })
      .finally(() => setLoading(false));
  }, [idOrUsername]);

  if (!idOrUsername) {
    notFound();
  }

  if (loading) {
    return (
      <div className="mx-auto flex min-h-96 max-w-7xl items-center justify-center px-4 py-8 sm:px-6 lg:px-8">
        <div className="h-12 w-12 animate-spin rounded-full border-2 border-white/30 border-t-white" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="mx-auto flex min-h-96 max-w-7xl flex-col items-center justify-center px-4 py-8 sm:px-6 lg:px-8">
        <p className="mb-4 text-red-400">{error}</p>
        <button
          type="button"
          onClick={() => window.location.reload()}
          className="rounded-md border border-white/20 bg-white/10 px-4 py-2 text-sm text-white hover:bg-white/20"
        >
          Retry
        </button>
      </div>
    );
  }

  if (!profile) {
    notFound();
  }

  if (!profile.isProfilePublic) {
    return (
      <div className="mx-auto max-w-2xl px-4 py-8 text-center sm:px-6 lg:px-8">
        <Lock className="mx-auto mb-4 h-16 w-16 text-white/40" />
        <h2 className="mb-2 text-xl font-semibold text-white">
          Profile is Private
        </h2>
        <p className="mb-2 text-white/80">
          {profile.message ?? "This user has chosen to keep their profile private."}
        </p>
        <p className="text-sm text-white/60">
          You can only view profiles that are set to public.
        </p>
      </div>
    );
  }

  const isOwnProfile =
    !!currentUser?.id && !!profile.id && currentUser.id === profile.id;
  const username = profile.username ?? "";
  const profileId = profile.id ?? "";

  return (
    <div className="mx-auto max-w-7xl px-4 py-4 sm:px-6 lg:px-8">
      <ProfileHeader profile={profile} isOwnProfile={isOwnProfile} />
      <nav
        className="mb-4 mt-4 flex space-x-4 overflow-x-auto"
        role="tablist"
        aria-label="Profile sections"
      >
        {profileTabs.map((tab) => {
          const Icon = tab.icon;
          const badge = tab.badge?.(profile);
          return (
            <button
              key={tab.id}
              type="button"
              role="tab"
              aria-selected={tab.id === activeTabId}
              onClick={() =>
                setUrlState({ tab: tab.id, page: 1, ...(tab.id === "collections" ? { q: "" } : {}) })
              }
              className={`flex items-center gap-2 whitespace-nowrap rounded-md px-4 py-2 text-sm font-medium transition-colors ${
                tab.id === activeTabId
                  ? "bg-white/20 text-white"
                  : "text-white/60 hover:bg-white/10 hover:text-white"
              }`}
            >
              <Icon className="h-4 w-4" />
              {tab.label}
              {badge !== undefined && badge !== "" && ` (${badge})`}
            </button>
          );
        })}
      </nav>
      <div className="min-h-96">
        {activeTab &&
          (() => {
            const TabComponent = activeTab.component;
            return (
              <PluginBoundary pluginId={activeTab.id}>
                <TabComponent
                  userId={profileId}
                  username={username}
                  profile={profile}
                />
              </PluginBoundary>
            );
          })()}
      </div>
    </div>
  );
}
