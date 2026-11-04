"use client";

import { useMemo, useState } from "react";
import { Plus, Pencil, Trash2 } from "lucide-react";
import { SettingsSection } from "@/components/settings/settings-section";
import { Button } from "@/components/primitives/button";
import { Input } from "@/components/primitives/input";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetFooter,
  SheetHeader,
  SheetTitle,
} from "@/components/ui/sheet";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { DataTablePagination } from "@/components/primitives/pagination";
import {
  getCategories,
  createCategory,
  updateCategory,
  deleteCategory,
  deleteCategories,
} from "@/lib/services/adminService";
import { useAdminQuery } from "@/lib/hooks/use-admin-query";
import { useAdminMutation } from "@/lib/hooks/use-admin-mutation";
import { formatDate } from "@/lib/utils/format";
import type { CategoryDto } from "@/lib/api/client";
import { CreateCategoryCommand, UpdateCategoryCommand } from "@/lib/api/client";
import { TableRowsSkeleton } from "@/components/ui/skeletons";
import { toast } from "sonner";

import { Textarea } from "@/components/ui/glass/textarea";
import { cn } from "@/lib/utils";
import { AdminScrollTable } from "./admin-scroll-table";

const PAGE_SIZE_OPTIONS = [10, 20, 50, 100] as const;
const colLg = "hidden lg:table-cell";
const colXl = "hidden xl:table-cell";
const DEFAULT_EDIT_COLOR = "#6366f1";

function colorForPicker(hex?: string | null): string {
  if (!hex?.startsWith("#")) {
    return DEFAULT_EDIT_COLOR;
  }
  if (hex.length === 4) {
    return `#${hex[1]}${hex[1]}${hex[2]}${hex[2]}${hex[3]}${hex[3]}`;
  }
  return hex.length === 7 ? hex : DEFAULT_EDIT_COLOR;
}

function ColorCell({ color }: { color?: string | null }) {
  if (!color?.trim()) {
    return <span className="text-white/40">—</span>;
  }
  return (
    <div className="flex items-center gap-2 min-w-[7rem]">
      <span
        className="h-6 w-6 shrink-0 rounded border border-white/20"
        style={{ backgroundColor: color }}
        aria-hidden
      />
      <span className="text-white/70 text-xs font-mono truncate">{color}</span>
    </div>
  );
}

function IconCell({ icon }: { icon?: string | null }) {
  if (!icon?.trim()) {
    return <span className="text-white/40">—</span>;
  }
  return <i className={`${icon} text-white/80`} aria-hidden />;
}

