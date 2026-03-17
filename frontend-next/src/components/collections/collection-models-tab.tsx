"use client";

import { ModelCard } from "@/components/models/model-card";
import { useCollectionDetail } from "@/contexts/CollectionDetailContext";
import type { Collection } from "@/lib/services/collectionsService";
import type { Model } from "@/lib/api/client";

function mapCollectionModelToModel(
  cm: {
    modelId: string;
    model?: { id: string; name?: string; description?: string; thumbnailUrl?: string };
  },
  owner: { id: string; username: string }
): Model {
  const m = cm.model;
  return {
    id: m?.id ?? cm.modelId,
    name: m?.name ?? "Untitled",
    description: m?.description ?? "",
    thumbnailUrl: m?.thumbnailUrl,
    author: { id: owner.id, username: owner.username },
    likes: 0,
    downloads: 0,
    comments: [],
    files: [],
    versions: [],
    tags: [],
    categories: [],
  } as unknown as Model;
}

interface CollectionModelsTabProps {
  collection: Collection;
}

export function CollectionModelsTab({ collection }: CollectionModelsTabProps) {
  const ctx = useCollectionDetail();
  const isOwner = ctx?.isOwner ?? false;
  const onRemoveModel = ctx?.onRemoveModel;
  const removingModelId = ctx?.removingModelId ?? null;
  const owner = collection.owner ?? {
    id: collection.ownerId,
    username: "Unknown",
  };
  const models = (collection.collectionModels ?? [])
    .filter((cm) => cm.model)
    .map((cm) => mapCollectionModelToModel(cm, owner));

  if (models.length === 0) {
    return (
      <div className="rounded-xl border border-white/20 bg-white/5 px-8 py-12 text-center">
        <p className="text-white/60">No models in this collection yet</p>
        {isOwner && (
          <p className="mt-2 text-sm text-white/50">
            Add models from their detail pages using &quot;Add to Collection&quot;
          </p>
        )}
      </div>
    );
  }

  return (
    <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
      {models.map((model) => (
        <div key={model.id} className="relative">
          {isOwner && onRemoveModel && (
            <button
              type="button"
              onClick={() => model.id && onRemoveModel(model.id)}
              disabled={removingModelId === model.id}
              className="absolute right-2 top-2 z-10 rounded bg-red-500/80 px-2 py-1 text-xs text-white hover:bg-red-500 disabled:opacity-50"
            >
              {removingModelId === model.id ? "Removing..." : "Remove"}
            </button>
          )}
          <ModelCard model={model} />
        </div>
      ))}
    </div>
  );
}
