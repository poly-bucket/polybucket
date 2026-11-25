import { useMemo } from "react";
import { useUserSettings } from "@/contexts/UserSettingsContext";

export function useUserLocale() {
  const { settings } = useUserSettings();

  return useMemo(
    () => ({
      language: settings?.language ?? "en",
      timeZone: settings?.timeZone ?? "UTC",
      measurementSystem: (settings?.measurementSystem ?? "metric") as "metric" | "imperial",
      theme: settings?.theme ?? "dark",
    }),
    [settings]
  );
}