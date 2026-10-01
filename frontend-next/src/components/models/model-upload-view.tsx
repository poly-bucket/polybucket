"use client";

import React, { useState, useEffect, useCallback, useMemo } from "react";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";
import { useAuth } from "@/contexts/AuthContext";
import { linkModelCategories, uploadModel } from "@/lib/services/modelsService";
import {
  getFileSettings,
  getExtensionsByCategory,
  isFileTypeAllowed,
} from "@/lib/services/fileTypeSettingsService";
import { getModelConfigurationSettings } from "@/lib/services/modelConfigurationSettingsService";
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
import ThumbnailGenerator from "./thumbnail-generator";
import { getUploadDefaults } from "@/lib/services/contentDefaultsService";
import {
  MAX_FILES_PER_UPLOAD,
  createUploadedFile,
  getUploadFileType,
  setThumbnailSelection,
} from "./upload-shared";
import type { UploadedFile } from "./file-queue";
import { UploadWizardShell } from "./upload/upload-wizard-shell";
import { UploadWizardSkeleton } from "@/components/ui/skeletons";
import { UploadFilesStep } from "./upload/upload-files-step";
import { UploadDetailsStep } from "./upload/upload-details-step";
import { UploadReviewStep } from "./upload/upload-review-step";
import { useUploadWizard } from "./upload/use-upload-wizard";
import {
  modelMetadataSchema,
  parseLicenseDefault,
  parsePrivacyDefault,
  type ModelMetadataFormValues,
} from "@/lib/models/model-metadata";
import { useUnsavedChanges } from "@/hooks/use-unsaved-changes";

function buildInitialMetadata(): ModelMetadataFormValues {
  const localUploadDefaults = getUploadDefaults();
  return {
    name: "",
    description: "",
    privacy: parsePrivacyDefault(localUploadDefaults.privacy),
    license: parseLicenseDefault(localUploadDefaults.license),
    aiGenerated: localUploadDefaults.aiGenerated,
    wip: localUploadDefaults.workInProgress,
    nsfw: localUploadDefaults.nsfw,
    isRemix: localUploadDefaults.remix,
    remixUrl: "",
  };
}

