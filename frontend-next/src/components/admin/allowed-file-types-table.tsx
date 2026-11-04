"use client";

import { useEffect, useState } from "react";
import { SettingsField } from "@/components/settings/settings-field";
import { Button } from "@/components/primitives/button";
import { Switch } from "@/components/primitives/switch";
import { DataTablePagination } from "@/components/primitives/pagination";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { getFileSettings, updateFileSettings } from "@/lib/services/adminService";
import { useAdminQuery } from "@/lib/hooks/use-admin-query";
import { useAdminMutation } from "@/lib/hooks/use-admin-mutation";
import type { FileTypeSettingsData } from "@/lib/api/client";
import { UpdateFileSettingsCommand } from "@/lib/api/client";
import { TableRowsSkeleton } from "@/components/ui/skeletons";
import {
  buildUpdateFileSettingsCommand,
  getFileTypeDisableConfirmReason,
} from "@/lib/utils/file-type-settings-utils";
import { AdminScrollTable } from "./admin-scroll-table";

const PAGE_SIZE = 50;

type AllowedFileTypesTableProps = {
  onDerivedAllowedFileTypesChange?: (derived: string) => void;
};

export function AllowedFileTypesTable({
  onDerivedAllowedFileTypesChange,
}: AllowedFileTypesTableProps) {
  const [page, setPage] = useState(1);
  const [disableConfirm, setDisableConfirm] = useState<{
    ft: FileTypeSettingsData;
    nextEnabled: boolean;
    reason: string;
  } | null>(null);

  const {
    data: fileSettings,
    isLoading,
    error,
    refetch,
  } = useAdminQuery(
    () => getFileSettings(page, PAGE_SIZE, undefined, undefined),
    [page]
  );

  const updateMutation = useAdminMutation(
    (cmd: UpdateFileSettingsCommand) => updateFileSettings(cmd),
    {
      onSuccess: () => refetch(),
      successMessage: "Allowed file type updated",
    }
  );

  const fileTypes = fileSettings?.fileTypes ?? [];
  const totalCount = fileSettings?.totalCount ?? 0;
  const totalPages = fileSettings?.totalPages ?? 1;

  useEffect(() => {
    const derived = fileSettings?.derivedAllowedFileTypes;
    if (derived != null) {
      onDerivedAllowedFileTypesChange?.(derived);
    }
  }, [fileSettings?.derivedAllowedFileTypes, onDerivedAllowedFileTypesChange]);

  const applyAllowedChange = async (
    ft: FileTypeSettingsData,
    enabled: boolean
  ) => {
    const cmd = buildUpdateFileSettingsCommand(ft, { enabled });
    await updateMutation.mutate(cmd);
  };

  const requestAllowedChange = (
    ft: FileTypeSettingsData,
    nextEnabled: boolean
  ) => {
    const reason = getFileTypeDisableConfirmReason(ft, nextEnabled, fileTypes);
    if (reason) {
      setDisableConfirm({ ft, nextEnabled, reason });
      return;
    }
    void applyAllowedChange(ft, nextEnabled);
  };

  return (
    <>
      <SettingsField
        label="Allowed file types"
        description="Allowed extensions for model uploads. Synced with the File Types tab; changes apply immediately."
      >
        {error && (
          <div className="rounded-lg border border-red-500/50 bg-red-500/10 px-3 py-2 text-red-400 text-sm mb-2">
            {error}
          </div>
        )}
        {isLoading ? (
          <TableRowsSkeleton variant="default" rows={3} />
        ) : fileTypes.length === 0 ? (
          <div className="text-center text-white/60 text-sm py-4 rounded-lg border border-white/10 glass-bg">
            No file types configured.
          </div>
        ) : (
          <AdminScrollTable
            className="[&>div:first-child]:max-h-56 [&>div:first-child]:overflow-y-auto"
            tableClassName="min-w-[16rem] text-sm"
            footer={
              totalCount > 0 ? (
                <div className="border-t border-white/10 px-2 py-2 bg-white/5">
                  <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
                    <div className="text-xs text-white/60">
                      {(page - 1) * PAGE_SIZE + 1}–
                      {Math.min(page * PAGE_SIZE, totalCount)} of {totalCount}
                    </div>
                    {totalPages > 1 && (
                      <DataTablePagination
                        page={page}
                        totalPages={totalPages}
                        onPageChange={setPage}
                      />
                    )}
                  </div>
                </div>
              ) : undefined
            }
          >
            <thead>
              <tr className="border-b border-white/10 bg-white/5 sticky top-0 z-10">
                <th className="text-left py-1.5 px-2 text-xs font-medium text-white/70">
                  File type
                </th>
                <th className="text-left py-1.5 px-2 text-xs font-medium text-white/70">
                  Category
                </th>
                <th className="text-right py-1.5 px-2 text-xs font-medium text-white/70 w-20">
                  Allowed
                </th>
              </tr>
            </thead>
            <tbody>
              {fileTypes.map((ft) => {
                const rowId = ft.id ?? ft.fileExtension ?? "";
                const allowed = ft.enabled ?? true;
                const label =
                  ft.displayName && ft.displayName !== ft.fileExtension
                    ? `${ft.fileExtension} (${ft.displayName})`
                    : ft.fileExtension;
                return (
                  <tr
                    key={rowId}
                    className="border-b border-white/5 hover:bg-white/5"
                  >
                    <td
                      className="py-1.5 px-2 font-mono text-xs text-white truncate max-w-[10rem]"
                      title={label ?? undefined}
                    >
                      {ft.fileExtension}
                    </td>
                    <td className="py-1.5 px-2 text-white/70 text-xs whitespace-nowrap">
                      {ft.category || "—"}
                    </td>
                    <td className="py-1 px-2">
                      <div className="flex justify-end">
                        <Switch
                          checked={allowed}
                          disabled={updateMutation.isLoading || !ft.id}
                          aria-label={`Allow ${ft.fileExtension} for model uploads`}
                          onCheckedChange={(v) => requestAllowedChange(ft, v)}
                        />
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </AdminScrollTable>
        )}
      </SettingsField>

      <Dialog
        open={!!disableConfirm}
        onOpenChange={(open) => !open && setDisableConfirm(null)}
      >
        <DialogContent variant="glass">
          <DialogHeader>
            <DialogTitle>Disable file type?</DialogTitle>
            <DialogDescription>{disableConfirm?.reason}</DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setDisableConfirm(null)}
              className="text-white"
            >
              Cancel
            </Button>
            <Button
              variant="glass"
              className="text-white"
              onClick={() => {
                if (!disableConfirm) return;
                void applyAllowedChange(
                  disableConfirm.ft,
                  disableConfirm.nextEnabled
                );
                setDisableConfirm(null);
              }}
            >
              Disable anyway
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}
