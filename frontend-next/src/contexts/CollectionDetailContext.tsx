"use client";

import { createContext, useContext } from "react";
import type { Collection } from "@/lib/services/collectionsService";

interface CollectionDetailContextValue {
  collection: Collection;
  isOwner: boolean;
  onRemoveModel?: (modelId: string) => void;
  removingModelId?: string | null;
}

const CollectionDetailContext = createContext<CollectionDetailContextValue | null>(null);

export function CollectionDetailProvider({
  value,
  children,
}: {
  value: CollectionDetailContextValue;
  children: React.ReactNode;
}) {
  return (
    <CollectionDetailContext.Provider value={value}>
      {children}
    </CollectionDetailContext.Provider>
  );
}

export function useCollectionDetail() {
  const ctx = useContext(CollectionDetailContext);
  return ctx;
}
