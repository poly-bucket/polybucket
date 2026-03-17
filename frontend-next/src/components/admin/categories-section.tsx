"use client";

import { useState } from "react";
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
  getCategories,
  createCategory,
  updateCategory,
  deleteCategory,
} from "@/lib/services/adminService";
import { useAdminQuery } from "@/lib/hooks/use-admin-query";
import { useAdminMutation } from "@/lib/hooks/use-admin-mutation";
import { formatDate } from "@/lib/utils/format";
import type { CategoryDto } from "@/lib/api/client";
import { CreateCategoryCommand, UpdateCategoryCommand } from "@/lib/api/client";

export function CategoriesSection() {
  const {
    data: categoriesData,
    isLoading,
    error,
    refetch,
  } = useAdminQuery(() => getCategories(1, 100, undefined));

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

  const [createOpen, setCreateOpen] = useState(false);
  const [createName, setCreateName] = useState("");
  const [editTarget, setEditTarget] = useState<CategoryDto | null>(null);
  const [editName, setEditName] = useState("");
  const [deleteTarget, setDeleteTarget] = useState<CategoryDto | null>(null);

  const categories = categoriesData?.categories ?? [];

  const handleCreate = async () => {
    const name = createName.trim();
    if (!name) return;
    await createMutation.mutate(new CreateCategoryCommand({ name }));
    setCreateOpen(false);
    setCreateName("");
  };

  const handleUpdate = async () => {
    if (!editTarget?.id || !editName.trim()) return;
    await updateMutation.mutate([
      editTarget.id,
      new UpdateCategoryCommand({ name: editName.trim() }),
    ]);
    setEditTarget(null);
    setEditName("");
  };

  const handleDelete = async () => {
    if (!deleteTarget?.id) return;
    await deleteMutation.mutate(deleteTarget.id);
    setDeleteTarget(null);
  };

  const openEdit = (c: CategoryDto) => {
    setEditTarget(c);
    setEditName(c.name ?? "");
  };

  return (
    <SettingsSection
      title="Categories"
      description="Manage model categories for organization and filtering"
    >
      {error && (
        <div className="rounded-lg border border-red-500/50 bg-red-500/10 px-4 py-3 text-red-400 mb-4">
          {error}
        </div>
      )}

      <div className="flex justify-end mb-4">
        <Button variant="glass" onClick={() => setCreateOpen(true)}>
          <Plus className="h-4 w-4 mr-2" />
          Create Category
        </Button>
      </div>

      {isLoading ? (
        <div className="text-center text-white/60 py-8">Loading categories...</div>
      ) : categories.length === 0 ? (
        <div className="text-center text-white/60 py-8 rounded-lg border border-white/10 glass-bg">
          No categories yet. Create one to get started.
        </div>
      ) : (
        <div className="rounded-lg border border-white/10 overflow-hidden">
          <table className="w-full">
            <thead>
              <tr className="border-b border-white/10 bg-white/5">
                <th className="text-left py-3 px-4 text-sm font-medium text-white/80">
                  Name
                </th>
                <th className="text-left py-3 px-4 text-sm font-medium text-white/80">
                  Created
                </th>
                <th className="text-right py-3 px-4 text-sm font-medium text-white/80">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody>
              {categories.map((c) => (
                <tr
                  key={c.id}
                  className="border-b border-white/5 hover:bg-white/5"
                >
                  <td className="py-3 px-4 text-white">{c.name}</td>
                  <td className="py-3 px-4 text-white/60 text-sm">
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
              ))}
            </tbody>
          </table>
        </div>
      )}

      <Dialog open={createOpen} onOpenChange={setCreateOpen}>
        <DialogContent variant="glass">
          <DialogHeader>
            <DialogTitle>Create Category</DialogTitle>
            <DialogDescription>
              Enter a name for the new category.
            </DialogDescription>
          </DialogHeader>
          <div>
            <label className="text-sm font-medium text-white/80">Name</label>
            <Input
              variant="glass"
              value={createName}
              onChange={(e) => setCreateName(e.target.value)}
              placeholder="Category name"
              className="mt-1 text-white"
            />
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setCreateOpen(false)}>
              Cancel
            </Button>
            <Button
              variant="glass"
              onClick={handleCreate}
              disabled={
                createMutation.isLoading || !createName.trim()
              }
            >
              {createMutation.isLoading ? "Creating..." : "Create"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog
        open={!!editTarget}
        onOpenChange={(open) => !open && setEditTarget(null)}
      >
        <DialogContent variant="glass">
          <DialogHeader>
            <DialogTitle>Edit Category</DialogTitle>
            <DialogDescription>
              Update the category name.
            </DialogDescription>
          </DialogHeader>
          <div>
            <label className="text-sm font-medium text-white/80">Name</label>
            <Input
              variant="glass"
              value={editName}
              onChange={(e) => setEditName(e.target.value)}
              placeholder="Category name"
              className="mt-1 text-white"
            />
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setEditTarget(null)}>
              Cancel
            </Button>
            <Button
              variant="glass"
              onClick={handleUpdate}
              disabled={
                updateMutation.isLoading || !editName.trim()
              }
            >
              {updateMutation.isLoading ? "Saving..." : "Save"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

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
    </SettingsSection>
  );
}
