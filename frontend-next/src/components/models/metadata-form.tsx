"use client";

import React from "react";
import { Input } from "@/components/primitives/input";
import { Button } from "@/components/primitives/button";
import { cn } from "@/lib/utils";
import type { PrivacySettings } from "@/lib/api/client";
import { Textarea } from "@/components/ui/glass/textarea";
import { Switch } from "@/components/primitives/switch";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";

const CATEGORIES = [
  "Art",
  "Technology",
  "Toys",
  "Tools",
  "Games",
  "Household",
  "Engineering",
  "Fashion",
  "Medical",
  "Other",
];

const LICENSES = [
  "MIT",
  "GPL",
  "Creative Commons",
  "Commercial",
  "Custom",
];

export interface ModelData {
  title: string;
  description: string;
  privacy: PrivacySettings;
  license: string;
  categories: string[];
  aiGenerated: boolean;
  workInProgress: boolean;
  nsfw: boolean;
  remix: boolean;
}

interface MetadataFormProps {
  data: ModelData;
  onChange: (field: keyof ModelData, value: unknown) => void;
  onCancel: () => void;
  onSubmit: () => void;
  isSubmitting: boolean;
  submitLabel?: string;
}

export default function MetadataForm({
  data,
  onChange,
  onCancel,
  onSubmit,
  isSubmitting,
  submitLabel = "Upload Model",
}: MetadataFormProps) {
  const handleCategoryToggle = (category: string) => {
    onChange(
      "categories",
      data.categories.includes(category)
        ? data.categories.filter((c) => c !== category)
        : [...data.categories, category]
    );
  };

  return (
    <div className="space-y-6">
      <div>
        <label className="block text-sm font-medium text-foreground mb-2">
          Title
        </label>
        <Input
          type="text"
          value={data.title}
          onChange={(e) => onChange("title", e.target.value)}
          placeholder="Enter model title"
          className="glass-bg border-white/20"
        />
      </div>

      <div>
        <label className="block text-sm font-medium text-foreground mb-2">
          Description
        </label>
        <Textarea
          variant="glass"
          value={data.description}
          onChange={(e) => onChange("description", e.target.value)}
          rows={4}
          placeholder="Enter model description"
          className="resize-none"
        />
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-foreground mb-2">
            Privacy
          </label>
          <Select
            value={data.privacy as string}
            onValueChange={(value) => onChange("privacy", value as PrivacySettings)}
          >
            <SelectTrigger variant="glass" className="w-full">
              <SelectValue placeholder="Select privacy" />
            </SelectTrigger>
            <SelectContent variant="glass">
              <SelectItem value="Public">Public</SelectItem>
              <SelectItem value="Private">Private</SelectItem>
              <SelectItem value="Unlisted">Unlisted</SelectItem>
            </SelectContent>
          </Select>
        </div>
        <div>
          <label className="block text-sm font-medium text-foreground mb-2">
            License
          </label>
          <Select value={data.license} onValueChange={(value) => onChange("license", value)}>
            <SelectTrigger variant="glass" className="w-full">
              <SelectValue placeholder="Select license" />
            </SelectTrigger>
            <SelectContent variant="glass">
              {LICENSES.map((license) => (
                <SelectItem key={license} value={license}>
                  {license}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      </div>

      <div>
        <label className="block text-sm font-medium text-foreground mb-2">
          Categories
        </label>
        <div className="flex flex-wrap gap-2">
          {CATEGORIES.map((category) => (
            <button
              key={category}
              type="button"
              onClick={() => handleCategoryToggle(category)}
              className={cn(
                "px-3 py-1 rounded-full text-sm border transition-colors",
                data.categories.includes(category)
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
        <label className="block text-sm font-medium text-foreground mb-3">
          Options
        </label>
        <div className="space-y-2">
          {[
            { key: "aiGenerated", label: "AI Generated" },
            { key: "workInProgress", label: "Work in Progress" },
            { key: "nsfw", label: "NSFW" },
            { key: "remix", label: "Remix of Another Model" },
          ].map(({ key, label }) => (
            <label
              key={key}
              className="flex items-center gap-2 cursor-pointer text-sm text-foreground"
            >
              <Switch
                checked={data[key as keyof ModelData] as boolean}
                onCheckedChange={(checked) =>
                  onChange(key as keyof ModelData, checked)
                }
              />
              {label}
            </label>
          ))}
        </div>
      </div>

      <div className="flex justify-end gap-4 pt-6">
        <Button variant="outline" onClick={onCancel}>
          Cancel
        </Button>
        <Button
          onClick={onSubmit}
          disabled={isSubmitting}
          className="disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {isSubmitting ? "Uploading..." : submitLabel}
        </Button>
      </div>
    </div>
  );
}
