"use client";

import { useState, useEffect, useCallback } from "react";
import { CheckCircle, Clock } from "lucide-react";
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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { DataTablePagination } from "@/components/primitives/pagination";
import {
  getAllReports,
  resolveReport,
} from "@/lib/services/moderationService";
import type { Report } from "@/lib/api/client";

interface ReportsTableProps {
  canHandleReports: boolean;
  onResolved?: () => void;
}

function formatDate(date?: Date | string): string {
  if (!date) return "-";
  return new Date(date).toLocaleString();
}

function getReasonVariant(reason?: string): "default" | "secondary" | "destructive" | "outline" {
  switch (reason) {
    case "Inappropriate":
    case "Malware":
      return "destructive";
    case "Spam":
      return "secondary";
    default:
      return "outline";
  }
}

function getTypeVariant(type?: string): "default" | "secondary" | "destructive" | "outline" {
  switch (type) {
    case "Model":
      return "default";
    case "Comment":
      return "secondary";
    default:
      return "outline";
  }
}

export function ReportsTable({
  canHandleReports,
  onResolved,
}: ReportsTableProps) {
  const [reports, setReports] = useState<Report[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [rowsPerPage] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [filter, setFilter] = useState<"all" | "unresolved" | "resolved">("unresolved");
  const [typeFilter, setTypeFilter] = useState<string>("all");
  const [selectedReport, setSelectedReport] = useState<Report | null>(null);
  const [resolution, setResolution] = useState("");
  const [resolving, setResolving] = useState(false);

  const fetchReports = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const isResolved =
        filter === "all" ? undefined : filter === "resolved";
      const type = typeFilter === "all" ? undefined : typeFilter;
      const response = await getAllReports(
        page,
        rowsPerPage,
        isResolved,
        type ?? null
      );
      setReports(response.reports ?? []);
      setTotalCount(response.totalCount ?? 0);
    } catch {
      setError("Failed to fetch reports");
    } finally {
      setLoading(false);
    }
  }, [page, rowsPerPage, filter, typeFilter]);

  useEffect(() => {
    fetchReports();
  }, [fetchReports]);

  useEffect(() => {
    setPage(1);
  }, [filter, typeFilter]);

  const totalPages = Math.max(1, Math.ceil(totalCount / rowsPerPage));

  const handleResolve = async () => {
    if (!selectedReport || !resolution.trim()) return;
    try {
      setResolving(true);
      await resolveReport(selectedReport.id!, resolution);
      setSelectedReport(null);
      setResolution("");
      await fetchReports();
      onResolved?.();
    } catch {
      setError("Failed to resolve report");
    } finally {
      setResolving(false);
    }
  };

  if (loading && reports.length === 0) {
    return (
      <div className="flex items-center justify-center py-16">
        <p className="text-white/60">Loading reports...</p>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {error && (
        <div className="rounded-lg border border-red-500/30 bg-red-500/10 p-3 text-red-400">
          {error}
        </div>
      )}

      <div className="flex flex-wrap items-center gap-2">
        <Button
          variant={filter === "unresolved" ? "default" : "outline"}
          size="sm"
          onClick={() => setFilter("unresolved")}
          className={
            filter !== "unresolved"
              ? "border-white/20 text-white/80 hover:bg-white/10"
              : ""
          }
        >
          <Clock className="mr-2 h-4 w-4" />
          Active Reports
        </Button>
        <Button
          variant={filter === "resolved" ? "default" : "outline"}
          size="sm"
          onClick={() => setFilter("resolved")}
          className={
            filter !== "resolved"
              ? "border-white/20 text-white/80 hover:bg-white/10"
              : ""
          }
        >
          <CheckCircle className="mr-2 h-4 w-4" />
          Resolved Reports
        </Button>
        <Button
          variant={filter === "all" ? "default" : "outline"}
          size="sm"
          onClick={() => setFilter("all")}
          className={
            filter !== "all"
              ? "border-white/20 text-white/80 hover:bg-white/10"
              : ""
          }
        >
          All Reports
        </Button>

        <Select value={typeFilter} onValueChange={setTypeFilter}>
          <SelectTrigger className="ml-2 w-[140px] border-white/20 bg-white/5 text-white">
            <SelectValue placeholder="Type" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Types</SelectItem>
            <SelectItem value="Model">Models</SelectItem>
            <SelectItem value="Comment">Comments</SelectItem>
          </SelectContent>
        </Select>
      </div>

      <div className="rounded-lg border border-white/10 overflow-hidden">
        <Table>
          <TableHeader>
            <TableRow className="border-white/10 hover:bg-transparent">
              <TableHead className="text-white/70">Type</TableHead>
              <TableHead className="text-white/70">Reason</TableHead>
              <TableHead className="text-white/70">Description</TableHead>
              <TableHead className="text-white/70">Status</TableHead>
              <TableHead className="text-white/70">Created</TableHead>
              <TableHead className="text-white/70">Resolved</TableHead>
              <TableHead className="text-white/70">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {reports.map((report) => (
              <TableRow
                key={report.id}
                className="border-white/10 hover:bg-white/5"
              >
                <TableCell>
                  <Badge variant={getTypeVariant(report.type)}>
                    {report.type ?? "-"}
                  </Badge>
                </TableCell>
                <TableCell>
                  <Badge variant={getReasonVariant(report.reason)}>
                    {report.reason ?? "-"}
                  </Badge>
                </TableCell>
                <TableCell className="max-w-[200px]">
                  <span className="truncate block text-sm text-white/90">
                    {(report.description ?? "").length > 100
                      ? `${report.description!.substring(0, 100)}...`
                      : report.description ?? "-"}
                  </span>
                </TableCell>
                <TableCell>
                  <Badge
                    variant={report.isResolved ? "default" : "secondary"}
                    className={
                      report.isResolved
                        ? "bg-green-500/30 text-green-300"
                        : "bg-amber-500/30 text-amber-300"
                    }
                  >
                    {report.isResolved ? "Resolved" : "Active"}
                  </Badge>
                </TableCell>
                <TableCell className="text-sm text-white/80">
                  {formatDate(report.createdAt)}
                </TableCell>
                <TableCell className="text-sm text-white/80">
                  {report.resolvedAt
                    ? formatDate(report.resolvedAt)
                    : "-"}
                </TableCell>
                <TableCell>
                  <div className="flex gap-2">
                    {canHandleReports && !report.isResolved && (
                      <Button
                        size="sm"
                        onClick={() => {
                          setSelectedReport(report);
                          setResolution("");
                        }}
                        className="bg-white/20 text-white hover:bg-white/30"
                      >
                        Resolve
                      </Button>
                    )}
                  </div>
                </TableCell>
              </TableRow>
            ))}
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
        open={!!selectedReport}
        onOpenChange={(open) => !open && setSelectedReport(null)}
      >
        <DialogContent className="border-white/20 bg-slate-900/95 text-white">
          <DialogHeader>
            <DialogTitle>Resolve Report</DialogTitle>
            <DialogDescription className="text-white/70">
              Add resolution notes for this report.
            </DialogDescription>
          </DialogHeader>
          {selectedReport && (
            <div className="space-y-4">
              <div className="rounded-lg border border-white/10 bg-white/5 p-3 text-sm">
                <p className="text-white/80">
                  <strong>Type:</strong> {selectedReport.type} |{" "}
                  <strong>Reason:</strong> {selectedReport.reason}
                </p>
                <p className="mt-2 text-white/70">
                  <strong>Description:</strong> {selectedReport.description}
                </p>
                <p className="mt-2 text-white/60">
                  <strong>Reported:</strong>{" "}
                  {formatDate(selectedReport.createdAt)}
                </p>
              </div>
              <div>
                <label className="mb-2 block text-sm font-medium text-white/80">
                  Resolution Notes
                </label>
                <textarea
                  value={resolution}
                  onChange={(e) => setResolution(e.target.value)}
                  placeholder="Describe the action taken..."
                  rows={4}
                  className="w-full rounded-md border border-white/20 bg-white/5 px-3 py-2 text-white placeholder:text-white/40 focus:border-white/40 focus:outline-none focus:ring-2 focus:ring-white/20"
                />
              </div>
            </div>
          )}
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setSelectedReport(null)}
              className="border-white/20 text-white/80"
            >
              Cancel
            </Button>
            <Button
              onClick={handleResolve}
              disabled={!resolution.trim() || resolving}
              className="bg-white/20 text-white hover:bg-white/30"
            >
              {resolving ? "Resolving..." : "Resolve Report"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
