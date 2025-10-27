import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { Separator } from "@/components/ui/separator"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { ThemeToggle } from "@/components/theme-toggle"
import { Download, Star, Github, ExternalLink, Calendar, Package } from "lucide-react"
import Link from "next/link"
import { InstallChart } from "@/components/install-chart"
import { VotingButtons } from "@/components/voting-buttons"

export default function PluginDetailPage() {
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

      <main className="container mx-auto px-4 py-8">
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div className="lg:col-span-2 space-y-6">
            <div>
              <div className="flex items-start justify-between mb-4">
                <div>
                  <h1 className="text-3xl font-bold mb-2">Advanced Data Tables</h1>
                  <p className="text-muted-foreground text-lg text-pretty">
                    Powerful data table component with sorting, filtering, and pagination built-in
                  </p>
                </div>
              </div>

              <div className="flex flex-wrap items-center gap-4 mb-6">
                <div className="flex items-center gap-2">
                  <Avatar className="h-8 w-8">
                    <AvatarImage src="/developer-avatar.png" />
                    <AvatarFallback>SC</AvatarFallback>
                  </Avatar>
                  <Link href="/profile/sarah-chen" className="text-sm font-medium hover:underline">
                    Sarah Chen
                  </Link>
                </div>
                <Separator orientation="vertical" className="h-6" />
                <Badge variant="outline">UI Components</Badge>
                <Separator orientation="vertical" className="h-6" />
                <div className="flex items-center gap-1.5 text-sm text-muted-foreground">
                  <Download className="h-4 w-4" />
                  <span>12,453 installs</span>
                </div>
                <div className="flex items-center gap-1.5 text-sm text-muted-foreground">
                  <Star className="h-4 w-4 fill-yellow-400 text-yellow-400" />
                  <span>4.8 rating</span>
                </div>
              </div>

              <div className="flex flex-wrap gap-2 mb-6">
                {["react", "typescript", "tables", "ui-components", "sorting", "filtering"].map((tag) => (
                  <Badge key={tag} variant="secondary">
                    {tag}
                  </Badge>
                ))}
              </div>
            </div>

            <Card>
              <CardHeader>
                <CardTitle>Installation</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="bg-muted rounded-lg p-4 font-mono text-sm">
                  npm install @pluginhub/advanced-data-tables
                </div>
                <p className="text-sm text-muted-foreground mt-3">
                  View the{" "}
                  <a href="#" className="text-primary hover:underline">
                    installation guide
                  </a>{" "}
                  for detailed setup instructions.
                </p>
              </CardContent>
            </Card>

            <Tabs defaultValue="overview" className="w-full">
              <TabsList className="w-full justify-start">
                <TabsTrigger value="overview">Overview</TabsTrigger>
                <TabsTrigger value="versions">Versions</TabsTrigger>
                <TabsTrigger value="analytics">Analytics</TabsTrigger>
              </TabsList>
              <TabsContent value="overview" className="space-y-4">
                <Card>
                  <CardHeader>
                    <CardTitle>About</CardTitle>
                  </CardHeader>
                  <CardContent className="prose prose-sm max-w-none">
                    <p>
                      Advanced Data Tables is a comprehensive solution for displaying and managing tabular data in your
                      application. Built with performance and flexibility in mind, it handles large datasets with ease.
                    </p>
                    <h3>Features</h3>
                    <ul>
                      <li>Multi-column sorting with custom sort functions</li>
                      <li>Advanced filtering with search and column-specific filters</li>
                      <li>Pagination with customizable page sizes</li>
                      <li>Row selection and bulk actions</li>
                      <li>Responsive design that works on all screen sizes</li>
                      <li>Fully typed with TypeScript</li>
                      <li>Accessible and keyboard navigable</li>
                    </ul>
                    <h3>Quick Start</h3>
                    <p>
                      Import the component and pass your data. The table will automatically handle sorting, filtering,
                      and pagination based on your configuration.
                    </p>
                  </CardContent>
                </Card>
              </TabsContent>
              <TabsContent value="versions">
                <Card>
                  <CardHeader>
                    <CardTitle>Version History</CardTitle>
                  </CardHeader>
                  <CardContent>
                    <div className="space-y-4">
                      {[
                        { version: "2.3.1", date: "2025-01-15", changes: "Bug fixes and performance improvements" },
                        {
                          version: "2.3.0",
                          date: "2025-01-08",
                          changes: "Added custom cell renderers and improved TypeScript types",
                        },
                        {
                          version: "2.2.0",
                          date: "2024-12-20",
                          changes: "New filtering options and accessibility enhancements",
                        },
                        {
                          version: "2.1.0",
                          date: "2024-12-01",
                          changes: "Performance optimizations for large datasets",
                        },
                      ].map((v) => (
                        <div key={v.version} className="flex items-start gap-4 pb-4 border-b last:border-0">
                          <Badge variant="outline" className="shrink-0">
                            v{v.version}
                          </Badge>
                          <div className="flex-1 min-w-0">
                            <p className="text-sm font-medium">{v.changes}</p>
                            <p className="text-xs text-muted-foreground mt-1">{v.date}</p>
                          </div>
                        </div>
                      ))}
                    </div>
                  </CardContent>
                </Card>
              </TabsContent>
              <TabsContent value="analytics">
                <Card>
                  <CardHeader>
                    <CardTitle>Install Trends</CardTitle>
                  </CardHeader>
                  <CardContent>
                    <InstallChart />
                  </CardContent>
                </Card>
              </TabsContent>
            </Tabs>
          </div>

          <div className="space-y-4">
            <Card>
              <CardContent className="pt-6 space-y-3">
                <Button className="w-full gap-2" size="lg">
                  <Download className="h-4 w-4" />
                  Install Component
                </Button>
                <Button variant="outline" className="w-full gap-2 bg-transparent" asChild>
                  <a href="https://github.com" target="_blank" rel="noopener noreferrer">
                    <Github className="h-4 w-4" />
                    View on GitHub
                  </a>
                </Button>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle className="text-base">Community Feedback</CardTitle>
              </CardHeader>
              <CardContent>
                <VotingButtons upvotes={234} downvotes={12} />
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle className="text-base">Information</CardTitle>
              </CardHeader>
              <CardContent className="space-y-3 text-sm">
                <div className="flex items-center gap-2">
                  <Package className="h-4 w-4 text-muted-foreground" />
                  <span className="text-muted-foreground">Version:</span>
                  <span className="font-medium">2.3.1</span>
                </div>
                <div className="flex items-center gap-2">
                  <Calendar className="h-4 w-4 text-muted-foreground" />
                  <span className="text-muted-foreground">Updated:</span>
                  <span className="font-medium">Jan 15, 2025</span>
                </div>
                <div className="flex items-center gap-2">
                  <ExternalLink className="h-4 w-4 text-muted-foreground" />
                  <span className="text-muted-foreground">License:</span>
                  <span className="font-medium">MIT</span>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle className="text-base">Links</CardTitle>
              </CardHeader>
              <CardContent className="space-y-2">
                <a href="#" className="flex items-center gap-2 text-sm text-primary hover:underline">
                  <ExternalLink className="h-3.5 w-3.5" />
                  Documentation
                </a>
                <a href="#" className="flex items-center gap-2 text-sm text-primary hover:underline">
                  <Github className="h-3.5 w-3.5" />
                  Report Issue
                </a>
                <a href="#" className="flex items-center gap-2 text-sm text-primary hover:underline">
                  <ExternalLink className="h-3.5 w-3.5" />
                  Changelog
                </a>
              </CardContent>
            </Card>
          </div>
        </div>
      </main>
    </div>
  )
}
