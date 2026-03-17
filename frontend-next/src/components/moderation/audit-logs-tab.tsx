"use client";

import { useState, useEffect, useCallback } from "react";
import { Clock, User, FileText } from "lucide-react";
import { Input } from "@/components/primitives/input";
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
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { DataTablePagination } from "@/components/primitives/pagination";
import { useDebouncedValue } from "@/lib/hooks/use-debounced-value";
import { getModerationAuditLogs } from "@/lib/services/moderationService";
import type { ModerationAuditDto } from "@/lib/api/client";

function formatDate(date?: Date | string): string {
  if (!date) return "-";
  return new Date(date).toLocaleString();
}

function formatAction(action?: string): string {
  if (!action) return "-";
  return action.replace(/([A-Z])/g, " $1").replace(/^./, (str) => str.toUpperCase());
}

function getActionVariant(
  action?: string
): "default" | "secondary" | "destructive" | "outline" {
  switch (action?.toLowerCase()) {
    case "edit":
      return "default";
    case "approvewithchanges":
      return "secondary";
    case "flagforreview":
      return "destructive";
    default:
      return "outline";
  }
}

export function AuditLogsTab() {
  const [logs, setLogs] = useState<ModerationAuditDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [actionFilter, setActionFilter] = useState<string>("all");
  const [userFilter, setUserFilter] = useState("");
  const [modelFilter, setModelFilter] = useState("");

  const debouncedUserFilter = useDebouncedValue(userFilter, 500);
  const debouncedModelFilter = useDebouncedValue(modelFilter, 500);

  const fetchLogs = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const action = actionFilter === "all" ? undefined : actionFilter;
      const userId = debouncedUserFilter.trim() || undefined;
      const modelId = debouncedModelFilter.trim() || undefined;
      const response = await getModerationAuditLogs(
        page,
        pageSize,
        action ?? null,
        userId ?? null,
        modelId ?? null
      );
      setLogs(response.logs ?? []);
      setTotalCount(response.totalCount ?? 0);
    } catch {
      setError("Failed to fetch audit logs");
    } finally {
      setLoading(false);
    }
  }, [
    page,
    pageSize,
    actionFilter,
    debouncedUserFilter,
    debouncedModelFilter,
  ]);

  useEffect(() => {
    fetchLogs();
  }, [fetchLogs]);

  useEffect(() => {
    setPage(1);
  }, [actionFilter, debouncedUserFilter, debouncedModelFilter]);

  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));

  if (loading && logs.length === 0) {
    return (
      <div className="flex items-center justify-center py-16">
        <p className="text-white/60">Loading audit logs...</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <Clock className="h-6 w-6 text-white/80" />
        <h2 className="text-2xl font-bold text-white">Moderation Audit Logs</h2>
      </div>

      {error && (
        <div className="rounded-lg border border-red-500/30 bg-red-500/10 p-3 text-red-400">
          {error}
        </div>
      )}

      <div className="flex flex-wrap items-center gap-3">
        <Select value={actionFilter} onValueChange={setActionFilter}>
          <SelectTrigger className="w-[180px] border-white/20 bg-white/5 text-white">
            <SelectValue placeholder="Action" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Actions</SelectItem>
            <SelectItem value="Edit">Edit</SelectItem>
            <SelectItem value="ApproveWithChanges">Approve with Changes</SelectItem>
            <SelectItem value="FlagForReview">Flag for Review</SelectItem>
            <SelectItem value="FeatureModel">Feature Model</SelectItem>
            <SelectItem value="UnfeatureModel">Unfeature Model</SelectItem>
          </SelectContent>
        </Select>

        <Input
          placeholder="Filter by user ID..."
          value={userFilter}
          onChange={(e) => setUserFilter(e.target.value)}
          className="w-48 border-white/20 bg-white/5 text-white placeholder:text-white/40"
        />

        <Input
          placeholder="Filter by model ID..."
          value={modelFilter}
          onChange={(e) => setModelFilter(e.target.value)}
          className="w-48 border-white/20 bg-white/5 text-white placeholder:text-white/40"
        />
      </div>

      <p className="text-sm text-white/60">
        Total audit logs: {totalCount}
      </p>

      <div className="rounded-lg border border-white/10 overflow-hidden">
        <Table>
          <TableHeader>
            <TableRow className="border-white/10 hover:bg-transparent">
              <TableHead className="text-white/70">Date</TableHead>
              <TableHead className="text-white/70">User</TableHead>
              <TableHead className="text-white/70">Action</TableHead>
              <TableHead className="text-white/70">Model ID</TableHead>
              <TableHead className="text-white/70">Notes</TableHead>
              <TableHead className="text-white/70">IP Address</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {logs.length === 0 && !loading ? (
              <TableRow className="border-white/10">
                <TableCell colSpan={6} className="py-12 text-center">
                  <FileText className="mx-auto mb-4 h-12 w-12 text-white/30" />
                  <p className="text-lg font-medium text-white/80">
                    No audit logs found
                  </p>
                  <p className="text-sm text-white/60">
                    No moderation actions have been logged yet
                  </p>
                </TableCell>
              </TableRow>
            ) : (
              logs.map((log) => (
                <TableRow
                  key={log.id}
                  className="border-white/10 hover:bg-white/5"
                >
                  <TableCell className="text-sm text-white/80">
                    {formatDate(log.performedAt)}
                  </TableCell>
                  <TableCell>
                    <div className="flex items-center gap-2">
                      <User className="h-4 w-4 text-white/60" />
                      <span className="text-white/90">
                        {log.performedByUser?.username ?? log.performedByUserId ?? "-"}
                      </span>
                    </div>
                  </TableCell>
                  <TableCell>
                    <Badge variant={getActionVariant(log.action)}>
                      {formatAction(log.action)}
                    </Badge>
                  </TableCell>
                  <TableCell className="font-mono text-sm text-white/80">
                    {log.modelId ?? "-"}
                  </TableCell>
                  <TableCell className="max-w-[200px]">
                    <span className="block truncate text-sm text-white/80">
                      {log.moderationNotes
                        ? log.moderationNotes.length > 100
                          ? `${log.moderationNotes.substring(0, 100)}...`
                          : log.moderationNotes
                        : "-"}
                    </span>
                  </TableCell>
                  <TableCell className="font-mono text-sm text-white/80">
                    {log.ipAddress ?? "-"}
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
    </div>
  );
}
