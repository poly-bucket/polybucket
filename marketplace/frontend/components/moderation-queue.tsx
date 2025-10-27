"use client"

import { Card, CardContent, CardFooter, CardHeader } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { Textarea } from "@/components/ui/textarea"
import { Github, ExternalLink, CheckCircle, XCircle } from "lucide-react"
import { useState } from "react"

const PENDING_SUBMISSIONS = [
  {
    id: "1",
    name: "Real-time Collaboration Kit",
    description: "Add real-time collaboration features to your app with WebSocket support",
    author: "Alex Thompson",
    authorAvatar: "/developer-avatar-7.jpg",
    submittedDate: "2025-01-20",
    version: "1.0.0",
    category: "Integrations",
    tags: ["websocket", "collaboration", "real-time"],
    githubUrl: "https://github.com/alexthompson/collab-kit",
  },
  {
    id: "2",
    name: "PDF Generator Pro",
    description: "Generate professional PDFs from your application data with custom templates",
    author: "Maria Garcia",
    authorAvatar: "/developer-avatar-8.jpg",
    submittedDate: "2025-01-19",
    version: "2.1.0",
    category: "Productivity",
    tags: ["pdf", "export", "templates"],
    githubUrl: "https://github.com/mariagarcia/pdf-gen",
  },
]

export function ModerationQueue() {
  const [feedback, setFeedback] = useState<Record<string, string>>({})

  return (
    <div className="space-y-4">
      {PENDING_SUBMISSIONS.map((submission) => (
        <Card key={submission.id}>
          <CardHeader>
            <div className="flex items-start justify-between gap-4">
              <div className="flex-1">
                <div className="flex items-center gap-3 mb-2">
                  <h3 className="font-semibold text-xl">{submission.name}</h3>
                  <Badge variant="outline">v{submission.version}</Badge>
                  <Badge variant="secondary">{submission.category}</Badge>
                </div>
                <p className="text-muted-foreground text-pretty mb-3">{submission.description}</p>
                <div className="flex flex-wrap gap-1.5">
                  {submission.tags.map((tag) => (
                    <Badge key={tag} variant="secondary" className="text-xs">
                      {tag}
                    </Badge>
                  ))}
                </div>
              </div>
            </div>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="flex items-center justify-between text-sm">
              <div className="flex items-center gap-2">
                <Avatar className="h-6 w-6">
                  <AvatarImage src={submission.authorAvatar || "/placeholder.svg"} />
                  <AvatarFallback>{submission.author[0]}</AvatarFallback>
                </Avatar>
                <span className="text-muted-foreground">Submitted by</span>
                <span className="font-medium">{submission.author}</span>
              </div>
              <span className="text-muted-foreground">{new Date(submission.submittedDate).toLocaleDateString()}</span>
            </div>

            <div className="flex gap-2">
              <Button variant="outline" size="sm" className="gap-2 bg-transparent" asChild>
                <a href={submission.githubUrl} target="_blank" rel="noopener noreferrer">
                  <Github className="h-4 w-4" />
                  View Repository
                </a>
              </Button>
              <Button variant="outline" size="sm" className="gap-2 bg-transparent">
                <ExternalLink className="h-4 w-4" />
                View README
              </Button>
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium">Feedback (optional)</label>
              <Textarea
                placeholder="Add notes or feedback for the submitter..."
                value={feedback[submission.id] || ""}
                onChange={(e) => setFeedback({ ...feedback, [submission.id]: e.target.value })}
                rows={2}
              />
            </div>
          </CardContent>
          <CardFooter className="border-t pt-4 flex gap-2">
            <Button className="gap-2 flex-1" variant="default">
              <CheckCircle className="h-4 w-4" />
              Approve
            </Button>
            <Button className="gap-2 flex-1" variant="destructive">
              <XCircle className="h-4 w-4" />
              Reject
            </Button>
          </CardFooter>
        </Card>
      ))}
    </div>
  )
}
