"use client";

import { useState, useEffect, useCallback } from "react";
import { UserMinus, CheckCircle } from "lucide-react";
import { Button } from "@/components/primitives/button";
import { Badge } from "@/components/ui/badge";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { DataTablePagination } from "@/components/primitives/pagination";
import { getBannedUsers, unbanUser } from "@/lib/services/moderationService";
import type { BannedUserDto } from "@/lib/api/client";

function formatDate(date?: Date | string): string {
  if (!date) return "-";
  return new Date(date).toLocaleString();
}

function getBanStatusBadge(user: BannedUserDto) {
  if (!user.banExpiresAt) {
    return <Badge variant="destructive" className="bg-red-500/30 text-red-300">Permanent</Badge>;
  }
  const expiresAt = new Date(user.banExpiresAt);
  const now = new Date();
  const isExpired = expiresAt < now;
  return (
    <Badge
      variant={isExpired ? "secondary" : "destructive"}
      className={
        isExpired
          ? "bg-amber-500/30 text-amber-300"
          : "bg-red-500/30 text-red-300"
      }
    >
      {isExpired ? "Expired" : "Temporary"}
    </Badge>
  );
}

function getRoleVariant(roleName?: string): "default" | "secondary" | "destructive" | "outline" {
  switch (roleName?.toLowerCase()) {
    case "admin":
      return "destructive";
    case "moderator":
      return "secondary";
    default:
      return "outline";
  }
}

export function BannedUsersTab() {
  const [bannedUsers, setBannedUsers] = useState<BannedUserDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [selectedUser, setSelectedUser] = useState<BannedUserDto | null>(null);
  const [unbanning, setUnbanning] = useState(false);

  const fetchBannedUsers = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await getBannedUsers(page, pageSize);
      setBannedUsers(response.users ?? []);
      setTotalCount(response.totalCount ?? 0);
    } catch {
      setError("Failed to fetch banned users");
    } finally {
      setLoading(false);
    }
  }, [page, pageSize]);

  useEffect(() => {
    fetchBannedUsers();
  }, [fetchBannedUsers]);

  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));

  const handleUnban = async () => {
    if (!selectedUser?.id) return;
    try {
      setUnbanning(true);
      await unbanUser(selectedUser.id);
      setSelectedUser(null);
      await fetchBannedUsers();
    } catch {
      setError("Failed to unban user");
    } finally {
      setUnbanning(false);
    }
  };

  if (loading && bannedUsers.length === 0) {
    return (
      <div className="flex items-center justify-center py-16">
        <p className="text-white/60">Loading banned users...</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <UserMinus className="h-6 w-6 text-white/80" />
        <h2 className="text-2xl font-bold text-white">Banned Users Management</h2>
      </div>

      {error && (
        <div className="rounded-lg border border-red-500/30 bg-red-500/10 p-3 text-red-400">
          {error}
        </div>
      )}

      <p className="text-sm text-white/60">
        Total banned users: {totalCount}
      </p>

      <div className="rounded-lg border border-white/10 overflow-hidden">
        <Table>
          <TableHeader>
            <TableRow className="border-white/10 hover:bg-transparent">
              <TableHead className="text-white/70">User</TableHead>
              <TableHead className="text-white/70">Role</TableHead>
              <TableHead className="text-white/70">Ban Status</TableHead>
              <TableHead className="text-white/70">Banned By</TableHead>
              <TableHead className="text-white/70">Banned Date</TableHead>
              <TableHead className="text-white/70">Expires</TableHead>
              <TableHead className="text-white/70">Reason</TableHead>
              <TableHead className="text-white/70">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {bannedUsers.length === 0 && !loading ? (
              <TableRow className="border-white/10">
                <TableCell colSpan={8} className="py-12 text-center">
                  <CheckCircle className="mx-auto mb-4 h-12 w-12 text-white/30" />
                  <p className="text-lg font-medium text-white/80">
                    No banned users found
                  </p>
                  <p className="text-sm text-white/60">
                    All users are currently in good standing
                  </p>
                </TableCell>
              </TableRow>
            ) : (
              bannedUsers.map((user) => (
                <TableRow
                  key={user.id}
                  className="border-white/10 hover:bg-white/5"
                >
                  <TableCell>
                    <div>
                      <p className="font-medium text-white/90">
                        {user.username ?? "-"}
                      </p>
                      <p className="text-xs text-white/60">
                        {user.email ?? "-"}
                      </p>
                    </div>
                  </TableCell>
                  <TableCell>
                    <Badge variant={getRoleVariant(user.roleName)}>
                      {user.roleName ?? "-"}
                    </Badge>
                  </TableCell>
                  <TableCell>{getBanStatusBadge(user)}</TableCell>
                  <TableCell className="text-sm text-white/80">
                    {user.bannedByUsername ?? "System"}
                  </TableCell>
                  <TableCell className="text-sm text-white/80">
                    {formatDate(user.bannedAt)}
                  </TableCell>
                  <TableCell className="text-sm text-white/80">
                    {user.banExpiresAt ? formatDate(user.banExpiresAt) : "Never"}
                  </TableCell>
                  <TableCell className="max-w-[150px]">
                    <span
                      className="block truncate text-sm text-white/80"
                      title={user.banReason ?? "No reason provided"}
                    >
                      {user.banReason ?? "No reason provided"}
                    </span>
                  </TableCell>
                  <TableCell>
                    <Button
                      size="sm"
                      variant="outline"
                      onClick={() => setSelectedUser(user)}
                      className="border-green-500/30 text-green-400 hover:bg-green-500/20"
                    >
                      <CheckCircle className="mr-2 h-4 w-4" />
                      Unban
                    </Button>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>

      {totalPages > 1 && (
        <DataTablePagination
          page={page}
          totalPages={totalPages}
          onPageChange={setPage}
        />
      )}

      <Dialog
        open={!!selectedUser}
        onOpenChange={(open) => !open && setSelectedUser(null)}
      >
        <DialogContent className="border-white/20 bg-slate-900/95 text-white">
          <DialogHeader>
            <DialogTitle>Confirm User Unban</DialogTitle>
            <DialogDescription className="text-white/70">
              This action will immediately restore the user&apos;s access to the platform.
            </DialogDescription>
          </DialogHeader>
          {selectedUser && (
            <div className="space-y-4">
              <p className="text-white/90">
                Are you sure you want to unban the following user?
              </p>
              <div className="rounded-lg border border-white/10 bg-white/5 p-3 text-sm">
                <p>
                  <strong>Username:</strong> {selectedUser.username}
                </p>
                <p className="text-white/70">
                  <strong>Email:</strong> {selectedUser.email}
                </p>
                <p className="text-white/70">
                  <strong>Banned:</strong> {formatDate(selectedUser.bannedAt)}
                </p>
                <p className="text-white/70">
                  <strong>Reason:</strong>{" "}
                  {selectedUser.banReason ?? "No reason provided"}
                </p>
              </div>
            </div>
          )}
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setSelectedUser(null)}
              className="border-white/20 text-white/80"
            >
              Cancel
            </Button>
            <Button
              onClick={handleUnban}
              disabled={unbanning}
              className="bg-green-500/30 text-green-300 hover:bg-green-500/40"
            >
              {unbanning ? "Unbanning..." : "Unban User"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
