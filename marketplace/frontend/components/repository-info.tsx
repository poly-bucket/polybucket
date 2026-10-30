"use client"

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Github, GitBranch, ExternalLink, FileText, AlertTriangle } from "lucide-react"
import { parseGitHubRepo } from "@/lib/github-links"

interface RepositoryInfoProps {
  repositoryUrl: string
  repositoryBranch?: string
  repositorySubpath?: string
  className?: string
}

export function RepositoryInfo({
  repositoryUrl,
  repositoryBranch,
  repositorySubpath,
  className = "",
}: RepositoryInfoProps) {
  const github = parseGitHubRepo(repositoryUrl)
  const displayName = github ? `${github.owner}/${github.repo}` : repositoryUrl

  return (
    <Card className={className}>
      <CardHeader className="pb-3">
        <CardTitle className="flex items-center gap-2 text-base">
          <Github className="h-4 w-4" />
          Repository
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="space-y-2">
          <p className="font-semibold">{displayName}</p>
          {repositoryBranch ? (
            <div className="flex items-center gap-2 text-sm text-muted-foreground">
              <GitBranch className="h-4 w-4" />
              Branch: {repositoryBranch}
            </div>
          ) : null}
          {repositorySubpath ? (
            <p className="text-sm text-muted-foreground">Subpath: {repositorySubpath}</p>
          ) : null}
        </div>

        <div className="flex flex-wrap gap-2">
          <Button variant="outline" size="sm" className="gap-2" asChild>
            <a href={repositoryUrl} target="_blank" rel="noopener noreferrer">
              <ExternalLink className="h-4 w-4" />
              View repository
            </a>
          </Button>
          {github ? (
            <>
              <Button variant="outline" size="sm" className="gap-2" asChild>
                <a href={github.readmeUrl} target="_blank" rel="noopener noreferrer">
                  <FileText className="h-4 w-4" />
                  README
                </a>
              </Button>
              <Button variant="outline" size="sm" className="gap-2" asChild>
                <a href={github.issuesUrl} target="_blank" rel="noopener noreferrer">
                  <AlertTriangle className="h-4 w-4" />
                  Issues
                </a>
              </Button>
            </>
          ) : null}
        </div>

        <p className="text-xs text-muted-foreground bg-muted/50 p-2 rounded">
          Plugins install from the GitHub repository on your PolyBucket instance. The marketplace catalog stores the
          repository URL and manifest metadata.
        </p>
      </CardContent>
    </Card>
  )
}
