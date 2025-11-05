"use client"

import { PluginGrid } from "@/components/plugin-grid"
import { SearchBar } from "@/components/search-bar"
import { CategoryFilter } from "@/components/category-filter"
import { ThemeToggle } from "@/components/theme-toggle"
import { Button } from "@/components/ui/button"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { PlusCircle, User, LogOut } from "lucide-react"
import Link from "next/link"
import { usePluginBrowse } from "@/hooks/usePluginBrowse"
import { useAuth } from "@/context/AuthContext"
import { useState } from "react"

export default function Home() {
  const { setSearch, setCategory } = usePluginBrowse()
  const { user, isAuthenticated, logout } = useAuth()
  const [selectedTags, setSelectedTags] = useState<string[]>([])

  const handleTagChange = (tags: string[]) => {
    setSelectedTags(tags)
    // Note: Tag filtering will be implemented when backend supports it
  }

  const handleLogout = async () => {
    try {
      await logout()
    } catch (error) {
      console.error('Logout error:', error)
    }
  }

  return (
    <div className="min-h-screen bg-background">
      <header className="border-b bg-card/50 backdrop-blur-sm sticky top-0 z-50">
        <div className="container mx-auto px-4 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-8">
              <Link href="/" className="flex items-center gap-2">
                <div className="h-8 w-8 rounded-lg bg-primary" />
                <span className="font-semibold text-xl">Polybucket Marketplace</span>
              </Link>
              <nav className="hidden md:flex items-center gap-6">
                <Link href="/" className="text-sm font-medium hover:text-primary transition-colors">
                  Browse
                </Link>
                <Link
                  href="/submit"
                  className="text-sm font-medium text-muted-foreground hover:text-primary transition-colors"
                >
                  Submit Component
                </Link>
                <Link
                  href="/docs"
                  className="text-sm font-medium text-muted-foreground hover:text-primary transition-colors"
                >
                  Documentation
                </Link>
              </nav>
            </div>
            <div className="flex items-center gap-3">
              <ThemeToggle />
              <Link href="/submit">
                <Button size="sm" className="gap-2">
                  <PlusCircle className="h-4 w-4" />
                  Submit Component
                </Button>
              </Link>
              {isAuthenticated && user ? (
                <div className="flex items-center gap-3">
                  <Link href="/profile">
                    <div className="flex items-center gap-2">
                      <Avatar className="h-8 w-8">
                        <AvatarImage src={user.avatarUrl} alt={user.displayName || user.username} />
                        <AvatarFallback>
                          {(user.displayName || user.username).charAt(0).toUpperCase()}
                        </AvatarFallback>
                      </Avatar>
                      <span className="text-sm font-medium hidden md:block">
                        {user.displayName || user.username}
                      </span>
                    </div>
                  </Link>
                  <Button variant="outline" size="sm" onClick={handleLogout} className="gap-2">
                    <LogOut className="h-4 w-4" />
                    <span className="hidden md:block">Sign Out</span>
                  </Button>
                </div>
              ) : (
                <Link href="/login">
                  <Button variant="outline" size="sm">
                    Sign In
                  </Button>
                </Link>
              )}
            </div>
          </div>
        </div>
      </header>

      <main className="container mx-auto px-4 py-8">
        <div className="mb-8">
          <h1 className="text-4xl font-bold mb-3 text-balance">Discover Components</h1>
          <p className="text-muted-foreground text-lg text-pretty">
            Browse and install community-built components to extend your application
          </p>
        </div>

        <div className="mb-6">
          <SearchBar onSearch={setSearch} />
        </div>

        <div className="flex flex-col lg:flex-row gap-6">
          <aside className="lg:w-64 shrink-0">
            <CategoryFilter 
              onCategoryChange={setCategory}
              onTagChange={handleTagChange}
              selectedTags={selectedTags}
            />
          </aside>
          <div className="flex-1">
            <PluginGrid />
          </div>
        </div>
      </main>
    </div>
  )
}
