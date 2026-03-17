"use client";

import React, {
  useState,
  useEffect,
  useCallback,
  Suspense,
  lazy,
} from "react";
import { Card } from "@/components/primitives/card";
import { ApiClientFactory } from "@/lib/api/clientFactory";
import { cn } from "@/lib/utils";

const ModelViewer = lazy(
  () => import("@/components/model-viewer/model-viewer")
);
const PDFViewer = lazy(() => import("@/components/viewers/pdf-viewer"));
const MarkdownViewer = lazy(() => import("@/components/viewers/markdown-viewer"));

export type CarouselItemType = "image" | "3d" | "pdf" | "markdown";

export interface CarouselItem {
  id: string;
  type: CarouselItemType;
  url?: string;
  fileName?: string;
  file?: File;
  mimeType?: string;
}

const MODEL_VIEWER_SUPPORTED = ["stl", "gltf", "glb", "3mf"];

function isModelViewerSupported(fileName: string): boolean {
  const ext = fileName
    .toLowerCase()
    .split(".")
    .pop();
  return ext ? MODEL_VIEWER_SUPPORTED.includes(ext) : false;
}

interface ModelDetailsCarouselProps {
  modelId: string;
  items: CarouselItem[];
  onLoadError?: (itemId: string, message: string) => void;
}

function ModelViewerFallback() {
  return (
    <div className="flex h-full min-h-[300px] items-center justify-center bg-white/5 text-white/60">
      <div className="text-center">
        <div className="mb-2 h-8 w-8 animate-pulse rounded-full bg-white/20" />
        <p className="text-sm">Loading 3D viewer...</p>
      </div>
    </div>
  );
}

