"use client";

import { useEffect } from "react";
import { Controller, type UseFormReturn } from "react-hook-form";
import { LicenseTypes, PrivacySettings } from "@/lib/api/client";
import { MODEL_CATEGORIES, type ModelMetadataFormValues } from "@/lib/models/model-metadata";
import { Input } from "@/components/primitives/input";
import { Textarea } from "@/components/ui/glass/textarea";
import { Switch } from "@/components/primitives/switch";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { cn } from "@/lib/utils";

export interface UploadMetadataFormProps {
  form: UseFormReturn<ModelMetadataFormValues>;
  categories: string[];
  allowModelRemixing?: boolean;
  descriptionRules?: { required: boolean; min: number; max: number };
  onCategoriesChange: (categories: string[]) => void;
  onValidityChange: (valid: boolean) => void;
  onDirtyChange: (dirty: boolean) => void;
}

const PRIVACY_HINTS: Record<string, string> = {
  [PrivacySettings.Public]: "Anyone can find it in browse and search.",
  [PrivacySettings.Unlisted]: "Only people with the link can open it.",
  [PrivacySettings.Private]: "Only you can see it.",
};

function FieldLabel({ htmlFor, children, hint }: { htmlFor?: string; children: string; hint?: string }) {
  return (
    <div className="mb-1.5 flex items-baseline justify-between gap-2">
      <label htmlFor={htmlFor} className="text-sm font-medium text-foreground">
        {children}
      </label>
      {hint && <span className="text-xs text-muted-foreground">{hint}</span>}
    </div>
  );
}

function FieldError({ message }: { message?: string }) {
  if (!message) return null;
  return (
    <p className="mt-1.5 text-xs text-destructive" role="alert">
      {message}
    </p>
  );
}

