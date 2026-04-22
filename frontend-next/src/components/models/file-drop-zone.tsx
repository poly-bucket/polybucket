"use client";

import React, { useRef } from "react";
import { Button } from "@/components/primitives/button";
import {
  Card,
  CardContent,
} from "@/components/primitives/card";
import { cn } from "@/lib/utils";

interface FileDropZoneProps {
  onFilesSelected: (files: File[]) => void;
  acceptFormats: string[];
  canAddMore: boolean;
  maxFiles: number;
  variant?: "large" | "compact";
  disabled?: boolean;
}

export default function FileDropZone({
  onFilesSelected,
  acceptFormats,
  canAddMore,
  maxFiles,
  variant = "large",
  disabled = false,
}: FileDropZoneProps) {
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (!canAddMore || disabled) return;
    const files = Array.from(e.dataTransfer.files);
    if (files.length > 0) onFilesSelected(files);
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = e.target.files;
    if (!files || !canAddMore || disabled) return;
    onFilesSelected(Array.from(files));
    e.target.value = "";
  };

  const handleClick = () => {
    if (canAddMore && !disabled) fileInputRef.current?.click();
  };

  const isCompact = variant === "compact";

  return (
    <Card
      variant="glass"
      role="region"
      aria-label="File upload area"
      className={cn(
        "cursor-pointer border border-dashed transition-colors gap-0 py-0 shadow-none",
        !isCompact && "w-full max-w-lg mx-auto",
        canAddMore && !disabled
          ? "border-white/20 hover:border-white/35"
          : "border-white/10 opacity-60 cursor-not-allowed"
      )}
      onDragOver={handleDragOver}
      onDrop={handleDrop}
      onClick={handleClick}
    >
      <CardContent
        className={cn(
          "flex flex-col items-center justify-center gap-3 text-center px-4 sm:px-6 sm:gap-4",
          isCompact ? "py-6 sm:py-7" : "py-8 sm:py-9"
        )}
      >
        <svg
          className={cn(
            "h-9 w-9 shrink-0 sm:h-10 sm:w-10 text-white/40",
            canAddMore && !disabled ? "text-white/50" : "text-white/30"
          )}
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
          aria-hidden
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12"
          />
        </svg>
        <p
          className={cn(
            "text-foreground font-medium",
            isCompact ? "text-sm" : "text-base sm:text-lg"
          )}
        >
          {canAddMore
            ? isCompact
              ? "Add more files"
              : "Drag files to upload"
            : `Maximum files reached (${maxFiles})`}
        </p>
        {!isCompact && canAddMore && (
          <p className="text-muted-foreground text-xs sm:text-sm">or</p>
        )}
        {(canAddMore && !disabled) && (
          <Button
            type="button"
            size="sm"
            variant="outline"
            className="mt-1"
            onClick={(e) => {
              e.stopPropagation();
              handleClick();
            }}
          >
            Choose Files
          </Button>
        )}
        {!isCompact && (
          <p className="text-muted-foreground text-[11px] sm:text-xs max-w-sm leading-relaxed">
            Supported: {acceptFormats.slice(0, 8).join(", ")}
            {acceptFormats.length > 8 ? "..." : ""}
          </p>
        )}
        <input
          ref={fileInputRef}
          type="file"
          multiple
          accept={acceptFormats.map((f) => (f.startsWith(".") ? f : `.${f}`)).join(",")}
          onChange={handleFileChange}
          className="hidden"
          disabled={disabled || !canAddMore}
        />
      </CardContent>
    </Card>
  );
}
