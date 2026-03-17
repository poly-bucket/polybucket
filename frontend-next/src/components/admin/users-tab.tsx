"use client";

import { useState, useEffect, useCallback } from "react";
import {
  RefreshCw,
  Search,
  Ban,
  ShieldCheck,
  Plus,
  Eye,
  EyeOff,
  Copy,
} from "lucide-react";
import { Card, CardContent } from "@/components/primitives/card";
import { Button } from "@/components/primitives/button";
import { Input } from "@/components/primitives/input";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
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
import { DataTablePagination } from "@/components/primitives/pagination";
import {
  getUsers,
  banUser,
  unbanUser,
  createUser,
  getAllRolesUnpaginated,
} from "@/lib/services/adminService";
import { useDebouncedValue } from "@/lib/hooks/use-debounced-value";
import { formatDate } from "@/lib/utils/format";
import { toast } from "sonner";
import type { UserDto, RoleDto } from "@/lib/api/client";
import { BanUserRequest, CreateUserCommand } from "@/lib/api/client";

export function UsersTab() {
  const [users, setUsers] = useState<UserDto[]>([]);
  const [roles, setRoles] = useState<RoleDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchQuery, setSearchQuery] = useState("");
  const debouncedSearch = useDebouncedValue(searchQuery, 500);
  const [pagination, setPagination] = useState({
    page: 1,
    pageSize: 20,
    totalCount: 0,
    totalPages: 1,
  });
  const [filters, setFilters] = useState({
    roleFilter: "all",
    statusFilter: "all",
    sortBy: "CreatedAt",
    sortDescending: true,
  });
  const [banDialog, setBanDialog] = useState<{
    open: boolean;
    user: UserDto | null;
    isBan: boolean;
    reason: string;
  }>({ open: false, user: null, isBan: true, reason: "" });
  const [createDialogOpen, setCreateDialogOpen] = useState(false);
  const [createForm, setCreateForm] = useState({
    email: "",
    username: "",
    roleId: "",
    firstName: "",
    lastName: "",
  });
  const [createResult, setCreateResult] = useState<{
    password?: string;
    userId?: string;
  } | null>(null);
  const [actionLoading, setActionLoading] = useState(false);
  const [createLoading, setCreateLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);

  const fetchUsers = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await getUsers(
        pagination.page,
        pagination.pageSize,
        debouncedSearch || undefined,
        filters.roleFilter === "all" ? undefined : filters.roleFilter,
        filters.statusFilter === "all" ? undefined : filters.statusFilter,
        filters.sortBy,
        filters.sortDescending
      );
      setUsers(response.users ?? []);
      setPagination((prev) => ({
        ...prev,
        totalCount: response.totalCount ?? 0,
        totalPages: response.totalPages ?? 1,
      }));
    } catch {
      setError("Failed to fetch users");
      toast.error("Failed to fetch users");
    } finally {
      setLoading(false);
    }
  }, [
    pagination.page,
    pagination.pageSize,
    debouncedSearch,
    filters.roleFilter,
    filters.statusFilter,
    filters.sortBy,
    filters.sortDescending,
  ]);

  const fetchRoles = useCallback(async () => {
    try {
      const allRoles = await getAllRolesUnpaginated();
      setRoles(allRoles ?? []);
    } catch {
      /* non-critical */
    }
  }, []);

  useEffect(() => {
    fetchUsers();
  }, [fetchUsers]);

  useEffect(() => {
    fetchRoles();
  }, [fetchRoles]);

  useEffect(() => {
    setPagination((prev) => ({ ...prev, page: 1 }));
  }, [debouncedSearch]);

  const handleBanUnban = async () => {
    if (!banDialog.user?.id) return;
    setActionLoading(true);
    try {
      if (banDialog.isBan) {
        const request = new BanUserRequest({ reason: banDialog.reason });
        await banUser(banDialog.user.id, request);
        toast.success("User banned");
      } else {
        await unbanUser(banDialog.user.id);
        toast.success("User unbanned");
      }
      setBanDialog({ open: false, user: null, isBan: true, reason: "" });
      await fetchUsers();
    } catch {
      toast.error("Failed to update user");
    } finally {
      setActionLoading(false);
    }
  };

  const handleCreateUser = async () => {
    if (!createForm.email.trim() || !createForm.username.trim() || !createForm.roleId)
      return;
    setCreateLoading(true);
    setCreateResult(null);
    try {
      const command = new CreateUserCommand({
        email: createForm.email.trim(),
        username: createForm.username.trim(),
        roleId: createForm.roleId,
        firstName: createForm.firstName.trim() || undefined,
        lastName: createForm.lastName.trim() || undefined,
      });
      const response = await createUser(command);
      setCreateResult({
        password: response.generatedPassword,
        userId: response.userId,
      });
      toast.success("User created");
      await fetchUsers();
    } catch {
      toast.error("Failed to create user");
    } finally {
      setCreateLoading(false);
    }
  };

  const closeCreateDialog = () => {
    setCreateDialogOpen(false);
    setCreateForm({
      email: "",
      username: "",
      roleId: "",
      firstName: "",
      lastName: "",
    });
    setCreateResult(null);
    setShowPassword(false);
  };

  const copyPassword = () => {
    if (createResult?.password) {
      navigator.clipboard.writeText(createResult.password);
      toast.success("Password copied to clipboard");
    }
  };

  const openBanDialog = (user: UserDto, isBan: boolean) => {
    setBanDialog({ open: true, user, isBan, reason: "" });
  };

  const roleFilterValue =
    filters.roleFilter === "all" ? "all" : filters.roleFilter;
  const statusFilterValue =
    filters.statusFilter === "all" ? "all" : filters.statusFilter;

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h2 className="text-2xl font-bold text-white">User Management</h2>
        <div className="flex gap-2">
          <Button
            variant="glass"
            onClick={() => setCreateDialogOpen(true)}
            className="text-white/70 hover:text-white border-white/20"
          >
            <Plus className="h-4 w-4 mr-2" />
            Create User
          </Button>
          <Button
            variant="outline"
            onClick={() => fetchUsers()}
            disabled={loading}
            className="text-white/70 hover:text-white border-white/20"
          >
            <RefreshCw className="h-4 w-4 mr-2" />
            {loading ? "Loading..." : "Refresh"}
          </Button>
        </div>
      </div>

      <Card variant="glass" className="border-white/20">
        <CardContent className="pt-6">
          <div className="flex flex-col sm:flex-row gap-4">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-white/40" />
              <Input
                variant="glass"
                placeholder="Search users by username, email..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-10 text-white placeholder:text-white/50"
              />
            </div>
            <Select
              value={roleFilterValue}
              onValueChange={(v) =>
                setFilters((prev) => ({ ...prev, roleFilter: v }))
              }
            >
              <SelectTrigger variant="glass" className="w-full sm:w-40 text-white">
                <SelectValue placeholder="All Roles" />
              </SelectTrigger>
              <SelectContent variant="glass">
                <SelectItem value="all">All Roles</SelectItem>
                {roles.map((role) => (
                  <SelectItem key={role.id} value={role.name ?? ""}>
                    {role.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Select
              value={statusFilterValue}
              onValueChange={(v) =>
                setFilters((prev) => ({ ...prev, statusFilter: v }))
              }
            >
              <SelectTrigger variant="glass" className="w-full sm:w-40 text-white">
                <SelectValue placeholder="All Status" />
              </SelectTrigger>
              <SelectContent variant="glass">
                <SelectItem value="all">All Status</SelectItem>
                <SelectItem value="Active">Active</SelectItem>
                <SelectItem value="Banned">Banned</SelectItem>
                <SelectItem value="Inactive">Inactive</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>

      {loading && (
        <Card variant="glass" className="border-white/20">
          <CardContent className="py-12 text-center text-white/60">
            Loading users...
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
                onClick={() => fetchUsers()}
                className="mt-2"
              >
                Retry
              </Button>
            </div>
          </CardContent>
        </Card>
      )}

      {!loading && !error && (
        <Card variant="glass" className="border-white/20 overflow-hidden">
          <div className="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow className="border-white/10 hover:bg-transparent">
                  <TableHead className="text-white/70">User</TableHead>
                  <TableHead className="text-white/70">Role</TableHead>
                  <TableHead className="text-white/70">Status</TableHead>
                  <TableHead className="text-white/70">Last Login</TableHead>
                  <TableHead className="text-white/70">Created</TableHead>
                  <TableHead className="text-white/70">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {users.length === 0 ? (
                  <TableRow className="border-white/10">
                    <TableCell
                      colSpan={6}
                      className="py-12 text-center text-white/60"
                    >
                      No users found
                    </TableCell>
                  </TableRow>
                ) : (
                  users.map((user) => (
                    <TableRow
                      key={user.id}
                      className="border-white/10 hover:bg-white/5"
                    >
                      <TableCell>
                        <div className="flex items-center gap-3">
                          <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary/30 text-sm font-medium text-white">
                            {user.username?.charAt(0)?.toUpperCase() ?? "?"}
                          </div>
                          <div>
                            <div className="font-medium text-white">
                              {user.username ?? "Unknown"}
                            </div>
                            <div className="text-sm text-white/60">
                              {user.email ?? "No email"}
                            </div>
                          </div>
                        </div>
                      </TableCell>
                      <TableCell>
                        <Badge
                          variant={
                            user.roleName === "Admin" ? "destructive" : "secondary"
                          }
                          className="bg-white/10 text-white border-white/20"
                        >
                          {user.roleName ?? "No Role"}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <Badge
                          variant={user.isBanned ? "destructive" : "default"}
                          className={
                            user.isBanned
                              ? "bg-red-500/30"
                              : "bg-green-500/30 text-white border-white/20"
                          }
                        >
                          {user.isBanned ? "Banned" : "Active"}
                        </Badge>
                      </TableCell>
                      <TableCell className="text-white/60">
                        {formatDate(user.lastLoginAt)}
                      </TableCell>
                      <TableCell className="text-white/60">
                        {formatDate(user.createdAt)}
                      </TableCell>
                      <TableCell>
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => openBanDialog(user, !user.isBanned)}
                          className="text-white/70 hover:text-white border-white/20"
                        >
                          {user.isBanned ? (
                            <>
                              <ShieldCheck className="h-4 w-4 mr-1" />
                              Unban
                            </>
                          ) : (
                            <>
                              <Ban className="h-4 w-4 mr-1" />
                              Ban
                            </>
                          )}
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </div>
          {pagination.totalPages > 1 && (
            <div className="border-t border-white/10 px-4 py-3 bg-white/5">
              <div className="flex items-center justify-between">
                <div className="text-sm text-white/60">
                  Showing{" "}
                  {(pagination.page - 1) * pagination.pageSize + 1} to{" "}
                  {Math.min(
                    pagination.page * pagination.pageSize,
                    pagination.totalCount
                  )}{" "}
                  of {pagination.totalCount} users
                </div>
                <DataTablePagination
                  page={pagination.page}
                  totalPages={pagination.totalPages}
                  onPageChange={(p) =>
                    setPagination((prev) => ({ ...prev, page: p }))
                  }
                />
              </div>
            </div>
          )}
        </Card>
      )}

      <Dialog
        open={banDialog.open}
        onOpenChange={(open) =>
          !open &&
          setBanDialog({ open: false, user: null, isBan: true, reason: "" })
        }
      >
        <DialogContent variant="glass">
          <DialogHeader>
            <DialogTitle>
              {banDialog.isBan ? "Ban User" : "Unban User"}
            </DialogTitle>
            <DialogDescription>
              {banDialog.isBan ? (
                <>
                  You are about to ban{" "}
                  <strong>{banDialog.user?.username}</strong>. Please provide a
                  reason.
                </>
              ) : (
                <>
                  You are about to unban{" "}
                  <strong>{banDialog.user?.username}</strong>.
                </>
              )}
            </DialogDescription>
          </DialogHeader>
          {banDialog.isBan && (
            <div className="space-y-2">
              <label className="text-sm font-medium text-white/80">
                Reason (required)
              </label>
              <Input
                variant="glass"
                value={banDialog.reason}
                onChange={(e) =>
                  setBanDialog((prev) => ({ ...prev, reason: e.target.value }))
                }
                placeholder="Enter ban reason"
                className="text-white"
              />
            </div>
          )}
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() =>
                setBanDialog({
                  open: false,
                  user: null,
                  isBan: true,
                  reason: "",
                })
              }
              className="border-white/20 text-white"
            >
              Cancel
            </Button>
            <Button
              onClick={handleBanUnban}
              disabled={
                actionLoading ||
                (banDialog.isBan && !banDialog.reason.trim())
              }
            >
              {actionLoading
                ? "Processing..."
                : banDialog.isBan
                  ? "Ban User"
                  : "Unban User"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog open={createDialogOpen} onOpenChange={(open) => !open && closeCreateDialog()}>
        <DialogContent variant="glass">
          <DialogHeader>
            <DialogTitle>Create User</DialogTitle>
            <DialogDescription>
              {createResult
                ? "User created. Copy the generated password and share it securely."
                : "Create a new user. A password will be generated."}
            </DialogDescription>
          </DialogHeader>
          {createResult ? (
            <div className="space-y-4">
              <div className="flex items-center gap-2">
                <Input
                  variant="glass"
                  type={showPassword ? "text" : "password"}
                  value={createResult.password ?? ""}
                  readOnly
                  className="flex-1 text-white font-mono"
                />
                <Button
                  variant="outline"
                  size="icon"
                  onClick={() => setShowPassword(!showPassword)}
                  className="border-white/20 text-white"
                >
                  {showPassword ? (
                    <EyeOff className="h-4 w-4" />
                  ) : (
                    <Eye className="h-4 w-4" />
                  )}
                </Button>
                <Button
                  variant="outline"
                  size="icon"
                  onClick={copyPassword}
                  className="border-white/20 text-white"
                >
                  <Copy className="h-4 w-4" />
                </Button>
              </div>
              <DialogFooter>
                <Button variant="glass" onClick={closeCreateDialog}>
                  Done
                </Button>
              </DialogFooter>
            </div>
          ) : (
            <>
              <div className="space-y-4">
                <div>
                  <label className="text-sm font-medium text-white/80">
                    Email (required)
                  </label>
                  <Input
                    variant="glass"
                    type="email"
                    value={createForm.email}
                    onChange={(e) =>
                      setCreateForm((prev) => ({ ...prev, email: e.target.value }))
                    }
                    placeholder="user@example.com"
                    className="mt-1 text-white"
                  />
                </div>
                <div>
                  <label className="text-sm font-medium text-white/80">
                    Username (required)
                  </label>
                  <Input
                    variant="glass"
                    value={createForm.username}
                    onChange={(e) =>
                      setCreateForm((prev) => ({ ...prev, username: e.target.value }))
                    }
                    placeholder="username"
                    className="mt-1 text-white"
                  />
                </div>
                <div>
                  <label className="text-sm font-medium text-white/80">
                    Role (required)
                  </label>
                  <Select
                    value={createForm.roleId}
                    onValueChange={(v) =>
                      setCreateForm((prev) => ({ ...prev, roleId: v }))
                    }
                  >
                    <SelectTrigger variant="glass" className="mt-1 w-full text-white">
                      <SelectValue placeholder="Select role" />
                    </SelectTrigger>
                    <SelectContent variant="glass">
                      {roles.map((role) => (
                        <SelectItem key={role.id} value={role.id ?? ""}>
                          {role.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="text-sm font-medium text-white/80">
                      First name
                    </label>
                    <Input
                      variant="glass"
                      value={createForm.firstName}
                      onChange={(e) =>
                        setCreateForm((prev) => ({
                          ...prev,
                          firstName: e.target.value,
                        }))
                      }
                      placeholder="Optional"
                      className="mt-1 text-white"
                    />
                  </div>
                  <div>
                    <label className="text-sm font-medium text-white/80">
                      Last name
                    </label>
                    <Input
                      variant="glass"
                      value={createForm.lastName}
                      onChange={(e) =>
                        setCreateForm((prev) => ({
                          ...prev,
                          lastName: e.target.value,
                        }))
                      }
                      placeholder="Optional"
                      className="mt-1 text-white"
                    />
                  </div>
                </div>
              </div>
              <DialogFooter>
                <Button variant="outline" onClick={closeCreateDialog} className="border-white/20 text-white">
                  Cancel
                </Button>
                <Button
                  onClick={handleCreateUser}
                  disabled={
                    createLoading ||
                    !createForm.email.trim() ||
                    !createForm.username.trim() ||
                    !createForm.roleId
                  }
                >
                  {createLoading ? "Creating..." : "Create User"}
                </Button>
              </DialogFooter>
            </>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
}
