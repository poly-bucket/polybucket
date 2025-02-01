import { useState, useEffect } from 'react';
import { Comment as CommentComponent } from './comment';
import { NewCommentForm } from './new-comment-form';
import { commentsService, Comment } from '../../services/comments.service';

interface CommentSectionProps {
  modelId: string;
}

export const CommentSection: React.FC<CommentSectionProps> = ({ modelId }) => {
  const [comments, setComments] = useState<Comment[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadComments();
  }, [modelId]);

  const loadComments = async () => {
    try {
      setIsLoading(true);
      const fetchedComments = await commentsService.getCommentsForModel(modelId);
      setComments(fetchedComments);
      setError(null);
    } catch (err) {
      setError('Failed to load comments');
      console.error('Error loading comments:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleNewComment = async (content: string) => {
    try {
      const newComment = await commentsService.addComment(modelId, content);
      setComments([newComment, ...comments]);
    } catch (err) {
      setError('Failed to add comment');
      console.error('Error adding comment:', err);
    }
  };

  const handleLike = async (commentId: string) => {
    try {
      await commentsService.likeComment(commentId);
      setComments(comments.map(comment => 
        comment.id === commentId 
          ? { ...comment, likes: comment.likes + 1 }
          : comment
      ));
    } catch (err) {
      console.error('Error liking comment:', err);
    }
  };

  const handleDislike = async (commentId: string) => {
    try {
      await commentsService.dislikeComment(commentId);
      setComments(comments.map(comment => 
        comment.id === commentId 
          ? { ...comment, dislikes: comment.dislikes + 1 }
          : comment
      ));
    } catch (err) {
      console.error('Error disliking comment:', err);
    }
  };

  if (isLoading) {
    return <div className="text-green-400">Loading comments...</div>;
  }

  if (error) {
    return <div className="text-red-500">{error}</div>;
  }

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-semibold text-green-400">Comments</h2>
      <NewCommentForm onSubmit={handleNewComment} />
      <div className="space-y-4">
        {comments.map((comment) => (
          <CommentComponent
            key={comment.id}
            comment={comment}
            onLike={() => handleLike(comment.id)}
            onDislike={() => handleDislike(comment.id)}
          />
        ))}
      </div>
    </div>
  );
}; 