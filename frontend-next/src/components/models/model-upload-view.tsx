"use client";

import React, {
  useState,
  useRef,
  useEffect,
  useCallback,
} from "react";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { useAuth } from "@/contexts/AuthContext";
import { uploadModel } from "@/lib/services/modelsService";
import {
  getFileSettings,
  getEnabledFileTypesByCategory,
  getExtensionsByCategory,
  isFileTypeAllowed,
} from "@/lib/services/fileTypeSettingsService";
import {
  getModelConfigurationSettings,
} from "@/lib/services/modelConfigurationSettingsService";
import {
  parseModelMarkdown,
  isMarkdownFile,
  generateMarkdownTemplate,
} from "@/lib/utils/markdownParser";
import {
  extractZipFile,
  isValidZipFile,
  convertToFiles,
} from "@/lib/utils/zipExtractor";
import type { FileTypeSettingsData } from "@/lib/api/client";
import { PrivacySettings } from "@/lib/api/client";
import FileDropZone from "./file-drop-zone";
import FileQueue, { type UploadedFile } from "./file-queue";
import MetadataForm, { type ModelData } from "./metadata-form";
import ThumbnailGenerator from "./thumbnail-generator";
import ModelViewer from "@/components/model-viewer/model-viewer";
import PDFViewer from "@/components/viewers/pdf-viewer";
import MarkdownViewer from "@/components/viewers/markdown-viewer";
import { Button } from "@/components/primitives/button";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/primitives/card";

const MAX_FILES_PER_UPLOAD = 20;

function ImagePreview({ file }: { file: File }) {
  const [url, setUrl] = React.useState<string | null>(null);
  React.useEffect(() => {
    const u = URL.createObjectURL(file);
    setUrl(u);
    return () => URL.revokeObjectURL(u);
  }, [file]);
  if (!url) return null;
  return (
    <div className="w-full h-full flex items-center justify-center p-4">
      <img
        src={url}
        alt="Preview"
        className="max-w-full max-h-full object-contain"
      />
    </div>
  );
}

function createUploadedFile(file: File): UploadedFile {
  return {
    id: Math.random().toString(36).slice(2, 11),
    name: file.name,
    size: file.size,
    type: file.type,
    file,
    progress: 0,
    isThumbnail: false,
  };
}

