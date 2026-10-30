"use client"

import { useState, useEffect, useCallback } from "react"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Separator } from "@/components/ui/separator"
import {
  Star,
  Shield,
  Sparkles,
  Calendar,
  User,
  Tag,
  Github,
  GitBranch,
  Package,
  FileText,
  AlertTriangle,
  Loader2,
  Copy,
} from "lucide-react"
import Link from "next/link"
import { MarketplacePluginDetails } from "@/types/plugin"
import { apiClient } from "@/lib/api"
import { MarketplaceHeader } from "@/components/marketplace-header"
import { RepositoryInfo } from "@/components/repository-info"
import { parseGitHubRepo } from "@/lib/github-links"
import { InstallChart } from "@/components/install-chart"
import { PluginReviewsSection } from "@/components/plugin-reviews-section"
import { toast } from "sonner"

interface PluginDetailsPageProps {
  pluginId: string
}

export function PluginDetailsPage({ pluginId }: PluginDetailsPageProps) {
  const [details, setDetails] = useState<MarketplacePluginDetails | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const loadPluginData = useCallback(async () => {
    setIsLoading(true)
    setError(null)
    try {
      const data = await apiClient.getPluginDetails(pluginId)
      setDetails(data)
    } catch (err) {
      setError("Failed to load plugin information")
      console.error("Plugin loading error:", err)
    } finally {
      setIsLoading(false)
    }
  }, [pluginId])

  useEffect(() => {
    loadPluginData()
  }, [loadPluginData])

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString("en-US", {
      year: "numeric",
      month: "long",
      day: "numeric",
    })
  }

  const copyPluginId = () => {
    void navigator.clipboard.writeText(pluginId)
    toast.success("Plugin ID copied")
  }

  if (isLoading) {
    return (
      <div className="min-h-screen bg-background">
        <MarketplaceHeader />
        <div className="container mx-auto px-4 py-8 flex items-center justify-center py-12">
          <Loader2 className="h-8 w-8 animate-spin" />
          <span className="ml-2">Loading plugin details...</span>
        </div>
      </div>
    )
  }

  if (error || !details) {
    return (
      <div className="min-h-screen bg-background">
        <MarketplaceHeader />
        <div className="container mx-auto px-4 py-8">
          <Card>
            <CardContent className="flex items-center justify-center py-12">
              <div className="text-center">
                <AlertTriangle className="h-12 w-12 text-red-500 mx-auto mb-4" />
                <h2 className="text-xl font-semibold mb-2">Plugin Not Found</h2>
                <p className="text-muted-foreground mb-4">{error}</p>
                <Link href="/">
                  <Button>Back to Marketplace</Button>
                </Link>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    )
  }

  const github = parseGitHubRepo(details.repositoryUrl)
  const authorProfile = details.author ? `/profile/${encodeURIComponent(details.author)}` : null

  return (
    <div className="min-h-screen bg-background">
      <MarketplaceHeader />
      <div className="container mx-auto px-4 py-8">
        <div className="mb-8">
          <div className="flex items-center gap-2 text-sm text-muted-foreground mb-4">
            <Link href="/" className="hover:text-foreground">
              Marketplace
            </Link>
            <span>/</span>
            <span>{details.category}</span>
            <span>/</span>
            <span className="text-foreground">{details.name}</span>
          </div>

          <div className="flex items-start justify-between gap-6 flex-wrap">
            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-3 mb-2 flex-wrap">
                <h1 className="text-3xl font-bold">{details.name}</h1>
                {details.isVerified && (
                  <Badge variant="secondary" className="gap-1">
                    <Shield className="h-3 w-3" />
                    Verified
                  </Badge>
                )}
                {details.isFeatured && (
                  <Badge variant="default" className="gap-1">
                    <Sparkles className="h-3 w-3" />
                    Featured
                  </Badge>
                )}
              </div>
              <p className="text-lg text-muted-foreground mb-4">{details.description}</p>
              {details.longDescription ? (
                <p className="text-muted-foreground mb-4 text-pretty">{details.longDescription}</p>
              ) : null}

              <div className="flex items-center gap-6 text-sm flex-wrap">
                <div className="flex items-center gap-2">
                  <User className="h-4 w-4 text-muted-foreground" />
                  {authorProfile ? (
                    <Link href={authorProfile} className="hover:underline">
                      {details.author}
                    </Link>
                  ) : (
                    <span>{details.author}</span>
                  )}
                </div>
                <div className="flex items-center gap-2">
                  <Package className="h-4 w-4 text-muted-foreground" />
                  <span>v{details.version}</span>
                </div>
                <div className="flex items-center gap-2">
                  <Calendar className="h-4 w-4 text-muted-foreground" />
                  <span>Updated {formatDate(details.updatedAt)}</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          <div className="lg:col-span-2 space-y-6">
            <Card>
              <CardHeader>
                <CardTitle>Statistics</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                  <div className="text-center">
                    <div className="text-2xl font-bold text-primary">{details.downloads.toLocaleString()}</div>
                    <div className="text-sm text-muted-foreground">Installs recorded</div>
                  </div>
                  <div className="text-center">
                    <div className="text-2xl font-bold text-primary">{details.averageRating.toFixed(1)}</div>
                    <div className="text-sm text-muted-foreground">Rating</div>
                  </div>
                  <div className="text-center">
                    <div className="text-2xl font-bold text-primary">{details.reviewCount}</div>
                    <div className="text-sm text-muted-foreground">Reviews</div>
                  </div>
                  <div className="text-center">
                    <div className="text-2xl font-bold text-primary">{details.tags.length}</div>
                    <div className="text-sm text-muted-foreground">Tags</div>
                  </div>
                </div>
              </CardContent>
            </Card>

            {details.tags.length > 0 ? (
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <Tag className="h-5 w-5" />
                    Tags
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="flex flex-wrap gap-2">
                    {details.tags.map((tag) => (
                      <Badge key={tag} variant="secondary">
                        {tag}
                      </Badge>
                    ))}
                  </div>
                </CardContent>
              </Card>
            ) : null}

            {details.versions.length > 0 ? (
              <Card>
                <CardHeader>
                  <CardTitle>Version history</CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                  {details.versions.map((v) => (
                    <div key={v.id} className="flex items-start gap-4 pb-4 border-b last:border-0">
                      <Badge variant="outline" className="shrink-0">
                        v{v.version}
                      </Badge>
                      <div className="flex-1 min-w-0">
                        <p className="text-sm">{v.releaseNotes || "No release notes"}</p>
                        <p className="text-xs text-muted-foreground mt-1">{formatDate(v.createdAt)}</p>
                      </div>
                    </div>
                  ))}
                </CardContent>
              </Card>
            ) : null}

            <Card>
              <CardHeader>
                <CardTitle>Install activity (30 days)</CardTitle>
              </CardHeader>
              <CardContent>
                <InstallChart pluginId={pluginId} />
              </CardContent>
            </Card>

            <RepositoryInfo
              repositoryUrl={details.repositoryUrl}
              repositoryBranch={details.repositoryBranch}
              repositorySubpath={details.repositorySubpath}
            />

            <PluginReviewsSection pluginId={pluginId} />
          </div>

          <div className="space-y-6">
            <Card>
              <CardHeader>
                <CardTitle>Install on PolyBucket</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4 text-sm">
                <p className="text-muted-foreground text-pretty">
                  Install this plugin from your PolyBucket instance. Open Admin → Plugins → Marketplace, set your
                  marketplace URL, find this plugin, and click Install.
                </p>
                <div className="rounded-lg border bg-muted/50 p-3 font-mono text-xs break-all">{pluginId}</div>
                <Button variant="outline" size="sm" className="gap-2 w-full" onClick={copyPluginId}>
                  <Copy className="h-4 w-4" />
                  Copy plugin ID
                </Button>
                <Separator />
                <div className="space-y-2">
                  <div className="flex items-center gap-2 text-muted-foreground">
                    <Github className="h-4 w-4" />
                    <span>Source on GitHub</span>
                  </div>
                  {details.repositoryBranch ? (
                    <div className="flex items-center gap-2 text-muted-foreground">
                      <GitBranch className="h-4 w-4" />
                      <span>{details.repositoryBranch}</span>
                    </div>
                  ) : null}
                  {details.targets.length > 0 ? (
                    <div className="flex flex-wrap gap-1 pt-1">
                      {details.targets.map((t) => (
                        <Badge key={t} variant="secondary">
                          {t}
                        </Badge>
                      ))}
                    </div>
                  ) : null}
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle className="text-base">Links</CardTitle>
              </CardHeader>
              <CardContent className="space-y-2">
                <Button variant="outline" className="w-full justify-start gap-2" asChild>
                  <a href={details.repositoryUrl} target="_blank" rel="noopener noreferrer">
                    <Github className="h-4 w-4" />
                    Repository
                  </a>
                </Button>
                {github ? (
                  <>
                    <Button variant="outline" className="w-full justify-start gap-2" asChild>
                      <a href={github.readmeUrl} target="_blank" rel="noopener noreferrer">
                        <FileText className="h-4 w-4" />
                        README
                      </a>
                    </Button>
                    <Button variant="outline" className="w-full justify-start gap-2" asChild>
                      <a href={github.issuesUrl} target="_blank" rel="noopener noreferrer">
                        <AlertTriangle className="h-4 w-4" />
                        Report issue
                      </a>
                    </Button>
                  </>
                ) : null}
                <div className="flex items-center gap-2 text-sm pt-2">
                  <Star className="h-4 w-4 fill-yellow-400 text-yellow-400" />
                  <span>
                    {details.averageRating.toFixed(1)} · {details.reviewCount} reviews
                  </span>
                </div>
                {details.license ? (
                  <p className="text-sm text-muted-foreground">License: {details.license}</p>
                ) : null}
              </CardContent>
            </Card>
          </div>
        </div>
      </div>
    </div>
  )
}
