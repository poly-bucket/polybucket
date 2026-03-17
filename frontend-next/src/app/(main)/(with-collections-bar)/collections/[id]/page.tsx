import type { Metadata } from "next";
import { CollectionDetailsPage } from "@/components/collections/collection-details-page";
import { collectionsService } from "@/lib/services/collectionsService";

type PageProps = {
  params: Promise<{ id: string }>;
};

export async function generateMetadata({
  params,
}: PageProps): Promise<Metadata> {
  try {
    const { id } = await params;
    const collection = await collectionsService.getCollectionById(id);
    return {
      title: `${collection.name} | Polybucket`,
      description: collection.description?.slice(0, 160) ?? undefined,
    };
  } catch {
    return { title: "Collection | Polybucket" };
  }
}

export default function CollectionPage({ params }: PageProps) {
  return <CollectionDetailsPage />;
}
