"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { toast } from "sonner";
import { Flag, MessageSquare, Reply, ThumbsDown, ThumbsUp, Trash2 } from "lucide-react";
import { useAuth } from "@/contexts/AuthContext";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/primitives/card";
import { Button } from "@/components/primitives/button";
import { Textarea } from "@/components/ui/glass/textarea";
import {
  applyReaction,
  COMMENT_MAX_LENGTH,
  createComment,
  deleteComment,
  getComments,
  reactToComment,
  removeCommentFromTree,
  removeCommentReaction,
  reportComment,
  updateCommentInTree,
  type CommentItem,
  type CommentReaction,
  type CommentTarget,
} from "@/lib/services/commentsService";
import { getAccountApiErrorMessage, isEmailUnverifiedError } from "@/lib/services/accountEmailService";

const PAGE_SIZE = 20;

function commentErrorMessage(error: unknown, fallback: string): string {
  if (isEmailUnverifiedError(error)) {
    return "Verify your email address to join the discussion.";
  }
  return getAccountApiErrorMessage(error, fallback);
}

interface ComposerProps {
  label: string;
  submitLabel: string;
  placeholder: string;
  autoFocus?: boolean;
  onSubmit: (content: string) => Promise<boolean>;
  onCancel?: () => void;
}

function CommentComposer({ label, submitLabel, placeholder, autoFocus, onSubmit, onCancel }: ComposerProps) {
  const [content, setContent] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const trimmed = content.trim();

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    if (!trimmed || submitting) {
      return;
    }
    setSubmitting(true);
    const posted = await onSubmit(trimmed);
    setSubmitting(false);
    if (posted) {
      setContent("");
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-2">
      <Textarea
        aria-label={label}
        value={content}
        maxLength={COMMENT_MAX_LENGTH}
        placeholder={placeholder}
        autoFocus={autoFocus}
        onChange={(event) => setContent(event.target.value)}
        className="min-h-20 text-white"
      />
      <div className="flex items-center justify-between gap-2">
        <span className="text-xs text-white/50">
          {content.length}/{COMMENT_MAX_LENGTH}
        </span>
        <div className="flex gap-2">
          {onCancel && (
            <Button type="button" variant="ghost" size="sm" onClick={onCancel}>
              Cancel
            </Button>
          )}
          <Button type="submit" variant="glass" size="sm" disabled={!trimmed || submitting}>
            {submitting ? "Posting..." : submitLabel}
          </Button>
        </div>
      </div>
    </form>
  );
}

interface CommentRowProps {
  comment: CommentItem;
  isReply?: boolean;
  onReact: (comment: CommentItem, reaction: CommentReaction) => void;
  onReply?: (comment: CommentItem) => void;
  onDelete: (comment: CommentItem) => void;
  onReport: (comment: CommentItem) => void;
}

function CommentRow({ comment, isReply, onReact, onReply, onDelete, onReport }: CommentRowProps) {
  return (
    <article
      aria-label={`Comment by ${comment.authorUsername}`}
      className={isReply ? "border-l border-white/10 pl-4" : undefined}
    >
      <header className="flex flex-wrap items-baseline gap-2 text-sm">
        <Link href={`/profile/${comment.authorId}`} className="font-medium text-white hover:underline">
          {comment.authorUsername}
        </Link>
        <time dateTime={comment.createdAt} className="text-white/50">
          {new Date(comment.createdAt).toLocaleDateString()}
        </time>
        {comment.isEdited && <span className="text-xs text-white/40">(edited)</span>}
      </header>
      <p className="mt-1 whitespace-pre-wrap break-words text-white/80">{comment.content}</p>
      <div className="mt-2 flex flex-wrap items-center gap-1 text-white/60">
        <Button
          type="button"
          variant="ghost"
          size="sm"
          aria-pressed={comment.userHasLiked}
          aria-label={`Like (${comment.likes})`}
          onClick={() => onReact(comment, "like")}
          className={comment.userHasLiked ? "text-white" : undefined}
        >
          <ThumbsUp className="h-4 w-4" />
          <span>{comment.likes}</span>
        </Button>
        <Button
          type="button"
          variant="ghost"
          size="sm"
          aria-pressed={comment.userHasDisliked}
          aria-label={`Dislike (${comment.dislikes})`}
          onClick={() => onReact(comment, "dislike")}
          className={comment.userHasDisliked ? "text-white" : undefined}
        >
          <ThumbsDown className="h-4 w-4" />
          <span>{comment.dislikes}</span>
        </Button>
        {onReply && (
          <Button type="button" variant="ghost" size="sm" onClick={() => onReply(comment)}>
            <Reply className="h-4 w-4" />
            Reply
          </Button>
        )}
        {comment.canDelete ? (
          <Button type="button" variant="ghost" size="sm" onClick={() => onDelete(comment)}>
            <Trash2 className="h-4 w-4" />
            Delete
          </Button>
        ) : (
          <Button type="button" variant="ghost" size="sm" onClick={() => onReport(comment)}>
            <Flag className="h-4 w-4" />
            Report
          </Button>
        )}
      </div>
    </article>
  );
}

export interface ModelCommentsProps {
  modelId: string;
}

