"use client";

import React, { useState, useEffect } from "react";
import { cn } from "@/lib/utils";

interface PDFViewerProps {
  file: File;
  width?: number | string;
  height?: number | string;
  className?: string;
}

export default function PDFViewer({
  file,
  width = "100%",
  height = "100%",
  className = "",
}: PDFViewerProps) {
  const [pdfUrl, setPdfUrl] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!file) return;
    setLoading(true);
    setError(null);
    const url = URL.createObjectURL(file);
    setPdfUrl(url);
    setLoading(false);
    return () => {
      URL.revokeObjectURL(url);
    };
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
        <p className="text-sm">Loading PDF...</p>
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

  if (!pdfUrl) return null;

  return (
    <object
      data={pdfUrl}
      type="application/pdf"
      className={cn("w-full rounded-lg", className)}
      style={{ width, height }}
    >
      <div
        className={cn(
          "flex items-center justify-center text-muted-foreground glass-bg rounded-lg",
          className
        )}
        style={{ width, height }}
      >
        <p className="text-sm">PDF preview not available in this browser</p>
      </div>
    </object>
  );
}
