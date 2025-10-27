import { SubmitPluginForm } from "@/components/submit-plugin-form"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { ThemeToggle } from "@/components/theme-toggle"
import Link from "next/link"
import { AlertCircle } from "lucide-react"
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert"

export default function SubmitPage() {
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
              <Link href="/login">
                <Button variant="outline" size="sm">
                  Sign In
                </Button>
              </Link>
            </div>
          </div>
        </div>
      </header>

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
            All components must be open source with a valid GitHub repository. Our team will review your submission for
            security and implementation quality before approval.
          </AlertDescription>
        </Alert>

        <Card>
          <CardHeader>
            <CardTitle>Component Details</CardTitle>
            <CardDescription>Provide information about your component. Fields marked with * are required.</CardDescription>
          </CardHeader>
          <CardContent>
            <SubmitPluginForm />
          </CardContent>
        </Card>
      </main>
    </div>
  )
}
