"use client"

import { Line, LineChart, XAxis, YAxis, CartesianGrid, ResponsiveContainer } from "recharts"
import { ChartContainer, ChartTooltip, ChartTooltipContent } from "@/components/ui/chart"

const data = [
  { month: "Jul", installs: 1200 },
  { month: "Aug", installs: 1800 },
  { month: "Sep", installs: 2400 },
  { month: "Oct", installs: 3200 },
  { month: "Nov", installs: 4100 },
  { month: "Dec", installs: 5800 },
  { month: "Jan", installs: 7200 },
]

export function InstallChart() {
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
        <LineChart data={data} margin={{ top: 5, right: 10, left: 10, bottom: 5 }}>
          <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
          <XAxis dataKey="month" className="text-xs" />
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
