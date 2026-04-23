"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import { ArrowLeft } from "lucide-react";
import { useAuth } from "@/contexts/AuthContext";
import { collectionsService } from "@/lib/services/collectionsService";
import {
  CollectionForm,
  type CollectionFormValues,
} from "@/components/collections/collection-form";
import { Button } from "@/components/primitives/button";

export default function EditCollectionPage() {
  const params = useParams();
  const router = useRouter();
  const { user } = useAuth();
  const id = params?.id as string | undefined;

  const [collection, setCollection] = useState<Awaited<
    ReturnType<typeof collectionsService.getCollectionById>
  > | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (!id) return;
    collectionsService
      .getCollectionById(id)
      .then(setCollection)
      .catch(() => setError("Failed to load collection"))
      .finally(() => setLoading(false));
  }, [id]);

  if (loading) {
    return (
      <div className="mx-auto max-w-2xl px-4 py-8 sm:px-6 lg:px-8">
        <div className="h-12 w-48 animate-pulse rounded bg-white/10" />
        <div className="mt-6 h-64 animate-pulse rounded bg-white/10" />
      </div>
    );
  }

  if (error || !collection) {
    return (
      <div className="mx-auto max-w-2xl px-4 py-8 sm:px-6 lg:px-8">
        <div className="rounded-xl border border-red-500/30 bg-red-500/10 p-6 text-center">
          <p className="text-red-400">{error || "Collection not found"}</p>
          <Button
            variant="outline"
            className="mt-4"
            onClick={() => router.push("/collections/mine")}
          >
            Back to Collections
          </Button>
        </div>
      </div>
    );
  }

  if (collection.ownerId !== user?.id) {
    return (
      <div className="mx-auto max-w-2xl px-4 py-8 sm:px-6 lg:px-8">
        <div className="rounded-xl border border-red-500/30 bg-red-500/10 p-6 text-center">
          <p className="text-red-400">You don&apos;t have permission to edit this collection</p>
          <Button
            variant="outline"
            className="mt-4"
            onClick={() => router.push("/collections/mine")}
          >
            Back to Collections
          </Button>
        </div>
      </div>
    );
  }

  const handleSubmit = async (values: CollectionFormValues) => {
    setIsSubmitting(true);
    try {
      await collectionsService.updateCollection({
        id: collection.id,
        name: values.name,
        description: values.description || undefined,
        visibility: values.visibility,
        password: values.password,
      });
      router.push(`/collections/${collection.id}`);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="mx-auto max-w-2xl px-4 py-8 sm:px-6 lg:px-8">
      <div className="mb-6 flex items-center gap-4">
        <Button variant="ghost" size="icon-sm" asChild>
          <Link href={`/collections/${id}`}>
            <ArrowLeft className="h-4 w-4" />
          </Link>
        </Button>
        <h1 className="text-2xl font-bold text-white">Edit Collection</h1>
      </div>
      <CollectionForm
        initialValues={{
          name: collection.name,
          description: collection.description ?? "",
          visibility: collection.visibility,
          password: "",
          avatar: collection.avatar,
        }}
        submitLabel="Save Changes"
        onSubmit={handleSubmit}
        isSubmitting={isSubmitting}
        onCancel={() => router.push(`/collections/${id}`)}
      />
    </div>
  );
}
