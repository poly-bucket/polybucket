"use client";

import { useState, useEffect, useCallback } from "react";
import { Flag, BarChart3, FileText, Users, Activity } from "lucide-react";
import { Card, CardContent } from "@/components/primitives/card";
import { Button } from "@/components/primitives/button";
import { Input } from "@/components/primitives/input";
import { getReportsAnalytics } from "@/lib/services/moderationService";
import type { ReportsAnalytics as ReportsAnalyticsType } from "@/lib/api/client";
import { ReportsAnalytics } from "./reports/reports-analytics";
import { ReportsTable } from "./reports/reports-table";
import { TopReported } from "./reports/top-reported";
import { ModeratorActivity } from "./reports/moderator-activity";
import { cn } from "@/lib/utils";

const SUB_TABS = [
  { id: "analytics", label: "Analytics", icon: BarChart3 },
  { id: "reports", label: "Reports", icon: Flag },
  { id: "top-reported", label: "Top Reported", icon: Users },
  { id: "moderator-activity", label: "Moderator Activity", icon: Activity },
] as const;

type SubTabId = (typeof SUB_TABS)[number]["id"];

export function ReportsDashboard({ canHandleReports = true }: { canHandleReports?: boolean }) {
  const [activeTab, setActiveTab] = useState<SubTabId>("reports");
  const [analytics, setAnalytics] = useState<ReportsAnalyticsType | null>(null);
  const [analyticsLoading, setAnalyticsLoading] = useState(false);
  const [dateRange, setDateRange] = useState({
    from: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().split("T")[0],
    to: new Date().toISOString().split("T")[0],
  });
  const [refreshKey, setRefreshKey] = useState(0);

  const fetchAnalytics = useCallback(async () => {
    try {
      setAnalyticsLoading(true);
      const fromDate = dateRange.from ? new Date(dateRange.from) : undefined;
      const toDate = dateRange.to ? new Date(dateRange.to) : undefined;
      const data = await getReportsAnalytics(fromDate ?? null, toDate ?? null);
      setAnalytics(data);
    } catch {
    } finally {
      setAnalyticsLoading(false);
    }
  }, [dateRange.from, dateRange.to]);

  useEffect(() => {
    fetchAnalytics();
  }, [fetchAnalytics]);

  const handleRefresh = () => {
    setRefreshKey((k) => k + 1);
    if (activeTab === "analytics") {
      fetchAnalytics();
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <Flag className="h-6 w-6 text-white/80" />
        <h2 className="text-2xl font-bold text-white">Reports Dashboard</h2>
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <Card variant="glass" className="border-white/20">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-white/60">Total Reports</p>
                <p className="text-2xl font-semibold text-white">
                  {analyticsLoading
                    ? "..."
                    : analytics?.totalReports ?? 0}
                </p>
              </div>
              <Flag className="h-8 w-8 text-blue-400/80" />
            </div>
          </CardContent>
        </Card>
        <Card variant="glass" className="border-white/20">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-white/60">Active Reports</p>
                <p className="text-2xl font-semibold text-amber-400">
                  {analyticsLoading
                    ? "..."
                    : analytics?.activeReports ?? 0}
                </p>
              </div>
              <FileText className="h-8 w-8 text-amber-400/80" />
            </div>
          </CardContent>
        </Card>
        <Card variant="glass" className="border-white/20">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-white/60">Resolved</p>
                <p className="text-2xl font-semibold text-green-400">
                  {analyticsLoading
                    ? "..."
                    : analytics?.resolvedReports ?? 0}
                </p>
              </div>
              <Activity className="h-8 w-8 text-green-400/80" />
            </div>
          </CardContent>
        </Card>
        <Card variant="glass" className="border-white/20">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-white/60">Dismissed</p>
                <p className="text-2xl font-semibold text-white">
                  {analyticsLoading
                    ? "..."
                    : analytics?.dismissedReports ?? 0}
                </p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      <div className="flex flex-wrap items-center gap-2 rounded-lg border border-white/10 bg-white/5 p-3">
        <span className="text-sm font-medium text-white/70">Date Range:</span>
        <Input
          type="date"
          value={dateRange.from}
          onChange={(e) =>
            setDateRange((prev) => ({ ...prev, from: e.target.value }))
          }
          className="h-8 w-40 border-white/20 bg-white/5 text-white"
        />
        <span className="text-white/60">to</span>
        <Input
          type="date"
          value={dateRange.to}
          onChange={(e) =>
            setDateRange((prev) => ({ ...prev, to: e.target.value }))
          }
          className="h-8 w-40 border-white/20 bg-white/5 text-white"
        />
        <Button
          variant="outline"
          size="sm"
          onClick={handleRefresh}
          disabled={analyticsLoading}
          className="border-white/20 text-white/80 hover:bg-white/10 hover:text-white"
        >
          Update
        </Button>
      </div>

      <div className="flex gap-2 border-b border-white/10 pb-2">
        {SUB_TABS.map((tab) => {
          const Icon = tab.icon;
          return (
            <button
              key={tab.id}
              type="button"
              onClick={() => setActiveTab(tab.id)}
              className={cn(
                "flex items-center gap-2 rounded-md px-4 py-2 text-sm font-medium transition-colors",
                activeTab === tab.id
                  ? "bg-white/20 text-white"
                  : "text-white/70 hover:bg-white/10 hover:text-white"
              )}
            >
              <Icon className="h-4 w-4" />
              {tab.label}
            </button>
          );
        })}
      </div>

      {activeTab === "analytics" && (
        <ReportsAnalytics
          analytics={analytics}
          loading={analyticsLoading}
          dateRange={dateRange}
          onRefresh={fetchAnalytics}
        />
      )}
      {activeTab === "reports" && (
        <ReportsTable
          key={refreshKey}
          canHandleReports={canHandleReports}
          onResolved={handleRefresh}
        />
      )}
      {activeTab === "top-reported" && (
        <TopReported dateRange={dateRange} key={refreshKey} />
      )}
      {activeTab === "moderator-activity" && (
        <ModeratorActivity dateRange={dateRange} key={refreshKey} />
      )}
    </div>
  );
}