export function ModelDetailsCarousel({
  modelId,
  items,
  onLoadError,
}: ModelDetailsCarouselProps) {
  const [activeIndex, setActiveIndex] = useState(0);
  const [loadedFiles, setLoadedFiles] = useState<Record<string, File>>({});

  const activeItem = items[activeIndex];

  const loadFile = useCallback(
    async (item: CarouselItem) => {
      if (!item.fileName || item.type === "image") return;
      const key = item.id;
      if (loadedFiles[key]) return;
      try {
        const client = ApiClientFactory.getApiClient();
        const resp = await client.streamFile_StreamModelFile(
          modelId,
          item.fileName
        );
        if (resp?.data instanceof Blob) {
          const file = new File([resp.data], item.fileName, {
            type: resp.data.type || "application/octet-stream",
          });
          setLoadedFiles((prev) => ({ ...prev, [key]: file }));
        }
      } catch (err) {
        onLoadError?.(key, err instanceof Error ? err.message : "Failed to load");
      }
    },
    [modelId, loadedFiles, onLoadError]
  );

  useEffect(() => {
    if (activeItem && activeItem.type !== "image" && activeItem.fileName) {
      loadFile(activeItem);
    }
  }, [activeItem?.id, activeItem?.type, activeItem?.fileName, loadFile]);

  const handleKeyDown = useCallback(
    (e: React.KeyboardEvent) => {
      if (e.key === "ArrowLeft") {
        e.preventDefault();
        setActiveIndex((i) => (i > 0 ? i - 1 : items.length - 1));
      } else if (e.key === "ArrowRight") {
        e.preventDefault();
        setActiveIndex((i) => (i < items.length - 1 ? i + 1 : 0));
      }
    },
    [items.length]
  );

  const renderActiveContent = () => {
    if (!activeItem) {
      return (
        <div className="flex h-full min-h-[300px] items-center justify-center bg-white/5 text-white/60">
          No preview available
        </div>
      );
    }

    if (activeItem.type === "image" && activeItem.url) {
      return (
        <img
          src={activeItem.url}
          alt=""
          className="h-full w-full object-contain"
        />
      );
    }

    if (activeItem.type === "3d" && activeItem.fileName) {
      const file = loadedFiles[activeItem.id];
      if (!file) {
        return (
          <div className="flex h-full min-h-[300px] items-center justify-center bg-white/5 text-white/60">
            <p className="text-sm">Loading model...</p>
          </div>
        );
      }
      if (!isModelViewerSupported(activeItem.fileName)) {
        return (
          <div className="flex h-full min-h-[300px] items-center justify-center bg-white/5 text-white/60">
            Preview not available for this file type
          </div>
        );
      }
      return (
        <Suspense fallback={<ModelViewerFallback />}>
          <ModelViewer file={file} fileName={activeItem.fileName} />
        </Suspense>
      );
    }

    if (activeItem.type === "pdf" && activeItem.fileName) {
      const file = loadedFiles[activeItem.id];
      if (!file) {
        return (
          <div className="flex h-full min-h-[300px] items-center justify-center bg-white/5 text-white/60">
            <p className="text-sm">Loading PDF...</p>
          </div>
        );
      }
      return (
        <Suspense fallback={<ModelViewerFallback />}>
          <PDFViewer
            file={file}
            width="100%"
            height="100%"
            className="h-full min-h-[300px]"
          />
        </Suspense>
      );
    }

    if (activeItem.type === "markdown" && activeItem.fileName) {
      const file = loadedFiles[activeItem.id];
      if (!file) {
        return (
          <div className="flex h-full min-h-[300px] items-center justify-center bg-white/5 text-white/60">
            <p className="text-sm">Loading markdown...</p>
          </div>
        );
      }
      return (
        <Suspense fallback={<ModelViewerFallback />}>
          <MarkdownViewer
            file={file}
            width="100%"
            height="100%"
            className="h-full min-h-[300px]"
          />
        </Suspense>
      );
    }

    return (
      <div className="flex h-full min-h-[300px] items-center justify-center bg-white/5 text-white/60">
        No preview available
      </div>
    );
  };

  if (items.length === 0) {
    return (
      <Card variant="glass" className="overflow-hidden border-white/20">
        <div
          className="flex min-h-[300px] items-center justify-center bg-white/5 text-white/60"
          style={{ aspectRatio: "16/9" }}
        >
          No preview available
        </div>
      </Card>
    );
  }

  return (
    <div className="space-y-4">
      <Card variant="glass" className="overflow-hidden border-white/20">
        <div
          className="relative bg-white/5"
          style={{
            aspectRatio: "16/9",
            minHeight: "300px",
            maxHeight: "70vh",
          }}
          id="carousel-viewer"
          role="region"
          aria-label="Model preview"
          onKeyDown={handleKeyDown}
          tabIndex={0}
        >
          {renderActiveContent()}
        </div>
      </Card>

      {items.length > 1 && (
        <div
          role="tablist"
          aria-label="Preview thumbnails"
          className="flex flex-wrap gap-2"
        >
          {items.map((item, index) => (
            <button
              key={item.id}
              type="button"
              role="tab"
              aria-selected={activeIndex === index}
              aria-controls="carousel-viewer"
              tabIndex={activeIndex === index ? 0 : -1}
              onClick={() => setActiveIndex(index)}
              className={cn(
                "flex h-16 w-16 shrink-0 overflow-hidden rounded-lg border-2 transition-all sm:h-20 sm:w-20",
                activeIndex === index
                  ? "border-blue-500 ring-2 ring-blue-500/50"
                  : "border-white/20 hover:border-white/40"
              )}
            >
              {item.type === "image" && item.url ? (
                <img
                  src={item.url}
                  alt=""
                  className="h-full w-full object-cover"
                />
              ) : item.type === "3d" ? (
                <div className="flex h-full w-full items-center justify-center bg-blue-600/30 text-xs text-white">
                  3D
                </div>
              ) : item.type === "pdf" ? (
                <div className="flex h-full w-full items-center justify-center bg-red-600/30 text-xs text-white">
                  PDF
                </div>
              ) : item.type === "markdown" ? (
                <div className="flex h-full w-full items-center justify-center bg-yellow-600/30 text-xs text-white">
                  MD
                </div>
              ) : (
                <div className="flex h-full w-full items-center justify-center bg-white/10 text-xs text-white">
                  File
                </div>
              )}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
