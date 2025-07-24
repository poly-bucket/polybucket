import React, { useState, useEffect } from 'react';
import { usePlugin, PluginComponentProps } from '../PluginAPI';

interface Comment {
  id: string;
  content: string;
  authorId: string;
  authorUsername: string;
  likes: number;
  dislikes: number;
  isEdited: boolean;
  isModerated: boolean;
  isHidden: boolean;
  parentCommentId?: string;
  createdAt: string;
  lastEditedAt?: string;
  userHasLiked: boolean;
  userHasDisliked: boolean;
  canEdit: boolean;
  canDelete: boolean;
  replies: Comment[];
}

interface CommentTarget {
  targetId: string;
  targetType: 'model' | 'user' | 'collection' | 'report';
}

interface EnhancedCommentsWidgetProps extends PluginComponentProps {
  targetId: string;
  targetType: 'model' | 'user' | 'collection' | 'report';
  readonly?: boolean;
  maxComments?: number;
  allowReplies?: boolean;
  showStatistics?: boolean;
}

const EnhancedCommentsWidget: React.FC<EnhancedCommentsWidgetProps> = ({
  pluginId,
  componentId,
  targetId,
  targetType,
  readonly = false,
  maxComments = 20,
  allowReplies = true,
  showStatistics = true
}) => {
  const plugin = usePlugin(pluginId);
  
  const [comments, setComments] = useState<Comment[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string>('');
  const [newComment, setNewComment] = useState('');
  const [replyingTo, setReplyingTo] = useState<string | null>(null);
  const [editingComment, setEditingComment] = useState<string | null>(null);
  const [editContent, setEditContent] = useState('');
  const [statistics, setStatistics] = useState<any>(null);

  const target: CommentTarget = { targetId, targetType };

  useEffect(() => {
    loadComments();
    if (showStatistics) {
      loadStatistics();
    }
  }, [targetId, targetType]);

  const loadComments = async () => {
    try {
      setLoading(true);
      const response = await plugin.api.makeRequest(`/comments/target/${targetType}/${targetId}?page=1&pageSize=${maxComments}`);
      if (response.ok) {
        const data = await response.json();
        setComments(data.comments || []);
      } else {
        setError('Failed to load comments');
      }
    } catch (err) {
      setError('Failed to load comments');
      console.error('Error loading comments:', err);
    } finally {
      setLoading(false);
    }
  };

  const loadStatistics = async () => {
    try {
      const response = await plugin.api.makeRequest(`/comments/statistics/${targetType}/${targetId}`);
      if (response.ok) {
        const stats = await response.json();
        setStatistics(stats);
      }
    } catch (err) {
      console.error('Error loading statistics:', err);
    }
  };

  const handleAddComment = async (content: string, parentId?: string) => {
    if (!content.trim()) return;

    try {
      const response = await plugin.api.makeRequest('/comments', {
        method: 'POST',
        body: JSON.stringify({
          target: { targetId, targetType },
          content: content.trim(),
          parentCommentId: parentId
        })
      });

      if (response.ok) {
        await loadComments();
        setNewComment('');
        setReplyingTo(null);
        plugin.showNotification('Comment added successfully', 'success');
      } else {
        plugin.showNotification('Failed to add comment', 'error');
      }
    } catch (err) {
      plugin.showNotification('Failed to add comment', 'error');
      console.error('Error adding comment:', err);
    }
  };

  const handleEditComment = async (commentId: string, content: string) => {
    try {
      const response = await plugin.api.makeRequest(`/comments/${commentId}`, {
        method: 'PUT',
        body: JSON.stringify({ content: content.trim() })
      });

      if (response.ok) {
        await loadComments();
        setEditingComment(null);
        setEditContent('');
        plugin.showNotification('Comment updated successfully', 'success');
      } else {
        plugin.showNotification('Failed to update comment', 'error');
      }
    } catch (err) {
      plugin.showNotification('Failed to update comment', 'error');
      console.error('Error updating comment:', err);
    }
  };

  const handleDeleteComment = async (commentId: string) => {
    if (!window.confirm('Are you sure you want to delete this comment?')) return;

    try {
      const response = await plugin.api.makeRequest(`/comments/${commentId}`, {
        method: 'DELETE'
      });

      if (response.ok) {
        await loadComments();
        plugin.showNotification('Comment deleted successfully', 'success');
      } else {
        plugin.showNotification('Failed to delete comment', 'error');
      }
    } catch (err) {
      plugin.showNotification('Failed to delete comment', 'error');
      console.error('Error deleting comment:', err);
    }
  };

  const handleLikeComment = async (commentId: string) => {
    try {
      const response = await plugin.api.makeRequest(`/comments/${commentId}/like`, {
        method: 'POST'
      });

      if (response.ok) {
        await loadComments();
      }
    } catch (err) {
      console.error('Error liking comment:', err);
    }
  };

  const handleDislikeComment = async (commentId: string) => {
    try {
      const response = await plugin.api.makeRequest(`/comments/${commentId}/dislike`, {
        method: 'POST'
      });

      if (response.ok) {
        await loadComments();
      }
    } catch (err) {
      console.error('Error disliking comment:', err);
    }
  };

  const handleReportComment = async (commentId: string, reason: string) => {
    try {
      const response = await plugin.api.makeRequest(`/comments/${commentId}/report`, {
        method: 'POST',
        body: JSON.stringify({ reason })
      });

      if (response.ok) {
        plugin.showNotification('Comment reported successfully', 'success');
      } else {
        plugin.showNotification('Failed to report comment', 'error');
      }
    } catch (err) {
      plugin.showNotification('Failed to report comment', 'error');
      console.error('Error reporting comment:', err);
    }
  };

  const renderComment = (comment: Comment, depth: number = 0) => {
    const isEditing = editingComment === comment.id;
    const maxDepth = 3;

    return (
      <div 
        key={comment.id} 
        className={`comment border-l-2 border-gray-200 pl-4 mb-4 ${depth > 0 ? 'ml-6' : ''}`}
        style={{ marginLeft: depth > 0 ? `${depth * 20}px` : '0' }}
      >
        <div className="comment-header flex items-center justify-between mb-2">
          <div className="flex items-center space-x-2">
            <span className="font-medium text-gray-900">{comment.authorUsername}</span>
            <span className="text-sm text-gray-500">
              {new Date(comment.createdAt).toLocaleString()}
            </span>
            {comment.isEdited && (
              <span className="text-xs text-gray-400">(edited)</span>
            )}
            {comment.isModerated && (
              <span className="text-xs bg-red-100 text-red-800 px-2 py-1 rounded">Moderated</span>
            )}
          </div>
          
          <div className="flex items-center space-x-2">
            {!readonly && (
              <>
                <button
                  onClick={() => handleLikeComment(comment.id)}
                  className={`text-sm px-2 py-1 rounded ${
                    comment.userHasLiked 
                      ? 'bg-green-100 text-green-800' 
                      : 'text-gray-600 hover:bg-gray-100'
                  }`}
                >
                  👍 {comment.likes}
                </button>
                
                <button
                  onClick={() => handleDislikeComment(comment.id)}
                  className={`text-sm px-2 py-1 rounded ${
                    comment.userHasDisliked 
                      ? 'bg-red-100 text-red-800' 
                      : 'text-gray-600 hover:bg-gray-100'
                  }`}
                >
                  👎 {comment.dislikes}
                </button>
              </>
            )}
          </div>
        </div>

        <div className="comment-content mb-2">
          {isEditing ? (
            <div className="space-y-2">
              <textarea
                value={editContent}
                onChange={(e) => setEditContent(e.target.value)}
                className="w-full p-2 border border-gray-300 rounded-md resize-none"
                rows={3}
                placeholder="Edit your comment..."
              />
              <div className="flex space-x-2">
                <button
                  onClick={() => handleEditComment(comment.id, editContent)}
                  className="px-3 py-1 bg-blue-500 text-white rounded text-sm hover:bg-blue-600"
                  disabled={!editContent.trim()}
                >
                  Save
                </button>
                <button
                  onClick={() => {
                    setEditingComment(null);
                    setEditContent('');
                  }}
                  className="px-3 py-1 bg-gray-500 text-white rounded text-sm hover:bg-gray-600"
                >
                  Cancel
                </button>
              </div>
            </div>
          ) : (
            <p className="text-gray-800">{comment.content}</p>
          )}
        </div>

        {!readonly && !comment.isModerated && (
          <div className="comment-actions flex space-x-2 text-sm">
            {allowReplies && depth < maxDepth && (
              <button
                onClick={() => setReplyingTo(replyingTo === comment.id ? null : comment.id)}
                className="text-blue-600 hover:text-blue-800"
              >
                Reply
              </button>
            )}
            
            {comment.canEdit && (
              <button
                onClick={() => {
                  setEditingComment(comment.id);
                  setEditContent(comment.content);
                }}
                className="text-green-600 hover:text-green-800"
              >
                Edit
              </button>
            )}
            
            {comment.canDelete && (
              <button
                onClick={() => handleDeleteComment(comment.id)}
                className="text-red-600 hover:text-red-800"
              >
                Delete
              </button>
            )}
            
            <button
              onClick={() => {
                const reason = window.prompt('Why are you reporting this comment?');
                if (reason) handleReportComment(comment.id, reason);
              }}
              className="text-orange-600 hover:text-orange-800"
            >
              Report
            </button>
          </div>
        )}

        {replyingTo === comment.id && (
          <div className="mt-3 p-3 bg-gray-50 rounded-md">
            <textarea
              value={newComment}
              onChange={(e) => setNewComment(e.target.value)}
              className="w-full p-2 border border-gray-300 rounded-md resize-none"
              rows={2}
              placeholder={`Reply to ${comment.authorUsername}...`}
            />
            <div className="flex space-x-2 mt-2">
              <button
                onClick={() => handleAddComment(newComment, comment.id)}
                className="px-3 py-1 bg-blue-500 text-white rounded text-sm hover:bg-blue-600"
                disabled={!newComment.trim()}
              >
                Reply
              </button>
              <button
                onClick={() => {
                  setReplyingTo(null);
                  setNewComment('');
                }}
                className="px-3 py-1 bg-gray-500 text-white rounded text-sm hover:bg-gray-600"
              >
                Cancel
              </button>
            </div>
          </div>
        )}

        {comment.replies.length > 0 && (
          <div className="replies mt-4">
            {comment.replies.map(reply => renderComment(reply, depth + 1))}
          </div>
        )}
      </div>
    );
  };

  if (loading) {
    return (
      <div className="comments-widget p-4">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500"></div>
        <p className="text-gray-600 mt-2">Loading comments...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="comments-widget p-4">
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
          {error}
        </div>
      </div>
    );
  }

  return (
    <div className="comments-widget p-4">
      <div className="comments-header mb-4">
        <h3 className="text-lg font-semibold text-gray-900">
          Comments {comments.length > 0 && `(${comments.length})`}
        </h3>
        
        {showStatistics && statistics && (
          <div className="statistics mt-2 text-sm text-gray-600">
            <span>{statistics.totalComments} comments • </span>
            <span>{statistics.totalLikes} likes • </span>
            <span>{statistics.totalDislikes} dislikes</span>
          </div>
        )}
      </div>

      {!readonly && (
        <div className="add-comment mb-6">
          <textarea
            value={newComment}
            onChange={(e) => setNewComment(e.target.value)}
            className="w-full p-3 border border-gray-300 rounded-md resize-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            rows={3}
            placeholder="Write a comment..."
          />
          <div className="flex justify-between items-center mt-2">
            <span className="text-sm text-gray-500">
              {newComment.length}/2000 characters
            </span>
            <button
              onClick={() => handleAddComment(newComment)}
              className="px-4 py-2 bg-blue-500 text-white rounded-md hover:bg-blue-600 disabled:bg-gray-300 disabled:cursor-not-allowed"
              disabled={!newComment.trim() || newComment.length > 2000}
            >
              Post Comment
            </button>
          </div>
        </div>
      )}

      <div className="comments-list">
        {comments.length === 0 ? (
          <div className="text-center py-8 text-gray-500">
            <p>No comments yet.</p>
            {!readonly && <p>Be the first to comment!</p>}
          </div>
        ) : (
          comments.map(comment => renderComment(comment))
        )}
      </div>
    </div>
  );
};

export default EnhancedCommentsWidget; 