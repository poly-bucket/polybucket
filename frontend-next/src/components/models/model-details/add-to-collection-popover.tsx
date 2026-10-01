"use client";

import { useCallback, useEffect, useId, useRef, useState } from "react";
import Link from "next/link";
import { toast } from "sonner";
import { Check, FolderPlus, Plus } from "lucide-react";
import { Button } from "@/components/primitives/button";
import { collectionsService, type Collection } from "@/lib/services/collectionsService";
import { getAccountApiErrorMessage, isEmailUnverifiedError } from "@/lib/services/accountEmailService";
import { CheckboxListSkeleton } from "@/components/ui/skeletons";

const COLLECTION_PAGE_SIZE = 50;

function collectionErrorMessage(error: unknown, fallback: string): string {
  if (isEmailUnverifiedError(error)) {
    return "Verify your email address to organize collections.";
  }
  return getAccountApiErrorMessage(error, fallback);
}

export interface AddToCollectionPopoverProps {
  modelId: string;
}

export function AddToCollectionPopover({ modelId }: AddToCollectionPopoverProps) {
  const panelId = useId();
  const containerRef = useRef<HTMLDivElement>(null);
  const [open, setOpen] = useState(false);
  const [collections, setCollections] = useState<Collection[] | null>(null);
  const [memberIds, setMemberIds] = useState<Set<string>>(new Set());
  const [pendingIds, setPendingIds] = useState<Set<string>>(new Set());
  const [loadError, setLoadError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoadError(null);
    try {
      const result = await collectionsService.getUserCollections(1, COLLECTION_PAGE_SIZE);
      const list = result.collections ?? [];
      setCollections(list);
      setMemberIds(
        new Set(
          list
            .filter((c) => c.collectionModels?.some((cm) => cm.modelId === modelId))
            .map((c) => c.id)
        )
      );
    } catch (error) {
      setLoadError(collectionErrorMessage(error, "Your collections could not be loaded."));
    }
  }, [modelId]);

  useEffect(() => {
    if (open && collections === null) {
      load();
    }
  }, [open, collections, load]);

  useEffect(() => {
    if (!open) {
      return;
    }
    const handlePointer = (event: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setOpen(false);
      }
    };
    const handleKey = (event: KeyboardEvent) => {
      if (event.key === "Escape") {
        setOpen(false);
      }
    };
    document.addEventListener("mousedown", handlePointer);
    document.addEventListener("keydown", handleKey);
    return () => {
      document.removeEventListener("mousedown", handlePointer);
      document.removeEventListener("keydown", handleKey);
    };
  }, [open]);

  const toggleMembership = async (collection: Collection) => {
    if (pendingIds.has(collection.id)) {
      return;
    }
    const isMember = memberIds.has(collection.id);
    setPendingIds((prev) => new Set(prev).add(collection.id));
    try {
      if (isMember) {
        await collectionsService.removeModelFromCollection(collection.id, modelId);
      } else {
        await collectionsService.addModelToCollection(collection.id, modelId);
      }
      setMemberIds((prev) => {
        const next = new Set(prev);
        if (isMember) {
          next.delete(collection.id);
        } else {
          next.add(collection.id);
        }
        return next;
      });
      toast.success(isMember ? `Removed from ${collection.name}` : `Added to ${collection.name}`);
    } catch (error) {
      toast.error(
        collectionErrorMessage(
          error,
          isMember ? "The model could not be removed." : "The model could not be added."
        )
      );
    } finally {
      setPendingIds((prev) => {
        const next = new Set(prev);
        next.delete(collection.id);
        return next;
      });
    }
  };

  return (
    <div ref={containerRef} className="relative">
      <Button
        type="button"
        variant="outline"
        className="w-full"
        aria-expanded={open}
        aria-controls={panelId}
        onClick={() => setOpen((value) => !value)}
      >
        <FolderPlus className="h-4 w-4" />
        Add to collection
      </Button>
      {open && (
        <div
          id={panelId}
          role="dialog"
          aria-label="Add to collection"
          className="absolute left-0 right-0 z-20 mt-2 rounded-lg border border-white/20 bg-black/90 p-2 shadow-xl backdrop-blur"
        >
          {loadError ? (
            <div role="alert" className="space-y-2 p-2 text-sm text-red-300">
              <p>{loadError}</p>
              <Button type="button" variant="outline" size="sm" onClick={load}>
                Retry
              </Button>
            </div>
          ) : collections === null ? (
            <div className="p-2">
              <CheckboxListSkeleton rows={4} />
            </div>
          ) : collections.length === 0 ? (
            <p className="p-2 text-sm text-white/60">You don&apos;t have any collections yet.</p>
          ) : (
            <ul className="max-h-64 space-y-1 overflow-y-auto">
              {collections.map((collection) => {
                const isMember = memberIds.has(collection.id);
                return (
                  <li key={collection.id}>
                    <button
                      type="button"
                      aria-pressed={isMember}
                      disabled={pendingIds.has(collection.id)}
                      onClick={() => toggleMembership(collection)}
                      className="flex w-full items-center justify-between gap-2 rounded-md px-3 py-2 text-left text-sm text-white hover:bg-white/10 disabled:opacity-50"
                    >
                      <span className="truncate">{collection.name}</span>
                      {isMember && <Check className="h-4 w-4 shrink-0 text-green-400" />}
                    </button>
                  </li>
                );
              })}
            </ul>
          )}
          <Link
            href="/collections/create"
            className="mt-1 flex items-center gap-2 rounded-md px-3 py-2 text-sm text-white/70 hover:bg-white/10 hover:text-white"
          >
            <Plus className="h-4 w-4" />
            New collection
          </Link>
        </div>
      )}
    </div>
  );
}
