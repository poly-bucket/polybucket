"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { ArrowLeft } from "lucide-react";
import { collectionsService } from "@/lib/services/collectionsService";
import { CollectionForm } from "@/components/collections/collection-form";
import { Button } from "@/components/primitives/button";

export default function CreateCollectionPage() {
  const router = useRouter();
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (values: {
    name: string;
    description: string;
    visibility: "Public" | "Private" | "Unlisted";
    password?: string;
    avatar?: string;
  }) => {
    setIsSubmitting(true);
    try {
      const collection = await collectionsService.createCollection({
        name: values.name,
        description: values.description || undefined,
        visibility: values.visibility,
        password: values.password,
        avatar: values.avatar,
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
          <Link href="/collections/mine">
            <ArrowLeft className="h-4 w-4" />
          </Link>
        </Button>
        <h1 className="text-2xl font-bold text-white">Create Collection</h1>
      </div>
      <CollectionForm
        submitLabel="Create"
        onSubmit={handleSubmit}
        isSubmitting={isSubmitting}
        onCancel={() => router.push("/collections/mine")}
      />
    </div>
  );
}