export default function ModelUploadView() {
  const router = useRouter();
  const { user, isAuthenticated, isLoading: authLoading } = useAuth();
  const [fileTypeSettings, setFileTypeSettings] = useState<
    FileTypeSettingsData[] | undefined
  >(undefined);
  const [settingsLoading, setSettingsLoading] = useState(true);
  const [uploadedFiles, setUploadedFiles] = useState<UploadedFile[]>([]);
  const [previewFile, setPreviewFile] = useState<UploadedFile | null>(null);
  const [modelData, setModelData] = useState<ModelData>({
    title: "",
    description: "",
    privacy: PrivacySettings.Public,
    license: "MIT",
    categories: [],
    aiGenerated: false,
    workInProgress: false,
    nsfw: false,
    remix: false,
  });
  const [currentStep, setCurrentStep] = useState(1);
  const [isUploading, setIsUploading] = useState(false);
  const [isExtractingZip, setIsExtractingZip] = useState(false);
  const [showThumbnailGenerator, setShowThumbnailGenerator] = useState(false);

  const default3D = [".stl", ".glb", ".gltf", ".3mf"];
  const defaultImage = [".png", ".jpg", ".jpeg", ".gif", ".webp"];
  const defaultArchive = [".zip"];
  const defaultDoc = [".pdf", ".md", ".markdown"];

  const raw3D = getExtensionsByCategory(fileTypeSettings, "3D").map((e) =>
    e.startsWith(".") ? e : `.${e}`
  );
  const rawImage = getExtensionsByCategory(fileTypeSettings, "Image").map(
    (e) => (e.startsWith(".") ? e : `.${e}`)
  );
  const rawZip = getExtensionsByCategory(fileTypeSettings, "Archive").map(
    (e) => (e.startsWith(".") ? e : `.${e}`)
  );
  const rawDoc = getExtensionsByCategory(fileTypeSettings, "Document").map(
    (e) => (e.startsWith(".") ? e : `.${e}`)
  );

  const supported3DFormats = raw3D.length ? raw3D : default3D;
  const supportedImageFormats = rawImage.length ? rawImage : defaultImage;
  const supportedZipFormats = rawZip.length ? rawZip : defaultArchive;
  const supportedDocFormats = rawDoc.length ? rawDoc : defaultDoc;

  const allSupportedFormats = [
    ...supported3DFormats,
    ...supportedImageFormats,
    ...supportedDocFormats,
    ...supportedZipFormats,
  ];

  const canAddMoreFiles = uploadedFiles.length < MAX_FILES_PER_UPLOAD;

  useEffect(() => {
    if (!isAuthenticated && !authLoading) {
      router.replace("/");
      return;
    }
  }, [isAuthenticated, authLoading, router]);

  useEffect(() => {
    const load = async () => {
      try {
        setSettingsLoading(true);
        const [fileResp, configResp] = await Promise.all([
          getFileSettings(),
          getModelConfigurationSettings(),
        ]);
        if (fileResp.fileTypes) setFileTypeSettings(fileResp.fileTypes);
        if (configResp.settings?.defaultPrivacySetting) {
          const p = String(
            configResp.settings.defaultPrivacySetting
          ).toLowerCase();
          if (p === "public")
            setModelData((prev) => ({ ...prev, privacy: PrivacySettings.Public }));
          else if (p === "private")
            setModelData((prev) => ({
              ...prev,
              privacy: PrivacySettings.Private,
            }));
          else if (p === "unlisted")
            setModelData((prev) => ({
              ...prev,
              privacy: PrivacySettings.Unlisted,
            }));
        }
      } catch (err) {
        console.error(err);
        toast.error("Failed to load settings");
      } finally {
        setSettingsLoading(false);
      }
    };
    load();
  }, []);

  const getFileType = useCallback(
    (fileName: string): "3d" | "image" | "pdf" | "markdown" | "unknown" => {
      const ext = fileName.toLowerCase().substring(fileName.lastIndexOf("."));
      if (supported3DFormats.includes(ext)) return "3d";
      if (supportedImageFormats.includes(ext)) return "image";
      if (fileName.toLowerCase().endsWith(".pdf")) return "pdf";
      if (isMarkdownFile(fileName)) return "markdown";
      return "unknown";
    },
    [supported3DFormats, supportedImageFormats]
  );

  const processZipFile = useCallback(
    async (file: File) => {
      if (!canAddMoreFiles) {
        toast.error(
          `Maximum of ${MAX_FILES_PER_UPLOAD} files allowed. Remove some before extracting.`
        );
        return;
      }
      const valid = await isValidZipFile(file);
      if (!valid) {
        toast.error("Invalid zip file");
        return;
      }
      setIsExtractingZip(true);
      const result = await extractZipFile(file);
      setIsExtractingZip(false);
      if (!result.success) {
        toast.error(result.error ?? "Failed to extract zip");
        return;
      }
      const files = convertToFiles(result.files);
      const remaining = MAX_FILES_PER_UPLOAD - uploadedFiles.length;
      const toAdd = files.slice(0, remaining);
      const newUploaded = toAdd.map(createUploadedFile);
      setUploadedFiles((prev) => [...prev, ...newUploaded]);
      const firstPreview = newUploaded.find(
        (f) =>
          getFileType(f.name) !== "unknown"
      );
      if (firstPreview && !previewFile)
        setPreviewFile(firstPreview);
      toast.success(
        `Zip extracted: ${toAdd.length} file(s) added. ${files.length - toAdd.length} skipped (max reached).`
      );
    },
    [canAddMoreFiles, uploadedFiles.length, previewFile, getFileType]
  );

  const processMarkdownFile = useCallback(async (file: File) => {
    try {
      const text = await file.text();
      const parsed = parseModelMarkdown(text);
      setModelData((prev) => ({
        ...prev,
        ...(parsed.title && { title: parsed.title }),
        ...(parsed.description && { description: parsed.description }),
        ...(parsed.privacy && { privacy: parsed.privacy }),
        ...(parsed.license && { license: parsed.license }),
        ...(parsed.categories && { categories: parsed.categories }),
        ...(parsed.aiGenerated !== undefined && {
          aiGenerated: parsed.aiGenerated,
        }),
        ...(parsed.workInProgress !== undefined && {
          workInProgress: parsed.workInProgress,
        }),
        ...(parsed.nsfw !== undefined && { nsfw: parsed.nsfw }),
        ...(parsed.remix !== undefined && { remix: parsed.remix }),
      }));
      toast.success(`Model details populated from ${file.name}`);
    } catch {
      toast.error("Failed to parse markdown file");
    }
  }, []);

  const handleFilesSelected = useCallback(
    async (files: File[]) => {
      const toAdd: File[] = [];
      const toProcess: File[] = [];

      for (const file of files) {
        if (!isFileTypeAllowed(fileTypeSettings, file)) {
          toast.error(`File ${file.name} is not allowed or exceeds size limit`);
          continue;
        }
        const ext = file.name
          .toLowerCase()
          .substring(file.name.lastIndexOf("."));
        if (supportedZipFormats.includes(ext)) {
          toProcess.push(file);
          continue;
        }
        if (isMarkdownFile(file.name)) {
          toProcess.push(file);
          continue;
        }
        if (
          uploadedFiles.length + toAdd.length >= MAX_FILES_PER_UPLOAD
        ) {
          toast.warning(
            `Max ${MAX_FILES_PER_UPLOAD} files. Only first ${
              MAX_FILES_PER_UPLOAD - uploadedFiles.length
            } added.`
          );
          break;
        }
        toAdd.push(file);
      }

      for (const file of toProcess) {
        const ext = file.name
          .toLowerCase()
          .substring(file.name.lastIndexOf("."));
        if (supportedZipFormats.includes(ext)) await processZipFile(file);
        else if (isMarkdownFile(file.name)) await processMarkdownFile(file);
      }

      const newFiles = toAdd.map(createUploadedFile);
      setUploadedFiles((prev) => [...prev, ...newFiles]);
      const firstPreviewable = newFiles.find(
        (f) => getFileType(f.name) !== "unknown"
      );
      if (firstPreviewable && !previewFile) setPreviewFile(firstPreviewable);
    },
    [
      fileTypeSettings,
      supportedZipFormats,
      uploadedFiles.length,
      previewFile,
      processZipFile,
      processMarkdownFile,
      getFileType,
    ]
  );

  const handleSelectFile = (id: string) => {
    const f = uploadedFiles.find((x) => x.id === id);
    if (f) setPreviewFile(f);
  };

  const handleRemoveFile = (id: string) => {
    setUploadedFiles((prev) => {
      const next = prev.filter((x) => x.id !== id);
      if (previewFile?.id === id)
        setPreviewFile(next.length > 0 ? next[0] : null);
      return next;
    });
  };

  const handleThumbnailToggle = (id: string, checked: boolean) => {
    setUploadedFiles((prev) =>
      prev.map((f) => ({
        ...f,
        isThumbnail: checked ? f.id === id : f.id === id ? false : f.isThumbnail,
      }))
    );
  };

  const handleClearAll = () => {
    setUploadedFiles([]);
    setPreviewFile(null);
  };

  const handleUpload = async () => {
    if (uploadedFiles.length === 0) return;
    setIsUploading(true);
    try {
      const thumbnailFile = uploadedFiles.find((f) => f.isThumbnail);
      const result = await uploadModel({
        modelData: {
          name: modelData.title,
          description: modelData.description,
          privacy: modelData.privacy,
          license: modelData.license,
          categories: modelData.categories,
          aiGenerated: modelData.aiGenerated,
          workInProgress: modelData.workInProgress,
          nsfw: modelData.nsfw,
          remix: modelData.remix,
          thumbnailFileId: thumbnailFile?.name,
        },
        files: uploadedFiles.map((f) => f.file),
      });
      toast.success("Model uploaded successfully");
      router.push(result.id ? `/models/${result.id}` : "/");
    } catch (err) {
      toast.error(
        err instanceof Error ? err.message : "Upload failed"
      );
    } finally {
      setIsUploading(false);
    }
  };

  const downloadMarkdownTemplate = () => {
    const blob = new Blob([generateMarkdownTemplate()], {
      type: "text/markdown",
    });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = "model-template.md";
    a.click();
    URL.revokeObjectURL(url);
  };

  if (authLoading || !isAuthenticated) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <p className="text-muted-foreground">Loading...</p>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-4xl px-4 py-8 space-y-8">
      <h1 className="text-2xl font-semibold text-white">Upload New Model</h1>
      <div className="flex gap-2 text-sm">
          {[1, 2, 3].map((step) => (
            <button
              key={step}
              type="button"
              onClick={() => setCurrentStep(step)}
              className={`px-3 py-1 rounded-full border ${
                currentStep === step
                  ? "bg-primary border-primary text-primary-foreground"
                  : "border-white/20 text-muted-foreground hover:border-white/40"
              }`}
            >
              {step === 1 && "Files"}
              {step === 2 && "Preview & Metadata"}
              {step === 3 && "Review"}
            </button>
          ))}
        </div>

        {currentStep === 1 && (
          <div className="space-y-6">
            {settingsLoading ? (
              <Card variant="glass" className="p-12 text-center">
                <p className="text-muted-foreground">
                  Loading file type settings...
                </p>
              </Card>
            ) : (
              <>
                <FileDropZone
                  onFilesSelected={handleFilesSelected}
                  acceptFormats={allSupportedFormats}
                  canAddMore={canAddMoreFiles}
                  maxFiles={MAX_FILES_PER_UPLOAD}
                  variant={uploadedFiles.length > 0 ? "compact" : "large"}
                  disabled={isExtractingZip}
                />
                <Card variant="glass" className="p-4">
                  <FileQueue
                    files={uploadedFiles}
                    selectedFileId={previewFile?.id ?? null}
                    onSelectFile={handleSelectFile}
                    onRemoveFile={handleRemoveFile}
                    onThumbnailToggle={handleThumbnailToggle}
                    getFileType={getFileType}
                    supportedImageFormats={supportedImageFormats}
                    maxFiles={MAX_FILES_PER_UPLOAD}
                    onClearAll={handleClearAll}
                  />
                </Card>
                <div className="flex justify-between items-center">
                  <button
                    type="button"
                    onClick={downloadMarkdownTemplate}
                    className="text-sm text-primary hover:underline"
                  >
                    Download markdown template
                  </button>
                  {uploadedFiles.length > 0 && (
                    <Button onClick={() => setCurrentStep(2)}>
                      Next: Preview & Metadata
                    </Button>
                  )}
                </div>
              </>
            )}
          </div>
        )}

        {currentStep === 2 && (
          <div className="space-y-6">
            <Card variant="glass" className="overflow-hidden">
              <CardHeader>
                <CardTitle>
                  Preview {previewFile && `: ${previewFile.name}`}
                </CardTitle>
              </CardHeader>
              <CardContent className="p-0">
                <div className="h-96 w-full bg-muted/30">
                  {previewFile ? (
                    (() => {
                      const ft = getFileType(previewFile.name);
                      if (ft === "3d")
                        return (
                          <ModelViewer
                            file={previewFile.file}
                            fileName={previewFile.name}
                            autoRotate
                          />
                        );
                      if (ft === "image")
                        return (
                          <ImagePreview file={previewFile.file} />
                        );
                      if (ft === "pdf")
                        return (
                          <PDFViewer
                            file={previewFile.file}
                            width="100%"
                            height={384}
                            className="h-96"
                          />
                        );
                      if (ft === "markdown")
                        return (
                          <MarkdownViewer
                            file={previewFile.file}
                            width="100%"
                            height={384}
                            className="h-96"
                          />
                        );
                      return (
                        <div className="w-full h-full flex items-center justify-center text-muted-foreground">
                          Preview not available for this file type
                        </div>
                      );
                    })()
                  ) : (
                    <div className="w-full h-full flex items-center justify-center text-muted-foreground">
                      Select a file from the queue to preview
                    </div>
                  )}
                </div>
              </CardContent>
            </Card>

            <Card variant="glass" className="p-6">
              <MetadataForm
                data={modelData}
                onChange={(field, value) =>
                  setModelData((prev) => ({ ...prev, [field]: value }))
                }
                onCancel={() => router.push("/")}
                onSubmit={() => setCurrentStep(3)}
                isSubmitting={false}
                submitLabel="Next: Review"
              />
            </Card>
          </div>
        )}

        {currentStep === 3 && (
          <div className="space-y-6">
            <Card variant="glass" className="p-6">
              <h2 className="text-lg font-semibold mb-4">Review & Upload</h2>
              <div className="space-y-2 text-sm text-muted-foreground">
                <p>
                  <span className="text-foreground font-medium">Title:</span>{" "}
                  {modelData.title || "(untitled)"}
                </p>
                <p>
                  <span className="text-foreground font-medium">Files:</span>{" "}
                  {uploadedFiles.length}
                </p>
                {previewFile && (
                  <p>
                    <span className="text-foreground font-medium">
                      Preview:
                    </span>{" "}
                    {previewFile.name}
                  </p>
                )}
              </div>
              <div className="flex gap-4 mt-6">
                <Button variant="outline" onClick={() => setCurrentStep(2)}>
                  Back
                </Button>
                <Button
                  onClick={handleUpload}
                  disabled={isUploading || uploadedFiles.length === 0}
                >
                  {isUploading ? "Uploading..." : "Upload Model"}
                </Button>
              </div>
            </Card>
          </div>
        )}

      {showThumbnailGenerator && previewFile && (
        <ThumbnailGenerator
          modelFile={previewFile.file}
          onThumbnailGenerated={() => setShowThumbnailGenerator(false)}
          onCancel={() => setShowThumbnailGenerator(false)}
        />
      )}
    </div>
  );
}
