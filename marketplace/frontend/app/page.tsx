"use client"

import { PluginGrid } from "@/components/plugin-grid"
import { SearchBar } from "@/components/search-bar"
import { CategoryFilter } from "@/components/category-filter"
import { MarketplaceHeader } from "@/components/marketplace-header"
import { usePluginBrowse } from "@/hooks/usePluginBrowse"
import { HomeFeaturedSections } from "@/components/home-featured-sections"

export default function Home() {
  const browse = usePluginBrowse()

  return (
    <div className="min-h-screen bg-background">
      <MarketplaceHeader />

      <main className="container mx-auto px-4 py-8">
        <div className="mb-8">
          <h1 className="text-4xl font-bold mb-3 text-balance">Discover Components</h1>
          <p className="text-muted-foreground text-lg text-pretty">
            Browse and install community-built components to extend your application
          </p>
        </div>

        <HomeFeaturedSections />

        <div className="mb-6">
          <SearchBar onSearch={browse.setSearch} />
        </div>

        <div className="flex flex-col lg:flex-row gap-6">
          <aside className="lg:w-64 shrink-0">
            <CategoryFilter
              onCategoryChange={browse.setCategory}
              onTagChange={browse.setTags}
              selectedTags={browse.selectedTags}
            />
          </aside>
          <div className="flex-1">
            <PluginGrid browse={browse} />
          </div>
        </div>
      </main>
    </div>
  )
}
