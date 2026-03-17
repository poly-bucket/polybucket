"use client";

import { useState, useEffect, useCallback, useMemo } from "react";
import {
  Plus,
  Pencil,
  Trash2,
  ChevronDown,
  ChevronUp,
  Shield,
  Copy,
  MoreHorizontal,
} from "lucide-react";
import { Card, CardContent } from "@/components/primitives/card";
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
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  getRoles,
  getAllPermissions,
  createRole,
  updateRole,
  deleteRole,
  assignPermissionsToRole,
  removePermissionsFromRole,
} from "@/lib/services/adminService";
import { toast } from "sonner";
import type { RoleDto, PermissionDto } from "@/lib/api/client";
import {
  CreateRoleRequest,
  UpdateRoleRequest,
  AssignPermissionsRequest,
  RemovePermissionsRequest,
} from "@/lib/api/client";

const DEFAULT_COLOR = "#3B82F6";

function groupPermissionsByCategory(
  permissions: PermissionDto[]
): Map<string, PermissionDto[]> {
  const map = new Map<string, PermissionDto[]>();
  for (const p of permissions) {
    if (!p.name) continue;
    const cat = p.category ?? "Other";
    const list = map.get(cat) ?? [];
    list.push(p);
    map.set(cat, list);
  }
  return map;
}

