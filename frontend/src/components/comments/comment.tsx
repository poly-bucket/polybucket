import { useState } from 'react';
import { formatDistanceToNow } from 'date-fns';
import { Comment as CommentType } from '../../services/comments.service';
import { ReportDialog } from '../report-dialog/report-dialog';
import { ReportType } from '../../services/reports.service';

interface CommentProps {
  comment: CommentType;
  onLike: () => void;
  onDislike: () => void;
}

export const Comment: React.FC<CommentProps> = ({ comment, onLike, onDislike }) => {
  const [isReportDialogOpen, setIsReportDialogOpen] = useState(false);

  return (
    <>
      <div className="p-4 bg-gray-900 border rounded-lg border-green-500/30">
        <div className="flex items-start justify-between">
          <div className="flex items-center gap-2">
            <span className="font-medium text-green-400">{comment.author.username}</span>
            <span className="text-sm text-green-300/60">
              {formatDistanceToNow(new Date(comment.createdAt), { addSuffix: true })}
            </span>
          </div>
          <button
            onClick={() => setIsReportDialogOpen(true)}
            className="text-sm text-green-400/60 hover:text-red-500"
            title="Report comment"
          >
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 21v-4m0 0V5a2 2 0 012-2h6.5l1 1H21l-3 6 3 6h-8.5l-1-1H5a2 2 0 00-2 2zm9-13.5V9" />
            </svg>
          </button>
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

      <ReportDialog
        isOpen={isReportDialogOpen}
        onClose={() => setIsReportDialogOpen(false)}
        type={ReportType.Comment}
        targetId={comment.id}
        targetName={`Comment by ${comment.author.username}`}
      />
    </>
  );
}; 