"use client";

import Link from "next/link";
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
import { Button } from "@/components/primitives/button";
import { Card } from "@/components/primitives/card";
import { UserAvatar } from "@/components/layout/user-avatar";
import { formatNumber } from "@/lib/utils/modelUtils";
import { formatDate } from "@/lib/utils/format";
import type { UserProfileData } from "@/lib/services/userProfileService";

interface ProfileHeaderProps {
  profile: UserProfileData;
  isOwnProfile: boolean;
}

export function ProfileHeader({ profile, isOwnProfile }: ProfileHeaderProps) {
  const displayName =
    profile.firstName && profile.lastName
      ? `${profile.firstName} ${profile.lastName}`
      : profile.username ?? "User";

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
            {isOwnProfile && (
              <Button variant="outline" size="sm" asChild>
                <Link href="/settings">
                  <Settings className="mr-1 h-4 w-4" />
                  Edit Profile
                </Link>
              </Button>
            )}
          </div>
          {profile.bio && (
            <p className="mb-3 line-clamp-2 text-sm text-white/90">
              {profile.bio}
            </p>
          )}
          {profile.showStatistics && (
            <div className="mb-3 flex flex-wrap items-center gap-x-4 gap-y-1 text-sm">
              <div className="flex items-center gap-1">
                <span className="font-semibold text-white">
                  {formatNumber(profile.totalModels)}
                </span>
                <span className="text-white/60">Models</span>
              </div>
              <div className="flex items-center gap-1">
                <span className="font-semibold text-white">
                  {formatNumber(profile.totalCollections)}
                </span>
                <span className="text-white/60">Collections</span>
              </div>
              <div className="flex items-center gap-1">
                <span className="font-semibold text-white">
                  {formatNumber(profile.totalLikes)}
                </span>
                <span className="text-white/60">Likes</span>
              </div>
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
                {profile.websiteUrl && (
                  <a
                    href={profile.websiteUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-blue-400 transition-colors hover:text-blue-300"
                    aria-label="Website"
                  >
                    <Link2 className="h-4 w-4" />
                  </a>
                )}
                {profile.twitterUrl && (
                  <a
                    href={profile.twitterUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-blue-400 transition-colors hover:text-blue-300"
                    aria-label="Twitter"
                  >
                    <Twitter className="h-4 w-4" />
                  </a>
                )}
                {profile.instagramUrl && (
                  <a
                    href={profile.instagramUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-blue-400 transition-colors hover:text-blue-300"
                    aria-label="Instagram"
                  >
                    <Instagram className="h-4 w-4" />
                  </a>
                )}
                {profile.youtubeUrl && (
                  <a
                    href={profile.youtubeUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-blue-400 transition-colors hover:text-blue-300"
                    aria-label="YouTube"
                  >
                    <Youtube className="h-4 w-4" />
                  </a>
                )}
              </div>
            )}
          </div>
        </div>
      </div>
    </Card>
  );
}
