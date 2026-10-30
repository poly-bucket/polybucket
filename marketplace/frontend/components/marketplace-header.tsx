"use client"

import { ThemeToggle } from "@/components/theme-toggle"
import { Button } from "@/components/ui/button"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { Badge } from "@/components/ui/badge"
import { PlusCircle, LogOut } from "lucide-react"
import Link from "next/link"
import { useAuth } from "@/context/AuthContext"

interface MarketplaceHeaderProps {
  showModeratorBadge?: boolean
}

export function MarketplaceHeader({ showModeratorBadge }: MarketplaceHeaderProps) {
  const { user, isAuthenticated, logout, isAdmin } = useAuth()

  const handleLogout = async () => {
    try {
      await logout()
    } catch (error) {
      console.error("Logout error:", error)
    }
  }

  return (
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
              {isAuthenticated && (
                <Link
                  href="/developer"
                  className="text-sm font-medium text-muted-foreground hover:text-primary transition-colors"
                >
                  Developer
                </Link>
              )}
              {isAuthenticated && isAdmin() && (
                <Link
                  href="/moderation"
                  className="text-sm font-medium text-muted-foreground hover:text-primary transition-colors"
                >
                  Moderation
                </Link>
              )}
            </nav>
          </div>
          <div className="flex items-center gap-3">
            {showModeratorBadge && <Badge variant="secondary">Moderator</Badge>}
            <ThemeToggle />
            <Link href="/submit">
              <Button size="sm" className="gap-2">
                <PlusCircle className="h-4 w-4" />
                <span className="hidden sm:inline">Submit Component</span>
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
  )
}
