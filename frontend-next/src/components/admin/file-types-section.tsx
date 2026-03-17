"use client";

import { useState } from "react";
import { Pencil } from "lucide-react";
import { SettingsSection } from "@/components/settings/settings-section";
import { Button } from "@/components/primitives/button";
import { Input } from "@/components/primitives/input";
import { Switch } from "@/components/primitives/switch";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { SettingsField } from "@/components/settings/settings-field";
import {
  getFileSettings,
  updateFileSettings,
} from "@/lib/services/adminService";
import { useAdminQuery } from "@/lib/hooks/use-admin-query";
import { useAdminMutation } from "@/lib/hooks/use-admin-mutation";
import type { FileTypeSettingsData } from "@/lib/api/client";
import { UpdateFileSettingsCommand } from "@/lib/api/client";

function formatBytes(bytes?: number | null): string {
  if (bytes == null || bytes === 0) return "—";
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

export function FileTypesSection() {
  const {
    data: fileSettings,
    isLoading,
    error,
    refetch,
  } = useAdminQuery(getFileSettings);

  const updateMutation = useAdminMutation(
    (cmd: UpdateFileSettingsCommand) => updateFileSettings(cmd),
    { onSuccess: refetch, successMessage: "File type updated" }
  );

  const [editTarget, setEditTarget] = useState<FileTypeSettingsData | null>(null);
  const [editForm, setEditForm] = useState<Partial<FileTypeSettingsData>>({});

  const fileTypes = fileSettings?.fileTypes ?? [];

  const openEdit = (ft: FileTypeSettingsData) => {
    setEditTarget(ft);
    setEditForm({
      id: ft.id,
      fileExtension: ft.fileExtension,
      displayName: ft.displayName,
      enabled: ft.enabled,
      maxFileSizeBytes: ft.maxFileSizeBytes,
      maxPerUpload: ft.maxPerUpload,
      mimeType: ft.mimeType,
      description: ft.description,
      requiresPreview: ft.requiresPreview,
      isCompressible: ft.isCompressible,
      category: ft.category,
      priority: ft.priority,
      isDefault: ft.isDefault,
    });
  };

  const handleSave = async () => {
    if (!editForm.id || !editForm.fileExtension) return;
    const cmd = new UpdateFileSettingsCommand({
      id: editForm.id,
      fileExtension: editForm.fileExtension,
      displayName: editForm.displayName,
      enabled: editForm.enabled,
      maxFileSizeBytes: editForm.maxFileSizeBytes,
      maxPerUpload: editForm.maxPerUpload,
      mimeType: editForm.mimeType,
      description: editForm.description,
      requiresPreview: editForm.requiresPreview,
      isCompressible: editForm.isCompressible,
      category: editForm.category,
      priority: editForm.priority,
      isDefault: editForm.isDefault,
    });
    await updateMutation.mutate(cmd);
    setEditTarget(null);
    setEditForm({});
  };

  const handleToggleEnabled = async (ft: FileTypeSettingsData) => {
    if (!ft.id || !ft.fileExtension) return;
    const cmd = new UpdateFileSettingsCommand({
      id: ft.id,
      fileExtension: ft.fileExtension,
      enabled: !ft.enabled,
      displayName: ft.displayName,
      maxFileSizeBytes: ft.maxFileSizeBytes,
      maxPerUpload: ft.maxPerUpload,
      mimeType: ft.mimeType,
    });
    await updateMutation.mutate(cmd);
  };

  return (
    <SettingsSection
      title="File Types"
      description="Configure allowed file types for model uploads"
    >
      {error && (
        <div className="rounded-lg border border-red-500/50 bg-red-500/10 px-4 py-3 text-red-400 mb-4">
          {error}
        </div>
      )}

      {isLoading ? (
        <div className="text-center text-white/60 py-8">Loading file types...</div>
      ) : fileTypes.length === 0 ? (
        <div className="text-center text-white/60 py-8 rounded-lg border border-white/10 glass-bg">
          No file types configured.
        </div>
      ) : (
        <div className="rounded-lg border border-white/10 overflow-hidden">
          <table className="w-full">
            <thead>
              <tr className="border-b border-white/10 bg-white/5">
                <th className="text-left py-3 px-4 text-sm font-medium text-white/80">
                  Extension
                </th>
                <th className="text-left py-3 px-4 text-sm font-medium text-white/80">
                  Display Name
                </th>
                <th className="text-left py-3 px-4 text-sm font-medium text-white/80">
                  Enabled
                </th>
                <th className="text-left py-3 px-4 text-sm font-medium text-white/80">
                  Max Size
                </th>
                <th className="text-left py-3 px-4 text-sm font-medium text-white/80">
                  MIME Type
                </th>
                <th className="text-right py-3 px-4 text-sm font-medium text-white/80">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody>
              {fileTypes.map((ft) => (
                <tr
                  key={ft.id}
                  className="border-b border-white/5 hover:bg-white/5"
                >
                  <td className="py-3 px-4 text-white font-mono">
                    {ft.fileExtension}
                  </td>
                  <td className="py-3 px-4 text-white">
                    {ft.displayName ?? ft.fileExtension}
                  </td>
                  <td className="py-3 px-4">
                    <Switch
                      checked={ft.enabled ?? true}
                      onCheckedChange={() => handleToggleEnabled(ft)}
                    />
                  </td>
                  <td className="py-3 px-4 text-white/60 text-sm">
                    {formatBytes(ft.maxFileSizeBytes)}
                  </td>
                  <td className="py-3 px-4 text-white/60 text-sm">
                    {ft.mimeType ?? "—"}
                  </td>
                  <td className="py-3 px-4 text-right">
                    <Button
                      variant="ghost"
                      size="icon"
                      onClick={() => openEdit(ft)}
                      className="text-white/80 hover:text-white"
                    >
                      <Pencil className="h-4 w-4" />
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <Dialog
        open={!!editTarget}
        onOpenChange={(open) => !open && setEditTarget(null)}
      >
        <DialogContent variant="glass" className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>Edit File Type</DialogTitle>
            <DialogDescription>
              Update file type configuration.
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <SettingsField label="Display Name">
              <Input
                variant="glass"
                value={editForm.displayName ?? ""}
                onChange={(e) =>
                  setEditForm((p) => ({ ...p, displayName: e.target.value }))
                }
                placeholder="e.g. 3D Model"
                className="text-white"
              />
            </SettingsField>
            <SettingsField label="Enabled">
              <Switch
                checked={editForm.enabled ?? true}
                onCheckedChange={(v: boolean) =>
                  setEditForm((p) => ({ ...p, enabled: v }))
                }
              />
            </SettingsField>
            <SettingsField label="Max File Size (bytes)">
              <Input
                variant="glass"
                type="number"
                value={editForm.maxFileSizeBytes ?? ""}
                onChange={(e) =>
                  setEditForm((p) => ({
                    ...p,
                    maxFileSizeBytes: parseInt(e.target.value, 10) || undefined,
                  }))
                }
                placeholder="e.g. 52428800"
                className="text-white"
              />
            </SettingsField>
            <SettingsField label="Max Per Upload">
              <Input
                variant="glass"
                type="number"
                value={editForm.maxPerUpload ?? ""}
                onChange={(e) =>
                  setEditForm((p) => ({
                    ...p,
                    maxPerUpload: parseInt(e.target.value, 10) || undefined,
                  }))
                }
                placeholder="e.g. 10"
                className="text-white"
              />
            </SettingsField>
            <SettingsField label="MIME Type">
              <Input
                variant="glass"
                value={editForm.mimeType ?? ""}
                onChange={(e) =>
                  setEditForm((p) => ({ ...p, mimeType: e.target.value }))
                }
                placeholder="e.g. model/stl"
                className="text-white"
              />
            </SettingsField>
            <SettingsField label="Description">
              <Input
                variant="glass"
                value={editForm.description ?? ""}
                onChange={(e) =>
                  setEditForm((p) => ({ ...p, description: e.target.value }))
                }
                placeholder="Optional description"
                className="text-white"
              />
            </SettingsField>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setEditTarget(null)}>
              Cancel
            </Button>
            <Button
              variant="glass"
              onClick={handleSave}
              disabled={updateMutation.isLoading}
            >
              {updateMutation.isLoading ? "Saving..." : "Save"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </SettingsSection>
  );
}
