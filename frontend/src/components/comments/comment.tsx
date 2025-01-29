import { formatDistanceToNow } from 'date-fns';

interface CommentProps {
  comment: {
    id: number;
    content: string;
    author: string;
    createdAt: string;
    likes: number;
    dislikes: number;
  };
  onLike: () => void;
  onDislike: () => void;
}

export const Comment: React.FC<CommentProps> = ({ comment, onLike, onDislike }) => {
  return (
    <div className="p-4 bg-gray-900 border rounded-lg border-green-500/30">
      <div className="flex items-start justify-between">
        <div className="flex items-center gap-2">
          <span className="font-medium text-green-400">{comment.author}</span>
          <span className="text-sm text-green-300/60">
            {formatDistanceToNow(new Date(comment.createdAt), { addSuffix: true })}
          </span>
        </div>
      </div>
      <p className="mt-2 text-green-300/80">{comment.content}</p>
      <div className="flex gap-4 mt-4">
        <button
          onClick={onLike}
          className="flex items-center gap-2 px-3 py-1 text-sm text-green-400 border rounded-md border-green-500/30 hover:bg-gray-800"
        >
          <span>👍</span>
          <span>{comment.likes}</span>
        </button>
        <button
          onClick={onDislike}
          className="flex items-center gap-2 px-3 py-1 text-sm text-green-400 border rounded-md border-green-500/30 hover:bg-gray-800"
        >
          <span>👎</span>
          <span>{comment.dislikes}</span>
        </button>
      </div>
    </div>
  );
}; 