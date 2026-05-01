"use client";

import { useState, useCallback } from "react";
import dynamic from "next/dynamic";
import { toast } from "sonner";
import type { Model } from "@/lib/api/client";
import { getApiConfig } from "@/lib/api/config";
import { getStoredUser } from "@/lib/auth/authSession";
import FileDropZone from "../file-drop-zone";
import FileQueue, { type UploadedFile } from "../file-queue";
import { Input } from "@/components/primitives/input";
import { Textarea } from "@/components/ui/glass/textarea";
import { Button } from "@/components/primitives/button";
import { Card, CardContent } from "@/components/primitives/card";
import { Skeleton } from "@/components/ui/skeleton";
import { ImageIcon } from "lucide-react";
import {
  MAX_FILES_PER_UPLOAD,
  SUPPORTED_3D_FORMATS,
  SUPPORTED_DOCUMENT_FORMATS,
  SUPPORTED_IMAGE_FORMATS,
  createUploadedFile,
  getUploadFileType,
  setThumbnailSelection,
} from "../upload-shared";

const ThumbnailGenerator = dynamic(
  () =>
    import("./thumbnail-generator").then((m) => ({
      default: m.ThumbnailGenerator,
    })),
  {
    ssr: false,
    loading: () => (
      <Skeleton className="h-64 w-full rounded-lg" />
    ),
  }
);

const SUPPORTED_FORMATS = [...SUPPORTED_3D_FORMATS, ...SUPPORTED_IMAGE_FORMATS, ...SUPPORTED_DOCUMENT_FORMATS];

interface ModelVersionManagerProps {
  model: Model;
  onCreateVersion: () => void;
  onCancel: () => void;
}

