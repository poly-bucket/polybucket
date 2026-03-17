"use client";

import Link from "next/link";
import { Card } from "@/components/primitives/card";
import { Heart, Download, MessageCircle } from "lucide-react";
import type { Model } from "@/lib/api/client";
import { cn } from "@/lib/utils";

function formatNumber(num: number | undefined): string {
  if (!num) return "0";
  if (num >= 1000000) return (num / 1000000).toFixed(1) + "M";
  if (num >= 1000) return (num / 1000).toFixed(1) + "K";
  return num.toString();
}

interface ModelCardProps {
  model: Model;
  onClick?: (model: Model) => void;
  className?: string;
}

export function ModelCard({ model, onClick, className }: ModelCardProps) {
  const thumbnailUrl = model.thumbnailUrl;
  const authorName = model.author?.username ?? "Unknown";
  const totalLikes = model.likes ?? 0;
  const downloads = model.downloads ?? 0;
  const commentsCount = model.comments?.length ?? 0;

  const handleImageError = (e: React.SyntheticEvent<HTMLImageElement>) => {
    (e.target as HTMLImageElement).style.display = "none";
    const container = e.currentTarget.parentElement;
    if (container) {
      const placeholder = document.createElement("div");
      placeholder.className = "flex h-full w-full items-center justify-center bg-white/10";
      placeholder.innerHTML = `
        <div class="text-center">
          <svg class="mx-auto mb-2 h-12 w-12 text-white/40" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
          </svg>
          <div class="text-sm text-white/40">No preview</div>
        </div>
      `;
      container.appendChild(placeholder);
    }
  };

  const content = (
    <>
      <div className="relative h-40 overflow-hidden bg-white/10 sm:h-48">
        {thumbnailUrl ? (
          <img
            src={thumbnailUrl}
            alt={model.name ?? "Model thumbnail"}
            className="h-full w-full object-cover transition-transform duration-300 group-hover:scale-105"
            onError={handleImageError}
          />
        ) : (
          <div className="flex h-full w-full items-center justify-center bg-white/10">
            <svg
              className="h-12 w-12 text-white/40"
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
      <div className="flex flex-1 flex-col p-3 sm:p-4">
        <h3 className="mb-1 min-h-[2.5rem] font-semibold leading-tight text-white line-clamp-2 text-sm">
          {model.name ?? "Untitled Model"}
        </h3>
        <p className="mb-2 text-xs text-white/60 sm:mb-3">by {authorName}</p>
        <div className="flex items-center justify-between text-xs text-white/60">
          <span className="flex items-center gap-1">
            <Heart className="h-3 w-3 fill-current text-red-400" />
            {formatNumber(totalLikes)}
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
      </div>
    </>
  );

  const cardClasses = cn(
    "group flex h-72 cursor-pointer flex-col overflow-hidden transition-all duration-200 sm:h-80",
    className
  );

  if (model.id) {
    return (
      <Link href={`/models/${model.id}`} className={cardClasses}>
        <Card variant="glass" className="flex h-full flex-col border-white/20 py-0">
          {content}
        </Card>
      </Link>
    );
  }

  return (
    <div
      className={cardClasses}
      onClick={() => onClick?.(model)}
      onKeyDown={(e) => e.key === "Enter" && onClick?.(model)}
      role="button"
      tabIndex={0}
    >
      <Card variant="glass" className="flex h-full flex-col border-white/20 py-0">
        {content}
      </Card>
    </div>
  );
}

export function ModelCardSkeleton() {
  return (
    <Card variant="glass" className="flex h-72 flex-col overflow-hidden border-white/20 py-0 sm:h-80">
      <div className="h-40 flex-shrink-0 animate-pulse bg-white/10 sm:h-48" />
      <div className="flex flex-1 flex-col gap-2 p-3 sm:p-4">
        <div className="h-4 w-3/4 animate-pulse rounded bg-white/10" />
        <div className="h-3 w-1/3 animate-pulse rounded bg-white/10" />
        <div className="mt-auto flex gap-4">
          <div className="h-3 w-8 animate-pulse rounded bg-white/10" />
          <div className="h-3 w-8 animate-pulse rounded bg-white/10" />
          <div className="h-3 w-8 animate-pulse rounded bg-white/10" />
        </div>
      </div>
    </Card>
  );
}
