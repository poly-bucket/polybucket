import api from './api';

export interface Comment {
  id: string;
  content: string;
  author: {
    id: string;
    username: string;
  };
  createdAt: string;
  likes: number;
  dislikes: number;
}

class CommentsService {
  async getCommentsForModel(modelId: string): Promise<Comment[]> {
    const response = await api.get<Comment[]>(`/comments/model/${modelId}`);
    return response.data;
  }

  async addComment(modelId: string, content: string): Promise<Comment> {
    const response = await api.post<Comment>(`/comments/model/${modelId}`, { content });
    return response.data;
  }

  async likeComment(commentId: string): Promise<void> {
    await api.post(`/comments/${commentId}/like`);
  }

  async dislikeComment(commentId: string): Promise<void> {
    await api.post(`/comments/${commentId}/dislike`);
  }

  async deleteComment(commentId: string): Promise<void> {
    await api.delete(`/comments/${commentId}`);
  }
}

export const commentsService = new CommentsService(); 