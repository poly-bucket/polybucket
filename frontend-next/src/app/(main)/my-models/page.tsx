"use client";

import { useEffect, useState, useCallback } from "react";
import Link from "next/link";
import { useAuth } from "@/contexts/AuthContext";
import { ApiClientFactory } from "@/lib/api/clientFactory";
import type { Model } from "@/lib/api/client";
import { toast } from "sonner";
import { ModelCard, ModelCardSkeleton } from "@/components/models/model-card";
import { Button } from "@/components/primitives/button";
import { DeleteModelDialog } from "@/components/models/model-details/delete-model-dialog";
import { CheckSquare, Square, Trash2 } from "lucide-react";

const PAGE_SIZE = 24;

export default function MyModelsPage() {
  const { user } = useAuth();
  const [models, setModels] = useState<Model[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [isDeleting, setIsDeleting] = useState(false);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);

  const fetchModels = useCallback(async () => {
    if (!user?.id) return;
    setLoading(true);
    try {
      const client = ApiClientFactory.getApiClient();
      const response = await client.getModelByUserId_GetModelsByUserId(
        user.id,
        1,
        PAGE_SIZE,
        false,
        true
      );
      setModels(response?.models ?? []);
    } catch (err) {
      console.error("Failed to fetch models:", err);
      toast.error("Failed to load your models");
      setModels([]);
    } finally {
      setLoading(false);
    }
  }, [user?.id]);

  useEffect(() => {
    fetchModels();
  }, [fetchModels]);

  const toggleSelect = (id: string) => {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };

  const toggleSelectAll = () => {
    if (selectedIds.size === models.length) {
      setSelectedIds(new Set());
    } else {
      setSelectedIds(new Set(models.map((m) => m.id!).filter(Boolean)));
    }
  };

  const handleBulkDelete = async () => {
    if (!user?.accessToken || selectedIds.size === 0) return;
    setIsDeleting(true);
    try {
      const client = ApiClientFactory.getApiClient();
      for (const id of selectedIds) {
        await client.deleteModel_DeleteModel(id);
      }
      toast.success(`${selectedIds.size} model(s) deleted`);
      setSelectedIds(new Set());
      setShowDeleteConfirm(false);
      await fetchModels();
    } catch (err) {
      toast.error("Failed to delete some models");
    } finally {
      setIsDeleting(false);
    }
  };

  const allSelected = models.length > 0 && selectedIds.size === models.length;

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      <h1 className="mb-6 text-2xl font-semibold text-white">My Models</h1>

      {models.length > 0 && (
        <div className="mb-4 flex flex-wrap items-center gap-3">
          <Button
            variant="outline"
            size="sm"
            onClick={toggleSelectAll}
          >
            {allSelected ? (
              <CheckSquare className="mr-2 h-4 w-4" />
            ) : (
              <Square className="mr-2 h-4 w-4" />
            )}
            {allSelected ? "Clear selection" : "Select all"}
          </Button>
          {selectedIds.size > 0 && (
            <Button
              variant="destructive"
              size="sm"
              onClick={() => setShowDeleteConfirm(true)}
              disabled={isDeleting}
            >
              <Trash2 className="mr-2 h-4 w-4" />
              Delete selected ({selectedIds.size})
            </Button>
          )}
        </div>
      )}

      {loading ? (
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
          {Array.from({ length: 8 }).map((_, i) => (
            <ModelCardSkeleton key={i} />
          ))}
        </div>
      ) : models.length === 0 ? (
        <div className="rounded-xl border border-white/20 bg-white/5 px-8 py-12 text-center">
          <p className="mb-4 text-white/60">You haven&apos;t uploaded any models yet</p>
          <Button asChild>
            <Link href="/models/upload">Upload your first model</Link>
          </Button>
        </div>
      ) : (
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
          {models.map((model) => {
            const isSelected = model.id ? selectedIds.has(model.id) : false;
            return (
              <div key={model.id} className="relative">
                {user?.id && (model.authorId === user.id || model.author?.id === user.id) && (
                  <div className="absolute left-2 top-2 z-10">
                    <button
                      type="button"
                      onClick={(e) => {
                        e.preventDefault();
                        e.stopPropagation();
                        if (model.id) toggleSelect(model.id);
                      }}
                      className={`
                        flex h-8 w-8 items-center justify-center rounded-md border-2 transition-colors
                        ${isSelected ? "border-primary bg-primary/20 text-white" : "border-white/30 bg-black/40 text-white/80 hover:bg-white/20"}
                      `}
                    >
                      {isSelected ? <CheckSquare className="h-4 w-4" /> : <Square className="h-4 w-4" />}
                    </button>
                  </div>
                )}
                <ModelCard model={model} />
              </div>
            );
          })}
        </div>
      )}

      <DeleteModelDialog
        isOpen={showDeleteConfirm}
        modelName={`${selectedIds.size} selected model(s)`}
        isDeleting={isDeleting}
        onClose={() => !isDeleting && setShowDeleteConfirm(false)}
        onConfirm={handleBulkDelete}
      />
    </div>
  );
}
