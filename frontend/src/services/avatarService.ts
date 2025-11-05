import { API_CONFIG } from '../api/config';
import { AxiosHttpClient } from '../api/axiosAdapter';
import { RegenerateAvatarClient, RegenerateAvatarRequest } from './api.client';
import { minidenticon } from 'minidenticons';

const sharedHttpClient = new AxiosHttpClient(API_CONFIG.baseUrl);

export interface RegenerateAvatarResponse {
  avatar: string;
  userId: string;
  salt?: string;
}

const avatarService = {
  async regenerateAvatar(salt?: string): Promise<RegenerateAvatarResponse> {
    const client = new RegenerateAvatarClient(API_CONFIG.baseUrl, sharedHttpClient);
    const request = new RegenerateAvatarRequest({
      salt: salt
    });
    const response = await client.regenerateAvatar(request);
    return response as any as RegenerateAvatarResponse;
  },

  previewAvatar(userId: string, salt?: string): string {
    const seed = salt ? `${userId}-${salt}` : userId;
    return minidenticon(seed, 50, 50);
  }
};

export default avatarService;
