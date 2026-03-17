"use client";

import { useEffect, useState, useCallback } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { Search, FolderPlus } from "lucide-react";
import { useDebouncedValue } from "@/lib/hooks/use-debounced-value";
import { useAuth } from "@/contexts/AuthContext";
import { collectionsService, type Collection } from "@/lib/services/collectionsService";
import { CollectionCard, mapCollectionToCardData } from "./collection-card";
import { SimplePagination } from "./simple-pagination";
import { DeleteCollectionDialog } from "./delete-collection-dialog";
import { Input } from "@/components/primitives/input";
import { Button } from "@/components/primitives/button";

const PAGE_SIZE = 12;
const DEBOUNCE_MS = 300;

export function CollectionsListPage() {
  const router = useRouter();
  const { user } = useAuth();
  const [collections, setCollections] = useState<Collection[]>([]);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [loading, setLoading] = useState(true);
  const [collectionToDelete, setCollectionToDelete] = useState<Collection | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);

  const debouncedSearch = useDebouncedValue(search, DEBOUNCE_MS);

  const loadCollections = useCallback(async () => {
    if (!user) return;
    try {
      setLoading(true);
      const response = await collectionsService.getUserCollections(
        page,
        PAGE_SIZE,
        debouncedSearch || undefined
      );
      setCollections(response.collections ?? []);
      setTotalPages(response.totalPages ?? 1);
      setTotalCount(response.totalCount ?? 0);
    } catch {
      setCollections([]);
      setTotalPages(1);
      setTotalCount(0);
    } finally {
      setLoading(false);
    }
  }, [user, page, debouncedSearch]);

  useEffect(() => {
    if (user) {
      loadCollections();
    } else {
      setCollections([]);
      setLoading(false);
    }
  }, [user, loadCollections]);

  const handleDelete = (collection: Collection) => {
    setCollectionToDelete(collection);
  };

  const handleConfirmDelete = async () => {
    if (!collectionToDelete) return;
    setIsDeleting(true);
    try {
      await collectionsService.deleteCollection(collectionToDelete.id);
      setCollections((prev) =>
        prev.filter((c) => c.id !== collectionToDelete.id)
      );
      setCollectionToDelete(null);
    } finally {
      setIsDeleting(false);
    }
  };

  const handleTogglePin = async (collection: Collection) => {
    try {
      await collectionsService.toggleFavorite(
        collection.id,
        !collection.favorite
      );
      loadCollections();
    } catch {
      //
    }
  };

  if (!user) {
    return (
      <div className="mx-auto max-w-7xl px-4 py-12 text-center sm:px-6 lg:px-8">
        <p className="text-white/60">Sign in to view your collections</p>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <h1 className="text-2xl font-bold text-white">My Collections</h1>
        <div className="flex gap-2">
          <Input
            variant="glass"
            placeholder="Search collections..."
            value={search}
            onChange={(e) => {
              setSearch(e.target.value);
              setPage(1);
            }}
            icon={<Search className="h-4 w-4" />}
          />
          <Button asChild>
            <Link href="/collections/create">
              <FolderPlus className="mr-1 h-4 w-4" />
              Create
            </Link>
          </Button>
        </div>
      </div>

      {loading ? (
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
          {Array.from({ length: 8 }).map((_, i) => (
            <div
              key={i}
              className="h-80 animate-pulse rounded-xl bg-white/10 sm:h-96"
            />
          ))}
        </div>
      ) : collections.length === 0 ? (
        <div className="rounded-xl border border-white/20 bg-white/5 px-8 py-12 text-center">
          <p className="mb-4 text-white/60">
            {debouncedSearch
              ? `No collections found matching "${debouncedSearch}"`
              : "No collections yet"}
          </p>
          {!debouncedSearch && (
            <Button asChild>
              <Link href="/collections/create">
                <FolderPlus className="mr-1 h-4 w-4" />
                Create your first collection
              </Link>
            </Button>
          )}
        </div>
      ) : (
        <>
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
            {collections.map((collection) => (
              <CollectionCard
                key={collection.id}
                collection={mapCollectionToCardData(collection)}
                onEdit={(c) => router.push(`/collections/${c.id}/edit`)}
                onDelete={() => handleDelete(collection)}
                onTogglePin={() => handleTogglePin(collection)}
              />
            ))}
          </div>
          <SimplePagination
            page={page}
            totalPages={totalPages}
            totalCount={totalCount}
            onPageChange={setPage}
          />
        </>
      )}

      <DeleteCollectionDialog
        isOpen={!!collectionToDelete}
        collectionName={collectionToDelete?.name ?? ""}
        onConfirm={handleConfirmDelete}
        onCancel={() => setCollectionToDelete(null)}
        isDeleting={isDeleting}
      />
    </div>
  );
}
