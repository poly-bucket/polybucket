"use client"

import { PluginCard } from "@/components/plugin-card"
import { SortSelect } from "@/components/sort-select"
import { Pagination } from "@/components/pagination"
import { usePluginBrowse } from "@/hooks/usePluginBrowse"
import { SORT_OPTIONS } from "@/types/plugin"
import { Loader2 } from "lucide-react"

export function PluginGrid() {
  const {
    plugins,
    loading,
    error,
    totalCount,
    page,
    totalPages,
    hasNextPage,
    hasPreviousPage,
    setSortBy,
    setPage,
  } = usePluginBrowse()

  if (error) {
    return (
      <div className="text-center py-8">
        <p className="text-red-500 mb-4">Error: {error}</p>
        <button 
          onClick={() => window.location.reload()} 
          className="text-blue-500 hover:underline"
        >
          Try again
        </button>
      </div>
    )
  }

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <p className="text-sm text-muted-foreground">
          {loading ? "Loading..." : `Showing ${totalCount} components`}
        </p>
        <SortSelect 
          value={SORT_OPTIONS.DOWNLOADS} 
          onValueChange={setSortBy}
          data-testid="sort-select"
        />
      </div>

      {loading ? (
        <div className="flex items-center justify-center py-12">
          <Loader2 className="h-8 w-8 animate-spin" />
        </div>
      ) : plugins.length === 0 ? (
        <div className="text-center py-12">
          <p className="text-muted-foreground">No plugins found matching your criteria.</p>
        </div>
      ) : (
        <>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
            {plugins.map((plugin) => (
              <PluginCard key={plugin.id} plugin={plugin} data-testid="plugin-card" />
            ))}
          </div>
          
          <Pagination
            currentPage={page}
            totalPages={totalPages}
            hasNextPage={hasNextPage}
            hasPreviousPage={hasPreviousPage}
            onPageChange={setPage}
            data-testid="pagination"
          />
        </>
      )}
    </div>
  )
}
