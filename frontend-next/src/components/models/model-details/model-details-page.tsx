"use client";

import { useEffect, useState, useCallback, useMemo } from "react";
import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { toast } from "sonner";
import { useAuth } from "@/contexts/AuthContext";
import { useModelDetailTabs, PluginBoundary } from "@/lib/plugins";
import { ApiClientFactory } from "@/lib/api/clientFactory";
import { fetchModelById } from "@/lib/services/modelsService";
import { getApiConfig } from "@/lib/api/config";
import { isMarkdownFile } from "@/lib/utils/modelUtils";
import type { Model, ModelFile } from "@/lib/api/client";
import { ModelDetailsCarousel, type CarouselItem } from "./model-details-carousel";
import { ModelDetailsSidebar } from "./model-details-sidebar";
import { ModelDetailsFiles } from "./model-details-files";
import { DeleteModelDialog } from "./delete-model-dialog";
import { EditModelModal } from "../edit-model/edit-model-modal";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/primitives/card";
import { Button } from "@/components/primitives/button";

function buildCarouselItems(model: Model): CarouselItem[] {
  const items: CarouselItem[] = [];
  const baseUrl = getApiConfig().baseUrl;

  if (model.thumbnailUrl) {
    items.push({
      id: "thumbnail",
      type: "image",
      url: model.thumbnailUrl,
    });
  }

  const files = model.files ?? [];
  const imageFiles = files.filter(
    (f) =>
      f.mimeType?.startsWith("image/") ||
      f.name?.toLowerCase().match(/\.(jpg|jpeg|png|gif|webp|bmp)$/)
  );
  imageFiles.forEach((file) => {
    if (file.name && file.id && model.id) {
      items.push({
        id: file.id,
        type: "image",
        url: `${baseUrl}/api/files/stream/model/${model.id}/${encodeURIComponent(file.name)}`,
      });
    }
  });

  const model3DFiles = files.filter(
    (f) =>
      f.mimeType?.startsWith("model/") ||
      f.name?.toLowerCase().match(/\.(stl|obj|fbx|gltf|glb|3mf|step|stp)$/)
  );
  model3DFiles.forEach((file) => {
    if (file.name && file.id) {
      items.push({
        id: file.id,
        type: "3d",
        fileName: file.name,
        mimeType: file.mimeType,
      });
    }
  });

  const docFiles = files.filter(
    (f) =>
      f.name?.toLowerCase().match(/\.(pdf|md|markdown|txt)$/) ||
      f.mimeType?.startsWith("text/") ||
      f.mimeType === "application/pdf"
  );
  docFiles.forEach((file) => {
    if (file.name && file.id) {
      items.push({
        id: file.id,
        type: isMarkdownFile(file.name) ? "markdown" : "pdf",
        fileName: file.name,
        mimeType: file.mimeType,
      });
    }
  });

  return items;
}

