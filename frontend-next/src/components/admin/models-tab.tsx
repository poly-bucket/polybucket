"use client";

import { useState, useEffect, useCallback } from "react";
import { Trash2 } from "lucide-react";
import { SettingsSection } from "@/components/settings/settings-section";
import { SettingsToggle } from "@/components/settings/settings-toggle";
import { SettingsField } from "@/components/settings/settings-field";
import { SettingsFooter } from "@/components/settings/settings-footer";
import { Button } from "@/components/primitives/button";
import { Input } from "@/components/primitives/input";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/primitives/tabs";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  getModelConfigurationSettings,
  updateModelConfigurationSettings,
  getSiteModelSettings,
  updateSiteModelSettings,
  deleteAllModels,
} from "@/lib/services/adminService";
import type {
  ModelConfigurationSettingsData,
  SiteModelSettingsData,
} from "@/lib/api/client";
import {
  UpdateModelConfigurationSettingsCommand,
  UpdateSiteModelSettingsCommand,
  DeleteAllModelsRequest,
} from "@/lib/api/client";
import { CategoriesSection } from "./categories-section";
import { FileTypesSection } from "./file-types-section";
import { SettingsSectionsSkeleton } from "@/components/ui/skeletons";
import { ModerationActionsSection } from "./moderation-actions-section";
import { ModelConfigurationAdvancedSection } from "./model-configuration-advanced-section";
import { getSiteSettings } from "@/lib/services/adminService";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { toast } from "sonner";
import Link from "next/link";

const BULK_DELETE_CONFIRM_TEXT = "DELETE ALL";