export function UploadMetadataForm({
  form,
  categories,
  allowModelRemixing = true,
  descriptionRules,
  onCategoriesChange,
  onValidityChange,
  onDirtyChange,
}: UploadMetadataFormProps) {
  const {
    register,
    control,
    watch,
    formState: { isValid, isDirty, errors, touchedFields, dirtyFields, isSubmitted },
  } = form;

  const isRemix = watch("isRemix");
  const privacy = watch("privacy");
  const descriptionLength = (watch("description") ?? "").length;

  const shouldShowError = (field: keyof ModelMetadataFormValues) =>
    Boolean(isSubmitted || touchedFields[field] || dirtyFields[field]);

  useEffect(() => {
    onValidityChange(isValid);
  }, [isValid, onValidityChange]);

  useEffect(() => {
    onDirtyChange(isDirty);
  }, [isDirty, onDirtyChange]);

  const handleCategoryToggle = (category: string) => {
    onCategoriesChange(
      categories.includes(category)
        ? categories.filter((c) => c !== category)
        : [...categories, category]
    );
  };

  const descriptionHint = descriptionRules
    ? descriptionRules.required
      ? descriptionRules.min > 0
        ? `Required · at least ${descriptionRules.min} characters`
        : "Required"
      : "Optional"
    : undefined;

  return (
    <div className="space-y-6">
      <div>
        <FieldLabel htmlFor="upload-title">Title</FieldLabel>
        <Input
          id="upload-title"
          {...register("name")}
          placeholder="e.g. Articulated desk dragon"
          className="glass-bg h-10 border-white/20 text-base"
          aria-invalid={shouldShowError("name") && !!errors.name}
        />
        <FieldError message={shouldShowError("name") ? errors.name?.message : undefined} />
      </div>

      <div>
        <FieldLabel htmlFor="upload-description" hint={descriptionHint}>
          Description
        </FieldLabel>
        <Textarea
          id="upload-description"
          {...register("description")}
          variant="glass"
          rows={5}
          placeholder="What it is, how to print it, and anything people should know before they start."
          className="resize-y"
          aria-invalid={shouldShowError("description") && !!errors.description}
        />
        <div className="mt-1.5 flex items-start justify-between gap-3">
          <FieldError
            message={shouldShowError("description") ? errors.description?.message : undefined}
          />
          {descriptionRules && descriptionRules.max > 0 && (
            <span className="ml-auto shrink-0 font-mono text-[11px] tabular-nums text-muted-foreground">
              {descriptionLength} / {descriptionRules.max}
            </span>
          )}
        </div>
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div>
          <FieldLabel>Visibility</FieldLabel>
          <Controller
            name="privacy"
            control={control}
            render={({ field }) => (
              <Select value={field.value} onValueChange={field.onChange}>
                <SelectTrigger variant="glass" className="w-full" aria-label="Visibility">
                  <SelectValue placeholder="Select visibility" />
                </SelectTrigger>
                <SelectContent variant="glass">
                  <SelectItem value={PrivacySettings.Public}>Public</SelectItem>
                  <SelectItem value={PrivacySettings.Unlisted}>Unlisted</SelectItem>
                  <SelectItem value={PrivacySettings.Private}>Private</SelectItem>
                </SelectContent>
              </Select>
            )}
          />
          {privacy && PRIVACY_HINTS[privacy] && (
            <p className="mt-1.5 text-xs text-muted-foreground">{PRIVACY_HINTS[privacy]}</p>
          )}
        </div>
        <div>
          <FieldLabel>License</FieldLabel>
          <Controller
            name="license"
            control={control}
            render={({ field }) => (
              <Select value={field.value} onValueChange={field.onChange}>
                <SelectTrigger variant="glass" className="w-full" aria-label="License">
                  <SelectValue placeholder="Select license" />
                </SelectTrigger>
                <SelectContent variant="glass">
                  {Object.values(LicenseTypes).map((license) => (
                    <SelectItem key={license} value={license}>
                      {license}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            )}
          />
          <FieldError message={shouldShowError("license") ? errors.license?.message : undefined} />
        </div>
      </div>

      <fieldset>
        <legend className="mb-2 text-sm font-medium text-foreground">Categories</legend>
        <div className="flex flex-wrap gap-1.5">
          {MODEL_CATEGORIES.map((category) => {
            const selected = categories.includes(category);
            return (
              <button
                key={category}
                type="button"
                onClick={() => handleCategoryToggle(category)}
                aria-pressed={selected}
                className={cn(
                  "rounded-full border px-3 py-1 text-sm outline-none motion-safe:transition-colors focus-visible:ring-2 focus-visible:ring-primary/60",
                  selected
                    ? "border-primary bg-primary text-primary-foreground"
                    : "border-white/15 bg-transparent text-foreground/85 hover:border-white/35"
                )}
              >
                {category}
              </button>
            );
          })}
        </div>
      </fieldset>

      <fieldset>
        <legend className="mb-2 text-sm font-medium text-foreground">Labels</legend>
        <div className="grid grid-cols-1 gap-2 sm:grid-cols-2">
          {(
            [
              { name: "aiGenerated" as const, label: "AI generated" },
              { name: "wip" as const, label: "Work in progress" },
              { name: "nsfw" as const, label: "NSFW" },
              ...(allowModelRemixing
                ? [{ name: "isRemix" as const, label: "Remix" }]
                : []),
            ] as const
          ).map(({ name, label }) => (
            <div key={name}>
              <label className="flex cursor-pointer items-center justify-between gap-3 rounded-lg border border-white/10 bg-white/[0.02] px-3 py-2.5 text-sm text-foreground motion-safe:transition-colors hover:border-white/20">
                {label}
                <Controller
                  name={name}
                  control={control}
                  render={({ field }) => (
                    <Switch checked={field.value} onCheckedChange={field.onChange} />
                  )}
                />
              </label>
              <FieldError message={errors[name]?.message} />
            </div>
          ))}
        </div>
      </fieldset>

      {allowModelRemixing && isRemix && (
        <div>
          <FieldLabel htmlFor="upload-remix-url">Original model URL</FieldLabel>
          <Input
            id="upload-remix-url"
            {...register("remixUrl")}
            type="url"
            placeholder="https://..."
            className="glass-bg border-white/20"
            aria-invalid={!!errors.remixUrl}
          />
          <FieldError message={errors.remixUrl?.message} />
        </div>
      )}
    </div>
  );
}
