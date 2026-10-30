"use client"

import { useEffect, useState } from "react"
import { PluginCard } from "@/components/plugin-card"
import { apiClient } from "@/lib/api"
import type { PluginSummary } from "@/types/plugin"

export function HomeFeaturedSections() {
  const [featured, setFeatured] = useState<PluginSummary[]>([])
  const [trending, setTrending] = useState<PluginSummary[]>([])

  useEffect(() => {
    apiClient.getFeaturedPlugins(6).then(setFeatured).catch(() => setFeatured([]))
    apiClient.getTrendingPlugins(6).then(setTrending).catch(() => setTrending([]))
  }, [])

  if (featured.length === 0 && trending.length === 0) {
    return null
  }

  return (
    <div className="space-y-10 mb-10">
      {featured.length > 0 ? (
        <section>
          <h2 className="text-2xl font-bold mb-4">Featured</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {featured.map((plugin) => (
              <PluginCard key={plugin.id} plugin={plugin} />
            ))}
          </div>
        </section>
      ) : null}
      {trending.length > 0 ? (
        <section>
          <h2 className="text-2xl font-bold mb-4">Trending</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {trending.map((plugin) => (
              <PluginCard key={plugin.id} plugin={plugin} />
            ))}
          </div>
        </section>
      ) : null}
    </div>
  )
}