function ConfigurationTab() {
  const [modelConfig, setModelConfig] =
    useState<ModelConfigurationSettingsData | null>(null);
  const [siteSettings, setSiteSettings] =
    useState<SiteModelSettingsData | null>(null);
  const [loading, setLoading] = useState(true);
  const [savingConfig, setSavingConfig] = useState(false);
  const [savingSite, setSavingSite] = useState(false);
  const [configDirty, setConfigDirty] = useState(false);
  const [siteDirty, setSiteDirty] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showAdvancedHint, setShowAdvancedHint] = useState(false);

  const fetchData = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const [configRes, siteRes, site] = await Promise.all([
        getModelConfigurationSettings(),
        getSiteModelSettings(),
        getSiteSettings().catch(() => null),
      ]);
      setModelConfig(configRes.settings ?? null);
      setSiteSettings(siteRes.settings ?? null);
      setShowAdvancedHint(site?.showAdvancedOptions ?? false);
    } catch {
      setError("Failed to load settings");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const handleConfigChange = (updates: Partial<ModelConfigurationSettingsData>) => {
    setModelConfig((prev) =>
      prev ? ({ ...prev, ...updates } as ModelConfigurationSettingsData) : null
    );
    setConfigDirty(true);
  };

  const handleSiteChange = (updates: Partial<SiteModelSettingsData>) => {
    setSiteSettings((prev) =>
      prev ? ({ ...prev, ...updates } as SiteModelSettingsData) : null
    );
    setSiteDirty(true);
  };

  const saveModelConfig = async () => {
    if (!modelConfig) return;
    setSavingConfig(true);
    try {
      const cmd = new UpdateModelConfigurationSettingsCommand({
        ...modelConfig,
        defaultPrivacySetting: modelConfig.defaultPrivacySetting ?? "Public",
      });
      await updateModelConfigurationSettings(cmd);
      setConfigDirty(false);
      toast.success("Model configuration saved");
    } catch {
      setError("Failed to save model configuration");
      toast.error("Failed to save model configuration");
    } finally {
      setSavingConfig(false);
    }
  };

  const saveSiteSettings = async () => {
    if (!siteSettings) return;
    setSavingSite(true);
    try {
      const cmd = new UpdateSiteModelSettingsCommand({
        ...siteSettings,
        allowedFileTypes: siteSettings.allowedFileTypes ?? "",
        defaultModelPrivacy: siteSettings.defaultModelPrivacy ?? "Public",
      });
      await updateSiteModelSettings(cmd);
      setSiteDirty(false);
      toast.success("Site model settings saved");
    } catch {
      setError("Failed to save site model settings");
      toast.error("Failed to save site model settings");
    } finally {
      setSavingSite(false);
    }
  };

  if (loading) {
    return <SettingsSectionsSkeleton sections={2} togglesPerSection={5} />;
  }

  return (
    <div className="space-y-6">
      {error && (
        <div className="rounded-lg border border-red-500/50 bg-red-500/10 px-4 py-3 text-red-400">
          {error}
        </div>
      )}

      <SettingsSection
        title="Model Configuration"
        description="Upload behavior and content rules (moderation workflow is under Moderation → Settings)"
      >
        {modelConfig && (
          <>
            <SettingsToggle
              label="Allow anonymous uploads"
              description="Unauthenticated users can upload models"
              checked={modelConfig.allowAnonUploads ?? false}
              onCheckedChange={(v) =>
                handleConfigChange({ allowAnonUploads: v })
              }
            />
            <SettingsToggle
              label="Allow anonymous downloads"
              description="Unauthenticated users can download public models"
              checked={modelConfig.allowAnonDownloads ?? false}
              onCheckedChange={(v) =>
                handleConfigChange({ allowAnonDownloads: v })
              }
            />
            <SettingsToggle
              label="Enable model versioning"
              description="Allow multiple versions per model"
              checked={modelConfig.enableModelVersioning ?? false}
              onCheckedChange={(v) =>
                handleConfigChange({ enableModelVersioning: v })
              }
            />
            <SettingsField
              label="Default privacy (model records)"
              description="Default visibility on the model entity"
            >
              <Select
                value={modelConfig.defaultPrivacySetting ?? "Public"}
                onValueChange={(v) =>
                  handleConfigChange({ defaultPrivacySetting: v })
                }
              >
                <SelectTrigger variant="glass">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent variant="glass">
                  <SelectItem value="Public">Public</SelectItem>
                  <SelectItem value="Private">Private</SelectItem>
                  <SelectItem value="Unlisted">Unlisted</SelectItem>
                </SelectContent>
              </Select>
            </SettingsField>
            <SettingsField label="Site-wide model limit">
              <Input
                variant="glass"
                type="number"
                value={modelConfig.limitTotalModels ?? ""}
                onChange={(e) =>
                  handleConfigChange({
                    limitTotalModels: parseInt(e.target.value, 10) || undefined,
                  })
                }
                className="text-white"
              />
            </SettingsField>
            <SettingsField label="Max models per user">
              <Input
                variant="glass"
                type="number"
                value={modelConfig.maxModelsPerUser ?? ""}
                onChange={(e) =>
                  handleConfigChange({
                    maxModelsPerUser: parseInt(e.target.value, 10) || undefined,
                  })
                }
                className="text-white"
              />
            </SettingsField>
            <SettingsField label="Min description length">
              <Input
                variant="glass"
                type="number"
                value={modelConfig.minDescriptionLength ?? ""}
                onChange={(e) =>
                  handleConfigChange({
                    minDescriptionLength: parseInt(e.target.value, 10) || undefined,
                  })
                }
                className="text-white"
              />
            </SettingsField>
            <SettingsField label="Max description length">
              <Input
                variant="glass"
                type="number"
                value={modelConfig.maxDescriptionLength ?? ""}
                onChange={(e) =>
                  handleConfigChange({
                    maxDescriptionLength: parseInt(e.target.value, 10) || undefined,
                  })
                }
                className="text-white"
              />
            </SettingsField>
            <SettingsField label="Max tags per model">
              <Input
                variant="glass"
                type="number"
                value={modelConfig.maxTagsPerModel ?? ""}
                onChange={(e) =>
                  handleConfigChange({
                    maxTagsPerModel: parseInt(e.target.value, 10) || undefined,
                  })
                }
                className="text-white"
              />
            </SettingsField>
            <SettingsField label="Max categories per model">
              <Input
                variant="glass"
                type="number"
                value={modelConfig.maxCategoriesPerModel ?? ""}
                onChange={(e) =>
                  handleConfigChange({
                    maxCategoriesPerModel: parseInt(e.target.value, 10) || undefined,
                  })
                }
                className="text-white"
              />
            </SettingsField>
            <SettingsToggle
              label="Allow NSFW content"
              checked={modelConfig.allowNSFWContent ?? false}
              onCheckedChange={(v) =>
                handleConfigChange({ allowNSFWContent: v })
              }
            />
            <SettingsToggle
              label="Allow AI-generated content"
              checked={modelConfig.allowAIGeneratedContent ?? true}
              onCheckedChange={(v) =>
                handleConfigChange({ allowAIGeneratedContent: v })
              }
            />
            <SettingsToggle
              label="Require model description"
              checked={modelConfig.requireModelDescription ?? true}
              onCheckedChange={(v) =>
                handleConfigChange({ requireModelDescription: v })
              }
            />
            <SettingsToggle
              label="Require model tags"
              checked={modelConfig.requireModelTags ?? false}
              onCheckedChange={(v) =>
                handleConfigChange({ requireModelTags: v })
              }
            />
            <SettingsToggle
              label="Require thumbnail"
              checked={modelConfig.requireThumbnail ?? false}
              onCheckedChange={(v) =>
                handleConfigChange({ requireThumbnail: v })
              }
            />
            <SettingsToggle
              label="Require category selection"
              checked={modelConfig.requireCategorySelection ?? false}
              onCheckedChange={(v) =>
                handleConfigChange({ requireCategorySelection: v })
              }
            />
            <SettingsToggle
              label="Require license selection"
              checked={modelConfig.requireLicenseSelection ?? true}
              onCheckedChange={(v) =>
                handleConfigChange({ requireLicenseSelection: v })
              }
            />
            <SettingsToggle
              label="Enable comments"
              checked={modelConfig.enableModelComments ?? true}
              onCheckedChange={(v) =>
                handleConfigChange({ enableModelComments: v })
              }
            />
            <SettingsToggle
              label="Enable likes"
              checked={modelConfig.enableModelLikes ?? true}
              onCheckedChange={(v) =>
                handleConfigChange({ enableModelLikes: v })
              }
            />
            <SettingsToggle
              label="Enable downloads"
              checked={modelConfig.enableModelDownloads ?? true}
              onCheckedChange={(v) =>
                handleConfigChange({ enableModelDownloads: v })
              }
            />
            <SettingsToggle
              label="Enable collections"
              checked={modelConfig.enableModelCollections ?? true}
              onCheckedChange={(v) =>
                handleConfigChange({ enableModelCollections: v })
              }
            />
            <ModelConfigurationAdvancedSection
              config={modelConfig}
              onChange={handleConfigChange}
              showAdvancedHint={showAdvancedHint}
            />
            <SettingsFooter
              onSave={saveModelConfig}
              isSaving={savingConfig}
              isDirty={configDirty}
            />
          </>
        )}
      </SettingsSection>

      <SettingsSection title="Site Model Settings" description="File upload and site-wide model settings">
        {siteSettings && (
          <>
            <SettingsField label="Max file size (bytes)" description="Maximum file size for uploads">
              <Input
                variant="glass"
                type="number"
                value={siteSettings.maxFileSizeBytes ?? ""}
                onChange={(e) =>
                  handleSiteChange({
                    maxFileSizeBytes: parseInt(e.target.value, 10) || undefined,
                  })
                }
                className="text-white"
              />
            </SettingsField>
            <SettingsField label="Max files per upload" description="Maximum number of files per model upload">
              <Input
                variant="glass"
                type="number"
                value={siteSettings.maxFilesPerUpload ?? ""}
                onChange={(e) =>
                  handleSiteChange({
                    maxFilesPerUpload: parseInt(e.target.value, 10) || undefined,
                  })
                }
                className="text-white"
              />
            </SettingsField>
            <SettingsField label="Allowed file types" description="Comma-separated extensions (e.g. .stl,.obj)">
              <Input
                variant="glass"
                value={siteSettings.allowedFileTypes ?? ""}
                onChange={(e) =>
                  handleSiteChange({ allowedFileTypes: e.target.value })
                }
                className="text-white"
              />
            </SettingsField>
            <SettingsToggle
              label="Enable file compression"
              checked={siteSettings.enableFileCompression ?? false}
              onCheckedChange={(v) =>
                handleSiteChange({ enableFileCompression: v })
              }
            />
            <SettingsToggle
              label="Auto-generate previews"
              checked={siteSettings.autoGeneratePreviews ?? false}
              onCheckedChange={(v) =>
                handleSiteChange({ autoGeneratePreviews: v })
              }
            />
            <SettingsField
              label="Default privacy (new uploads on site)"
              description="Site default before per-model overrides"
            >
              <Select
                value={siteSettings.defaultModelPrivacy ?? "Public"}
                onValueChange={(v) =>
                  handleSiteChange({ defaultModelPrivacy: v })
                }
              >
                <SelectTrigger variant="glass">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent variant="glass">
                  <SelectItem value="Public">Public</SelectItem>
                  <SelectItem value="Private">Private</SelectItem>
                  <SelectItem value="Unlisted">Unlisted</SelectItem>
                </SelectContent>
              </Select>
            </SettingsField>
            <SettingsToggle
              label="Allow public browsing"
              description="When off, the site requires sign-in to browse"
              checked={siteSettings.allowPublicBrowsing ?? true}
              onCheckedChange={(v) =>
                handleSiteChange({ allowPublicBrowsing: v })
              }
            />
            <SettingsToggle
              label="Require login for upload"
              checked={siteSettings.requireLoginForUpload ?? false}
              onCheckedChange={(v) =>
                handleSiteChange({ requireLoginForUpload: v })
              }
            />
            <p className="text-sm text-white/50">
              Moderation workflow toggles live in{" "}
              <Link href="/moderation/settings" className="text-primary hover:underline">
                Moderation → Settings
              </Link>
              .
            </p>
            <SettingsFooter
              onSave={saveSiteSettings}
              isSaving={savingSite}
              isDirty={siteDirty}
            />
          </>
        )}
      </SettingsSection>
    </div>
  );
}

