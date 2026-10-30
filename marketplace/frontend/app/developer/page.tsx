"use client"

import { useEffect, useState } from "react"
import Link from "next/link"
import { useRouter } from "next/navigation"
import { MarketplaceHeader } from "@/components/marketplace-header"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { useAuth } from "@/context/AuthContext"
import { apiClient, type DeveloperAnalytics, type DeveloperPluginSummary, type DeveloperStats } from "@/lib/api"
import { Line, LineChart, XAxis, YAxis, CartesianGrid, ResponsiveContainer } from "recharts"

export default function DeveloperDashboardPage() {
  const { isAuthenticated, isLoading } = useAuth()
  const router = useRouter()
  const [stats, setStats] = useState<DeveloperStats | null>(null)
  const [plugins, setPlugins] = useState<DeveloperPluginSummary[]>([])
  const [analytics, setAnalytics] = useState<DeveloperAnalytics | null>(null)

  useEffect(() => {
    if (isLoading) return
    if (!isAuthenticated) {
      router.replace("/login?returnUrl=/developer")
      return
    }
    Promise.all([
      apiClient.getDeveloperStats(),
      apiClient.getDeveloperPlugins(),
      apiClient.getDeveloperAnalytics(),
    ])
      .then(([s, p, a]) => {
        setStats(s)
        setPlugins(p)
        setAnalytics(a)
      })
      .catch(() => undefined)
  }, [isAuthenticated, isLoading, router])

  if (isLoading || !isAuthenticated) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center">
        <p className="text-muted-foreground">Loading…</p>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-background">
      <MarketplaceHeader />
      <main className="container mx-auto px-4 py-8 space-y-8">
        <div className="flex flex-wrap items-center justify-between gap-4">
          <div>
            <h1 className="text-3xl font-bold">Developer dashboard</h1>
            <p className="text-muted-foreground">Your published components and install activity</p>
          </div>
          <Button asChild>
            <Link href="/submit">Submit new component</Link>
          </Button>
        </div>

        {stats ? (
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            <Card>
              <CardHeader className="pb-2">
                <CardTitle className="text-sm font-medium text-muted-foreground">Plugins</CardTitle>
              </CardHeader>
              <CardContent className="text-2xl font-bold">{stats.totalPlugins}</CardContent>
            </Card>
            <Card>
              <CardHeader className="pb-2">
                <CardTitle className="text-sm font-medium text-muted-foreground">Total installs</CardTitle>
              </CardHeader>
              <CardContent className="text-2xl font-bold">{stats.totalDownloads.toLocaleString()}</CardContent>
            </Card>
            <Card>
              <CardHeader className="pb-2">
                <CardTitle className="text-sm font-medium text-muted-foreground">Last 30 days</CardTitle>
              </CardHeader>
              <CardContent className="text-2xl font-bold">{stats.monthlyDownloads.toLocaleString()}</CardContent>
            </Card>
            <Card>
              <CardHeader className="pb-2">
                <CardTitle className="text-sm font-medium text-muted-foreground">Avg rating</CardTitle>
              </CardHeader>
              <CardContent className="text-2xl font-bold">{stats.averageRating.toFixed(1)}</CardContent>
            </Card>
          </div>
        ) : null}

        <Card>
          <CardHeader>
            <CardTitle>Installs (30 days)</CardTitle>
          </CardHeader>
          <CardContent>
        {analytics && analytics.downloads.length > 0 ? (
              <div className="h-[240px]">
                <ResponsiveContainer width="100%" height="100%">
                  <LineChart
                    data={analytics.downloads.map((d) => ({ date: d.date, installs: d.count }))}
                    margin={{ top: 5, right: 10, left: 10, bottom: 5 }}
                  >
                    <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
                    <XAxis dataKey="date" className="text-xs" />
                    <YAxis className="text-xs" />
                    <Line type="monotone" dataKey="installs" stroke="hsl(var(--primary))" strokeWidth={2} />
                  </LineChart>
                </ResponsiveContainer>
              </div>
            ) : (
              <p className="text-sm text-muted-foreground">No install activity recorded yet.</p>
            )}
          </CardContent>
        </Card>

        <section>
          <h2 className="text-xl font-bold mb-4">Your plugins</h2>
          {plugins.length === 0 ? (
            <p className="text-muted-foreground">No plugins yet. Submit a repository for review.</p>
          ) : (
            <div className="space-y-3">
              {plugins.map((plugin) => (
                <Card key={plugin.id}>
                  <CardContent className="py-4 flex flex-col md:flex-row md:items-center md:justify-between gap-3">
                    <div>
                      <Link href={`/plugin/${plugin.id}`} className="font-semibold hover:underline">
                        {plugin.name}
                      </Link>
                      <p className="text-sm text-muted-foreground">{plugin.description}</p>
                      <p className="text-xs text-muted-foreground mt-1">
                        v{plugin.version} · {plugin.downloads.toLocaleString()} installs · {plugin.status}
                      </p>
                    </div>
                    <Button variant="outline" size="sm" asChild>
                      <Link
                        href={`/submit?pluginId=${encodeURIComponent(plugin.id)}&repositoryUrl=${encodeURIComponent("")}`}
                      >
                        Submit update
                      </Link>
                    </Button>
                  </CardContent>
                </Card>
              ))}
            </div>
          )}
        </section>
      </main>
    </div>
  )
}
