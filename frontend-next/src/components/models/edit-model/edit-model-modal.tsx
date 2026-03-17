"use client";

import { useState, useCallback } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
} from "@/components/ui/dialog";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/primitives/tabs";
import type { Model } from "@/lib/api/client";
import { ModelEditForm } from "./model-edit-form";
import { ModelVersionManager } from "./model-version-manager";
import { VersionEditor, type ExtendedModelVersion } from "./version-editor";
import { Pencil, CloudUpload, FileEdit } from "lucide-react";

interface EditModelModalProps {
  model: Model;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onModelUpdate?: (model: Model) => void;
  onVersionCreated?: () => void;
  onVersionUpdate?: (versionId: string, updated: ExtendedModelVersion) => void;
}

export function EditModelModal({
  model,
  open,
  onOpenChange,
  onModelUpdate,
  onVersionCreated,
  onVersionUpdate,
}: EditModelModalProps) {
  const [activeTab, setActiveTab] = useState("edit");
  const [isDirty, setIsDirty] = useState(false);

  const handleOpenChange = useCallback(
    (nextOpen: boolean) => {
      if (!nextOpen && isDirty) {
        const confirmed = window.confirm(
          "You have unsaved changes. Are you sure you want to close?"
        );
        if (!confirmed) return;
      }
      setIsDirty(false);
      onOpenChange(nextOpen);
    },
    [isDirty, onOpenChange]
  );

  const handleModelUpdate = useCallback(
    (updated: Model) => {
      onModelUpdate?.(updated);
      onOpenChange(false);
    },
    [onModelUpdate, onOpenChange]
  );

  const handleCancel = useCallback(() => {
    if (isDirty) {
      const confirmed = window.confirm(
        "You have unsaved changes. Are you sure you want to close?"
      );
      if (!confirmed) return;
    }
    setIsDirty(false);
    onOpenChange(false);
  }, [isDirty, onOpenChange]);

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent
        variant="glass"
        className="max-w-4xl max-h-[85vh] flex flex-col sm:max-w-4xl"
      >
        <DialogHeader>
          <DialogTitle id="edit-model-dialog-title">
            Manage Model
          </DialogTitle>
          <DialogDescription id="edit-model-dialog-description">
            {model.name ?? "Untitled Model"}
          </DialogDescription>
        </DialogHeader>

        <Tabs
          value={activeTab}
          onValueChange={setActiveTab}
          className="flex flex-col flex-1 min-h-0"
        >
          <TabsList
            variant="glass"
            className="w-full flex flex-wrap"
            aria-label="Edit model sections"
          >
            <TabsTrigger variant="glass" value="edit">
              <Pencil className="h-4 w-4 mr-2" />
              Edit Details
            </TabsTrigger>
            <TabsTrigger variant="glass" value="version">
              <CloudUpload className="h-4 w-4 mr-2" />
              New Version
            </TabsTrigger>
            <TabsTrigger variant="glass" value="editVersion">
              <FileEdit className="h-4 w-4 mr-2" />
              Edit Versions
            </TabsTrigger>
          </TabsList>

          <div className="flex-1 overflow-y-auto mt-4 min-h-0">
            {activeTab === "edit" && (
              <TabsContent value="edit" variant="glass" className="mt-0">
                <ModelEditForm
                  model={model}
                  onSave={handleModelUpdate}
                  onCancel={handleCancel}
                  onDirtyChange={setIsDirty}
                />
              </TabsContent>
            )}

            {activeTab === "version" && (
              <TabsContent value="version" variant="glass" className="mt-0">
                <ModelVersionManager
                  model={model}
                  onCreateVersion={onVersionCreated ?? (() => onOpenChange(false))}
                  onCancel={handleCancel}
                />
              </TabsContent>
            )}

            {activeTab === "editVersion" && (
              <TabsContent value="editVersion" variant="glass" className="mt-0">
                <VersionEditor
                  model={model}
                  onVersionUpdate={onVersionUpdate ?? (() => {})}
                  onCancel={handleCancel}
                />
              </TabsContent>
            )}
          </div>
        </Tabs>
      </DialogContent>
    </Dialog>
  );
}
