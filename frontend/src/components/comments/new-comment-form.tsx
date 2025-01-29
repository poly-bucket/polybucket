import { useState } from 'react';

interface NewCommentFormProps {
  onSubmit: (content: string) => void;
}

export const NewCommentForm: React.FC<NewCommentFormProps> = ({ onSubmit }) => {
  const [content, setContent] = useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (content.trim()) {
      onSubmit(content);
      setContent('');
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <textarea
        value={content}
        onChange={(e) => setContent(e.target.value)}
        placeholder="Write a comment..."
        className="w-full h-24 p-3 bg-gray-900 border rounded-lg border-green-500/30 focus:border-green-500 focus:ring-1 focus:ring-green-500"
        required
      />
      <button type="submit" className="btn-primary">
        Post Comment
      </button>
    </form>
  );
}; 