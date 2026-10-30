"use client";

import { useEffect, useMemo, useState } from "react";
import {
  CartesianGrid,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { Button } from "@/components/primitives/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/primitives/card";
import {
  fetchFollowerHistory,
  type FollowerHistoryPoint,
} from "@/lib/services/userFollowService";
import { ChartBlockSkeleton } from "@/components/ui/skeletons";

type DayWindow = 7 | 30 | 90;

interface FollowerHistoryChartProps {
  userId: string;
}

function formatDayLabel(value: string): string {
  const date = new Date(value);
  return date.toLocaleDateString(undefined, { month: "short", day: "numeric" });
}

export function FollowerHistoryChart({ userId }: FollowerHistoryChartProps) {
  const [days, setDays] = useState<DayWindow>(30);
  const [points, setPoints] = useState<FollowerHistoryPoint[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError(null);
    fetchFollowerHistory(userId, days)
      .then((result) => {
        if (!cancelled) {
          setPoints(result.points);
        }
      })
      .catch(() => {
        if (!cancelled) {
          setError("Could not load follower history.");
          setPoints([]);
        }
      })
      .finally(() => {
        if (!cancelled) {
          setLoading(false);
        }
      });
    return () => {
      cancelled = true;
    };
  }, [userId, days]);

  const chartData = useMemo(
    () =>
      points.map((p) => ({
        date: p.date,
        followers: p.followerCount,
      })),
    [points]
  );

  return (
    <Card variant="glass" className="mb-6 border-white/20">
      <CardHeader className="flex flex-row flex-wrap items-center justify-between gap-3 pb-2">
        <CardTitle className="text-lg text-white">Followers over time</CardTitle>
        <div className="flex gap-2">
          {([7, 30, 90] as DayWindow[]).map((window) => (
            <Button
              key={window}
              type="button"
              size="sm"
              variant={days === window ? "glass" : "outline"}
              onClick={() => setDays(window)}
            >
              {window}d
            </Button>
          ))}
        </div>
      </CardHeader>
      <CardContent>
        {loading ? (
          <ChartBlockSkeleton />
        ) : error ? (
          <p className="text-center text-sm text-red-300">{error}</p>
        ) : (
          <div className="h-64 w-full">
            <ResponsiveContainer width="100%" height="100%">
              <LineChart data={chartData} margin={{ top: 8, right: 8, left: 0, bottom: 0 }}>
                <CartesianGrid stroke="rgba(255,255,255,0.1)" strokeDasharray="3 3" />
                <XAxis
                  dataKey="date"
                  tickFormatter={formatDayLabel}
                  stroke="rgba(255,255,255,0.5)"
                  tick={{ fill: "rgba(255,255,255,0.6)", fontSize: 11 }}
                />
                <YAxis
                  allowDecimals={false}
                  stroke="rgba(255,255,255,0.5)"
                  tick={{ fill: "rgba(255,255,255,0.6)", fontSize: 11 }}
                />
                <Tooltip
                  contentStyle={{
                    backgroundColor: "rgba(15, 23, 42, 0.95)",
                    border: "1px solid rgba(255,255,255,0.2)",
                    borderRadius: "8px",
                    color: "#fff",
                  }}
                  labelFormatter={(label) => formatDayLabel(String(label))}
                />
                <Line
                  type="monotone"
                  dataKey="followers"
                  stroke="#60a5fa"
                  strokeWidth={2}
                  dot={false}
                  name="Followers"
                />
              </LineChart>
            </ResponsiveContainer>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
