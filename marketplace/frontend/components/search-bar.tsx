"use client"

import { Input } from "@/components/ui/input"
import { Search, X } from "lucide-react"
import { useState, useEffect } from "react"

interface SearchBarProps {
  onSearch: (search: string) => void;
  placeholder?: string;
  className?: string;
}

export function SearchBar({ onSearch, placeholder = "Search components by name, tags, or keywords...", className }: SearchBarProps) {
  const [searchTerm, setSearchTerm] = useState("")

  // Debounce search to avoid too many API calls
  useEffect(() => {
    const timer = setTimeout(() => {
      onSearch(searchTerm)
    }, 300)

    return () => clearTimeout(timer)
  }, [searchTerm, onSearch])

  const handleClear = () => {
    setSearchTerm("")
    onSearch("")
  }

  return (
    <div className={`relative ${className}`}>
      <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
      <Input 
        placeholder={placeholder}
        className="pl-10 pr-10 h-11"
        value={searchTerm}
        onChange={(e) => setSearchTerm(e.target.value)}
      />
      {searchTerm && (
        <button
          onClick={handleClear}
          className="absolute right-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground hover:text-foreground"
        >
          <X className="h-4 w-4" />
        </button>
      )}
    </div>
  )
}
