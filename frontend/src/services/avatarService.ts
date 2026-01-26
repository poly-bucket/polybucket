import { ApiClientFactory } from '../api/clientFactory';
import { RegenerateAvatarRequest } from '../api/client';
import { minidenticon } from 'minidenticons';

const api = () => ApiClientFactory.getApiClient();

export interface RegenerateAvatarResponse {
  avatar: string;
  userId: string;
  salt?: string;
}

const avatarService = {
  async regenerateAvatar(salt?: string): Promise<RegenerateAvatarResponse> {
    const request = new RegenerateAvatarRequest({ salt });
    const response = await api().regenerateAvatar_RegenerateAvatar(request);
    return response as any as RegenerateAvatarResponse;
  },

  previewAvatar(userId: string, salt?: string): string {
    const seed = salt ? `${userId}-${salt}` : userId;
    return minidenticon(seed, 50, 50);
  }
};

export default avatarService;
