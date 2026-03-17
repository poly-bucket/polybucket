"use client";

import { useState, useEffect } from "react";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { getModeratorActivity } from "@/lib/services/moderationService";
import type { ModeratorActivity as ModeratorActivityType } from "@/lib/api/client";

interface ModeratorActivityProps {
  dateRange: { from: string; to: string };
}

function formatDuration(hours?: number): string {
  if (hours == null || hours < 0) return "-";
  if (hours < 1) return "< 1 hour";
  if (hours < 24) return `${Math.round(hours)} hours`;
  return `${Math.round(hours / 24)} days`;
}

export function ModeratorActivity({ dateRange }: ModeratorActivityProps) {
  const [activity, setActivity] = useState<ModeratorActivityType[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function fetch() {
      try {
        setLoading(true);
        const fromDate = dateRange.from ? new Date(dateRange.from) : undefined;
        const toDate = dateRange.to ? new Date(dateRange.to) : undefined;
        const data = await getModeratorActivity(fromDate ?? null, toDate ?? null);
        setActivity(data ?? []);
      } catch {
      } finally {
        setLoading(false);
      }
    }
    fetch();
  }, [dateRange.from, dateRange.to]);

  if (loading) {
    return (
      <div className="flex items-center justify-center py-16">
        <p className="text-white/60">Loading moderator activity...</p>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <h3 className="text-lg font-semibold text-white">Moderator Activity</h3>
      <div className="rounded-lg border border-white/10 overflow-hidden">
        <Table>
          <TableHeader>
            <TableRow className="border-white/10 hover:bg-transparent">
              <TableHead className="text-white/70">Moderator</TableHead>
              <TableHead className="text-white/70">Resolved</TableHead>
              <TableHead className="text-white/70">Dismissed</TableHead>
              <TableHead className="text-white/70">Archived</TableHead>
              <TableHead className="text-white/70">Avg Resolution</TableHead>
              <TableHead className="text-white/70">Last Activity</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {activity.length === 0 ? (
              <TableRow className="border-white/10">
                <TableCell
                  colSpan={6}
                  className="py-8 text-center text-white/60"
                >
                  No moderator activity in this period.
                </TableCell>
              </TableRow>
            ) : (
              activity.map((m) => (
                <TableRow
                  key={m.moderatorId}
                  className="border-white/10 hover:bg-white/5"
                >
                  <TableCell className="text-white/90">
                    {m.moderatorName ?? m.moderatorId ?? "-"}
                  </TableCell>
                  <TableCell className="text-white/80">
                    {m.reportsResolved ?? 0}
                  </TableCell>
                  <TableCell className="text-white/80">
                    {m.reportsDismissed ?? 0}
                  </TableCell>
                  <TableCell className="text-white/80">
                    {m.reportsArchived ?? 0}
                  </TableCell>
                  <TableCell className="text-white/80">
                    {formatDuration(m.averageResolutionTime)}
                  </TableCell>
                  <TableCell className="text-white/80">
                    {m.lastActivity
                      ? new Date(m.lastActivity).toLocaleString()
                      : "-"}
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>
    </div>
  );
}
