import { Card, CardContent } from "@/components/ui/card"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { Button } from "@/components/ui/button"
import { ThemeToggle } from "@/components/theme-toggle"
import { Github, Twitter, Globe } from "lucide-react"
import Link from "next/link"
import { PluginCard } from "@/components/plugin-card"

const USER_PLUGINS = [
  {
    id: "1",
    name: "Advanced Data Tables",
    description: "Powerful data table component with sorting, filtering, and pagination built-in",
    author: "Sarah Chen",
    authorAvatar: "/developer-avatar.png",
    downloads: 12453,
    rating: 4.8,
    version: "2.3.1",
    tags: ["react", "tables", "ui-components"],
    category: "UI Components",
  },
  {
    id: "7",
    name: "State Manager Lite",
    description: "Lightweight state management solution with minimal boilerplate",
    author: "Sarah Chen",
    authorAvatar: "/developer-avatar.png",
    downloads: 5234,
    rating: 4.6,
    version: "1.2.0",
    tags: ["state", "react", "typescript"],
    category: "Productivity",
  },
]

export default function ProfilePage() {
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
        <Card className="mb-8">
          <CardContent className="pt-6">
            <div className="flex flex-col md:flex-row gap-6">
              <Avatar className="h-24 w-24">
                <AvatarImage src="/developer-avatar.png" />
                <AvatarFallback>SC</AvatarFallback>
              </Avatar>
              <div className="flex-1">
                <h1 className="text-3xl font-bold mb-2">Sarah Chen</h1>
                <p className="text-muted-foreground mb-4 text-pretty">
                  Full-stack developer passionate about building tools that make developers' lives easier. Specializing
                  in React, TypeScript, and UI component libraries.
                </p>
                <div className="flex flex-wrap gap-2 mb-4">
                  <Button variant="outline" size="sm" className="gap-2 bg-transparent" asChild>
                    <a href="https://github.com" target="_blank" rel="noopener noreferrer">
                      <Github className="h-4 w-4" />
                      GitHub
                    </a>
                  </Button>
                  <Button variant="outline" size="sm" className="gap-2 bg-transparent" asChild>
                    <a href="https://twitter.com" target="_blank" rel="noopener noreferrer">
                      <Twitter className="h-4 w-4" />
                      Twitter
                    </a>
                  </Button>
                  <Button variant="outline" size="sm" className="gap-2 bg-transparent" asChild>
                    <a href="https://example.com" target="_blank" rel="noopener noreferrer">
                      <Globe className="h-4 w-4" />
                      Website
                    </a>
                  </Button>
                </div>
                <div className="flex flex-wrap gap-4 text-sm">
                  <div>
                    <span className="text-muted-foreground">Components:</span>{" "}
                    <span className="font-semibold">{USER_PLUGINS.length}</span>
                  </div>
                  <div>
                    <span className="text-muted-foreground">Total Downloads:</span>{" "}
                    <span className="font-semibold">17,687</span>
                  </div>
                  <div>
                    <span className="text-muted-foreground">Joined:</span>{" "}
                    <span className="font-semibold">March 2024</span>
                  </div>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>

        <div className="mb-4">
          <h2 className="text-2xl font-bold">Published Components</h2>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {USER_PLUGINS.map((plugin) => (
            <PluginCard key={plugin.id} plugin={plugin} />
          ))}
        </div>
      </main>
    </div>
  )
}
