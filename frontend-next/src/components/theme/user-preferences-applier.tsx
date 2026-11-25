"use client";

import { useEffect } from "react";
import { useUserSettings } from "@/contexts/UserSettingsContext";

function resolveDarkMode(theme: string | undefined): boolean {
  if (theme === "light") return false;
  if (theme === "dark") return true;
  if (typeof window !== "undefined") {
    return window.matchMedia("(prefers-color-scheme: dark)").matches;
  }
  return true;
}

export function UserPreferencesApplier() {
  const { settings } = useUserSettings();

  useEffect(() => {
    const language = settings?.language ?? "en";
    document.documentElement.lang = language;
  }, [settings?.language]);

  useEffect(() => {
    const theme = settings?.theme ?? "dark";
    const apply = () => {
      const isDark = resolveDarkMode(theme);
      document.documentElement.classList.toggle("dark", isDark);
    };

    apply();

    if (theme !== "system") {
      return;
    }

    const media = window.matchMedia("(prefers-color-scheme: dark)");
    const listener = () => apply();
    media.addEventListener("change", listener);
    return () => media.removeEventListener("change", listener);
  }, [settings?.theme]);

  return null;
}