import axiosInstance from "@/lib/api/axiosConfig";

export interface ModelReactionResult {
  likes: number;
  dislikes: number;
  userHasLiked: boolean;
  userHasDisliked: boolean;
  changed: boolean;
}

export type ModelReaction = "like" | "dislike";

export async function reactToModel(
  modelId: string,
  reaction: ModelReaction
): Promise<ModelReactionResult> {
  const { data } = await axiosInstance.post<ModelReactionResult>(
    `/api/models/${modelId}/${reaction}`
  );
  return data;
}

export async function removeModelReaction(
  modelId: string,
  reaction: ModelReaction
): Promise<ModelReactionResult> {
  const { data } = await axiosInstance.delete<ModelReactionResult>(
    `/api/models/${modelId}/${reaction}`
  );
  return data;
}
