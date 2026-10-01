"use client";

import { useCallback, useEffect, useState } from "react";
import { CheckCircle, XCircle, Box } from "lucide-react";
import { Button } from "@/components/primitives/button";
import { Badge } from "@/components/ui/badge";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Textarea } from "@/components/ui/textarea";
import { DataTablePagination } from "@/components/primitives/pagination";
import { ModelQueueCardSkeleton } from "@/components/ui/skeletons";
import {
  approveModel,
  getModelsAwaitingModeration,
  rejectModel,
  type ModelModerationQueueItem,
} from "@/lib/services/modelModerationService";

function formatDate(value: string): string {
  return new Date(value).toLocaleString();
}

export function ModelQueueTab() {
  const [items, setItems] = useState<ModelModerationQueueItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
  const [actionLoading, setActionLoading] = useState(false);
  const [rejectTarget, setRejectTarget] = useState<ModelModerationQueueItem | null>(null);
  const [rejectReason, setRejectReason] = useState("");

  const loadQueue = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await getModelsAwaitingModeration(page, pageSize);
      setItems(response.items ?? []);
      setTotalCount(response.totalCount ?? 0);
    } catch {
      setError("Failed to load models awaiting moderation.");
    } finally {
      setLoading(false);
    }
  }, [page, pageSize]);

  useEffect(() => {
    loadQueue();
  }, [loadQueue]);

  const handleApprove = async (modelId: string) => {
    try {
      setActionLoading(true);
      await approveModel(modelId);
      await loadQueue();
    } catch {
      setError("Failed to approve model.");
    } finally {
      setActionLoading(false);
    }
  };

  const handleRejectConfirm = async () => {
    if (!rejectTarget) return;
    try {
      setActionLoading(true);
      await rejectModel(rejectTarget.id, rejectReason.trim() || undefined);
      setRejectTarget(null);
      setRejectReason("");
      await loadQueue();
    } catch {
      setError("Failed to reject model.");
    } finally {
      setActionLoading(false);
    }
  };

  if (loading && items.length === 0) {
    return <ModelQueueCardSkeleton count={4} />;
  }

  return (
    <div className="space-y-6">
      {error && (
        <p className="rounded-lg border border-red-500/30 bg-red-500/10 px-4 py-2 text-sm text-red-200">
          {error}
        </p>
      )}

      {items.length === 0 ? (
        <div className="rounded-lg border border-white/10 glass-bg p-12 text-center">
          <Box className="mx-auto mb-4 h-10 w-10 text-white/40" />
          <p className="text-white/80">No models are waiting for review.</p>
        </div>
      ) : (
        <div className="space-y-4">
          {items.map((model) => (
            <div
              key={model.id}
              className="rounded-lg border border-white/10 glass-bg p-4 md:p-6"
            >
              <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
                <div className="min-w-0 flex-1">
                  <div className="mb-2 flex flex-wrap items-center gap-2">
                    <h3 className="text-lg font-semibold text-white">{model.name}</h3>
                    {model.fileFormat && (
                      <Badge variant="outline" className="text-white/70">
                        {model.fileFormat.toUpperCase()}
                      </Badge>
                    )}
                  </div>
                  <p className="text-sm text-white/60">
                    By {model.userName} · {formatDate(model.createdAt)}
                  </p>
                  <p className="mt-2 text-sm text-white/80 line-clamp-3">
                    {model.description}
                  </p>
                </div>
                <div className="flex shrink-0 gap-2">
                  <Button
                    size="sm"
                    disabled={actionLoading}
                    onClick={() => handleApprove(model.id)}
                    className="gap-1"
                  >
                    <CheckCircle className="h-4 w-4" />
                    Approve
                  </Button>
                  <Button
                    size="sm"
                    variant="outline"
                    disabled={actionLoading}
                    onClick={() => setRejectTarget(model)}
                    className="gap-1 border-red-500/40 text-red-200 hover:bg-red-500/10"
                  >
                    <XCircle className="h-4 w-4" />
                    Reject
                  </Button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {totalPages > 1 && (
        <DataTablePagination
          page={page}
          totalPages={totalPages}
          onPageChange={setPage}
        />
      )}

      <Dialog open={rejectTarget != null} onOpenChange={(open) => !open && setRejectTarget(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Reject model</DialogTitle>
            <DialogDescription>
              Reject &quot;{rejectTarget?.name}&quot;. Optionally provide a reason for the author.
            </DialogDescription>
          </DialogHeader>
          <Textarea
            value={rejectReason}
            onChange={(e) => setRejectReason(e.target.value)}
            placeholder="Reason for rejection (optional)"
            rows={4}
          />
          <DialogFooter>
            <Button variant="outline" onClick={() => setRejectTarget(null)}>
              Cancel
            </Button>
            <Button
              variant="destructive"
              disabled={actionLoading}
              onClick={handleRejectConfirm}
            >
              Reject
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
