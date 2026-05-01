"use client";

import { useState, useEffect, useMemo, useCallback } from "react";
import { Card, CardContent } from "@/components/primitives/card";
import { Input } from "@/components/primitives/input";
import { Button } from "@/components/primitives/button";
import { cn } from "@/lib/utils";
import {
  minidenticonSvg,
  svgToDataUrl,
  maxCollectionAvatarSvgLength,
  resolvedImageSrcFromAvatarField,
} from "@/lib/avatar/minidenticon";
import { Lock, LockOpen, RefreshCw } from "lucide-react";

export interface CollectionFormValues {
  name: string;
  description: string;
  visibility: "Public" | "Private" | "Unlisted";
  password?: string;
  /** Raw SVG from minidenticon, sent to API */
  avatar?: string;
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

function randomSalt(): string {
  const chars =
    "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
  let result = "";
  for (let i = 0; i < 8; i++) {
    result += chars.charAt(Math.floor(Math.random() * chars.length));
  }
  return result;
}

function initialIconMode(
  initialAvatar: string | undefined
): "upload" | "generate" {
  return initialAvatar?.trim() ? "upload" : "generate";
}

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
  const [iconMode, setIconMode] = useState<"upload" | "generate">(() =>
    initialIconMode(initialValues?.avatar)
  );
  const [uploadedAvatar, setUploadedAvatar] = useState(
    () => initialValues?.avatar?.trim() ?? ""
  );
  const [collectionSalt, setCollectionSalt] = useState(() => randomSalt());
  const [isSeedLocked, setIsSeedLocked] = useState(false);
  const [lockedNameSeed, setLockedNameSeed] = useState("");

  const trimmedName = name.trim();
  const effectiveNameSeed =
    iconMode === "generate" && isSeedLocked ? lockedNameSeed : trimmedName;

  const generatedAvatarSvg = useMemo(() => {
    if (iconMode !== "generate" || !collectionSalt || !effectiveNameSeed) {
      return "";
    }
    return minidenticonSvg(`${effectiveNameSeed}-${collectionSalt}`, 50, 50);
  }, [effectiveNameSeed, collectionSalt, iconMode]);

  const generatedPreviewSrc = useMemo(
    () => (generatedAvatarSvg ? svgToDataUrl(generatedAvatarSvg) : ""),
    [generatedAvatarSvg]
  );

  const uploadedPreviewSrc = useMemo(() => {
    if (!uploadedAvatar) return "";
    return (
      resolvedImageSrcFromAvatarField(uploadedAvatar) ?? svgToDataUrl(uploadedAvatar)
    );
  }, [uploadedAvatar]);

  const activePreviewSrc =
    iconMode === "upload" ? uploadedPreviewSrc : generatedPreviewSrc;

  const reshuffleSalt = useCallback(() => {
    if (isSeedLocked) return;
    setIconMode("generate");
    setCollectionSalt(randomSalt());
  }, [isSeedLocked]);

  const toggleSeedLock = useCallback(() => {
    if (iconMode !== "generate") return;
    if (isSeedLocked) {
      setIsSeedLocked(false);
      setLockedNameSeed("");
      return;
    }
    setLockedNameSeed(trimmedName);
    setIsSeedLocked(true);
  }, [iconMode, isSeedLocked, trimmedName]);

