import { MarketplaceHeader } from "@/components/marketplace-header"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"

export default function DocsPage() {
  return (
    <div className="min-h-screen bg-background">
      <MarketplaceHeader />
      <main className="container mx-auto px-4 py-8 max-w-3xl space-y-8">
        <div>
          <h1 className="text-3xl font-bold mb-2">Documentation</h1>
          <p className="text-muted-foreground">How to publish and install PolyBucket plugins through the marketplace.</p>
        </div>

        <Card>
          <CardHeader>
            <CardTitle>polybucket-plugin.json</CardTitle>
          </CardHeader>
          <CardContent className="prose prose-sm dark:prose-invert max-w-none space-y-3">
            <p>Each plugin repository must include a manifest at the repo root or in your submission subpath:</p>
            <ul>
              <li>
                <strong>id</strong> — stable plugin identifier (used in the catalog and PolyBucket install)
              </li>
              <li>
                <strong>name</strong>, <strong>description</strong>, <strong>version</strong>
              </li>
              <li>
                <strong>targets</strong> — array including <code>api</code>, <code>frontend</code>, or both
              </li>
              <li>
                <strong>tags</strong> (optional) — lowercase labels for browse filters
              </li>
            </ul>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Submit a component</CardTitle>
          </CardHeader>
          <CardContent className="prose prose-sm dark:prose-invert max-w-none">
            <ol>
              <li>Sign in with GitHub.</li>
              <li>Open Submit Component and enter your public GitHub repository URL.</li>
              <li>Set branch and subpath if the manifest is not at the repository root.</li>
              <li>Wait for an admin to approve the submission. Approval reads the manifest and publishes the listing.</li>
              <li>To ship a new version, submit again with an updated manifest version.</li>
            </ol>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Install on your PolyBucket instance</CardTitle>
          </CardHeader>
          <CardContent className="prose prose-sm dark:prose-invert max-w-none">
            <ol>
              <li>In PolyBucket, open Admin → Plugins → Marketplace.</li>
              <li>Set the marketplace base URL to this catalog service.</li>
              <li>Browse the catalog and click Install on a plugin.</li>
              <li>PolyBucket downloads the GitHub archive, validates the manifest, and installs API and/or frontend assets.</li>
            </ol>
          </CardContent>
        </Card>
      </main>
    </div>
  )
}
