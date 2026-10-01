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
  onCategoriesChange: (categories: string[]) => void;
  onValidityChange: (valid: boolean) => void;
  onDirtyChange: (dirty: boolean) => void;
}

export function UploadMetadataForm({
  form,
  categories,
  onCategoriesChange,
  onValidityChange,
  onDirtyChange,
}: UploadMetadataFormProps) {
  const {
    register,
    control,
    watch,
    formState: { isValid, isDirty, errors },
  } = form;

  const isRemix = watch("isRemix");

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

  return (
    <div className="space-y-6">
      <div>
        <label className="block text-sm font-medium text-foreground mb-2">Title</label>
        <Input
          {...register("name")}
          placeholder="Enter model title"
          className="glass-bg border-white/20"
          aria-invalid={!!errors.name}
        />
        {errors.name?.message && (
          <p className="mt-1 text-xs text-destructive">{errors.name.message}</p>
        )}
      </div>

      <div>
        <label className="block text-sm font-medium text-foreground mb-2">Description</label>
        <Textarea
          {...register("description")}
          variant="glass"
          rows={4}
          placeholder="Enter model description"
          className="resize-none"
          aria-invalid={!!errors.description}
        />
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-foreground mb-2">Privacy</label>
          <Controller
            name="privacy"
            control={control}
            render={({ field }) => (
              <Select value={field.value} onValueChange={field.onChange}>
                <SelectTrigger variant="glass" className="w-full">
                  <SelectValue placeholder="Select privacy" />
                </SelectTrigger>
                <SelectContent variant="glass">
                  <SelectItem value={PrivacySettings.Public}>Public</SelectItem>
                  <SelectItem value={PrivacySettings.Private}>Private</SelectItem>
                  <SelectItem value={PrivacySettings.Unlisted}>Unlisted</SelectItem>
                </SelectContent>
              </Select>
            )}
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-foreground mb-2">License</label>
          <Controller
            name="license"
            control={control}
            render={({ field }) => (
              <Select value={field.value} onValueChange={field.onChange}>
                <SelectTrigger variant="glass" className="w-full">
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
        </div>
      </div>

      <div>
        <label className="block text-sm font-medium text-foreground mb-2">Categories</label>
        <div className="flex flex-wrap gap-2">
          {MODEL_CATEGORIES.map((category) => (
            <button
              key={category}
              type="button"
              onClick={() => handleCategoryToggle(category)}
              className={cn(
                "px-3 py-1 rounded-full text-sm border transition-colors",
                categories.includes(category)
                  ? "bg-primary border-primary text-primary-foreground"
                  : "bg-transparent border-white/20 text-foreground hover:border-white/40"
              )}
            >
              {category}
            </button>
          ))}
        </div>
      </div>

      <div>
        <label className="block text-sm font-medium text-foreground mb-3">Options</label>
        <div className="space-y-2">
          {(
            [
              { name: "aiGenerated" as const, label: "AI Generated" },
              { name: "wip" as const, label: "Work in Progress" },
              { name: "nsfw" as const, label: "NSFW" },
              { name: "isRemix" as const, label: "Remix of Another Model" },
            ] as const
          ).map(({ name, label }) => (
            <label key={name} className="flex items-center gap-2 cursor-pointer text-sm text-foreground">
              <Controller
                name={name}
                control={control}
                render={({ field }) => (
                  <Switch checked={field.value} onCheckedChange={field.onChange} />
                )}
              />
              {label}
            </label>
          ))}
        </div>
      </div>

      {isRemix && (
        <div>
          <label className="block text-sm font-medium text-foreground mb-2">Original model URL</label>
          <Input
            {...register("remixUrl")}
            type="url"
            placeholder="https://..."
            className="glass-bg border-white/20"
            aria-invalid={!!errors.remixUrl}
          />
          {errors.remixUrl?.message && (
            <p className="mt-1 text-xs text-destructive">{errors.remixUrl.message}</p>
          )}
        </div>
      )}
    </div>
  );
}
