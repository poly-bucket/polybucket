"use client";

import { useEffect, useMemo, useState } from "react";
import { Pencil } from "lucide-react";
import { SettingsSection } from "@/components/settings/settings-section";
import { Button } from "@/components/primitives/button";
import { Input } from "@/components/primitives/input";
import { Switch } from "@/components/primitives/switch";
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetFooter,
  SheetHeader,
  SheetTitle,
} from "@/components/ui/sheet";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { SettingsField } from "@/components/settings/settings-field";
import { Badge } from "@/components/ui/badge";
import { Textarea } from "@/components/ui/glass/textarea";
import { DataTablePagination } from "@/components/primitives/pagination";
import {
  getFileSettings,
  updateFileSettings,
  bulkUpdateFileSettings,
  syncAllowedFileTypes,
} from "@/lib/services/adminService";
import { useAdminQuery } from "@/lib/hooks/use-admin-query";
import { useAdminMutation } from "@/lib/hooks/use-admin-mutation";
import type { FileTypeSettingsData } from "@/lib/api/client";
import {
  BulkUpdateFileSettingsCommand,
  UpdateFileSettingsCommand,
} from "@/lib/api/client";
import { TableRowsSkeleton } from "@/components/ui/skeletons";
import { formatDate } from "@/lib/utils/format";
import { toast } from "sonner";
import { ApiClientFactory } from "@/lib/api/clientFactory";
import {
  buildUpdateFileSettingsCommand,
  bytesToDisplayMb,
  displayMbToBytes,
  FILE_TYPE_CATEGORY_OPTIONS,
  formatBytes,
  validateFileTypeForm,
  getFileTypeDisableConfirmReason,
} from "@/lib/utils/file-type-settings-utils";
import { cn } from "@/lib/utils";
import { AdminScrollTable } from "./admin-scroll-table";

const PAGE_SIZE_OPTIONS = [10, 20, 50, 100] as const;

const colLg = "hidden lg:table-cell";
const colXl = "hidden xl:table-cell";

type EditFormState = {
  displayName: string;
  enabled: boolean;
  maxSizeMb: string;
  maxPerUpload: string;
  mimeType: string;
  description: string;
  requiresPreview: boolean;
  isCompressible: boolean;
  category: string;
  priority: string;
  isDefault: boolean;
};

function boolBadge(value?: boolean) {
  return value ? (
    <Badge variant="outline" className="border-emerald-500/40 text-emerald-300">
      Yes
    </Badge>
  ) : (
    <Badge variant="outline" className="border-white/20 text-white/50">
      No
    </Badge>
  );
}

