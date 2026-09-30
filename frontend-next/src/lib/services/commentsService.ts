import axiosInstance from "@/lib/api/axiosConfig";

export type CommentTargetType = "Model" | "UserProfile" | "Collection" | "Report";

export interface CommentTarget {
  targetId: string;
  targetType: CommentTargetType;
}

export interface CommentItem {
  id: string;
  content: string;
  authorId: string;
  authorUsername: string;
  target: CommentTarget;
  likes: number;
  dislikes: number;
  isEdited: boolean;
  isModerated: boolean;
  isHidden: boolean;
  parentCommentId?: string | null;
  createdAt: string;
  lastEditedAt?: string | null;
  userHasLiked: boolean;
  userHasDisliked: boolean;
  canEdit: boolean;
  canDelete: boolean;
  replies: CommentItem[];
}

export interface CommentStatistics {
  totalComments: number;
  totalReplies: number;
  totalLikes: number;
  totalDislikes: number;
  moderatedComments: number;
  lastCommentAt?: string | null;
}

export interface CommentsPage {
  comments: CommentItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  statistics: CommentStatistics;
}

export interface CommentReactionResult {
  likes: number;
  dislikes: number;
  userHasLiked: boolean;
  userHasDisliked: boolean;
  changed: boolean;
}

export type CommentReaction = "like" | "dislike";

export const COMMENT_MAX_LENGTH = 2000;

const targetPath: Record<CommentTargetType, string> = {
  Model: "model",
  UserProfile: "userprofile",
  Collection: "collection",
  Report: "report",
};

export async function getComments(
  target: CommentTarget,
  page = 1,
  pageSize = 20
): Promise<CommentsPage> {
  const { data } = await axiosInstance.get<CommentsPage>(
    `/api/comments/target/${targetPath[target.targetType]}/${target.targetId}`,
    { params: { page, pageSize } }
  );
  return data;
}

export async function createComment(
  target: CommentTarget,
  content: string,
  parentCommentId?: string
): Promise<CommentItem> {
  const { data } = await axiosInstance.post<CommentItem>("/api/comments", {
    target,
    content,
    parentCommentId: parentCommentId ?? null,
  });
  return data;
}

export async function updateComment(commentId: string, content: string): Promise<CommentItem> {
  const { data } = await axiosInstance.put<CommentItem>(`/api/comments/${commentId}`, { content });
  return data;
}

export async function deleteComment(commentId: string): Promise<void> {
  await axiosInstance.delete(`/api/comments/${commentId}`);
}

export async function reactToComment(
  commentId: string,
  reaction: CommentReaction
): Promise<CommentReactionResult> {
  const { data } = await axiosInstance.post<CommentReactionResult>(
    `/api/comments/${commentId}/${reaction}`
  );
  return data;
}

export async function removeCommentReaction(
  commentId: string,
  reaction: CommentReaction
): Promise<CommentReactionResult> {
  const { data } = await axiosInstance.delete<CommentReactionResult>(
    `/api/comments/${commentId}/${reaction}`
  );
  return data;
}

export async function reportComment(commentId: string, reason: string): Promise<void> {
  await axiosInstance.post(`/api/comments/${commentId}/report`, { reason });
}

export function applyReaction(comment: CommentItem, result: CommentReactionResult): CommentItem {
  return {
    ...comment,
    likes: result.likes,
    dislikes: result.dislikes,
    userHasLiked: result.userHasLiked,
    userHasDisliked: result.userHasDisliked,
  };
}

export function updateCommentInTree(
  comments: CommentItem[],
  commentId: string,
  update: (comment: CommentItem) => CommentItem
): CommentItem[] {
  return comments.map((comment) => {
    if (comment.id === commentId) {
      return update(comment);
    }
    if (comment.replies.length === 0) {
      return comment;
    }
    return { ...comment, replies: updateCommentInTree(comment.replies, commentId, update) };
  });
}

export function removeCommentFromTree(comments: CommentItem[], commentId: string): CommentItem[] {
  return comments
    .filter((comment) => comment.id !== commentId)
    .map((comment) =>
      comment.replies.length === 0
        ? comment
        : { ...comment, replies: removeCommentFromTree(comment.replies, commentId) }
    );
}
