import { useState } from 'react';
import { Comment } from './comment';
import { NewCommentForm } from './new-comment-form';

interface CommentSectionProps {
  modelId: number;
}

export const CommentSection: React.FC<CommentSectionProps> = ({ modelId }) => {
  // This would normally come from an API
  const [comments, setComments] = useState([
    {
      id: 1,
      content: "This is an amazing model! The details are incredible.",
      author: "Jane Smith",
      createdAt: "2024-03-15T10:30:00",
      likes: 12,
      dislikes: modelId,
    },
    // Add more sample comments...
  ]);

  const handleNewComment = (content: string) => {
    // This would normally be an API call
    const newComment = {
      id: comments.length + 1,
      content,
      author: "Current User",
      createdAt: new Date().toISOString(),
      likes: 0,
      dislikes: 0,
    };
    setComments([newComment, ...comments]);
  };

  const handleLike = (commentId: number) => {
    // This would normally be an API call
    setComments(comments.map(comment => 
      comment.id === commentId 
        ? { ...comment, likes: comment.likes + 1 }
        : comment
    ));
  };

  const handleDislike = (commentId: number) => {
    // This would normally be an API call
    setComments(comments.map(comment => 
      comment.id === commentId 
        ? { ...comment, dislikes: comment.dislikes + 1 }
        : comment
    ));
  };

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-semibold text-green-400">Comments</h2>
      <NewCommentForm onSubmit={handleNewComment} />
      <div className="space-y-4">
        {comments.map((comment) => (
          <Comment
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