export function ModelComments({ modelId }: ModelCommentsProps) {
  const { isAuthenticated } = useAuth();
  const [comments, setComments] = useState<CommentItem[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [loading, setLoading] = useState(false);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [replyingTo, setReplyingTo] = useState<string | null>(null);

  const target: CommentTarget = { targetId: modelId, targetType: "Model" };

  const load = useCallback(
    async (nextPage: number) => {
      setLoading(true);
      setLoadError(null);
      try {
        const result = await getComments({ targetId: modelId, targetType: "Model" }, nextPage, PAGE_SIZE);
        setComments((previous) => (nextPage === 1 ? result.comments : [...previous, ...result.comments]));
        setTotalCount(result.totalCount);
        setTotalPages(result.totalPages);
        setPage(nextPage);
      } catch (error) {
        setLoadError(commentErrorMessage(error, "Comments could not be loaded."));
      } finally {
        setLoading(false);
      }
    },
    [modelId]
  );

  useEffect(() => {
    if (isAuthenticated) {
      load(1);
    }
  }, [isAuthenticated, load]);

  const handleCreate = async (content: string): Promise<boolean> => {
    try {
      const created = await createComment(target, content);
      setComments((previous) => [{ ...created, replies: created.replies ?? [] }, ...previous]);
      setTotalCount((count) => count + 1);
      return true;
    } catch (error) {
      toast.error(commentErrorMessage(error, "Your comment could not be posted."));
      return false;
    }
  };

  const handleReply = async (parent: CommentItem, content: string): Promise<boolean> => {
    try {
      const created = await createComment(target, content, parent.id);
      setComments((previous) =>
        updateCommentInTree(previous, parent.id, (comment) => ({
          ...comment,
          replies: [...comment.replies, { ...created, replies: [] }],
        }))
      );
      setReplyingTo(null);
      return true;
    } catch (error) {
      toast.error(commentErrorMessage(error, "Your reply could not be posted."));
      return false;
    }
  };

  const handleReact = async (comment: CommentItem, reaction: CommentReaction) => {
    const active = reaction === "like" ? comment.userHasLiked : comment.userHasDisliked;
    try {
      const result = active
        ? await removeCommentReaction(comment.id, reaction)
        : await reactToComment(comment.id, reaction);
      setComments((previous) => updateCommentInTree(previous, comment.id, (c) => applyReaction(c, result)));
    } catch (error) {
      toast.error(commentErrorMessage(error, "Your reaction could not be saved."));
    }
  };

  const handleDelete = async (comment: CommentItem) => {
    if (!window.confirm("Delete this comment?")) {
      return;
    }
    try {
      await deleteComment(comment.id);
      setComments((previous) => removeCommentFromTree(previous, comment.id));
      if (!comment.parentCommentId) {
        setTotalCount((count) => Math.max(0, count - 1));
      }
      toast.success("Comment deleted");
    } catch (error) {
      toast.error(commentErrorMessage(error, "The comment could not be deleted."));
    }
  };

  const handleReport = async (comment: CommentItem) => {
    const reason = window.prompt("Why are you reporting this comment?")?.trim();
    if (!reason) {
      return;
    }
    try {
      await reportComment(comment.id, reason);
      toast.success("Thanks, a moderator will review this comment.");
    } catch (error) {
      toast.error(commentErrorMessage(error, "The report could not be sent."));
    }
  };

  return (
    <Card variant="glass" className="border-white/20">
      <CardHeader>
        <CardTitle className="flex items-center gap-2 text-white">
          <MessageSquare className="h-5 w-5" />
          Comments{isAuthenticated && totalCount > 0 ? ` (${totalCount})` : ""}
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-6">
        {!isAuthenticated ? (
          <p className="text-white/70">
            <Link href="/login" className="font-medium text-white underline">
              Sign in
            </Link>{" "}
            to read and join the discussion.
          </p>
        ) : (
          <>
            <CommentComposer
              label="Write a comment"
              submitLabel="Post comment"
              placeholder="Share feedback, print settings, or questions"
              onSubmit={handleCreate}
            />

            {loadError && (
              <div role="alert" className="flex items-center justify-between gap-2 text-sm text-red-300">
                <span>{loadError}</span>
                <Button type="button" variant="outline" size="sm" onClick={() => load(1)}>
                  Retry
                </Button>
              </div>
            )}

            {!loadError && !loading && comments.length === 0 && (
              <p className="text-white/60">No comments yet. Start the conversation.</p>
            )}

            <ul className="space-y-6">
              {comments.map((comment) => (
                <li key={comment.id} className="space-y-3">
                  <CommentRow
                    comment={comment}
                    onReact={handleReact}
                    onReply={(c) => setReplyingTo(replyingTo === c.id ? null : c.id)}
                    onDelete={handleDelete}
                    onReport={handleReport}
                  />
                  {comment.replies.length > 0 && (
                    <ul className="ml-4 space-y-3">
                      {comment.replies.map((reply) => (
                        <li key={reply.id}>
                          <CommentRow
                            comment={reply}
                            isReply
                            onReact={handleReact}
                            onDelete={handleDelete}
                            onReport={handleReport}
                          />
                        </li>
                      ))}
                    </ul>
                  )}
                  {replyingTo === comment.id && (
                    <div className="ml-4">
                      <CommentComposer
                        label={`Reply to ${comment.authorUsername}`}
                        submitLabel="Post reply"
                        placeholder="Write a reply"
                        autoFocus
                        onSubmit={(content) => handleReply(comment, content)}
                        onCancel={() => setReplyingTo(null)}
                      />
                    </div>
                  )}
                </li>
              ))}
            </ul>

            {loading && <p className="text-sm text-white/60">Loading comments...</p>}

            {!loading && page < totalPages && (
              <Button type="button" variant="outline" size="sm" onClick={() => load(page + 1)}>
                Load more comments
              </Button>
            )}
          </>
        )}
      </CardContent>
    </Card>
  );
}
