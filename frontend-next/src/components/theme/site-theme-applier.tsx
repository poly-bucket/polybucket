"use client";

import { useEffect } from "react";
import {
  applySiteThemeColors,
  loadStoredSiteTheme,
} from "@/lib/theme/apply-site-theme";

export function SiteThemeApplier() {
  useEffect(() => {
    const stored = loadStoredSiteTheme();
    if (stored) {
      applySiteThemeColors(stored);
    }
  }, []);

  return null;
}
