import type { IModel } from "@/lib/api/client";

export type ModelWithReactions = IModel;

export function getModelReactionsState(model: ModelWithReactions) {
  return {
    likes: model.likes ?? 0,
    dislikes: model.dislikes ?? 0,
    userHasLiked: model.isLikedByCurrentUser ?? false,
    userHasDisliked: model.isDislikedByCurrentUser ?? false,
  };
}

export function applyModelReaction<T extends ModelWithReactions>(
  model: T,
  result: {
    likes: number;
    dislikes: number;
    userHasLiked: boolean;
    userHasDisliked: boolean;
  }
): T {
  return {
    ...model,
    likes: result.likes,
    dislikes: result.dislikes,
    isLikedByCurrentUser: result.userHasLiked,
    isDislikedByCurrentUser: result.userHasDisliked,
  };
}
