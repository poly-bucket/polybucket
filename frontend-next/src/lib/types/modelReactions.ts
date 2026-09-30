import type { Model } from "@/lib/api/client";

export type ModelWithReactions = Model & {
  dislikes?: number;
  isLikedByCurrentUser?: boolean;
  isDislikedByCurrentUser?: boolean;
};

export function getModelReactionsState(model: ModelWithReactions) {
  return {
    likes: model.likes ?? 0,
    dislikes: model.dislikes ?? 0,
    userHasLiked: model.isLikedByCurrentUser ?? false,
    userHasDisliked: model.isDislikedByCurrentUser ?? false,
  };
}

export function applyModelReaction(
  model: ModelWithReactions,
  result: {
    likes: number;
    dislikes: number;
    userHasLiked: boolean;
    userHasDisliked: boolean;
  }
): ModelWithReactions {
  return {
    ...model,
    likes: result.likes,
    dislikes: result.dislikes,
    isLikedByCurrentUser: result.userHasLiked,
    isDislikedByCurrentUser: result.userHasDisliked,
  };
}
