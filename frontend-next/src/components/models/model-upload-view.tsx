"use client";

import React, {
  useState,
  useEffect,
  useCallback,
} from "react";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { useAuth } from "@/contexts/AuthContext";
import { uploadModel } from "@/lib/services/modelsService";
import {
  getFileSettings,
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
import UploadPreviewCarousel from "./upload-preview-carousel";
import { Button } from "@/components/primitives/button";
import {
  Card,
} from "@/components/primitives/card";
import { cn } from "@/lib/utils";
import { getUploadDefaults } from "@/lib/services/contentDefaultsService";
import {
  MAX_FILES_PER_UPLOAD,
  createUploadedFile,
  getUploadFileType,
  setThumbnailSelection,
} from "./upload-shared";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";

export default function ModelUploadView() {
  const router = useRouter();
  const { isAuthenticated, isLoading: authLoading } = useAuth();
  const localUploadDefaults = getUploadDefaults();
  const [fileTypeSettings, setFileTypeSettings] = useState<
    FileTypeSettingsData[] | undefined
  >(undefined);
  const [settingsLoading, setSettingsLoading] = useState(true);
  const [uploadedFiles, setUploadedFiles] = useState<UploadedFile[]>([]);
  const [previewFile, setPreviewFile] = useState<UploadedFile | null>(null);
  const [selectedThumbnailFileId, setSelectedThumbnailFileId] = useState<string | null>(null);
  const [modelData, setModelData] = useState<ModelData>({
    title: "",
    description: "",
    privacy: localUploadDefaults.privacy as PrivacySettings,
    license: localUploadDefaults.license,
    categories: [],
    aiGenerated: localUploadDefaults.aiGenerated,
    workInProgress: localUploadDefaults.workInProgress,
    nsfw: localUploadDefaults.nsfw,
    remix: localUploadDefaults.remix,
  });
  const [currentStep, setCurrentStep] = useState(1);
  const [isUploading, setIsUploading] = useState(false);
  const [isExtractingZip, setIsExtractingZip] = useState(false);
  const [showThumbnailGenerator, setShowThumbnailGenerator] = useState(false);

  const default3D = [".stl", ".obj", ".fbx", ".glb", ".gltf", ".3mf", ".step", ".stp"];
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
      return getUploadFileType(fileName, supported3DFormats, supportedImageFormats);
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
      const takenNames = new Set(uploadedFiles.map((uploadedFile) => uploadedFile.name));
      const newUploaded = toAdd.map((fileItem) => {
        const nextFile = createUploadedFile(fileItem, takenNames);
        takenNames.add(nextFile.name);
        return nextFile;
      });
      setUploadedFiles((prev) => [...prev, ...newUploaded]);
      const firstPreview = newUploaded.find(
        (fileItem) =>
          getFileType(fileItem.name) !== "unknown"
      );
      if (firstPreview && !previewFile)
        setPreviewFile(firstPreview);
      toast.success(
        `Zip extracted: ${toAdd.length} file(s) added. ${files.length - toAdd.length} skipped (max reached).`
      );
    },
    [canAddMoreFiles, uploadedFiles, previewFile, getFileType]
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

      const takenNames = new Set(uploadedFiles.map((uploadedFile) => uploadedFile.name));
      const newFiles = toAdd.map((fileItem) => {
        const nextFile = createUploadedFile(fileItem, takenNames);
        takenNames.add(nextFile.name);
        return nextFile;
      });
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
      if (selectedThumbnailFileId === id) {
        setSelectedThumbnailFileId(null);
      }
      if (previewFile?.id === id)
        setPreviewFile(next.length > 0 ? next[0] : null);
      return next;
    });
  };

  const handleThumbnailToggle = (id: string, checked: boolean) => {
    const selectedId = checked ? id : null;
    setSelectedThumbnailFileId(selectedId);
    setUploadedFiles((prev) => setThumbnailSelection(prev, selectedId));
  };

  const handleClearAll = () => {
    setUploadedFiles([]);
    setPreviewFile(null);
    setSelectedThumbnailFileId(null);
  };

  const handleThumbnailSelected = (value: string) => {
    const selectedId = value === "none" ? null : value;
    setSelectedThumbnailFileId(selectedId);
    setUploadedFiles((prev) => setThumbnailSelection(prev, selectedId));
  };

  const handleThumbnailGenerated = useCallback(
    (blob: Blob, fileName: string) => {
      const generatedFile = new File([blob], fileName, { type: "image/png" });
      const takenNames = new Set(uploadedFiles.map((uploadedFile) => uploadedFile.name));
      const generatedUploaded = createUploadedFile(generatedFile, takenNames);
      setUploadedFiles((prev) => {
        const next = [...prev, generatedUploaded];
        return setThumbnailSelection(next, generatedUploaded.id);
      });
      setSelectedThumbnailFileId(generatedUploaded.id);
      setPreviewFile(generatedUploaded);
      toast.success("Generated thumbnail added to upload queue");
    },
    [uploadedFiles]
  );

  const thumbnailCandidates = uploadedFiles.filter(
    (fileItem) => getFileType(fileItem.name) === "image"
  );

  const selectedThumbnailFile = uploadedFiles.find(
    (fileItem) => fileItem.id === selectedThumbnailFileId
  );

  useEffect(() => {
    if (selectedThumbnailFileId && !uploadedFiles.some((fileItem) => fileItem.id === selectedThumbnailFileId)) {
      setSelectedThumbnailFileId(null);
      setUploadedFiles((prev) => setThumbnailSelection(prev, null));
    }
  }, [selectedThumbnailFileId, uploadedFiles]);

  useEffect(() => {
    setUploadedFiles((prev) => setThumbnailSelection(prev, selectedThumbnailFileId));
  }, [selectedThumbnailFileId]);

  const canGenerateThumbnail = previewFile ? getFileType(previewFile.name) === "3d" : false;

  const reviewThumbnailPreviewUrl = React.useMemo(() => {
    if (!selectedThumbnailFile) return null;
    const objectUrl = URL.createObjectURL(selectedThumbnailFile.file);
    return objectUrl;
  }, [selectedThumbnailFile]);

  useEffect(() => {
    return () => {
      if (reviewThumbnailPreviewUrl) URL.revokeObjectURL(reviewThumbnailPreviewUrl);
    };
  }, [reviewThumbnailPreviewUrl]);

  const getThumbnailFileIdForUpload = () => {
    if (!selectedThumbnailFileId) return undefined;
    return uploadedFiles.find((fileItem) => fileItem.id === selectedThumbnailFileId)?.name;
  };

  const handleUpload = async () => {
    if (uploadedFiles.length === 0) return;
    setIsUploading(true);
    try {
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
          thumbnailFileId: getThumbnailFileIdForUpload(),
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
    <div
      className={cn(
        "mx-auto w-full px-4 py-6 sm:py-8 space-y-6 sm:space-y-8",
        currentStep === 1 ? "max-w-xl lg:max-w-2xl" : "max-w-4xl"
      )}
    >
      <h1 className="text-xl sm:text-2xl font-semibold tracking-tight text-white">
        Upload New Model
      </h1>
      <div className="flex flex-wrap gap-2 text-xs sm:text-sm">
          {[1, 2, 3].map((step) => (
            <button
              key={step}
              type="button"
              onClick={() => setCurrentStep(step)}
              className={`px-2.5 py-1 rounded-full border transition-colors ${
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
            <UploadPreviewCarousel
              files={uploadedFiles}
              activeFileId={previewFile?.id ?? null}
              getFileType={getFileType}
              onActiveFileChange={handleSelectFile}
              onOpenThumbnailGenerator={() => setShowThumbnailGenerator(true)}
            />

            <Card variant="glass" className="p-6 space-y-4">
              <div className="space-y-2">
                <h3 className="text-lg font-medium">Thumbnail Selection</h3>
                <p className="text-sm text-muted-foreground">
                  Choose which image should be used as the model thumbnail.
                </p>
              </div>
              <Select
                value={selectedThumbnailFileId ?? "none"}
                onValueChange={handleThumbnailSelected}
              >
                <SelectTrigger variant="glass" className="w-full">
                  <SelectValue placeholder="Select thumbnail image" />
                </SelectTrigger>
                <SelectContent variant="glass">
                  <SelectItem value="none">No thumbnail selected</SelectItem>
                  {thumbnailCandidates.map((fileItem) => (
                    <SelectItem key={fileItem.id} value={fileItem.id}>
                      {fileItem.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              {canGenerateThumbnail && (
                <Button
                  variant="outline"
                  size="sm"
                  className="w-fit"
                  onClick={() => setShowThumbnailGenerator(true)}
                >
                  Generate thumbnail from current 3D preview
                </Button>
              )}
              {thumbnailCandidates.length === 0 && (
                <p className="text-sm text-muted-foreground">
                  Upload or generate at least one image to select a thumbnail.
                </p>
              )}
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
                <p>
                  <span className="text-foreground font-medium">Thumbnail:</span>{" "}
                  {selectedThumbnailFile ? selectedThumbnailFile.name : "None selected"}
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
              {reviewThumbnailPreviewUrl && (
                <div className="mt-4 rounded-md border border-white/20 bg-white/5 p-3">
                  <p className="text-xs text-muted-foreground mb-2">Selected thumbnail preview</p>
                  <img
                    src={reviewThumbnailPreviewUrl}
                    alt="Selected thumbnail preview"
                    className="h-28 w-28 rounded object-cover"
                  />
                </div>
              )}
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

      {showThumbnailGenerator && previewFile && canGenerateThumbnail && (
        <ThumbnailGenerator
          modelFile={previewFile.file}
          open={showThumbnailGenerator}
          onOpenChange={setShowThumbnailGenerator}
          onThumbnailGenerated={handleThumbnailGenerated}
        />
      )}
    </div>
  );
}
