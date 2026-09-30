"use client";

import { useCallback, useState } from "react";
import { ThumbsDown, ThumbsUp } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/primitives/button";
import { formatNumber } from "@/lib/utils/modelUtils";
import {
  reactToModel,
  removeModelReaction,
  type ModelReaction,
  type ModelReactionResult,
} from "@/lib/services/modelReactionsService";

export interface ModelReactionsState {
  likes: number;
  dislikes: number;
  userHasLiked: boolean;
  userHasDisliked: boolean;
}

interface ModelReactionsProps {
  modelId: string;
  reactions: ModelReactionsState;
  isOwner: boolean;
  isFederated: boolean;
  isAuthenticated: boolean;
  onUpdate: (result: ModelReactionResult) => void;
}

export function ModelReactions({
  modelId,
  reactions,
  isOwner,
  isFederated,
  isAuthenticated,
  onUpdate,
}: ModelReactionsProps) {
  const [pending, setPending] = useState(false);

  const canReact = isAuthenticated && !isOwner && !isFederated;

  const handleReact = useCallback(
    async (reaction: ModelReaction) => {
      if (!canReact || pending) return;
      const active =
        reaction === "like" ? reactions.userHasLiked : reactions.userHasDisliked;
      setPending(true);
      try {
        const result = active
          ? await removeModelReaction(modelId, reaction)
          : await reactToModel(modelId, reaction);
        onUpdate(result);
      } catch {
        toast.error("Could not update your reaction");
      } finally {
        setPending(false);
      }
    },
    [canReact, pending, modelId, reactions.userHasLiked, reactions.userHasDisliked, onUpdate]
  );

  return (
    <div className="flex items-center justify-center gap-3 rounded-lg border border-white/10 bg-white/5 p-3">
      <Button
        type="button"
        variant="ghost"
        size="sm"
        disabled={!canReact || pending}
        aria-pressed={reactions.userHasLiked}
        aria-label={`Like (${reactions.likes})`}
        onClick={() => handleReact("like")}
        className={reactions.userHasLiked ? "text-white" : "text-white/70"}
      >
        <ThumbsUp className="h-4 w-4" />
        <span>{formatNumber(reactions.likes)}</span>
      </Button>
      <Button
        type="button"
        variant="ghost"
        size="sm"
        disabled={!canReact || pending}
        aria-pressed={reactions.userHasDisliked}
        aria-label={`Dislike (${reactions.dislikes})`}
        onClick={() => handleReact("dislike")}
        className={reactions.userHasDisliked ? "text-white" : "text-white/70"}
      >
        <ThumbsDown className="h-4 w-4" />
        <span>{formatNumber(reactions.dislikes)}</span>
      </Button>
    </div>
  );
}
