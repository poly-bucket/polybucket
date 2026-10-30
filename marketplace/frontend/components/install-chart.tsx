"use client"

import { useEffect, useState } from "react"
import { Line, LineChart, XAxis, YAxis, CartesianGrid, ResponsiveContainer } from "recharts"
import { ChartContainer, ChartTooltip, ChartTooltipContent } from "@/components/ui/chart"
import { apiClient, type PluginDownloadStat } from "@/lib/api"

interface InstallChartProps {
  pluginId: string
  days?: number
}

export function InstallChart({ pluginId, days = 30 }: InstallChartProps) {
  const [data, setData] = useState<PluginDownloadStat[]>([])

  useEffect(() => {
    apiClient
      .getPluginDownloadStats(pluginId, days)
      .then(setData)
      .catch(() => setData([]))
  }, [pluginId, days])

  if (data.length === 0) {
    return <p className="text-sm text-muted-foreground py-8 text-center">No install activity in this period.</p>
  }

  const chartData = data.map((row) => ({ date: row.date, installs: row.count }))

  return (
    <ChartContainer
      config={{
        installs: {
          label: "Installs",
          color: "hsl(var(--primary))",
        },
      }}
      className="h-[300px]"
    >
      <ResponsiveContainer width="100%" height="100%">
        <LineChart data={chartData} margin={{ top: 5, right: 10, left: 10, bottom: 5 }}>
          <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
          <XAxis dataKey="date" className="text-xs" />
          <YAxis className="text-xs" />
          <ChartTooltip content={<ChartTooltipContent />} />
          <Line
            type="monotone"
            dataKey="installs"
            stroke="var(--color-installs)"
            strokeWidth={2}
            dot={{ fill: "var(--color-installs)" }}
          />
        </LineChart>
      </ResponsiveContainer>
    </ChartContainer>
  )
}
