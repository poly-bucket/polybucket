"use client";

import { useEffect, useMemo, useState } from "react";
import {
  minidenticonSvg,
  resolvedImageSrcFromAvatarField,
  svgToDataUrl,
} from "@/lib/avatar/minidenticon";
import { cn } from "@/lib/utils";

export interface UserAvatarProps {
  /** User id; preferred seed for the generated identicon. Falls back to `username` when empty. */
  userId?: string;
  username: string;
  profilePictureUrl?: string;
  /** Stored SVG, URL, or data URL (from API). */
  avatar?: string;
  className?: string;
  size?: "sm" | "md" | "lg";
}

const sizeClasses = {
  sm: "h-8 w-8 text-xs",
  md: "h-10 w-10 text-sm",
  lg: "h-12 w-12 text-base",
};

function identiconSeed(userId: string | undefined, username: string): string {
  const u = (userId && userId.trim()) || username.trim();
  return u || "user";
}

export function UserAvatar({
  userId,
  username,
  profilePictureUrl,
  avatar,
  className,
  size = "sm",
}: UserAvatarProps) {
  const [profileFailed, setProfileFailed] = useState(false);
  const [storedFailed, setStoredFailed] = useState(false);

  useEffect(() => {
    setProfileFailed(false);
    setStoredFailed(false);
  }, [profilePictureUrl, avatar, userId, username]);

  const minidenticonSrc = useMemo(() => {
    return svgToDataUrl(
      minidenticonSvg(identiconSeed(userId, username), 50, 50)
    );
  }, [userId, username]);

  const resolvedStoredSrc = useMemo(
    () => resolvedImageSrcFromAvatarField(avatar),
    [avatar]
  );

  const showProfile =
    Boolean(profilePictureUrl?.trim()) && !profileFailed;
  const showStored = Boolean(resolvedStoredSrc) && !storedFailed;
  const title = `${username}'s avatar`;

  const imgClass = cn(
    "shrink-0 rounded-full object-cover [image-rendering:pixelated]",
    sizeClasses[size],
    className
  );

  if (showProfile) {
    return (
      <img
        src={profilePictureUrl}
        alt={`${username}'s profile picture`}
        className={imgClass}
        title={title}
        onError={() => setProfileFailed(true)}
      />
    );
  }

  if (showStored) {
    return (
      <img
        src={resolvedStoredSrc!}
        alt={`${username}'s avatar`}
        className={imgClass}
        title={title}
        onError={() => setStoredFailed(true)}
      />
    );
  }

  return (
    <img
      src={minidenticonSrc}
      alt={title}
      className={imgClass}
      title={title}
    />
  );
}