  const handleUploadedFileChange = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const file = e.target.files?.[0];
      if (!file) {
        return;
      }
      const reader = new FileReader();
      reader.onload = () => {
        const result = reader.result;
        if (typeof result !== "string") {
          setError("Could not read selected image.");
          return;
        }
        setError("");
        setUploadedAvatar(result);
        setIconMode("upload");
      };
      reader.onerror = () => {
        setError("Could not read selected image.");
      };
      reader.readAsDataURL(file);
      e.target.value = "";
    },
    []
  );

  useEffect(() => {
    if (initialValues == null) return;
    setName(initialValues.name ?? "");
    setDescription(initialValues.description ?? "");
    setVisibility(initialValues.visibility ?? "Private");
    setPassword(initialValues.password ?? "");
    const next = initialValues.avatar?.trim() ?? "";
    setUploadedAvatar(next);
    setIconMode(initialIconMode(initialValues.avatar));
    setIsSeedLocked(false);
    setLockedNameSeed("");
  }, [
    initialValues?.name,
    initialValues?.description,
    initialValues?.visibility,
    initialValues?.password,
    initialValues?.avatar,
  ]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    if (!name.trim()) {
      setError("Collection name is required");
      return;
    }
    const payloadAvatar =
      iconMode === "upload"
        ? uploadedAvatar || undefined
        : generatedAvatarSvg || undefined;
    if (
      payloadAvatar &&
      payloadAvatar.length > maxCollectionAvatarSvgLength
    ) {
      setError("Collection icon is too large. Try a new pattern.");
      return;
    }
    try {
      await onSubmit({
        name: name.trim(),
        description: description.trim(),
        visibility,
        password: visibility === "Unlisted" ? password.trim() || undefined : undefined,
        avatar: payloadAvatar,
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
              <span className="text-sm font-medium text-white">Collection Icon</span>
              <div className="rounded-md border border-white/15 bg-white/5 p-3">
                <div className="flex items-center gap-3">
                  <div className="h-16 w-16 shrink-0 overflow-hidden rounded-lg border border-white/20 bg-white/10">
                    {activePreviewSrc ? (
                      <img
                        src={activePreviewSrc}
                        alt=""
                        className={cn(
                          "h-full w-full",
                          iconMode === "generate"
                            ? "[image-rendering:pixelated]"
                            : "object-cover"
                        )}
                      />
                    ) : (
                      <div className="flex h-full w-full items-center justify-center text-xs text-white/40">
                        No icon
                      </div>
                    )}
                  </div>
                  <div>
                    <p className="text-sm font-medium text-white">Final icon preview</p>
                    <p className="text-xs text-white/50">
                      This is the icon that will be used when creating the collection.
                    </p>
                  </div>
                </div>
              </div>
              <div className="grid gap-3 md:grid-cols-2">
                <div
                  role="button"
                  tabIndex={0}
                  onClick={() => setIconMode("generate")}
                  onKeyDown={(e) => {
                    if (e.key === "Enter" || e.key === " ") {
                      e.preventDefault();
                      setIconMode("generate");
                    }
                  }}
                  className={cn(
                    "space-y-3 rounded-md border p-3 text-left transition-colors",
                    iconMode === "generate"
                      ? "border-primary/80 bg-primary/10"
                      : "border-white/15 bg-white/5 hover:border-white/30"
                  )}
                >
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <p className="text-sm font-medium text-white">Generated</p>
                      <p className="text-xs text-white/50">
                        Random pattern from your collection name and seed.
                      </p>
                    </div>
                    <div className="flex items-center gap-2">
                      <Button
                        type="button"
                        variant="outline"
                        size="icon-sm"
                        aria-label={isSeedLocked ? "Unlock icon seed" : "Lock icon seed"}
                        onClick={(e) => {
                          e.stopPropagation();
                          setIconMode("generate");
                          toggleSeedLock();
                        }}
                      >
                        {isSeedLocked ? (
                          <Lock className="h-4 w-4" />
                        ) : (
                          <LockOpen className="h-4 w-4" />
                        )}
                      </Button>
                      <Button
                        type="button"
                        variant="outline"
                        size="icon-sm"
                        aria-label="New pattern"
                        onClick={(e) => {
                          e.stopPropagation();
                          setIconMode("generate");
                          reshuffleSalt();
                        }}
                        disabled={isSeedLocked}
                      >
                        <RefreshCw className="h-4 w-4" />
                      </Button>
                    </div>
                  </div>
                  <p className="text-xs text-white/50">
                    {isSeedLocked
                      ? "Generated icon is locked. Unlock to let it change while typing."
                      : "Use refresh to generate a new pattern."}
                  </p>
                </div>
                <div
                  role="button"
                  tabIndex={0}
                  onClick={() => setIconMode("upload")}
                  onKeyDown={(e) => {
                    if (e.key === "Enter" || e.key === " ") {
                      e.preventDefault();
                      setIconMode("upload");
                    }
                  }}
                  className={cn(
                    "space-y-3 rounded-md border p-3 transition-colors",
                    iconMode === "upload"
                      ? "border-primary/80 bg-primary/10"
                      : "border-white/15 bg-white/5 hover:border-white/30"
                  )}
                >
                  <div>
                    <p className="text-sm font-medium text-white">Upload Image</p>
                    <p className="text-xs text-white/50">
                      Upload your own collection icon image.
                    </p>
                  </div>
                  <label className="inline-flex cursor-pointer items-center rounded-md border border-white/20 bg-white/5 px-3 py-2 text-xs font-medium text-white transition-colors hover:border-white/40">
                    Choose image
                    <input
                      type="file"
                      accept="image/*,.svg"
                      className="sr-only"
                      onChange={(e) => {
                        setIconMode("upload");
                        handleUploadedFileChange(e);
                      }}
                    />
                  </label>
                </div>
              </div>
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
