"use client";

import { useEffect, useState, useCallback } from "react";
import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { ArrowLeft, Pencil, Trash2, Eye, EyeOff, Link2 } from "lucide-react";
import { useAuth } from "@/contexts/AuthContext";
import { useCollectionDetailTabs, PluginBoundary } from "@/lib/plugins";
import { collectionsService, type Collection } from "@/lib/services/collectionsService";
import { ModelCardSkeleton } from "@/components/models/model-card";
import { CollectionDetailProvider } from "@/contexts/CollectionDetailContext";
import { CollectionModelsTab } from "./collection-models-tab";
import { Button } from "@/components/primitives/button";
import { PasswordPrompt } from "./password-prompt";
import { DeleteCollectionDialog } from "./delete-collection-dialog";
function getVisibilityIcon(visibility?: string) {
  switch (visibility) {
    case "Public":
      return <Eye className="h-4 w-4 text-green-400" />;
    case "Unlisted":
      return <Link2 className="h-4 w-4 text-yellow-400" />;
    default:
      return <EyeOff className="h-4 w-4 text-gray-400" />;
  }
}

export function CollectionDetailsPage() {
  const params = useParams();
  const router = useRouter();
  const { user } = useAuth();
  const id = params?.id as string | undefined;

  const [collection, setCollection] = useState<Collection | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [showPasswordPrompt, setShowPasswordPrompt] = useState(false);
  const [passwordError, setPasswordError] = useState("");
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);
  const [removingModelId, setRemovingModelId] = useState<string | null>(null);

  const collectionDetailTabs = useCollectionDetailTabs();
  const [selectedTabId, setSelectedTabId] = useState<string | null>(null);
  const activeTabId = selectedTabId ?? collectionDetailTabs[0]?.id ?? "models";
  const activeTab = collectionDetailTabs.find((t) => t.id === activeTabId);

  const loadCollection = useCallback(async () => {
    if (!id) return;
    try {
      setLoading(true);
      setError("");
      const data = await collectionsService.getCollectionById(id);
      setCollection(data);
    } catch (err: unknown) {
      const axiosErr = err as { response?: { status?: number } };
      if (axiosErr?.response?.status === 401) {
        setShowPasswordPrompt(true);
        setError("");
      } else {
        setError("Failed to load collection");
      }
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    if (id) loadCollection();
  }, [id, loadCollection]);

  const handlePasswordSubmit = async (password: string) => {
    if (!id) return;
    try {
      setPasswordError("");
      const data = await collectionsService.accessCollection(id, password);
      setCollection(data);
      setShowPasswordPrompt(false);
    } catch {
      setPasswordError("Incorrect password");
    }
  };

  const handlePasswordCancel = () => {
    setShowPasswordPrompt(false);
    router.push("/collections/mine");
  };

  const handleDelete = async () => {
    if (!collection) return;
    setIsDeleting(true);
    try {
      await collectionsService.deleteCollection(collection.id);
      setShowDeleteDialog(false);
      router.push("/collections/mine");
    } catch {
      setError("Failed to delete collection");
      setIsDeleting(false);
    }
  };

  const handleRemoveModel = async (modelId: string) => {
    if (!collection?.id) return;
    setRemovingModelId(modelId);
    try {
      await collectionsService.removeModelFromCollection(collection.id, modelId);
      setCollection((prev) => {
        if (!prev?.collectionModels) return prev;
        return {
          ...prev,
          collectionModels: prev.collectionModels.filter(
            (cm) => cm.modelId !== modelId && cm.model?.id !== modelId
          ),
        };
      });
    } catch {
      setError("Failed to remove model");
    } finally {
      setRemovingModelId(null);
    }
  };

  const isOwner = !!user?.id && !!collection?.ownerId && user.id === collection.ownerId;

  const owner = collection?.owner ?? { id: collection?.ownerId ?? "", username: "Unknown" };

  if (loading && !collection) {
    return (
      <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
        <div className="mb-6 h-8 w-48 animate-pulse rounded bg-white/10" />
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
          {Array.from({ length: 8 }).map((_, i) => (
            <ModelCardSkeleton key={i} />
          ))}
        </div>
      </div>
    );
  }

  if (showPasswordPrompt && !collection) {
    return (
      <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
        <PasswordPrompt
          open={showPasswordPrompt}
          collectionName={id ?? ""}
          onSubmit={handlePasswordSubmit}
          onCancel={handlePasswordCancel}
          error={passwordError}
        />
      </div>
    );
  }

  if ((error && !showPasswordPrompt) || (!collection && !loading)) {
    return (
      <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
        <div className="rounded-xl border border-red-500/30 bg-red-500/10 p-6 text-center">
          <p className="text-red-400">{error || "Collection not found"}</p>
          <Button
            variant="outline"
            className="mt-4"
            onClick={() => router.push("/collections/mine")}
          >
            Back to Collections
          </Button>
        </div>
      </div>
    );
  }

  const modelCount =
    collection?.collectionModels?.filter((cm) => cm.model).length ?? 0;

  const contextValue = {
    collection: collection!,
    isOwner,
    onRemoveModel: isOwner ? handleRemoveModel : undefined,
    removingModelId,
  };

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
        <div className="flex items-start gap-4">
          <Button
            variant="ghost"
            size="icon-sm"
            asChild
          >
            <Link href="/collections/mine">
              <ArrowLeft className="h-4 w-4" />
            </Link>
          </Button>
          <div>
            <h1 className="text-2xl font-bold text-white">{collection?.name}</h1>
            {collection?.description && (
              <p className="mt-1 text-white/70">{collection.description}</p>
            )}
            <div className="mt-3 flex flex-wrap items-center gap-3 text-sm text-white/60">
              <span className="flex items-center gap-1">
                {getVisibilityIcon(collection?.visibility)}
                {collection?.visibility}
              </span>
              <span>{modelCount} models</span>
              {owner && (
                <Link
                  href={`/profile/${owner.id ?? owner.username}`}
                  className="text-white/60 hover:text-white/90"
                >
                  by {owner.username}
                </Link>
              )}
            </div>
          </div>
        </div>
        {isOwner && (
          <div className="flex gap-2">
            <Button variant="outline" size="sm" asChild>
              <Link href={`/collections/${id}/edit`}>
                <Pencil className="mr-1 h-4 w-4" />
                Edit
              </Link>
            </Button>
            <Button
              variant="destructive"
              size="sm"
              onClick={() => setShowDeleteDialog(true)}
            >
              <Trash2 className="mr-1 h-4 w-4" />
              Delete
            </Button>
          </div>
        )}
      </div>

      {collectionDetailTabs.length > 0 ? (
        <nav className="mb-4 flex gap-2" role="tablist">
          {collectionDetailTabs.map((tab) => {
            const Icon = tab.icon;
            const isActive = tab.id === activeTabId;
            return (
              <button
                key={tab.id}
                type="button"
                role="tab"
                aria-selected={isActive}
                onClick={() => setSelectedTabId(tab.id)}
                className={`flex items-center gap-2 rounded-md px-4 py-2 text-sm font-medium transition-colors ${
                  isActive ? "bg-white/20 text-white" : "text-white/80 hover:bg-white/10 hover:text-white"
                }`}
              >
                <Icon className="h-4 w-4" />
                {tab.label}
              </button>
            );
          })}
        </nav>
      ) : null}

      <div className="mt-6">
        {activeTab ? (
          <CollectionDetailProvider value={contextValue}>
            <PluginBoundary pluginId={activeTab.id}>
              <activeTab.component collection={collection!} />
            </PluginBoundary>
          </CollectionDetailProvider>
        ) : (
          <CollectionDetailProvider value={contextValue}>
            <CollectionModelsTab collection={collection!} />
          </CollectionDetailProvider>
        )}
      </div>

      <PasswordPrompt
        open={showPasswordPrompt}
        collectionName={id ?? ""}
        onSubmit={handlePasswordSubmit}
        onCancel={handlePasswordCancel}
        error={passwordError}
      />

      <DeleteCollectionDialog
        isOpen={showDeleteDialog}
        collectionName={collection?.name ?? ""}
        onConfirm={handleDelete}
        onCancel={() => setShowDeleteDialog(false)}
        isDeleting={isDeleting}
      />
    </div>
  );
}
