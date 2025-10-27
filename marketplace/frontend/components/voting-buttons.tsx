"use client"

import { Button } from "@/components/ui/button"
import { ThumbsUp, ThumbsDown } from "lucide-react"
import { useState } from "react"

interface VotingButtonsProps {
  upvotes: number
  downvotes: number
}

export function VotingButtons({ upvotes, downvotes }: VotingButtonsProps) {
  const [vote, setVote] = useState<"up" | "down" | null>(null)

  return (
    <div className="flex items-center gap-2">
      <Button
        variant={vote === "up" ? "default" : "outline"}
        size="sm"
        className="flex-1 gap-2"
        onClick={() => setVote(vote === "up" ? null : "up")}
      >
        <ThumbsUp className="h-4 w-4" />
        {upvotes + (vote === "up" ? 1 : 0)}
      </Button>
      <Button
        variant={vote === "down" ? "default" : "outline"}
        size="sm"
        className="flex-1 gap-2"
        onClick={() => setVote(vote === "down" ? null : "down")}
      >
        <ThumbsDown className="h-4 w-4" />
        {downvotes + (vote === "down" ? 1 : 0)}
      </Button>
    </div>
  )
}