export function CategoriesSection() {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState<number>(20);
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [bulkDeleteOpen, setBulkDeleteOpen] = useState(false);

  const {
    data: categoriesData,
    isLoading,
    error,
    refetch,
  } = useAdminQuery(
    () => getCategories(page, pageSize, undefined),
    [page, pageSize]
  );

  const createMutation = useAdminMutation(
    (cmd: CreateCategoryCommand) => createCategory(cmd),
    { onSuccess: refetch, successMessage: "Category created" }
  );
  const updateMutation = useAdminMutation(
    ([id, cmd]: [string, UpdateCategoryCommand]) => updateCategory(id, cmd),
    { onSuccess: refetch, successMessage: "Category updated" }
  );
  const deleteMutation = useAdminMutation(
    (id: string) => deleteCategory(id),
    { onSuccess: refetch, successMessage: "Category deleted" }
  );
  const bulkDeleteMutation = useAdminMutation(
    (ids: string[]) => deleteCategories(ids),
    { onSuccess: refetch, showToast: false }
  );

  const [createOpen, setCreateOpen] = useState(false);
  const [createName, setCreateName] = useState("");
  const [editTarget, setEditTarget] = useState<CategoryDto | null>(null);
  const [editName, setEditName] = useState("");
  const [editDescription, setEditDescription] = useState("");
  const [editColor, setEditColor] = useState(DEFAULT_EDIT_COLOR);
  const [deleteTarget, setDeleteTarget] = useState<CategoryDto | null>(null);

  const categories = categoriesData?.categories ?? [];
  const totalCount = categoriesData?.totalCount ?? 0;
  const totalPages = categoriesData?.totalPages ?? 1;

  const pageIds = useMemo(
    () => categories.map((c) => c.id).filter(Boolean) as string[],
    [categories]
  );

  const allOnPageSelected =
    pageIds.length > 0 && pageIds.every((id) => selectedIds.has(id));

  const someOnPageSelected =
    pageIds.some((id) => selectedIds.has(id)) && !allOnPageSelected;

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
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  };

  const closeCreateSheet = () => {
    setCreateOpen(false);
    setCreateName("");
  };

  const handleCreate = async () => {
    const name = createName.trim();
    if (!name) return;
    await createMutation.mutate(new CreateCategoryCommand({ name }));
    closeCreateSheet();
  };

  const closeEditSheet = () => {
    setEditTarget(null);
    setEditName("");
    setEditDescription("");
    setEditColor(DEFAULT_EDIT_COLOR);
  };

  const handleUpdate = async () => {
    if (!editTarget?.id || !editName.trim()) return;
    await updateMutation.mutate([
      editTarget.id,
      new UpdateCategoryCommand({
        id: editTarget.id,
        name: editName.trim(),
        description: editDescription.trim(),
        color: editColor.trim(),
      }),
    ]);
    closeEditSheet();
  };

  const handleDelete = async () => {
    if (!deleteTarget?.id) return;
    await deleteMutation.mutate(deleteTarget.id);
    setDeleteTarget(null);
  };

  const handleBulkDelete = async () => {
    const ids = Array.from(selectedIds);
    if (ids.length === 0) return;

    const result = await bulkDeleteMutation.mutate(ids);
    setBulkDeleteOpen(false);
    setSelectedIds(new Set());

    if (!result) return;

    if (result.failedCount === 0) {
      toast.success(
        result.deletedCount === 1
          ? "1 category deleted"
          : `${result.deletedCount} categories deleted`
      );
    } else if (result.deletedCount === 0) {
      toast.error("No categories could be deleted");
    } else {
      toast.warning(
        `${result.deletedCount} deleted, ${result.failedCount} could not be deleted`
      );
    }
  };

  const openEdit = (c: CategoryDto) => {
    setEditTarget(c);
    setEditName(c.name ?? "");
    setEditDescription(c.description ?? "");
    setEditColor(colorForPicker(c.color));
  };

  const handlePageSizeChange = (value: string) => {
    setPageSize(Number(value));
    setPage(1);
    setSelectedIds(new Set());
  };

  return (
    <SettingsSection
      className="min-w-0"
      title="Categories"
      description="Manage model categories for organization and filtering"
    >
      {error && (
        <div className="rounded-lg border border-red-500/50 bg-red-500/10 px-4 py-3 text-red-400 mb-4">
          {error}
        </div>
      )}

      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between mb-4">
        <div className="flex flex-wrap items-center gap-2">
          {selectedIds.size > 0 && (
            <Button
              variant="destructive"
              onClick={() => setBulkDeleteOpen(true)}
              disabled={bulkDeleteMutation.isLoading}
            >
              <Trash2 className="h-4 w-4 mr-2" />
              Delete selected ({selectedIds.size})
            </Button>
          )}
        </div>
        <div className="flex flex-wrap items-center gap-2 justify-end">
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
          <Button variant="glass" onClick={() => setCreateOpen(true)}>
            <Plus className="h-4 w-4 mr-2" />
            Create Category
          </Button>
        </div>
      </div>

      {isLoading ? (
        <TableRowsSkeleton variant="default" rows={5} />
      ) : categories.length === 0 ? (
        <div className="text-center text-white/60 py-8 rounded-lg border border-white/10 glass-bg">
          No categories yet. Create one to get started.
        </div>
      ) : (
        <AdminScrollTable
          tableClassName="min-w-[28rem] lg:min-w-[40rem] xl:min-w-[52rem]"
          footer={
            totalCount > 0 ? (
              <div className="border-t border-white/10 px-4 py-3 bg-white/5">
                <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                  <div className="text-sm text-white/60">
                    Showing {(page - 1) * pageSize + 1} to{" "}
                    {Math.min(page * pageSize, totalCount)} of {totalCount}{" "}
                    categories
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
                    aria-label="Select all categories on this page"
                    checked={allOnPageSelected}
                    ref={(el) => {
                      if (el) {
                        el.indeterminate = someOnPageSelected;
                      }
                    }}
                    onChange={toggleSelectAllOnPage}
                    className="h-4 w-4 rounded border-white/30 bg-white/10 accent-purple-500"
                  />
                </th>
                <th className="text-left py-3 px-4 text-sm font-medium text-white/80">
                  Name
                </th>
                <th className="text-left py-3 px-4 text-sm font-medium text-white/80">
                  Color
                </th>
                <th className="text-left py-3 px-4 text-sm font-medium text-white/80">
                  Icon
                </th>
                <th
                  className={cn(
                    "text-left py-3 px-4 text-sm font-medium text-white/80 min-w-[12rem]",
                    colLg
                  )}
                >
                  Description
                </th>
                <th
                  className={cn(
                    "text-left py-3 px-4 text-sm font-medium text-white/80 min-w-[10rem]",
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
              {categories.map((c) => {
                const rowId = c.id ?? "";
                const createdByTitle = [c.createdByUsername, c.createdByEmail]
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
                        aria-label={`Select ${c.name}`}
                        checked={rowId ? selectedIds.has(rowId) : false}
                        disabled={!rowId}
                        onChange={() => rowId && toggleRowSelection(rowId)}
                        className="h-4 w-4 rounded border-white/30 bg-white/10 accent-purple-500"
                      />
                    </td>
                    <td className="py-3 px-4 text-white">{c.name}</td>
                    <td className="py-3 px-4">
                      <ColorCell color={c.color} />
                    </td>
                    <td className="py-3 px-4">
                      <IconCell icon={c.icon} />
                    </td>
                    <td
                      className={cn(
                        "py-3 px-4 text-white/70 text-sm max-w-xs line-clamp-2",
                        colLg
                      )}
                      title={c.description || undefined}
                    >
                      {c.description?.trim() ? c.description : "—"}
                    </td>
                    <td
                      className={cn("py-3 px-4 text-sm", colXl)}
                      title={createdByTitle || undefined}
                    >
                      <div className="text-white">{c.createdByUsername || "Unknown"}</div>
                      {c.createdByEmail ? (
                        <div className="text-white/50 text-xs truncate max-w-[10rem]">
                          {c.createdByEmail}
                        </div>
                      ) : null}
                    </td>
                    <td
                      className={cn(
                        "py-3 px-4 text-white/60 text-sm",
                        colXl
                      )}
                    >
                      {formatDate(c.createdAt)}
                    </td>
                    <td className="py-3 px-4 text-right">
                      <div className="flex justify-end gap-2">
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => openEdit(c)}
                          className="text-white/80 hover:text-white"
                        >
                          <Pencil className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => setDeleteTarget(c)}
                          className="text-red-400 hover:text-red-300"
                        >
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
        </AdminScrollTable>
      )}

      <Sheet
        open={createOpen}
        onOpenChange={(open) => {
          if (open) setCreateOpen(true);
          else closeCreateSheet();
        }}
      >
        <SheetContent side="right" variant="glass" className="sm:max-w-md">
          <SheetHeader className="pr-10">
            <SheetTitle>Create Category</SheetTitle>
            <SheetDescription>
              Enter a name for the new category.
            </SheetDescription>
          </SheetHeader>
          <div className="flex-1 space-y-4 overflow-y-auto px-4">
            <div>
              <label className="text-sm font-medium text-white/80">Name</label>
              <Input
                variant="glass"
                value={createName}
                onChange={(e) => setCreateName(e.target.value)}
                placeholder="Category name"
                className="mt-1 text-white placeholder:text-white/50"
              />
            </div>
          </div>
          <SheetFooter className="sm:flex-row sm:justify-end">
            <Button variant="glass" onClick={closeCreateSheet} className="text-white">
              Cancel
            </Button>
            <Button
              variant="glass"
              onClick={handleCreate}
              className="text-white"
              disabled={createMutation.isLoading || !createName.trim()}
            >
              {createMutation.isLoading ? "Creating..." : "Create Category"}
            </Button>
          </SheetFooter>
        </SheetContent>
      </Sheet>

      <Sheet
        open={!!editTarget}
        onOpenChange={(open) => {
          if (!open) closeEditSheet();
        }}
      >
        <SheetContent side="right" variant="glass" className="sm:max-w-md">
          <SheetHeader className="pr-10">
            <SheetTitle>Edit Category</SheetTitle>
            <SheetDescription>
              Update the category name, description, and color.
            </SheetDescription>
          </SheetHeader>
          <div className="flex-1 space-y-4 overflow-y-auto px-4">
            <div>
              <label className="text-sm font-medium text-white/80">Name</label>
              <Input
                variant="glass"
                value={editName}
                onChange={(e) => setEditName(e.target.value)}
                placeholder="Category name"
                className="mt-1 text-white placeholder:text-white/50"
              />
            </div>
            <div>
              <label className="text-sm font-medium text-white/80">
                Description
              </label>
              <Textarea
                variant="glass"
                value={editDescription}
                onChange={(e) => setEditDescription(e.target.value)}
                placeholder="Describe this category"
                rows={4}
                className="mt-1 text-white placeholder:text-white/50"
              />
            </div>
            <div>
              <label className="text-sm font-medium text-white/80">Color</label>
              <div className="mt-1 flex items-center gap-2">
                <input
                  type="color"
                  value={colorForPicker(editColor)}
                  onChange={(e) => setEditColor(e.target.value)}
                  className="h-10 w-10 cursor-pointer rounded border-2 border-white/20 bg-transparent"
                />
                <Input
                  variant="glass"
                  value={editColor}
                  onChange={(e) => setEditColor(e.target.value)}
                  className="flex-1 text-white font-mono text-sm"
                  placeholder="#6366f1"
                />
              </div>
            </div>
          </div>
          <SheetFooter className="sm:flex-row sm:justify-end">
            <Button variant="glass" onClick={closeEditSheet} className="text-white">
              Cancel
            </Button>
            <Button
              variant="glass"
              onClick={handleUpdate}
              className="text-white"
              disabled={updateMutation.isLoading || !editName.trim()}
            >
              {updateMutation.isLoading ? "Saving..." : "Save"}
            </Button>
          </SheetFooter>
        </SheetContent>
      </Sheet>

      <Dialog
        open={!!deleteTarget}
        onOpenChange={(open) => !open && setDeleteTarget(null)}
      >
        <DialogContent variant="glass">
          <DialogHeader>
            <DialogTitle>Delete Category</DialogTitle>
            <DialogDescription>
              Are you sure you want to delete &quot;{deleteTarget?.name}&quot;?
              This cannot be undone.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDeleteTarget(null)}>
              Cancel
            </Button>
            <Button
              variant="destructive"
              onClick={handleDelete}
              disabled={deleteMutation.isLoading}
            >
              {deleteMutation.isLoading ? "Deleting..." : "Delete"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog open={bulkDeleteOpen} onOpenChange={setBulkDeleteOpen}>
        <DialogContent variant="glass">
          <DialogHeader>
            <DialogTitle>Delete selected categories</DialogTitle>
            <DialogDescription>
              Delete {selectedIds.size} selected{" "}
              {selectedIds.size === 1 ? "category" : "categories"}? Categories
              in use by models will not be removed.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setBulkDeleteOpen(false)}>
              Cancel
            </Button>
            <Button
              variant="destructive"
              onClick={handleBulkDelete}
              disabled={bulkDeleteMutation.isLoading}
            >
              {bulkDeleteMutation.isLoading ? "Deleting..." : "Delete selected"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </SettingsSection>
  );
}
