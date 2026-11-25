"use client";

import { useUserSettings } from "@/contexts/UserSettingsContext";
import { getDefaultSettings } from "@/lib/services/userSettingsService";
import {
  resolveModelBrowseLayout,
  type ResolvedModelBrowseLayout,
} from "@/lib/layout/model-browse-layout";

export function useModelBrowseLayout(): ResolvedModelBrowseLayout {
  const { settings } = useUserSettings();
  return resolveModelBrowseLayout(settings ?? getDefaultSettings());
}