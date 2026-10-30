"use client"

import { SubmitPluginForm } from "@/components/submit-plugin-form"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert"
import { MarketplaceHeader } from "@/components/marketplace-header"
import { AlertCircle } from "lucide-react"
import { useAuth } from "@/context/AuthContext"
import Link from "next/link"
import { Button } from "@/components/ui/button"

export default function SubmitPage() {
  const { isAuthenticated, isLoading } = useAuth()

  return (
    <div className="min-h-screen bg-background">
      <MarketplaceHeader />

      <main className="container mx-auto px-4 py-8 max-w-4xl">
        <div className="mb-8">
          <h1 className="text-3xl font-bold mb-3">Submit a Component</h1>
          <p className="text-muted-foreground text-lg text-pretty">
            Share your component with the community. All submissions are reviewed by our moderation team.
          </p>
        </div>

        <Alert className="mb-6">
          <AlertCircle className="h-4 w-4" />
          <AlertTitle>Submission Guidelines</AlertTitle>
          <AlertDescription>
            All components must be open source with a valid GitHub repository containing polybucket-plugin.json.
          </AlertDescription>
        </Alert>

        {!isLoading && !isAuthenticated ? (
          <Card>
            <CardContent className="py-8 text-center space-y-4">
              <p className="text-muted-foreground">Sign in with GitHub before submitting a component.</p>
              <Button asChild>
                <Link href="/login?returnUrl=/submit">Sign in</Link>
              </Button>
            </CardContent>
          </Card>
        ) : (
          <Card>
            <CardHeader>
              <CardTitle>Component Details</CardTitle>
              <CardDescription>Provide information about your component. Fields marked with * are required.</CardDescription>
            </CardHeader>
            <CardContent>
              <SubmitPluginForm />
            </CardContent>
          </Card>
        )}
      </main>
    </div>
  )
}
