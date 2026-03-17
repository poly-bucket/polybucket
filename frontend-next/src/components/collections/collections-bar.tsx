"use client";

import Link from "next/link";
import { ChevronLeft, ChevronRight, FolderPlus } from "lucide-react";
import type { CollectionCardData } from "./collection-card";
import { Button } from "@/components/primitives/button";
import { cn } from "@/lib/utils";

export const SIDEBAR_STORAGE_KEY = "collectionsSidebarCollapsed";

interface CollectionsBarProps {
  collections: CollectionCardData[];
  isCollapsed: boolean;
  onToggle: () => void;
  loading?: boolean;
}

export function CollectionsBar({
  collections,
  isCollapsed,
  onToggle,
  loading = false,
}: CollectionsBarProps) {
  return (
    <aside
      className={cn(
        "flex shrink-0 flex-col border-r border-white/10 transition-all duration-300",
        "glass-bg",
        isCollapsed ? "w-16" : "w-64"
      )}
    >
      <div className="flex h-14 items-center justify-between border-b border-white/10 px-2">
        {!isCollapsed && (
          <span className="truncate text-sm font-medium text-white">
            Favorites
          </span>
        )}
        <Button
          variant="ghost"
          size="icon-sm"
          onClick={onToggle}
          aria-label={isCollapsed ? "Expand sidebar" : "Collapse sidebar"}
        >
          {isCollapsed ? (
            <ChevronRight className="h-4 w-4" />
          ) : (
            <ChevronLeft className="h-4 w-4" />
          )}
        </Button>
      </div>

      <div className="flex min-h-0 flex-1 flex-col overflow-y-auto p-2">
        <Link
          href="/collections/create"
          className={cn(
            "mb-3 flex items-center gap-2 rounded-md px-3 py-2 text-sm text-white/90 transition-colors hover:bg-white/10",
            isCollapsed && "justify-center px-2"
          )}
        >
          <FolderPlus className="h-4 w-4 shrink-0" />
          {!isCollapsed && <span>New Collection</span>}
        </Link>

        {loading ? (
          <div className="space-y-2">
            {[1, 2, 3].map((i) => (
              <div
                key={i}
                className="h-12 animate-pulse rounded-md bg-white/10"
              />
            ))}
          </div>
        ) : (
          <div className="space-y-1">
            {collections.map((c) => (
              <Link
                key={c.id}
                href={`/collections/${c.id}`}
                className={cn(
                  "block rounded-md px-3 py-2 text-sm transition-colors hover:bg-white/10",
                  isCollapsed && "flex justify-center px-2"
                )}
              >
                {isCollapsed ? (
                  <span className="text-lg font-semibold text-white/80">
                    {(c.name ?? "C").charAt(0).toUpperCase()}
                  </span>
                ) : (
                  <div className="flex flex-col">
                    <span className="truncate font-medium text-white">
                      {c.name}
                    </span>
                    <span className="text-xs text-white/50">
                      {c.modelCount ?? 0} models
                    </span>
                  </div>
                )}
              </Link>
            ))}
          </div>
        )}
      </div>
    </aside>
  );
}