export function ModelVersionManager({
  model,
  onCreateVersion,
  onCancel,
}: ModelVersionManagerProps) {
  const [versionName, setVersionName] = useState(
    `${model.name ?? "Model"} v${(model.versions?.length ?? 0) + 1}`
  );
  const [versionNotes, setVersionNotes] = useState("");
  const [uploadedFiles, setUploadedFiles] = useState<UploadedFile[]>([]);
  const [selectedThumbnailFileId, setSelectedThumbnailFileId] = useState<string | null>(null);
  const [selectedFileId, setSelectedFileId] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showThumbnailGenerator, setShowThumbnailGenerator] = useState(false);

  const versionCount = model.versions?.length ?? 0;
  const canAddMore = uploadedFiles.length < MAX_FILES_PER_UPLOAD;
  const has3DFile = uploadedFiles.some((fileItem) => getUploadFileType(fileItem.name, SUPPORTED_3D_FORMATS, SUPPORTED_IMAGE_FORMATS) === "3d");
  const first3DFile = uploadedFiles.find((fileItem) => getUploadFileType(fileItem.name, SUPPORTED_3D_FORMATS, SUPPORTED_IMAGE_FORMATS) === "3d");

  const handleFilesSelected = useCallback((files: File[]) => {
    const remaining = MAX_FILES_PER_UPLOAD - uploadedFiles.length;
    const takenNames = new Set(uploadedFiles.map((fileItem) => fileItem.name));
    const toAdd = files
      .filter((fileItem) => {
        const ext = fileItem.name.toLowerCase().substring(fileItem.name.lastIndexOf("."));
        return SUPPORTED_FORMATS.includes(ext);
      })
      .slice(0, remaining);
    const newFiles = toAdd.map((fileItem) => {
      const nextFile = createUploadedFile(fileItem, takenNames);
      takenNames.add(nextFile.name);
      return nextFile;
    });
    setUploadedFiles((prev) => [...prev, ...newFiles]);
    if (!selectedFileId && newFiles.length > 0) {
      setSelectedFileId(newFiles[0].id);
    }
  }, [uploadedFiles, selectedFileId]);

  const handleSelectFile = useCallback((id: string) => {
    setSelectedFileId(id);
  }, []);

  const handleRemoveFile = useCallback((id: string) => {
    setUploadedFiles((prev) => prev.filter((fileItem) => fileItem.id !== id));
    if (selectedFileId === id) {
      const nextSelected = uploadedFiles.find((fileItem) => fileItem.id !== id);
      setSelectedFileId(nextSelected?.id ?? null);
    }
    if (selectedThumbnailFileId === id) {
      setSelectedThumbnailFileId(null);
    }
  }, [selectedFileId, selectedThumbnailFileId, uploadedFiles]);

  const handleThumbnailToggle = useCallback((id: string, checked: boolean) => {
    const selectedId = checked ? id : null;
    setSelectedThumbnailFileId(selectedId);
    setUploadedFiles((prev) => setThumbnailSelection(prev, selectedId));
  }, []);

  const handleClearAll = useCallback(() => {
    setUploadedFiles([]);
    setSelectedFileId(null);
    setSelectedThumbnailFileId(null);
  }, []);

  const handleThumbnailGenerated = useCallback(
    (blob: Blob, fileName: string) => {
      const file = new File([blob], fileName, { type: "image/png" });
      const newUploaded: UploadedFile = {
        ...createUploadedFile(file, new Set(uploadedFiles.map((fileItem) => fileItem.name))),
      };
      setUploadedFiles((prev) => setThumbnailSelection([...prev, newUploaded], newUploaded.id));
      setSelectedThumbnailFileId(newUploaded.id);
      setSelectedFileId(newUploaded.id);
      toast.success("Thumbnail generated and added to queue");
    },
    [uploadedFiles]
  );

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!model.id || !has3DFile || !versionName.trim()) return;

    setIsSubmitting(true);
    try {
      const token = typeof window !== "undefined" ? getStoredUser()?.accessToken : null;
      if (!token) {
        toast.error("Authentication required");
        return;
      }

      const formData = new FormData();
      formData.append("Name", versionName.trim());
      formData.append("Notes", versionNotes.trim());

      const thumbnailFile = uploadedFiles.find(
        (fileItem) =>
          fileItem.id === selectedThumbnailFileId &&
          getUploadFileType(fileItem.name, SUPPORTED_3D_FORMATS, SUPPORTED_IMAGE_FORMATS) === "image"
      );
      if (thumbnailFile) {
        formData.append("ThumbnailFileId", thumbnailFile.name);
      }

      uploadedFiles.forEach((uf) => {
        formData.append("Files", uf.file);
      });

      const { baseUrl } = getApiConfig();
      const response = await fetch(`${baseUrl}/api/models/${model.id}/versions`, {
        method: "POST",
        headers: { Authorization: `Bearer ${token}` },
        body: formData,
      });

      if (!response.ok) {
        const errText = await response.text();
        throw new Error(errText || `Upload failed: ${response.statusText}`);
      }

      onCreateVersion();
    } catch (err) {
      toast.error(err instanceof Error ? err.message : "Failed to create version");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6 py-4">
      <Card variant="glass" className="border-white/20">
        <CardContent className="pt-6">
          <div className="space-y-1 mb-4">
            <h3 className="text-lg font-semibold text-white">
              {model.name ?? "Untitled Model"}
            </h3>
            <p className="text-sm text-white/60">
              Current: v{versionCount} · Creating: v{versionCount + 1}
            </p>
          </div>

          <div className="grid grid-cols-1 gap-4">
            <div>
              <label htmlFor="version-name" className="block text-sm font-medium text-white mb-2">
                Version Name *
              </label>
              <Input
                id="version-name"
                variant="glass"
                value={versionName}
                onChange={(e) => setVersionName(e.target.value)}
                placeholder="Enter version name"
                required
                disabled={isSubmitting}
              />
            </div>
            <div>
              <label htmlFor="version-notes" className="block text-sm font-medium text-white mb-2">
                Version Notes
              </label>
              <Textarea
                id="version-notes"
                variant="glass"
                value={versionNotes}
                onChange={(e) => setVersionNotes(e.target.value)}
                placeholder="Describe what's new in this version..."
                rows={4}
                className="resize-none"
                disabled={isSubmitting}
              />
            </div>
          </div>
        </CardContent>
      </Card>

      <div className="rounded-lg border border-white/20 bg-white/5 p-3 text-sm text-white/70">
        Version notes, files, and thumbnail are persisted. Bill of materials and print settings are managed in Edit Versions and remain local-only for now.
      </div>

      <div>
        <label className="block text-sm font-medium text-white mb-2">
          Files for New Version *
        </label>
        <FileDropZone
          onFilesSelected={handleFilesSelected}
          acceptFormats={SUPPORTED_FORMATS}
          canAddMore={canAddMore}
          maxFiles={MAX_FILES_PER_UPLOAD}
          variant="large"
          disabled={isSubmitting}
        />
      </div>

      {uploadedFiles.length > 0 && has3DFile && (
        <div className="flex justify-end">
          <Button
            type="button"
            variant="outline"
            size="sm"
            onClick={() => setShowThumbnailGenerator(true)}
            disabled={isSubmitting}
            aria-label="Generate thumbnail from 3D model"
          >
            <ImageIcon className="h-4 w-4 mr-2" />
            Generate Thumbnail from 3D
          </Button>
        </div>
      )}

      {uploadedFiles.length > 0 && (
        <FileQueue
          files={uploadedFiles}
          selectedFileId={selectedFileId}
          onSelectFile={handleSelectFile}
          onRemoveFile={handleRemoveFile}
          onThumbnailToggle={handleThumbnailToggle}
          getFileType={(fileName) => getUploadFileType(fileName, SUPPORTED_3D_FORMATS, SUPPORTED_IMAGE_FORMATS)}
          supportedImageFormats={SUPPORTED_IMAGE_FORMATS}
          maxFiles={MAX_FILES_PER_UPLOAD}
          onClearAll={handleClearAll}
        />
      )}

      {uploadedFiles.length > 0 && !has3DFile && (
        <div className="p-4 rounded-lg border border-amber-500/30 bg-amber-500/10 text-amber-200 text-sm">
          At least one 3D model file (.stl, .obj, .fbx, .gltf, .glb, .3mf, .step, .stp) is required.
        </div>
      )}

      {first3DFile && (
        <ThumbnailGenerator
          modelFile={first3DFile.file}
          open={showThumbnailGenerator}
          onOpenChange={setShowThumbnailGenerator}
          onThumbnailGenerated={handleThumbnailGenerated}
        />
      )}

      <div className="flex justify-end gap-4 pt-6 border-t border-white/10">
        <Button type="button" variant="outline" onClick={onCancel} disabled={isSubmitting}>
          Cancel
        </Button>
        <Button
          type="submit"
          variant="glass"
          disabled={isSubmitting || !versionName.trim() || !has3DFile}
        >
          {isSubmitting ? "Creating Version..." : "Create Version"}
        </Button>
      </div>
    </form>
  );
}
