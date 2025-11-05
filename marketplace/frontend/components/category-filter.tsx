"use client"

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Checkbox } from "@/components/ui/checkbox"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { useState, useEffect } from "react"
import { apiClient } from "@/lib/api"

interface CategoryFilterProps {
  onCategoryChange: (category: string | null) => void;
  onTagChange: (tags: string[]) => void;
  selectedCategory?: string | null;
  selectedTags?: string[];
}

export function CategoryFilter({ onCategoryChange, onTagChange, selectedCategory, selectedTags = [] }: CategoryFilterProps) {
  const [categories, setCategories] = useState<string[]>([])
  const [popularTags, setPopularTags] = useState<string[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const fetchData = async () => {
      try {
        const [categoriesData, tagsData] = await Promise.all([
          apiClient.getCategories(),
          apiClient.getPopularTags()
        ])
        setCategories(categoriesData)
        setPopularTags(tagsData)
      } catch (error) {
        console.error('Failed to fetch filter data:', error)
        // Fallback to mock data
        setCategories([
          "UI Components",
          "Authentication", 
          "Data Visualization",
          "Integrations",
          "Productivity",
          "Analytics",
          "Themes",
          "Localization",
          "Other"
        ])
        setPopularTags([
          "react", "typescript", "api", "dashboard", "charts", "forms", 
          "tables", "notifications", "authentication", "ui-components"
        ])
      } finally {
        setLoading(false)
      }
    }

    fetchData()
  }, [])

  const handleCategoryToggle = (category: string) => {
    if (selectedCategory === category) {
      onCategoryChange(null)
    } else {
      onCategoryChange(category)
    }
  }

  const handleTagToggle = (tag: string) => {
    const newTags = selectedTags.includes(tag)
      ? selectedTags.filter(t => t !== tag)
      : [...selectedTags, tag]
    onTagChange(newTags)
  }

  const clearFilters = () => {
    onCategoryChange(null)
    onTagChange([])
  }

  if (loading) {
    return (
      <div className="space-y-4">
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Categories</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            {[...Array(5)].map((_, i) => (
              <div key={i} className="h-6 bg-muted animate-pulse rounded" />
            ))}
          </CardContent>
        </Card>
      </div>
    )
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="text-sm font-medium">Filters</h3>
        {(selectedCategory || selectedTags.length > 0) && (
          <Button variant="ghost" size="sm" onClick={clearFilters}>
            Clear all
          </Button>
        )}
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="text-base">Categories</CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          {categories.map((category) => (
            <div key={category} className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                <Checkbox 
                  id={category} 
                  checked={selectedCategory === category}
                  onCheckedChange={() => handleCategoryToggle(category)}
                  data-testid="category-checkbox"
                />
                <label htmlFor={category} className="text-sm font-medium leading-none cursor-pointer">
                  {category}
                </label>
              </div>
            </div>
          ))}
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="text-base">Popular Tags</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex flex-wrap gap-2">
            {popularTags.map((tag) => (
              <Badge 
                key={tag} 
                variant={selectedTags.includes(tag) ? "default" : "secondary"}
                className="cursor-pointer hover:bg-secondary/80"
                onClick={() => handleTagToggle(tag)}
                data-testid="tag-badge"
              >
                {tag}
              </Badge>
            ))}
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