export function FileTypesSection() {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState<number>(20);
  const [searchInput, setSearchInput] = useState("");
  const [searchTerm, setSearchTerm] = useState<string | undefined>(undefined);
  const [categoryFilter, setCategoryFilter] = useState<string>("All");
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [storageMaxBytes, setStorageMaxBytes] = useState<number | undefined>();

  const [editTarget, setEditTarget] = useState<FileTypeSettingsData | null>(null);
  const [editForm, setEditForm] = useState<EditFormState | null>(null);
  const [disableConfirm, setDisableConfirm] = useState<{
    ft: FileTypeSettingsData;
    nextEnabled: boolean;
    reason: string;
  } | null>(null);

  useEffect(() => {
    const t = setTimeout(() => {
      setSearchTerm(searchInput.trim() || undefined);
      setPage(1);
    }, 300);
    return () => clearTimeout(t);
  }, [searchInput]);

  useEffect(() => {
    ApiClientFactory.getApiClient()
      .getFileConfig_GetFileConfig()
      .then((cfg) => setStorageMaxBytes(cfg.maxTotalFileSize))
      .catch(() => undefined);
  }, []);

  const categoryQuery =
    categoryFilter === "All" ? undefined : categoryFilter;

  const {
    data: fileSettings,
    isLoading,
    error,
    refetch,
  } = useAdminQuery(
    () => getFileSettings(page, pageSize, searchTerm, categoryQuery),
    [page, pageSize, searchTerm, categoryQuery]
  );

  const updateMutation = useAdminMutation(
    (cmd: UpdateFileSettingsCommand) => updateFileSettings(cmd),
    { onSuccess: refetch, successMessage: "File type updated" }
  );

  const bulkMutation = useAdminMutation(
    (cmd: BulkUpdateFileSettingsCommand) => bulkUpdateFileSettings(cmd),
    { onSuccess: refetch, showToast: false }
  );

  const syncMutation = useAdminMutation(
    async () => syncAllowedFileTypes(),
    { onSuccess: refetch, successMessage: "Allowed file types synced" }
  );

  const fileTypes = fileSettings?.fileTypes ?? [];
  const totalCount = fileSettings?.totalCount ?? 0;
  const totalPages = fileSettings?.totalPages ?? 1;
  const tabCategories = fileSettings?.categories ?? [];

  const pageIds = useMemo(
    () => fileTypes.map((ft) => ft.id).filter(Boolean) as string[],
    [fileTypes]
  );

  const allOnPageSelected =
    pageIds.length > 0 && pageIds.every((id) => selectedIds.has(id));

  const someOnPageSelected =
    pageIds.some((id) => selectedIds.has(id)) && !allOnPageSelected;

  const validation = editForm
    ? validateFileTypeForm({
        displayName: editForm.displayName,
        maxSizeMb: editForm.maxSizeMb,
        maxPerUpload: editForm.maxPerUpload,
        mimeType: editForm.mimeType,
        description: editForm.description,
        storageMaxBytes,
      })
    : { valid: true };

  const toggleSelectAllOnPage = () => {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (allOnPageSelected) {
        pageIds.forEach((id) => next.delete(id));
      } else {
        pageIds.forEach((id) => next.add(id));
      }
      return next;
    });
  };

  const toggleRowSelection = (id: string) => {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };

  const openEdit = (ft: FileTypeSettingsData) => {
    setEditTarget(ft);
    setEditForm({
      displayName: ft.displayName ?? "",
      enabled: ft.enabled ?? true,
      maxSizeMb: bytesToDisplayMb(ft.maxFileSizeBytes),
      maxPerUpload: String(ft.maxPerUpload ?? ""),
      mimeType: ft.mimeType ?? "",
      description: ft.description ?? "",
      requiresPreview: ft.requiresPreview ?? false,
      isCompressible: ft.isCompressible ?? true,
      category: ft.category ?? "3D",
      priority: String(ft.priority ?? 0),
      isDefault: ft.isDefault ?? false,
    });
  };

  const closeEditSheet = () => {
    setEditTarget(null);
    setEditForm(null);
  };

  const applyEnabledChange = async (
    ft: FileTypeSettingsData,
    enabled: boolean
  ) => {
    const cmd = buildUpdateFileSettingsCommand(ft, { enabled });
    await updateMutation.mutate(cmd);
  };

  const needsDisableConfirm = (
    ft: FileTypeSettingsData,
    nextEnabled: boolean
  ): string | null =>
    getFileTypeDisableConfirmReason(ft, nextEnabled, fileTypes);

  const requestEnabledChange = (
    ft: FileTypeSettingsData,
    nextEnabled: boolean
  ) => {
    const reason = needsDisableConfirm(ft, nextEnabled);
    if (reason) {
      setDisableConfirm({ ft, nextEnabled, reason });
      return;
    }
    void applyEnabledChange(ft, nextEnabled);
  };

  const handleSave = async () => {
    if (!editTarget || !editForm || !validation.valid) return;
    if (editTarget.enabled && !editForm.enabled) {
      const reason = needsDisableConfirm(editTarget, false);
      if (reason) {
        setDisableConfirm({ ft: editTarget, nextEnabled: false, reason });
        return;
      }
    }
    const maxFileSizeBytes = displayMbToBytes(parseFloat(editForm.maxSizeMb));
    const cmd = buildUpdateFileSettingsCommand(editTarget, {
      displayName: editForm.displayName.trim(),
      enabled: editForm.enabled,
      maxFileSizeBytes,
      maxPerUpload: parseInt(editForm.maxPerUpload, 10),
      mimeType: editForm.mimeType.trim(),
      description: editForm.description.trim(),
      requiresPreview: editForm.requiresPreview,
      isCompressible: editForm.isCompressible,
      category: editForm.category.trim(),
      priority: parseInt(editForm.priority, 10) || 0,
      isDefault: editForm.isDefault,
    });
    await updateMutation.mutate(cmd);
    closeEditSheet();
  };

  const handleBulk = async (enabled: boolean) => {
    const ids = Array.from(selectedIds);
    if (ids.length === 0) return;
    const result = await bulkMutation.mutate(
      new BulkUpdateFileSettingsCommand({ ids, enabled })
    );
    setSelectedIds(new Set());
    if (!result) return;
    if (result.failedCount === 0) {
      toast.success(
        enabled
          ? `${result.updatedCount} file type(s) enabled`
          : `${result.updatedCount} file type(s) disabled`
      );
    } else {
      toast.warning(
        `${result.updatedCount} updated, ${result.failedCount} failed`
      );
    }
  };

  const handlePageSizeChange = (value: string) => {
    setPageSize(Number(value));
    setPage(1);
    setSelectedIds(new Set());
  };

  return (
    <SettingsSection
      className="min-w-0"
      title="File Types"
      description="Configure allowed file types for model uploads"
    >
      {error && (
        <div className="rounded-lg border border-red-500/50 bg-red-500/10 px-4 py-3 text-red-400 mb-4">
          {error}
        </div>
      )}

      {fileSettings && fileSettings.allowedFileTypesInSync === false && (
        <div className="rounded-lg border border-amber-500/40 bg-amber-500/10 px-4 py-3 mb-4 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <p className="text-amber-200 text-sm">
            Site allowed file types are out of sync with enabled file types in
            this table. Upload config uses the file types table; sync to update
            the legacy site string.
          </p>
          <Button
            variant="glass"
            size="sm"
            onClick={() => void syncMutation.mutate(undefined)}
            disabled={syncMutation.isLoading}
            className="text-white shrink-0"
          >
            {syncMutation.isLoading ? "Syncing..." : "Sync now"}
          </Button>
        </div>
      )}

      <div className="flex flex-wrap gap-2 mb-4">
        <Button
          type="button"
          variant={categoryFilter === "All" ? "glass" : "outline"}
          size="sm"
          className="text-white"
          onClick={() => {
            setCategoryFilter("All");
            setPage(1);
          }}
        >
          All
        </Button>
        {tabCategories.map((cat) => (
          <Button
            key={cat}
            type="button"
            variant={categoryFilter === cat ? "glass" : "outline"}
            size="sm"
            className="text-white"
            onClick={() => {
              setCategoryFilter(cat);
              setPage(1);
            }}
          >
            {cat}
          </Button>
        ))}
      </div>

      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between mb-4">
        <div className="flex flex-wrap items-center gap-2">
          {selectedIds.size > 0 && (
            <>
              <Button
                variant="glass"
                size="sm"
                onClick={() => handleBulk(true)}
                disabled={bulkMutation.isLoading}
                className="text-white"
              >
                Enable selected ({selectedIds.size})
              </Button>
              <Button
                variant="outline"
                size="sm"
                onClick={() => handleBulk(false)}
                disabled={bulkMutation.isLoading}
                className="text-white"
              >
                Disable selected ({selectedIds.size})
              </Button>
            </>
          )}
        </div>
        <div className="flex flex-wrap items-center gap-2">
          <Input
            variant="glass"
            placeholder="Search file types..."
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            className="w-full sm:w-64 text-white"
          />
          <Select value={String(pageSize)} onValueChange={handlePageSizeChange}>
            <SelectTrigger variant="glass" className="w-[140px] text-white">
              <SelectValue placeholder="Page size" />
            </SelectTrigger>
            <SelectContent variant="glass">
              {PAGE_SIZE_OPTIONS.map((size) => (
                <SelectItem key={size} value={String(size)}>
                  {size} per page
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      </div>

      {isLoading ? (
        <TableRowsSkeleton variant="default" rows={6} />
      ) : fileTypes.length === 0 ? (
        <div className="text-center text-white/60 py-8 rounded-lg border border-white/10 glass-bg">
          No file types match your filters.
        </div>
      ) : (
        <AdminScrollTable
          tableClassName="min-w-[28rem] lg:min-w-[42rem] xl:min-w-[64rem]"
          footer={
            totalCount > 0 ? (
              <div className="border-t border-white/10 px-4 py-3 bg-white/5">
                <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                  <div className="text-sm text-white/60">
                    Showing {(page - 1) * pageSize + 1} to{" "}
                    {Math.min(page * pageSize, totalCount)} of {totalCount}{" "}
                    file types
                  </div>
                  {totalPages > 1 && (
                    <DataTablePagination
                      page={page}
                      totalPages={totalPages}
                      onPageChange={(p) => {
                        setPage(p);
                        setSelectedIds(new Set());
                      }}
                    />
                  )}
                </div>
              </div>
            ) : undefined
          }
        >
              <thead>
                <tr className="border-b border-white/10 bg-white/5">
                  <th className="w-12 py-3 px-4">
                    <input
                      type="checkbox"
                      aria-label="Select all file types on this page"
                      checked={allOnPageSelected}
                      ref={(el) => {
                        if (el) el.indeterminate = someOnPageSelected;
                      }}
                      onChange={toggleSelectAllOnPage}
                      className="h-4 w-4 rounded border-white/30 bg-white/10 accent-purple-500"
                    />
                  </th>
                  <th className="text-left py-3 px-4 text-sm font-medium text-white/80">
                    Extension
                  </th>
                  <th className="text-left py-3 px-4 text-sm font-medium text-white/80">
                    Display Name
                  </th>
                  <th className="text-left py-3 px-4 text-sm font-medium text-white/80">
                    Enabled
                  </th>
                  <th
                    className={cn(
                      "text-left py-3 px-4 text-sm font-medium text-white/80",
                      colLg
                    )}
                  >
                    Max Size
                  </th>
                  <th
                    className={cn(
                      "text-left py-3 px-4 text-sm font-medium text-white/80",
                      colLg
                    )}
                  >
                    Max / Upload
                  </th>
                  <th
                    className={cn(
                      "text-left py-3 px-4 text-sm font-medium text-white/80 min-w-[10rem]",
                      colXl
                    )}
                  >
                    Description
                  </th>
                  <th
                    className={cn(
                      "text-left py-3 px-4 text-sm font-medium text-white/80",
                      colLg
                    )}
                  >
                    Preview
                  </th>
                  <th
                    className={cn(
                      "text-left py-3 px-4 text-sm font-medium text-white/80",
                      colLg
                    )}
                  >
                    Compress
                  </th>
                  <th className="text-left py-3 px-4 text-sm font-medium text-white/80">
                    Category
                  </th>
                  <th
                    className={cn(
                      "text-left py-3 px-4 text-sm font-medium text-white/80",
                      colXl
                    )}
                  >
                    MIME
                  </th>
                  <th
                    className={cn(
                      "text-left py-3 px-4 text-sm font-medium text-white/80 min-w-[8rem]",
                      colXl
                    )}
                  >
                    Created by
                  </th>
                  <th
                    className={cn(
                      "text-left py-3 px-4 text-sm font-medium text-white/80",
                      colXl
                    )}
                  >
                    Created
                  </th>
                  <th className="text-right py-3 px-4 text-sm font-medium text-white/80">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody>
                {fileTypes.map((ft) => {
                  const rowId = ft.id ?? "";
                  const createdByTitle = [ft.createdByUsername, ft.createdByEmail]
                    .filter(Boolean)
                    .join(" · ");
                  return (
                    <tr
                      key={rowId}
                      className="border-b border-white/5 hover:bg-white/5"
                    >
                      <td className="py-3 px-4">
                        <input
                          type="checkbox"
                          aria-label={`Select ${ft.fileExtension}`}
                          checked={rowId ? selectedIds.has(rowId) : false}
                          disabled={!rowId}
                          onChange={() => rowId && toggleRowSelection(rowId)}
                          className="h-4 w-4 rounded border-white/30 bg-white/10 accent-purple-500"
                        />
                      </td>
                      <td className="py-3 px-4 text-white font-mono">
                        {ft.fileExtension}
                      </td>
                      <td className="py-3 px-4 text-white">
                        {ft.displayName ?? ft.fileExtension}
                      </td>
                      <td className="py-3 px-4">
                        <Switch
                          checked={ft.enabled ?? true}
                          onCheckedChange={(v) =>
                            requestEnabledChange(ft, v)
                          }
                        />
                      </td>
                      <td
                        className={cn(
                          "py-3 px-4 text-white/60 text-sm",
                          colLg
                        )}
                      >
                        {formatBytes(ft.maxFileSizeBytes)}
                      </td>
                      <td
                        className={cn(
                          "py-3 px-4 text-white/60 text-sm",
                          colLg
                        )}
                      >
                        {ft.maxPerUpload ?? "—"}
                      </td>
                      <td
                        className={cn(
                          "py-3 px-4 text-white/70 text-sm max-w-xs line-clamp-2",
                          colXl
                        )}
                        title={ft.description || undefined}
                      >
                        {ft.description?.trim() ? ft.description : "—"}
                      </td>
                      <td className={cn("py-3 px-4", colLg)}>
                        {boolBadge(ft.requiresPreview)}
                      </td>
                      <td className={cn("py-3 px-4", colLg)}>
                        {boolBadge(ft.isCompressible)}
                      </td>
                      <td className="py-3 px-4 text-white/80 text-sm">
                        {ft.category || "—"}
                      </td>
                      <td
                        className={cn(
                          "py-3 px-4 text-white/60 text-sm max-w-[8rem] truncate",
                          colXl
                        )}
                        title={ft.mimeType || undefined}
                      >
                        {ft.mimeType ?? "—"}
                      </td>
                      <td
                        className={cn("py-3 px-4 text-sm", colXl)}
                        title={createdByTitle || undefined}
                      >
                        <div className="text-white">
                          {ft.createdByUsername || "Unknown"}
                        </div>
                        {ft.createdByEmail ? (
                          <div className="text-white/50 text-xs truncate max-w-[8rem]">
                            {ft.createdByEmail}
                          </div>
                        ) : null}
                      </td>
                      <td
                        className={cn(
                          "py-3 px-4 text-white/60 text-sm",
                          colXl
                        )}
                      >
                        {formatDate(ft.createdAt)}
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
                  );
                })}
              </tbody>
        </AdminScrollTable>
      )}

      <Sheet
        open={!!editTarget && !!editForm}
        onOpenChange={(open) => {
          if (!open) closeEditSheet();
        }}
      >
        <SheetContent side="right" variant="glass" className="sm:max-w-md">
          <SheetHeader className="pr-10">
            <SheetTitle>Edit File Type</SheetTitle>
            <SheetDescription>
              Update file type configuration for uploads.
            </SheetDescription>
          </SheetHeader>
          {editForm && editTarget && (
            <div className="flex-1 space-y-4 overflow-y-auto px-4 pb-4">
              <SettingsField label="Extension">
                <Input
                  variant="glass"
                  value={editTarget.fileExtension ?? ""}
                  readOnly
                  className="text-white/70"
                />
              </SettingsField>
              <SettingsField
                label="Display name"
                description="Shown in upload UI and admin lists."
              >
                <Input
                  variant="glass"
                  value={editForm.displayName}
                  onChange={(e) =>
                    setEditForm((p) =>
                      p ? { ...p, displayName: e.target.value } : p
                    )
                  }
                  className={cn(
                    "text-white",
                    validation.displayName && "border-red-500/50"
                  )}
                />
                {validation.displayName && (
                  <p className="text-red-400 text-xs mt-1">
                    {validation.displayName}
                  </p>
                )}
              </SettingsField>
              <SettingsField
                label="Enabled"
                description="When on, this type is included in upload config (GetFileConfig)."
              >
                <Switch
                  checked={editForm.enabled}
                  onCheckedChange={(v) =>
                    setEditForm((p) => (p ? { ...p, enabled: v } : p))
                  }
                />
              </SettingsField>
              <SettingsField
                label="Max file size (MB)"
                description={`Stored as bytes on the server. ${editForm.maxSizeMb ? `≈ ${displayMbToBytes(parseFloat(editForm.maxSizeMb) || 0).toLocaleString()} bytes` : ""}`}
              >
                <Input
                  variant="glass"
                  type="number"
                  step="0.01"
                  min="0"
                  value={editForm.maxSizeMb}
                  onChange={(e) =>
                    setEditForm((p) =>
                      p ? { ...p, maxSizeMb: e.target.value } : p
                    )
                  }
                  className="text-white"
                />
                {validation.maxSizeMb && (
                  <p className="text-red-400 text-xs mt-1">
                    {validation.maxSizeMb}
                  </p>
                )}
              </SettingsField>
              <SettingsField
                label="Max per upload"
                description="Maximum files of this type allowed in one upload batch (1–100)."
              >
                <Input
                  variant="glass"
                  type="number"
                  value={editForm.maxPerUpload}
                  onChange={(e) =>
                    setEditForm((p) =>
                      p ? { ...p, maxPerUpload: e.target.value } : p
                    )
                  }
                  className="text-white"
                />
                {validation.maxPerUpload && (
                  <p className="text-red-400 text-xs mt-1">
                    {validation.maxPerUpload}
                  </p>
                )}
              </SettingsField>
              <SettingsField label="MIME type">
                <Input
                  variant="glass"
                  value={editForm.mimeType}
                  onChange={(e) =>
                    setEditForm((p) =>
                      p ? { ...p, mimeType: e.target.value } : p
                    )
                  }
                  className="text-white"
                />
                {validation.mimeType && (
                  <p className="text-red-400 text-xs mt-1">
                    {validation.mimeType}
                  </p>
                )}
              </SettingsField>
              <SettingsField label="Description">
                <Textarea
                  variant="glass"
                  value={editForm.description}
                  onChange={(e) =>
                    setEditForm((p) =>
                      p ? { ...p, description: e.target.value } : p
                    )
                  }
                  rows={3}
                  className="text-white"
                />
                {validation.description && (
                  <p className="text-red-400 text-xs mt-1">
                    {validation.description}
                  </p>
                )}
              </SettingsField>
              <SettingsField
                label="Requires preview"
                description="Upload flow may require a preview or thumbnail for this type."
              >
                <Switch
                  checked={editForm.requiresPreview}
                  onCheckedChange={(v) =>
                    setEditForm((p) =>
                      p ? { ...p, requiresPreview: v } : p
                    )
                  }
                />
              </SettingsField>
              <SettingsField
                label="Is compressible"
                description="When site compression is enabled, large files of this type may be compressed."
              >
                <Switch
                  checked={editForm.isCompressible}
                  onCheckedChange={(v) =>
                    setEditForm((p) =>
                      p ? { ...p, isCompressible: v } : p
                    )
                  }
                />
              </SettingsField>
              <SettingsField
                label="Category"
                description="Groups types in the upload picker (with priority for order)."
              >
                <Select
                  value={editForm.category}
                  onValueChange={(v) =>
                    setEditForm((p) => (p ? { ...p, category: v } : p))
                  }
                >
                  <SelectTrigger variant="glass" className="text-white">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent variant="glass">
                    {FILE_TYPE_CATEGORY_OPTIONS.map((c) => (
                      <SelectItem key={c} value={c}>
                        {c}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </SettingsField>
              <SettingsField
                label="Priority"
                description="Lower numbers appear first within the same category."
              >
                <Input
                  variant="glass"
                  type="number"
                  value={editForm.priority}
                  onChange={(e) =>
                    setEditForm((p) =>
                      p ? { ...p, priority: e.target.value } : p
                    )
                  }
                  className="text-white"
                />
              </SettingsField>
              <SettingsField
                label="Default type"
                description="Seeded defaults; disabling may affect expected formats."
              >
                <Switch
                  checked={editForm.isDefault}
                  onCheckedChange={(v) =>
                    setEditForm((p) => (p ? { ...p, isDefault: v } : p))
                  }
                />
              </SettingsField>
              <div className="rounded-lg border border-white/10 bg-white/5 p-3 text-xs text-white/60 space-y-1">
                <p>Created: {formatDate(editTarget.createdAt)}</p>
                <p>
                  Created by: {editTarget.createdByUsername ?? "Unknown"}
                  {editTarget.createdByEmail
                    ? ` (${editTarget.createdByEmail})`
                    : ""}
                </p>
                {editTarget.updatedAt ? (
                  <p>Updated: {formatDate(editTarget.updatedAt)}</p>
                ) : null}
                {editTarget.updatedByUsername ? (
                  <p>Updated by: {editTarget.updatedByUsername}</p>
                ) : null}
              </div>
            </div>
          )}
          <SheetFooter className="sm:flex-row sm:justify-end">
            <Button variant="glass" onClick={closeEditSheet} className="text-white">
              Cancel
            </Button>
            <Button
              variant="glass"
              onClick={handleSave}
              disabled={updateMutation.isLoading || !validation.valid}
              className="text-white"
            >
              {updateMutation.isLoading ? "Saving..." : "Save"}
            </Button>
          </SheetFooter>
        </SheetContent>
      </Sheet>

      <Dialog
        open={!!disableConfirm}
        onOpenChange={(open) => !open && setDisableConfirm(null)}
      >
        <DialogContent variant="glass" className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>Disable file type?</DialogTitle>
            <DialogDescription>{disableConfirm?.reason}</DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDisableConfirm(null)}>
              Cancel
            </Button>
            <Button
              variant="glass"
              onClick={async () => {
                if (!disableConfirm) return;
                if (editTarget && editForm && disableConfirm.ft.id === editTarget.id && !disableConfirm.nextEnabled) {
                  const maxFileSizeBytes = displayMbToBytes(parseFloat(editForm.maxSizeMb));
                  const cmd = buildUpdateFileSettingsCommand(editTarget, {
                    displayName: editForm.displayName.trim(),
                    enabled: false,
                    maxFileSizeBytes,
                    maxPerUpload: parseInt(editForm.maxPerUpload, 10),
                    mimeType: editForm.mimeType.trim(),
                    description: editForm.description.trim(),
                    requiresPreview: editForm.requiresPreview,
                    isCompressible: editForm.isCompressible,
                    category: editForm.category.trim(),
                    priority: parseInt(editForm.priority, 10) || 0,
                    isDefault: editForm.isDefault,
                  });
                  await updateMutation.mutate(cmd);
                  closeEditSheet();
                } else {
                  await applyEnabledChange(
                    disableConfirm.ft,
                    disableConfirm.nextEnabled
                  );
                }
                setDisableConfirm(null);
              }}
            >
              Disable anyway
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </SettingsSection>
  );
}
