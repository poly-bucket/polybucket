"use client";

import { useEffect, useState, useCallback } from "react";
import { Search } from "lucide-react";
import { useDebouncedValue } from "@/lib/hooks/use-debounced-value";
import { Input } from "@/components/primitives/input";
import { CollectionCard, mapPublicUserCollectionDtoToCardData } from "@/components/collections/collection-card";
import { SimplePagination } from "@/components/collections/simple-pagination";
import { fetchUserCollections } from "@/lib/services/userProfileService";
import type { PublicUserCollectionListItemDto } from "@/lib/api/client";

const PAGE_SIZE = 12;
const DEBOUNCE_MS = 300;

interface ProfileCollectionsTabProps {
  username: string;
  page: number;
  q: string;
  onPageChange: (page: number) => void;
  onSearchChange: (q: string) => void;
}

export function ProfileCollectionsTab({
  username,
  page,
  q,
  onPageChange,
  onSearchChange,
}: ProfileCollectionsTabProps) {
  const [collections, setCollections] = useState<PublicUserCollectionListItemDto[]>([]);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const debouncedSearch = useDebouncedValue(q, DEBOUNCE_MS);

  const loadCollections = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await fetchUserCollections(
        username,
        page,
        PAGE_SIZE,
        debouncedSearch || undefined
      );
      setCollections(response.collections ?? []);
      setTotalPages(response.totalPages ?? 1);
      setTotalCount(response.totalCount ?? 0);
    } catch (err) {
      const message =
        (err as { result?: { message?: string }; message?: string })?.result?.message ??
        (err as { message?: string })?.message ??
        "Failed to load collections";
      setError(message);
      setCollections([]);
    } finally {
      setLoading(false);
    }
  }, [username, page, debouncedSearch]);

  useEffect(() => {
    loadCollections();
  }, [loadCollections]);

  return (
    <div>
      <div className="mb-4">
        <Input
          variant="glass"
          placeholder="Search collections..."
          value={q}
          onChange={(e) => onSearchChange(e.target.value)}
          icon={<Search className="h-4 w-4" />}
        />
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
          <p className="text-white/60">
            {error ??
              (debouncedSearch
              ? `No collections found matching "${debouncedSearch}"`
              : "No collections found")}
          </p>
        </div>
      ) : (
        <>
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
            {collections.map((collection) => (
              <CollectionCard
                key={collection.id}
                collection={mapPublicUserCollectionDtoToCardData(collection)}
                showOwner={false}
              />
            ))}
          </div>
          <SimplePagination
            page={page}
            totalPages={totalPages}
            totalCount={totalCount}
            onPageChange={onPageChange}
          />
        </>
      )}
    </div>
  );
}
