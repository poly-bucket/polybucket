"use client";

import Link from "next/link";
import { Eye, EyeOff, Link2, MoreHorizontal, Pencil, Trash2, Bookmark, BookmarkCheck } from "lucide-react";
import { Card } from "@/components/primitives/card";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Button } from "@/components/primitives/button";

export interface CollectionCardData {
  id: string;
  name: string;
  description?: string;
  visibility?: string;
  modelCount?: number;
  thumbnailUrl?: string;
  favorite?: boolean;
  owner?: { id: string; username: string };
}

interface CollectionCardProps {
  collection: CollectionCardData;
  showOwner?: boolean;
  onEdit?: (collection: CollectionCardData) => void;
  onDelete?: (collection: CollectionCardData) => void;
  onTogglePin?: (collection: CollectionCardData) => void;
}

function getVisibilityIcon(visibility?: string) {
  switch (visibility) {
    case "Public":
      return <Eye className="h-4 w-4 text-green-400" aria-label="Public" />;
    case "Unlisted":
      return <Link2 className="h-4 w-4 text-yellow-400" aria-label="Unlisted" />;
    case "Private":
    default:
      return <EyeOff className="h-4 w-4 text-gray-400" aria-label="Private" />;
  }
}

export function CollectionCard({
  collection,
  showOwner = false,
  onEdit,
  onDelete,
  onTogglePin,
}: CollectionCardProps) {
  const href = collection.id ? `/collections/${collection.id}` : "#";
  const hasActions = onEdit || onDelete || onTogglePin;

  const thumbnailContent = (
    <div className="flex h-full w-full items-center justify-center overflow-hidden bg-gradient-to-br from-gray-700 to-gray-900">
      {collection.thumbnailUrl ? (
        <img
          src={collection.thumbnailUrl}
          alt={collection.name}
          className="h-full w-full object-cover transition-transform duration-300 group-hover:scale-105"
        />
      ) : (
        <div className="flex h-20 w-20 items-center justify-center rounded-full bg-white/10 backdrop-blur-sm">
          <span className="text-2xl font-bold text-white/60">
            {(collection.name ?? "C").charAt(0).toUpperCase()}
          </span>
        </div>
      )}
    </div>
  );

  return (
    <div className="group relative flex h-80 flex-col sm:h-96">
      <Link href={href} className="flex flex-1 flex-col min-h-0">
        <Card
          variant="glass"
          className="flex h-full cursor-pointer flex-col overflow-hidden border-white/20 gap-0 py-0 transition-all duration-200 hover:shadow-[0_12px_40px_rgba(0,0,0,0.4)]"
        >
          <div className="relative h-40 flex-shrink-0 overflow-hidden bg-gradient-to-br from-gray-700 to-gray-900 sm:h-48">
            {thumbnailContent}
            <div className="absolute left-2 top-2 flex flex-wrap gap-1">
              {collection.visibility === "Private" && (
                <span className="rounded bg-red-500/80 px-2 py-0.5 text-xs text-white">
                  Private
                </span>
              )}
              {collection.visibility === "Unlisted" && (
                <span className="rounded bg-yellow-500/80 px-2 py-0.5 text-xs text-white">
                  Unlisted
                </span>
              )}
              {collection.visibility === "Public" && (
                <span className="rounded bg-green-500/80 px-2 py-0.5 text-xs text-white">
                  Public
                </span>
              )}
            </div>
            {hasActions && (
              <div
                className="absolute right-2 top-2 opacity-0 transition-opacity group-hover:opacity-100"
                onClick={(e) => e.preventDefault()}
              >
                <DropdownMenu>
                  <DropdownMenuTrigger asChild>
                    <Button
                      variant="ghost"
                      size="icon-sm"
                      className="h-8 w-8 rounded-full bg-white/90 text-gray-700 hover:bg-white"
                    >
                      <MoreHorizontal className="h-4 w-4" />
                    </Button>
                  </DropdownMenuTrigger>
                  <DropdownMenuContent align="end" className="w-48">
                    {onTogglePin && (
                      <DropdownMenuItem
                        onClick={(e) => {
                          e.preventDefault();
                          onTogglePin(collection);
                        }}
                      >
                        {collection.favorite ? (
                          <>
                            <BookmarkCheck className="h-4 w-4" />
                            Unpin from sidebar
                          </>
                        ) : (
                          <>
                            <Bookmark className="h-4 w-4" />
                            Pin to sidebar
                          </>
                        )}
                      </DropdownMenuItem>
                    )}
                    {onEdit && (
                      <DropdownMenuItem
                        onClick={(e) => {
                          e.preventDefault();
                          onEdit(collection);
                        }}
                      >
                        <Pencil className="h-4 w-4" />
                        Edit
                      </DropdownMenuItem>
                    )}
                    {onDelete && (
                      <DropdownMenuItem
                        variant="destructive"
                        onClick={(e) => {
                          e.preventDefault();
                          onDelete(collection);
                        }}
                      >
                        <Trash2 className="h-4 w-4" />
                        Delete
                      </DropdownMenuItem>
                    )}
                  </DropdownMenuContent>
                </DropdownMenu>
              </div>
            )}
          </div>
          <div className="flex min-h-0 flex-1 flex-col p-3 sm:p-4">
            <h3 className="mb-1 min-h-[2.5rem] line-clamp-2 text-sm font-semibold leading-tight text-white">
              {collection.name ?? "Untitled Collection"}
            </h3>
            {collection.description && (
              <p className="mb-2 line-clamp-2 flex-shrink-0 text-xs text-white/60 sm:mb-3">
                {collection.description}
              </p>
            )}
            <div className="mt-auto flex items-center justify-between text-xs text-white/60">
              <span className="flex items-center gap-1">
                {getVisibilityIcon(collection.visibility)}
                {collection.modelCount ?? 0} models
              </span>
              {showOwner && collection.owner && (
                <Link
                  href={`/profile/${collection.owner.id ?? collection.owner.username}`}
                  className="text-white/60 hover:text-white/90"
                  onClick={(e) => e.stopPropagation()}
                >
                  {collection.owner.username}
                </Link>
              )}
            </div>
          </div>
        </Card>
      </Link>
    </div>
  );
}

export function mapPublicUserCollectionDtoToCardData(
  dto: { id?: string; name?: string; description?: string; visibility?: string; modelCount?: number; avatar?: string }
): CollectionCardData {
  return {
    id: dto.id ?? "",
    name: dto.name ?? "Untitled",
    description: dto.description,
    visibility: dto.visibility,
    modelCount: dto.modelCount,
    thumbnailUrl: dto.avatar?.startsWith("http") ? dto.avatar : undefined,
  };
}

export function mapCollectionToCardData(
  collection: {
    id: string;
    name: string;
    description?: string;
    visibility?: string;
    favorite?: boolean;
    owner?: { id: string; username: string };
    collectionModels?: Array<{ model?: { thumbnailUrl?: string } }>;
    avatar?: string;
  }
): CollectionCardData {
  const thumbnailUrl =
    collection.collectionModels?.[0]?.model?.thumbnailUrl ??
    (collection.avatar?.startsWith("http") ? collection.avatar : undefined);
  return {
    id: collection.id,
    name: collection.name,
    description: collection.description,
    visibility: collection.visibility,
    modelCount: collection.collectionModels?.length ?? 0,
    thumbnailUrl,
    favorite: collection.favorite,
    owner: collection.owner,
  };
}
