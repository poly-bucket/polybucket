"use client"

import { useEffect, useState } from "react"
import { useParams } from "next/navigation"
import { MarketplaceHeader } from "@/components/marketplace-header"
import { PluginCard } from "@/components/plugin-card"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { apiClient, type PublicUserProfile } from "@/lib/api"
import Link from "next/link"

export default function PublicProfilePage() {
  const params = useParams()
  const username =
    typeof params.username === "string" ? params.username : Array.isArray(params.username) ? params.username[0] : ""

  const [profile, setProfile] = useState<PublicUserProfile | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!username) return
    apiClient
      .getPublicProfile(username)
      .then(setProfile)
      .catch(() => setError("User not found"))
  }, [username])

  return (
    <div className="min-h-screen bg-background">
      <MarketplaceHeader />
      <main className="container mx-auto px-4 py-8">
        {error ? (
          <Card>
            <CardContent className="py-12 text-center">
              <p className="text-muted-foreground mb-4">{error}</p>
              <Link href="/">
                <Button>Back to Marketplace</Button>
              </Link>
            </CardContent>
          </Card>
        ) : !profile ? (
          <p className="text-muted-foreground">Loading profile…</p>
        ) : (
          <>
            <Card className="mb-8">
              <CardContent className="pt-6">
                <div className="flex flex-col md:flex-row gap-6">
                  <Avatar className="h-24 w-24">
                    <AvatarImage src={profile.avatarUrl} alt={profile.displayName || profile.username} />
                    <AvatarFallback>{(profile.displayName || profile.username).charAt(0).toUpperCase()}</AvatarFallback>
                  </Avatar>
                  <div className="flex-1">
                    <h1 className="text-3xl font-bold mb-2">{profile.displayName || profile.username}</h1>
                    {profile.bio ? <p className="text-muted-foreground mb-4 text-pretty">{profile.bio}</p> : null}
                    <div className="flex flex-wrap gap-4 text-sm">
                      <span>
                        <span className="text-muted-foreground">Components:</span>{" "}
                        <span className="font-semibold">{profile.pluginCount}</span>
                      </span>
                      <span>
                        <span className="text-muted-foreground">Total installs recorded:</span>{" "}
                        <span className="font-semibold">{profile.totalDownloads.toLocaleString()}</span>
                      </span>
                    </div>
                    {profile.githubProfileUrl ? (
                      <Button variant="outline" size="sm" className="mt-4" asChild>
                        <a href={profile.githubProfileUrl} target="_blank" rel="noopener noreferrer">
                          GitHub profile
                        </a>
                      </Button>
                    ) : null}
                  </div>
                </div>
              </CardContent>
            </Card>

            <h2 className="text-2xl font-bold mb-4">Published components</h2>
            {profile.plugins.length === 0 ? (
              <p className="text-muted-foreground">No published components yet.</p>
            ) : (
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {profile.plugins.map((plugin) => (
                  <PluginCard key={plugin.id} plugin={plugin} />
                ))}
              </div>
            )}
          </>
        )}
      </main>
    </div>
  )
}
