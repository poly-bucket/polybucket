"use client";

import type { ModelWithReactions } from "@/lib/types/modelReactions";
import type { ResolvedModelBrowseLayout } from "@/lib/layout/model-browse-layout";
import { useModelBrowseLayout } from "@/lib/layout/use-model-browse-layout";
import { ModelCard, ModelCardSkeleton } from "@/components/models/model-card";
import { ModelListRow, ModelListRowSkeleton } from "@/components/models/model-list-row";

interface ModelBrowseModelProps {
  model: ModelWithReactions;
  layout?: ResolvedModelBrowseLayout;
  className?: string;
  onClick?: (model: ModelWithReactions) => void;
}

export function ModelBrowseModel({
  model,
  layout,
  className,
  onClick,
}: ModelBrowseModelProps) {
  const resolved = useModelBrowseLayout();
  const active = layout ?? resolved;

  if (active.viewType === "list") {
    return (
      <ModelListRow
        model={model}
        size={active.cardSize}
        className={className}
        onClick={onClick}
      />
    );
  }

  return (
    <ModelCard
      model={model}
      size={active.cardSize}
      className={className}
      onClick={onClick}
    />
  );
}

export function ModelBrowseModelSkeleton({
  layout,
}: {
  layout?: ResolvedModelBrowseLayout;
}) {
  const resolved = useModelBrowseLayout();
  const active = layout ?? resolved;

  if (active.viewType === "list") {
    return <ModelListRowSkeleton size={active.cardSize} />;
  }

  return <ModelCardSkeleton size={active.cardSize} />;
}