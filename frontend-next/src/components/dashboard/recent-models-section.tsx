"use client";

import { useEffect, useState } from "react";
import { fetchRecentModels, RECENT_MODELS_SKELETON_COUNT } from "@/lib/services/modelsService";
import type { GetModelsResponse } from "@/lib/api/client";
import { ModelCard, ModelCardSkeleton } from "@/components/models/model-card";
import { ModelBrowseGrid } from "@/components/models/model-browse-grid";
import { useModelBrowseLayout } from "@/lib/layout/use-model-browse-layout";
import { Button } from "@/components/primitives/button";

export function RecentModelsSection() {
  const layout = useModelBrowseLayout();
  const [data, setData] = useState<GetModelsResponse | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  const loadModels = () => {
    setLoading(true);
    setError(null);
    fetchRecentModels()
      .then((response) => {
        setData(response);
      })
      .catch((err) => {
        setError(err instanceof Error ? err.message : "Could not load models");
      })
      .finally(() => {
        setLoading(false);
      });
  };

  useEffect(() => {
    loadModels();
  }, []);

  if (loading) {
    return (
      <section className="mt-8">
        <h2 className="mb-4 text-lg font-semibold text-white">Recent Models</h2>
        <ModelBrowseGrid layout={layout}>
          {Array.from({ length: RECENT_MODELS_SKELETON_COUNT }).map((_, i) => (
            <ModelCardSkeleton key={i} size={layout.cardSize} />
          ))}
        </ModelBrowseGrid>
      </section>
    );
  }

  if (error) {
    return (
      <section className="mt-8">
        <h2 className="mb-4 text-lg font-semibold text-white">Recent Models</h2>
        <div className="rounded-xl border border-white/20 bg-white/5 px-6 py-8 text-center">
          <p className="mb-4 text-white/80">{error}</p>
          <Button variant="outline" onClick={loadModels}>
            Retry
          </Button>
        </div>
      </section>
    );
  }

  const models = data?.models ?? [];
  if (models.length === 0) {
    return (
      <section className="mt-8">
        <h2 className="mb-4 text-lg font-semibold text-white">Recent Models</h2>
        <div className="rounded-xl border border-white/20 bg-white/5 px-6 py-12 text-center">
          <p className="text-white/80">No models yet.</p>
        </div>
      </section>
    );
  }

  return (
    <section className="mt-8">
      <h2 className="mb-4 text-lg font-semibold text-white">Recent Models</h2>
      <ModelBrowseGrid layout={layout}>
        {models.map((model) => (
          <ModelCard key={model.id} model={model} size={layout.cardSize} />
        ))}
      </ModelBrowseGrid>
    </section>
  );
}
