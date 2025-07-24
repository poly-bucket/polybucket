import api from '../utils/axiosConfig';
import { minidenticon } from 'minidenticons';

export interface RegenerateAvatarRequest {
  salt?: string;
}

export interface RegenerateAvatarResponse {
  avatar: string;
  userId: string;
  salt?: string;
}

const avatarService = {
  // Regenerate user avatar with optional salt
  async regenerateAvatar(salt?: string): Promise<RegenerateAvatarResponse> {
    const response = await api.post('/users/avatar/regenerate', { salt });
    return response.data;
  },

  // Preview avatar generation (client-side only, doesn't save)
  previewAvatar(userId: string, salt?: string): string {
    const seed = salt ? `${userId}-${salt}` : userId;
    return minidenticon(seed, 50, 50);
  }
};

export default avatarService; 