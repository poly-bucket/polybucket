"use client";

import { useState, useEffect } from "react";
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import { cn } from "@/lib/utils";

interface MarkdownViewerProps {
  file: File;
  width?: number | string;
  height?: number | string;
  className?: string;
}

export default function MarkdownViewer({
  file,
  width = "100%",
  height = "100%",
  className = "",
}: MarkdownViewerProps) {
  const [content, setContent] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!file) return;
    setLoading(true);
    setError(null);
    file
      .text()
      .then((text) => {
        setContent(text);
        setLoading(false);
      })
      .catch((err) => {
        setError(err instanceof Error ? err.message : "Failed to load markdown");
        setLoading(false);
      });
  }, [file]);

  if (loading) {
    return (
      <div
        className={cn(
          "flex items-center justify-center text-muted-foreground glass-bg rounded-lg",
          className
        )}
        style={{ width, height }}
      >
        <p className="text-sm">Loading markdown...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div
        className={cn(
          "flex items-center justify-center text-destructive glass-bg rounded-lg",
          className
        )}
        style={{ width, height }}
      >
        <p className="text-sm">{error}</p>
      </div>
    );
  }

  return (
    <div
      className={cn(
        "overflow-auto p-4 glass-bg rounded-lg prose prose-invert prose-sm max-w-none",
        className
      )}
      style={{ width, height }}
    >
      <ReactMarkdown remarkPlugins={[remarkGfm]}>{content}</ReactMarkdown>
    </div>
  );
}
