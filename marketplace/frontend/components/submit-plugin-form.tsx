"use client"

import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Badge } from "@/components/ui/badge"
import { X } from "lucide-react"
import { useState } from "react"

const CATEGORIES = [
  "Productivity",
  "UI Components",
  "Data Visualization",
  "Authentication",
  "Analytics",
  "Integrations",
]

export function SubmitPluginForm() {
  const [tags, setTags] = useState<string[]>([])
  const [tagInput, setTagInput] = useState("")

  const addTag = () => {
    if (tagInput.trim() && !tags.includes(tagInput.trim())) {
      setTags([...tags, tagInput.trim()])
      setTagInput("")
    }
  }

  const removeTag = (tag: string) => {
    setTags(tags.filter((t) => t !== tag))
  }

  return (
    <form className="space-y-6">
      <div className="space-y-2">
        <Label htmlFor="name">Component Name *</Label>
        <Input id="name" placeholder="My Awesome Component" required />
      </div>

      <div className="space-y-2">
        <Label htmlFor="description">Description *</Label>
        <Textarea id="description" placeholder="A brief description of what your component does..." rows={3} required />
      </div>

      <div className="space-y-2">
        <Label htmlFor="github">GitHub Repository URL *</Label>
        <Input id="github" type="url" placeholder="https://github.com/username/repo" required />
        <p className="text-xs text-muted-foreground">Must be a public repository with an open source license</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div className="space-y-2">
          <Label htmlFor="version">Version *</Label>
          <Input id="version" placeholder="1.0.0" required />
        </div>

        <div className="space-y-2">
          <Label htmlFor="category">Category *</Label>
          <Select required>
            <SelectTrigger id="category">
              <SelectValue placeholder="Select a category" />
            </SelectTrigger>
            <SelectContent>
              {CATEGORIES.map((cat) => (
                <SelectItem key={cat} value={cat.toLowerCase().replace(/\s+/g, "-")}>
                  {cat}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      </div>

      <div className="space-y-2">
        <Label htmlFor="tags">Tags</Label>
        <div className="flex gap-2">
          <Input
            id="tags"
            placeholder="Add tags (e.g., react, typescript)"
            value={tagInput}
            onChange={(e) => setTagInput(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === "Enter") {
                e.preventDefault()
                addTag()
              }
            }}
          />
          <Button type="button" variant="secondary" onClick={addTag}>
            Add
          </Button>
        </div>
        {tags.length > 0 && (
          <div className="flex flex-wrap gap-2 mt-2">
            {tags.map((tag) => (
              <Badge key={tag} variant="secondary" className="gap-1">
                {tag}
                <button type="button" onClick={() => removeTag(tag)} className="hover:text-destructive">
                  <X className="h-3 w-3" />
                </button>
              </Badge>
            ))}
          </div>
        )}
      </div>

      <div className="space-y-2">
        <Label htmlFor="readme">README URL (optional)</Label>
        <Input id="readme" type="url" placeholder="https://github.com/username/repo/blob/main/README.md" />
        <p className="text-xs text-muted-foreground">Link to your README for screenshots and demo videos</p>
      </div>

      <div className="flex gap-3 pt-4">
        <Button type="submit" size="lg">
          Submit for Review
        </Button>
        <Button type="button" variant="outline" size="lg">
          Save Draft
        </Button>
      </div>
    </form>
  )
}
