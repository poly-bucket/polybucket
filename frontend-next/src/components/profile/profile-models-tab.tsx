"use client";

import { useEffect, useState, useCallback } from "react";
import { ModelCard, ModelCardSkeleton } from "@/components/models/model-card";
import { SimplePagination } from "@/components/collections/simple-pagination";
import { fetchUserModels } from "@/lib/services/userProfileService";
import type { Model } from "@/lib/api/client";
import type { UserModelDto } from "@/lib/api/client";

const PAGE_SIZE = 12;

interface ProfileModelsTabProps {
  username: string;
  profileUsername: string;
  page: number;
  onPageChange: (page: number) => void;
}

function mapToModel(userModel: UserModelDto, authorUsername: string): Model {
  return {
    ...userModel,
    author: { username: authorUsername },
    comments: [],
  } as unknown as Model;
}

export function ProfileModelsTab({
  username,
  profileUsername,
  page,
  onPageChange,
}: ProfileModelsTabProps) {
  const [models, setModels] = useState<UserModelDto[]>([]);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);

  const loadModels = useCallback(async () => {
    setLoading(true);
    try {
      const response = await fetchUserModels(username, page, PAGE_SIZE);
      setModels(response.models ?? []);
      setTotalPages(response.totalPages ?? 1);
      setTotalCount(response.totalCount ?? 0);
    } catch {
      setModels([]);
      setTotalPages(1);
      setTotalCount(0);
    } finally {
      setLoading(false);
    }
  }, [username, page]);

  useEffect(() => {
    loadModels();
  }, [loadModels]);

  if (loading) {
    return (
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        {Array.from({ length: PAGE_SIZE }).map((_, i) => (
          <ModelCardSkeleton key={i} />
        ))}
      </div>
    );
  }

  if (models.length === 0) {
    return (
      <div className="rounded-xl border border-white/20 bg-white/5 px-8 py-12 text-center">
        <p className="text-white/60">No models found</p>
      </div>
    );
  }

  const modelsForCard = models.map((m) => mapToModel(m, profileUsername));

  return (
    <div>
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        {modelsForCard.map((model) => (
          <ModelCard key={model.id} model={model} />
        ))}
      </div>
      <SimplePagination
        page={page}
        totalPages={totalPages}
        totalCount={totalCount}
        onPageChange={onPageChange}
      />
    </div>
  );
}
