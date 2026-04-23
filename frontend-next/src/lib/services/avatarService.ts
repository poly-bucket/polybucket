import { ApiClientFactory } from "@/lib/api/clientFactory";
import { RegenerateAvatarRequest } from "@/lib/api/client";
import { previewUserAvatarSvg } from "@/lib/avatar/minidenticon";

const api = () => ApiClientFactory.getApiClient();

export async function regenerateUserAvatar(
  salt?: string
): Promise<{ avatar: string; userId?: string }> {
  const request = new RegenerateAvatarRequest({ salt });
  const response = await api().regenerateAvatar_RegenerateAvatar(request);
  if (!response?.avatar) {
    throw new Error("Regenerate did not return an avatar");
  }
  return { avatar: response.avatar, userId: response.userId };
}

export function previewUserAvatar(
  userId: string,
  salt?: string
): string {
  return previewUserAvatarSvg(userId, salt);
}
