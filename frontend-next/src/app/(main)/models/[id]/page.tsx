import type { Metadata } from "next";
import { fetchModelById } from "@/lib/services/modelsService";
import { ModelDetailsPage } from "@/components/models/model-details/model-details-page";

type PageProps = {
  params: Promise<{ id: string }>;
};

export async function generateMetadata({
  params,
}: PageProps): Promise<Metadata> {
  try {
    const { id } = await params;
    const response = await fetchModelById(id);
    const model = response?.model;
    if (model?.name) {
      return {
        title: `${model.name} | Polybucket`,
        description: model.description?.slice(0, 160) ?? undefined,
      };
    }
  } catch {
    // Fall back to default
  }
  return {
    title: "Model | Polybucket",
  };
}

export default function Page({ params }: PageProps) {
  return <ModelDetailsPage />;
}
