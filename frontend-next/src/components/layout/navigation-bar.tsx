"use client";

import { useState, useRef, useEffect } from "react";
import Link from "next/link";
import { useAuth } from "@/contexts/AuthContext";
import { useNavItems } from "@/lib/plugins";
import { UserAvatar } from "./user-avatar";
import { UserMenu } from "./user-menu";
import { SearchCommand, useSearchCommand } from "@/components/search/search-command";
import { ChevronDown, Search } from "lucide-react";

function useIsMac() {
  const [isMac, setIsMac] = useState(false);
  useEffect(() => {
    setIsMac(navigator.platform?.toUpperCase().includes("MAC") ?? false);
  }, []);
  return isMac;
}

export function NavigationBar() {
  const { user, isAuthenticated } = useAuth();
  const navItems = useNavItems();
  const [isUserMenuOpen, setIsUserMenuOpen] = useState(false);
  const userMenuAnchorRef = useRef<HTMLButtonElement>(null);
  const search = useSearchCommand();
  const isMac = useIsMac();

  const leftItems = navItems.filter((item) => item.position === "left");
  const rightItems = navItems.filter((item) => item.position === "right");

  return (
    <>
      <header className="fixed left-0 right-0 top-0 z-50 h-20 overflow-hidden border-b border-white/20 glass-bg">
        <div className="mx-auto flex h-full max-w-screen-2xl items-center px-4 sm:px-6 lg:px-8">
          <div className="flex h-full w-full items-center justify-between">
            <div className="flex flex-shrink-0 items-center gap-1.5">
              {leftItems.map((item) => {
                const Icon = item.icon;
                return (
                  <Link
                    key={item.id}
                    href={item.href}
                    className="flex h-full items-center text-white/60 transition-colors duration-200 hover:text-white"
                  >
                    <Icon className="mr-1 h-3.5 w-3.5" strokeWidth={2} />
                    <span className="text-xs">{item.label}</span>
                  </Link>
                );
              })}
              {leftItems.length > 0 && (
                <div className="mx-1 h-4 w-px bg-white/20" />
              )}
              <div className="flex items-center gap-1.5">
                <h1 className="text-2xl font-semibold leading-tight text-white">
                  Polybucket
                </h1>
              </div>
            </div>

            <button
              type="button"
              onClick={() => search.setOpen(true)}
              className="hidden items-center gap-2 rounded-md border border-white/15 bg-white/5 px-3 py-1.5 text-xs text-white/50 transition-colors hover:border-white/25 hover:bg-white/10 hover:text-white/70 sm:flex"
            >
              <Search className="h-3.5 w-3.5" />
              <span>Search...</span>
              <kbd className="ml-2 rounded border border-white/15 bg-white/5 px-1.5 py-0.5 font-mono text-[10px] text-white/30">
                {isMac ? "⌘" : "Ctrl+"}K
              </kbd>
            </button>
            <button
              type="button"
              onClick={() => search.setOpen(true)}
              className="flex items-center rounded-md p-1.5 text-white/50 transition-colors hover:bg-white/10 hover:text-white sm:hidden"
              aria-label="Search"
            >
              <Search className="h-4 w-4" />
            </button>

            <div className="flex items-center justify-end gap-1.5">
              {rightItems.map((item) => {
                const Icon = item.icon;
                return (
                  <Link
                    key={item.id}
                    href={item.href}
                    className="flex h-7 items-center gap-1 rounded-md border border-white/20 bg-white/10 px-2 py-0.5 text-xs text-white transition-colors hover:border-white/30 hover:bg-white/15"
                  >
                    <Icon className="h-3 w-3" strokeWidth={2} />
                    <span>{item.label}</span>
                  </Link>
                );
              })}

              {isAuthenticated ? (
                <button
                  ref={userMenuAnchorRef}
                  type="button"
                  onClick={() => setIsUserMenuOpen((prev) => !prev)}
                  className="flex h-full items-center gap-1 rounded p-0.5 transition-colors duration-200 hover:bg-white/10"
                >
                  <UserAvatar username={user?.username ?? ""} size="sm" />
                  <ChevronDown className="h-3 w-3 text-white/60" strokeWidth={2} />
                </button>
              ) : (
                <Link
                  href="/login"
                  className="flex h-7 items-center rounded-md border border-white/20 bg-white/10 px-2 py-0.5 text-xs text-white transition-colors hover:border-white/30 hover:bg-white/15"
                >
                  Sign In
                </Link>
              )}
            </div>
          </div>
        </div>
      </header>

      {isAuthenticated && (
        <UserMenu
          isOpen={isUserMenuOpen}
          onClose={() => setIsUserMenuOpen(false)}
          anchorRef={userMenuAnchorRef}
        />
      )}

      <SearchCommand open={search.open} onOpenChange={search.setOpen} />
    </>
  );
}
