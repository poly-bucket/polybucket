"use client";

import type { ReportsAnalytics as ReportsAnalyticsType } from "@/lib/api/client";

interface ReportsAnalyticsProps {
  analytics: ReportsAnalyticsType | null;
  loading: boolean;
  dateRange: { from: string; to: string };
  onRefresh: () => void;
}

export function ReportsAnalytics({
  analytics,
  loading,
}: ReportsAnalyticsProps) {
  if (loading) {
    return (
      <div className="flex items-center justify-center py-16">
        <p className="text-white/60">Loading analytics...</p>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <h3 className="text-lg font-semibold text-white">
        Reports Analytics Overview
      </h3>
      <p className="text-sm text-white/60">
        View trends, reason statistics, and type distribution below.
      </p>
      <div className="rounded-lg border border-white/10 bg-white/5 p-6">
        <p className="text-center text-white/60">
          {analytics?.dailyTrends?.length
            ? "Analytics charts and detailed breakdowns can be extended here."
            : "Analytics data is available. Additional charts and breakdowns can be added."}
        </p>
      </div>
    </div>
  );
}
