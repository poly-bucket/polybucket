"use client"

import { ModerationQueue } from "@/components/moderation-queue"
import { Card, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { useAuth } from "@/context/AuthContext"
import Link from "next/link"
import { useRouter } from "next/navigation"
import { useEffect, useState } from "react"
import { MarketplaceHeader } from "@/components/marketplace-header"

export default function ModerationPage() {
  const [pendingCount, setPendingCount] = useState(0)
  const { isAdmin, isLoading, isAuthenticated } = useAuth()
  const router = useRouter()

  useEffect(() => {
    if (isLoading) {
      return
    }
    if (!isAuthenticated || !isAdmin()) {
      router.replace("/")
    }
  }, [isAdmin, isAuthenticated, isLoading, router])

  if (isLoading || !isAuthenticated || !isAdmin()) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center">
        <p className="text-muted-foreground">Loading…</p>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-background">
      <MarketplaceHeader showModeratorBadge />

      <main className="container mx-auto px-4 py-8">
        <div className="mb-8">
          <h1 className="text-3xl font-bold mb-3">Moderation Queue</h1>
          <p className="text-muted-foreground text-lg">Review and approve component submissions</p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
          <Card>
            <CardHeader className="pb-3">
              <CardDescription>Pending Review</CardDescription>
              <CardTitle className="text-3xl">{pendingCount}</CardTitle>
            </CardHeader>
          </Card>
        </div>

        <ModerationQueue onCountsChange={(counts) => setPendingCount(counts.pending)} />
      </main>
    </div>
  )
}
