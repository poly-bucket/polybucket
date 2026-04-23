"use client";

import { useEffect, useRef } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useAuth } from "@/contexts/AuthContext";
import { useUserMenuItems } from "@/lib/plugins";
import { UserAvatar } from "./user-avatar";

interface UserMenuProps {
  isOpen: boolean;
  onClose: () => void;
  anchorRef: React.RefObject<HTMLElement | null>;
}

function MenuItem({
  icon: Icon,
  label,
  href,
  onClick,
  variant = "default",
}: {
  icon: React.ComponentType<{ className?: string }>;
  label: string;
  href?: string;
  onClick?: () => void;
  variant?: "default" | "danger";
}) {
  const className =
    variant === "danger"
      ? "flex w-full items-center gap-3 px-4 py-2 text-sm text-red-400 hover:bg-white/10 transition-colors"
      : "flex w-full items-center gap-3 px-4 py-2 text-sm text-white/90 hover:bg-white/10 transition-colors";

  const content = (
    <>
      <Icon className="h-4 w-4 shrink-0" />
      {label}
    </>
  );

  if (href) {
    return (
      <Link href={href} className={className} onClick={onClick}>
        {content}
      </Link>
    );
  }

  return (
    <button type="button" className={className} onClick={onClick}>
      {content}
    </button>
  );
}

export function UserMenu({ isOpen, onClose, anchorRef, ...props }: UserMenuProps) {
  const router = useRouter();
  const { user, logout } = useAuth();
  const menuItems = useUserMenuItems();
  const menuRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (
        menuRef.current &&
        !menuRef.current.contains(event.target as Node) &&
        anchorRef.current &&
        !anchorRef.current.contains(event.target as Node)
      ) {
        onClose();
      }
    };

    if (isOpen) {
      document.addEventListener("mousedown", handleClickOutside);
    }
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, [isOpen, onClose, anchorRef]);

  const handleLogout = () => {
    logout();
    onClose();
    router.push("/login");
  };

  const handleNavigate = () => {
    onClose();
  };

  if (!isOpen) return null;

  const top = anchorRef.current
    ? anchorRef.current.getBoundingClientRect().bottom + 8
    : 0;
  const right = anchorRef.current
    ? window.innerWidth - anchorRef.current.getBoundingClientRect().right
    : 16;

  const linkItems = menuItems.filter((item) => item.id !== "logout");
  const logoutItem = menuItems.find((item) => item.id === "logout");

  return (
    <div className="fixed inset-0 z-50">
      <div
        className="absolute inset-0 bg-black/20 backdrop-blur-sm"
        onClick={onClose}
        aria-hidden
      />
      <div
        ref={menuRef}
        className="absolute z-50 w-64 rounded-md border border-white/20 bg-white/10 py-2 shadow-lg backdrop-blur-md"
        style={{ top, right }}
        {...props}
      >
        <div className="border-b border-white/20 px-4 py-3">
          <div className="flex items-center gap-3">
            <UserAvatar
              userId={user?.id ?? ""}
              username={user?.username ?? ""}
              profilePictureUrl={user?.profilePictureUrl}
              avatar={user?.avatar}
              size="md"
            />
            <div className="min-w-0 flex-1">
              <p className="truncate text-sm font-medium text-white">
                {user?.username}
              </p>
              <p className="truncate text-xs text-white/60">{user?.email}</p>
            </div>
          </div>
        </div>
        <div className="py-1">
          {linkItems.map((item) => {
            const Icon = item.icon;
            const href =
              item.id === "profile" && user
                ? `/profile/${user.username ?? user.id}`
                : item.href;
            return (
              <MenuItem
                key={item.id}
                icon={Icon}
                label={item.label}
                href={href}
                onClick={item.href ? handleNavigate : (item.onClick ?? handleNavigate)}
                variant={item.variant}
              />
            );
          })}
          {logoutItem && (
            <>
              <div className="my-2 border-t border-white/20" />
              <MenuItem
                icon={logoutItem.icon}
                label={logoutItem.label}
                onClick={handleLogout}
                variant={logoutItem.variant ?? "danger"}
              />
            </>
          )}
        </div>
      </div>
    </div>
  );
}
