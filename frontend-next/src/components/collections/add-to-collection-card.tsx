"use client";

import { useState, useCallback, useEffect } from "react";
import { toast } from "sonner";
import { FolderPlus } from "lucide-react";
import type { Model } from "@/lib/api/client";
import { collectionsService, type Collection } from "@/lib/services/collectionsService";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/primitives/card";
import { Button } from "@/components/primitives/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

interface AddToCollectionCardProps {
  model: Model;
  isOwner: boolean;
}

export function AddToCollectionCard({ model, isOwner }: AddToCollectionCardProps) {
  const [collections, setCollections] = useState<Collection[]>([]);
  const [loading, setLoading] = useState(false);
  const [open, setOpen] = useState(false);

  const loadCollections = useCallback(async () => {
    setLoading(true);
    try {
      const response = await collectionsService.getUserCollections(1, 100, null);
      setCollections(response.collections ?? []);
    } catch {
      toast.error("Failed to load collections");
      setCollections([]);
    } finally {
      setLoading(false);
    }
  }, []);

  const handleOpenChange = (isOpen: boolean) => {
    setOpen(isOpen);
  };

  useEffect(() => {
    if (open) {
      loadCollections();
    }
  }, [open, loadCollections]);

  const handleSelect = async (collectionId: string) => {
    if (!model.id) return;
    try {
      await collectionsService.addModelToCollection(collectionId, model.id);
      toast.success("Model added to collection");
      setOpen(false);
    } catch {
      toast.error("Failed to add model to collection");
    }
  };

  if (!model.id) return null;

  return (
    <Card variant="glass" className="border-white/20">
      <CardHeader>
        <CardTitle className="text-white">Add to Collection</CardTitle>
      </CardHeader>
      <CardContent>
        <DropdownMenu open={open} onOpenChange={handleOpenChange}>
          <DropdownMenuTrigger asChild>
            <Button variant="outline" className="w-full">
              <FolderPlus className="mr-2 h-4 w-4" />
              Add to Collection
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="start" className="max-h-64 w-56 overflow-y-auto">
            {loading ? (
              <div className="px-4 py-3 text-sm text-white/60">
                Loading collections...
              </div>
            ) : collections.length === 0 ? (
              <div className="px-4 py-3 text-sm text-white/60">
                No collections. Create one first.
              </div>
            ) : (
              collections.map((c) => (
                <DropdownMenuItem
                  key={c.id}
                  onClick={() => handleSelect(c.id)}
                >
                  {c.name}
                </DropdownMenuItem>
              ))
            )}
          </DropdownMenuContent>
        </DropdownMenu>
      </CardContent>
    </Card>
  );
}