function BulkOperationsTab() {
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [deleteStep, setDeleteStep] = useState<1 | 2>(1);
  const [deleteConfirmText, setDeleteConfirmText] = useState("");
  const [deletePassword, setDeletePassword] = useState("");
  const [deleting, setDeleting] = useState(false);

  const confirmTextMatch = deleteConfirmText.trim() === BULK_DELETE_CONFIRM_TEXT;
  const canProceedToStep2 = confirmTextMatch;

  const resetDialog = () => {
    setDeleteDialogOpen(false);
    setDeleteStep(1);
    setDeleteConfirmText("");
    setDeletePassword("");
  };

  const handleBulkDelete = async () => {
    if (!deletePassword.trim()) return;
    setDeleting(true);
    try {
      await deleteAllModels(
        new DeleteAllModelsRequest({ adminPassword: deletePassword })
      );
      resetDialog();
      toast.success("All models deleted");
    } catch {
      toast.error("Failed to delete models");
    } finally {
      setDeleting(false);
    }
  };

  return (
    <SettingsSection
      title="Bulk Operations"
      description="Dangerous operations that affect all models"
    >
      <div className="flex items-center justify-between py-4">
        <div>
          <p className="font-medium text-white">Delete all models</p>
          <p className="text-sm text-white/60">
            Permanently delete every model on the site. This cannot be undone.
          </p>
        </div>
        <Button
          variant="destructive"
          onClick={() => setDeleteDialogOpen(true)}
          className="text-white"
        >
          <Trash2 className="h-4 w-4 mr-2" />
          Delete All Models
        </Button>
      </div>

      <Dialog open={deleteDialogOpen} onOpenChange={(open) => !open && resetDialog()}>
        <DialogContent variant="glass">
          <DialogHeader>
            <DialogTitle>Delete All Models</DialogTitle>
            <DialogDescription>
              {deleteStep === 1
                ? `This will permanently delete all models. Type ${BULK_DELETE_CONFIRM_TEXT} to continue.`
                : "Enter your admin password to confirm this destructive action."}
            </DialogDescription>
          </DialogHeader>
          {deleteStep === 1 ? (
            <div>
              <label className="text-sm font-medium text-white/80">
                Confirmation text
              </label>
              <Input
                variant="glass"
                value={deleteConfirmText}
                onChange={(e) => setDeleteConfirmText(e.target.value)}
                placeholder={BULK_DELETE_CONFIRM_TEXT}
                className="mt-1 text-white"
              />
            </div>
          ) : (
            <div>
              <label className="text-sm font-medium text-white/80">
                Admin password
              </label>
              <Input
                variant="glass"
                type="password"
                value={deletePassword}
                onChange={(e) => setDeletePassword(e.target.value)}
                placeholder="Enter admin password"
                className="mt-1 text-white"
              />
            </div>
          )}
          <DialogFooter>
            <Button variant="outline" onClick={resetDialog}>
              Cancel
            </Button>
            {deleteStep === 1 ? (
              <Button
                variant="destructive"
                onClick={() => setDeleteStep(2)}
                disabled={!canProceedToStep2}
              >
                Next
              </Button>
            ) : (
              <Button
                variant="destructive"
                onClick={handleBulkDelete}
                disabled={deleting || !deletePassword.trim()}
              >
                {deleting ? "Deleting..." : "Delete All"}
              </Button>
            )}
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </SettingsSection>
  );
}

export function ModelsTab() {
  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold text-white">Model Settings</h2>

      <Tabs defaultValue="configuration" className="w-full">
        <TabsList variant="glass">
          <TabsTrigger variant="glass" value="configuration">
            Configuration
          </TabsTrigger>
          <TabsTrigger variant="glass" value="categories">
            Categories
          </TabsTrigger>
          <TabsTrigger variant="glass" value="file-types">
            File Types
          </TabsTrigger>
          <TabsTrigger variant="glass" value="moderation">
            Moderation
          </TabsTrigger>
          <TabsTrigger variant="glass" value="bulk">
            Bulk Operations
          </TabsTrigger>
        </TabsList>
        <TabsContent variant="glass" value="configuration">
          <ConfigurationTab />
        </TabsContent>
        <TabsContent variant="glass" value="categories">
          <CategoriesSection />
        </TabsContent>
        <TabsContent variant="glass" value="file-types">
          <FileTypesSection />
        </TabsContent>
        <TabsContent variant="glass" value="moderation">
          <ModerationActionsSection />
        </TabsContent>
        <TabsContent variant="glass" value="bulk">
          <BulkOperationsTab />
        </TabsContent>
      </Tabs>
    </div>
  );
}