export default function ModelUploadView() {
  const router = useRouter();
  const { isAuthenticated, isLoading: authLoading } = useAuth();
  const [fileTypeSettings, setFileTypeSettings] = useState<
    FileTypeSettingsData[] | undefined
  >(undefined);
  const [settingsLoading, setSettingsLoading] = useState(true);
  const [uploadedFiles, setUploadedFiles] = useState<UploadedFile[]>([]);
  const [previewFile, setPreviewFile] = useState<UploadedFile | null>(null);
  const [selectedThumbnailFileId, setSelectedThumbnailFileId] = useState<string | null>(null);
  const [categories, setCategories] = useState<string[]>([]);
  const [metadataValid, setMetadataValid] = useState(false);
  const [metadataDirty, setMetadataDirty] = useState(false);
  const [isUploading, setIsUploading] = useState(false);
  const [isExtractingZip, setIsExtractingZip] = useState(false);
  const [showThumbnailGenerator, setShowThumbnailGenerator] = useState(false);

  const metadataForm = useForm<ModelMetadataFormValues>({
    resolver: zodResolver(modelMetadataSchema),
    defaultValues: buildInitialMetadata(),
    mode: "onChange",
  });

  const metadataValues = metadataForm.watch();

  const default3D = [".stl", ".obj", ".fbx", ".glb", ".gltf", ".3mf", ".step", ".stp"];
  const defaultImage = [".png", ".jpg", ".jpeg", ".gif", ".webp"];
  const defaultArchive = [".zip"];
  const defaultDoc = [".pdf", ".md", ".markdown"];

  const raw3D = getExtensionsByCategory(fileTypeSettings, "3D").map((e) =>
    e.startsWith(".") ? e : `.${e}`
  );
  const rawImage = getExtensionsByCategory(fileTypeSettings, "Image").map((e) =>
    e.startsWith(".") ? e : `.${e}`
  );
  const rawZip = getExtensionsByCategory(fileTypeSettings, "Archive").map((e) =>
    e.startsWith(".") ? e : `.${e}`
  );
  const rawDoc = getExtensionsByCategory(fileTypeSettings, "Document").map((e) =>
    e.startsWith(".") ? e : `.${e}`
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

  const formatGroups = useMemo(
    () => [
      { label: "3D models", extensions: supported3DFormats },
      { label: "Images", extensions: supportedImageFormats },
      { label: "Documents", extensions: supportedDocFormats },
      { label: "Archives", extensions: supportedZipFormats },
    ],
    [supported3DFormats, supportedImageFormats, supportedDocFormats, supportedZipFormats]
  );

  const canAddMoreFiles = uploadedFiles.length < MAX_FILES_PER_UPLOAD;

  const getFileType = useCallback(
    (fileName: string) => {
      return getUploadFileType(fileName, supported3DFormats, supportedImageFormats);
    },
    [supported3DFormats, supportedImageFormats]
  );

  const handleAutoTitle = useCallback(
    (title: string) => {
      metadataForm.setValue("name", title, { shouldValidate: true, shouldDirty: true });
    },
    [metadataForm]
  );

  const { applyAutoTitleIfNeeded, ...wizard } = useUploadWizard({
    fileCount: uploadedFiles.length,
    settingsLoaded: !settingsLoading,
    metadataValid,
    isFormDirty: metadataDirty,
    uploadedFileNames: uploadedFiles.map((f) => f.name),
    getFileType,
    currentTitle: metadataValues.name,
    onAutoTitle: handleAutoTitle,
  });

  useUnsavedChanges(wizard.isDirty);

  useEffect(() => {
    if (!isAuthenticated && !authLoading) {
      router.replace("/");
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
          const p = String(configResp.settings.defaultPrivacySetting).toLowerCase();
          if (p === "public")
            metadataForm.setValue("privacy", PrivacySettings.Public);
          else if (p === "private")
            metadataForm.setValue("privacy", PrivacySettings.Private);
          else if (p === "unlisted")
            metadataForm.setValue("privacy", PrivacySettings.Unlisted);
        }
      } catch (err) {
        console.error(err);
        toast.error("Failed to load settings");
      } finally {
        setSettingsLoading(false);
      }
    };
    load();
  }, [metadataForm]);

  const applyMarkdownToForm = useCallback(
    (parsed: ReturnType<typeof parseModelMarkdown>) => {
      if (parsed.title) metadataForm.setValue("name", parsed.title, { shouldValidate: true });
      if (parsed.description)
        metadataForm.setValue("description", parsed.description, { shouldDirty: true });
      if (parsed.privacy) metadataForm.setValue("privacy", parsed.privacy);
      if (parsed.license)
        metadataForm.setValue("license", parseLicenseDefault(parsed.license));
      if (parsed.categories) setCategories(parsed.categories);
      if (parsed.aiGenerated !== undefined)
        metadataForm.setValue("aiGenerated", parsed.aiGenerated);
      if (parsed.workInProgress !== undefined)
        metadataForm.setValue("wip", parsed.workInProgress);
      if (parsed.nsfw !== undefined) metadataForm.setValue("nsfw", parsed.nsfw);
      if (parsed.remix !== undefined) metadataForm.setValue("isRemix", parsed.remix);
    },
    [metadataForm]
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
      const previousCount = uploadedFiles.length;
      const remaining = MAX_FILES_PER_UPLOAD - uploadedFiles.length;
      const toAdd = files.slice(0, remaining);
      const takenNames = new Set(uploadedFiles.map((uploadedFile) => uploadedFile.name));
      const newUploaded = toAdd.map((fileItem) => {
        const nextFile = createUploadedFile(fileItem, takenNames);
        takenNames.add(nextFile.name);
        return nextFile;
      });
      setUploadedFiles((prev) => {
        const next = [...prev, ...newUploaded];
        applyAutoTitleIfNeeded(previousCount, next.map((f) => f.name));
        return next;
      });
      const firstPreview = newUploaded.find((fileItem) => getFileType(fileItem.name) !== "unknown");
      if (firstPreview && !previewFile) setPreviewFile(firstPreview);
      toast.success(
        `Zip extracted: ${toAdd.length} file(s) added. ${files.length - toAdd.length} skipped (max reached).`
      );
    },
    [canAddMoreFiles, uploadedFiles, previewFile, getFileType, applyAutoTitleIfNeeded]
  );

  const processMarkdownFile = useCallback(
    async (file: File) => {
      try {
        const text = await file.text();
        const parsed = parseModelMarkdown(text);
        applyMarkdownToForm(parsed);
        toast.success(`Model details populated from ${file.name}`);
      } catch {
        toast.error("Failed to parse markdown file");
      }
    },
    [applyMarkdownToForm]
  );

  const handleFilesSelected = useCallback(
    async (files: File[]) => {
      const toAdd: File[] = [];
      const toProcess: File[] = [];

      for (const file of files) {
        if (!isFileTypeAllowed(fileTypeSettings, file)) {
          toast.error(`File ${file.name} is not allowed or exceeds size limit`);
          continue;
        }
        const ext = file.name.toLowerCase().substring(file.name.lastIndexOf("."));
        if (supportedZipFormats.includes(ext)) {
          toProcess.push(file);
          continue;
        }
        if (isMarkdownFile(file.name)) {
          toProcess.push(file);
          continue;
        }
        if (uploadedFiles.length + toAdd.length >= MAX_FILES_PER_UPLOAD) {
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
        const ext = file.name.toLowerCase().substring(file.name.lastIndexOf("."));
        if (supportedZipFormats.includes(ext)) await processZipFile(file);
        else if (isMarkdownFile(file.name)) await processMarkdownFile(file);
      }

      if (toAdd.length === 0) return;

      const previousCount = uploadedFiles.length;
      const takenNames = new Set(uploadedFiles.map((uploadedFile) => uploadedFile.name));
      const newFiles = toAdd.map((fileItem) => {
        const nextFile = createUploadedFile(fileItem, takenNames);
        takenNames.add(nextFile.name);
        return nextFile;
      });
      setUploadedFiles((prev) => {
        const next = [...prev, ...newFiles];
        applyAutoTitleIfNeeded(previousCount, next.map((f) => f.name));
        return next;
      });
      const firstPreviewable = newFiles.find((f) => getFileType(f.name) !== "unknown");
      if (firstPreviewable && !previewFile) setPreviewFile(firstPreviewable);
    },
    [
      fileTypeSettings,
      supportedZipFormats,
      uploadedFiles,
      previewFile,
      processZipFile,
      processMarkdownFile,
      getFileType,
      applyAutoTitleIfNeeded,
    ]
  );

  useEffect(() => {
    setMetadataValid(modelMetadataSchema.safeParse(metadataValues).success);
  }, [metadataValues]);

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
      if (previewFile?.id === id) setPreviewFile(next.length > 0 ? next[0] : null);
      return next;
    });
  };

  const handleClearAll = () => {
    setUploadedFiles([]);
    setPreviewFile(null);
    setSelectedThumbnailFileId(null);
  };

  const handleSelectThumbnail = (id: string) => {
    setSelectedThumbnailFileId(id);
    setUploadedFiles((prev) => setThumbnailSelection(prev, id));
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

  const imageFiles = uploadedFiles.filter((fileItem) => getFileType(fileItem.name) === "image");

  const selectedThumbnailFile = uploadedFiles.find(
    (fileItem) => fileItem.id === selectedThumbnailFileId
  );

  useEffect(() => {
    if (
      selectedThumbnailFileId &&
      !uploadedFiles.some((fileItem) => fileItem.id === selectedThumbnailFileId)
    ) {
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
    return URL.createObjectURL(selectedThumbnailFile.file);
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
    if (uploadedFiles.length === 0 || !metadataValid) return;
    const data = metadataForm.getValues();
    setIsUploading(true);
    try {
      const result = await uploadModel({
        modelData: {
          name: data.name.trim(),
          description: data.description || undefined,
          privacy: data.privacy,
          license: data.license,
          categories,
          aiGenerated: data.aiGenerated,
          workInProgress: data.wip,
          nsfw: data.nsfw,
          remix: data.isRemix,
          remixUrl: data.isRemix ? data.remixUrl : undefined,
          thumbnailFileId: getThumbnailFileIdForUpload(),
        },
        files: uploadedFiles.map((f) => f.file),
      });

      if (result.id) {
        if (categories.length > 0) {
          const { failed, skipped } = await linkModelCategories(result.id, categories);
          if (skipped > 0) {
            toast.warning(`${skipped} category name(s) were not found on the server`);
          }
          if (failed > 0) {
            toast.error(`Some categories could not be linked (${failed})`);
          }
        }
      }

      toast.success("Model uploaded successfully");
      router.push(result.id ? `/models/${result.id}` : "/");
    } catch (err) {
      toast.error(err instanceof Error ? err.message : "Upload failed");
    } finally {
      setIsUploading(false);
    }
  };

  const downloadMarkdownTemplate = () => {
    const blob = new Blob([generateMarkdownTemplate()], { type: "text/markdown" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = "model-template.md";
    a.click();
    URL.revokeObjectURL(url);
  };

  const handleCancel = () => {
    if (wizard.isDirty) {
      const confirmed = window.confirm(
        "You have unsaved changes. Are you sure you want to leave?"
      );
      if (!confirmed) return;
    }
    router.push("/");
  };

  if (authLoading || !isAuthenticated) {
    return <UploadWizardSkeleton />;
  }

  return (
    <>
      <UploadWizardShell
        currentStep={wizard.currentStep}
        stepCompletion={wizard.stepCompletion}
        isStepReachable={wizard.isStepReachable}
        onStepClick={wizard.goToStep}
        canAdvance={wizard.canAdvanceFromCurrent}
        isUploading={isUploading}
        onBack={wizard.goBack}
        onNext={wizard.goNext}
        onUpload={handleUpload}
        onCancel={handleCancel}
      >
        {wizard.currentStep === "files" && (
          <UploadFilesStep
            settingsLoading={settingsLoading}
            isExtractingZip={isExtractingZip}
            uploadedFiles={uploadedFiles}
            selectedFileId={previewFile?.id ?? null}
            acceptFormats={allSupportedFormats}
            formatGroups={formatGroups}
            canAddMore={canAddMoreFiles}
            maxFiles={MAX_FILES_PER_UPLOAD}
            getFileType={getFileType}
            onFilesSelected={handleFilesSelected}
            onSelectFile={handleSelectFile}
            onRemoveFile={handleRemoveFile}
            onClearAll={handleClearAll}
            onDownloadMarkdownTemplate={downloadMarkdownTemplate}
          />
        )}

        {wizard.currentStep === "details" && (
          <UploadDetailsStep
            uploadedFiles={uploadedFiles}
            previewFileId={previewFile?.id ?? null}
            getFileType={getFileType}
            onSelectPreviewFile={handleSelectFile}
            imageFiles={imageFiles}
            selectedThumbnailFileId={selectedThumbnailFileId}
            onSelectThumbnail={handleSelectThumbnail}
            canGenerateFrom3d={canGenerateThumbnail}
            onOpenThumbnailGenerator={() => setShowThumbnailGenerator(true)}
            metadataForm={metadataForm}
            categories={categories}
            onCategoriesChange={setCategories}
            onMetadataValidityChange={setMetadataValid}
            onMetadataDirtyChange={setMetadataDirty}
          />
        )}

        {wizard.currentStep === "review" && (
          <UploadReviewStep
            metadata={metadataValues}
            categories={categories}
            files={uploadedFiles}
            getFileType={getFileType}
            thumbnailPreviewUrl={reviewThumbnailPreviewUrl}
            hasThumbnail={selectedThumbnailFileId !== null}
          />
        )}
      </UploadWizardShell>

      {showThumbnailGenerator && previewFile && canGenerateThumbnail && (
        <ThumbnailGenerator
          modelFile={previewFile.file}
          open={showThumbnailGenerator}
          onOpenChange={setShowThumbnailGenerator}
          onThumbnailGenerated={handleThumbnailGenerated}
        />
      )}
    </>
  );
}
