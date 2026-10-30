"use client"

import { Card, CardContent, CardFooter, CardHeader } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import { Textarea } from "@/components/ui/textarea"
import { Github, CheckCircle, XCircle } from "lucide-react"
import { useCallback, useEffect, useState } from "react"
import { apiClient, type SubmissionSummary } from "@/lib/api"
import { toast } from "sonner"

interface ModerationQueueProps {
  onCountsChange?: (counts: { pending: number }) => void
}

export function ModerationQueue({ onCountsChange }: ModerationQueueProps) {
  const [submissions, setSubmissions] = useState<SubmissionSummary[]>([])
  const [loading, setLoading] = useState(true)
  const [feedback, setFeedback] = useState<Record<string, string>>({})
  const [actionId, setActionId] = useState<string | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    try {
      const items = await apiClient.getSubmissions("pending")
      setSubmissions(items)
      onCountsChange?.({ pending: items.length })
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Failed to load submissions")
    } finally {
      setLoading(false)
    }
  }, [onCountsChange])

  useEffect(() => {
    load()
  }, [load])

  const handleApprove = async (id: string) => {
    setActionId(id)
    try {
      const result = await apiClient.approveSubmission(id)
      if (!result.success) {
        toast.error(result.message)
        return
      }
      toast.success(result.message)
      await load()
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Approve failed")
    } finally {
      setActionId(null)
    }
  }

  const handleReject = async (id: string) => {
    setActionId(id)
    try {
      await apiClient.rejectSubmission(id, feedback[id] ?? "")
      toast.success("Submission rejected")
      await load()
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Reject failed")
    } finally {
      setActionId(null)
    }
  }

  if (loading) {
    return <p className="text-muted-foreground">Loading pending submissions…</p>
  }

  if (submissions.length === 0) {
    return (
      <div className="rounded-lg border p-8 text-center text-muted-foreground">
        No submissions are waiting for review.
      </div>
    )
  }

  return (
    <div className="space-y-4">
      {submissions.map((submission) => (
        <Card key={submission.id}>
          <CardHeader>
            <div className="flex items-start justify-between gap-4">
              <div className="flex-1">
                <div className="flex items-center gap-3 mb-2">
                  <h3 className="font-semibold text-xl">{submission.name}</h3>
                  <Badge variant="secondary">{submission.category}</Badge>
                </div>
                <p className="text-muted-foreground text-pretty mb-3">{submission.description}</p>
              </div>
            </div>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="flex items-center justify-between text-sm">
              <span className="text-muted-foreground">
                Submitted {new Date(submission.submittedAt).toLocaleString()}
              </span>
            </div>

            <div className="flex gap-2">
              <Button variant="outline" size="sm" className="gap-2 bg-transparent" asChild>
                <a href={submission.repositoryUrl} target="_blank" rel="noopener noreferrer">
                  <Github className="h-4 w-4" />
                  View Repository
                </a>
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
            <Button
              className="gap-2 flex-1"
              variant="default"
              disabled={actionId === submission.id}
              onClick={() => handleApprove(submission.id)}
            >
              <CheckCircle className="h-4 w-4" />
              Approve
            </Button>
            <Button
              className="gap-2 flex-1"
              variant="destructive"
              disabled={actionId === submission.id}
              onClick={() => handleReject(submission.id)}
            >
              <XCircle className="h-4 w-4" />
              Reject
            </Button>
          </CardFooter>
        </Card>
      ))}
    </div>
  )
}
