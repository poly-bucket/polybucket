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
import { ModerationActionsSection } from "./moderation-actions-section";
import { toast } from "sonner";

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

  const fetchData = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const [configRes, siteRes] = await Promise.all([
        getModelConfigurationSettings(),
        getSiteModelSettings(),
      ]);
      setModelConfig(configRes.settings ?? null);
      setSiteSettings(siteRes.settings ?? null);
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
    return (
      <div className="text-center text-white/60 py-12">Loading settings...</div>
    );
  }

  return (
    <div className="space-y-6">
      {error && (
        <div className="rounded-lg border border-red-500/50 bg-red-500/10 px-4 py-3 text-red-400">
          {error}
        </div>
      )}

      <SettingsSection title="Model Configuration" description="Configure model upload and behavior">
        {modelConfig && (
          <>
            <SettingsToggle
              label="Require upload moderation"
              description="Models must be approved before publication"
              checked={modelConfig.requireUploadModeration ?? false}
              onCheckedChange={(v) =>
                handleConfigChange({ requireUploadModeration: v })
              }
            />
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
            <SettingsToggle
              label="Require moderation"
              checked={siteSettings.requireModeration ?? false}
              onCheckedChange={(v) =>
                handleSiteChange({ requireModeration: v })
              }
            />
            <SettingsToggle
              label="Require login for upload"
              checked={siteSettings.requireLoginForUpload ?? false}
              onCheckedChange={(v) =>
                handleSiteChange({ requireLoginForUpload: v })
              }
            />
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
