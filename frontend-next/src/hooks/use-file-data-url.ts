"use client";

import { useEffect, useState } from "react";

export function useFileDataUrl(file: File | null | undefined): string | null {
  const [dataUrl, setDataUrl] = useState<string | null>(null);

  useEffect(() => {
    if (!file) {
      setDataUrl(null);
      return;
    }

    let cancelled = false;
    const reader = new FileReader();
    reader.onload = () => {
      if (cancelled || typeof reader.result !== "string") return;
      setDataUrl(reader.result);
    };
    reader.onerror = () => {
      if (cancelled) return;
      setDataUrl(null);
    };
    reader.readAsDataURL(file);

    return () => {
      cancelled = true;
    };
  }, [file]);

  return dataUrl;
}