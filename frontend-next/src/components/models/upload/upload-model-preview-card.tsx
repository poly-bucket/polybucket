"use client";

import { Card } from "@/components/primitives/card";
import { cn } from "@/lib/utils";

interface UploadModelPreviewCardProps {
  title: string;
  description: string;
  thumbnailUrl: string | null;
  categories: string[];
  className?: string;
}

export function UploadModelPreviewCard({
  title,
  description,
  thumbnailUrl,
  categories,
  className,
}: UploadModelPreviewCardProps) {
  const excerpt =
    description.length > 120 ? `${description.slice(0, 120).trim()}…` : description;

  return (
    <Card variant="glass" className={cn("overflow-hidden", className)}>
      <div className="relative h-40 bg-white/10 sm:h-48">
        {thumbnailUrl ? (
          <img src={thumbnailUrl} alt="" className="h-full w-full object-cover" />
        ) : (
          <div className="flex h-full w-full items-center justify-center text-sm text-muted-foreground">
            No thumbnail
          </div>
        )}
      </div>
      <div className="p-4 space-y-2">
        <h3 className="font-semibold text-foreground line-clamp-2">{title}</h3>
        {excerpt ? (
          <p className="text-sm text-muted-foreground line-clamp-3">{excerpt}</p>
        ) : (
          <p className="text-sm text-muted-foreground italic">No description</p>
        )}
        {categories.length > 0 && (
          <div className="flex flex-wrap gap-1.5 pt-1">
            {categories.map((cat) => (
              <span
                key={cat}
                className="rounded-full border border-white/20 px-2 py-0.5 text-xs text-muted-foreground"
              >
                {cat}
              </span>
            ))}
          </div>
        )}
      </div>
    </Card>
  );
}
