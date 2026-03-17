"use client";

import { useState, useEffect } from "react";
import { Package, User, MessageSquare } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/primitives/card";
import {
  getTopReportedModels,
  getTopReportedUsers,
  getTopReportedComments,
} from "@/lib/services/moderationService";
import type { TopReportedItem } from "@/lib/api/client";

interface TopReportedProps {
  dateRange: { from: string; to: string };
}

export function TopReported({ dateRange }: TopReportedProps) {
  const [models, setModels] = useState<TopReportedItem[]>([]);
  const [users, setUsers] = useState<TopReportedItem[]>([]);
  const [comments, setComments] = useState<TopReportedItem[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function fetch() {
      try {
        setLoading(true);
        const [modelsRes, usersRes, commentsRes] = await Promise.all([
          getTopReportedModels(10),
          getTopReportedUsers(10),
          getTopReportedComments(10),
        ]);
        setModels(modelsRes ?? []);
        setUsers(usersRes ?? []);
        setComments(commentsRes ?? []);
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
        <p className="text-white/60">Loading top reported items...</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <h3 className="text-lg font-semibold text-white">Top Reported Items</h3>
      <div className="grid gap-6 md:grid-cols-3">
        <Card variant="glass" className="border-white/20">
          <CardHeader className="pb-2">
            <CardTitle className="flex items-center gap-2 text-base text-white">
              <Package className="h-4 w-4" />
              Models
            </CardTitle>
          </CardHeader>
          <CardContent>
            <ul className="space-y-2">
              {models.length === 0 ? (
                <li className="text-sm text-white/60">No data</li>
              ) : (
                models.map((item) => (
                  <li
                    key={item.id ?? item.name}
                    className="flex justify-between text-sm"
                  >
                    <span className="truncate text-white/90">{item.name}</span>
                    <span className="ml-2 shrink-0 text-white/60">
                      {item.reportCount ?? 0} reports
                    </span>
                  </li>
                ))
              )}
            </ul>
          </CardContent>
        </Card>
        <Card variant="glass" className="border-white/20">
          <CardHeader className="pb-2">
            <CardTitle className="flex items-center gap-2 text-base text-white">
              <User className="h-4 w-4" />
              Users
            </CardTitle>
          </CardHeader>
          <CardContent>
            <ul className="space-y-2">
              {users.length === 0 ? (
                <li className="text-sm text-white/60">No data</li>
              ) : (
                users.map((item) => (
                  <li
                    key={item.id ?? item.name}
                    className="flex justify-between text-sm"
                  >
                    <span className="truncate text-white/90">{item.name}</span>
                    <span className="ml-2 shrink-0 text-white/60">
                      {item.reportCount ?? 0} reports
                    </span>
                  </li>
                ))
              )}
            </ul>
          </CardContent>
        </Card>
        <Card variant="glass" className="border-white/20">
          <CardHeader className="pb-2">
            <CardTitle className="flex items-center gap-2 text-base text-white">
              <MessageSquare className="h-4 w-4" />
              Comments
            </CardTitle>
          </CardHeader>
          <CardContent>
            <ul className="space-y-2">
              {comments.length === 0 ? (
                <li className="text-sm text-white/60">No data</li>
              ) : (
                comments.map((item) => (
                  <li
                    key={item.id ?? item.name}
                    className="flex justify-between text-sm"
                  >
                    <span className="truncate text-white/90">
                      {(item.name ?? "").slice(0, 30)}
                      {(item.name?.length ?? 0) > 30 ? "..." : ""}
                    </span>
                    <span className="ml-2 shrink-0 text-white/60">
                      {item.reportCount ?? 0} reports
                    </span>
                  </li>
                ))
              )}
            </ul>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