export function ModelDetailsPage() {
  const params = useParams();
  const router = useRouter();
  const id = params?.id as string | undefined;
  const { user, isAuthenticated } = useAuth();

  const [model, setModel] = useState<Model | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);
  const [downloadingFiles, setDownloadingFiles] = useState<Set<string>>(
    new Set()
  );
  const [selectedTabId, setSelectedTabId] = useState<string | null>(null);

  const modelDetailTabs = useModelDetailTabs();
  const activeTabId =
    selectedTabId && modelDetailTabs.some((t) => t.id === selectedTabId)
      ? selectedTabId
      : modelDetailTabs[0]?.id ?? null;
  const activeTab = modelDetailTabs.find((t) => t.id === activeTabId);

  const loadModel = useCallback(async () => {
    if (!id) {
      setError("Model ID not provided");
      setLoading(false);
      return;
    }
    setLoading(true);
    setError(null);
    try {
      const response = await fetchModelById(id);
      if (response?.model?.id && response.model.name) {
        setModel(response.model);
      } else {
        setError("Model not found or invalid response structure");
      }
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to load model";
      setError(message);
      toast.error("Failed to load model. Check your connection.");
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    loadModel();
  }, [loadModel]);

  const carouselItems = useMemo(
    () => (model ? buildCarouselItems(model) : []),
    [model]
  );

  const isOwner =
    model &&
    isAuthenticated &&
    user &&
    (user.id === model.authorId || user.id === model.author?.id);

  const handleDownload = useCallback(async () => {
    if (!model?.id || !isAuthenticated) {
      toast.error("Sign in to download models");
      return;
    }
    try {
      const client = ApiClientFactory.getApiClient();
      const response = await client.downloadModel_DownloadModel(model.id);
      if (response?.data instanceof Blob && response.data.size > 0) {
        const url = URL.createObjectURL(response.data);
        const link = document.createElement("a");
        link.href = url;
        link.download = response.fileName ?? `${model.name ?? "model"}.zip`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        setTimeout(() => URL.revokeObjectURL(url), 5000);
      } else {
        toast.error("Download failed: No file data received");
      }
    } catch (err) {
      toast.error(
        "Download failed: " + (err instanceof Error ? err.message : "Unknown error")
      );
    }
  }, [model, isAuthenticated]);

  const handleFileDownload = useCallback(
    async (file: { id: string; name: string }) => {
      if (!model?.id || !isAuthenticated) {
        toast.error("Sign in to download files");
        return;
      }
      setDownloadingFiles((prev) => new Set(prev).add(file.id));
      try {
        const client = ApiClientFactory.getApiClient();
        const response = await client.streamFile_StreamModelFile(
          model.id,
          file.name
        );
        if (response?.data instanceof Blob) {
          const url = URL.createObjectURL(response.data);
          const link = document.createElement("a");
          link.href = url;
          link.download = file.name;
          document.body.appendChild(link);
          link.click();
          document.body.removeChild(link);
          URL.revokeObjectURL(url);
        }
      } catch {
        toast.error("File download failed");
      } finally {
        setDownloadingFiles((prev) => {
          const next = new Set(prev);
          next.delete(file.id);
          return next;
        });
      }
    },
    [model?.id, isAuthenticated]
  );

  const handleDelete = useCallback(async () => {
    if (!model?.id || !user?.accessToken) {
      toast.error("Unable to delete: Missing authentication");
      return;
    }
    setIsDeleting(true);
    try {
      const client = ApiClientFactory.getApiClient();
      await client.deleteModel_DeleteModel(model.id);
      toast.success("Model deleted");
      router.push("/dashboard");
    } catch (err) {
      toast.error(
        "Delete failed: " + (err instanceof Error ? err.message : "Unknown error")
      );
    } finally {
      setIsDeleting(false);
      setShowDeleteConfirm(false);
    }
  }, [model?.id, user?.accessToken, router]);

  const handleShare = useCallback(() => {
    if (typeof window !== "undefined") {
      navigator.clipboard
        .writeText(window.location.href)
        .then(() => toast.success("Link copied to clipboard"))
        .catch(() => toast.error("Failed to copy link"));
    }
  }, []);

  const handleCarouselLoadError = useCallback((itemId: string, message: string) => {
    toast.error(`Failed to load preview: ${message}`);
  }, []);

  const handleModelUpdate = useCallback((updated: Model) => {
    setModel(updated);
    toast.success("Model updated");
  }, []);

  const handleVersionCreated = useCallback(() => {
    setShowEditModal(false);
    loadModel();
    toast.success("Version created successfully");
  }, [loadModel]);

  const handleVersionUpdate = useCallback(
    (versionId: string, updated: { name?: string; notes?: string }) => {
      setModel((prev) => {
        if (!prev?.versions) return prev;
        return {
          ...prev,
          versions: prev.versions.map((v) =>
            v.id === versionId ? { ...v, ...updated } : v
          ),
        } as Model;
      });
      toast.success("Version updated");
    },
    []
  );

  if (loading) {
    return (
      <div className="flex min-h-[50vh] items-center justify-center">
        <div className="h-12 w-12 animate-spin rounded-full border-2 border-white/30 border-t-white" />
      </div>
    );
  }

  if (error || !model) {
    return (
      <div className="mx-auto max-w-2xl px-4 py-16 text-center">
        <h1 className="mb-4 text-2xl font-semibold text-white">
          Model Not Found
        </h1>
        <p className="mb-6 text-white/80">{error ?? "Model not found"}</p>
        <div className="flex flex-col items-center gap-4 sm:flex-row sm:justify-center">
          <Button variant="glass" asChild>
            <Link href="/dashboard">Back to Dashboard</Link>
          </Button>
          <Button variant="outline" onClick={loadModel}>
            Retry
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-[95rem] px-3 py-4 sm:px-4 sm:py-6 md:px-6 lg:px-8 lg:py-8">
      <nav className="mb-6" aria-label="Breadcrumb">
        <ol className="flex items-center gap-2">
          <li>
            <Link
              href="/dashboard"
              className="text-white/60 hover:text-white"
            >
              3D Models
            </Link>
          </li>
          <li className="text-white/40">/</li>
          <li>
            <span className="font-medium text-white">{model.name}</span>
          </li>
        </ol>
      </nav>

      <div className="grid gap-6 lg:grid-cols-6 lg:gap-8 xl:grid-cols-7 xl:gap-12 2xl:grid-cols-8">
        <div className="lg:col-span-4 xl:col-span-5 2xl:col-span-6 space-y-6">
          <ModelDetailsCarousel
            modelId={model.id!}
            items={carouselItems}
            onLoadError={handleCarouselLoadError}
          />

          {modelDetailTabs.length > 0 && (
            <>
              <nav
                className="flex gap-2 overflow-x-auto"
                role="tablist"
                aria-label="Model details"
              >
                {modelDetailTabs.map((tab) => {
                  const Icon = tab.icon;
                  return (
                    <button
                      key={tab.id}
                      type="button"
                      role="tab"
                      aria-selected={tab.id === activeTabId}
                      onClick={() => setSelectedTabId(tab.id)}
                      className={`flex items-center gap-2 whitespace-nowrap rounded-md px-4 py-2 text-sm font-medium transition-colors ${
                        tab.id === activeTabId
                          ? "bg-white/20 text-white"
                          : "text-white/60 hover:bg-white/10 hover:text-white"
                      }`}
                    >
                      <Icon className="h-4 w-4" />
                      {tab.label}
                    </button>
                  );
                })}
              </nav>
              {activeTab && (() => {
                const TabComponent = activeTab.component;
                return (
                  <PluginBoundary pluginId={activeTab.id}>
                    <TabComponent model={model} />
                  </PluginBoundary>
                );
              })()}
            </>
          )}

          <Card variant="glass" className="border-white/20">
            <CardHeader>
              <CardTitle className="text-white">Description</CardTitle>
            </CardHeader>
            <CardContent>
              <p className="text-white/80 leading-relaxed">
                {model.description || "No description provided."}
              </p>
            </CardContent>
          </Card>

          <ModelDetailsFiles
            files={model.files ?? []}
            downloadingFileIds={downloadingFiles}
            isAuthenticated={isAuthenticated}
            onFileDownload={handleFileDownload}
          />
        </div>

        <div className="lg:col-span-2">
          <ModelDetailsSidebar
            model={model}
            isOwner={!!isOwner}
            isFederated={!!model.isFederated}
            isAuthenticated={isAuthenticated}
            isDeleting={isDeleting}
            onDownload={handleDownload}
            onDelete={() => setShowDeleteConfirm(true)}
            onShare={handleShare}
            onEdit={() => setShowEditModal(true)}
          />
        </div>
      </div>

      <DeleteModelDialog
        isOpen={showDeleteConfirm}
        modelName={model.name ?? "this model"}
        isDeleting={isDeleting}
        onClose={() => setShowDeleteConfirm(false)}
        onConfirm={handleDelete}
      />

      <EditModelModal
        model={model}
        open={showEditModal}
        onOpenChange={setShowEditModal}
        onModelUpdate={handleModelUpdate}
        onVersionCreated={handleVersionCreated}
        onVersionUpdate={handleVersionUpdate}
      />
    </div>
  );
}
