"use client"

import { ModerationQueue } from "@/components/moderation-queue"
import { Card, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import { ThemeToggle } from "@/components/theme-toggle"
import Link from "next/link"
import { useState } from "react"

export default function ModerationPage() {
  const [pendingCount, setPendingCount] = useState(0)

  return (
    <div className="min-h-screen bg-background">
      <header className="border-b bg-card/50 backdrop-blur-sm sticky top-0 z-50">
        <div className="container mx-auto px-4 py-4">
          <div className="flex items-center justify-between">
            <Link href="/" className="flex items-center gap-2">
              <div className="h-8 w-8 rounded-lg bg-primary" />
              <span className="font-semibold text-xl">Polybucket Marketplace</span>
            </Link>
            <div className="flex items-center gap-3">
              <ThemeToggle />
              <Badge variant="secondary">Moderator</Badge>
              <Link href="/login">
                <Button variant="outline" size="sm">
                  Sign Out
                </Button>
              </Link>
            </div>
          </div>
        </div>
      </header>

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
