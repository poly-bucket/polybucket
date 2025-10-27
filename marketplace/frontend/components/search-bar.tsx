"use client"

import { Input } from "@/components/ui/input"
import { Search } from "lucide-react"

export function SearchBar() {
  return (
    <div className="relative">
      <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
      <Input placeholder="Search components by name, tags, or keywords..." className="pl-10 h-11" />
    </div>
  )
}