export function RolesTab() {
  const [roles, setRoles] = useState<RoleDto[]>([]);
  const [permissions, setPermissions] = useState<PermissionDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [pagination, setPagination] = useState({
    page: 1,
    pageSize: 50,
    totalCount: 0,
    totalPages: 1,
  });
  const [searchTerm, setSearchTerm] = useState("");
  const [sortBy, setSortBy] = useState<"name" | "priority">("priority");
  const [sortDescending, setSortDescending] = useState(true);
  const [expandedRoleId, setExpandedRoleId] = useState<string | null>(null);
  const [pendingPermissions, setPendingPermissions] = useState<
    Record<string, Set<string>>
  >({});
  const [permissionSaving, setPermissionSaving] = useState<string | null>(null);
  const [createOpen, setCreateOpen] = useState(false);
  const [editOpen, setEditOpen] = useState(false);
  const [deleteOpen, setDeleteOpen] = useState(false);
  const [selectedRole, setSelectedRole] = useState<RoleDto | null>(null);
  const [createForm, setCreateForm] = useState({
    name: "",
    description: "",
    priority: 100,
    color: DEFAULT_COLOR,
    initialPermissions: [] as string[],
  });
  const [editForm, setEditForm] = useState({
    name: "",
    description: "",
    priority: 100,
    color: DEFAULT_COLOR,
  });
  const [saving, setSaving] = useState(false);

  const fetchRoles = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await getRoles(
        pagination.page,
        pagination.pageSize,
        searchTerm || undefined,
        sortBy,
        sortDescending
      );
      setRoles(response.roles ?? []);
      setPagination((prev) => ({
        ...prev,
        totalCount: response.pagination?.totalCount ?? 0,
        totalPages: response.pagination?.totalPages ?? 1,
      }));
    } catch {
      setError("Failed to load roles");
      toast.error("Failed to load roles");
    } finally {
      setLoading(false);
    }
  }, [
    pagination.page,
    pagination.pageSize,
    searchTerm,
    sortBy,
    sortDescending,
  ]);

  const fetchPermissions = useCallback(async () => {
    try {
      const perms = await getAllPermissions();
      setPermissions(perms ?? []);
    } catch {
      /* non-critical */
    }
  }, []);

  useEffect(() => {
    fetchRoles();
    fetchPermissions();
  }, [fetchRoles, fetchPermissions]);

  const groupedPermissions = useMemo(
    () => groupPermissionsByCategory(permissions),
    [permissions]
  );

  const getEffectivePermissions = (role: RoleDto): Set<string> => {
    const pending = pendingPermissions[role.id ?? ""];
    if (pending) return pending;
    return new Set(role.permissions ?? []);
  };

  const isPermissionDirty = (role: RoleDto): boolean => {
    const effective = getEffectivePermissions(role);
    const original = new Set(role.permissions ?? []);
    if (effective.size !== original.size) return true;
    for (const p of effective) {
      if (!original.has(p)) return true;
    }
    return false;
  };

  const togglePermissionLocal = (role: RoleDto, permissionName: string) => {
    const roleId = role.id ?? "";
    const effective = new Set(getEffectivePermissions(role));
    if (effective.has(permissionName)) {
      effective.delete(permissionName);
    } else {
      effective.add(permissionName);
    }
    setPendingPermissions((prev) => ({ ...prev, [roleId]: effective }));
  };

  const savePermissions = async (role: RoleDto) => {
    if (!role.id) return;
    const effective = getEffectivePermissions(role);
    const original = new Set(role.permissions ?? []);
    const toAdd = [...effective].filter((p) => !original.has(p));
    const toRemove = [...original].filter((p) => !effective.has(p));

    setPermissionSaving(role.id);
    try {
      if (toAdd.length > 0) {
        await assignPermissionsToRole(
          role.id,
          new AssignPermissionsRequest({ permissionNames: toAdd })
        );
      }
      if (toRemove.length > 0) {
        await removePermissionsFromRole(
          role.id,
          new RemovePermissionsRequest({ permissionNames: toRemove })
        );
      }
      setPendingPermissions((prev) => {
        const next = { ...prev };
        delete next[role.id ?? ""];
        return next;
      });
      toast.success("Permissions updated");
      await fetchRoles();
    } catch {
      toast.error("Failed to update permissions");
    } finally {
      setPermissionSaving(null);
    }
  };

  const handleCreate = async () => {
    if (!createForm.name.trim()) return;
    setSaving(true);
    try {
      const request = new CreateRoleRequest({
        name: createForm.name.trim(),
        description: createForm.description.trim(),
        priority: createForm.priority,
        color: createForm.color,
        isSystemRole: false,
        isDefault: false,
        initialPermissions: createForm.initialPermissions,
      });
      await createRole(request);
      setCreateOpen(false);
      setCreateForm({
        name: "",
        description: "",
        priority: 100,
        color: DEFAULT_COLOR,
        initialPermissions: [],
      });
      toast.success("Role created");
      await fetchRoles();
    } catch {
      toast.error("Failed to create role");
    } finally {
      setSaving(false);
    }
  };

  const handleUpdate = async () => {
    if (!selectedRole?.id || !editForm.name.trim()) return;
    setSaving(true);
    try {
      const request = new UpdateRoleRequest({
        name: editForm.name.trim(),
        description: editForm.description.trim(),
        priority: editForm.priority,
        color: editForm.color,
        isDefault: selectedRole.isDefault,
        isActive: selectedRole.isActive ?? true,
      });
      await updateRole(selectedRole.id, request);
      setEditOpen(false);
      setSelectedRole(null);
      toast.success("Role updated");
      await fetchRoles();
    } catch {
      toast.error("Failed to update role");
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    if (!selectedRole?.id) return;
    setSaving(true);
    try {
      await deleteRole(selectedRole.id);
      setDeleteOpen(false);
      setSelectedRole(null);
      toast.success("Role deleted");
      await fetchRoles();
    } catch {
      toast.error("Failed to delete role");
    } finally {
      setSaving(false);
    }
  };

  const handleDuplicate = (role: RoleDto) => {
    setCreateForm({
      name: `${role.name} (copy)`,
      description: role.description ?? "",
      priority: role.priority ?? 100,
      color: role.color ?? DEFAULT_COLOR,
      initialPermissions: role.permissions ?? [],
    });
    setCreateOpen(true);
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h2 className="text-2xl font-bold text-white">Roles & Permissions</h2>
        <Button variant="glass" onClick={() => setCreateOpen(true)}>
          <Plus className="h-4 w-4 mr-2" />
          Create Role
        </Button>
      </div>

      <Card variant="glass" className="border-white/20">
        <CardContent className="pt-6">
          <div className="flex flex-col sm:flex-row gap-4">
            <div className="relative flex-1">
              <Input
                variant="glass"
                placeholder="Search roles..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="text-white placeholder:text-white/50"
              />
            </div>
            <Select
              value={sortBy}
              onValueChange={(v) =>
                setSortBy(v as "name" | "priority")
              }
            >
              <SelectTrigger variant="glass" className="w-full sm:w-40 text-white">
                <SelectValue />
              </SelectTrigger>
              <SelectContent variant="glass">
                <SelectItem value="priority">Sort by Priority</SelectItem>
                <SelectItem value="name">Sort by Name</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>

      {loading && (
        <Card variant="glass" className="border-white/20">
          <CardContent className="py-12 text-center text-white/60">
            Loading roles...
          </CardContent>
        </Card>
      )}

      {error && !loading && (
        <Card variant="glass" className="border-red-500/50">
          <CardContent className="py-6">
            <div className="text-center text-red-400">
              <p className="font-medium">{error}</p>
              <Button
                variant="outline"
                onClick={() => fetchRoles()}
                className="mt-2"
              >
                Retry
              </Button>
            </div>
          </CardContent>
        </Card>
      )}

      {!loading && !error && (
        <div className="space-y-4">
          {roles.map((role) => {
            const isExpanded = expandedRoleId === role.id;
            const effectivePerms = getEffectivePermissions(role);
            const dirty = isPermissionDirty(role);
            const savingPerms = permissionSaving === role.id;
            return (
              <Card
                key={role.id}
                variant="glass"
                className="border-white/20 overflow-hidden"
              >
                <div
                  className="cursor-pointer p-4"
                  onClick={() =>
                    setExpandedRoleId(isExpanded ? null : role.id ?? null)
                  }
                >
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-3">
                      <span
                        className="rounded-full px-3 py-1 text-sm font-medium text-white"
                        style={{
                          backgroundColor: role.color ?? DEFAULT_COLOR,
                          border: `2px solid ${role.color ?? DEFAULT_COLOR}`,
                        }}
                      >
                        {role.name}
                      </span>
                      <span className="text-sm text-white/60">
                        {role.userCount ?? 0} users
                      </span>
                      <span className="rounded bg-white/10 px-2 py-0.5 text-xs text-white/60">
                        Priority: {role.priority}
                      </span>
                    </div>
                    <div
                      className="flex items-center gap-2"
                      onClick={(e) => e.stopPropagation()}
                    >
                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button
                            variant="ghost"
                            size="icon-sm"
                            className="text-white/60 hover:text-white"
                          >
                            <MoreHorizontal className="h-4 w-4" />
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent variant="glass">
                          <DropdownMenuItem
                            onClick={() => handleDuplicate(role)}
                            className="text-white"
                          >
                            <Copy className="h-4 w-4 mr-2" />
                            Duplicate
                          </DropdownMenuItem>
                          <DropdownMenuItem
                            onClick={() => {
                              setSelectedRole(role);
                              setEditForm({
                                name: role.name ?? "",
                                description: role.description ?? "",
                                priority: role.priority ?? 100,
                                color: role.color ?? DEFAULT_COLOR,
                              });
                              setEditOpen(true);
                            }}
                            className="text-white"
                          >
                            <Pencil className="h-4 w-4 mr-2" />
                            Edit
                          </DropdownMenuItem>
                          {role.canBeDeleted && (
                            <DropdownMenuItem
                              variant="destructive"
                              onClick={() => {
                                setSelectedRole(role);
                                setDeleteOpen(true);
                              }}
                            >
                              <Trash2 className="h-4 w-4 mr-2" />
                              Delete
                            </DropdownMenuItem>
                          )}
                        </DropdownMenuContent>
                      </DropdownMenu>
                      {isExpanded ? (
                        <ChevronUp className="h-4 w-4 text-white/60" />
                      ) : (
                        <ChevronDown className="h-4 w-4 text-white/60" />
                      )}
                    </div>
                  </div>
                  <p className="mt-2 text-sm text-white/80">{role.description}</p>
                </div>

                {isExpanded && (
                  <div className="border-t border-white/10 bg-white/5 p-4">
                    <div className="mb-3 flex items-center justify-between">
                      <h4 className="flex items-center gap-2 text-sm font-medium text-white">
                        <Shield className="h-4 w-4" />
                        Permissions ({effectivePerms.size})
                      </h4>
                      {dirty && (
                        <Button
                          size="sm"
                          onClick={() => savePermissions(role)}
                          disabled={savingPerms}
                        >
                          {savingPerms ? "Saving..." : "Save Permissions"}
                        </Button>
                      )}
                    </div>
                    <div className="space-y-4">
                      {Array.from(groupedPermissions.entries())
                        .sort(([a], [b]) => a.localeCompare(b))
                        .map(([category, perms]) => (
                          <div key={category}>
                            <div className="mb-2 text-xs font-medium uppercase text-white/50">
                              {category}
                            </div>
                            <div className="grid grid-cols-1 gap-2 sm:grid-cols-2 lg:grid-cols-3">
                              {perms.map((perm) => {
                                const permName = perm.name ?? "";
                                const checked =
                                  effectivePerms.has(permName);
                                return (
                                  <label
                                    key={permName}
                                    className="flex cursor-pointer items-center gap-2 rounded bg-white/5 px-2 py-1.5 text-sm"
                                  >
                                    <Switch
                                      checked={checked}
                                      onCheckedChange={() =>
                                        togglePermissionLocal(role, permName)
                                      }
                                      className="data-[state=checked]:bg-primary"
                                    />
                                    <span className="text-white/90">
                                      {permName}
                                    </span>
                                  </label>
                                );
                              })}
                            </div>
                          </div>
                        ))}
                    </div>
                  </div>
                )}
              </Card>
            );
          })}
        </div>
      )}

      <Dialog open={createOpen} onOpenChange={setCreateOpen}>
        <DialogContent variant="glass">
          <DialogHeader>
            <DialogTitle>Create Role</DialogTitle>
            <DialogDescription>
              Add a new role with name and optional initial permissions.
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div>
              <label className="text-sm font-medium text-white/80">Name</label>
              <Input
                variant="glass"
                value={createForm.name}
                onChange={(e) =>
                  setCreateForm((prev) => ({ ...prev, name: e.target.value }))
                }
                placeholder="Role name"
                className="mt-1 text-white"
              />
            </div>
            <div>
              <label className="text-sm font-medium text-white/80">
                Description
              </label>
              <Input
                variant="glass"
                value={createForm.description}
                onChange={(e) =>
                  setCreateForm((prev) => ({
                    ...prev,
                    description: e.target.value,
                  }))
                }
                placeholder="Role description"
                className="mt-1 text-white"
              />
            </div>
            <div>
              <label className="text-sm font-medium text-white/80">Priority</label>
              <Input
                variant="glass"
                type="number"
                value={createForm.priority}
                onChange={(e) =>
                  setCreateForm((prev) => ({
                    ...prev,
                    priority: parseInt(e.target.value, 10) || 100,
                  }))
                }
                className="mt-1 text-white"
              />
            </div>
            <div>
              <label className="text-sm font-medium text-white/80">Color</label>
              <div className="mt-1 flex items-center gap-2">
                <input
                  type="color"
                  value={createForm.color}
                  onChange={(e) =>
                    setCreateForm((prev) => ({ ...prev, color: e.target.value }))
                  }
                  className="h-10 w-10 cursor-pointer rounded border-2 border-white/20 bg-transparent"
                />
                <Input
                  variant="glass"
                  value={createForm.color}
                  onChange={(e) =>
                    setCreateForm((prev) => ({ ...prev, color: e.target.value }))
                  }
                  className="flex-1 text-white font-mono text-sm"
                />
              </div>
            </div>
          </div>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setCreateOpen(false)}
              className="border-white/20 text-white"
            >
              Cancel
            </Button>
            <Button
              onClick={handleCreate}
              disabled={saving || !createForm.name.trim()}
            >
              {saving ? "Creating..." : "Create"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog open={editOpen} onOpenChange={setEditOpen}>
        <DialogContent variant="glass">
          <DialogHeader>
            <DialogTitle>Edit Role: {selectedRole?.name}</DialogTitle>
            <DialogDescription>Update role details.</DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div>
              <label className="text-sm font-medium text-white/80">Name</label>
              <Input
                variant="glass"
                value={editForm.name}
                onChange={(e) =>
                  setEditForm((prev) => ({ ...prev, name: e.target.value }))
                }
                placeholder="Role name"
                className="mt-1 text-white"
              />
            </div>
            <div>
              <label className="text-sm font-medium text-white/80">
                Description
              </label>
              <Input
                variant="glass"
                value={editForm.description}
                onChange={(e) =>
                  setEditForm((prev) => ({
                    ...prev,
                    description: e.target.value,
                  }))
                }
                placeholder="Role description"
                className="mt-1 text-white"
              />
            </div>
            <div>
              <label className="text-sm font-medium text-white/80">Priority</label>
              <Input
                variant="glass"
                type="number"
                value={editForm.priority}
                onChange={(e) =>
                  setEditForm((prev) => ({
                    ...prev,
                    priority: parseInt(e.target.value, 10) || 100,
                  }))
                }
                className="mt-1 text-white"
              />
            </div>
            <div>
              <label className="text-sm font-medium text-white/80">Color</label>
              <div className="mt-1 flex items-center gap-2">
                <input
                  type="color"
                  value={editForm.color}
                  onChange={(e) =>
                    setEditForm((prev) => ({ ...prev, color: e.target.value }))
                  }
                  className="h-10 w-10 cursor-pointer rounded border-2 border-white/20 bg-transparent"
                />
                <Input
                  variant="glass"
                  value={editForm.color}
                  onChange={(e) =>
                    setEditForm((prev) => ({ ...prev, color: e.target.value }))
                  }
                  className="flex-1 text-white font-mono text-sm"
                />
              </div>
            </div>
          </div>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setEditOpen(false)}
              className="border-white/20 text-white"
            >
              Cancel
            </Button>
            <Button onClick={handleUpdate} disabled={saving}>
              {saving ? "Saving..." : "Save"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog open={deleteOpen} onOpenChange={setDeleteOpen}>
        <DialogContent variant="glass">
          <DialogHeader>
            <DialogTitle>Delete Role</DialogTitle>
            <DialogDescription>
              Are you sure you want to delete the role{" "}
              <strong>{selectedRole?.name}</strong>? This action cannot be undone.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setDeleteOpen(false)}
              className="border-white/20 text-white"
            >
              Cancel
            </Button>
            <Button
              variant="destructive"
              onClick={handleDelete}
              disabled={saving}
            >
              {saving ? "Deleting..." : "Delete"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
