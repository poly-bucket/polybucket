"use client"

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Checkbox } from "@/components/ui/checkbox"
import { Badge } from "@/components/ui/badge"

const CATEGORIES = [
  { id: "productivity", label: "Productivity", count: 45 },
  { id: "ui-components", label: "UI Components", count: 89 },
  { id: "data-viz", label: "Data Visualization", count: 34 },
  { id: "authentication", label: "Authentication", count: 23 },
  { id: "analytics", label: "Analytics", count: 28 },
  { id: "integrations", label: "Integrations", count: 56 },
]

const POPULAR_TAGS = ["react", "typescript", "api", "dashboard", "charts", "forms", "tables", "notifications"]

export function CategoryFilter() {
  return (
    <div className="space-y-4">
      <Card>
        <CardHeader>
          <CardTitle className="text-base">Categories</CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          {CATEGORIES.map((category) => (
            <div key={category.id} className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                <Checkbox id={category.id} />
                <label htmlFor={category.id} className="text-sm font-medium leading-none cursor-pointer">
                  {category.label}
                </label>
              </div>
              <span className="text-xs text-muted-foreground">{category.count}</span>
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
            {POPULAR_TAGS.map((tag) => (
              <Badge key={tag} variant="secondary" className="cursor-pointer hover:bg-secondary/80">
                {tag}
              </Badge>
            ))}
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
