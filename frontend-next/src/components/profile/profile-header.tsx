"use client";

import type { ReactNode } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import {
  Calendar,
  MapPin,
  User,
  Link2,
  Twitter,
  Youtube,
  Instagram,
  Settings,
} from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/primitives/button";
import { Card } from "@/components/primitives/card";
import { UserAvatar } from "@/components/layout/user-avatar";
import { formatNumber } from "@/lib/utils/modelUtils";
import { formatDate } from "@/lib/utils/format";
import type { UserProfileData } from "@/lib/services/userProfileService";
import { safeExternalHref } from "@/lib/security/safeUrl";
import { followUser, unfollowUser } from "@/lib/services/userFollowService";
import { useAuth } from "@/contexts/AuthContext";

interface ProfileHeaderProps {
  profile: UserProfileData;
  isOwnProfile: boolean;
  onSelectTab?: (tabId: string) => void;
  onFollowChange?: () => void;
}

function SocialLink({
  href,
  label,
  children,
}: {
  href: string | undefined | null;
  label: string;
  children: ReactNode;
}) {
  const safe = safeExternalHref(href);
  if (!safe) return null;
  return (
    <a
      href={safe}
      target="_blank"
      rel="noopener noreferrer"
      className="text-blue-400 transition-colors hover:text-blue-300"
      aria-label={label}
    >
      {children}
    </a>
  );
}

function StatButton({
  count,
  label,
  tabId,
  onSelectTab,
}: {
  count?: number;
  label: string;
  tabId: string;
  onSelectTab?: (tabId: string) => void;
}) {
  const content = (
    <>
      <span className="font-semibold text-white">{formatNumber(count)}</span>
      <span className="text-white/60">{label}</span>
    </>
  );

  if (!onSelectTab) {
    return <div className="flex items-center gap-1">{content}</div>;
  }

  return (
    <button
      type="button"
      className="flex items-center gap-1 rounded-md px-1 py-0.5 transition-colors hover:bg-white/10"
      onClick={() => onSelectTab(tabId)}
    >
      {content}
    </button>
  );
}

export function ProfileHeader({
  profile,
  isOwnProfile,
  onSelectTab,
  onFollowChange,
}: ProfileHeaderProps) {
  const router = useRouter();
  const { isAuthenticated } = useAuth();
  const [isFollowing, setIsFollowing] = useState(profile.isFollowedByCurrentUser ?? false);
  const [followPending, setFollowPending] = useState(false);

  useEffect(() => {
    setIsFollowing(profile.isFollowedByCurrentUser ?? false);
  }, [profile.isFollowedByCurrentUser, profile.id]);

  const displayName =
    profile.firstName && profile.lastName
      ? `${profile.firstName} ${profile.lastName}`
      : profile.username ?? "User";

  const handleFollowClick = async () => {
    if (!profile.id) return;
    if (!isAuthenticated) {
      router.push("/login");
      return;
    }
    setFollowPending(true);
    try {
      if (isFollowing) {
        await unfollowUser(profile.id);
        setIsFollowing(false);
        toast.success("Unfollowed");
      } else {
        await followUser(profile.id);
        setIsFollowing(true);
        toast.success("Following");
      }
      onFollowChange?.();
    } catch {
      toast.error(isFollowing ? "Could not unfollow" : "Could not follow");
    } finally {
      setFollowPending(false);
    }
  };

  return (
    <Card variant="glass" className="border-white/20 p-4">
      {profile.isBanned && (
        <div className="mb-4 rounded-md border border-red-500/50 bg-red-500/10 px-4 py-2 text-sm text-red-400">
          This account is suspended
        </div>
      )}
      <div className="flex flex-col gap-4 sm:flex-row sm:gap-4">
        <div className="flex-shrink-0">
          <UserAvatar
            userId={profile.id ?? ""}
            username={profile.username ?? ""}
            profilePictureUrl={profile.profilePictureUrl}
            avatar={profile.avatar}
            size="lg"
            className="h-20 w-20"
          />
        </div>
        <div className="min-w-0 flex-1">
          <div className="mb-2 flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <h1 className="text-xl font-bold text-white">{displayName}</h1>
              {profile.firstName && profile.lastName && profile.username && (
                <p className="text-sm text-white/70">@{profile.username}</p>
              )}
            </div>
            <div className="flex flex-wrap gap-2">
              {!isOwnProfile && profile.isProfilePublic !== false && (
                <Button
                  type="button"
                  variant={isFollowing ? "outline" : "glass"}
                  size="sm"
                  disabled={followPending}
                  onClick={handleFollowClick}
                >
                  {isFollowing ? "Following" : "Follow"}
                </Button>
              )}
              {isOwnProfile && (
                <Button variant="outline" size="sm" asChild>
                  <Link href="/settings">
                    <Settings className="mr-1 h-4 w-4" />
                    Edit Profile
                  </Link>
                </Button>
              )}
            </div>
          </div>
          {profile.bio && (
            <p className="mb-3 line-clamp-2 text-sm text-white/90">
              {profile.bio}
            </p>
          )}
          {profile.showStatistics && (
            <div className="mb-3 flex flex-wrap items-center gap-x-4 gap-y-1 text-sm">
              <StatButton
                count={profile.totalModels}
                label="Models"
                tabId="models"
                onSelectTab={onSelectTab}
              />
              <StatButton
                count={profile.totalCollections}
                label="Collections"
                tabId="collections"
                onSelectTab={onSelectTab}
              />
              <StatButton
                count={profile.totalFollowers}
                label="Followers"
                tabId="followers"
                onSelectTab={onSelectTab}
              />
              <StatButton
                count={profile.totalFollowing}
                label="Following"
                tabId="following"
                onSelectTab={onSelectTab}
              />
              <StatButton
                count={profile.totalLikes}
                label="Likes"
                tabId="likes"
                onSelectTab={onSelectTab}
              />
              <div className="flex items-center gap-1">
                <span className="font-semibold text-white">
                  {formatNumber(profile.totalDownloads)}
                </span>
                <span className="text-white/60">Downloads</span>
              </div>
            </div>
          )}
          <div className="flex flex-wrap items-center gap-x-4 gap-y-1 text-xs text-white/60">
            {profile.country && (
              <div className="flex items-center gap-1">
                <MapPin className="h-3 w-3" />
                {profile.country}
              </div>
            )}
            <div className="flex items-center gap-1">
              <Calendar className="h-3 w-3" />
              Member since {formatDate(profile.createdAt)}
            </div>
            {profile.roleName && (
              <div className="flex items-center gap-1">
                <User className="h-3 w-3" />
                {profile.roleName}
              </div>
            )}
            {(profile.websiteUrl ||
              profile.twitterUrl ||
              profile.instagramUrl ||
              profile.youtubeUrl) && (
              <div className="ml-auto flex items-center gap-2">
                <SocialLink href={profile.websiteUrl} label="Website">
                  <Link2 className="h-4 w-4" />
                </SocialLink>
                <SocialLink href={profile.twitterUrl} label="Twitter">
                  <Twitter className="h-4 w-4" />
                </SocialLink>
                <SocialLink href={profile.instagramUrl} label="Instagram">
                  <Instagram className="h-4 w-4" />
                </SocialLink>
                <SocialLink href={profile.youtubeUrl} label="YouTube">
                  <Youtube className="h-4 w-4" />
                </SocialLink>
              </div>
            )}
          </div>
        </div>
      </div>
    </Card>
  );
}
