"use client";

import { cn } from "@/lib/utils";

interface UserAvatarProps {
  username: string;
  profilePictureUrl?: string;
  className?: string;
  size?: "sm" | "md" | "lg";
}

const sizeClasses = {
  sm: "h-8 w-8 text-xs",
  md: "h-10 w-10 text-sm",
  lg: "h-12 w-12 text-base",
};

function getInitials(username: string): string {
  const parts = username.trim().split(/\s+/);
  if (parts.length >= 2) {
    return (parts[0][0] + parts[1][0]).toUpperCase();
  }
  return username.slice(0, 2).toUpperCase();
}

export function UserAvatar({
  username,
  profilePictureUrl,
  className,
  size = "sm",
}: UserAvatarProps) {
  const initials = getInitials(username);

  if (profilePictureUrl) {
    return (
      <img
        src={profilePictureUrl}
        alt={`${username}'s profile picture`}
        className={cn(
          "shrink-0 rounded-full object-cover",
          sizeClasses[size],
          className
        )}
        title={`${username}'s avatar`}
      />
    );
  }

  return (
    <div
      className={cn(
        "flex shrink-0 items-center justify-center rounded-full bg-white/20 font-medium text-white",
        sizeClasses[size],
        className
      )}
      title={`${username}'s avatar`}
    >
      {initials}
    </div>
  );
}
