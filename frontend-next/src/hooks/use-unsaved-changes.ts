"use client";

import { useEffect } from "react";
import { usePathname } from "next/navigation";

export function useUnsavedChanges(isDirty: boolean) {
  const pathname = usePathname();

  useEffect(() => {
    const handleBeforeUnload = (e: BeforeUnloadEvent) => {
      if (isDirty) {
        e.preventDefault();
      }
    };

    window.addEventListener("beforeunload", handleBeforeUnload);
    return () => window.removeEventListener("beforeunload", handleBeforeUnload);
  }, [isDirty]);
}
