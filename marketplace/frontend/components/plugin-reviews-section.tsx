"use client"

import { useCallback, useEffect, useState } from "react"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Textarea } from "@/components/ui/textarea"
import { Label } from "@/components/ui/label"
import { apiClient, type PluginReviewItem } from "@/lib/api"
import { useAuth } from "@/context/AuthContext"
import { toast } from "sonner"
import Link from "next/link"

interface PluginReviewsSectionProps {
  pluginId: string
}

export function PluginReviewsSection({ pluginId }: PluginReviewsSectionProps) {
  const { isAuthenticated, user } = useAuth()
  const [reviews, setReviews] = useState<PluginReviewItem[]>([])
  const [averageRating, setAverageRating] = useState(0)
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [rating, setRating] = useState(5)
  const [title, setTitle] = useState("")
  const [content, setContent] = useState("")
  const [editingId, setEditingId] = useState<string | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    try {
      const data = await apiClient.getPluginReviews(pluginId)
      setReviews(data.reviews)
      setAverageRating(data.averageRating)
    } catch {
      setReviews([])
    } finally {
      setLoading(false)
    }
  }, [pluginId])

  useEffect(() => {
    load()
  }, [load])

  const myReview = reviews.find((r) => user && r.userId === user.id)

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault()
    if (!isAuthenticated) {
      toast.error("Sign in to leave a review")
      return
    }

    setSubmitting(true)
    try {
      if (editingId) {
        await apiClient.updatePluginReview(pluginId, editingId, { rating, title, content })
        toast.success("Review updated")
      } else {
        await apiClient.createPluginReview(pluginId, { rating, title, content })
        toast.success("Review submitted")
      }
      setTitle("")
      setContent("")
      setEditingId(null)
      await load()
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Review failed")
    } finally {
      setSubmitting(false)
    }
  }

  const startEdit = (review: PluginReviewItem) => {
    setEditingId(review.id)
    setRating(review.rating)
    setTitle(review.title)
    setContent(review.content)
  }

  const handleDelete = async (reviewId: string) => {
    try {
      await apiClient.deletePluginReview(pluginId, reviewId)
      toast.success("Review deleted")
      await load()
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Delete failed")
    }
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Reviews</CardTitle>
        <p className="text-sm text-muted-foreground">
          {loading ? "Loading…" : `${reviews.length} reviews · ${averageRating.toFixed(1)} average`}
        </p>
      </CardHeader>
      <CardContent className="space-y-6">
        {isAuthenticated && !myReview && !editingId ? (
          <form className="space-y-3 border rounded-lg p-4" onSubmit={handleSubmit}>
            <div className="space-y-2">
              <Label htmlFor="rating">Rating (1–5)</Label>
              <Input
                id="rating"
                type="number"
                min={1}
                max={5}
                value={rating}
                onChange={(e) => setRating(Number(e.target.value))}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="review-title">Title</Label>
              <Input id="review-title" value={title} onChange={(e) => setTitle(e.target.value)} required />
            </div>
            <div className="space-y-2">
              <Label htmlFor="review-content">Review</Label>
              <Textarea id="review-content" value={content} onChange={(e) => setContent(e.target.value)} required rows={3} />
            </div>
            <Button type="submit" disabled={submitting}>
              {submitting ? "Saving…" : "Submit review"}
            </Button>
          </form>
        ) : !isAuthenticated ? (
          <p className="text-sm text-muted-foreground">
            <Link href="/login" className="text-primary hover:underline">
              Sign in
            </Link>{" "}
            to write a review.
          </p>
        ) : null}

        {editingId ? (
          <form className="space-y-3 border rounded-lg p-4" onSubmit={handleSubmit}>
            <p className="text-sm font-medium">Edit your review</p>
            <Input type="number" min={1} max={5} value={rating} onChange={(e) => setRating(Number(e.target.value))} />
            <Input value={title} onChange={(e) => setTitle(e.target.value)} required />
            <Textarea value={content} onChange={(e) => setContent(e.target.value)} required rows={3} />
            <div className="flex gap-2">
              <Button type="submit" disabled={submitting}>
                Save
              </Button>
              <Button type="button" variant="outline" onClick={() => setEditingId(null)}>
                Cancel
              </Button>
            </div>
          </form>
        ) : null}

        <div className="space-y-4">
          {reviews.map((review) => (
            <div key={review.id} className="border-b pb-4 last:border-0">
              <div className="flex items-center justify-between gap-2">
                <p className="font-medium">{review.username}</p>
                <span className="text-sm text-muted-foreground">{review.rating}/5</span>
              </div>
              {review.title ? <p className="font-semibold text-sm mt-1">{review.title}</p> : null}
              <p className="text-sm text-muted-foreground mt-1 text-pretty">{review.content}</p>
              {user && review.userId === user.id && !editingId ? (
                <div className="flex gap-2 mt-2">
                  <Button type="button" variant="ghost" size="sm" onClick={() => startEdit(review)}>
                    Edit
                  </Button>
                  <Button type="button" variant="ghost" size="sm" onClick={() => handleDelete(review.id)}>
                    Delete
                  </Button>
                </div>
              ) : null}
            </div>
          ))}
          {!loading && reviews.length === 0 ? (
            <p className="text-sm text-muted-foreground">No reviews yet.</p>
          ) : null}
        </div>
      </CardContent>
    </Card>
  )
}
