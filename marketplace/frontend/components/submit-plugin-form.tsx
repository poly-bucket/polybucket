"use client"

import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { useState } from "react"
import { apiClient } from "@/lib/api"
import { toast } from "sonner"

const CATEGORIES = [
  "Productivity",
  "UI Components",
  "Data Visualization",
  "Authentication",
  "Analytics",
  "Integrations",
  "Themes",
]

export function SubmitPluginForm() {
  const [name, setName] = useState("")
  const [description, setDescription] = useState("")
  const [repositoryUrl, setRepositoryUrl] = useState("")
  const [repositoryBranch, setRepositoryBranch] = useState("main")
  const [repositorySubpath, setRepositorySubpath] = useState("")
  const [category, setCategory] = useState("")
  const [license, setLicense] = useState("MIT")
  const [submitting, setSubmitting] = useState(false)

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault()
    if (!category) {
      toast.error("Select a category")
      return
    }

    setSubmitting(true)
    try {
      await apiClient.createSubmission({
        name,
        description,
        category,
        license,
        repositoryUrl,
        repositoryBranch,
        repositorySubpath,
      })
      toast.success("Submission received. A marketplace admin will review it.")
      setName("")
      setDescription("")
      setRepositoryUrl("")
      setRepositorySubpath("")
      setCategory("")
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Submission failed")
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <form className="space-y-6" onSubmit={handleSubmit}>
      <div className="space-y-2">
        <Label htmlFor="name">Component Name *</Label>
        <Input
          id="name"
          placeholder="My Awesome Component"
          required
          value={name}
          onChange={(e) => setName(e.target.value)}
        />
      </div>

      <div className="space-y-2">
        <Label htmlFor="description">Description *</Label>
        <Textarea
          id="description"
          placeholder="A brief description of what your component does..."
          rows={3}
          required
          value={description}
          onChange={(e) => setDescription(e.target.value)}
        />
      </div>

      <div className="space-y-2">
        <Label htmlFor="github">GitHub Repository URL *</Label>
        <Input
          id="github"
          type="url"
          placeholder="https://github.com/username/repo"
          required
          value={repositoryUrl}
          onChange={(e) => setRepositoryUrl(e.target.value)}
        />
        <p className="text-xs text-muted-foreground">
          The repository must contain polybucket-plugin.json at the root or in the subpath below.
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div className="space-y-2">
          <Label htmlFor="branch">Branch</Label>
          <Input
            id="branch"
            placeholder="main"
            value={repositoryBranch}
            onChange={(e) => setRepositoryBranch(e.target.value)}
          />
        </div>

        <div className="space-y-2">
          <Label htmlFor="category">Category *</Label>
          <Select required value={category} onValueChange={setCategory}>
            <SelectTrigger id="category">
              <SelectValue placeholder="Select a category" />
            </SelectTrigger>
            <SelectContent>
              {CATEGORIES.map((cat) => (
                <SelectItem key={cat} value={cat}>
                  {cat}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      </div>

      <div className="space-y-2">
        <Label htmlFor="subpath">Repository subpath (optional)</Label>
        <Input
          id="subpath"
          placeholder="examples/plugins/my-plugin"
          value={repositorySubpath}
          onChange={(e) => setRepositorySubpath(e.target.value)}
        />
      </div>

      <div className="space-y-2">
        <Label htmlFor="license">License</Label>
        <Input id="license" value={license} onChange={(e) => setLicense(e.target.value)} />
      </div>

      <div className="flex gap-3 pt-4">
        <Button type="submit" size="lg" disabled={submitting}>
          {submitting ? "Submitting…" : "Submit for Review"}
        </Button>
      </div>
    </form>
  )
}
