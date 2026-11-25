"use client";

import Link from "next/link";
import { Card } from "@/components/primitives/card";
import { Heart, Download, MessageCircle, ThumbsDown } from "lucide-react";
import type { ModelWithReactions } from "@/lib/types/modelReactions";
import { Skeleton } from "@/components/ui/glass/skeleton";
import { cn } from "@/lib/utils";
import { isImageUrl } from "@/lib/utils/modelUtils";
import type { ModelCardSize } from "@/lib/layout/model-browse-layout";

function formatNumber(num: number | undefined): string {
  if (!num) return "0";
  if (num >= 1000000) return (num / 1000000).toFixed(1) + "M";
  if (num >= 1000) return (num / 1000).toFixed(1) + "K";
  return num.toString();
}

const ROW_SIZE: Record<ModelCardSize, { row: string; thumb: string; title: string }> = {
  small: {
    row: "min-h-[4.5rem] gap-3 p-2",
    thumb: "h-14 w-14 shrink-0",
    title: "text-sm",
  },
  medium: {
    row: "min-h-[5.5rem] gap-4 p-3",
    thumb: "h-16 w-16 shrink-0 sm:h-[4.5rem] sm:w-[4.5rem]",
    title: "text-sm sm:text-base",
  },
  large: {
    row: "min-h-[6.5rem] gap-4 p-4",
    thumb: "h-20 w-20 shrink-0 sm:h-24 sm:w-24",
    title: "text-base",
  },
};

interface ModelListRowProps {
  model: ModelWithReactions;
  onClick?: (model: ModelWithReactions) => void;
  className?: string;
  size?: ModelCardSize;
}

export function ModelListRow({ model, onClick, className, size = "medium" }: ModelListRowProps) {
  const styles = ROW_SIZE[size] ?? ROW_SIZE.medium;
  const thumbnailUrl = isImageUrl(model.thumbnailUrl) ? model.thumbnailUrl : undefined;
  const authorName = model.author?.username ?? "Unknown";
  const totalLikes = model.likes ?? 0;
  const totalDislikes = model.dislikes ?? 0;
  const downloads = model.downloads ?? 0;
  const commentsCount = model.comments?.length ?? 0;

  const content = (
    <Card
      variant="glass"
      className={cn(
        "group flex w-full flex-row items-center border-white/20 py-0 transition-all duration-200 hover:border-white/30",
        styles.row,
        className
      )}
    >
      <div className={cn("relative overflow-hidden rounded-md bg-white/10", styles.thumb)}>
        {thumbnailUrl ? (
          <img
            src={thumbnailUrl}
            alt={model.name ?? "Model thumbnail"}
            className="h-full w-full object-cover"
          />
        ) : (
          <div className="flex h-full w-full items-center justify-center">
            <svg
              className="h-8 w-8 text-white/40"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10"
              />
            </svg>
          </div>
        )}
      </div>
      <div className="flex min-w-0 flex-1 flex-col justify-center">
        <h3 className={cn("truncate font-semibold text-white", styles.title)}>
          {model.name ?? "Untitled Model"}
        </h3>
        <p className="truncate text-xs text-white/60">by {authorName}</p>
      </div>
      <div className="hidden shrink-0 items-center gap-3 text-xs text-white/60 sm:flex">
        <span className="flex items-center gap-1">
          <Heart className="h-3 w-3 fill-current text-red-400" />
          {formatNumber(totalLikes)}
        </span>
        <span className="flex items-center gap-1">
          <ThumbsDown className="h-3 w-3 text-orange-400" />
          {formatNumber(totalDislikes)}
        </span>
        <span className="flex items-center gap-1">
          <Download className="h-3 w-3 text-green-400" />
          {formatNumber(downloads)}
        </span>
        <span className="flex items-center gap-1">
          <MessageCircle className="h-3 w-3 text-blue-400" />
          {formatNumber(commentsCount)}
        </span>
      </div>
    </Card>
  );

  if (model.id) {
    return (
      <Link href={`/models/${model.id}`} className="block w-full">
        {content}
      </Link>
    );
  }

  return (
    <div
      className="w-full cursor-pointer"
      onClick={() => onClick?.(model)}
      onKeyDown={(e) => e.key === "Enter" && onClick?.(model)}
      role="button"
      tabIndex={0}
    >
      {content}
    </div>
  );
}

export function ModelListRowSkeleton({ size = "medium" }: { size?: ModelCardSize }) {
  const styles = ROW_SIZE[size] ?? ROW_SIZE.medium;
  return (
    <Card variant="glass" className={cn("flex flex-row items-center border-white/20 py-0", styles.row)}>
      <Skeleton className={cn("shrink-0 rounded-md", styles.thumb)} />
      <div className="flex min-w-0 flex-1 flex-col gap-2">
        <Skeleton className="h-4 w-2/3" />
        <Skeleton className="h-3 w-1/3" />
      </div>
      <div className="hidden gap-2 sm:flex">
        <Skeleton className="h-3 w-8" />
        <Skeleton className="h-3 w-8" />
        <Skeleton className="h-3 w-8" />
      </div>
    </Card>
  );
}