"use client";

import { useState, useEffect } from "react";
import { Card, CardContent } from "@/components/primitives/card";
import { Input } from "@/components/primitives/input";
import { Button } from "@/components/primitives/button";
import { cn } from "@/lib/utils";

export interface CollectionFormValues {
  name: string;
  description: string;
  visibility: "Public" | "Private" | "Unlisted";
  password?: string;
}

interface CollectionFormProps {
  initialValues?: Partial<CollectionFormValues>;
  onSubmit: (values: CollectionFormValues) => Promise<void>;
  submitLabel: string;
  isSubmitting: boolean;
  onCancel: () => void;
}

const NAME_MAX_LENGTH = 100;
const DESCRIPTION_MAX_LENGTH = 500;

export function CollectionForm({
  initialValues,
  onSubmit,
  submitLabel,
  isSubmitting,
  onCancel,
}: CollectionFormProps) {
  const [name, setName] = useState(initialValues?.name ?? "");
  const [description, setDescription] = useState(
    initialValues?.description ?? ""
  );
  const [visibility, setVisibility] = useState<
    "Public" | "Private" | "Unlisted"
  >(initialValues?.visibility ?? "Private");
  const [password, setPassword] = useState(initialValues?.password ?? "");
  const [error, setError] = useState("");

  useEffect(() => {
    if (initialValues) {
      setName(initialValues.name ?? "");
      setDescription(initialValues.description ?? "");
      setVisibility(initialValues.visibility ?? "Private");
      setPassword(initialValues.password ?? "");
    }
  }, [initialValues]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    if (!name.trim()) {
      setError("Collection name is required");
      return;
    }
    try {
      await onSubmit({
        name: name.trim(),
        description: description.trim(),
        visibility,
        password: visibility === "Unlisted" ? password.trim() || undefined : undefined,
      });
    } catch {
      setError("Something went wrong. Please try again.");
    }
  };

  return (
    <Card variant="glass" className="border-white/20">
      <form onSubmit={handleSubmit}>
        <CardContent className="space-y-6 pt-6">
            {error && (
              <div className="rounded-md border border-red-500/30 bg-red-500/10 p-3 text-sm text-red-400">
                {error}
              </div>
            )}

            <div className="space-y-2">
              <label htmlFor="collection-name" className="text-sm font-medium text-white">
                Collection Name *
              </label>
              <Input
                id="collection-name"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="Enter collection name"
                maxLength={NAME_MAX_LENGTH}
                required
              />
              <p className="text-xs text-white/50">
                {name.length}/{NAME_MAX_LENGTH} characters
              </p>
            </div>

            <div className="space-y-2">
              <label htmlFor="collection-description" className="text-sm font-medium text-white">
                Description
              </label>
              <textarea
                id="collection-description"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder="Describe your collection (optional)"
                maxLength={DESCRIPTION_MAX_LENGTH}
                rows={4}
                className={cn(
                  "w-full rounded-md border border-white/20 bg-transparent px-3 py-2",
                  "text-base text-white placeholder:text-white/40",
                  "focus:border-ring focus:outline-none focus:ring-2 focus:ring-ring/50"
                )}
              />
              <p className="text-xs text-white/50">
                {description.length}/{DESCRIPTION_MAX_LENGTH} characters
              </p>
            </div>

            <div className="space-y-3">
              <span className="text-sm font-medium text-white">Visibility</span>
              <div className="space-y-3">
                <label className="flex cursor-pointer items-start gap-3">
                  <input
                    type="radio"
                    name="visibility"
                    value="Public"
                    checked={visibility === "Public"}
                    onChange={(e) =>
                      setVisibility(e.target.value as "Public" | "Private" | "Unlisted")
                    }
                    className="mt-1 h-4 w-4 border-white/30 text-primary focus:ring-primary"
                  />
                  <div>
                    <span className="text-sm font-medium text-white">Public</span>
                    <p className="text-xs text-white/60">
                      Anyone can see and browse this collection
                    </p>
                  </div>
                </label>
                <label className="flex cursor-pointer items-start gap-3">
                  <input
                    type="radio"
                    name="visibility"
                    value="Unlisted"
                    checked={visibility === "Unlisted"}
                    onChange={(e) =>
                      setVisibility(e.target.value as "Public" | "Private" | "Unlisted")
                    }
                    className="mt-1 h-4 w-4 border-white/30 text-primary focus:ring-primary"
                  />
                  <div>
                    <span className="text-sm font-medium text-white">Unlisted</span>
                    <p className="text-xs text-white/60">
                      Only people with the link can see this collection
                    </p>
                  </div>
                </label>
                <label className="flex cursor-pointer items-start gap-3">
                  <input
                    type="radio"
                    name="visibility"
                    value="Private"
                    checked={visibility === "Private"}
                    onChange={(e) =>
                      setVisibility(e.target.value as "Public" | "Private" | "Unlisted")
                    }
                    className="mt-1 h-4 w-4 border-white/30 text-primary focus:ring-primary"
                  />
                  <div>
                    <span className="text-sm font-medium text-white">Private</span>
                    <p className="text-xs text-white/60">
                      Only you can see this collection
                    </p>
                  </div>
                </label>
              </div>
            </div>

            {visibility === "Unlisted" && (
              <div className="space-y-2">
                <label htmlFor="collection-password" className="text-sm font-medium text-white">
                  Password Protection (Optional)
                </label>
                <Input
                  id="collection-password"
                  type="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="Enter password to protect this collection"
                  maxLength={100}
                />
                <p className="text-xs text-white/50">
                  If set, people will need this password to access your collection.
                  Leave empty to remove password protection.
                </p>
              </div>
            )}

            <div className="flex gap-3 border-t border-white/20 pt-6">
              <Button
                type="button"
                variant="outline"
                onClick={onCancel}
                disabled={isSubmitting}
                className="flex-1"
              >
                Cancel
              </Button>
              <Button
                type="submit"
                disabled={isSubmitting || !name.trim()}
                className="flex-1"
              >
                {isSubmitting ? "Saving..." : submitLabel}
              </Button>
            </div>
          </CardContent>
      </form>
    </Card>
  );
}
